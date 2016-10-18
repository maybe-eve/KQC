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
    type KosResult =
      | Ally of (string * string * bool * int)
      | Corp of (string * string * bool * KosResult * int)
      | Player of (string * bool * KosResult * int)
      | Error
      | NotFound of string

    type Reason = KOS | RBL | KillInProvi | TooYoung | TrigHappy | NPCCorp

    type Judge =
      | Threat of Reason list
      | Danger of Reason list
      | Caution of Reason list
      | NoInformation
      | Safe

    type Message = Kos of KosResult | Jud of Judge | Text of string | CharaIcon of string

    type EveOfficial = XmlProvider<"<eveapi version=\"2\"><currentTime>2016-10-17 13:50:48</currentTime><result><rowset name=\"characters\" key=\"characterID\" columns=\"name,characterID\"><row name=\"example\" characterID=\"0\"/></rowset></result><cachedUntil>2016-11-17 13:50:48</cachedUntil></eveapi>">

    type EveWho = JsonProvider<"{\"info\":{\"character_id\":\"00000000\",\"corporation_id\":\"00000000\",\"alliance_id\":\"00000000\",\"faction_id\":\"0\",\"name\":\"example\",\"sec_status\":\"10.0\"},\"history\":[{\"corporation_id\":\"1000168\",\"start_date\":\"2016-09-23 08:05:00\",\"end_date\":\"2016-10-09 02:09:00\"},{\"corporation_id\":\"00000000\",\"start_date\":\"2016-10-09 02:10:00\",\"end_date\":null}]}">

    type EveWhoCorp = JsonProvider<"{\"info\":{\"corporation_id\":\"00000000\",\"alliance_id\":\"00000000\",\"name\":\"Sample Corp Please Ignore\",\"ticker\":\"SAMPL\",\"member_count\":\"42\",\"is_npc_corp\":\"0\",\"avg_sec_status\":\"0.0\",\"active\":\"0\",\"ceoID\":\"00000000\",\"taxRate\":\"100\",\"description\":\"foo bar baz\"}}">

    let print a =
      sprintf "%A" a

    let isNpcCorp id =
      id >= 1000002 && id <= 1000182
    
    let isKos (kr : KosResult) =
      match kr with
        | Error | NotFound _ -> false
        | Ally(_, _, s, _) -> s
        | Corp(_, _, s, _, _) -> s
        | Player(_, s, _, _) -> s

    let getName (kr : KosResult) =
      match kr with
        | Error -> "Error"
        | NotFound s -> s
        | Ally(s, _, _, _) -> s
        | Corp(s, _, _, _, _) -> s
        | Player(s, _, _, _) -> s

    let getType (kr : KosResult) =
      match kr with
        | Error -> "Error"
        | NotFound s -> "N/A"
        | Ally(_, _, _, _) -> "Ally"
        | Corp(_, _, _, _, _) -> "Corp"
        | Player(_, _, _, _) -> "Player"

    let rec flatKR (kr : KosResult) =
      match kr with
        | Error | NotFound _
        | Ally(_, _, _, _) -> [kr]
        | Corp(_, _, _, a, _) -> [kr; a]
        | Player(_, _, c, _) -> kr :: (flatKR c)

    let getReasons jud =
      match jud with
        | Threat x -> x
        | Danger x -> x
        | Caution x -> x
        | _ -> []

    let rec parseResult (r : JsonValue) =
      match r.Properties |> Seq.tryFind (fun (x, _) -> x = "type") |> Option.map (fun (_, x) -> x.AsString ()) with
          | Some "pilot" ->
            KosResult.Player(r?label.AsString(), r?kos.AsBoolean(), parseResult(r?corp), r?eveid.AsInteger())
          | Some "corp" ->
            KosResult.Corp(r?label.AsString(), r?ticker.AsString(), r?kos.AsBoolean(), parseResult(r?alliance), r?eveid.AsInteger())
          | Some "alliance" ->
            KosResult.Ally(r?label.AsString(), r?ticker.AsString(), r?kos.AsBoolean(), r?eveid.AsInteger())
          | Some _
          | None -> KosResult.Error

    let reqString (uri : string) =
      let wr = WebRequest.Create uri in
      use rs = wr.GetResponse() in
      use st = rs.GetResponseStream() in
      use sr = new StreamReader(st, Encoding.UTF8) in
      sr.ReadToEnd()

    let checkKosByName (name : string) =
      let nq = name.Replace(' ', '+') in
      let u = sprintf "http://kos.cva-eve.org/api/?c=json&type=unit&q=%s" nq in
      let res = reqString u in
      let d = JsonValue.Parse res in
      d?results.AsArray() |> Seq.map parseResult

    let getEveIdByName (name : string) =
      let n = name.Replace(" ", "%20") in
      let u = sprintf "https://api.eveonline.com/eve/CharacterID.xml.aspx?names=%s" n in 
      let res = reqString u in
      let d = EveOfficial.Parse res in
      d.Result.Rowset.Row.CharacterId

    let eveWho id =
      let u = sprintf "https://evewho.com/api.php?type=character&id=%i"  id in
      let res = reqString u in
      EveWho.Parse res

    let eveWhoCorp id =
      let u = sprintf "https://evewho.com/api.php?type=corporation&id=%i"  id in
      let res = reqString u in
      EveWhoCorp.Parse res

    let checkProviKills id =
      try
        let u = sprintf "https://zkillboard.com/character/kills/%i" id in
        let p = "<a href=\"/region/10000047/\">Providence</a>" in
        let res = reqString u in
        res.Contains p
      with
        | _ -> false

    let checkRecentKillCount id =
      try
        let u = sprintf "https://zkillboard.com/api/kills/characterID/%i/pastSeconds/604800/" id in
        let res = reqString u in
        Regex.Matches(res, "killID").Count
      with
        | _ -> 0

    let getIconUriById id =
      sprintf "https://image.eveonline.com/Character/%i_64.jpg" id
      
    let fullCheckSource name =
      Observable.Create<Message>((fun (obs : IObserver<Message>) ->
        let mutable isUnknown = true in
        let rs = checkKosByName name in
        let r = 
          rs  
          |> Seq.choose (fun x -> 
            match x with 
              | Player(_,_,_,_) -> Some x 
              | _ -> None
          )
          |> Seq.tryFind (fun x -> (getName x) = name) in
        let id = 
          if r.IsNone then
            obs.OnNext (Text "No KOS results found.");
            obs.OnNext (Kos (KosResult.NotFound name));
            getEveIdByName name
          else
            isUnknown <- false;
            let ks = flatKR r.Value in
            let mutable f = false in
            let mutable p = None in
            for k in ks do
              f <- isKos k || f;
              obs.OnNext(Kos k);
              match k with
                | Player(_, _, _, _) -> p <- Some k
                | _ -> ()
            if f then
              p |> Option.map (function | Player(_, _, _, id) -> id | _ -> 0)
                |> Option.filter ((<>) 0)
                |> Option.iter (fun id -> obs.OnNext(CharaIcon (getIconUriById id)));
              obs.OnNext (Jud(Judge.Threat [Reason.KOS])); -1
            else
              match p with
                | Some (Player(_, _, corp, id)) -> 
                  id
                | Some(_)
                | None -> obs.OnNext (Jud Judge.Safe); -1
        in
        match id with
          | 0 ->
            obs.OnNext (Text "This user doesn't exist.");
            obs.OnNext (Jud Judge.NoInformation)
          | -1 -> ()
          | id ->
            obs.OnNext(CharaIcon (getIconUriById id));
            let who = eveWho id in

            if who.History.Length = 0 then
              obs.OnNext (Text "This user doesn't exist.");
              obs.OnNext (Jud Judge.NoInformation)
            else
              let rl = List<Reason>() in
              if (checkProviKills id) then
                obs.OnNext(Text "This player has killed someone in Providence recently.");
                rl.Add Reason.KillInProvi
            
              let c = checkRecentKillCount id in
              if c > 0 then
                obs.OnNext(Text (sprintf "This player has killed %i ship(s) in this week." c));
                rl.Add Reason.TrigHappy

              if isNpcCorp (who.Info.CorporationId) then
                obs.OnNext(Text "This player is a member of a NPC corp.");
                rl.Add Reason.NPCCorp

              let hist = who.History in

              let d = (DateTime.Now - (Seq.head hist).StartDate).Days in
              if d < 14 then
                obs.OnNext(Text (sprintf "This player is only %i day(s) old." d));
                rl.Add Reason.TooYoung

              if isUnknown then
                let l = (hist |> Seq.last).CorporationId |> eveWhoCorp in
                let lrs = checkKosByName l.Info.Name in
                let lr = 
                  lrs 
                  |> Seq.choose (fun x -> 
                    match x with 
                      | Corp(_,_,_,_,_) -> Some x 
                      | _ -> None
                  )
                  |> Seq.tryFind (fun x -> (getName x) = l.Info.Name) 
                in
                match lr with
                  | Some(Corp(cn, _, isKos, Ally(an, _, aIsKos, _), _)) ->
                    isUnknown <- false;
                    for x in flatKR lr.Value do
                      obs.OnNext(Kos x);
                    if isKos || aIsKos then
                      obs.OnNext(Text (sprintf "His corp \"%s\" is KOS. Thus he is KOS." cn));
                      rl.Add Reason.RBL
                  | Some _ | None ->
                    ()

              let hr = hist |> Seq.rev |> SeqX.skipSafe 1 |> SeqX.skipWhileSafe (fun x -> isNpcCorp x.CorporationId) in
              if Seq.length hr > 0 then
                let l = (Seq.head hr).CorporationId |> eveWhoCorp in
                let lrs = checkKosByName l.Info.Name in
                let lr = 
                  lrs 
                  |> Seq.choose (fun x -> 
                    match x with 
                      | Corp(_,_,_,_,_) -> Some x 
                      | _ -> None
                  )
                  |> Seq.tryFind (fun x -> (getName x) = l.Info.Name) 
                in
                match lr with
                  | Some(Corp(cn, _, isKos, Ally(an, _, aIsKos, _), _)) ->
                    for x in flatKR lr.Value do
                      obs.OnNext(Kos x);
                    if isKos || aIsKos then
                      obs.OnNext(Text (sprintf "His last player corp \"%s\" is KOS. Thus he is RBL." cn));
                      rl.Add Reason.RBL
                        
                  | _ -> ()

              if rl.Contains(Reason.KOS) || rl.Contains(Reason.RBL) then
                obs.OnNext(Jud (Judge.Threat (List.ofSeq rl)))
              else if isUnknown && (rl.Contains(Reason.KillInProvi) || rl.Contains(Reason.TrigHappy)) then
                obs.OnNext(Jud (Judge.Danger (List.ofSeq rl)))
              else if isUnknown || rl.Contains(Reason.NPCCorp) || rl.Contains(Reason.TooYoung) then
                obs.OnNext(Jud (Judge.Caution (List.ofSeq rl)))
              else
                obs.OnNext(Jud Judge.Safe)

        obs.OnCompleted();
        Action(fun () -> ())
      ))

  end

