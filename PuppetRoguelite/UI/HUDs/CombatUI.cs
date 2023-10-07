using Nez;
using Nez.Sprites;
using Nez.UI;
using PuppetRoguelite.Components;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.UI.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PuppetRoguelite.UI.HUDs
{
    public class CombatUI : UICanvas
    {
        //elements
        Table _table;
        Label _playerHealthLabel;
        ProgressBar _apProgressBar;
        Label _playerApLabel;
        Table _topLeftTable;
        Table _topRightTable;
        Table _bottomMiddleTable;
        Table _apTable;
        Label _simTipLabel;

        //skins
        Skin _basicSkin;

        public override void Initialize()
        {
            base.Initialize();

            //base table
            _table = Stage.AddElement(new Table());
            _table.SetFillParent(true);
            _table.SetDebug(false);

            //load skin
            _basicSkin = Skin.CreateDefaultSkin();

            //arrange elements
            ArrangeElements();

            ConnectToGlobalEmitters();
        }

        public override void OnAddedToEntity()
        {
            base.OnAddedToEntity();

            ConnectToEmitters();
        }

        void ArrangeElements()
        {
            //status section in top left
            //var statusTable = new Table();
            //statusTable.Defaults().Left().SetSpaceBottom(5);
            //_table.Add(statusTable).Expand().Top().Left().SetPadTop(5).SetPadLeft(5);
            //_playerHealthLabel = new Label("HP: ", _basicSkin);
            //statusTable.Add(_playerHealthLabel);
            //statusTable.Row();
            //statusTable.Add(new Label("$", _basicSkin));
            //statusTable.Row();
            //statusTable.Add(new Label("&", _basicSkin));

            _topLeftTable = new Table().Top().Left().PadTop(5).PadLeft(5);
            _table.Add(_topLeftTable).Grow();
            _playerHealthLabel = new Label("HP: ", _basicSkin);
            _topLeftTable.Add(_playerHealthLabel);
            //_topLeftTable.Pack();

            _topRightTable = new Table().Top().Right().PadTop(5).PadRight(5);
            _table.Add(_topRightTable).Grow();
            _topRightTable.Add(new Label("$", _basicSkin));
            _topRightTable.Pack();

            _table.Row();

            var bottomTable = new Table();
            _table.Add(bottomTable).SetColspan(2).Grow().SetPadBottom(10);

            var bottomLeftTable = new Table();
            bottomTable.Add(bottomLeftTable).Width(480 * .75f).Expand().Bottom().Left();

            _apTable = new Table();
            _apTable.Defaults().SetSpaceRight(8).Grow();
            bottomLeftTable.Add(_apTable).Width(480 * .5f).Expand().Right();

            _simTipLabel = new Label("* Press 'C' to view Action Sequence", _basicSkin);
            _simTipLabel.SetWrap(true);
            _simTipLabel.SetVisible(false);
            bottomTable.Add(_simTipLabel).SetPadLeft(480 * .025f).SetPadRight(480 * .025f).Width(480 * .2f).Expand().Bottom().Right();

            _apTable.Add(new TextButton("test", _basicSkin));
            _apTable.Add(new TextButton("test", _basicSkin));

            //_bottomMiddleTable = new Table().PadLeft(10);
            //_bottomMiddleTable.DebugAll();
            //_table.Add(_bottomMiddleTable).Expand().SetFillY().SetColspan(2).Left();
            //var leftTable = new Table();
            //_bottomMiddleTable.Add(leftTable).Width(480 * .75f).Expand().Bottom().Right();
            //var apGroup = new Table();
            //leftTable.Add(apGroup).Width(480 * .5f).Right();
            //apGroup.Add(new TextButton("hi", _basicSkin));

            //var apGroup = new Table().PadBottom(5);
            //apGroup.Defaults().SetSpaceRight(8);
            //_bottomMiddleTable.Add(apGroup).Width(480 * .75f).Bottom().Right().Expand();
            //var horGroup = new Table();
            //apGroup.Add(horGroup).Width(480 * .5f).Right().Expand();
            //horGroup.Add(new TextButton("hi", _basicSkin));

            //var labelTable = new Table().Right().Bottom();
            //labelTable.DebugAll();
            //_bottomMiddleTable.Add(labelTable).Width(480 * .25f);
            //var simTipLabel = new Label("* Press 'C' to view Action Sequence", _basicSkin);
            //simTipLabel.SetWrap(true);
            //labelTable.Add(simTipLabel).Grow();

            //_bottomMiddleTable.Add(simTipLabel);
            //var labelContainer = new Container(simTipLabel);
            //labelContainer.SetRight();
            //_bottomMiddleTable.Add(labelContainer).Width(480 * .25f).Expand().Right();
            //apGroup.Add(new TextButton("hi", _basicSkin));
            //apGroup.Add(new TextButton("hi", _basicSkin));
            //apGroup.Pack();
            //_bottomMiddleTable.Pack();

            //ap table
            //_table.Row();
            //var apTable = new Table();
            //apTable.Defaults().Center();
            //_table.Add(apTable).Expand().Bottom().SetPadBottom(10);
            //_playerApLabel = new ApLabel("/", _basicSkin);
            //apTable.Add(_playerApLabel).SetSpaceBottom(5);
            //apTable.Row();
            //_apProgressBar = new ApProgressBar(_basicSkin);
            //apTable.Add(_apProgressBar);
        }

        void ConnectToEmitters()
        {
            //player health
            var playerHealthComponent = Entity.Scene.FindComponentsOfType<HealthComponent>().FirstOrDefault(h => h.Entity.Name == "player");
            if (playerHealthComponent != null)
            {
                OnPlayerHealthChanged(playerHealthComponent);
                playerHealthComponent.Emitter.AddObserver(HealthComponentEventType.HealthChanged, OnPlayerHealthChanged);
            }
        }

        void ConnectToGlobalEmitters()
        {
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.DodgePhaseStarted, OnDodgePhaseStarted);
        }

        void OnDodgePhaseStarted()
        {
            //get initial hp values
            var playerHealthComponent = Entity.Scene.FindComponentsOfType<HealthComponent>().FirstOrDefault(h => h.Entity.Name == "player");
            if (playerHealthComponent != null)
            {
                OnPlayerHealthChanged(playerHealthComponent);
            }
        }

        public void OnPlayerHealthChanged(HealthComponent healthComponent)
        {
            _playerHealthLabel.SetText($"HP: {healthComponent.Health}/{healthComponent.MaxHealth}");
            _topLeftTable.Pack();
        }

        public void SetShowSimTipLabel(bool show)
        {
            _simTipLabel.SetVisible(show);
        }
    }
}
