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

module Tactical =
  begin
    
    type Message =
      | Gang of (string * int) seq
      | ShipInfo of ((string * int) * float * float * ((string * int) * int) seq * ((string * int) * float) seq) seq
      | TzInfo of (int * int) seq

    let getKillDetail i = 
      Observable.Create<Message>((fun (obs : IObserver<Message>) ->
        try
          let nget = cachedTypeNameGetter () in
          let dat = 
            sprintf "https://zkillboard.com/api/kills/characterID/%i/pastSeconds/604800/" i
            |> reqString
            |> zKillboard.Parse
          in
          let (involved, kills) =
            let a = 
              dat
              |> Seq.map (fun x -> x.Attackers)
              |> Seq.concat
            let b = 
              a
              |> Seq.filter (fun x -> x.CharacterId <> i)
              |> Seq.map (fun x -> x.CharacterName)
              |> Seq.choose id
              |> Seq.groupBy id
              |> Seq.map (fun (x, l) -> (x, Seq.length l))
              |> Seq.sortByDescending snd
              |> Seq.toArray |> Array.toSeq 
            Gang b |> obs.OnNext;
            let c = 
              a
              |> Seq.filter (fun x -> x.CharacterId = i)
              |> Seq.map (fun x -> (x.ShipTypeId, x.WeaponTypeId))
            (b, c)
          let losses =
            sprintf "https://zkillboard.com/api/losses/characterID/%i/pastSeconds/604800/" i
            |> reqString
            |> zKillboard.Parse
            |> Seq.map (fun x -> 
              let ship = x.Victim.ShipTypeId in
              let fit = x.Items |> Seq.map (fun y -> y.TypeId) in
              (ship, fit)
              )
          let shipInfo = 
            let s1 = kills |> Seq.map fst in
            let s2 = losses |> Seq.map fst in
            let kc = Seq.length kills |> float in
            let lc = Seq.length losses |> float in
            let ids = List () in
            let ss = 
              [s1; s2] 
              |> Seq.concat 
              |> Seq.distinct
              |> Seq.map (fun x ->
                let ks = kills |> Seq.filter (fst >> ((=) x)) in
                let ls = losses |> Seq.filter (fst >> ((=) x)) in
                let killFreq = (Seq.length ks |> float) / kc * 100.0 |> Math.Ceiling in
                let lossFreq = (Seq.length ls |> float) / lc * 100.0 |> Math.Ceiling in
                let fitTend = 
                  let b = ls |> Seq.map snd |> Seq.concat in 
                  b 
                  |> Seq.countBy id 
                  in
                let damageTend =
                  ks 
                  |> Seq.map snd 
                  |> Seq.countBy id 
                  |> Seq.map (fun (x, i) -> 
                    (x, float i / float (Seq.length ks) * 100.0 |> Math.Ceiling)
                  ) in
                fitTend |> Seq.iter (fst >> ids.Add);
                damageTend |> Seq.iter (fst >> ids.Add);
                ids.Add x;
                (x, killFreq, lossFreq, fitTend, damageTend)
              ) |> Seq.toArray in
            let dic = 
              if not (Seq.isEmpty ids) then
                getTypeNameDictById ids
              else
                Dictionary() :> IDictionary<int, string>
            in
            ss 
            |> Seq.map (fun (sid, k, l, fts, dts) ->
              let fts' = fts |> Seq.map (fun (id, f) -> ((dic.[id], id), f)) |> Seq.toArray |> Array.toSeq in
              let dts' = dts |> Seq.map (fun (id, f) -> ((dic.[id], id), f)) |> Seq.toArray |> Array.toSeq in
              ((dic.[sid], sid), k, l, fts', dts')
            )
            |> Seq.sortByDescending (fun (_, k, _, _, _) -> k) 
            |> Seq.toArray |> Array.toSeq 
          in
          ShipInfo shipInfo |> obs.OnNext;
          let tz = 
            dat
            |> Seq.map (fun x -> x.KillTime.Hour)
            |> Seq.groupBy id
            |> Seq.map (fun (t, xs) -> (t, Seq.length xs))
          in 
          let tzr = 
            [0..23]
            |> Seq.map (fun x -> (x, tz |> Seq.tryFind (fun (t, c) -> t = x)))
            |> Seq.map (function | (x, Some(y)) -> (x, snd y) | (x, None) -> (x, 0))
            |> Seq.sortBy fst
          in
          tzr |> TzInfo |> obs.OnNext;
          
        with
          | _ -> ()
        obs.OnCompleted();
        Action(fun () -> ())
      ))
  end