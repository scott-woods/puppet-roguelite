using Nez;
using Nez.UI;
using PuppetRoguelite.Components.Cameras;
using PuppetRoguelite.Components.Characters.Enemies.ChainBot;
using PuppetRoguelite.Components.Characters.Enemies.Ghoul;
using PuppetRoguelite.Components.Characters.Enemies.OrbMage;
using PuppetRoguelite.Components.Characters.Enemies.Spitter;
using PuppetRoguelite.Components.Characters.Player;
using PuppetRoguelite.Components.Characters.Player.PlayerActions;
using PuppetRoguelite.Components.Characters.Player.PlayerActions.Attacks;
using PuppetRoguelite.Components.Characters.Player.PlayerActions.Support;
using PuppetRoguelite.Components.Characters.Player.PlayerActions.Utilities;
using PuppetRoguelite.Components.Characters.Player.States;
using PuppetRoguelite.Components.Shared;
using PuppetRoguelite.Components.TiledComponents;
using PuppetRoguelite.Entities;
using PuppetRoguelite.GlobalManagers;
using PuppetRoguelite.Models;
using PuppetRoguelite.SaveData;
using PuppetRoguelite.SceneComponents.CombatManager;
using PuppetRoguelite.Scenes;
using PuppetRoguelite.StaticData;
using PuppetRoguelite.UI.Menus;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PuppetRoguelite.Cutscenes
{
    public class TutorialCutscenes
    {
        public static IEnumerator DashTutorial()
        {
            //lock gates
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Gate_close);
            var gates = Game1.Scene.FindComponentsOfType<Gate>().Where(g => g.TmxObject.Name == "DashTutorialGate");
            foreach (var gate in gates)
            {
                gate.Lock();
            }

            //wait a second
            yield return Coroutine.WaitForSeconds(1f);

            //stop player
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            //read dialogue
            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Alright, congratulations on knowing enough to at least get yourself locked in this room!"),
                new DialogueLine("First thing you'll want to know is how to Dash. Or is it a Dodge Roll? Something like that..."),
                new DialogueLine("Just press Space while you're moving in any direction, and you'll see what I mean.")
            });

            //allow player to move
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);

            //wait for player to dash
            while (PlayerController.Instance.StateMachine.CurrentState.GetType() != typeof(DashState))
                yield return null;

            //wait for dash to finish
            while (PlayerController.Instance.StateMachine.CurrentState.GetType() == typeof(DashState))
                yield return null;

            //stop player movement again
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            //more text
            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Nice! Dashing is a good way to move around a bit quicker."),
                new DialogueLine("You'll also go right through enemy attacks during a Dash without a scratch!"),
                new DialogueLine("Just keep in mind that there's a short cooldown after each one. You'll get a feel for it."),
                new DialogueLine("When you're ready, go on ahead to the next room.")
            });

            //allow player to move
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);

            //unlock gates
            foreach (var gate in gates)
            {
                gate.Unlock();
            }
        }

        public static IEnumerator AttackTutorial()
        {
            //set player actions
            PlayerController.Instance.ActionsManager.AttackActions = new List<PlayerActionType>()
            {
                PlayerActionType.FromType(typeof(DashAttack))
            };
            PlayerController.Instance.ActionsManager.UtilityActions.Clear();
            PlayerController.Instance.ActionsManager.SupportActions.Clear();

            //get camera
            var camera = Game1.Scene.Camera.GetComponent<DeadzoneFollowCamera>();

            //destroy other triggers
            var triggers = Game1.Scene.FindComponentsOfType<AreaTrigger>().Where(t => t.TmxObject.Name == "AttackTutorial");
            foreach (var trigger in triggers)
            {
                trigger.Entity.Destroy();
            }

            //lock gates
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Gate_close);
            var gates = Game1.Scene.FindComponentsOfType<Gate>().Where(g => g.TmxObject.Name == "AttackTutorialGate");
            foreach (var gate in gates)
            {
                gate.Lock();
            }

            //wait a second
            yield return Coroutine.WaitForSeconds(1f);

            //stop player
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            //read dialogue
            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Dashing is cool and all, but you won't survive in this place without a way to fight back."),
                new DialogueLine("That sword you've got there is a bit of a relic, but looks like someone made a few modifications to it."),
                new DialogueLine("It may not be the strongest thing out there, but it's incredibly lightweight and easy to swing."),
                new DialogueLine("Just aim your Cursor in the direction you want to attack, and Left Click to attack.")
            });

            //let player move
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);

            //wait for melee
            while (PlayerController.Instance.StateMachine.CurrentState.GetType() != typeof(AttackState))
                yield return null;

            //wait for melee to finish
            while (PlayerController.Instance.StateMachine.CurrentState.GetType() == typeof(AttackState))
                yield return null;

            //stop player
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            //successful attack dialogue
            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Well done! If you didn't already notice, you can attack in a 3-hit combo if you keep clicking during the attack."),
                new DialogueLine("You can even change direction mid-attack if you move your cursor!"),
                new DialogueLine("...okay, maybe that's not actually that exciting. But good to know!"),
                new DialogueLine("Let's move on to a more practical application, shall we?"),
            });

            //start encounter
            Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterStarted);

            //wait one frame
            yield return null;

            //manually change to tutorial state
            Game1.GameStateManager.GameState = GlobalManagers.GameState.Tutorial;

            //get enemy spawn
            var enemySpawnPoint = Game1.Scene.FindComponentsOfType<EnemySpawnPoint>().Where(s => s.TmxObject.Name == "DummyEnemySpawn").FirstOrDefault();

            //spawn enemy
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Enemy_spawn);
            var enemy = Game1.Scene.AddEntity(new PausableEntity("dummy-enemy")).AddComponent(new ChainBot(Game1.Scene.FindEntity("map")));
            enemy.Entity.SetPosition(enemySpawnPoint.Entity.Position);
            var combatManager = Game1.Scene.GetOrCreateSceneComponent<CombatManager>();
            combatManager.AddEnemy(enemy);

            //move camera to enemy
            camera.SetFollowTarget(null);
            var camTween = camera.Entity.TweenPositionTo(enemy.Entity.Position, .5f);
            camTween.Start();

            //wait until camera has finished moving
            while (camTween.IsRunning())
                yield return null;

            //read lines
            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("See that guy? He's an old Chain Bot, and normally they'll want to kill you."),
                new DialogueLine("Don't worry though, this one has given up on its hopes and dreams. How fortunate!"),
                new DialogueLine("His misery shall pave the way for your education. Go ahead, give him a wack.")
            });

            //return camera to player
            camTween = camera.Entity.TweenPositionTo(PlayerController.Instance.Entity.Position);
            camTween.Start();
            while (camTween.IsRunning())
                yield return null;
            camera.SetFollowTarget(PlayerController.Instance.Entity);

            //allow player to move
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);

            //wait for enemy to take damage
            var enemyHealthComponent = enemy.GetComponent<HealthComponent>();
            while (enemyHealthComponent.Health == enemyHealthComponent.MaxHealth)
                yield return null;

            //wait for player to finish their attack
            while (PlayerController.Instance.StateMachine.CurrentState.GetType() == typeof(AttackState))
                yield return null;

            //stop player
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            //lines
            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("See? Doesn't bother him at all."),
                new DialogueLine("Listen up now, this is where things get a bit more complex."),
                new DialogueLine("You see those little Bars at the bottom of your view? Those are your AP, or Action Point, bars."),
                new DialogueLine("That's the real modification they made to your sword; dealing damage charges up your abilities."),
                new DialogueLine("As long as you have at least one, you can trigger the Turn State, freezing time and giving you a moment to think.")
            });

            var apComp = PlayerController.Instance.ActionPointComponent;
            if (apComp.ActionPoints > 0)
            {
                yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
                {
                    new DialogueLine("Looks like you've already got one AP there, nice job!")
                });
            }
            else
            {
                yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
                {
                    new DialogueLine("Give him a few more hits, and notice how your meter fills up.")
                });

                //allow player to move
                Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);

                //wait for ap
                while (apComp.ActionPoints < 1)
                    yield return null;

                //wait for attack to finish
                if (PlayerController.Instance.StateMachine.CurrentState.GetType() == typeof(AttackState))
                {
                    while (PlayerController.Instance.StateMachine.CurrentState.GetType() == typeof(AttackState))
                        yield return null;
                }

                //stop player
                Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

                yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
                {
                    new DialogueLine("Nice work! Now you've got a single Action Point to work with.")
                });
            }

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("You'll need a little bit more than that to do most things though."),
                new DialogueLine("Luckily, we've got the power of Tutorial Cheats on our side. Here's an extra point.")
            });

            //add ap and wait a moment
            apComp.ActionPoints += 1;
            yield return Coroutine.WaitForSeconds(1f);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Isn't it nice to get things for free?"),
                new DialogueLine("Now for the fun part. Stand near the Chain Bot, and try pressing 'E' to activate the Turn Phase.")
            });

            //allow player movement
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);

            //disable player melee
            PlayerController.Instance.IsMeleeEnabled = false;

            //change to AttackTutorial state so turn can be triggered
            Game1.GameStateManager.GameState = GlobalManagers.GameState.AttackTutorial;

            //wait for turn to be triggered
            while (combatManager.CombatState != CombatState.Turn)
                yield return null;

            //disable menu
            var actionTypeSelector = Game1.Scene.FindComponentOfType<ActionTypeSelector>();
            actionTypeSelector.DisableForTutorial();
            actionTypeSelector.SetTutorialModeEnabled(true);

            yield return Coroutine.WaitForSeconds(1f);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("This is the Turn State. As you can see, that Chain Bot has stopped moving."),
                new DialogueLine("Above your head, you'll see a few buttons. Let's go over your options."),
                new DialogueLine("The first one there is for Attack type Actions."),
                new DialogueLine("Attacks are your main source of damage, and generally your most important Actions.")
            });

            actionTypeSelector.Stage.SetGamepadFocusElement(actionTypeSelector.UtilitiesButton);

            yield return Coroutine.WaitForSeconds(.5f);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("This one is for Utility Actions."),
                new DialogueLine("Utilities usually don't deal damage directly, but offer useful ways to improve your combos.")
            });

            actionTypeSelector.Stage.SetGamepadFocusElement(actionTypeSelector.SupportButton);

            yield return Coroutine.WaitForSeconds(.5f);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("This is for Support Actions."),
                new DialogueLine("Support Actions can heal you, or provide temporary buffs to your Stats.")
            });

            actionTypeSelector.Stage.SetGamepadFocusElement(actionTypeSelector.ExecuteButton);

            yield return Coroutine.WaitForSeconds(.5f);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Finally, this is the Execute Button."),
                new DialogueLine("Whenever you're ready to execute your Actions and resume normal combat, you'll hit this."),
                new DialogueLine("Time will start moving again immediately, so enemies might have time to move out of the way of your Actions."),
                new DialogueLine("You move pretty quick during your Actions, but it's still good to keep this in mind."),
                new DialogueLine("Also, know that once you Execute, any AP you haven't used will go to waste!"),
                new DialogueLine("You can navigate the menu with the Mouse or Keyboard, and Left Click or press 'E' to select."),
                new DialogueLine("Go ahead and open up the Attacks menu.")
            });

            //enable ui control
            actionTypeSelector.Stage.KeyboardEmulatesGamepad = true;
            actionTypeSelector.AttackButton.SetDisabled(false);

            //wait for attack button to be clicked
            actionTypeSelector.AttackButton.OnClicked += AttackButtonClickedHandler;
            bool buttonClicked = false;
            void AttackButtonClickedHandler(Button button)
            {
                buttonClicked = true;
                actionTypeSelector.AttackButton.OnClicked -= AttackButtonClickedHandler;
            }
            while (!buttonClicked)
                yield return null;

            //get action menu
            var actionMenu = Game1.Scene.FindComponentOfType<ActionMenu>();
            actionMenu.DisableForTutorial();
            actionMenu.SetTutorialModeEnabled(true);

            yield return Coroutine.WaitForSeconds(1f);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("This is the Action Menu. It will display your equipped Actions for the category you selected."),
                new DialogueLine("On the left is the name of the Action, and the right shows the AP cost."),
                new DialogueLine("Please select your 'Dash' Action.")
            });

            //enable menu nav
            actionMenu.Stage.KeyboardEmulatesGamepad = true;

            //enable dash button
            var listButtons = actionMenu.GetButtons();
            var dashButton = listButtons.FirstOrDefault(b => b.GetText() == PlayerActionUtils.GetName(typeof(DashAttack)));
            dashButton.SetDisabled(false);
            dashButton.SetMouseEnabled(true);

            //wait for dash button clicked
            dashButton.OnClicked += DashButtonClickedHandler;
            bool dashButtonClicked = false;
            void DashButtonClickedHandler(Button button)
            {
                dashButtonClicked = true;
                dashButton.OnClicked -= DashButtonClickedHandler;
            }
            while (!dashButtonClicked)
                yield return null;

            //get dash attack component and disable confirmation
            var dashAttack = Game1.Scene.EntitiesOfType<DashAttack>().FirstOrDefault();
            dashAttack.SetConfirmationEnabled(false);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Most Actions require some kind of setup."),
                new DialogueLine("Generally, you'll aim with the mouse, and Left Click or press E to confirm."),
                new DialogueLine("While in this prep phase, you can see a little simulation of what will happen."),
                new DialogueLine("Some Actions, like heals or buffs, don't require any preparation."),
                new DialogueLine("Up to this point, you can press 'X' at any time to go back."),
                new DialogueLine("That's currently disabled for the purpose of your EDUCATION, however."),
                new DialogueLine("Try to aim the Dash Attack such that you'll go through the Chain Bot.")
            });

            yield return Coroutine.WaitForSeconds(.5f);

            //enable confirmation and wait for prep to finish
            dashAttack.SetConfirmationEnabled(true);
            bool dashAttackPrepFinished = false;
            dashAttack.Emitter.AddObserver(PlayerActionEvent.PrepFinished, OnDashPrepFinished);
            void OnDashPrepFinished(PlayerAction action)
            {
                dashAttack.Emitter.RemoveObserver(PlayerActionEvent.PrepFinished, OnDashPrepFinished);
                dashAttackPrepFinished = true;
            }
            while (!dashAttackPrepFinished)
                yield return null;

            //disable action type selector again
            actionTypeSelector = Game1.Scene.FindComponentOfType<ActionTypeSelector>();
            actionTypeSelector.DisableForTutorial();
            actionTypeSelector.SetTutorialModeEnabled(true);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Well, I have no way of knowing whether or not you're going to actually hit him..."),
                new DialogueLine("But rest assured, I'll be sure to mock you relentlessly if you somehow miss."),
                new DialogueLine("Take note that your perspective has shifted to where your Dash Attack will take you."),
                new DialogueLine("As you queue up Actions, subsequent Actions will be based off of your most recent location."),
                new DialogueLine("You can queue as many Actions as you can afford with the AP you started the Turn with."),
                new DialogueLine("Once you're done, press Execute to perform everything you setup!"),
                new DialogueLine("Since the Dash Attack cost all of your AP, there's nothing else you can do this Turn."),
                new DialogueLine("Click the Execute Button and see what happens!")
            });

            //set enemy health to 1
            enemyHealthComponent.Health = 1;

            //enable execute button and wait for click
            actionTypeSelector.Stage.KeyboardEmulatesGamepad = true;
            actionTypeSelector.ExecuteButton.SetDisabled(false);
            actionTypeSelector.ExecuteButton.SetMouseEnabled(true);
            actionTypeSelector.ExecuteButton.OnClicked += ExecuteClickedHandler;
            bool executeClicked = false;
            void ExecuteClickedHandler(Button button)
            {
                actionTypeSelector.ExecuteButton.OnClicked -= ExecuteClickedHandler;
                executeClicked = true;
            }
            while (!executeClicked)
                yield return null;

            int attempts = 0;
            while (attempts < 4)
            {
                //wait for execution to finish
                bool hasKilledEnemy = false;
                bool hasExecutionFinished = false;
                void OnExecutionFinished()
                {
                    Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.TurnPhaseCompleted, OnExecutionFinished);

                    if (enemyHealthComponent.Health <= 0)
                        hasKilledEnemy = true;

                    hasExecutionFinished = true;
                }
                Emitters.CombatEventsEmitter.AddObserver(CombatEvents.TurnPhaseCompleted, OnExecutionFinished);
                while (!hasExecutionFinished)
                    yield return null;

                //stop player
                Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

                //if execution finished without killing enemy
                if (!hasKilledEnemy)
                {
                    List<List<DialogueLine>> missLines = new List<List<DialogueLine>>()
                    {
                        new List<DialogueLine>()
                        {
                            new DialogueLine("Wow, you actually missed?"),
                            new DialogueLine("Okay, to be fair, maybe you didn't know the range of the Dash yet."),
                            new DialogueLine("I'm going to hope that that's the problem and give you another chance.")
                        },
                        new List<DialogueLine>()
                        {
                            new DialogueLine("You missed again? Really?"),
                            new DialogueLine("I wasn't expecting this.. I didn't actually have any jokes ready to mock you with."),
                            new DialogueLine("Give me a second, I'll come up with something while you try again.")
                        },
                        new List<DialogueLine>()
                        {
                            new DialogueLine("Alright, I've got one!"),
                            new DialogueLine("I asked you to use the Dash Attack, but I think you accidentally used the Trash Attack!"),
                            new DialogueLine("..."),
                            new DialogueLine("Comedy under pressure is not my strong suit. Just try again...")
                        }
                    };

                    if (attempts < 3)
                    {
                        yield return GlobalTextboxManager.DisplayText(missLines[attempts]);

                        //add more ap
                        apComp.ActionPoints += 2;

                        //allow player to move
                        Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);

                        attempts++;
                        continue;
                    }
                    else
                    {
                        yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
                        {
                            new DialogueLine("This poor Chain Bot just wants to be put out of his misery, and yet you deny him."),
                            new DialogueLine("You truly are a monster."),
                            new DialogueLine("Fortunately, I have the strength to do what you could not.")
                        });

                        enemyHealthComponent.Health = 0;
                        break;
                    }
                }
                else
                {
                    Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

                    yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
                    {
                        new DialogueLine("Nicely done.")
                    });
                    break;
                }
            }

            //allow player to move
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);

            yield return Coroutine.WaitForSeconds(1f);

            //stop player
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Lucky you! Looks like the Chain Bot had a few Dollahs on him."),
                new DialogueLine("Whoever made you hooked you up with a Dollah Magnet, it seems."),
                new DialogueLine("Don't worry about picking up Dollahs, they'll always go straight to you."),
                new DialogueLine("You'll gain several Dollahs as you defeat enemies, and you keep what you earn when you return to the Hub."),
                new DialogueLine("Remember how I said you can string together several Actions in one Turn?"),
                new DialogueLine("The more enemies you defeat in one turn, the number of Dollahs they'll drop multiplies!"),
                new DialogueLine("If you're efficient with your Turns, you can make substantially more Dollahs in every fight.")
            });

            //allow movement
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);

            yield return Coroutine.WaitForSeconds(1f);

            //stop player
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Your overall goal is simple: defeat enemies, and find the two Chests in the dungeon."),
                new DialogueLine("The Chests contain keys that you can use to unlock the Boss Gate."),
                new DialogueLine("The gate has two bookcases on either side. Use the keys on those shelves to unlock the Gate."),
                new DialogueLine("Once you go through the Boss Gate, you can't return to the main Dungeon."),
                new DialogueLine("Defeating the Boss will grant you the schematics for a new Action, plus a nice sum of Dollahs!"),
                new DialogueLine("Now, you ready for a real fight? The room ahead contains an example Key Chest, but several enemies, too."),
                new DialogueLine("Don't forget to experiment with your Utility and Support Actions! Good luck!")
            });

            //unlock gates
            foreach (var gate in gates)
                gate.Unlock();

            //enable melee
            PlayerController.Instance.IsMeleeEnabled = true;

            //allow movement
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);
        }

        public static IEnumerator CombatTutorial()
        {
            //give player util and support actions
            PlayerController.Instance.ActionsManager.UtilityActions.Add(PlayerActionType.FromType(typeof(Teleport)));
            PlayerController.Instance.ActionsManager.SupportActions.Add(PlayerActionType.FromType(typeof(HealingAura)));

            //lock gates
            Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Gate_close);
            var gates = Game1.Scene.FindComponentsOfType<Gate>().Where(g => g.TmxObject.Name == "CombatTutorialGate").ToList();
            foreach (var gate in gates)
            {
                gate.Lock();
            }

            //stop music
            Game1.AudioManager.StopMusic();

            //wait 1 second before spawning enemies
            yield return Coroutine.WaitForSeconds(1f);

            //play music
            Game1.AudioManager.PlayMusic(Songs.BabyFight);

            //emit combat started
            Emitters.CombatEventsEmitter.Emit(CombatEvents.EncounterStarted);

            //spawn enemies
            var combatManager = Game1.Scene.GetOrCreateSceneComponent<CombatManager>();
            var spawnPoints = Game1.Scene.FindComponentsOfType<EnemySpawnPoint>();
            foreach (var spawnPoint in spawnPoints)
            {
                Type enemyType;
                switch (spawnPoint.TmxObject.Name)
                {
                    case "TutorialGhoul":
                        enemyType = typeof(Ghoul); break;
                    case "TutorialChainbot":
                        enemyType = typeof(ChainBot); break;
                    case "TutorialSpitter":
                        enemyType = typeof(Spitter); break;
                    case "TutorialOrbMage":
                        enemyType = typeof(OrbMage); break;
                    default:
                        enemyType = null; break;
                }

                if (enemyType != null)
                {
                    Game1.AudioManager.PlaySound(Nez.Content.Audio.Sounds.Enemy_spawn);
                    combatManager.AddEnemy(spawnPoint.SpawnEnemy(enemyType));
                    yield return Coroutine.WaitForSeconds(.2f);
                }
            }

            bool hasCombatEnded = false;
            void OnCombatEnded()
            {
                Emitters.CombatEventsEmitter.RemoveObserver(CombatEvents.EncounterEnded, OnCombatEnded);
                hasCombatEnded = true;
            }
            Emitters.CombatEventsEmitter.AddObserver(CombatEvents.EncounterEnded, OnCombatEnded);
            while (!hasCombatEnded)
                yield return null;

            //stop music
            Game1.AudioManager.StopMusic();

            //unlock key gate
            var keyGate = gates.FirstOrDefault(g => g.TmxObject.Properties.TryGetValue("KeyGate", out var value));
            keyGate.Unlock();
            gates.Remove(keyGate);

            //stop player
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Excellent work! You managed to defeat a group of enemies without dying!"),
                new DialogueLine("Of course, I modified those ones so they couldn't actually kill you..."),
                new DialogueLine("But I think this still counts as an achievement. If you're willing to set the bar low."),
            });

            //play music
            Game1.AudioManager.PlayMusic(Songs.BabyMode);

            //allow movement
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);

            //wait for chest to be opened
            var tutorialChest = Game1.Scene.FindComponentOfType<TutorialChest>();
            tutorialChest.OnOpened += OnTutorialChestOpened;
            bool hasTutorialChestOpened = false;
            void OnTutorialChestOpened()
            {
                tutorialChest.OnOpened -= OnTutorialChestOpened;
                hasTutorialChestOpened = true;
            }
            while (!hasTutorialChestOpened)
                yield return null;

            //open other gates
            foreach (var gate in gates)
            {
                gate.Unlock();
            }
        }

        public static IEnumerator TutorialFinal()
        {
            //stop player
            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneStarted);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Welp, that's pretty much all I can teach you."),
                new DialogueLine("You see that Shelf over there?")
            });

            var camera = Game1.Scene.Camera.GetComponent<DeadzoneFollowCamera>();
            camera.SetFollowTarget(null);
            var tutorialShelf = Game1.Scene.FindComponentsOfType<InteractTrigger>().FirstOrDefault(i => i.TmxObject.Name == "TutorialShelf");
            var camTween = camera.Entity.TweenPositionTo(tutorialShelf.Entity.Position, .5f);
            camTween.Start();
            while (camTween.IsRunning())
                yield return null;

            yield return Coroutine.WaitForSeconds(.5f);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("It's exactly what you'll find next to the Boss Gate in the real dungeon."),
                new DialogueLine("You can unlock it by interacting with it once you've opened a Key Chest.")
            });

            camTween = camera.Entity.TweenPositionTo(PlayerController.Instance.Entity.Position, .5f);
            camTween.Start();
            while (camTween.IsRunning())
                yield return null;

            camera.SetFollowTarget(PlayerController.Instance.Entity);

            yield return Coroutine.WaitForSeconds(.5f);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("Oh, and one more thing before you go."),
                new DialogueLine("There's two shopowners back in the Hub, the Action Shop and Upgrade Shop."),
                new DialogueLine("The Action Shop is the kind old lady on the left, where you can swap out your Actions."),
                new DialogueLine("The Upgrade Shop is the gentleman on the right, where you can use your Dollahs to upgrade your stats."),
                new DialogueLine("Don't forget to use their services from time to time!"),
                new DialogueLine("When you're ready, interact with the Shelf so we can head on back home.")
            });

            Emitters.CutsceneEmitter.Emit(CutsceneEvents.CutsceneEnded);
        }

        public static IEnumerator TutorialShelf()
        {
            //return player actions to normal
            PlayerController.Instance.ActionsManager.AttackActions = PlayerData.Instance.AttackActions;
            PlayerController.Instance.ActionsManager.UtilityActions = PlayerData.Instance.UtilityActions;
            PlayerController.Instance.ActionsManager.SupportActions = PlayerData.Instance.SupportActions;

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("The slots on the Shelf look perfectly sized to fit that Cereal Box."),
                new DialogueLine("Your hands quiver as you sheepishly line it up...")
            });

            yield return Game1.AudioManager.PlaySoundCoroutine(Nez.Content.Audio.Sounds.Cereal_slot_1);

            yield return GlobalTextboxManager.DisplayText(new List<DialogueLine>()
            {
                new DialogueLine("The fit is perfect, down to the last nanometer."),
                new DialogueLine("Suddenly, satisfied with your efforts, you find yourself falling into a deep sleep...")
            });

            Game1.SceneManager.ChangeScene(typeof(NewHub), "0", delayBeforeFadeInDuration: 2f, fadeInDuration: 2f, fadeOutDuration: 2f);
        }
    }
}
