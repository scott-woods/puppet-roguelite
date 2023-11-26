using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez;
using Nez.Textures;
using Nez.UI;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Characters.Player.PlayerComponents;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.Models;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections.Generic;

namespace PuppetRoguelite.UI.HUDs
{
    public class CombatUI : CustomCanvas
    {
        //elements
        Table _table;
        Table _topRightTable;
        Table _apTable;
        Label _simTipLabel;
        Label _dollahLabel;
        List<BasicApProgressBar> _apProgressBars = new List<BasicApProgressBar>();
        Table _heartsTable;
        List<HeartElement> _hearts = new List<HeartElement>();

        //skins
        Skin _basicSkin;

        //textures
        List<SpriteDrawable> _heartSprites;
        Sprite _dollahSprite;

        //misc
        bool _shouldApTableBeVisible = false;

        #region LIFECYCLE

        public override void Initialize()
        {
            base.Initialize();

            Stage.IsFullScreen = true;

            //load skin
            _basicSkin = CustomSkins.CreateBasicSkin();

            //render layer
            SetRenderLayer((int)RenderLayers.ScreenSpaceRenderLayer);

            //load textures
            _heartSprites = new List<SpriteDrawable>()
            {
                new SpriteDrawable(Entity.Scene.Content.LoadTexture(Nez.Content.Textures.UI.Hearthealthsprite4)),
                new SpriteDrawable(Entity.Scene.Content.LoadTexture(Nez.Content.Textures.UI.Hearthealthsprite3)),
                new SpriteDrawable(Entity.Scene.Content.LoadTexture(Nez.Content.Textures.UI.Heartspritehealth2)),
                new SpriteDrawable(Entity.Scene.Content.LoadTexture(Nez.Content.Textures.UI.Heartspritehealth1)),
                new SpriteDrawable(Entity.Scene.Content.LoadTexture(Nez.Content.Textures.UI.Heartspritefullhealth)),
            };

            //dollah
            var dollahTexture = Entity.Scene.Content.LoadTexture(Nez.Content.Textures.Tilesets.Dungeon_prison_props);
            _dollahSprite = new Sprite(dollahTexture, new Rectangle(80, 240, 16, 16));
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            //connect to emitters
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterEnded, OnEncounterEnded);
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);

            //ap progress bars
            if (PlayerController.Instance.ActionPointComponent != null)
            {
                Emitters.ActionPointEmitter.AddObserver(ActionPointEvents.ActionPointsProgressChanged, OnActionPointsProgressChanged);
            }

            //player health
            if (PlayerController.Instance.Entity.TryGetComponent<HealthComponent>(out var hc))
            {
                hc.Emitter.AddObserver(HealthComponentEventType.MaxHealthChanged, OnPlayerMaxHealthChanged);
                hc.Emitter.AddObserver(HealthComponentEventType.HealthChanged, OnPlayerHealthChanged);
            }

            //dollahs
            if (PlayerController.Instance.Entity.TryGetComponent<DollahInventory>(out var dollahInventory))
            {
                dollahInventory.Emitter.AddObserver(DollahInventoryEvents.DollahsChanged, OnDollahsChanged);
            }

            CreateUI();
        }

        public override void OnRemovedFromEntity()
        {
            //connect to emitters
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.EncounterStarted, OnEncounterStarted);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.EncounterEnded, OnEncounterEnded);
            Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseTriggered, OnTurnPhaseTriggered);

            _apProgressBars.Clear();
            _apTable.ClearChildren();

            ClearUI();
        }

        #endregion

        void CreateUI()
        {
            //base table
            _table = Stage.AddElement(new Table());
            _table.SetWidth(Game1.UIResolution.X);
            _table.SetHeight(Game1.UIResolution.Y);
            _table.SetFillParent(false).Pad(Game1.UIResolution.Y * .04f);

            ArrangeElements();

            //ap progress bars
            if (PlayerController.Instance.ActionPointComponent != null)
            {
                //setup ap progress bars
                for (int i = 0; i < PlayerController.Instance.ActionPointComponent.MaxActionPoints; i++)
                {
                    var bar = new BasicApProgressBar(_basicSkin);
                    _apProgressBars.Add(bar);
                    _apTable.Add(bar).GrowX();
                }
            }

            //player health
            if (PlayerController.Instance.Entity.TryGetComponent<HealthComponent>(out var hc))
            {
                OnPlayerMaxHealthChanged(hc);
                OnPlayerHealthChanged(hc);
            }

            //dollahs
            if (PlayerController.Instance.Entity.TryGetComponent<DollahInventory>(out var dollahInventory))
            {
                OnDollahsChanged(dollahInventory);
            }
        }

        void ClearUI()
        {
            _table?.Clear();
            _table.Remove();
            _table = null;
            _apProgressBars?.Clear();
            _hearts?.Clear();
        }

        void ArrangeElements()
        {
            //player hearts
            _heartsTable = new Table();
            _heartsTable.Defaults().SetSpaceRight(Game1.UIResolution.X * .01f);
            _table.Add(_heartsTable)
                .Top().Left();

            _table.Row();

            //dollahs table
            var dollahsTable = new Table();
            _table.Add(dollahsTable).Top().Left().SetSpaceTop(Game1.UIResolution.Y * .01f);
            var dollahImage = new Image(_dollahSprite);
            dollahImage.SetScaleX(Game1.ResolutionScale.X * 1.25f);
            dollahImage.SetScaleY(Game1.ResolutionScale.Y * 1.25f);
            dollahsTable.Add(dollahImage);
            _dollahLabel = new Label("", _basicSkin, "default_xxl");
            dollahsTable.Add(_dollahLabel).SetPadTop(5);

            _table.Row();

            var bottomTable = new Table();
            _table.Add(bottomTable).Grow();

            //var bottomLeftTable = new Table();
            //bottomTable.Add(bottomLeftTable).Width(Game1.UIResolution.X * .75f).Expand().Bottom();

            _apTable = new Table();
            _apTable.Defaults().Space(50);
            _apTable.SetVisible(_shouldApTableBeVisible);
            bottomTable.Add(_apTable)
                .Bottom()
                .Expand()
                .SetFillX()
                .SetPadLeft(Game1.UIResolution.X * .1f)
                .SetPadRight(Game1.UIResolution.X * .1f)
                .SetPadBottom(Game1.UIResolution.Y * .05f);

            //_simTipLabel = new Label("* Press 'C' to view Action Sequence", _basicSkin);
            //_simTipLabel.SetWrap(true);
            //_simTipLabel.SetVisible(false);
            //bottomTable.Add(_simTipLabel).SetPadLeft(Game1.DesignResolution.X * .025f).SetPadRight(Game1.DesignResolution.X * .025f).Width(Game1.DesignResolution.X * .2f).Expand().Bottom().Right();
        }

        public void SetShowSimTipLabel(bool show)
        {
            _simTipLabel.SetVisible(show);
        }

        #region OBSERVERS

        void OnDollahsChanged(DollahInventory inv)
        {
            _dollahLabel.SetText($"{inv.Dollahs}");
        }

        void OnEncounterStarted()
        {
            //get initial hp values
            var playerHealthComponent = PlayerController.Instance.Entity.GetComponent<HealthComponent>();
            if (playerHealthComponent != null)
            {
                OnPlayerHealthChanged(playerHealthComponent);
            }

            //show relevant elements
            _shouldApTableBeVisible = true;
            _apTable.SetVisible(true);
        }

        void OnEncounterEnded()
        {
            //hide elements
            _shouldApTableBeVisible = false;
            _apTable.SetVisible(false);
        }

        void OnTurnPhaseTriggered()
        {
            //var apComp = PlayerController.Instance.ActionPointComponent;

            //if (apComp.ActionPoints < apComp.MaxActionPoints)
            //{
            //    _apProgressBars[apComp.ActionPoints].SetValue(0);
            //}
        }

        void OnActionPointsProgressChanged(ActionPointComponent apComp)
        {
            int fullBars = apComp.ActionPoints;
            float partialProgress = (float)apComp.DamageAccumulated / apComp.DamageRequiredPerPoint;

            for (int i = 0; i < _apProgressBars.Count; i++)
            {
                if (i < fullBars)
                {
                    _apProgressBars[i].SetValue(1f);
                }
                else if (i == fullBars)
                {
                    _apProgressBars[i].SetValue(partialProgress);
                }
                else
                {
                    _apProgressBars[i].SetValue(0);
                }
            }
        }

        public void OnPlayerHealthChanged(HealthComponent healthComponent)
        {
            var hp = healthComponent.Health;
            foreach (var heart in _hearts)
            {
                var fillLevel = Math.Min(hp, 4);
                heart.SetFillLevel(fillLevel);
                hp -= 4;
            }
        }

        void OnPlayerMaxHealthChanged(HealthComponent hc)
        {
            _heartsTable.Clear();

            int hp = hc.Health;
            for (int i = 0; i < Math.Ceiling((decimal)hc.MaxHealth / 4); i++)
            {
                var fillLevel = Math.Min(hp, 4);
                var heart = new HeartElement(_heartSprites, fillLevel);
                _heartsTable.Add(heart);
                _hearts.Add(heart);
                hp -= 4;
            }
        }

        #endregion
    }
}
