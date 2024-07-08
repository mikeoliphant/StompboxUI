using System;
using Stompbox;
using UILayout;

StompboxClient client = new StompboxClient(inClientMode: true);

StompboxGame.InterfaceType = EStompboxInterfaceType.Pedalboard;

StompboxGame game = new StompboxGame();

int width = 1024;
int height = 640;

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

