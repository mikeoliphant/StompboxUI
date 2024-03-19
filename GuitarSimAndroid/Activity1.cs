using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using UILayout;
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
        MonoGameHost GameHost;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            StompboxGame.DAWMode = false;

            guitarClient = new StompboxClient(inClientMode: true, StompboxGame.DAWMode);

            StompboxGame game = new StompboxGame();

            //game.SetScreenScale(scale, resizeScreen: true);

            using (GameHost = new MonoGameHost(0, 0, fullscreen: true))
            {
                GameHost.IsMouseVisible = true;

                view = GameHost.Services.GetService(typeof(View)) as View;
                view.KeepScreenOn = true;

                SetContentView(view);

                GameHost.StartGame(game);
            }
        }
    }
}
