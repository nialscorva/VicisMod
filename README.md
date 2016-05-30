# Vici's Mod
This is a mod I created for Fortress Craft Evolved, a game very similar to a
heavily modded minecraft but better. I'm hoping by sharing the source code for
my project, others may learn from me and maybe even contribute to it!

I hereby give permission for anyone to use my code for their own machines.

# Version 2
PowWowMk1
=========
"It's like WiFi, but for power"

Will wirelessly transport 10 pps per machine in a 3m radius sphere (10 ppspm).
Cost: double of a T1 LPT - 4 Lithum, 10 Iron, 4 Tin

PowWowMk2
=========
Will wirelessly transport 40 pps per machine in a 4m radius sphere (40 ppspm).
Cost: double of a T2 LPT - 64 Titanium, 64 Nickel

PowWowMk3
=========
Will wirelessly transport 320 pps per machine in a 5m radius sphere (320 ppspm).
Cost: double of a T3 LPT - 370 Titanium, 190 Nickel, 150 Gold

PowWowMk4
=========
Will wirelessly transport unlimited pps per machine in a 7m radius sphere.
Cost: 1 T4 Battery - 1728 Titanium, 864 Nickel, 405 Gold, 162 Tin Plates

*What you lose in capacity, you gain in powered surface area!*

# Version 3
CompactSolarMk1
===============
A compact version of the Solar Panel, takes the place of 8
Cost: 8 Solar Panels, 8 Crystal Deposit

CompactSolarMk2
===============
A compact version of the Solar Panel, takes the place of 64
Cost: 8 CompactSolarMk1, 64 Crystal Deposit

CompactSolarMk3
===============
A compact version of the Solar Panel, takes the place of 512
Cost: 8 CompactSolarMk2, 512 Crystal Deposit
*Ignores T4 and T5 battery transfer rates, as is pletiful or rush mode this can
generate > 10,000 pps*

# Version 4
MassCrateModules
================
* MassCrateModuleVanilla
    * Same as the Mass Storage Crate of the normal game, but a bit more intelligent about storing stacks. This is the foundation of the new MassCrateModules.
* MassCrateModuleLinker - requires Vici's Mass Storage Mk1 research
    * This stores nothing, but is dirt cheap (literally!) and can be used to link multiple groups of MassCrateModules. Might be useful for late game and item distribution...
* MassCrateModule100 - requires Vici's Mass Storage Mk1 research
    * Just like a Storage Hopper, but for Mass Storage - stores up to 100 items.
* MassCrateModule200 - requires Vici's Mass Storage Mk2 research
    * By this stage, you should be hoarding stacks of resources (that are stackable!). This will store up to 200 of a single, stackable item.
* MassCrateModule500 - requires Vici's Mass Storage Mk3 research
    * Will store 500 of a single, stackable item.
* MassCrateModule1000 - requires Vici's Mass Storage Mk4 research
    * Will store 1000 of a single, stackable item.
* MassCrateModule10000 - requires Vici's Mass Storage Mk5 research
    * Will store 10000 of a single, stackable item.
* MassCrateModulePoweredMk1 - requires Vici's Mass Storage Mk5 research
    * Will store unlimited items, but consumes power to do so. 20 items / power / second

MassTakers
==========
* MassTakerVanilla
    * Sightly more efficient that a Mass Storage Input Port, but not by much. Interacts only with MassCrateModules
* MassTakerMk1 - requires Vici's Mass Storage Mk1 research
    * Carries 5 items at a time
* MassTakerMk2 - requires Vici's Mass Storage Mk2 research
    * Carries 5 items at a time, flies 3 m/s
* MassTakerM3 - requires Vici's Mass Storage Mk3 research
    * Carries 30 items at a time, flies 2 m/s
* MassTakerMk4 - requires Vici's Mass Storage Mk4 research
    * Carries 10 items at a time, teleports to drop off crate for 256 power, flies back at 5 m/s
* MassTakerMk5 - requires Vici's Mass Storage Mk5 research
    * Carris 1 item at a time, teleports to drop off and back for 512 power each way

MassGivers
==========
* MassGiverVanilla
    * Sightly more efficient that a Mass Storage Input Port, but not by much. Interacts only with MassCrateModules
* MassGiverMk1 - requires Vici's Mass Storage Mk1 research
    * Carries 5 items at a time
* MassGiverMk2 - requires Vici's Mass Storage Mk2 research
    * Carries 5 items at a time, flies 3 m/s
* MassGiverM3 - requires Vici's Mass Storage Mk3 research
    * Carries 30 items at a time, flies 2 m/s
* MassGiverMk4 - requires Vici's Mass Storage Mk4 research
    * Carries 10 items at a time, teleports to drop off crate for 256 power, flies back at 5 m/s
* MassGiverMk5 - requires Vici's Mass Storage Mk5 research
    * Carries 1 item at a time, teleports to drop off and back for 512 power each way

# Version 5
Fixed Crashes caused by MassCrateModules.

Crystal Converters
==================
Each one will convert their associated crystals for 100k power

* Diamond To Emerald
* Emerald To Ruby
* Ruby To Sapphire
* Sapphire To Topaz
* Topaz To Sugalite
* Sugalite To Diamond
