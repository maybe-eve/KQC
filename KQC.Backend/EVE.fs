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

type httpResponse =
  | OK of string
  | Error of HttpStatusCode * string

module Response =
  begin
    let map (r : httpResponse) f =
      match r with
        | OK s -> f s |> OK
        | Error (e, s) -> r

    let toOption (r : httpResponse) =
      match r with
        | OK s -> Some s
        | Error _ -> None
  end

[<AutoOpen>]
module OptionX =
  begin
    let (|?) o v =
      defaultArg o v

    let concat o1 o2 =
      match o1 with
        | Some x -> Option.map (fun y -> (x, y)) o2
        | None -> None
  end

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

    type EveItems = XmlProvider<"https://api.eveonline.com/eve/TypeName.xml.aspx?ids=645,646">

    type EsiWho = JsonProvider<"https://esi.tech.ccp.is/latest/characters/1633218082/">

    type EsiWhoCorpHistory = JsonProvider<"https://esi.tech.ccp.is/latest/characters/1633218082/corporationhistory/">

    type EsiCorp = JsonProvider<"https://esi.tech.ccp.is/latest/corporations/1000171/">

    type EsiWhoAllyHistory = JsonProvider<"https://esi.tech.ccp.is/latest/corporations/869043665/alliancehistory/">

    type EsiType = JsonProvider<"https://esi.tech.ccp.is/latest/universe/types/645/">

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

    let req (uri : string) =
      let wr = (WebRequest.Create uri) :?> HttpWebRequest in
      wr.UserAgent = "KQC (github.com/maybe-eve/KQC)" |> ignore;
      use rs = wr.GetResponse() :?> HttpWebResponse in
      use st = rs.GetResponseStream() in
      use sr = new StreamReader(st, Encoding.UTF8) in
      if rs.StatusCode = HttpStatusCode.OK then
        sr.ReadToEnd() |> OK
      else
        (rs.StatusCode, sr.ReadToEnd()) |> Error

    let esiWho id =
      let u = sprintf "https://esi.tech.ccp.is/latest/characters/%i/" id in
      let u2 = u + "corporationhistory/" in
      let res = req u |> Response.toOption in
      let res2 = req u2 |> Response.toOption in
      OptionX.concat (Option.map EsiWho.Parse res) (Option.map EsiWhoCorpHistory.Parse res2)

    let esiCorp id =
      let u = sprintf "https://esi.tech.ccp.is/latest/corporations/%i/" id in
      let u2 = u + "alliancehistory/" in
      let res = req u |> Response.toOption in
      let res2 = req u2 |> Response.toOption in
      OptionX.concat (Option.map EsiCorp.Parse res) (Option.map EsiWhoAllyHistory.Parse res2)

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

    let getCharaIdByName (name : string) =
      let n = name.Replace(" ", "%20") in
      let u = sprintf "https://api.eveonline.com/eve/CharacterID.xml.aspx?names=%s" n in 
      let res = reqString u in
      let d = EveChara.Parse res in
      d.Result.Rowset.Row.CharacterId

    let getCharaNameById id =
      let (x, _) = (esiWho id).Value in
      x.Name

    let getTypeNameById id =
      let u = sprintf "https://esi.tech.ccp.is/latest/universe/types/%i/" id in
      let res = reqString u in
      let d = EsiType.Parse res in
      d.TypeName

    let getTypeNamesById (ids : int seq) =
      let s = String.Join(",", ids) in
      let u = sprintf "https://api.eveonline.com/eve/TypeName.xml.aspx?ids=%s" s in
      let res = reqString u in
      let d = EveItems.Parse res in
      d.Result.Rowset.Rows |> Seq.map (fun x -> x.TypeName)

    let getTypeNameDictById (ids : int seq) =
      let sids = Seq.splitInto 10 ids in
      let ns = sids |> Seq.map getTypeNamesById |> Seq.concat in
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

    let checkRecentKillCountById id =
      getRecentKillById id |> Seq.length

    let getIconUriById id =
      sprintf "https://image.eveonline.com/Character/%i_64.jpg" id

    let getRenderUriById id =
      sprintf "https://imageserver.eveonline.com/Render/%i_64.png" id

  end

