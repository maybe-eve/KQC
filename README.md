# KQC - KOS Quick Checker

## What's this?

KQC is a frontend software for [CVA KOS - Kill On Sight](http://kos.cva-eve.org/).

Checking [zKillboard](https://zkillboard.com) and [Eve Who](http://evewho.com), KQC can determine whether a player is RBL or not. (automatically!)

Also, KQC fetches the number of kills he/she have made, and whether in Providence or not. (automatically!!)

In addition, you can check 

* which ship do the red loves

* who was roaming with him/her before

* when he/she is active

* what modules he/she is likely to be fitting

, if you needed. (automatically!!!)

You can do this just with Ctrl+C and one click.

(Thanks to the developers of those websites for providing API!)

## Screenshots

![No KOS results? No problem.](http://i.imgur.com/p6y6Amo.png)

![He seems not red... Oops, he is RBL!](http://i.imgur.com/aMWZizp.png)

![It looks he love Drake](http://i.imgur.com/XqxHqgG.png)

![hehe... ALT obvious...](http://i.imgur.com/nD8J3gT.png)

![Now I can guess what kind of fit his Drake is](http://i.imgur.com/iYhTLrs.png)

## How to use

1. Ctrl-C the desired pilot name.

2. Push the button (or the notification icon).

3. Smile. (or panic. (DON'T PANIC.))

## System requirements

* .NET Framework 4.5.2 or above (the latest is highly recommended!)

## Contribution

I would appreciate it if you could do something like...

* Submit a pull request(PR) / bug report

* Replace my poor English with your great one (sending PR helps me a lot!)

* Give ISK to [Mayvie Takashina](https://zkillboard.com/character/96773588/)

* Tell me in game that you are using this software

## Disclaimer

I don't (can't) guarantee this software to be always accurate.

Use at your own risk, and beware of your enemies all the time. Fly safe!

## For developers and Linux/Mac (Mono) hackers

KQC.exe is just a GUI frontend. You can use all the features by adding KQC.Backend.dll (only depends on fsharp and rx) to your reference.

```EVE.fullCheckSource(string name)``` returns ```IObservable<Message>```, so you can cook it in Reactive Extensions.

There are also a lot of tasty methods inside ```module EVE```. See EVE.fs for details.

## Who are you?

[Mayvie Takashina](https://zkillboard.com/character/96773588/).

Japanene noob. 

## License

KQC is licensed under the GPL 3.0.
