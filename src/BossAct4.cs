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
    // Beat of Death Power / Пульс смерти
    // ==========================================
    public class BeatOfDeathPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override LocString Title => new LocString("powers", "BEAT_OF_DEATH_POWER.name");
        public override LocString Description => new LocString("powers", "BEAT_OF_DEATH_POWER.description");

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var player = cardPlay.Card?.Owner;
            if (player?.Creature != null && Owner != null && Owner.IsAlive)
            {
                Flash();
                await CreatureCmd.Damage(choiceContext, player.Creature, Amount, ValueProp.Unblockable | ValueProp.Unpowered, Owner);
            }
        }
    }

    // ==========================================
    // Spire Invincible Power / Несокрушимость Шпиля
    // ==========================================
    public class SpireInvinciblePower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override LocString Title => new LocString("powers", "SPIRE_INVINCIBLE_POWER.name");
        public override LocString Description => new LocString("powers", "SPIRE_INVINCIBLE_POWER.description");

        private decimal _damageTakenThisTurn = 0m;
        public decimal MaxDamagePerTurn => 300m;

        public override Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (side == CombatSide.Player)
            {
                _damageTakenThisTurn = 0m;
                SetAmount((int)MaxDamagePerTurn);
            }
            return Task.CompletedTask;
        }

        public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
        {
            if (target == Owner && amount > 0)
            {
                decimal remaining = Math.Max(0m, MaxDamagePerTurn - _damageTakenThisTurn);
                if (amount > remaining)
                {
                    return -(amount - remaining);
                }
            }
            return 0m;
        }

        public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
        {
            if (target == Owner && result.TotalDamage > 0)
            {
                _damageTakenThisTurn += result.TotalDamage;
                SetAmount((int)Math.Max(0m, MaxDamagePerTurn - _damageTakenThisTurn));
            }
            return Task.CompletedTask;
        }
    }

    // ==========================================
    // The Corrupted Heart / Осквернённое Сердце Шпиля (Act 4 Pinnacle Boss)
    // ==========================================
    public class SpireHeart : MonsterModel
    {
        public override int MinInitialHp => 750;
        public override int MaxInitialHp => 800;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");
        public override LocString Title => new LocString("monsters", "SPIRE_HEART.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        public override async Task AfterAddedToRoom()
        {
            await base.AfterAddedToRoom();
            await PowerCmd.Apply<BeatOfDeathPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
            await PowerCmd.Apply<SpireInvinciblePower>(new ThrowingPlayerChoiceContext(), Creature, 300m, Creature, null);
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateDebilitate = new MoveState("HEART_DEBILITATE", MoveDebilitate, new SingleAttackIntent(18), new DebuffIntent());
            var stateBloodShots = new MoveState("HEART_BLOOD_SHOTS", MoveBloodShots, new MultiAttackIntent(2, 15));
            var stateEchoSlam = new MoveState("HEART_ECHO_SLAM", MoveEchoSlam, new SingleAttackIntent(45), new DebuffIntent());
            var stateCleansing = new MoveState("HEART_CLEANSING", MoveCleansing, new DefendIntent(), new BuffIntent());

            stateDebilitate.FollowUpState = stateBloodShots;
            stateBloodShots.FollowUpState = stateEchoSlam;
            stateEchoSlam.FollowUpState = stateCleansing;
            stateCleansing.FollowUpState = stateBloodShots;

            return new MonsterMoveStateMachine(new MonsterState[] { stateDebilitate, stateBloodShots, stateEchoSlam, stateCleansing }, stateDebilitate);
        }

        private async Task MoveDebilitate(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -70f, 0.16f);
            await DamageCmd.Attack(18).FromMonster(this)
                .WithAttackerAnim("Attack", 0.16f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_big_slash")
                .Execute(null);

            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null)
            {
                await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), player.Creature, 2m, Creature, null);
                await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), player.Creature, 2m, Creature, null);
                await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), player.Creature, 2m, Creature, null);
            }
            await Cmd.Wait(0.2f);
        }

        private async Task MoveBloodShots(IReadOnlyList<Creature> targets)
        {
            for (int i = 0; i < 15; i++)
            {
                await ModVfx.AttackLunge(Creature, -50f, 0.06f);
                await DamageCmd.Attack(2).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.05f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(null);
                await Cmd.Wait(0.04f);
            }
        }

        private async Task MoveEchoSlam(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -90f, 0.22f);
            await DamageCmd.Attack(45).FromMonster(this)
                .WithAttackerAnim("Attack", 0.22f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_big_slash")
                .Execute(null);

            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null)
            {
                await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), player.Creature, 1m, Creature, null);
            }
            await Cmd.Wait(0.2f);
        }

        private async Task MoveCleansing(IReadOnlyList<Creature> targets)
        {
            await CreatureCmd.GainBlock(Creature, 45, ValueProp.Move, null);
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 2m, Creature, null);

            var beat = Creature.GetPower<BeatOfDeathPower>();
            if (beat != null)
            {
                beat.SetAmount(beat.Amount + 1);
            }
            await Cmd.Wait(0.2f);
        }

        public override CreatureAnimator GenerateAnimator(MegaSprite controller)
        {
            if (controller == null) return null!;
            try
            {
                return ModelDb.Monster<MegaCrit.Sts2.Core.Models.Monsters.TheForgotten>().GenerateAnimator(controller);
            }
            catch
            {
                AnimState animState = new AnimState("idle_loop", isLooping: true);
                AnimState state = new AnimState("die");
                CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
                creatureAnimator.AddAnyState("Idle", animState);
                creatureAnimator.AddAnyState("Dead", state);
                return creatureAnimator;
            }
        }
    }

    // ==========================================
    // Act 4 Heart Boss Encounter
    // ==========================================
    public class SpireHeartBoss : EncounterModel
    {
        public override RoomType RoomType => RoomType.Boss;
        public override string BossNodePath => "res://images/map/placeholder/beast_boss_icon";
        public override MegaSkeletonDataResource? BossNodeSpineResource => null;
        public override IEnumerable<MonsterModel> AllPossibleMonsters => new[] { ModelDb.Monster<SpireHeart>() };
        public override float GetCameraScaling() => 0.75f;
        public override Vector2 GetCameraOffset() => Vector2.Down * 60f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<SpireHeart>().ToMutable(), null)
            };
        }
    }
}
