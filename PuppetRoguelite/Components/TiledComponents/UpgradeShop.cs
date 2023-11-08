using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Models;
using PuppetRoguelite.SceneComponents;
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
    public class UpgradeShop : TiledComponent
    {
        //entities
        Entity _menuEntity;

        //components
        SpriteAnimator _animator;
        BoxCollider _collider;
        YSorter _ySorter;
        OriginComponent _originComponent;
        Interactable _interactable;

        //misc
        bool _isShowingMenu = false;

        public UpgradeShop(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            _collider = Entity.AddComponent(new BoxCollider(-48, -10, 93, 10));

            _animator = Entity.AddComponent(new SpriteAnimator());
            _animator.SetLocalOffset(new Vector2(0, -54));

            _originComponent = Entity.AddComponent(new OriginComponent(_collider));

            _ySorter = Entity.AddComponent(new YSorter(_animator, _originComponent));

            _interactable = Entity.AddComponent(new Interactable(OnInteracted));

            //load sprites
            var texture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Shops.Tech_shop);
            var sprites = Sprite.SpritesFromAtlas(texture, 108, 108);
            _animator.AddAnimation("Idle", AnimatedSpriteHelper.GetSpriteArrayFromRange(sprites, 0, 17));
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
            //display text
            var textboxManager = Entity.Scene.GetOrCreateSceneComponent<TextboxManager>();
            var lines = new List<DialogueLine>()
            {
                new DialogueLine("Baba booey"),
            };
            yield return textboxManager.DisplayTextbox(lines);

            //display shop
            if (_menuEntity == null)
            {
                _menuEntity = Entity.Scene.CreateEntity("shop-ui");
                _menuEntity.AddComponent(new UpgradeShopMenu(OnShopClosed));
            }
            else _menuEntity.SetEnabled(true);

            //while menu is showing, wait
            _isShowingMenu = true;
            while (_isShowingMenu)
            {
                yield return null;
            }

            //show more lines
            lines = new List<DialogueLine>()
            {
                new DialogueLine("Dash fast eat ass, kid.")
            };
            yield return textboxManager.DisplayTextbox(lines);
            Debug.Log("finished with shop");
        }

        void OnShopClosed()
        {
            _menuEntity.SetEnabled(false);
            _isShowingMenu = false;
        }
    }
}
