using System;
using Stompbox;
using UILayout;

StompboxGame.DAWMode = true;

StompboxClient client = new StompboxClient(inClientMode: true, StompboxGame.DAWMode);

//client.MidiCallback = SendMidiCommand;

StompboxGame game = new StompboxGame();

//game.SetScreenScale(scale, resizeScreen: true);

using (MonoGameHost GameHost = new MonoGameHost(1280, 800, fullscreen: false))
{
    GameHost.IsMouseVisible = true;
    GameHost.StartGame(game);

    StompboxClient.Instance.NeedUIReload = true;
}

