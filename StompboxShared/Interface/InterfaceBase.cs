using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UILayout;

namespace Stompbox
{
    public class InterfaceBase : Dock
    {
        protected void DoSavePresetAs()
        {
            Layout.Current.ShowTextInputPopup("File Name:", null, delegate (string presetName)
            {
                if (!string.IsNullOrEmpty(presetName))
                {
                    StompboxClient.Instance.SendCommand("SavePreset " + presetName);
                    StompboxClient.Instance.SendCommand("List Presets");
                    StompboxClient.Instance.UpdateProgram();
                }
            });
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
                            StompboxGame.Instance.SetInterfaceType(type);
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
