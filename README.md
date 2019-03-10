# PBM-code-snippets
Some examples of my game development work written in C# using the Unity game engine.
The files included pertain to highlight a couple different aspects of my current project,
code-named PBM.
##  Overview of files
Below is an overview of the files included in this repo.

### Container.cs
Example of setting up gameObjects, in this case containers, and player interactions with gameObjects in the game. Includes:
- Whether containers are un/locked
- Containers opening
- Containers dropping items
- Checking whether the player has, and removing, requisite keys on unlocking locked containers
- Setting which items appear in which containers

### DeathMessage.cs
Example of using UI to display a message to the player on dying in the game. Includes:
- Turning on the UI panel containing a message to the player on dying
- Setting up a semi-random message for the player, appended to refer to enemy which killed the player, current scene

### MusicManager.cs
Example of handling assets, in this case audio song clips, in the game. Includes:
- Basic playlists of audio clips
- Selecting playlists, clips based on scene
- Shuffling playlists
- Playing playlists, clips
- Fading clips in and out for scene transitions

### NPCMovement.cs
Example of handling movement behaviour for NPCs in the game. Includes:
- Wander behaviour, i.e. moving either 1) around starting area or 2) around entire scene
- Patrol behaviour, i.e. moving between two points based on either 1) having reached points or 2) time elapsed
- Handles things like ensuring target destination valid, setting new destination points

### XmlManager.cs
Example of managing various XML databases used in the game. Includes:
- Various classes relating to information kept in databases
- Public interface for making requests, changes to databases
- Reading, writing to databases
- Setting up new game, saving game, loading game
