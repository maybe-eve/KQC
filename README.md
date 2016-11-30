# KQC - KOS Quick Checker

## What's this?

KQC is a frontend software for [CVA KOS - Kill On Sight](http://kos.cva-eve.org/).

Checking [zKillboard](https://zkillboard.com), KQC can determine whether a player is RBL or not. (automatically!)

Also, KQC fetches the number of kills he/she have made, and whether in Providence or not. (automatically!!)

You can do this just with Ctrl+C and one click.

(Thanks to the developers of those websites for providing API!)

## Screenshots

![No KOS results? No problem.](http://i.imgur.com/p6y6Amo.png)

![He seems not red... Oops, he is RBL!](http://i.imgur.com/aMWZizp.png)

## How to use

1. Ctrl-C the desired pilot name.

2. Push the button (or the notification icon).

3. Smile. (or panic. (DON'T PANIC.))

## System requirements

* .NET Framework 4.5.2 or above (the latest is highly recommended!)

## For developers and Linux/Mac (Mono) hackers

KQC.exe is just a GUI frontend. You can use all the features by adding KQC.Backend.dll (only depends on fsharp and rx) to your reference.

```EVE.fullCheckSource(string name)``` returns ```IObservable<Message>```, so you can cook it in Reactive Extensions.

There are also a lot of tasty methods inside ```module EVE```. See EVE.fs for details.

## Disclaimer

I don't (can't) guarantee this software to be always accurate.

Use at your own risk, and beware of your enemies all the time.

## Who are you?

[Mayvie Takashina](https://zkillboard.com/character/96773588/).

Japanene noob. Give me some ISK!!

## License

KQC is licensed under the GPL 3.0.
