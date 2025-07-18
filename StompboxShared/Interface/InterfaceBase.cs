using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UILayout;

namespace Stompbox
{
    public enum EStompboxInterfaceType
    {
        DAW,
        Mobile,
        Pedalboard
    }

    public class InterfaceBase : Dock
    {
        public static EStompboxInterfaceType InterfaceType { get; set; } = EStompboxInterfaceType.DAW;

        protected void DoSavePresetAs()
        {
            Layout.Current.ShowTextInputPopup("File Name:", null, delegate (string presetName)
            {
                if (!string.IsNullOrEmpty(presetName))
                {
                    StompboxClient.Instance.SavePresetAs(presetName);
                }
            });
        }

        public static InterfaceBase GetInterface()
        {
            switch (InterfaceType)
            {
                case EStompboxInterfaceType.DAW:
                    return new DAWInterface();
                case EStompboxInterfaceType.Mobile:
#if ANDROID
                    StompboxActivity.Instance.RequestedOrientation = Android.Content.PM.ScreenOrientation.UserPortrait;
#endif
                    return new MobileInterface();
                case EStompboxInterfaceType.Pedalboard:
#if ANDROID
                    StompboxActivity.Instance.RequestedOrientation = Android.Content.PM.ScreenOrientation.UserLandscape;
#endif
                    return new PedalboardInterface();
            }

            throw new InvalidOperationException();
        }

        public static void SetInterfaceType(EStompboxInterfaceType interfaceType)
        {
            InterfaceType = interfaceType;

            StompboxLayout.Instance.RootUIElement = GetInterface();
        }
    }

    public class ExtraOptionsButton : ImageButton
    {
        public ExtraOptionsButton(EStompboxInterfaceType currentInterfaceType)
            : base("MoreButton")
        {
            ClickAction = delegate
            {
                Layout.Current.ShowPopup(new ExtraOptionsMenu(currentInterfaceType), ContentBounds.Center);
            };
        }
    }

    public class ExtraOptionsMenu : Menu
    {
        public ExtraOptionsMenu(EStompboxInterfaceType currentInterfaceType)
        {
            List<MenuItem> menuItems = new();

            foreach (EStompboxInterfaceType type in Enum.GetValues(typeof(EStompboxInterfaceType)))
            {
                if (type != currentInterfaceType)
                {
                    menuItems.Add(new ContextMenuItem()
                    {
                        Text = "Switch to " + type.ToString() + " interface",
                        AfterCloseAction = delegate
                        {
                            InterfaceBase.SetInterfaceType(type);
                            StompboxClient.Instance.NeedUIReload = true;
                        }
                    });
                }
            }

            menuItems.Add(new ContextMenuItem()
            {
                Text = "Shut down stompbox",
                AfterCloseAction = delegate
                {
                    Layout.Current.ShowConfirmationPopup("Shut down!\n\nAre you sure?",
                        delegate
                        {
                            StompboxClient.Instance.SendCommand("Shutdown");
                        });
                }
            });

            SetMenuItems(menuItems);
        }
    }
}
