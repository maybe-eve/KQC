(*
KQC - KOS Quick Checker
Copyright (c) 2016 maybe-eve
This file is part of KQC.
KQC is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.
This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.
You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.
*)

namespace KQC.Backend

open System
open System.Net
open System.Text
open System.IO
open System.Xml
open System.Xml.Linq
open System.Reactive
open System.Reactive.Subjects
open System.Reactive.Linq
open System.Collections.Generic
open System.Text.RegularExpressions
open FSharp.Data
open FSharp.Data.JsonExtensions

module SeqX = 
  begin
    let skipSafe n s =
      s
      |> Seq.mapi (fun i elem -> i, elem)
      |> Seq.choose (fun (i, elem) -> if i >= n then Some(elem) else None)

    let skipWhileSafe f s =
      s
      |> Seq.choose (fun elem -> if f elem then None else Some(elem))
  end

module EVE =
  begin
    type EveChara = XmlProvider<"https://api.eveonline.com/eve/CharacterID.xml.aspx?names=ISD+Parrot">

    type EveItem = XmlProvider<"https://api.eveonline.com/eve/TypeName.xml.aspx?ids=645">

    type EveItems = XmlProvider<"https://api.eveonline.com/eve/TypeName.xml.aspx?ids=645,646">

    type EveWho = JsonProvider<"https://evewho.com/api.php?type=character&id=1633218082">

    type EveWhoCorp = JsonProvider<"https://evewho.com/api.php?type=corporation&id=869043665">

    type EveWhoAlly = JsonProvider<"https://evewho.com/api.php?type=alliance&id=99000102">

    type zKillboard = JsonProvider<"https://zkillboard.com/api/kills/regionID/10000047/pastSeconds/604800/">

    type EveSystem = JsonProvider<"https://crest-tq.eveonline.com/solarsystems/30002290/">

    type EveConst = JsonProvider<"https://crest-tq.eveonline.com/constellations/20000336/">

    type EveRegion = JsonProvider<"https://crest-tq.eveonline.com/regions/11000016/">

    let print a =
      sprintf "%A" a

    let reqString (uri : string) =
      let wr = (WebRequest.Create uri) :?> HttpWebRequest in
      wr.UserAgent = "KQC (github.com/maybe-eve/KQC)" |> ignore;
      use rs = wr.GetResponse() in
      use st = rs.GetResponseStream() in
      use sr = new StreamReader(st, Encoding.UTF8) in
      sr.ReadToEnd()

    let getCharaIdByName (name : string) =
      let n = name.Replace(" ", "%20") in
      let u = sprintf "https://api.eveonline.com/eve/CharacterID.xml.aspx?names=%s" n in 
      let res = reqString u in
      let d = EveChara.Parse res in
      d.Result.Rowset.Row.CharacterId

    let getCharaNameById id =
      let u = sprintf "https://api.eveonline.com/eve/CharacterName.xml.aspx?ids=%i" id in
      let res = reqString u in
      let d = EveChara.Parse res in
      d.Result.Rowset.Row.Name

    let getTypeNameById id =
      let u = sprintf "https://api.eveonline.com/eve/TypeName.xml.aspx?ids=%i" id in
      let res = reqString u in
      let d = EveItem.Parse res in
      d.Result.Rowset.Row.TypeName

    let getTypeNamesById (ids : int seq) =
      let s = String.Join(",", ids) in
      let u = sprintf "https://api.eveonline.com/eve/TypeName.xml.aspx?ids=%s" s in
      let res = reqString u in
      let d = EveItems.Parse res in
      d.Result.Rowset.Rows |> Seq.map (fun x -> x.TypeName)

    let getTypeNameDictById (ids : int seq) =
      let ns = getTypeNamesById ids in
      Seq.zip ids ns |> dict

    type cachedTypeNameGetter () =
      let dict = new Dictionary<int, string>()
      member this.get id =
        if dict.ContainsKey id then
          dict.[id]
        else
          let s = getTypeNameById id in
          dict.[id] <- s;
          s

    type cachedSystemInfoGetter () =
      
      let sysdict = new Dictionary<int, (string * int)>()
      let condict = new Dictionary<int, (string * string)>()
      let regdict = new Dictionary<string, string>()

      member this.get id =
        let (sn, cid) = 
          if sysdict.ContainsKey id then
            sysdict.[id]
          else
            let sys = sprintf "https://crest-tq.eveonline.com/solarsystems/%i/" id |> reqString |> EveSystem.Parse in
            let x = (sys.Name, sys.Constellation.Id) in
            sysdict.Add(id, x); x
        in
        let (cn, ruri) = 
          if condict.ContainsKey cid then
            condict.[cid]
          else
            let con = sprintf "https://crest-tq.eveonline.com/constellations/%i/" cid |> reqString |> EveConst.Parse in
            let x = (con.Name, con.Region.Href) in
            condict.Add(cid, x); x
        in
        let rn =
          if regdict.ContainsKey ruri then
            regdict.[ruri]
          else
            let reg = ruri |> reqString |> EveRegion.Parse in
            let x = reg.Name in
            regdict.Add(ruri, x); x
        in
        (sn, cn, rn)

    let StaticSystemInfoGetter = cachedSystemInfoGetter ()

    let eveWho id =
      let u = sprintf "https://evewho.com/api.php?type=character&id=%i"  id in
      let res = reqString u in
      EveWho.Parse res

    let eveWhoCorp id =
      let u = sprintf "https://evewho.com/api.php?type=corporation&id=%i"  id in
      let res = reqString u in
      EveWhoCorp.Parse res

    let eveWhoAlly id =
      let u = sprintf "https://evewho.com/api.php?type=alliance&id=%i"  id in
      let res = reqString u in
      EveWhoAlly.Parse res

    let isNpcCorp id =
      id >= 1000002 && id <= 1000182

    let getRecentProviKills () =
      try
        reqString "https://zkillboard.com/api/kills/regionID/10000047/pastSeconds/604800/"
        |> zKillboard.Parse
        |> Array.toSeq
      with
        | _ -> Seq.empty

    let getRecentKillById id = 
      try
        sprintf "https://zkillboard.com/api/kills/characterID/%i/pastSeconds/604800/" id
        |> reqString
        |> zKillboard.Parse
        |> Array.toSeq
      with
        | _ -> Seq.empty

    let checkProviKillCountById id =
      getRecentProviKills ()
      |> Seq.map (fun x -> x.Attackers)
      |> Seq.concat
      |> Seq.exists (fun x -> x.CharacterId = id)

    let checkRecentKillCountById id =
      getRecentKillById id |> Seq.length

    let getIconUriById id =
      sprintf "https://image.eveonline.com/Character/%i_64.jpg" id

    let getRenderUriById id =
      sprintf "https://imageserver.eveonline.com/Render/%i_64.png" id

  end

