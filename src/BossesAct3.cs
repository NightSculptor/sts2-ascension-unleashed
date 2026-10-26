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
    // 7. CHRONOS TITAN (ACT 3 BOSS)
    // ==========================================

    public class ChronosLoopPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override LocString Title => new LocString("powers", "CHRONOS_LOOP_POWER.name");
        public override LocString Description => new LocString("powers", "CHRONOS_LOOP_POWER.description");

        public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            SetAmount(6);
            return base.AfterPlayerTurnStart(choiceContext, player);
        }

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner == null || !Owner.IsAlive) return;

            SetAmount(Amount - 1);
            if (Amount <= 0)
            {
                SetAmount(6);
                Flash();
                
                // Titan gains 20 Block and +2 Strength
                await CreatureCmd.GainBlock(Owner, 20, ValueProp.Move, null);
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, 2m, Owner, null);

                // Force player turn to end!
                if (cardPlay.Card.Owner != null)
                {
                    try
                    {
                        PlayerCmd.EndTurn(cardPlay.Card.Owner, false, null);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"[AnimeSurvivalMod] Time Warp end turn exception: {ex.Message}");
                    }
                }
            }
        }
    }

    public class ChronosTitan : MonsterModel
    {
        public override int MinInitialHp => 450;
        public override int MaxInitialHp => 500;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/waterfall_giant");
        public override LocString Title => new LocString("monsters", "CHRONOS_TITAN.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        public override async Task AfterAddedToRoom()
        {
            await base.AfterAddedToRoom();
            await PowerCmd.Apply<ChronosLoopPower>(new ThrowingPlayerChoiceContext(), Creature, 6m, Creature, null);
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateStasis = new MoveState("TITAN_STASIS", MoveStasis, new DefendIntent(), new StatusIntent(2));
            var stateTimeRift = new MoveState("TITAN_RIFT", MoveTimeRift, new MultiAttackIntent(11, 3));
            var stateParadox = new MoveState("TITAN_PARADOX", MoveParadox, new SingleAttackIntent(34));
            var stateReversal = new MoveState("TITAN_REVERSAL", MoveReversal, new BuffIntent());

            stateStasis.FollowUpState = stateTimeRift;
            stateTimeRift.FollowUpState = stateParadox;
            stateParadox.FollowUpState = stateReversal;
            stateReversal.FollowUpState = stateStasis;

            return new MonsterMoveStateMachine(new MonsterState[] { stateStasis, stateTimeRift, stateParadox, stateReversal }, stateStasis);
        }

        private async Task MoveStasis(IReadOnlyList<Creature> targets)
        {
            await CreatureCmd.GainBlock(Creature, 35, ValueProp.Move, null);
            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null && CombatState != null)
            {
                var dazed = new List<CardModel>
                {
                    CombatState.CreateCard<MegaCrit.Sts2.Core.Models.Cards.Dazed>(player),
                    CombatState.CreateCard<MegaCrit.Sts2.Core.Models.Cards.Dazed>(player)
                };
                await CardPileCmd.AddGeneratedCardsToCombat(dazed, PileType.Discard, player);
            }
            await Cmd.Wait(0.2f);
        }

        private async Task MoveTimeRift(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -60f, 0.14f);
            for (int i = 0; i < 3; i++)
            {
                await DamageCmd.Attack(11).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.12f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_attack_blunt")
                    .Execute(null);
                await Cmd.Wait(0.1f);
            }
        }

        private async Task MoveParadox(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -80f, 0.16f);
            await DamageCmd.Attack(34).FromMonster(this)
                .WithAttackerAnim("Attack", 0.20f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.2f);
        }

        private async Task MoveReversal(IReadOnlyList<Creature> targets)
        {
            // Cleanses all debuffs from Chronos
            var debuffs = Creature.Powers.Where(p => p.Type == PowerType.Debuff).ToList();
            foreach (var d in debuffs)
            {
                await PowerCmd.Remove(d);
            }

            // Heals 40 HP
            await CreatureCmd.Heal(Creature, 40);
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 2m, Creature, null);

            // Reverses player hands (discard hand and redraw 5 cards)
            if (CombatState != null)
            {
                foreach (var p in CombatState.Players)
                {
                    try
                    {
                        var handCards = p.PlayerCombatState.Hand.Cards.ToList();
                        if (handCards.Count > 0)
                        {
                            await CardPileCmd.Add(handCards, PileType.Discard);
                        }
                        await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), 5m, p);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"[AnimeSurvivalMod] Time Reversal discard error: {ex.Message}");
                    }
                }
            }

            await Cmd.Wait(0.25f);
        }

        public override CreatureAnimator GenerateAnimator(MegaSprite controller)
        {
            if (controller == null) return null!;
            try
            {
                return ModelDb.Monster<MegaCrit.Sts2.Core.Models.Monsters.WaterfallGiant>().GenerateAnimator(controller);
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

    public class ChronosTitanBoss : EncounterModel
    {
        public override RoomType RoomType => RoomType.Boss;
        public override string BossNodePath => "res://images/map/placeholder/test_subject_boss_icon";
        public override MegaSkeletonDataResource? BossNodeSpineResource => null;
        public override IEnumerable<MonsterModel> AllPossibleMonsters => new[] { ModelDb.Monster<ChronosTitan>() };
        public override float GetCameraScaling() => 0.80f;
        public override Vector2 GetCameraOffset() => Vector2.Down * 60f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<ChronosTitan>().ToMutable(), null)
            };
        }
    }

    // ==========================================
    // 8. ASTRAL DEVOURER (ACT 3 BOSS) & ASTRAL STAR
    // ==========================================

    public class AstralStar : MonsterModel
    {
        public override int MinInitialHp => 18;
        public override int MaxInitialHp => 22;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");
        public override LocString Title => new LocString("monsters", "ASTRAL_STAR.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            if (creature == Creature && CombatState != null)
            {
                // When killed by players, grants blessing!
                foreach (var p in CombatState.Players)
                {
                    await PowerCmd.Apply<StrengthPower>(choiceContext, p.Creature, 2m, p.Creature, null);
                    await CardPileCmd.Draw(choiceContext, 1m, p);
                }
            }
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var statePulse = new MoveState("STAR_PULSE", MovePulse, new MultiAttackIntent(3, 2));
            var stateLight = new MoveState("STAR_LIGHT", MoveLight, new DefendIntent());

            statePulse.FollowUpState = stateLight;
            stateLight.FollowUpState = statePulse;

            return new MonsterMoveStateMachine(new MonsterState[] { statePulse, stateLight }, statePulse);
        }

        private async Task MovePulse(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -30f, 0.10f);
            for (int i = 0; i < 2; i++)
            {
                await DamageCmd.Attack(3).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.10f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_attack_blunt")
                    .Execute(null);
                await Cmd.Wait(0.08f);
            }
        }

        private async Task MoveLight(IReadOnlyList<Creature> targets)
        {
            var devourer = CombatState?.Enemies.FirstOrDefault(e => e.IsAlive && e.Monster is AstralDevourer);
            if (devourer != null)
            {
                await CreatureCmd.GainBlock(devourer, 14, ValueProp.Move, null);
            }
            else
            {
                await CreatureCmd.GainBlock(Creature, 10, ValueProp.Move, null);
            }
            await Cmd.Wait(0.15f);
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

    public class CosmicHungerPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override LocString Title => new LocString("powers", "COSMIC_HUNGER_POWER.name");
        public override LocString Description => new LocString("powers", "COSMIC_HUNGER_POWER.description");

        public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side == CombatSide.Enemy && Owner != null && Owner.IsAlive)
            {
                SetAmount(Amount + 1);
                Flash();
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, 1m, Owner, null);
            }
        }
    }

    public class AstralDevourer : MonsterModel
    {
        public override int MinInitialHp => 470;
        public override int MaxInitialHp => 520;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_insatiable");
        public override LocString Title => new LocString("monsters", "ASTRAL_DEVOURER.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        public override async Task AfterAddedToRoom()
        {
            await base.AfterAddedToRoom();
            await PowerCmd.Apply<CosmicHungerPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateCharge = new MoveState("ASTRAL_CHARGE", MoveCharge, new DefendIntent(), new DebuffIntent());
            var stateDevourOrRay = new MoveState("ASTRAL_DEVOUR", MoveDevourOrRay, new SingleAttackIntent(28), new BuffIntent());
            var stateCollapse = new MoveState("ASTRAL_COLLAPSE", MoveCollapse, new MultiAttackIntent(8, 4));
            var stateHorizon = new MoveState("ASTRAL_HORIZON", MoveEventHorizon, new SingleAttackIntent(40));

            stateCharge.FollowUpState = stateDevourOrRay;
            stateDevourOrRay.FollowUpState = stateCollapse;
            stateCollapse.FollowUpState = stateHorizon;
            stateHorizon.FollowUpState = stateCharge;

            return new MonsterMoveStateMachine(new MonsterState[] { stateCharge, stateDevourOrRay, stateCollapse, stateHorizon }, stateCharge);
        }

        private async Task MoveCharge(IReadOnlyList<Creature> targets)
        {
            await CreatureCmd.GainBlock(Creature, 40, ValueProp.Move, null);
            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null)
            {
                await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), player.Creature, 2m, Creature, null);
            }
            await Cmd.Wait(0.2f);
        }

        private async Task MoveDevourOrRay(IReadOnlyList<Creature> targets)
        {
            var star = CombatState?.Enemies.FirstOrDefault(e => e.IsAlive && e.Monster is AstralStar);
            if (star != null)
            {
                // Devour Astral Star!
                await CreatureCmd.Kill(star);
                await CreatureCmd.Heal(Creature, 40);
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 4m, Creature, null);
                await Cmd.Wait(0.25f);
            }
            else
            {
                // Cosmic Ray attack
                await ModVfx.AttackLunge(Creature, -70f, 0.16f);
                await DamageCmd.Attack(28).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.18f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_attack_blunt")
                    .Execute(null);
                await Cmd.Wait(0.2f);
            }
        }

        private async Task MoveCollapse(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -50f, 0.12f);
            for (int i = 0; i < 4; i++)
            {
                await DamageCmd.Attack(8).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.12f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_attack_blunt")
                    .Execute(null);
                await Cmd.Wait(0.1f);
            }
        }

        private async Task MoveEventHorizon(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -100f, 0.20f);
            await DamageCmd.Attack(40).FromMonster(this)
                .WithAttackerAnim("Attack", 0.25f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.25f);
        }

        public override CreatureAnimator GenerateAnimator(MegaSprite controller)
        {
            if (controller == null) return null!;
            try
            {
                return ModelDb.Monster<MegaCrit.Sts2.Core.Models.Monsters.TheInsatiable>().GenerateAnimator(controller);
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

    public class AstralDevourerBoss : EncounterModel
    {
        public override RoomType RoomType => RoomType.Boss;
        public override string BossNodePath => "res://images/map/placeholder/aeonglass_boss_icon";
        public override MegaSkeletonDataResource? BossNodeSpineResource => null;
        public override IEnumerable<MonsterModel> AllPossibleMonsters => new MonsterModel[]
        {
            ModelDb.Monster<AstralDevourer>(),
            ModelDb.Monster<AstralStar>()
        };
        public override float GetCameraScaling() => 0.80f;
        public override Vector2 GetCameraOffset() => Vector2.Down * 60f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<AstralDevourer>().ToMutable(), null),
                (ModelDb.Monster<AstralStar>().ToMutable(), null),
                (ModelDb.Monster<AstralStar>().ToMutable(), null)
            };
        }
    }

    // ==========================================
    // 9. SPIRE ARCHITECT (ACT 3 BOSS)
    // ==========================================

    public class ArchitectGeometryPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override LocString Title => new LocString("powers", "ARCHITECT_GEOMETRY_POWER.name");
        public override LocString Description => new LocString("powers", "ARCHITECT_GEOMETRY_POWER.description");

        private bool _ruinEntered = false;

        public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target != Owner) return;
            if (Owner == null || !Owner.IsAlive) return;

            float hpPct = (float)Owner.CurrentHp / Owner.MaxHp;

            // Phase 1 (>66% HP): Inverted Architecture - responds to attacks with 4 Block
            if (hpPct > 0.66f)
            {
                SetAmount(1);
                if (props.IsPoweredAttack() && result.TotalDamage > 0)
                {
                    Flash();
                    await CreatureCmd.GainBlock(Owner, 4, ValueProp.Move, null);
                }
            }
            else if (hpPct > 0.33f)
            {
                SetAmount(2);
            }
            else
            {
                SetAmount(3);
                // Phase 3 (<=33% HP): Total Ruin entrance
                if (!_ruinEntered)
                {
                    _ruinEntered = true;
                    Flash();
                    await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, 8m, Owner, null);
                }
            }
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (Owner == null || !Owner.IsAlive) return;
            float hpPct = (float)Owner.CurrentHp / Owner.MaxHp;

            // Phase 2 (33%-66% HP): Spatial Distortion - disorients players on turn start
            if (hpPct > 0.33f && hpPct <= 0.66f)
            {
                SetAmount(2);
                Flash();
                await PowerCmd.Apply<FrailPower>(choiceContext, player.Creature, 1m, Owner, null);
            }
        }
    }

    public class SpireArchitect : MonsterModel
    {
        public override int MinInitialHp => 520;
        public override int MaxInitialHp => 580;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/architect");
        public override LocString Title => new LocString("monsters", "SPIRE_ARCHITECT.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        public override async Task AfterAddedToRoom()
        {
            await base.AfterAddedToRoom();
            await PowerCmd.Apply<ArchitectGeometryPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateFoundation = new MoveState("ARCH_FOUNDATION", MoveFoundation, new SingleAttackIntent(24), new DefendIntent());
            var stateBeam = new MoveState("ARCH_BEAM", MoveBeam, new MultiAttackIntent(12, 3));
            var statePillar = new MoveState("ARCH_PILLAR", MovePillar, new DefendIntent(), new BuffIntent());
            var stateCollapse = new MoveState("ARCH_COLLAPSE", MoveCollapse, new SingleAttackIntent(48));

            stateFoundation.FollowUpState = stateBeam;
            stateBeam.FollowUpState = statePillar;
            statePillar.FollowUpState = stateCollapse;
            stateCollapse.FollowUpState = stateFoundation;

            return new MonsterMoveStateMachine(new MonsterState[] { stateFoundation, stateBeam, statePillar, stateCollapse }, stateFoundation);
        }

        private async Task MoveFoundation(IReadOnlyList<Creature> targets)
        {
            await CreatureCmd.GainBlock(Creature, 25, ValueProp.Move, null);
            await ModVfx.AttackLunge(Creature, -70f, 0.16f);
            await DamageCmd.Attack(24).FromMonster(this)
                .WithAttackerAnim("Attack", 0.18f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.2f);
        }

        private async Task MoveBeam(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -50f, 0.12f);
            for (int i = 0; i < 3; i++)
            {
                await DamageCmd.Attack(12).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.12f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_attack_blunt")
                    .Execute(null);
                await Cmd.Wait(0.1f);
            }
        }

        private async Task MovePillar(IReadOnlyList<Creature> targets)
        {
            await CreatureCmd.GainBlock(Creature, 40, ValueProp.Move, null);
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 3m, Creature, null);
            await Cmd.Wait(0.2f);
        }

        private async Task MoveCollapse(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -100f, 0.22f);
            await DamageCmd.Attack(48).FromMonster(this)
                .WithAttackerAnim("Attack", 0.25f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.25f);
        }

        public override CreatureAnimator GenerateAnimator(MegaSprite controller)
        {
            if (controller == null) return null!;
            try
            {
                return ModelDb.Monster<MegaCrit.Sts2.Core.Models.Monsters.Architect>().GenerateAnimator(controller);
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

    public class SpireArchitectBoss : EncounterModel
    {
        public override RoomType RoomType => RoomType.Boss;
        public override string BossNodePath => "res://images/map/placeholder/lagavulin_matriarch_boss_icon";
        public override MegaSkeletonDataResource? BossNodeSpineResource => null;
        public override IEnumerable<MonsterModel> AllPossibleMonsters => new[] { ModelDb.Monster<SpireArchitect>() };
        public override float GetCameraScaling() => 0.80f;
        public override Vector2 GetCameraOffset() => Vector2.Down * 60f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<SpireArchitect>().ToMutable(), null)
            };
        }
    }
}
