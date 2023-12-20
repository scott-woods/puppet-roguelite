using Microsoft.Xna.Framework;
using Nez;
using Nez.Sprites;
using Nez.Textures;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Enums;
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.Models;
using PuppetRoguelite.SaveData;
using PuppetRoguelite.SaveData.Unlocks;
using PuppetRoguelite.SaveData.Upgrades;
using PuppetRoguelite.SceneComponents;
using PuppetRoguelite.Scenes;
using PuppetRoguelite.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Nez.Tweens.Easing;

namespace PuppetRoguelite.Components.Characters.NPCs.Proto
{
    public class Proto : Component
    {
        //components
        SpriteAnimator _animator;
        BoxCollider _collider;
        YSorter _ySorter;
        OriginComponent _originComponent;

        //misc
        bool _hasMentionedNewSchematic = false;

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
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            yield return GlobalTextboxManager.DisplayText(GetDialogueLines());

            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);
        }

        List<DialogueLine> GetDialogueLines()
        {
            //check for anything contextual
            if (!_hasMentionedNewSchematic)
            {
                if (DungeonRuns.Instance.Runs.Count > 0)
                {
                    var unlock = DungeonRuns.Instance.Runs.Last().UnlockedAction;
                    if (unlock != null)
                    {
                        _hasMentionedNewSchematic = true;
                        return ProtoDialogue.GetLinesForUnlock(unlock);
                    }
                }
            }

            //try upgrade suggestions
            if (PlayerUpgradeData.Instance.MaxHpUpgrade.CurrentLevel == 0 || PlayerUpgradeData.Instance.MaxApUpgrade.CurrentLevel == 0 ||
                PlayerUpgradeData.Instance.AttackSlotsUpgrade.CurrentLevel == 0 || PlayerUpgradeData.Instance.UtilitySlotsUpgrade.CurrentLevel == 0 ||
                PlayerUpgradeData.Instance.SupportSlotsUpgrade.CurrentLevel == 0)
            {
                if (Nez.Random.Chance(.1f))
                    return ProtoDialogue.GetUpgradeRecommendation();
            }

            return ProtoDialogue.DefaultLines.RandomItem();
        }
    }
}
