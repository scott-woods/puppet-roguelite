using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Characters.Player.PlayerComponents;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Models;
using PuppetRoguelite.Models.Items;
using PuppetRoguelite.SceneComponents;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class Shelf : TiledComponent
    {
        List<DialogueLine> _noKeyLines = new List<DialogueLine>()
        {
            new DialogueLine("This finely crafted shelf looks very out of place."),
            new DialogueLine("It's pretty sad to see the shelf so barren."),
            new DialogueLine("Interesting, the gap between shelves looks incredibly cereal box-shaped...")
        };

        bool _slotted = false;
        CerealBox _slottedBox;

        //SpriteRenderer _renderer;
        Collider _collider;
        Interactable _interactable;

        public Shelf(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            _collider = Entity.AddComponent(new BoxCollider(TmxObject.Width, TmxObject.Height));

            _interactable = Entity.AddComponent(new Interactable(OnInteracted, true));
        }

        IEnumerator OnInteracted()
        {
            var textboxManager = Entity.Scene.GetOrCreateSceneComponent<TextboxManager>();
            var player = PlayerController.Instance;

            if (!_slotted)
            {
                if (PlayerController.Instance.Entity.TryGetComponent<Inventory>(out var inventory))
                {
                    var keys = inventory.GetItems().Where(i => i.Type == ItemType.Key).ToList();
                    if (!keys.Any())
                    {
                        yield return textboxManager.DisplayTextbox(_noKeyLines);
                    }
                    else
                    {
                        //get key and make first line
                        var key = keys.First() as CerealBox;
                        var tryLines = new List<DialogueLine>()
                        {
                            new DialogueLine($"You try slotting the {key.Name} box into the shelf...")
                        };
                        yield return textboxManager.DisplayTextbox(tryLines);

                        //remove key
                        inventory.RemoveItem(key);
                        _slottedBox = key;
                        _slotted = true;

                        //play sound
                        var sounds = new List<string>()
                        {
                            Nez.Content.Audio.Sounds.Cereal_slot_1,
                            Nez.Content.Audio.Sounds.Cereal_slot_2,
                            Nez.Content.Audio.Sounds.Cereal_slot_3,
                        };
                        Game1.AudioManager.PauseMusic();
                        yield return Game1.AudioManager.PlaySoundCoroutine(sounds.RandomItem());

                        yield return Coroutine.WaitForSeconds(1.25f);

                        //resume music
                        Game1.AudioManager.ResumeMusic();

                        //second textbox
                        var successLines = new List<DialogueLine>()
                        {
                            new DialogueLine($"Perfect fit! So, so satisfying.")
                        };
                        yield return textboxManager.DisplayTextbox(successLines);

                        //handle boss gate
                        var bossGate = Entity.Scene.FindComponentOfType<BossGate>();
                        if (bossGate != null)
                        {
                            bossGate.AddKey();
                        }
                    }
                }
            }
            else
            {
                yield return textboxManager.DisplayTextbox(new List<DialogueLine>()
                {
                    new DialogueLine($"The box of {_slottedBox.Name} is resting just where you left it.")
                });
            }
        }
    }
}
