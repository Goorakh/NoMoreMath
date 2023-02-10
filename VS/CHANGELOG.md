**1.3.0 Changes:**

* Added config options (Thanks prod#0339!)
  * Includes Risk of Options compatibility

**1.2.0 Changes:**

* Added Effective Health
* Improved accuracy of Holdout Zone Charge time estimation for client players if the host player also has the mod. (Still works without the host having the mod, but will not be as accurate)

**1.1.2 Changes:**

* Fixed holdout zone time remaining not working for clients in multiplayer

**1.1.1 Changes:**

* Fixed a bug where the game would freeze if hacking beacons are used on Chance Shrines

**1.1.0 Changes:**

* Added "Effective Cost" to all interactables. This value is the amount of money you will lose after buying it (if you have Executive Card, the effective cost is 90% of the actual cost)
* Added amount of money given for blood shrines.
* Added charging time in seconds to holdout zones.
* Chance Shrine activation count is now calculated via the effective cost instead of the plain cost.
* Halcyon beacons now consider the effective cost when checking if the player can afford them all.

**1.0.0 Changes:**

* First release.