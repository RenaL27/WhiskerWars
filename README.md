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
* Left-click an empty grid tile to place a cat. The first cat is available immediately; each successful placement starts a 10-second cooldown.
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