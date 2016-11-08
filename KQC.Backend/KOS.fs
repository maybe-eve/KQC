﻿(*
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

open KQC
open KQC.Backend.EVE

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

module KOS =
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

    type Message = Kos of KosResult | Jud of Judge | Text of string | CharaIcon of string | Id of int
        
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

    let checkByName (name : string) =
      let nq = name.Replace(' ', '+') in
      let u = sprintf "http://kos.cva-eve.org/api/?c=json&type=unit&q=%s" nq in
      let res = reqString u in
      let d = JsonValue.Parse res in
      d?results.AsArray() |> Seq.map parseResult

    /// <summary> Generate dummy messages. Use this for testing. </summary>
    let dummyCheckSource () =
      Observable.Create<Message>((fun (obs : IObserver<Message>) ->
        for k in flatKR (KosResult.Player("Sample Bad Guy", false, KosResult.Corp("NPC Corp", "NPC-Z", false, KosResult.Ally("None", "", false, 0), 0), 9680952)) do
          Kos k |> obs.OnNext;
        CharaIcon (getIconUriById 96809520) |> obs.OnNext;
        Text "This player has killed someone in Providence recently." |> obs.OnNext;
        sprintf "This player has killed %i ship(s) in this week." 42 |> Text |> obs.OnNext;
        Text "This player is a member of a NPC corp." |> obs.OnNext;
        for k in flatKR (KosResult.Corp("Pirates Inc.", "NPC-Z", false, KosResult.Ally("Outlaw Alliance", "", true, 0), 0)) do
          Kos k |> obs.OnNext;
        sprintf "This player is RBL because his/her last player corp \"%s\" is KOS." "Pirates Inc." |> Text |> obs.OnNext;
        Judge.Threat [Reason.NPCCorp; Reason.TrigHappy; Reason.KillInProvi; Reason.RBL] |> Jud |> obs.OnNext;
        obs.OnCompleted();
        Action(fun () -> ())
      ))
      
    let fullCheckSource name =
      Observable.Create<Message>((fun (obs : IObserver<Message>) ->
        try
          let print text =
            Text text |> obs.OnNext;
          let rs = checkByName name in
          let r = 
            rs  
            |> Seq.choose (fun x -> 
              match x with 
                | Player(_,_,_,_) -> Some x 
                | _ -> None
            )
            |> Seq.tryFind (fun x -> (getName x) = name) in
          let (id, corpId) = 
            if r.IsNone then
              print "No KOS results found.";
              KosResult.NotFound name |> Kos |> obs.OnNext;
              (getCharaIdByName name, None)
            else
              let foldr (b, d) k =
                Kos k |> obs.OnNext;
                let x = 
                  match k with
                    | Player(_, _, _, _) -> Some k
                    | _ -> d
                (isKos k || b, x)
              let (f, p) = flatKR r.Value |> Seq.fold foldr (false, None) in
              if f then
                p |> Option.map (function | Player(_, _, _, id) -> id | _ -> 0)
                  |> Option.filter ((<>) 0)
                  |> Option.map (fun id -> id |> Id |> obs.OnNext; id)
                  |> Option.iter (getIconUriById >> CharaIcon >> obs.OnNext)
                Judge.Threat [Reason.KOS] |> Jud |> obs.OnNext;
                (-1, None)
              else
                match p with
                  | Some (Player(_, _, Corp(_, _, _, _, corpId), id)) -> 
                    (id, Some corpId)
                  | Some _
                  | None -> Jud Judge.Safe |> obs.OnNext; (-1, None)
          in
          match id with
            | 0 ->
              print "This user doesn't exist."
              Jud Judge.NoInformation |> obs.OnNext
            | -1 -> ()
            | id ->
              getIconUriById id |> CharaIcon |> obs.OnNext;
              let who = eveWho id in

              if who.JsonValue.Item("info") = JsonValue.Null then
                print "This user doesn't exist.";
                Jud Judge.NoInformation |> obs.OnNext
              else
                id |> Id |> obs.OnNext;
                let rl = List<Reason>() in

                let c = checkRecentKillCountById id in
                if c > 0 then
                  sprintf "This player has killed %i ship(s) in this week." c |> print;
                  rl.Add Reason.TrigHappy;
                  if (checkProviKillCountById id) then
                    print "This player has killed someone in Providence recently.";
                    rl.Add Reason.KillInProvi

                let hist = who.History in

                let d = (DateTime.Now - (Seq.head hist).StartDate).Days in
                if d < 14 then
                  sprintf "This player is only %i day(s) old." d |> print
                  rl.Add Reason.TooYoung

                let mutable isUnknown = 
                  if corpId.IsNone then
                    "Using corp data from EveWho..." + Environment.NewLine + "(can be outdated!)" |> print;
                    let l = (hist |> Seq.last).CorporationId |> eveWhoCorp in
                    let lrs = checkByName l.Info.Name in
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
                        flatKR lr.Value |> List.iter (Kos >> obs.OnNext);
                        if isKos || aIsKos then
                          sprintf "This player is KOS because his/her corp \"%s\" is KOS." cn |> print;
                          rl.Add Reason.RBL
                        false
                      | Some _ | None -> 
                        KosResult.NotFound l.Info.Name |> Kos |> obs.OnNext;
                        if l.Info.AllianceId = 0 then 
                          print "No information available for this player and his/her corp.";
                          true
                        else
                          true
                  else false

                if isNpcCorp (defaultArg corpId who.Info.CorporationId) then
                  print "This player is a member of a NPC corp.";
                  rl.Add Reason.NPCCorp
                  let hr = hist |> Seq.rev |> SeqX.skipWhileSafe (fun x -> isNpcCorp x.CorporationId) in
                  if Seq.length hr > 0 then
                    let l = (Seq.head hr).CorporationId |> eveWhoCorp in
                    let lrs = checkByName l.Info.Name in
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
                        flatKR lr.Value |> List.iter (Kos >> obs.OnNext);
                        if isKos || aIsKos then
                          sprintf "This player is RBL because his/her last player corp \"%s\" is KOS." cn |> print;
                          rl.Add Reason.RBL
                      | _ -> ()
                  else
                    print "This player has never been a member of any player corps.";
                    isUnknown <- true

                if rl.Contains(Reason.KOS) || rl.Contains(Reason.RBL) then
                  Judge.Threat (List.ofSeq rl) |> Jud |> obs.OnNext;
                else if isUnknown && (rl.Contains(Reason.KillInProvi) || rl.Contains(Reason.TrigHappy)) then
                  Judge.Danger (List.ofSeq rl) |> Jud |> obs.OnNext;
                else if isUnknown && (rl.Contains(Reason.NPCCorp) || rl.Contains(Reason.TooYoung)) then
                  Judge.Caution (List.ofSeq rl) |> Jud |> obs.OnNext;
                else
                  Judge.Safe |> Jud |> obs.OnNext;
        with
          | :? WebException as e ->
            let dom = e.Response.ResponseUri.Host in
            let ec = e.Status.ToString() in
            sprintf "%s seems to be down right now. %s Error Code: %s" dom Environment.NewLine ec |> Text |> obs.OnNext;
            Jud Judge.NoInformation |> obs.OnNext
          | e -> reraise ()

        obs.OnCompleted();
        Action(fun () -> ())
      ))
  end
