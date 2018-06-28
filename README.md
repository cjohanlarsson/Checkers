# Checkers

A console version of checkers built in Unity 2018.1.4f1 (with no extra packages or dependencies).

## Build

Try out the WebGL build here:
http://trickytoken.com/builds/CheckersWebGL/index.html


## Code

Gameplay/Game.cs - code that manages Checkers game logic

Ux/Manager.cs - code that manages the console view and AI


## Known Bugs

If the game ends on 'O' (the player) turn, the game over message isn't displayed (due to it being clobbered around line 41 or Manager.cs).
