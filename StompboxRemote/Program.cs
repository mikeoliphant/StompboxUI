using System;
using Stompbox;
using StompboxAPI;
using UILayout;

RemoteClient client = new();

InterfaceBase.InterfaceType = EStompboxInterfaceType.DAW;

StompboxLayout game = new StompboxLayout();

int width = 1000;
int height = 540;

if (InterfaceBase.InterfaceType == EStompboxInterfaceType.Mobile)
{
    width = 540;
    height = 960;
}
else if (InterfaceBase.InterfaceType == EStompboxInterfaceType.Pedalboard)
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

