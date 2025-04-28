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

            timer.Repeat(3f, 2, () =>
            {
                for (int i = 0; i < 10; i++)
                {
                    CreateSpinUI(player, 0.5f, 20, (float)i / 10,index);
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

        void CreateSpinUI(BasePlayer player, float speed, int Repeat, float space,int index)
        {
            float spaceL = space;
            float alpha = 1f;
            float ani = 0f;
            timer.Repeat(speed, Repeat, () =>
                        {
                            alpha -= 0.1f;
                            ani += 0.05f;

                            var container = new CuiElementContainer();
                            CuiHelper.DestroyUi(player, $"FadeText{index}");

                            container.Add(new CuiLabel
                            {
                                Text = { Text = $"{space}", FontSize = 30, Align = TextAnchor.MiddleCenter, Color = $"1 1 1 {alpha}" },
                                RectTransform = { AnchorMin = $"{ani} 0.5", AnchorMax = $"{ani + spaceL} 0.6" }
                            }, "Overlay", $"FadeText{index}");

                            CuiHelper.AddUi(player, container);
                        });
        }


        private void Unload()
        {
            specialChests.Clear();
        }
    }

}