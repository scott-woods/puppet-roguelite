using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.Models;
using PuppetRoguelite.SaveData;
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
    public class UpgradeShop : TiledComponent
    {
        //components
        SpriteAnimator _animator;
        BoxCollider _collider;
        YSorter _ySorter;
        OriginComponent _originComponent;
        Interactable _interactable;
        UpgradeShopMenu _menu;

        //misc
        bool _isShowingMenu = false;
        bool _purchaseMade = false;

        public UpgradeShop(TmxObject tmxObject, Entity mapEntity) : base(tmxObject, mapEntity)
        {
        }

        public override void Initialize()
        {
            base.Initialize();

            _collider = Entity.AddComponent(new BoxCollider(-48, -10, 93, 10));
            _collider.PhysicsLayer = 0;
            Flags.SetFlag(ref _collider.PhysicsLayer, (int)PhysicsLayers.Interactable);
            Flags.SetFlag(ref _collider.PhysicsLayer, (int)PhysicsLayers.Environment);

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
            _purchaseMade = false;

            //pick dialogue lines
            List<DialogueLine> lines;
            if (PlayerData.Instance.Dollahs >= 400)
                lines = UpgradeShopDialogue.HighWealth;
            else if (PlayerData.Instance.Dollahs >= 250)
                lines = UpgradeShopDialogue.MediumWealth;
            else if (PlayerData.Instance.Dollahs <= 50)
                lines = UpgradeShopDialogue.LowWealth;
            else
                lines = UpgradeShopDialogue.DefaultLines.RandomItem();

            //display text
            yield return GlobalTextboxManager.DisplayText(lines);

            //_menu = Entity.Scene.Camera.AddComponent(new UpgradeShopMenu(OnShopClosed));
            _menu = Entity.Scene.CreateEntity("upgrade-shop-ui")
                .AddComponent(new UpgradeShopMenu(OnShopClosed));

            //wait for menu to close
            _isShowingMenu = true;
            while (_isShowingMenu)
            {
                yield return null;
            }

            //show ending lines
            if (_purchaseMade)
                lines = UpgradeShopDialogue.ExitWithPurchase;
            else
                lines = UpgradeShopDialogue.ExitNoPurchase;

            //show more lines
            yield return GlobalTextboxManager.DisplayText(lines);
            Debug.Log("finished with shop");
        }

        void OnShopClosed(bool purchaseMade)
        {
            _purchaseMade = purchaseMade;
            _menu.Entity.Destroy();
            _isShowingMenu = false;
        }
    }
}
