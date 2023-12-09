using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Models;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.Tools;
using PuppetRoguelite.UI.Menus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.Components.TiledComponents
{
    public class ActionShop : TiledComponent
    {
        //components
        SpriteAnimator _animator;
        BoxCollider _collider;
        YSorter _ySorter;
        OriginComponent _originComponent;
        Interactable _interactable;
        ActionShopMenu _menu;

        //misc
        bool _isShowingMenu = false;

        public ActionShop(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            _collider = Entity.AddComponent(new BoxCollider(-26, -10, 55, 10));
            _collider.PhysicsLayer = 0;
            Flags.SetFlag(ref _collider.PhysicsLayer, (int)PhysicsLayers.Interactable);
            Flags.SetFlag(ref _collider.PhysicsLayer, (int)PhysicsLayers.Environment);

            _animator = Entity.AddComponent(new SpriteAnimator());
            _animator.SetLocalOffset(new Vector2(0, -26));

            _originComponent = Entity.AddComponent(new OriginComponent(_collider));

            _ySorter = Entity.AddComponent(new YSorter(_animator, _originComponent));

            _interactable = Entity.AddComponent(new Interactable(OnInteracted));

            //load sprites
            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Shops.Herb_shop);
            var sprites = Sprite.SpritesFromAtlas(texture, 70, 52);
            _animator.AddAnimation("Idle", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 0, 23));
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            if (!_animator.IsRunning)
            {
                _animator.Play("Idle");
            }
        }

        IEnumerator OnInteracted()
        {
            //get textbox manager
            var textboxManager = Entity.Scene.GetOrCreateSceneComponent<TextboxManager>();

            //pick lines
            List<DialogueLine> lines;
            lines = ActionShopDialogue.DefaultLines.RandomItem();

            //display text
            yield return textboxManager.DisplayTextbox(lines);

            _menu = Entity.Scene.CreateEntity("menu-ui").AddComponent(new ActionShopMenu(OnShopClosed));
            //_menu = Entity.Scene.Camera.AddComponent(new ActionShopMenu(OnShopClosed));

            //wait for menu to close
            _isShowingMenu = true;
            while (_isShowingMenu)
            {
                yield return null;
            }

            //show more lines
            lines = ActionShopDialogue.ExitLines.RandomItem();
            yield return textboxManager.DisplayTextbox(lines);
            Debug.Log("finished with shop");
        }

        void OnShopClosed()
        {
            _menu.Entity.Destroy();
            _isShowingMenu = false;
        }
    }
}
