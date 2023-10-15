using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;
using PuppetRoguelite.Components.Characters;
using PuppetRoguelite.Enums;
using PuppetRoguelite.SceneComponents;
using System;
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
            _interactable.Emitter.AddObserver(InteractableEvents.Interacted, OnInteracted);
        }

        void OnInteracted()
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
                        var key = keys.First() as CerealBox;
                        var lines = new List<DialogueLine>()
                        {
                            new DialogueLine($"You try slotting the {key.Name} box into the shelf..."),
                            new DialogueLine($"Perfect fit! So, so satisfying.")
                        };
                        inventory.RemoveItem(key);
                        _slottedBox = key;
                        _slotted = true;

                        textboxManager.DisplayTextbox(lines);

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
