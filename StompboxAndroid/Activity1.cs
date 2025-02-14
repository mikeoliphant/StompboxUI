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

    public class Activity1 : AndroidGameActivity
    {
        public static Activity1 Instance { get; private set; }

        View view;
        RemoteClient guitarClient;
        MonoGameHost GameHost;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Instance = this;

            //StompboxGame.DAWMode = false;

            guitarClient = new RemoteClient();

            StompboxGame.InterfaceType = EStompboxInterfaceType.Mobile;

            StompboxGame game = new StompboxGame();

            //game.SetScreenScale(scale, resizeScreen: true);

            GameHost = new MonoGameHost(0, 0, fullscreen: true);

            GameHost.IsMouseVisible = true;

            view = GameHost.Services.GetService(typeof(View)) as View;
            view.KeepScreenOn = true;

            SetContentView(view);

            GameHost.StartGame(game);
        }
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }
    }
}
