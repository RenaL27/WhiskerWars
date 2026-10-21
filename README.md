# Whisker Wars

Whisker Wars is a Unity game where the player controls a cat that shoots at incoming rats.

## Features

* Player-controlled cat
* Automatic gunfire
* Bullet collision and destruction
* Rat spawning system
* Multiple rat enemies
* Basic 3D environment

## Controls

* Move the player using the configured movement controls
* The gun fires automatically
* Left-click an empty grid tile to place a cat. The first cat is available immediately; each successful placement starts a 5-second cooldown.
* Each placed cat has a gun that automatically fires left toward incoming rats once per second.
* The top-left indicator shows when another cat is ready. Occupied tiles cannot receive another cat.
* Placement cooldown uses game time and pauses when the game is paused.

## Built With

* Unity
* C#

## How to Run

1. Open the project in Unity.
2. Open the main game scene.
3. Press the Play button.

## Project Structure

* `Assets/` — game scripts, models, prefabs, and scenes
* `Packages/` — Unity package configuration
* `ProjectSettings/` — Unity project settings

## Illustrated game screens

The opening screen's red Play button opens How to Play. Continue to Game opens
cat selection. Gameplay pauses while menus or cat selection are open.
The spawner sends 90 rats into random lanes, in a normal/normal/fast pattern.
Every batch of six increases rat speed.
Defeat every spawned rat to see Mission Success. A rat reaching the finish
line triggers Mission Failed. Both red Return buttons reset the scene and return
to the opening screen.

Screen artwork lives in `Assets/Resources/GameScreens`. Button hit areas scale
with the artwork and preserve its 16:9 aspect ratio. Round size and spawn timing
can be adjusted on each RatSpawner in the Inspector.
