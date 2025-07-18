using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using Microsoft.Xna.Framework;
using UILayout;
using StompboxAPI;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Stompbox
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

    public class StompboxActivity : AndroidGameActivity
    {
        public static StompboxActivity Instance { get; private set; }

        View view;
        RemoteClient guitarClient;
        MonoGameHost GameHost;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Instance = this;

            //StompboxLayout.DAWMode = false;

            guitarClient = new RemoteClient();

            InterfaceBase.InterfaceType = EStompboxInterfaceType.Mobile;

            StompboxLayout game = new StompboxLayout();

            //game.SetScreenScale(scale, resizeScreen: true);

            GameHost = new MonoGameHost(0, 0, fullscreen: true);

            GameHost.IsMouseVisible = true;

            view = GameHost.Services.GetService(typeof(View)) as View;
            view.KeepScreenOn = true;

            SetContentView(view);

            GameHost.StartGame(game);
        }
    }
}
