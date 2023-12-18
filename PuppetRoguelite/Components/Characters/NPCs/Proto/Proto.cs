using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.Models;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.Scenes;
using PuppetRoguelite.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.Characters.NPCs.Proto
{
    public class Proto : Component
    {
        //components
        SpriteAnimator _animator;
        BoxCollider _collider;
        YSorter _ySorter;
        OriginComponent _originComponent;

        public override void Initialize()
        {
            base.Initialize();

            _collider = Entity.AddComponent(new BoxCollider(-5, 4, 11, 5));
            _collider.PhysicsLayer = 0;
            Flags.SetFlag(ref _collider.PhysicsLayer, (int)PhysicsLayers.Interactable);
            Flags.SetFlag(ref _collider.PhysicsLayer, (int)PhysicsLayers.Environment);

            _animator = Entity.AddComponent(new SpriteAnimator());
            _animator.SetLocalOffset(new Vector2(0, -7));
            _animator.Speed = .5f;
            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Characters.Proto.Proto_spritesheet);
            var sprites = Sprite.SpritesFromAtlas(texture, 48, 32);
            _animator.AddAnimation("Idle", AnimatedSpriteHelper.GetSpriteArrayByRow(sprites, 3, 15, 15));

            _originComponent = Entity.AddComponent(new OriginComponent(_collider));

            _ySorter = Entity.AddComponent(new YSorter(_animator, _originComponent));

            Entity.AddComponent(new Shadow(_animator, new Vector2(0, 7), Vector2.One));

            Entity.AddComponent(new Interactable(HandleInteraction));
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            _animator.Play("Idle");
        }

        IEnumerator HandleInteraction()
        {
            var lines = new List<DialogueLine>()
            {
                new DialogueLine("aborignal taste")
            };

            yield return GlobalTextboxManager.DisplayText(lines);

            Game1.SceneManager.ChangeScene(typeof(TutorialRoom), "0");
        }
    }
}
