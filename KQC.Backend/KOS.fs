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

    type KosStatus = KOS | RBL | NotKOS | NoData | NotExist

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
      | NotExist
      | Safe

    type Message = Kos of KosResult | Jud of Judge | Text of string | CharaIcon of string | Id of int
        
    let isKos (kr : KosResult) =
      match kr with
        | Error | NotFound _ -> false
        | Ally(_, _, s, _) -> s
        | Corp(_, _, s, _, _) -> s
        | Player(_, s, _, _) -> s

    let rec isKosRec (kr : KosResult) =
      match kr with
        | Error | NotFound _ -> false
        | Ally(_, _, s, _) -> s
        | Corp(_, _, s, a, _) -> s || isKosRec a
        | Player(_, s, c, _) -> s || isKosRec c

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
        | Player(_, _, _, _) -> "Pilot"

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
      if (d.Properties |> Seq.exists (fun (x, _) -> x = "results")) then
        d?results.AsArray() |> Seq.map parseResult
      else
        seq []

    let checkExact (name : string) (kosType : string) =
      let r =
        let nq = name.Replace(' ', '+') in
        let u = sprintf "http://kos.cva-eve.org/api/?c=json&type=%s&q=%s" (kosType.ToLower()) nq in
        let rec f () =
          let res = req u in
          match res with
            | OK s -> 
              let d = JsonValue.Parse s in
                if (d.Properties |> Seq.exists (fun (x, _) -> x = "results")) then
                  d?results.AsArray() |> Seq.map parseResult
                else
                  seq []
            | httpResponse.Error (e, s) ->
              System.Threading.Thread.Sleep(50);
              f()
         in f()
      in
      r |> Seq.filter (fun x -> getName x = name) |> Seq.tryFind (fun x -> getType x = kosType)

    /// <summary> Generate dummy messages. Use this for testing. </summary>
    let dummyCheckSource () =
      Observable.Create<Message>((fun (obs : IObserver<Message>) ->
        for k in flatKR (KosResult.Player("Sample Bad Guy", false, KosResult.Corp("NPC Corp", "NPC-Z", false, KosResult.Ally("None", "", false, 0), 0), 9680952)) do
          Kos k |> obs.OnNext;
        CharaIcon (getIconUriById 96809520) |> obs.OnNext;
        Text "This pilot has killed someone in Providence recently." |> obs.OnNext;
        sprintf "This pilot has killed %i ship(s) in this week." 42 |> Text |> obs.OnNext;
        Text "This pilot is a member of a NPC corp." |> obs.OnNext;
        for k in flatKR (KosResult.Corp("Pirates Inc.", "NPC-Z", false, KosResult.Ally("Outlaw Alliance", "", true, 0), 0)) do
          Kos k |> obs.OnNext;
        sprintf "This pilot is RBL because his/her last player corp \"%s\" is KOS." "Pirates Inc." |> Text |> obs.OnNext;
        Judge.Threat [Reason.NPCCorp; Reason.TrigHappy; Reason.KillInProvi; Reason.RBL] |> Jud |> obs.OnNext;
        obs.OnCompleted();
        Action(fun () -> ())
      ))

    let isNpcAndNotMM x =
      (isNpcCorp x) && (x <> 1000182)

    let quickCheck (name : string) =
      let c1 = checkExact name "Pilot" in
      match c1 with
        | Some p when (isKosRec p) -> KosStatus.KOS
        | p ->
          match (name |> getCharaIdByName |> esiWho) with
            | Some (pinfo, phist) ->
              if isNpcAndNotMM pinfo.CorporationId then
                let hr = phist |> SeqX.skipWhileSafe (fun x -> isNpcAndNotMM x.CorporationId) in
                  if (Seq.length hr) > 0 then
                    let clname = (Seq.head hr).CorporationId |> esiCorp |> Option.map (fun (x, _) -> x.CorporationName) |? "" in
                    match (checkExact clname "Corp") with
                      | Some cl when (isKosRec cl) -> KosStatus.RBL
                      | _ -> NotKOS
                  else
                    NotKOS
              else 
                match (esiCorp pinfo.CorporationId) with
                  | Some (c0info, chist) ->
                    match (checkExact c0info.CorporationName "Corp") with
                      | Some c0 when (isKosRec c0) -> KosStatus.KOS
                      | Some _ -> NotKOS
                      | None -> if (Option.isSome p) then NotKOS else NoData
                  | None -> if (Option.isSome p) then NotKOS else NoData   
            | None -> KosStatus.NotExist

    let fullCheckSource name =
      Observable.Create<Message>((fun (obs : IObserver<Message>) ->
        try
          let print text =
            Text text |> obs.OnNext;

          let runAsync f =
            async { do f() } |> Async.Start

          let r = checkExact name "Pilot" in
          let (id, corpId) = 
            match r with
              | None ->
                print "No KOS results found.";
                KosResult.NotFound name |> Kos |> obs.OnNext;
                (getCharaIdByName name, None)
              | Some rv ->
                let foldr b k =
                  Kos k |> obs.OnNext;
                  isKos k || b
                let f = flatKR rv |> Seq.fold foldr false in
                if f then
                  r |> Option.map (function | Player(_, _, _, id) -> id | _ -> 0)
                    |> Option.filter ((<>) 0)
                    |> Option.map (fun id -> id |> Id |> obs.OnNext; id)
                    |> Option.iter (getIconUriById >> CharaIcon >> obs.OnNext)
                  Judge.Threat [Reason.KOS] |> Jud |> obs.OnNext;
                  (-1, None)
                else
                  match rv with
                    | Player(_, _, Corp(_, _, _, _, corpId), id) -> 
                      (id, Some corpId)
                    | _ -> Jud Judge.Safe |> obs.OnNext; (-1, None)
          in
          match id with
            | 0 ->
              print "This user doesn't exist."
              Jud Judge.NotExist |> obs.OnNext
            | -1 -> ()
            | id ->

              let gi =
                async {
                  do getIconUriById id |> CharaIcon |> obs.OnNext
                } |> Async.StartAsTask in

              print "Retrieving user information...";
              let esiwho = esiWho id in

              match esiwho with
                | None ->
                  print "This user doesn't exist.";
                  Jud Judge.NotExist |> obs.OnNext
                | Some (who, corphist) ->
                  id |> Id |> obs.OnNext;
                  let rl = List<Reason>() in
                  
                  let kc = 
                    async {
                      let c = checkRecentKillCountById id in
                      if c > 0 then
                        do sprintf "This pilot has killed %i ship(s) in this week." c |> print;
                        do rl.Add Reason.TrigHappy;
                    } 
                    |> Async.StartAsTask in
                  
                  let ty =
                    async {
                      let d = (DateTime.Now - (Seq.last corphist).StartDate).Days in
                      if d < 14 then
                        sprintf "This pilot is only %i day(s) old." d |> print
                        rl.Add Reason.TooYoung
                    }
                    |> Async.StartAsTask in



                  let isUnknown = 
                    if isNpcAndNotMM who.CorporationId then
                      print "This pilot is a member of a NPC corp.";
                      rl.Add Reason.NPCCorp;
                      KosResult.Corp("NPC Corp", "", false, KosResult.Error, 0) |> Kos |> obs.OnNext;
                      let hr = corphist |> SeqX.skipWhileSafe (fun x -> isNpcAndNotMM x.CorporationId) in
                      if Seq.length hr > 0 then
                        let l = (Seq.head hr).CorporationId |> esiCorp |> Option.map (fun (x, _) -> x.CorporationName) |? "" in
                        let lr = checkExact l "Corp" in
                        match lr with
                          | Some(Corp(cn, _, isKos, Ally(an, _, aIsKos, _), _)) ->
                            flatKR lr.Value |> List.iter (Kos >> obs.OnNext);
                            if isKos || aIsKos then
                              sprintf "This pilot is RBL because his/her last player corp \"%s\" is KOS." cn |> print;
                              rl.Add Reason.RBL;
                            true
                          | _ ->
                            KosResult.NotFound l |> Kos |> obs.OnNext;
                            false
                      else
                        print "This pilot has never been a member of any player corps.";
                        true
                    elif corpId.IsNone then
                      let (lr, corpName, allyhist) = 
                        match who.CorporationId |> esiCorp with
                          | Some (corp, ah) ->
                            let ret = checkExact corp.CorporationName "Corp" in
                            (ret, corp.CorporationName, ah)
                          | None -> (None, "", Seq.toArray [])
                      in
                      match lr with
                        | Some(Corp(cn, _, isKos, Ally(an, _, aIsKos, _), _)) ->
                          flatKR lr.Value |> List.iter (Kos >> obs.OnNext);
                          if isKos || aIsKos then
                            sprintf "This pilot is KOS because his/her corp \"%s\" is KOS." cn |> print;
                            rl.Add Reason.KOS;
                          false
                        | Some _ | None -> 
                          KosResult.NotFound corpName |> Kos |> obs.OnNext;
                          print "No information available for both This pilot and his/her corp.";
                          true
                    else false
                  in

                  gi.Wait();
                  kc.Wait();
                  ty.Wait();
                
                  if rl.Contains(Reason.KOS) || rl.Contains(Reason.RBL) then
                    Judge.Threat (List.ofSeq rl) |> Jud |> obs.OnNext;
                  else if isUnknown && (rl.Contains(Reason.KillInProvi) || rl.Contains(Reason.TrigHappy)) then
                    Judge.Danger (List.ofSeq rl) |> Jud |> obs.OnNext;
                  else if isUnknown && (rl.Contains(Reason.NPCCorp) || rl.Contains(Reason.TooYoung)) then
                    Judge.Caution (List.ofSeq rl) |> Jud |> obs.OnNext;
                  else if isUnknown then
                    Judge.NoInformation |> Jud |> obs.OnNext;
                  else
                    Judge.Safe |> Jud |> obs.OnNext;
        with
          | :? WebException as e ->
            let dom = e.Response.ResponseUri.Host in
            let ec = e.Status.ToString() in
            sprintf "%s seems to be down right now. %sStatus: %s%sMessage: %s" dom Environment.NewLine ec Environment.NewLine e.Message |> Text |> obs.OnNext;
            Jud Judge.NotExist |> obs.OnNext
          | e -> reraise ()

        obs.OnCompleted();
        Action(fun () -> ())
      ))
  end
