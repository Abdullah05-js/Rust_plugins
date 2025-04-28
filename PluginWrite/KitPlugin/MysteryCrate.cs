using Oxide.Core;
using Oxide.Plugins;
using UnityEngine;
using Oxide.Game.Rust.Cui;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System;
using Oxide.Core.Libraries;
using ConVar;


namespace Oxide.Plugins
{
    [Info("MysteryCrate", "Thodex", "1.0.0")]
    class MysteryCrate : RustPlugin
    {

        void OnServerShutdown()
        {
            Unload();
        }

        private List<StorageContainer> specialChests = new List<StorageContainer>();
        [ChatCommand("chest")]
        private void SpawnChestCommand(BasePlayer player, string command, string[] args)
        {
            Vector3 position = player.transform.position + (player.transform.forward * 2f);
            Quaternion rotation = Quaternion.identity;

            var entity = GameManager.server.CreateEntity("assets/prefabs/deployable/large wood storage/box.wooden.large.prefab", position, rotation) as StorageContainer;
            if (entity != null)
            {
                entity.skinID = 813269955;
                entity.Spawn();
                specialChests.Add(entity);
                player.ChatMessage("Special chest spawned!");
            }
        }

        [ChatCommand("get")]
        private void SpawngetCommand(BasePlayer player, string command, string[] args)
        {
            //Item key1 = ItemManager.CreateByName("fridge", 1, 2925286668);
            //player.GiveItem(key1);
            Item key = ItemManager.CreateByName("blood", 1, 2095609692);
            key.name = "vote crate key";
            player.GiveItem(key);
            player.ChatMessage("You have received a key item!");
        }


        [ChatCommand("fire")]
        private void SpawnAndFirework(BasePlayer player)
        {
            Vector3 spawnPosition = player.transform.position + player.transform.forward * 2f; // Position in front of the player
            Quaternion spawnRotation = Quaternion.identity; // Default rotation

            string fireworkPrefab = "assets/prefabs/deployable/fireworks/mortarblue.prefab";

            var fireworkEntity = GameManager.server.CreateEntity(fireworkPrefab, spawnPosition, spawnRotation);

            if (fireworkEntity != null)
            {
                fireworkEntity.Spawn();
                timer.Once(1f, () =>
                {
                    Effect.server.Run("assets/bundled/prefabs/fx/explosions/explosion_02.prefab", fireworkEntity.transform.position);
                    fireworkEntity.Kill();
                });
            }
        }


        [ChatCommand("ui")]
        private void SpawnuiFirework(BasePlayer player)
        {
            int index = 1;

            // Repeat the firework effect (timer, loop for 20 iterations)
            timer.Repeat(0.5f, 20, () =>
            {
                for (int i = 0; i < 1; i++)  // 1 item per iteration
                {
                    // Create and animate a spin UI with an icon
                    CreateSpinUI(player, 0.5f, 20,0.2f, index, "https://www.dropbox.com/scl/fi/tril3ylyha7pwca81b8od/164x164.png?dl=1");
                    index++;
                }
            });
        }



        private object CanLootEntity(BasePlayer player, StorageContainer container)
        {
            if (specialChests.Contains(container))
            {
                Item heldItem = player.GetActiveItem();
                if (heldItem != null && heldItem.info.shortname == "blood")
                {
                    player.inventory.Take(null, ItemManager.FindItemDefinition(heldItem.info.shortname).itemid, 1);
                    Effect.server.Run("assets/bundled/prefabs/fx/explosions/explosion_02.prefab", container.transform.position);

                    return false; // block loot
                }
                else
                {
                    player.ChatMessage("You need a special item to open this chest!");
                    return false; // block loot
                }
            }
            return null;
        }

        object OnItemAction(Item item, string action, BasePlayer player)
        {
            if (item.info.shortname == "blood")
            {
                player.ChatMessage("You cannot drop this item.");
                return false;
            }
            return null;
        }

        void CreateSpinUI(BasePlayer player, float speed, int Repeat, float space, int index, string iconurl)
        {
            float ani = 0f;

            // Repeat the animation with timer
            timer.Repeat(speed, Repeat, () =>
            {
                ani += 0.05f;  // Increment the animation value for movement

                var container = new CuiElementContainer();

                // Destroy any existing UI to avoid overlap
                CuiHelper.DestroyUi(player, $"FadeText{index}");

                // Create the panel to hold the icon (transparent background, positioned dynamically)
                var panel = new CuiPanel
                {
                    Image = { Color = "0 0 0 0" },  // Transparent background
                    RectTransform = { AnchorMin = $"{ani} 0.5", AnchorMax = $"{ani + space} 0.6"}, // Position the panel
                    CursorEnabled = false
                };

                container.Add(panel, "Overlay", $"FadeText{index}");

                // Add an icon to the panel (stretch the image to fit the panel)
                var icon = new CuiElement
                {
                    Name = "IconElement",
                    Parent = $"FadeText{index}", // Attach icon to the correct panel
                    Components =
                    {
                new CuiImageComponent { Sprite = iconurl},  // Set the icon URL (external link or asset path)
                new CuiRectTransformComponent {AnchorMin = "0 0", AnchorMax = "1 1"}  // Stretch the image to fill the panel
                    }
                    
                };

                container.Add(icon);

                // Add the UI container to the player
                CuiHelper.AddUi(player, container);
            });
        }


        private void Unload()
        {
            specialChests.Clear();
        }
    }

}