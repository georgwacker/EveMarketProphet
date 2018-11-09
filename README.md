![EveMarketProphet](Docs/Images/EMP.png)

# EveMarketProphet (EMP)

Opportunistic trading tool for EVE Online. 
Calculates lucrative trade routes between market regions based on player location, cargo space, capital and configurable thresholds.
Uses live market data via ESI API. Windows WPF application.

# Setup

1. Unzip EMP
2. Unzip ```sqlite-latest.sqlite``` from [Fuzzwork SDE](https://www.fuzzwork.co.uk/dump/sqlite-latest.sqlite.bz2) to the ```Data\``` directory
3. Setup [Authentication](#authentication) **[recommended]**

## Authentication

Reading the current player location, setting waypoints and opening market windows requires authentication via OAuth2.

1. Open EMP and go to **Settings**
2. Click **Authenticate (via Browser)**
3. A window/tab opens in your default browser to log into your EVE account
4. Select the character to authenticate (you can repeat this later to select a different one)
5. EMP will show a success message and the selected character name

Notes:
- EMP listens on port 8989 for the authentication flow, once it has aquired the token, the HTTP server shuts down
- If the authentication flow gets stuck, you can press the 'X' button to shut down the HTTP server and try again
- Users of version 1.0 who have setup their own app via the EVE developer portal can delete it


# User Guide

## Workflow

![workflow](Docs/Images/workflow.png)

The toolbar is split into logical sections for common settings, actions, filtering and options. 
Going over each element from left-to-right will guide you through the general workflow.

Although not strictly necessary before fetching the live market data, it makes sense to make a mental note of the common settings first and change them according to the trade you want to make - be it in a fast ship, blockade runner or freighter.

Settings are stored on every application exit or manually via the **Settings** window.
Most settings can be changed after fetching the market data to then recalculate routes with new parameters (see [Options](#options)).

The general workflow is thus:

1. Check (and alter common) settings
2. Select a region preset from the dropdown and click **Fetch Data**
3. Click **Find Routes** to calculate profitable trips
4. Review trips, select one to run _OR_ change settings and repeat 3.
5. No good trips? Go to 2. and widen the search with a different preset

Theoretically, you can just use **Find Routes** again after completing a trip, thus calculating all available trade routes based on your new location.
Due to the short time necessary to download the market data, I recommend to always do a **Fetch Data** first.

To give you and idea on the numbers, it takes roughly 20 seconds to fetch all market data from the 4 biggest market regions (see [Region Presets](#region-presets)) and 4-8 seconds to calculate the routes (depending on your settings).
These numbers are heavily dependent on your machine and internet connection.

## Region Presets

Currently there are 3 presets:

 * **Hubs Main:** The Forge, Domain, Heimatar, Sinq-Laison
 * **Hubs All:** Hubs Main + Metropolis, Essence, Tash-Murkon, Khanid
 * **All:** All regions excluding WH-Space

## Trip

![trip](Docs/Images/trip.png)

A trip is a single trade route between two stations, with one or many transactions.

The common starting location of all transactions does not have to be equal to your current player location. 
The jump diagram shows all waypoints, including the way to the starting location. 

_Hovering over individual dots will show the system name with security status and the overall jumps for the trip._
_Hovering over the station names will also show their security status._

In this example, a particular trip between Hek and Jita has the following attributes:

* **Profit / Jump:** 4.2m ISK
* **Trip Profit:** 78.4m ISK
* **Trip Cost:** 521.6m ISK
* **Trip Weight:** 12.6k m3

The **From** and **To** buttons will set the appropiate waypoints ingame, holding shift while clicking will add the waypoints without clearing existing ones.
Keep in mind that your ingame settings for safer/faster routes still applies, so you may need to change it.

**Filter** shows trips with common starting and ending _systems_, to better plan round-trips.
The **Clear Filter** button in the toolbar will remove the filtering.


## Transaction

![tx](Docs/Images/tx.png)

A transaction is based on a pair of buy- and sell-orders between two stations of one particular item type.
Sometimes they are split up to denote different sell orders with individual prices to fulfill a single buy order at the target location.

In this example, 2 units of _'Augmented' Hammerhead_ are being sold for 34m ISK each, while 19 units are bought at the target location for 42.1m ISK each. 
The quantity of the transaction is 2, although we could have made a potential bigger profit.

This transaction with 2 units weighs in at 20m3. _Hovering over the field will show the individual type weight._

The total cost or investment for this trade comes in at 68m ISK, while the overall profit sums up to 15.4m ISK.

_Clicking on the type icon will open the associated market window ingame, when the authentication is setup._


## Options

![options](Docs/Images/options.png)

* **Base Profit:** Earliest threshold in the computation, base profit for the transaction without jumps, takes quantity into account
* **Filler Profit:** Prevents filling the cargo with miniscule amounts of low volume goods, settings lower than Base Profit have no effect
* **Profit/Jump:** Base profit per jump between two stations for a trip, not including the route to the start of the trip, filters out trips before expensive route finding
* **Location:** Fallback ```SolarSystemID``` for the player location, when offline or not using authentication, basis to route and calculate profitable trips
* **Accounting:** Level 5 of the Accounting skill lowers the tax rate from 2% to 1% and is important for profit calculations, applies to sell orders
* **Stations in NullSec:** Removes stations in 0.0 and lower from the dataset, AFAIK there is currently no way to check if a station is NPC-operated. When leaving this option on, you may not be able to dock there. **[Applies to next Fetch Data]**
* **Illegal Goods:** Removes illegal goods from the dataset, does not take regional differences into account **[Applies to next Fetch Data]**

# Caveats

With everything in EVE Online be suspicious and mindful. If a deal looks too good to be true, it probably is a scam or market scheme.
Buy orders can simply fail or be already fulfilled once you reach your destination.

Keep in mind that although rare, EMP does support minimum buyouts. Be careful when skipping sell orders that are listed to complete the buyout.

There are currently no safe-guards in place to filter out suspicious transactions with abnormal margins.

I am not responsible if you miss out on a big deal, get blown up with valuable cargo or tricked by market manipulation / sophisticated honey pots for traders.

Fly safe. Trade safe.

# FAQ

## Q: Is the old setup via dev portal still required?
No, EMP uses the new OAuth2 authentication flow for native apps. 

## Q: What about the Broker Fee?
This style of opportunistic trading is all about the instant profit and therefor you will rarely setup long-term sell orders. Especially when travelling to far out systems, where you can't update the market orders easily.

# Frameworks

* [QuickGraph](https://github.com/YaccConstructor/QuickGraph) 
* [Entity Framework Core](https://github.com/aspnet/EntityFramework)
* [ZXing.Net](https://zxingnet.codeplex.com/)
* [Json.NET](http://www.newtonsoft.com/json)
* [Prism [_Core_]](https://github.com/PrismLibrary/)
* [SQLite](https://sqlite.org/)
* [RateGate](http://www.jackleitch.net/2010/10/better-rate-limiting-with-dot-net/) 
* [GitVersion](https://github.com/GitTools/GitVersion)
* [MSBuildTasks](https://github.com/loresoft/msbuildtasks)
* [Flurl](https://flurl.io/)

# Credits

* Title & Logo Concept by **NNowheremaNN**
* Feedback & Ideas by **Pallustris**
* Pulse Icon by **Ho Thi Ngoc Trinh** (CC)
* Type Icons & Game Data by **CCP hf**
* **NavBot** & **EveNav** for Inspiration
* SDE SQLite conversion by **Fuzzwork**

# Donations

**Send ISK to:** ```Cindril```

**Bitcoin:** ```1DppXkNfPKbs1JiF2vZ3m89QQKnToPAuMZ```

[![paypal](https://www.paypalobjects.com/webstatic/en_US/i/btn/png/gold-rect-paypal-60px.png)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=SE7KHFVJ2UHQ4)

