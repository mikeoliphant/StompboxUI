using System;
using Stompbox;
using UILayout;

StompboxGame.DAWMode = false;

StompboxClient client = new StompboxClient(inClientMode: true, StompboxGame.DAWMode);

//client.MidiCallback = SendMidiCommand;

StompboxGame game = new StompboxGame();

//game.SetScreenScale(scale, resizeScreen: true);

using (MonoGameHost GameHost = new MonoGameHost(540, 960, fullscreen: false))
{
    GameHost.IsMouseVisible = true;
    GameHost.StartGame(game);

    StompboxClient.Instance.NeedUIReload = true;
}

