using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using Stompbox;

namespace StompboxAndroid
{
    [Activity(
        Label = "@string/app_name",
        MainLauncher = true,
        Icon = "@mipmap/ic_launcher",
        AlwaysRetainTaskState = true,
        LaunchMode = LaunchMode.SingleInstance,
        ScreenOrientation = ScreenOrientation.FullUser,        
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.ScreenSize
    )]

    public class Activity1 : AndroidGameActivity
    {
        View view;
        StompboxClient guitarClient;
        Mono game;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            GuitarGame.DAWMode = false;

            guitarClient = new GuitarClient(inClientMode: true, GuitarGame.DAWMode);

            game = new XNAGame();

            GuitarGame pixGame = new GuitarGame(game.ScreenWidth, game.ScreenHeight);

            GuitarClient.DebugAction = PixGame.Debug;

            view = game.Services.GetService(typeof(View)) as View;
            view.KeepScreenOn = true;

            SetContentView(view);

            game.Window.AllowUserResizing = true;

            game.IsMouseVisible = true;

            game.StartGame(pixGame);
        }
    }
}
