﻿using System;
using Stompbox;
using StompboxAPI;
using UILayout;

RemoteClient client = new();

StompboxGame.InterfaceType = EStompboxInterfaceType.DAW;

StompboxGame game = new StompboxGame();

int width = 1000;
int height = 540;

if (StompboxGame.InterfaceType == EStompboxInterfaceType.Mobile)
{
    width = 540;
    height = 960;
}
else if (StompboxGame.InterfaceType == EStompboxInterfaceType.Pedalboard)
{
    width = 960;
    height = 540;
}

using (MonoGameHost GameHost = new MonoGameHost(width, height, fullscreen: false))
{
    GameHost.IsMouseVisible = true;
    GameHost.StartGame(game);

    client.Disconnect();
}

