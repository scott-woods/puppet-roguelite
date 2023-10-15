﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Items;
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

        public Shelf(TmxObject tmxObject, string mapId) : base(tmxObject, mapId)
        {

        }

        public override void Initialize()
        {
            base.Initialize();

            _collider = Entity.AddComponent(new BoxCollider(TmxObject.Width, TmxObject.Height));

            _interactable = Entity.AddComponent(new Interactable());
            _interactable.Emitter.AddObserver(InteractableEvents.Interacted, () => Game1.StartCoroutine(OnInteracted()));
        }

        IEnumerator OnInteracted()
        {
            var textboxManager = Entity.Scene.GetOrCreateSceneComponent<TextboxManager>();

            if (!_slotted)
            {
                if (PlayerController.Instance.Entity.TryGetComponent<Inventory>(out var inventory))
                {
                    var keys = inventory.GetItems().Where(i => i.Type == ItemType.Key).ToList();
                    if (!keys.Any())
                    {
                        textboxManager.DisplayTextbox(_noKeyLines);
                    }
                    else
                    {
                        //get key and make first line
                        var key = keys.First() as CerealBox;
                        var tryLines = new List<DialogueLine>()
                        {
                            new DialogueLine($"You try slotting the {key.Name} box into the shelf...")
                        };
                        textboxManager.DisplayTextbox(tryLines);
                        while (textboxManager.IsActive)
                        {
                            yield return null;
                        }

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
                        var channel = Game1.AudioManager.PlaySound(sounds.RandomItem());
                        while (channel.IsPlaying)
                        {
                            yield return null;
                        }

                        //second textbox
                        var successLines = new List<DialogueLine>()
                        {
                            new DialogueLine($"Perfect fit! So, so satisfying.")
                        };
                        textboxManager.DisplayTextbox(successLines);
                        while (textboxManager.IsActive)
                        {
                            yield return null;
                        }

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
                textboxManager.DisplayTextbox(new List<DialogueLine>()
                {
                    new DialogueLine($"The box of {_slottedBox.Name} is resting just where you left it.")
                });
            }
        }
    }
}
