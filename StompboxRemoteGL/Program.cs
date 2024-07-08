using System;
using Stompbox;
using UILayout;

StompboxClient client = new StompboxClient(inClientMode: true);

//client.MidiCallback = SendMidiCommand;

StompboxGame game = new StompboxGame();

//game.SetScreenScale(scale, resizeScreen: true);

//int width = 540;
//int height = 960;
int width = 1024;
int height = 640;

using (MonoGameHost GameHost = new MonoGameHost(width, height, fullscreen: false))
{
    GameHost.IsMouseVisible = true;
    GameHost.StartGame(game);

    client.Disconnect();
}
