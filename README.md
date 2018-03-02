# MrServer
MrConnect server source code.

MrConnect is a Discord bot that works like a middle man receiving and sending data from/to both discord and the server.
It might seem pointless to create a server on top, but by doing so I allow myself to create my own custom command builder library and to not rely on Discord API wrappers as much.

E.g. Discord.net pushes a new update and let's say I have 100 commands. 
Well time to rewrite all of those 100 commands if they were not on the server...
(P.S. I have nothing against Discord.Net, I just dislike the lack of control which would not make sense to release to the public on DIscord.Net's part either)

Current goals are to make a multifunctional Discord bot with Osu!, World of Warcraft commands.
