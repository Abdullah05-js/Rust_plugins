using Oxide.Core;
using Oxide.Plugins;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;


namespace Oxide.Plugins
{
    [Info("KitPlugin", "Thodex", "1.0.0")]
    class KitPlugin : RustPlugin
    {
        private Dictionary<string,string> PlayersUI = new Dictionary<string, string>();
            
        [ChatCommand("kit")]
        private void KitCommandHandler(BasePlayer player, string command, string[] args)
        {
            CuiElementContainer container = new CuiElementContainer();
            string uiID = CuiHelper.GetGuid();
            PlayersUI[player._name] = uiID;
            container.Add(new CuiPanel
            {
                Image = { Color = "0 0 0 0.8" },
                RectTransform = { AnchorMin = "0.3 0.3", AnchorMax = "0.7 0.7" },
                CursorEnabled = true
            }, "Overlay", uiID);

            container.Add(new CuiLabel
            {
                Text = { Text = "Crate Reward", FontSize = 20 },
                RectTransform = { AnchorMin = "0 0.8", AnchorMax = "1 1" }
            }, uiID);
    
            container.Add(new CuiButton
            {
                Button = { Color = "0.2 0.6 0.2 1", Command = "crateui.spin", Close = uiID },
                Text = { Text = "Spin!", FontSize = 16 },
                RectTransform = { AnchorMin = "0.4 0.05", AnchorMax = "0.6 0.15" }
            }, uiID);

            CuiHelper.AddUi(player, container);
        }


        [ConsoleCommand("crateui.spin")]

        private void GiveKit(ConsoleSystem.Arg arg)
        {
            var player = arg.Player();
            Item kit_free1 = ItemManager.CreateByName("rifle.ak", 1);
            Item kit_free2 = ItemManager.CreateByName("ammo.rifle", 64);
            player.GiveItem(kit_free1);
            player.GiveItem(kit_free2);
            CuiHelper.DestroyUi(player, PlayersUI[player._name]);
            PlayersUI.Remove("player._name");
        }
    }
}