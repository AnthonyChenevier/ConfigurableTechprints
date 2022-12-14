# Configurable Techprints
A Rimworld mod to give players the ability to generate techprints for any research and modify their values to customize the gameplay experience.

Do you want to set a hard barrier between industrial technology and more primative tech? Make electricity require 5 techprints and cost 5x the research - you can also prevent it from generating in trader's stock.
Spaceship ending too easy? Lock off all spacer tech.
Want to make certain guns or armour upgrades a little harder to come by? A techprint limit on those techs but a lower research cost'll do ya.

Configurable Techprints leverages the exisiting intergration of techprints in vanilla/royalty, using them as valid gifts or quest rewards and showing up in trader's inventories if they were already set up to sell them.
They can also be used to speed up research if you have extras after unlocking the project, a much more likely event with the number and range of techprints this mod can unlock.

Techprints can be generated for any research project, with price, number and rarity based on the project's research cost, tech level and a number of modifiers that can be changed in the settings. 
Players can choose to auto-generate techprints by tech level this way, set custom techprint values for any project they wish, or any combination of the two.

As adding dozens of techprints to the game can somewhat change the parameters of their possible stock, techprint traders are also handled by this mod and are also customizeable to a smaller extent.
The settings page allows players to modify trader stock counts to accomodate the techprints that have been generated, or to add techprints to traders and factions who do not stock them by default (including any mod-added traders).
This can be used just to extend techprint trading to modded factions, or change which faction has access to which techprint on a more finely-tuned basis.

Comes with a heavily documented settings page to give players the information they need to modifiy the techonlogies and traders they wish.

### Links:
-------
- Github: https://github.com/AnthonyChenevier/ConfigurableTechprints

### Planned Features/Future Plans:
-------------------------------
- Per-Save settings: Currently all features are global and most changes are made to the defs on game load, requiring a restart to propagate any changed settings. This means running multiple saves with the mod is a pain as you would have to change the settings and restart for each save game. A way of maintaining some form of save-specific data is planned, but not a high priority.
- Presets: Pretty dependent on the above point - it would be nice to be able to create and save presets for certain techprint setups, such as settings for the 3 scenarios described above. This could be implemented as a group of scenario parts or as a preset list on the mod settings page, but is very likely dependent on having these things be save-dependent first
- Tech-level techprint art: I'd like to make the techprint art tech-level specific, as the vanilla texture is for an Ultratech techprint (from the filename). However, I'm a poor code monkey and struggle with GIMP and Inkscape and asthetics, so that might need some external attention.

### Compatibility:
---------------
*This section needs work. When I know more about what issues might arise I'll update here*
-Should be compatible with everything that doesn't touch the vanilla usage of techprints or research progression
-Probably not compatible or fully compatible with mods that do do those things - Techshards, Randomized Research, etc. Test and let me know what you find and I'll put it here.
-Might also have issues with anything that modifies traders's stock generation significantly - most importantly the TechprintUtility C# Class, and the StockGenerator_Techprints class that are used to select techprints by a weight value.
-Not yet tested with all of the various research screen/layout mods but if they handle techprints dynamically we should be fine.

### Notes:
-------
This is not the first mod I've made, but it's the first that I could call ready for public consumption. I'd wanted to do my own version of this for a long time but hadn't gotten the inspiriation to make it until I saw a similar mod on the workshop - "Lost Technology 1/2/World" - https://steamcommunity.com/sharedfiles/filedetails/?id=2019918121&searchtext=lost+technology. It's a great mod but didn't do exactly what I wanted; but all of the source was provided with no licence, and digging through that code gave me the idea for this. The project therefore started as a fork of 'Lost Technology' but has been rewritten so extensively to the point where maybe only a reference in a meta-file exists somewhere. Why didn't I contribute to that mod, you may ask? This started as tinkering late at night with what was available and grew quickly to be a very different mod, both technically and in scope and it just didn't seem like the right move. Never-the-less I would very much like to thank YAYO, telardo and any other contributors to that mod for setting me on the path to making my own. Cheers wonderful people!
P.S. The other mod I've worked on is a bit dusty and would require some work to bring it up to current standards, but if Biotech doesn't make it obsolete then consider if blood transfusions, bloodletting for cooking and blood-derived hormone-based drugs sounds interesting... watch this space.