#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Intents;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace AnimeSurvivalMod
{
    // ==========================================
    // 1. CLOCKWORK COLOSSUS (ACT 1 BOSS)
    // ==========================================
    public class TensionPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override LocString Title => new LocString("powers", "TENSION_POWER.name");
        public override LocString Description => new LocString("powers", "TENSION_POWER.description");

        public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target == Owner && Owner != null && result.TotalDamage > 0)
            {
                SetAmount(Math.Min(10, Amount + 1));
                Flash();
            }
            return Task.CompletedTask;
        }
    }

    public class ClockworkColossus : MonsterModel
    {
        public override int MinInitialHp => 280;
        public override int MaxInitialHp => 310;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");
        public override LocString Title => new LocString("monsters", "CLOCKWORK_COLOSSUS.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/axebot/axebot_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/axebot/axebot_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/axebot/axebot_die";

        public override async Task AfterAddedToRoom()
        {
            await base.AfterAddedToRoom();
            await PowerCmd.Apply<TensionPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateWindUp1 = new MoveState("COLOSSUS_WINDUP1", MoveWindUp1, new DefendIntent(), new BuffIntent());
            var stateSteamVent = new MoveState("COLOSSUS_STEAM", MoveSteamVent, new SingleAttackIntent(14), new DebuffIntent());
            var stateWindUp2 = new MoveState("COLOSSUS_WINDUP2", MoveWindUp2, new DefendIntent(), new BuffIntent());
            var stateOverdrive = new MoveState("COLOSSUS_OVERDRIVE", MoveOverdrive, new MultiAttackIntent(8, 4));
            var stateMalfunction = new MoveState("COLOSSUS_MALFUNCTION", MoveMalfunction, new StunIntent());

            stateWindUp1.FollowUpState = stateSteamVent;
            stateSteamVent.FollowUpState = stateWindUp2;
            stateWindUp2.FollowUpState = stateOverdrive;
            stateOverdrive.FollowUpState = stateMalfunction;
            stateMalfunction.FollowUpState = stateWindUp1;

            return new MonsterMoveStateMachine(new MonsterState[] { stateWindUp1, stateSteamVent, stateWindUp2, stateOverdrive, stateMalfunction }, stateWindUp1);
        }

        private async Task MoveWindUp1(IReadOnlyList<Creature> targets)
        {
            await CreatureCmd.GainBlock(Creature, 18, ValueProp.Move, null);
            var p = Creature.GetPower<TensionPower>();
            if (p != null) p.SetAmount(Math.Min(10, p.Amount + 3));
            await Cmd.Wait(0.2f);
        }

        private async Task MoveSteamVent(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -65f);
            await DamageCmd.Attack(14).FromMonster(this)
                .WithAttackerAnim("Attack", 0.15f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_fire_burst")
                .Execute(null);

            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null)
            {
                await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), player.Creature, 2m, Creature, null);
            }
            var p = Creature.GetPower<TensionPower>();
            if (p != null) p.SetAmount(Math.Min(10, p.Amount + 3));
            await Cmd.Wait(0.2f);
        }

        private async Task MoveWindUp2(IReadOnlyList<Creature> targets)
        {
            await CreatureCmd.GainBlock(Creature, 24, ValueProp.Move, null);
            var p = Creature.GetPower<TensionPower>();
            if (p != null) p.SetAmount(Math.Min(10, p.Amount + 4));
            await Cmd.Wait(0.2f);
        }

        private async Task MoveOverdrive(IReadOnlyList<Creature> targets)
        {
            var p = Creature.GetPower<TensionPower>();
            int tensionBonus = (p != null) ? p.Amount : 0;
            int hitDamage = 6 + tensionBonus;

            for (int i = 0; i < 4; i++)
            {
                await ModVfx.AttackLunge(Creature, -75f, 0.10f);
                await DamageCmd.Attack(hitDamage).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.10f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_big_slash")
                    .Execute(null);
                await Cmd.Wait(0.08f);
            }

            if (p != null) p.SetAmount(1);
            await Cmd.Wait(0.2f);
        }

        private async Task MoveMalfunction(IReadOnlyList<Creature> targets)
        {
            // Mechanism stalls from extreme tension! Loses all block, gains 2 Vulnerable, takes double damage!
            if (Creature.Block > 0)
            {
                await CreatureCmd.LoseBlock(new ThrowingPlayerChoiceContext(), Creature, Creature.Block, Creature);
            }
            await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), Creature, 2m, Creature, null);
            await Cmd.Wait(0.35f);
        }

        public override CreatureAnimator GenerateAnimator(MegaSprite controller)
        {
            if (controller == null) return null!;
            AnimState animState = new AnimState("idle_loop", isLooping: true);
            AnimState state = new AnimState("die");
            CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
            creatureAnimator.AddAnyState("Idle", animState);
            creatureAnimator.AddAnyState("Dead", state);
            return creatureAnimator;
        }
    }

    public class ClockworkColossusBoss : EncounterModel
    {
        public override RoomType RoomType => RoomType.Boss;
        public override string BossNodePath => "res://images/map/placeholder/vantom_boss_icon";
        public override MegaSkeletonDataResource? BossNodeSpineResource => null;
        public override IEnumerable<MonsterModel> AllPossibleMonsters => new[] { ModelDb.Monster<ClockworkColossus>() };
        public override float GetCameraScaling() => 0.82f;
        public override Vector2 GetCameraOffset() => Vector2.Down * 60f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<ClockworkColossus>().ToMutable(), null)
            };
        }
    }

    // ==========================================
    // 2. MYCELIAL BROODMOTHER (ACT 1 BOSS)
    // ==========================================
    public class CocoonIncubationPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override LocString Title => new LocString("powers", "COCOON_POWER.name");
        public override LocString Description => new LocString("powers", "COCOON_POWER.description");

        public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side == CombatSide.Enemy && Owner != null && Owner.IsAlive)
            {
                SetAmount(Amount - 1);
                if (Amount <= 0)
                {
                    Flash();
                    // Explodes!
                    foreach (var p in CombatState.Players)
                    {
                        await CreatureCmd.Damage(choiceContext, p.Creature, 12m, ValueProp.Unblockable | ValueProp.Move, Owner);
                        var sporeCards = new List<CardModel>
                        {
                            CombatState.CreateCard<SporeCard>(p),
                            CombatState.CreateCard<SporeCard>(p)
                        };
                        await CardPileCmd.AddGeneratedCardsToCombat(sporeCards, PileType.Discard, p);
                    }
                    await CreatureCmd.Kill(Owner);
                }
            }
        }
    }

    public class SporeCocoon : MonsterModel
    {
        public override int MinInitialHp => 24;
        public override int MaxInitialHp => 26;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");
        public override LocString Title => new LocString("monsters", "SPORE_COCOON.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Plant;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        public override async Task AfterAddedToRoom()
        {
            await base.AfterAddedToRoom();
            await PowerCmd.Apply<CocoonIncubationPower>(new ThrowingPlayerChoiceContext(), Creature, 2m, Creature, null);
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var statePulsate = new MoveState("COCOON_PULSATE", MovePulsate, new BuffIntent());
            statePulsate.FollowUpState = statePulsate;
            return new MonsterMoveStateMachine(new MonsterState[] { statePulsate }, statePulsate);
        }

        private async Task MovePulsate(IReadOnlyList<Creature> targets)
        {
            await Cmd.Wait(0.2f);
        }

        public override CreatureAnimator GenerateAnimator(MegaSprite controller)
        {
            if (controller == null) return null!;
            AnimState animState = new AnimState("idle_loop", isLooping: true);
            AnimState state = new AnimState("die");
            CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
            creatureAnimator.AddAnyState("Idle", animState);
            creatureAnimator.AddAnyState("Dead", state);
            return creatureAnimator;
        }
    }

    public class MycelialBroodmother : MonsterModel
    {
        public override int MinInitialHp => 260;
        public override int MaxInitialHp => 290;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");
        public override LocString Title => new LocString("monsters", "MYCELIAL_BROODMOTHER.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Plant;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateNursery = new MoveState("BROOD_NURSERY", MoveNursery, new DefendIntent(), new SummonIntent());
            var stateLash = new MoveState("BROOD_LASH", MoveLash, new SingleAttackIntent(16), new DebuffIntent());
            var stateEruption = new MoveState("BROOD_ERUPTION", MoveEruption, new MultiAttackIntent(8, 2));
            var stateToxicSpike = new MoveState("BROOD_SPIKE", MoveToxicSpike, new SingleAttackIntent(24), new DebuffIntent());

            stateNursery.FollowUpState = stateLash;
            stateLash.FollowUpState = stateEruption;
            stateEruption.FollowUpState = stateToxicSpike;
            stateToxicSpike.FollowUpState = stateNursery;

            return new MonsterMoveStateMachine(new MonsterState[] { stateNursery, stateLash, stateEruption, stateToxicSpike }, stateNursery);
        }

        private async Task MoveNursery(IReadOnlyList<Creature> targets)
        {
            await CreatureCmd.GainBlock(Creature, 22, ValueProp.Move, null);
            var aliveCocoons = CombatState?.Enemies.Where(e => e.IsAlive && e.Monster is SporeCocoon).ToList();
            if (aliveCocoons != null && aliveCocoons.Count > 0)
            {
                foreach (var c in aliveCocoons)
                {
                    await CreatureCmd.GainBlock(c, 10, ValueProp.Move, null);
                }
            }
            else
            {
                var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
                if (player != null && CombatState != null)
                {
                    var spores = new List<CardModel>
                    {
                        CombatState.CreateCard<SporeCard>(player),
                        CombatState.CreateCard<SporeCard>(player)
                    };
                    await CardPileCmd.AddGeneratedCardsToCombat(spores, PileType.Discard, player);
                }
            }
            await Cmd.Wait(0.2f);
        }

        private async Task MoveLash(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -70f);
            await DamageCmd.Attack(16).FromMonster(this)
                .WithAttackerAnim("Attack", 0.15f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_poison_impact")
                .Execute(null);

            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null)
            {
                await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), player.Creature, 2m, Creature, null);
            }
            await Cmd.Wait(0.2f);
        }

        private async Task MoveEruption(IReadOnlyList<Creature> targets)
        {
            int statusCount = 0;
            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null)
            {
                foreach (var pile in player.Piles)
                {
                    if (pile != null && pile.Cards != null)
                    {
                        statusCount += pile.Cards.Count(c => c != null && (c.Type == CardType.Status || c.Type == CardType.Curse));
                    }
                }
            }
            int bonus = Math.Min(12, statusCount * 2);

            for (int i = 0; i < 2; i++)
            {
                await ModVfx.AttackLunge(Creature, -60f, 0.10f);
                await DamageCmd.Attack(8 + bonus).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.12f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_attack_blunt")
                    .Execute(null);
                await Cmd.Wait(0.10f);
            }
        }

        private async Task MoveToxicSpike(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -85f);
            await DamageCmd.Attack(24).FromMonster(this)
                .WithAttackerAnim("Attack", 0.18f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_big_slash")
                .Execute(null);

            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null)
            {
                await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), player.Creature, 2m, Creature, null);
            }
            await Cmd.Wait(0.2f);
        }

        public override CreatureAnimator GenerateAnimator(MegaSprite controller)
        {
            if (controller == null) return null!;
            AnimState animState = new AnimState("idle_loop", isLooping: true);
            AnimState state = new AnimState("die");
            CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
            creatureAnimator.AddAnyState("Idle", animState);
            creatureAnimator.AddAnyState("Dead", state);
            return creatureAnimator;
        }
    }

    public class MycelialBroodmotherBoss : EncounterModel
    {
        public override RoomType RoomType => RoomType.Boss;
        public override string BossNodePath => "res://images/map/placeholder/the_kin_boss_icon";
        public override MegaSkeletonDataResource? BossNodeSpineResource => null;
        public override IEnumerable<MonsterModel> AllPossibleMonsters => new MonsterModel[]
        {
            ModelDb.Monster<MycelialBroodmother>(),
            ModelDb.Monster<SporeCocoon>()
        };
        public override float GetCameraScaling() => 0.82f;
        public override Vector2 GetCameraOffset() => Vector2.Down * 60f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<MycelialBroodmother>().ToMutable(), null),
                (ModelDb.Monster<SporeCocoon>().ToMutable(), null),
                (ModelDb.Monster<SporeCocoon>().ToMutable(), null)
            };
        }
    }

    // ==========================================
    // 3. SANGUINE INQUISITOR (ACT 1 BOSS)
    // ==========================================
    public class MartyrdomPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override LocString Title => new LocString("powers", "MARTYRDOM_POWER.name");
        public override LocString Description => new LocString("powers", "MARTYRDOM_POWER.description");

        private decimal _lastCalculatedStrength = 0;

        public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target != Owner || Owner == null) return;
            decimal missingHp = Math.Max(0m, (decimal)(Owner.MaxHp - Owner.CurrentHp));
            decimal targetStrength = Math.Floor(missingHp / 15m);
            decimal delta = targetStrength - _lastCalculatedStrength;

            if (delta > 0)
            {
                _lastCalculatedStrength = targetStrength;
                SetAmount((int)targetStrength);
                Flash();
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, delta, Owner, null);
            }
        }
    }

    public class SanguineInquisitor : MonsterModel
    {
        public override int MinInitialHp => 275;
        public override int MaxInitialHp => 305;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");
        public override LocString Title => new LocString("monsters", "SANGUINE_INQUISITOR.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        public override async Task AfterAddedToRoom()
        {
            await base.AfterAddedToRoom();
            await PowerCmd.Apply<MartyrdomPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateBrand = new MoveState("INQ_BRAND", MoveBrand, new SingleAttackIntent(14), new DebuffIntent());
            var stateFlagellation = new MoveState("INQ_FLAGELLATION", MoveFlagellation, new DefendIntent(), new BuffIntent());
            var stateJudgment = new MoveState("INQ_JUDGMENT", MoveJudgment, new SingleAttackIntent(24));
            var statePurifying = new MoveState("INQ_PURIFYING", MovePurifying, new MultiAttackIntent(7, 3));

            stateBrand.FollowUpState = stateFlagellation;
            stateFlagellation.FollowUpState = stateJudgment;
            stateJudgment.FollowUpState = statePurifying;
            statePurifying.FollowUpState = stateBrand;

            return new MonsterMoveStateMachine(new MonsterState[] { stateBrand, stateFlagellation, stateJudgment, statePurifying }, stateBrand);
        }

        private async Task MoveBrand(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -60f);
            await DamageCmd.Attack(14).FromMonster(this)
                .WithAttackerAnim("Attack", 0.15f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(null);

            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null)
            {
                await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), player.Creature, 2m, Creature, null);
            }
            await Cmd.Wait(0.2f);
        }

        private async Task MoveFlagellation(IReadOnlyList<Creature> targets)
        {
            // Self-harm feeds Martyrdom dynamically (+1 Strength per 15 HP lost)!
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Creature, 15m, ValueProp.Unblockable | ValueProp.Unpowered, Creature);
            await CreatureCmd.GainBlock(Creature, 22, ValueProp.Move, null);
            
            // Applies Weak to all players
            foreach (var p in CombatState.Players)
            {
                await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), p.Creature, 2m, Creature, null);
            }
            await Cmd.Wait(0.25f);
        }

        private async Task MoveJudgment(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -85f);
            await DamageCmd.Attack(24).FromMonster(this)
                .WithAttackerAnim("Attack", 0.18f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_big_slash")
                .Execute(null);
            await Cmd.Wait(0.2f);
        }

        private async Task MovePurifying(IReadOnlyList<Creature> targets)
        {
            for (int i = 0; i < 3; i++)
            {
                await ModVfx.AttackLunge(Creature, -65f, 0.10f);
                await DamageCmd.Attack(7).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.12f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_fire_burst")
                    .Execute(null);
                await Cmd.Wait(0.08f);
            }
        }

        public override CreatureAnimator GenerateAnimator(MegaSprite controller)
        {
            if (controller == null) return null!;
            AnimState animState = new AnimState("idle_loop", isLooping: true);
            AnimState state = new AnimState("die");
            CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
            creatureAnimator.AddAnyState("Idle", animState);
            creatureAnimator.AddAnyState("Dead", state);
            return creatureAnimator;
        }
    }

    public class SanguineInquisitorBoss : EncounterModel
    {
        public override RoomType RoomType => RoomType.Boss;
        public override string BossNodePath => "res://images/map/placeholder/waterfall_giant_boss_icon";
        public override MegaSkeletonDataResource? BossNodeSpineResource => null;
        public override IEnumerable<MonsterModel> AllPossibleMonsters => new[] { ModelDb.Monster<SanguineInquisitor>() };
        public override float GetCameraScaling() => 0.82f;
        public override Vector2 GetCameraOffset() => Vector2.Down * 60f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<SanguineInquisitor>().ToMutable(), null)
            };
        }
    }
}
