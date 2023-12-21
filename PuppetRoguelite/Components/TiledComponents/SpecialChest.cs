using Microsoft.Xna.Framework;
using Nez;
using Nez.Textures;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.Models;
using PuppetRoguelite.SaveData;
using PuppetRoguelite.SaveData.Unlocks;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.Scenes;
using PuppetRoguelite.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class SpecialChest : Chest
    {
        Sprite _openSprite, _closedSprite;

        public SpecialChest(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            //load sprites
            var chestTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Objects.Large_chest);
            _openSprite = new Sprite(chestTexture, new RectangleF(32, 0, 32, 32));
            _closedSprite = new Sprite(chestTexture, new RectangleF(0, 0, 32, 32));
            _renderer.SetSprite(_closedSprite);
            _renderer.SetRenderLayer(-(int)_renderer.Bounds.Center.Y);

            _collider = Entity.AddComponent(new BoxCollider(-16, 0, 32, 16));
            Flags.SetFlagExclusive(ref _collider.PhysicsLayer, (int)PhysicsLayers.Interactable);
        }

        public override IEnumerator HandleInteraction()
        {
            _renderer.SetSprite(_openSprite);
            _interactable.Active = false;

            //display text
            var lines = new List<DialogueLine>()
            {
                new DialogueLine("You look inside the box..."),
            };
            yield return GlobalTextboxManager.DisplayText(lines);

            //if any unlocks left to get, get a new unlock
            var unlocks = ActionUnlockData.Instance.Unlocks.Where(u => !u.IsUnlocked).ToList();
            if (unlocks.Any())
            {
                var unlock = unlocks.RandomItem();
                ActionUnlockData.Instance.UnlockAction(unlock.Action);

                DungeonRuns.Instance.SetRunUnlockedAction(unlock.Action);

                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Special_chest_sound);
                var unlockLines = new List<DialogueLine>()
                {
                    new DialogueLine($"You found the schematics for {PlayerActionUtils.GetName(unlock.Action.ToType())}!")
                };
                yield return GlobalTextboxManager.DisplayText(unlockLines);
            }
            else
            {
                //no new unlocks, just drop money i guess
                var dropper = Entity.AddComponent(new DollahDropper(75, 0));
                dropper.DropDollahs();

                Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Special_chest_sound);
                var dollahLines = new List<DialogueLine>()
                {
                    new DialogueLine("You found a bunch of dollahs!")
                };
                yield return GlobalTextboxManager.DisplayText(dollahLines);
            }

            //Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Space_ship_3);

            Game1.GameStateManager.ReturnToHubAfterSuccess();
        }
    }
}
