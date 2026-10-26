#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.DevConsole;
using MegaCrit.Sts2.Core.DevConsole.ConsoleCommands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Intents;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Audio;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Unlocks;

using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Models.RelicPools;

namespace AnimeSurvivalMod
{
    [ModInitializer(nameof(Init))]
    public class ModEntry
    {
        public static void Init()
        {
            try
            {
                Log.Info("[AnimeSurvivalMod] Initializing AnimeSurvivalMod...");
                Log.Info("[AnimeSurvivalMod] HasArg('autoslay') = " + CommandLineHelper.HasArg("autoslay"));
                Log.Info("[AnimeSurvivalMod] Raw OS.GetCmdlineArgs = " + string.Join(" | ", OS.GetCmdlineArgs()));
                var harmony = new Harmony("com.white.animesurvival");
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                Log.Info("[AnimeSurvivalMod] ChronoWarden and Encounter patches successfully applied!");

                // Register 27 custom cards to character card pools
                // 1. Ironclad
                ModHelper.AddModelToPool<IroncladCardPool, BloodRetribution>();
                ModHelper.AddModelToPool<IroncladCardPool, DeepConcentration>();
                ModHelper.AddModelToPool<IroncladCardPool, Egoist>();
                ModHelper.AddModelToPool<IroncladCardPool, SacrificialCleave>();
                ModHelper.AddModelToPool<IroncladCardPool, TitanicBastion>();
                ModHelper.AddModelToPool<IroncladCardPool, IronLoan>();
                ModHelper.AddModelToPool<IroncladCardPool, MartyrsBrand>();
                ModHelper.AddModelToPool<IroncladCardPool, AshenRequiem>();
                ModHelper.AddModelToPool<IroncladCardPool, SiegeStance>();
                ModHelper.AddModelToPool<IroncladCardPool, BloodParity>();
                ModHelper.AddModelToPool<IroncladCardPool, RedHotAnvil>();
                ModHelper.AddModelToPool<IroncladCardPool, DoomToll>();
                ModHelper.AddModelToPool<IroncladCardPool, SecondHeart>();
                ModHelper.AddModelToPool<IroncladCardPool, UnstoppableRampage>();
                ModHelper.AddModelToPool<IroncladCardPool, CombustionPact>();

                // 2. Silent
                ModHelper.AddModelToPool<SilentCardPool, DivertAndScatter>();
                ModHelper.AddModelToPool<SilentCardPool, DanceOfPoisonBlades>();
                ModHelper.AddModelToPool<SilentCardPool, ToxicRicochet>();
                ModHelper.AddModelToPool<SilentCardPool, ShadowVeil>();
                ModHelper.AddModelToPool<SilentCardPool, EnvenomedArsenal>();
                ModHelper.AddModelToPool<SilentCardPool, PhantomRicochet>();
                ModHelper.AddModelToPool<SilentCardPool, WellPoisoner>();
                ModHelper.AddModelToPool<SilentCardPool, ShadowDouble>();
                ModHelper.AddModelToPool<SilentCardPool, SleeveArsenal>();
                ModHelper.AddModelToPool<SilentCardPool, CatalyticOutbreak>();
                ModHelper.AddModelToPool<SilentCardPool, DeadlyGambit>();
                ModHelper.AddModelToPool<SilentCardPool, SmokescreenFlurry>();
                ModHelper.AddModelToPool<SilentCardPool, GlassStiletto>();
                ModHelper.AddModelToPool<SilentCardPool, NeurotoxinFlash>();

                // Tokens (Shivs, Boulders, etc.)
                ModHelper.AddModelToPool<TokenCardPool, PoisonShiv>();
                ModHelper.AddModelToPool<TokenCardPool, SporeCard>();
                ModHelper.AddModelToPool<TokenCardPool, GlassShard>();
                ModHelper.AddModelToPool<TokenCardPool, CharonsDebt>();
                ModHelper.AddModelToPool<TokenCardPool, CosmicStardust>();

                // 3. Necrobinder
                ModHelper.AddModelToPool<NecrobinderCardPool, SkeletonArmy>();
                ModHelper.AddModelToPool<NecrobinderCardPool, SoulHarvest>();
                ModHelper.AddModelToPool<NecrobinderCardPool, DoomShroud>();
                ModHelper.AddModelToPool<NecrobinderCardPool, BonePurge>();
                ModHelper.AddModelToPool<NecrobinderCardPool, ShiftBurden>();
                ModHelper.AddModelToPool<NecrobinderCardPool, HexReflection>();
                ModHelper.AddModelToPool<NecrobinderCardPool, SoulTransfusion>();
                ModHelper.AddModelToPool<NecrobinderCardPool, NecroticTether>();
                ModHelper.AddModelToPool<NecrobinderCardPool, HarvestOfMalice>();
                ModHelper.AddModelToPool<NecrobinderCardPool, OssuaryFusion>();
                ModHelper.AddModelToPool<NecrobinderCardPool, CharonsObol>();
                ModHelper.AddModelToPool<NecrobinderCardPool, PyreOfTheDamned>();
                ModHelper.AddModelToPool<NecrobinderCardPool, DeathEcho>();
                ModHelper.AddModelToPool<NecrobinderCardPool, ShroudTear>();
                ModHelper.AddModelToPool<NecrobinderCardPool, EssenceSiphon>();
                ModHelper.AddModelToPool<NecrobinderCardPool, WraithStep>();
                ModHelper.AddModelToPool<NecrobinderCardPool, SigilOfDecay>();

                // 4. Defect
                ModHelper.AddModelToPool<DefectCardPool, OverclockFocus>();
                ModHelper.AddModelToPool<DefectCardPool, PlasmaBeam>();
                ModHelper.AddModelToPool<DefectCardPool, ShortCircuit>();
                ModHelper.AddModelToPool<DefectCardPool, NuclearReactor>();
                ModHelper.AddModelToPool<DefectCardPool, MatrixCapacitor>();
                ModHelper.AddModelToPool<DefectCardPool, CoreMeltdown>();
                ModHelper.AddModelToPool<DefectCardPool, Superconductor>();
                ModHelper.AddModelToPool<DefectCardPool, QuantumEntanglement>();
                ModHelper.AddModelToPool<DefectCardPool, PlasmaOverdrive>();
                ModHelper.AddModelToPool<DefectCardPool, RecursiveCompile>();
                ModHelper.AddModelToPool<DefectCardPool, AbsoluteZero>();
                ModHelper.AddModelToPool<DefectCardPool, BiosPatch>();
                ModHelper.AddModelToPool<DefectCardPool, ImpulseCapacitor>();
                ModHelper.AddModelToPool<DefectCardPool, MemoryLeak>();

                // 5. Regent
                ModHelper.AddModelToPool<RegentCardPool, BladeOrbitResonance>();
                ModHelper.AddModelToPool<RegentCardPool, GravitationalSingularity>();
                ModHelper.AddModelToPool<RegentCardPool, CrownTribute>();
                ModHelper.AddModelToPool<RegentCardPool, AstralEclipse>();
                ModHelper.AddModelToPool<RegentCardPool, SupernovaCoronation>();
                ModHelper.AddModelToPool<RegentCardPool, RoyalEdictExecution>();
                ModHelper.AddModelToPool<RegentCardPool, GravitationalCollapse>();
                ModHelper.AddModelToPool<RegentCardPool, AegisOfTheCrown>();
                ModHelper.AddModelToPool<RegentCardPool, BladeSuperOrbit>();
                ModHelper.AddModelToPool<RegentCardPool, WeightOfKings>();
                ModHelper.AddModelToPool<RegentCardPool, WormholeSlash>();
                ModHelper.AddModelToPool<RegentCardPool, AntimatterPulse>();
                ModHelper.AddModelToPool<RegentCardPool, VoidAscension>();
                ModHelper.AddModelToPool<RegentCardPool, MeteorBarrage>();

                // Register 12 custom relics to relic pools
                // Shared Relics
                ModHelper.AddModelToPool<SharedRelicPool, CrimsonPact>();
                ModHelper.AddModelToPool<SharedRelicPool, SoulLantern>();
                ModHelper.AddModelToPool<SharedRelicPool, ObsidianAnvil>();
                ModHelper.AddModelToPool<SharedRelicPool, ClockworkGear>();
                ModHelper.AddModelToPool<SharedRelicPool, CursedAmulet>();
                ModHelper.AddModelToPool<SharedRelicPool, SpireCompass>();
                ModHelper.AddModelToPool<SharedRelicPool, HeartOfInfinity>();

                // Character-Specific Relics
                ModHelper.AddModelToPool<IroncladRelicPool, WarDrum>();
                ModHelper.AddModelToPool<SilentRelicPool, ViperFang>();
                ModHelper.AddModelToPool<DefectRelicPool, CapacitorCore>();
                ModHelper.AddModelToPool<NecrobinderRelicPool, GraveyardSoil>();
                ModHelper.AddModelToPool<RegentRelicPool, CelestialPrism>();

                Log.Info("[AnimeSurvivalMod] Successfully registered all 12 custom relics!");

                Log.Info("[AnimeSurvivalMod] Successfully registered all 27 custom cards across all 5 characters!");
            }
            catch (Exception ex)
            {
                Log.Error($"[AnimeSurvivalMod] Error during init: {ex}");
            }
        }
    }

    public static class ModVfx
    {
        public static async Task AttackLunge(Creature? attacker, float distance = -70f, float duration = 0.14f)
        {
            if (attacker == null) return;
            try
            {
                var node = NCombatRoom.Instance?.GetCreatureNode(attacker);
                var visuals = node?.Visuals;
                var sprite = visuals?.GetNodeOrNull<Sprite2D>("%Visuals") ?? visuals?.GetNodeOrNull<Sprite2D>("Visuals");
                if (sprite != null)
                {
                    Vector2 orig = sprite.Position;
                    var tween = sprite.CreateTween();
                    tween.TweenProperty(sprite, "position:x", orig.X + distance, duration)
                         .SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.In);
                    tween.TweenProperty(sprite, "position:x", orig.X, duration * 1.4)
                         .SetTrans(Tween.TransitionType.Sine).SetEase(Tween.EaseType.Out);
                    await Cmd.Wait(duration);
                }
            }
            catch {}
        }
    }

    public class ChronoWarden : MonsterModel
    {
        public override int MinInitialHp => 9999;
        public override int MaxInitialHp => 9999;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");

        public override LocString Title => new LocString("monsters", "CHRONO_WARDEN.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        public override async Task AfterAddedToRoom()
        {
            await base.AfterAddedToRoom();
            await PowerCmd.Apply<EscapeArtistPower>(new ThrowingPlayerChoiceContext(), Creature, 5m, Creature, null);
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var state1 = new MoveState("CHRONO_1", Move1, new SingleAttackIntent(4));
            var state2 = new MoveState("CHRONO_2", Move2, new SingleAttackIntent(6), new DefendIntent());
            var state3 = new MoveState("CHRONO_3", Move3, new SingleAttackIntent(8));
            var state4 = new MoveState("CHRONO_4", Move4, new SingleAttackIntent(10), new DefendIntent());
            var stateFinal = new MoveState("CHRONO_ESCAPE", MoveEscape, new EscapeIntent());

            state1.FollowUpState = state2;
            state2.FollowUpState = state3;
            state3.FollowUpState = state4;
            state4.FollowUpState = stateFinal;
            stateFinal.FollowUpState = stateFinal;

            return new MonsterMoveStateMachine(new MonsterState[] { state1, state2, state3, state4, stateFinal }, state1);
        }

        private async Task Move1(IReadOnlyList<Creature> targets)
        {
            await DamageCmd.Attack(4).FromMonster(this)
                .WithAttackerAnim("Attack", 0.15f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.25f);
        }

        private async Task Move2(IReadOnlyList<Creature> targets)
        {
            await CreatureCmd.GainBlock(this.Creature, 8, ValueProp.Move, null);
            await DamageCmd.Attack(6).FromMonster(this)
                .WithAttackerAnim("Attack", 0.15f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.25f);
        }

        private async Task Move3(IReadOnlyList<Creature> targets)
        {
            await DamageCmd.Attack(8).FromMonster(this)
                .WithAttackerAnim("Attack", 0.15f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.25f);
        }

        private async Task Move4(IReadOnlyList<Creature> targets)
        {
            await CreatureCmd.GainBlock(this.Creature, 12, ValueProp.Move, null);
            await DamageCmd.Attack(10).FromMonster(this)
                .WithAttackerAnim("Attack", 0.15f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.25f);
        }

        private async Task MoveEscape(IReadOnlyList<Creature> targets)
        {
            NCombatRoom.Instance?.GetCreatureNode(Creature)?.ToggleIsInteractable(false);
            SfxCmd.Play(CastSfx);
            await CreatureCmd.TriggerAnim(Creature, "Cast", 0.5f);
            await Cmd.Wait(0.6f);
            await CreatureCmd.Escape(Creature);
        }

        public override CreatureAnimator GenerateAnimator(MegaSprite controller)
        {
            if (controller == null) return null!;
            AnimState animState = new AnimState("idle_loop", isLooping: true);
            AnimState animState2 = new AnimState("debuff");
            AnimState animState3 = new AnimState("attack");
            AnimState animState4 = new AnimState("hurt");
            AnimState state = new AnimState("die");
            animState2.NextState = animState;
            animState3.NextState = animState;
            animState4.NextState = animState;
            CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
            creatureAnimator.AddAnyState("Idle", animState);
            creatureAnimator.AddAnyState("Cast", animState2);
            creatureAnimator.AddAnyState("Attack", animState3);
            creatureAnimator.AddAnyState("Dead", state);
            creatureAnimator.AddAnyState("Hit", animState4);
            return creatureAnimator;
        }
    }

    public class ChronoWardenEncounter : EncounterModel
    {
        public override RoomType RoomType => RoomType.Monster;

        public override IEnumerable<MonsterModel> AllPossibleMonsters => new[] { ModelDb.Monster<ChronoWarden>() };

        public override float GetCameraScaling() => 0.88f;

        public override Vector2 GetCameraOffset() => Vector2.Down * 50f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<ChronoWarden>().ToMutable(), null)
            };
        }
    }

    public class MirrorStancePower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override LocString Title => new LocString("powers", "MIRROR_STANCE_POWER.name");
        public override LocString Description => new LocString("powers", "MIRROR_STANCE_POWER.description");

        public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target != Owner) return;
            if (dealer == null || dealer == Owner) return;
            if (!props.IsPoweredAttack()) return;
            if (result.TotalDamage <= 0) return;

            decimal reflectAmount = Math.Max(3m, Math.Floor((decimal)result.TotalDamage * 0.30m));

            Flash();
            await CreatureCmd.Damage(choiceContext, dealer, reflectAmount, ValueProp.Move, Owner);
            await PowerCmd.Remove(this);
        }
    }

    public class MirrorPhantom : MonsterModel
    {
        public override int MinInitialHp => 68;
        public override int MaxInitialHp => 76;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");

        public override LocString Title => new LocString("monsters", "MIRROR_PHANTOM.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateStance = new MoveState("MIRROR_STANCE", MoveStance, new DefendIntent(), new BuffIntent());
            var stateShards = new MoveState("MIRROR_SHARDS", MoveShards, new MultiAttackIntent(3, 3));
            var stateBeam = new MoveState("MIRROR_BEAM", MoveBeam, new SingleAttackIntent(14));

            stateStance.FollowUpState = stateShards;
            stateShards.FollowUpState = stateBeam;
            stateBeam.FollowUpState = stateStance;

            return new MonsterMoveStateMachine(new MonsterState[] { stateStance, stateShards, stateBeam }, stateStance);
        }

        private async Task MoveStance(IReadOnlyList<Creature> targets)
        {
            await CreatureCmd.GainBlock(Creature, 12, ValueProp.Move, null);
            await PowerCmd.Apply<MirrorStancePower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
            await Cmd.Wait(0.25f);
        }

        private async Task MoveShards(IReadOnlyList<Creature> targets)
        {
            for (int i = 0; i < 3; i++)
            {
                await DamageCmd.Attack(3).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.1f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(null);
                await Cmd.Wait(0.12f);
            }
        }

        private async Task MoveBeam(IReadOnlyList<Creature> targets)
        {
            await DamageCmd.Attack(14).FromMonster(this)
                .WithAttackerAnim("Attack", 0.2f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, CastSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.25f);
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

    public class MirrorPhantomEncounter : EncounterModel
    {
        public override RoomType RoomType => RoomType.Monster;

        public override IEnumerable<MonsterModel> AllPossibleMonsters => new[] { ModelDb.Monster<MirrorPhantom>() };

        public override float GetCameraScaling() => 0.88f;

        public override Vector2 GetCameraOffset() => Vector2.Down * 40f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<MirrorPhantom>().ToMutable(), null)
            };
        }
    }

    // ==========================================
    // 3. Chrono Alchemist / Песочный Хрономант
    // ==========================================
    public class TimeDistortionPower : PowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override LocString Title => new LocString("powers", "TIME_DISTORTION_POWER.name");
        public override LocString Description => new LocString("powers", "TIME_DISTORTION_POWER.description");

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Player == Owner.Player)
            {
                await PowerCmd.Decrement(this);
            }
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == Owner.Player)
            {
                if (Amount < 5)
                {
                    await PowerCmd.ModifyAmount(choiceContext, this, 5m - Amount, Owner, null, false);
                }
            }
        }

        public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
        {
            if (Amount <= 0)
            {
                return false;
            }
            return base.ShouldPlay(card, autoPlayType);
        }
    }

    public class ChronoAlchemist : MonsterModel
    {
        public override int MinInitialHp => 68;
        public override int MaxInitialHp => 76;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");

        public override LocString Title => new LocString("monsters", "CHRONO_ALCHEMIST.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateTrap = new MoveState("ALCHEMIST_TRAP", MoveTrap, new SingleAttackIntent(7), new DebuffIntent());
            var stateShift = new MoveState("ALCHEMIST_SHIFT", MoveShift, new DefendIntent(), new MultiAttackIntent(5, 2));
            var stateTempest = new MoveState("ALCHEMIST_TEMPEST", MoveTempest, new SingleAttackIntent(15));

            stateTrap.FollowUpState = stateShift;
            stateShift.FollowUpState = stateTempest;
            stateTempest.FollowUpState = stateShift;

            return new MonsterMoveStateMachine(new MonsterState[] { stateTrap, stateShift, stateTempest }, stateTrap);
        }

        private async Task MoveTrap(IReadOnlyList<Creature> targets)
        {
            await DamageCmd.Attack(7).FromMonster(this)
                .WithAttackerAnim("Attack", 0.15f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(null);

            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null && !player.Creature.HasPower<TimeDistortionPower>())
            {
                await PowerCmd.Apply<TimeDistortionPower>(new ThrowingPlayerChoiceContext(), player.Creature, 5m, Creature, null);
            }
            await Cmd.Wait(0.2f);
        }

        private async Task MoveShift(IReadOnlyList<Creature> targets)
        {
            await CreatureCmd.GainBlock(Creature, 12, ValueProp.Move, null);
            for (int i = 0; i < 2; i++)
            {
                await DamageCmd.Attack(5).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.12f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(null);
                await Cmd.Wait(0.12f);
            }
        }

        private async Task MoveTempest(IReadOnlyList<Creature> targets)
        {
            await DamageCmd.Attack(15).FromMonster(this)
                .WithAttackerAnim("Attack", 0.25f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, CastSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.25f);
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

    public class ChronoAlchemistEncounter : EncounterModel
    {
        public override RoomType RoomType => RoomType.Monster;
        public override IEnumerable<MonsterModel> AllPossibleMonsters => new[] { ModelDb.Monster<ChronoAlchemist>() };
        public override float GetCameraScaling() => 0.88f;
        public override Vector2 GetCameraOffset() => Vector2.Down * 40f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<ChronoAlchemist>().ToMutable(), null)
            };
        }
    }

    // ==========================================
    // 4. Spore Goliath / Споровый Симбиот
    // ==========================================
    public sealed class SporeCard : CardModel
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        public SporeCard()
            : base(0, CardType.Status, CardRarity.Token, TargetType.None)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner?.Creature != null)
            {
                await CreatureCmd.Damage(choiceContext, Owner.Creature, 2m, ValueProp.Unblockable, Owner.Creature);
            }
        }
    }

    public class SporeGoliath : MonsterModel
    {
        public override int MinInitialHp => 50;
        public override int MaxInitialHp => 58;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");

        public override LocString Title => new LocString("monsters", "SPORE_GOLIATH.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Plant;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateSlam = new MoveState("GOLIATH_SLAM", MoveSlam, new SingleAttackIntent(8));
            var stateEmission = new MoveState("GOLIATH_EMISSION", MoveEmission, new DebuffIntent(), new StatusIntent(1));
            var stateBurst = new MoveState("GOLIATH_BURST", MoveBurst, new SingleAttackIntent(10));

            stateSlam.FollowUpState = stateEmission;
            stateEmission.FollowUpState = stateBurst;
            stateBurst.FollowUpState = stateSlam;

            return new MonsterMoveStateMachine(new MonsterState[] { stateSlam, stateEmission, stateBurst }, stateSlam);
        }

        private async Task MoveSlam(IReadOnlyList<Creature> targets)
        {
            await DamageCmd.Attack(8).FromMonster(this)
                .WithAttackerAnim("Attack", 0.2f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.2f);
        }

        private async Task MoveEmission(IReadOnlyList<Creature> targets)
        {
            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null && CombatState != null)
            {
                var sporeCards = new List<CardModel>
                {
                    CombatState.CreateCard<SporeCard>(player)
                };
                await CardPileCmd.AddGeneratedCardsToCombat(sporeCards, PileType.Discard, player);
                await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), player.Creature, 1m, Creature, null);
            }
            await Cmd.Wait(0.25f);
        }

        private async Task MoveBurst(IReadOnlyList<Creature> targets)
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
            int totalDamage = 5 + (statusCount * 2);
            await DamageCmd.Attack(totalDamage).FromMonster(this)
                .WithAttackerAnim("Attack", 0.25f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, CastSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.25f);
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

    public class SporeGoliathEncounter : EncounterModel
    {
        public override RoomType RoomType => RoomType.Monster;
        public override IEnumerable<MonsterModel> AllPossibleMonsters => new[] { ModelDb.Monster<SporeGoliath>() };
        public override float GetCameraScaling() => 0.88f;
        public override Vector2 GetCameraOffset() => Vector2.Down * 40f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<SporeGoliath>().ToMutable(), null)
            };
        }
    }

    // ==========================================
    // 5. Slag Cinder / Шлаковый Автоматон
    // ==========================================
    public class SlagFurnacePower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override LocString Title => new LocString("powers", "SLAG_FURNACE_POWER.name");
        public override LocString Description => new LocString("powers", "SLAG_FURNACE_POWER.description");

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.Type == CardType.Attack && cardPlay.Player != null)
            {
                Flash();
                await CreatureCmd.Damage(choiceContext, cardPlay.Player.Creature, 1m, ValueProp.Move, Owner);
            }
        }
    }

    public class SlagCinder : MonsterModel
    {
        public override int MinInitialHp => 80;
        public override int MaxInitialHp => 90;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");

        public override LocString Title => new LocString("monsters", "SLAG_CINDER.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        public override async Task AfterAddedToRoom()
        {
            await base.AfterAddedToRoom();
            await PowerCmd.Apply<SlagFurnacePower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateHeat = new MoveState("SLAG_HEAT_UP", MoveHeatUp, new SingleAttackIntent(8), new BuffIntent());
            var stateHammer = new MoveState("SLAG_HAMMER", MoveHammer, new MultiAttackIntent(6, 2));
            var stateVent = new MoveState("SLAG_VENT", MoveVent, new DefendIntent(), new DebuffIntent());

            stateHeat.FollowUpState = stateHammer;
            stateHammer.FollowUpState = stateVent;
            stateVent.FollowUpState = stateHeat;

            return new MonsterMoveStateMachine(new MonsterState[] { stateHeat, stateHammer, stateVent }, stateHeat);
        }

        private async Task MoveHeatUp(IReadOnlyList<Creature> targets)
        {
            if (!Creature.HasPower<SlagFurnacePower>())
            {
                await PowerCmd.Apply<SlagFurnacePower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
            }
            await DamageCmd.Attack(8).FromMonster(this)
                .WithAttackerAnim("Attack", 0.15f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.2f);
        }

        private async Task MoveHammer(IReadOnlyList<Creature> targets)
        {
            for (int i = 0; i < 2; i++)
            {
                await DamageCmd.Attack(6).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.15f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_attack_blunt")
                    .Execute(null);
                await Cmd.Wait(0.12f);
            }
        }

        private async Task MoveVent(IReadOnlyList<Creature> targets)
        {
            var furnace = Creature.GetPower<SlagFurnacePower>();
            if (furnace != null)
            {
                await PowerCmd.Remove(furnace);
            }
            await CreatureCmd.GainBlock(Creature, 24, ValueProp.Move, null);
            await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
            await Cmd.Wait(0.25f);
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

    public class SlagCinderEncounter : EncounterModel
    {
        public override RoomType RoomType => RoomType.Monster;
        public override IEnumerable<MonsterModel> AllPossibleMonsters => new[] { ModelDb.Monster<SlagCinder>() };
        public override float GetCameraScaling() => 0.88f;
        public override Vector2 GetCameraOffset() => Vector2.Down * 40f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<SlagCinder>().ToMutable(), null)
            };
        }
    }

    // ==========================================
    // 6. The Puppeteer / Теневой Марионеточник
    // ==========================================
    public class PuppetMinion : MonsterModel
    {
        public override int MinInitialHp => 24;
        public override int MaxInitialHp => 28;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");

        public override LocString Title => new LocString("monsters", "PUPPET_MINION.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateGuard = new MoveState("MINION_GUARD", MoveGuard, new DefendIntent(), new BuffIntent());
            var stateStrike = new MoveState("MINION_STRIKE", MoveStrike, new SingleAttackIntent(6));

            stateGuard.FollowUpState = stateStrike;
            stateStrike.FollowUpState = stateGuard;

            return new MonsterMoveStateMachine(new MonsterState[] { stateGuard, stateStrike }, stateGuard);
        }

        private async Task MoveGuard(IReadOnlyList<Creature> targets)
        {
            if (CombatState?.Creatures != null)
            {
                var puppeteer = CombatState.Creatures.FirstOrDefault(c => c.Monster is ThePuppeteer && c.IsAlive);
                if (puppeteer != null)
                {
                    await CreatureCmd.GainBlock(puppeteer, 8, ValueProp.Move, null);
                }
                else
                {
                    await CreatureCmd.GainBlock(Creature, 8, ValueProp.Move, null);
                }
            }
            await Cmd.Wait(0.2f);
        }

        private async Task MoveStrike(IReadOnlyList<Creature> targets)
        {
            await DamageCmd.Attack(6).FromMonster(this)
                .WithAttackerAnim("Attack", 0.15f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(null);
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

    public class PuppetBondsPower : PowerModel
    {
        private bool _enraged = false;

        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override LocString Title => new LocString("powers", "PUPPET_BONDS_POWER.name");
        public override LocString Description => new LocString("powers", "PUPPET_BONDS_POWER.description");

        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
        {
            if (target == Owner && HasLivingPuppet())
            {
                return 0.70m;
            }
            return 1m;
        }

        public override async Task BeforeSideTurnStart(PlayerChoiceContext choiceContext, CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
        {
            if (side == CombatSide.Enemy && Owner != null && Owner.IsAlive && !_enraged && !HasLivingPuppet())
            {
                _enraged = true;
                Flash();
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, 2m, Owner, null);
            }
        }

        private bool HasLivingPuppet()
        {
            if (CombatState?.Creatures == null) return false;
            foreach (var c in CombatState.Creatures)
            {
                if (c != null && c != Owner && c.Monster is PuppetMinion && c.IsAlive)
                {
                    return true;
                }
            }
            return false;
        }
    }

    public class ThePuppeteer : MonsterModel
    {
        public override int MinInitialHp => 62;
        public override int MaxInitialHp => 70;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");

        public override LocString Title => new LocString("monsters", "THE_PUPPETEER.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Stone;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        public override async Task AfterAddedToRoom()
        {
            await base.AfterAddedToRoom();
            await PowerCmd.Apply<PuppetBondsPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateStrings = new MoveState("PUPPET_STRINGS", MoveStrings, new SingleAttackIntent(7), new CardDebuffIntent());
            var stateNeedle = new MoveState("PUPPET_NEEDLE", MoveNeedle, new SingleAttackIntent(12));
            var stateDance = new MoveState("PUPPET_DANCE", MoveDance, new MultiAttackIntent(4, 3));

            stateStrings.FollowUpState = stateNeedle;
            stateNeedle.FollowUpState = stateDance;
            stateDance.FollowUpState = stateStrings;

            return new MonsterMoveStateMachine(new MonsterState[] { stateStrings, stateNeedle, stateDance }, stateStrings);
        }

        private async Task MoveStrings(IReadOnlyList<Creature> targets)
        {
            await DamageCmd.Attack(7).FromMonster(this)
                .WithAttackerAnim("Attack", 0.15f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(null);

            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null && player.PlayerCombatState?.Hand?.Cards != null && player.PlayerCombatState.Hand.Cards.Count > 0)
            {
                var cardToDiscard = player.PlayerCombatState.Hand.Cards[0];
                await CardCmd.Discard(new ThrowingPlayerChoiceContext(), cardToDiscard);
            }
            await Cmd.Wait(0.2f);
        }

        private async Task MoveNeedle(IReadOnlyList<Creature> targets)
        {
            await DamageCmd.Attack(12).FromMonster(this)
                .WithAttackerAnim("Attack", 0.2f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(null);
            await Cmd.Wait(0.2f);
        }

        private async Task MoveDance(IReadOnlyList<Creature> targets)
        {
            for (int i = 0; i < 3; i++)
            {
                await DamageCmd.Attack(4).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.12f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(null);
                await Cmd.Wait(0.12f);
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

    public class ThePuppeteerEncounter : EncounterModel
    {
        public override RoomType RoomType => RoomType.Monster;

        public override IEnumerable<MonsterModel> AllPossibleMonsters => new MonsterModel[]
        {
            ModelDb.Monster<ThePuppeteer>(),
            ModelDb.Monster<PuppetMinion>()
        };

        public override float GetCameraScaling() => 0.85f;
        public override Vector2 GetCameraOffset() => Vector2.Down * 30f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<ThePuppeteer>().ToMutable(), null),
                (ModelDb.Monster<PuppetMinion>().ToMutable(), null)
            };
        }
    }

    public static class ModActHelper
    {
        public static readonly HashSet<ModelId> Act1BossIds = new HashSet<ModelId>
        {
            ModelDb.Encounter<ClockworkColossusBoss>().Id,
            ModelDb.Encounter<MycelialBroodmotherBoss>().Id,
            ModelDb.Encounter<SanguineInquisitorBoss>().Id
        };

        public static readonly HashSet<ModelId> Act2BossIds = new HashSet<ModelId>
        {
            ModelDb.Encounter<MirroredSovereignBoss>().Id,
            ModelDb.Encounter<AlchemicalChimeraBoss>().Id,
            ModelDb.Encounter<NullExecutionerBoss>().Id
        };

        public static readonly HashSet<ModelId> Act3BossIds = new HashSet<ModelId>
        {
            ModelDb.Encounter<ChronosTitanBoss>().Id,
            ModelDb.Encounter<AstralDevourerBoss>().Id,
            ModelDb.Encounter<SpireArchitectBoss>().Id,
            ModelDb.Encounter<SpireHeartBoss>().Id
        };

        public static readonly HashSet<ModelId> AllModBossIds = new HashSet<ModelId>
        {
            ModelDb.Encounter<ClockworkColossusBoss>().Id,
            ModelDb.Encounter<MycelialBroodmotherBoss>().Id,
            ModelDb.Encounter<SanguineInquisitorBoss>().Id,
            ModelDb.Encounter<MirroredSovereignBoss>().Id,
            ModelDb.Encounter<AlchemicalChimeraBoss>().Id,
            ModelDb.Encounter<NullExecutionerBoss>().Id,
            ModelDb.Encounter<ChronosTitanBoss>().Id,
            ModelDb.Encounter<AstralDevourerBoss>().Id,
            ModelDb.Encounter<SpireArchitectBoss>().Id,
            ModelDb.Encounter<SpireHeartBoss>().Id
        };

        public static int GetActIndex(ActModel? act)
        {
            if (act == null) return 0;
            string entry = (act.Id.Entry ?? "").ToUpperInvariant();
            if (act.Index >= 3 || entry.Contains("ACT4") || entry.Contains("ACT_4") || entry.Contains("HEART")) return 3;
            if (act.Index == 2 || entry.Contains("GLORY") || entry.Contains("ACT3") || entry.Contains("ACT_3")) return 2;
            if (act.Index == 1 || entry.Contains("HIVE") || entry.Contains("ACT2") || entry.Contains("ACT_2")) return 1;
            if (act.Index == 0 || entry.Contains("OVERGROWTH") || entry.Contains("UNDERDOCKS") || entry.Contains("ACT1") || entry.Contains("ACT_1")) return 0;
            if (act.Index > 0) return Math.Min(3, act.Index);
            return 0;
        }

        public static List<EncounterModel> GetActBosses(int actIdx)
        {
            if (actIdx == 0)
            {
                return new List<EncounterModel>
                {
                    ModelDb.Encounter<ClockworkColossusBoss>(),
                    ModelDb.Encounter<MycelialBroodmotherBoss>(),
                    ModelDb.Encounter<SanguineInquisitorBoss>()
                };
            }
            if (actIdx == 1)
            {
                return new List<EncounterModel>
                {
                    ModelDb.Encounter<MirroredSovereignBoss>(),
                    ModelDb.Encounter<AlchemicalChimeraBoss>(),
                    ModelDb.Encounter<NullExecutionerBoss>()
                };
            }
            if (actIdx >= 3)
            {
                return new List<EncounterModel>
                {
                    ModelDb.Encounter<SpireHeartBoss>()
                };
            }
            return new List<EncounterModel>
            {
                ModelDb.Encounter<ChronosTitanBoss>(),
                ModelDb.Encounter<AstralDevourerBoss>(),
                ModelDb.Encounter<SpireArchitectBoss>(),
                ModelDb.Encounter<SpireHeartBoss>()
            };
        }

        public static bool IsModBossFromWrongAct(ModelId id, int currentActIdx)
        {
            if (currentActIdx == 0)
            {
                return Act2BossIds.Contains(id) || Act3BossIds.Contains(id);
            }
            if (currentActIdx == 1)
            {
                return Act1BossIds.Contains(id) || Act3BossIds.Contains(id);
            }
            if (currentActIdx == 2)
            {
                return Act1BossIds.Contains(id) || Act2BossIds.Contains(id);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(ActModel), "get_AllEncounters")]
    public static class ActAllEncountersPatch
    {
        public static IEnumerable<EncounterModel> Postfix(IEnumerable<EncounterModel> __result, ActModel __instance)
        {
            int actIdx = ModActHelper.GetActIndex(__instance);
            var existingIds = new HashSet<ModelId>();
            foreach (var enc in __result)
            {
                if (enc == null) continue;
                if (ModActHelper.IsModBossFromWrongAct(enc.Id, actIdx)) continue;
                existingIds.Add(enc.Id);
                yield return enc;
            }

            var encounters = new List<EncounterModel>();
            if (actIdx == 0)
            {
                encounters.Add(ModelDb.Encounter<SporeGoliathEncounter>());
                encounters.Add(ModelDb.Encounter<ChronoWardenEncounter>());
                encounters.AddRange(ModActHelper.GetActBosses(0));
            }
            else if (actIdx == 1)
            {
                encounters.Add(ModelDb.Encounter<ThePuppeteerEncounter>());
                encounters.Add(ModelDb.Encounter<MirrorPhantomEncounter>());
                encounters.Add(ModelDb.Encounter<ChronoAlchemistEncounter>());
                encounters.Add(ModelDb.Encounter<SlagCinderEncounter>());
                encounters.Add(ModelDb.Encounter<SporeGoliathEncounter>());
                encounters.AddRange(ModActHelper.GetActBosses(1));
            }
            else
            {
                encounters.Add(ModelDb.Encounter<ThePuppeteerEncounter>());
                encounters.Add(ModelDb.Encounter<MirrorPhantomEncounter>());
                encounters.Add(ModelDb.Encounter<ChronoAlchemistEncounter>());
                encounters.Add(ModelDb.Encounter<SlagCinderEncounter>());
                encounters.Add(ModelDb.Encounter<ChronoWardenEncounter>());
                encounters.AddRange(ModActHelper.GetActBosses(2));
            }

            foreach (var enc in encounters)
            {
                if (!existingIds.Contains(enc.Id))
                {
                    yield return enc;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ActModel), "get_AllWeakEncounters")]
    public static class ActAllWeakEncountersPatch
    {
        public static IEnumerable<EncounterModel> Postfix(IEnumerable<EncounterModel> __result, ActModel __instance)
        {
            var existingIds = new HashSet<ModelId>();
            foreach (var enc in __result)
            {
                if (enc != null)
                {
                    existingIds.Add(enc.Id);
                    yield return enc;
                }
            }

            var weakEncounters = new List<EncounterModel>();
            string entry = __instance?.Id.Entry ?? "";
            int actIdx = ModActHelper.GetActIndex(__instance);

            if (actIdx == 0 || entry.Contains("OVERGROWTH") || entry.Contains("UNDERDOCKS"))
            {
                // Act 1: strictly keep weak pool EMPTY of complex mod encounters!
                // Floors 1-3 remain vanilla easy starter fights.
            }
            else if (actIdx == 1 || entry.Contains("HIVE"))
            {
                weakEncounters.Add(ModelDb.Encounter<MirrorPhantomEncounter>());
            }
            else
            {
                weakEncounters.Add(ModelDb.Encounter<MirrorPhantomEncounter>());
            }

            foreach (var enc in weakEncounters)
            {
                if (!existingIds.Contains(enc.Id))
                {
                    yield return enc;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ActModel), "get_AllRegularEncounters")]
    public static class ActAllRegularEncountersPatch
    {
        public static IEnumerable<EncounterModel> Postfix(IEnumerable<EncounterModel> __result, ActModel __instance)
        {
            var existingIds = new HashSet<ModelId>();
            foreach (var enc in __result)
            {
                if (enc != null)
                {
                    existingIds.Add(enc.Id);
                    yield return enc;
                }
            }

            var regularEncounters = new List<EncounterModel>();
            string entry = __instance?.Id.Entry ?? "";
            int actIdx = ModActHelper.GetActIndex(__instance);

            if (actIdx == 0 || entry.Contains("OVERGROWTH") || entry.Contains("UNDERDOCKS"))
            {
                regularEncounters.Add(ModelDb.Encounter<SporeGoliathEncounter>());
                regularEncounters.Add(ModelDb.Encounter<ChronoWardenEncounter>());
            }
            else if (actIdx == 1 || entry.Contains("HIVE"))
            {
                regularEncounters.Add(ModelDb.Encounter<ThePuppeteerEncounter>());
                regularEncounters.Add(ModelDb.Encounter<MirrorPhantomEncounter>());
                regularEncounters.Add(ModelDb.Encounter<ChronoAlchemistEncounter>());
                regularEncounters.Add(ModelDb.Encounter<SlagCinderEncounter>());
            }
            else
            {
                regularEncounters.Add(ModelDb.Encounter<ThePuppeteerEncounter>());
                regularEncounters.Add(ModelDb.Encounter<MirrorPhantomEncounter>());
                regularEncounters.Add(ModelDb.Encounter<ChronoAlchemistEncounter>());
                regularEncounters.Add(ModelDb.Encounter<SlagCinderEncounter>());
                regularEncounters.Add(ModelDb.Encounter<ChronoWardenEncounter>());
            }

            foreach (var enc in regularEncounters)
            {
                if (!existingIds.Contains(enc.Id))
                {
                    yield return enc;
                }
            }
        }
    }

    [HarmonyPatch(typeof(ActModel), nameof(ActModel.ApplyDiscoveryOrderModifications))]
    public static class ActApplyDiscoveryOrderModificationsPatch
    {
        public static bool Prefix(ActModel __instance, UnlockState unlockState)
        {
            Log.Info($"[AnimeSurvivalMod] Bypassed vanilla tutorial/discovery order for Act '{__instance?.Id.Entry}'.");
            return false;
        }
    }

    [HarmonyPatch(typeof(ActModel), nameof(ActModel.GenerateRooms))]
    public static class ActGenerateRoomsPatch
    {
        public static void Postfix(ActModel __instance, Rng rng, UnlockState unlockState, bool isMultiplayer)
        {
            try
            {
                var traverse = Traverse.Create(__instance);
                var rooms = traverse.Field("_rooms").GetValue<RoomSet>();
                if (rooms?.normalEncounters == null || rooms.normalEncounters.Count == 0) return;

                string entry = __instance.Id.Entry ?? "";
                int actIdx = ModActHelper.GetActIndex(__instance);

                // 1. Weak encounter injection (slots 0..weakLimit-1)
                var weakPool = new List<EncounterModel>();
                if (actIdx == 0 || entry.Contains("OVERGROWTH") || entry.Contains("UNDERDOCKS"))
                {
                    // In Act 1, keep weakPool empty so floors 1-3 remain vanilla easy fights!
                }
                else if (actIdx == 1 || entry.Contains("HIVE"))
                {
                    weakPool.Add(ModelDb.Encounter<MirrorPhantomEncounter>());
                }
                else
                {
                    weakPool.Add(ModelDb.Encounter<MirrorPhantomEncounter>());
                }

                int numWeak = 3;
                try { numWeak = traverse.Property("NumberOfWeakEncounters").GetValue<int>(); } catch {}
                if (numWeak <= 0) numWeak = 3;
                int weakLimit = Math.Min(numWeak, rooms.normalEncounters.Count);
                bool hasCustomWeak = false;
                for (int i = 0; i < weakLimit; i++)
                {
                    if (weakPool.Any(m => m.Id == rooms.normalEncounters[i].Id))
                    {
                        hasCustomWeak = true;
                        break;
                    }
                }

                if (!hasCustomWeak && weakPool.Count > 0 && weakLimit > 0)
                {
                    int injectIdx = rng.NextInt(weakLimit);
                    var chosen = weakPool[rng.NextInt(weakPool.Count)];
                    rooms.normalEncounters[injectIdx] = chosen;
                    Log.Info($"[AnimeSurvivalMod] Guaranteed mod weak encounter '{chosen.Id.Entry}' at slot {injectIdx}");
                }

                // 2. Regular encounter injection (slots weakLimit..weakLimit+3)
                var regPool = new List<EncounterModel>();
                if (actIdx == 0 || entry.Contains("OVERGROWTH") || entry.Contains("UNDERDOCKS"))
                {
                    regPool.Add(ModelDb.Encounter<SporeGoliathEncounter>());
                    regPool.Add(ModelDb.Encounter<ChronoWardenEncounter>());
                }
                else if (actIdx == 1 || entry.Contains("HIVE"))
                {
                    regPool.Add(ModelDb.Encounter<ThePuppeteerEncounter>());
                    regPool.Add(ModelDb.Encounter<SlagCinderEncounter>());
                    regPool.Add(ModelDb.Encounter<MirrorPhantomEncounter>());
                    regPool.Add(ModelDb.Encounter<ChronoAlchemistEncounter>());
                }
                else
                {
                    regPool.Add(ModelDb.Encounter<ThePuppeteerEncounter>());
                    regPool.Add(ModelDb.Encounter<SlagCinderEncounter>());
                    regPool.Add(ModelDb.Encounter<MirrorPhantomEncounter>());
                    regPool.Add(ModelDb.Encounter<ChronoAlchemistEncounter>());
                }

                int regStart = weakLimit;
                int regEnd = Math.Min(weakLimit + 4, rooms.normalEncounters.Count);
                if (regEnd > regStart && regPool.Count > 0)
                {
                    bool hasCustomReg = false;
                    for (int i = regStart; i < regEnd; i++)
                    {
                        if (regPool.Any(m => m.Id == rooms.normalEncounters[i].Id))
                        {
                            hasCustomReg = true;
                            break;
                        }
                    }
                    if (!hasCustomReg)
                    {
                        int injectIdx = regStart + rng.NextInt(regEnd - regStart);
                        var chosen = regPool[rng.NextInt(regPool.Count)];
                        rooms.normalEncounters[injectIdx] = chosen;
                        Log.Info($"[AnimeSurvivalMod] Guaranteed mod regular encounter '{chosen.Id.Entry}' at slot {injectIdx}");
                    }
                }

                // 3. Act Boss promotion (65% chance to feature a mod boss)
                actIdx = ModActHelper.GetActIndex(__instance);
                var actBosses = ModActHelper.GetActBosses(actIdx);

                if (actBosses.Count > 0)
                {
                    bool currentIsThisActMod = rooms.Boss != null && actBosses.Any(b => b.Id == rooms.Boss.Id);
                    bool currentIsWrongActMod = rooms.Boss != null && ModActHelper.IsModBossFromWrongAct(rooms.Boss.Id, actIdx);

                    if (currentIsWrongActMod || (!currentIsThisActMod && rng.NextInt(100) < 65))
                    {
                        rooms.Boss = actBosses[rng.NextInt(actBosses.Count)];
                        Log.Info($"[AnimeSurvivalMod] Featured mod boss for Act '{entry}' (Index {actIdx}): {rooms.Boss.Id.Entry}");
                    }
                }

                Log.Info($"[AnimeSurvivalMod] Act '{entry}' room generation complete. Boss: {rooms.Boss?.Id.Entry}");
            }
            catch (Exception ex)
            {
                Log.Error($"[AnimeSurvivalMod] Error in ActGenerateRoomsPatch: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(ActModel), nameof(ActModel.ValidateRoomsAfterLoad))]
    public static class ActValidateRoomsAfterLoadPatch
    {
        public static void Postfix(ActModel __instance, Rng rng)
        {
            try
            {
                var traverse = Traverse.Create(__instance);
                var rooms = traverse.Field("_rooms").GetValue<RoomSet>();
                if (rooms?.Boss == null) return;
                int actIdx = ModActHelper.GetActIndex(__instance);
                if (ModActHelper.IsModBossFromWrongAct(rooms.Boss.Id, actIdx))
                {
                    var valid = ModActHelper.GetActBosses(actIdx);
                    rooms.Boss = valid[rng.NextInt(valid.Count)];
                    Log.Warn($"[AnimeSurvivalMod] ValidateRoomsAfterLoad corrected wrong-act boss to '{rooms.Boss.Id.Entry}' for Act {actIdx}!");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[AnimeSurvivalMod] Error in ValidateRoomsAfterLoad patch: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(ActModel), "get_BossEncounter")]
    public static class ActBossEncounterPatch
    {
        public static void Postfix(ActModel __instance, ref EncounterModel __result)
        {
            if (__instance == null || __result == null) return;
            int actIdx = ModActHelper.GetActIndex(__instance);
            if (ModActHelper.IsModBossFromWrongAct(__result.Id, actIdx))
            {
                var validBosses = ModActHelper.GetActBosses(actIdx);
                var oldId = __result.Id.Entry;
                __result = validBosses[0];
                Log.Warn($"[AnimeSurvivalMod] ActBossEncounterPatch auto-corrected wrong-act boss '{oldId}' in Act {actIdx} to '{__result.Id.Entry}'!");
            }
        }
    }

    [HarmonyPatch(typeof(ActModel), "get_AllBossEncounters")]
    public static class ActAllBossEncountersPatch
    {
        public static IEnumerable<EncounterModel> Postfix(IEnumerable<EncounterModel> __result, ActModel __instance)
        {
            int actIdx = ModActHelper.GetActIndex(__instance);
            var existingIds = new HashSet<ModelId>();
            foreach (var enc in __result)
            {
                if (enc == null) continue;
                if (ModActHelper.IsModBossFromWrongAct(enc.Id, actIdx)) continue;
                existingIds.Add(enc.Id);
                yield return enc;
            }

            var bosses = ModActHelper.GetActBosses(actIdx);
            foreach (var b in bosses)
            {
                if (!existingIds.Contains(b.Id))
                {
                    yield return b;
                }
            }
        }
    }

    [HarmonyPatch(typeof(EncounterModel), "get_BossNodePath")]
    public static class EncounterBossNodePathPatch
    {
        public static void Postfix(EncounterModel __instance, ref string __result)
        {
            if (__instance != null && __instance.RoomType == RoomType.Boss)
            {
                if (string.IsNullOrEmpty(__result) || (!ResourceLoader.Exists(__result) && !ResourceLoader.Exists(__result + ".png")))
                {
                    __result = "res://images/map/placeholder/vantom_boss_icon";
                }
            }
        }
    }

    [HarmonyPatch(typeof(EncounterModel), "get_MapNodeAssetPaths")]
    public static class EncounterMapNodeAssetPathsPatch
    {
        public static void Postfix(EncounterModel __instance, ref IEnumerable<string> __result)
        {
            if (__instance != null && __instance.RoomType == RoomType.Boss)
            {
                var filtered = new List<string>();
                foreach (var p in __result)
                {
                    if (!string.IsNullOrEmpty(p) && ResourceLoader.Exists(p))
                    {
                        filtered.Add(p);
                    }
                }
                if (filtered.Count == 0)
                {
                    filtered.Add("res://images/map/placeholder/vantom_boss_icon.png");
                    filtered.Add("res://images/map/placeholder/vantom_boss_icon_outline.png");
                }
                __result = filtered;
            }
        }
    }

    [HarmonyPatch(typeof(ImageHelper), nameof(ImageHelper.GetRoomIconPath))]
    public static class ImageHelperRoomIconPathPatch
    {
        public static void Postfix(MapPointType mapPointType, RoomType roomType, ModelId? modelId, ref string? __result)
        {
            if (roomType == RoomType.Boss)
            {
                if (string.IsNullOrEmpty(__result) || !ResourceLoader.Exists(__result))
                {
                    string id = modelId?.Entry ?? "";
                    if (id.Contains("Colossus", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/vantom_boss.png";
                    else if (id.Contains("Broodmother", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/the_kin_boss.png";
                    else if (id.Contains("Inquisitor", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/waterfall_giant_boss.png";
                    else if (id.Contains("Sovereign", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/kaiser_crab_boss.png";
                    else if (id.Contains("Chimera", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/knowledge_demon_boss.png";
                    else if (id.Contains("Executioner", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/soul_fysh_boss.png";
                    else if (id.Contains("Chronos", StringComparison.OrdinalIgnoreCase) || id.Contains("Titan", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/test_subject_boss.png";
                    else if (id.Contains("Devourer", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/aeonglass_boss.png";
                    else if (id.Contains("Architect", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/lagavulin_matriarch_boss.png";
                    else __result = "res://images/ui/run_history/vantom_boss.png";
                }
            }
        }
    }

    [HarmonyPatch(typeof(ImageHelper), nameof(ImageHelper.GetRoomIconOutlinePath))]
    public static class ImageHelperRoomIconOutlinePathPatch
    {
        public static void Postfix(MapPointType mapPointType, RoomType roomType, ModelId? modelId, ref string? __result)
        {
            if (roomType == RoomType.Boss)
            {
                if (string.IsNullOrEmpty(__result) || !ResourceLoader.Exists(__result))
                {
                    string id = modelId?.Entry ?? "";
                    if (id.Contains("Colossus", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/vantom_boss_outline.png";
                    else if (id.Contains("Broodmother", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/the_kin_boss_outline.png";
                    else if (id.Contains("Inquisitor", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/waterfall_giant_boss_outline.png";
                    else if (id.Contains("Sovereign", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/kaiser_crab_boss_outline.png";
                    else if (id.Contains("Chimera", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/knowledge_demon_boss_outline.png";
                    else if (id.Contains("Executioner", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/soul_fysh_boss_outline.png";
                    else if (id.Contains("Chronos", StringComparison.OrdinalIgnoreCase) || id.Contains("Titan", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/test_subject_boss_outline.png";
                    else if (id.Contains("Devourer", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/aeonglass_boss_outline.png";
                    else if (id.Contains("Architect", StringComparison.OrdinalIgnoreCase)) __result = "res://images/ui/run_history/lagavulin_matriarch_boss_outline.png";
                    else __result = "res://images/ui/run_history/vantom_boss_outline.png";
                }
            }
        }
    }

    [HarmonyPatch(typeof(AssetCache), "LoadAsset")]
    public static class AssetCacheLoadAssetPatch
    {
        public static bool Prefix(AssetCache __instance, string path, ref Resource __result)
        {
            try
            {
                if (ResourceLoader.Exists(path))
                {
                    return true;
                }
            }
            catch {}

            Log.Warn($"[AnimeSurvivalMod] AssetCache.LoadAsset intercepted missing asset: '{path}'. Preventing crash.");
            if (path != null && (path.EndsWith(".png") || path.EndsWith(".tres")))
            {
                if (ResourceLoader.Exists("res://images/map/placeholder/vantom_boss_icon.png"))
                {
                    __result = ResourceLoader.Load<Resource>("res://images/map/placeholder/vantom_boss_icon.png");
                    return false;
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(FightConsoleCmd), nameof(FightConsoleCmd.Process))]
    public static class FightConsoleCmdPatch
    {
        public static void Prefix(ref string[] args)
        {
            if (args != null && args.Length > 0)
            {
                string norm = args[0].Replace("_", "").ToUpperInvariant();
                if (norm == "CHRONOWARDEN" || norm == "CHRONOWARDENENCOUNTER"
                    || norm == "GARGOYLE" || norm == "STONEGARGOYLE" || norm == "DIONYSUS" || norm == "STATUE")
                {
                    args[0] = "CHRONO_WARDEN_ENCOUNTER";
                }
                else if (norm == "MIRRORPHANTOM" || norm == "MIRRORPHANTOMENCOUNTER"
                    || norm == "MIRROR" || norm == "PHANTOM")
                {
                    args[0] = "MIRROR_PHANTOM_ENCOUNTER";
                }
                else if (norm == "CHRONOALCHEMIST" || norm == "CHRONOALCHEMISTENCOUNTER"
                    || norm == "ALCHEMIST" || norm == "CHRONO")
                {
                    args[0] = "CHRONO_ALCHEMIST_ENCOUNTER";
                }
                else if (norm == "SPOREGOLIATH" || norm == "SPOREGOLIATHENCOUNTER"
                    || norm == "SPORE" || norm == "GOLIATH" || norm == "SYMBIOTE")
                {
                    args[0] = "SPORE_GOLIATH_ENCOUNTER";
                }
                else if (norm == "SLAGCINDER" || norm == "SLAGCINDERENCOUNTER"
                    || norm == "SLAG" || norm == "CINDER" || norm == "AUTOMATON")
                {
                    args[0] = "SLAG_CINDER_ENCOUNTER";
                }
                else if (norm == "THEPUPPETEER" || norm == "THEPUPPETEERENCOUNTER"
                    || norm == "PUPPETEER" || norm == "PUPPET")
                {
                    args[0] = "THE_PUPPETEER_ENCOUNTER";
                }
                // Act 1 Bosses
                else if (norm == "CLOCKWORKCOLOSSUS" || norm == "COLOSSUS" || norm == "CLOCKWORK")
                {
                    args[0] = "CLOCKWORK_COLOSSUS_BOSS";
                }
                else if (norm == "MYCELIALBROODMOTHER" || norm == "BROODMOTHER" || norm == "MYCELIAL")
                {
                    args[0] = "MYCELIAL_BROODMOTHER_BOSS";
                }
                else if (norm == "SANGUINEINQUISITOR" || norm == "INQUISITOR" || norm == "SANGUINE")
                {
                    args[0] = "SANGUINE_INQUISITOR_BOSS";
                }
                // Act 2 Bosses
                else if (norm == "MIRROREDSOVEREIGN" || norm == "SOVEREIGN" || norm == "MIRRORKING")
                {
                    args[0] = "MIRRORED_SOVEREIGN_BOSS";
                }
                else if (norm == "ALCHEMICALCHIMERA" || norm == "CHIMERA")
                {
                    args[0] = "ALCHEMICAL_CHIMERA_BOSS";
                }
                else if (norm == "NULLEXECUTIONER" || norm == "EXECUTIONER" || norm == "NULL")
                {
                    args[0] = "NULL_EXECUTIONER_BOSS";
                }
                // Act 3 Bosses
                else if (norm == "CHRONOSTITAN" || norm == "TITAN")
                {
                    args[0] = "CHRONOS_TITAN_BOSS";
                }
                else if (norm == "ASTRALDEVOURER" || norm == "DEVOURER" || norm == "ASTRAL")
                {
                    args[0] = "ASTRAL_DEVOURER_BOSS";
                }
                else if (norm == "SPIREARCHITECT" || norm == "ARCHITECT" || norm == "SPIREBOSS")
                {
                    args[0] = "SPIRE_ARCHITECT_BOSS";
                }
            }
        }
    }

    [HarmonyPatch(typeof(CardConsoleCmd), nameof(CardConsoleCmd.Process))]
    public static class CardConsoleCmdPatch
    {
        public static void Prefix(ref string[] args)
        {
            if (args != null && args.Length > 0)
            {
                string norm = args[0].Replace("_", "").ToUpperInvariant();
                foreach (var card in ModelDb.AllCards)
                {
                    if (card.Id.Entry.Replace("_", "").ToUpperInvariant() == norm)
                    {
                        args[0] = card.Id.Entry;
                        break;
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(NCreatureVisuals), nameof(NCreatureVisuals.IsPlayingHurtAnimation))]
    public static class CreatureVisualsHurtPatch
    {
        public static bool Prefix(NCreatureVisuals __instance, ref bool __result)
        {
            if (__instance.SpineBody == null)
            {
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MonsterModel), nameof(MonsterModel.CreateVisuals))]
    public static class MonsterCustomVisualsPatch
    {
        private static Texture2D? _dionysusTexture;
        private static Texture2D? _mirrorPhantomTexture;
        private static Texture2D? _chronoAlchemistTexture;
        private static Texture2D? _sporeGoliathTexture;
        private static Texture2D? _slagCinderTexture;
        private static Texture2D? _thePuppeteerTexture;
        private static Texture2D? _puppetMinionTexture;
        // Boss Textures
        private static Texture2D? _clockworkColossusTexture;
        private static Texture2D? _mycelialBroodmotherTexture;
        private static Texture2D? _sanguineInquisitorTexture;
        private static Texture2D? _mirroredSovereignTexture;
        private static Texture2D? _alchemicalChimeraTexture;
        private static Texture2D? _nullExecutionerTexture;
        private static Texture2D? _chronosTitanTexture;
        private static Texture2D? _astralDevourerTexture;
        private static Texture2D? _spireArchitectTexture;

        public static string GetImagesDir()
        {
            var candidates = new List<string>();
            try
            {
                string loc = Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(loc))
                {
                    candidates.Add(Path.Combine(Path.GetDirectoryName(loc)!, "images"));
                }
            }
            catch {}

            try
            {
                string exeDir = Path.GetDirectoryName(OS.GetExecutablePath()) ?? "";
                if (!string.IsNullOrEmpty(exeDir))
                {
                    candidates.Add(Path.Combine(exeDir, "mods", "AnimeSurvivalMod", "images"));
                }
            }
            catch {}

            candidates.Add(Path.Combine(AppContext.BaseDirectory, "mods", "AnimeSurvivalMod", "images"));
            candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "mods", "AnimeSurvivalMod", "images"));

            foreach (var c in candidates)
            {
                if (Directory.Exists(c))
                {
                    return c;
                }
            }
            return candidates[0];
        }

        public static Texture2D? GetTexture(string filename)
        {
            try
            {
                string dir = GetImagesDir();
                string path = Path.Combine(dir, filename);
                Log.Info($"[AnimeSurvivalMod] Searching for monster sprite at: {path}");
                if (File.Exists(path))
                {
                    var img = Image.LoadFromFile(path);
                    if (img != null)
                    {
                        var tex = ImageTexture.CreateFromImage(img);
                        Log.Info($"[AnimeSurvivalMod] Loaded monster texture {filename} ({img.GetWidth()}x{img.GetHeight()})!");
                        return tex;
                    }
                }
                else
                {
                    Log.Warn($"[AnimeSurvivalMod] Monster sprite not found: {path}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[AnimeSurvivalMod] Failed to load monster texture {filename}: {ex}");
            }
            return null;
        }

        public static void Postfix(MonsterModel __instance, ref NCreatureVisuals __result)
        {
            if (__instance is ChronoWarden)
            {
                _dionysusTexture ??= GetTexture("statue_of_dionysus.png");
                AttachCustomSprite(__result, _dionysusTexture, new Vector2(0.48f, 0.48f), new Vector2(0f, -260f), new Vector2(-160f, -520f), new Vector2(320f, 520f), new Vector2(0f, -540f), new Vector2(0f, -260f), isFloating: false);
            }
            else if (__instance is MirrorPhantom)
            {
                _mirrorPhantomTexture ??= GetTexture("mirror_phantom.png");
                AttachCustomSprite(__result, _mirrorPhantomTexture, new Vector2(0.48f, 0.48f), new Vector2(0f, -250f), new Vector2(-160f, -500f), new Vector2(320f, 500f), new Vector2(0f, -530f), new Vector2(0f, -250f), isFloating: true);
            }
            else if (__instance is ChronoAlchemist)
            {
                _chronoAlchemistTexture ??= GetTexture("chrono_alchemist.png");
                AttachCustomSprite(__result, _chronoAlchemistTexture, new Vector2(0.48f, 0.48f), new Vector2(0f, -240f), new Vector2(-180f, -480f), new Vector2(360f, 480f), new Vector2(0f, -500f), new Vector2(0f, -240f), isFloating: true);
            }
            else if (__instance is SporeGoliath)
            {
                _sporeGoliathTexture ??= GetTexture("spore_goliath.png");
                AttachCustomSprite(__result, _sporeGoliathTexture, new Vector2(0.50f, 0.50f), new Vector2(0f, -240f), new Vector2(-210f, -480f), new Vector2(420f, 480f), new Vector2(0f, -510f), new Vector2(0f, -240f), isFloating: false);
            }
            else if (__instance is SlagCinder)
            {
                _slagCinderTexture ??= GetTexture("slag_cinder.png");
                AttachCustomSprite(__result, _slagCinderTexture, new Vector2(0.48f, 0.48f), new Vector2(0f, -230f), new Vector2(-190f, -470f), new Vector2(380f, 470f), new Vector2(0f, -500f), new Vector2(0f, -230f), isFloating: false);
            }
            else if (__instance is ThePuppeteer)
            {
                _thePuppeteerTexture ??= GetTexture("the_puppeteer.png");
                AttachCustomSprite(__result, _thePuppeteerTexture, new Vector2(0.48f, 0.48f), new Vector2(0f, -240f), new Vector2(-150f, -480f), new Vector2(300f, 480f), new Vector2(0f, -510f), new Vector2(0f, -240f), isFloating: false);
            }
            else if (__instance is PuppetMinion)
            {
                _puppetMinionTexture ??= GetTexture("puppet_minion.png");
                AttachCustomSprite(__result, _puppetMinionTexture, new Vector2(0.42f, 0.42f), new Vector2(0f, -200f), new Vector2(-120f, -400f), new Vector2(240f, 400f), new Vector2(0f, -420f), new Vector2(0f, -200f), isFloating: true);
            }
            // Act 1 Bosses
            else if (__instance is ClockworkColossus)
            {
                _clockworkColossusTexture ??= GetTexture("clockwork_colossus.png");
                AttachCustomSprite(__result, _clockworkColossusTexture, new Vector2(0.50f, 0.50f), new Vector2(0f, -270f), new Vector2(-210f, -540f), new Vector2(420f, 540f), new Vector2(0f, -570f), new Vector2(0f, -270f), isFloating: false);
            }
            else if (__instance is MycelialBroodmother)
            {
                _mycelialBroodmotherTexture ??= GetTexture("mycelial_broodmother.png");
                AttachCustomSprite(__result, _mycelialBroodmotherTexture, new Vector2(0.50f, 0.50f), new Vector2(0f, -280f), new Vector2(-220f, -560f), new Vector2(440f, 560f), new Vector2(0f, -590f), new Vector2(0f, -280f), isFloating: false);
            }
            else if (__instance is SanguineInquisitor)
            {
                _sanguineInquisitorTexture ??= GetTexture("sanguine_inquisitor.png");
                AttachCustomSprite(__result, _sanguineInquisitorTexture, new Vector2(0.50f, 0.50f), new Vector2(0f, -280f), new Vector2(-200f, -560f), new Vector2(400f, 560f), new Vector2(0f, -590f), new Vector2(0f, -280f), isFloating: false);
            }
            // Act 2 Bosses
            else if (__instance is MirroredSovereign)
            {
                _mirroredSovereignTexture ??= GetTexture("mirrored_sovereign.png");
                AttachCustomSprite(__result, _mirroredSovereignTexture, new Vector2(0.50f, 0.50f), new Vector2(0f, -260f), new Vector2(-190f, -520f), new Vector2(380f, 520f), new Vector2(0f, -550f), new Vector2(0f, -260f), isFloating: true);
            }
            else if (__instance is AlchemicalChimera)
            {
                _alchemicalChimeraTexture ??= GetTexture("alchemical_chimera.png");
                AttachCustomSprite(__result, _alchemicalChimeraTexture, new Vector2(0.50f, 0.50f), new Vector2(0f, -250f), new Vector2(-220f, -500f), new Vector2(440f, 500f), new Vector2(0f, -530f), new Vector2(0f, -250f), isFloating: false);
            }
            else if (__instance is NullExecutioner)
            {
                _nullExecutionerTexture ??= GetTexture("null_executioner.png");
                if (_nullExecutionerTexture != null)
                {
                    AttachCustomSprite(__result, _nullExecutionerTexture, new Vector2(0.50f, 0.50f), new Vector2(0f, -270f), new Vector2(-210f, -540f), new Vector2(420f, 540f), new Vector2(0f, -570f), new Vector2(0f, -270f), isFloating: false);
                }
            }
            // Act 3 Bosses
            else if (__instance is ChronosTitan)
            {
                _chronosTitanTexture ??= GetTexture("chronos_titan.png");
                if (_chronosTitanTexture != null)
                {
                    AttachCustomSprite(__result, _chronosTitanTexture, new Vector2(0.50f, 0.50f), new Vector2(0f, -270f), new Vector2(-210f, -540f), new Vector2(420f, 540f), new Vector2(0f, -570f), new Vector2(0f, -270f), isFloating: true);
                }
            }
            else if (__instance is AstralDevourer)
            {
                _astralDevourerTexture ??= GetTexture("astral_devourer.png");
                if (_astralDevourerTexture != null)
                {
                    AttachCustomSprite(__result, _astralDevourerTexture, new Vector2(0.50f, 0.50f), new Vector2(0f, -270f), new Vector2(-210f, -540f), new Vector2(420f, 540f), new Vector2(0f, -570f), new Vector2(0f, -270f), isFloating: true);
                }
            }
            else if (__instance is SpireArchitect)
            {
                _spireArchitectTexture ??= GetTexture("spire_architect.png");
                if (_spireArchitectTexture != null)
                {
                    AttachCustomSprite(__result, _spireArchitectTexture, new Vector2(0.50f, 0.50f), new Vector2(0f, -270f), new Vector2(-210f, -540f), new Vector2(420f, 540f), new Vector2(0f, -570f), new Vector2(0f, -270f), isFloating: false);
                }
            }
            // Boss Minions
            else if (__instance is SporeCocoon)
            {
                _sporeGoliathTexture ??= GetTexture("spore_goliath.png");
                AttachCustomSprite(__result, _sporeGoliathTexture, new Vector2(0.28f, 0.28f), new Vector2(0f, -140f), new Vector2(-120f, -280f), new Vector2(240f, 280f), new Vector2(0f, -300f), new Vector2(0f, -140f), isFloating: false);
            }
            else if (__instance is MirrorClone)
            {
                _mirrorPhantomTexture ??= GetTexture("mirror_phantom.png");
                AttachCustomSprite(__result, _mirrorPhantomTexture, new Vector2(0.30f, 0.30f), new Vector2(0f, -150f), new Vector2(-100f, -300f), new Vector2(200f, 300f), new Vector2(0f, -320f), new Vector2(0f, -150f), isFloating: true);
            }
            else if (__instance is AstralStar)
            {
                _astralDevourerTexture ??= GetTexture("astral_devourer.png");
                AttachCustomSprite(__result, _astralDevourerTexture, new Vector2(0.25f, 0.25f), new Vector2(0f, -130f), new Vector2(-100f, -260f), new Vector2(200f, 260f), new Vector2(0f, -280f), new Vector2(0f, -130f), isFloating: true);
            }
        }

        private static void AttachCustomSprite(NCreatureVisuals visuals, Texture2D? tex, Vector2 scale, Vector2 pos, Vector2 boundsPos, Vector2 boundsSize, Vector2 intentPosCoord, Vector2 centerPosCoord, bool isFloating = true)
        {
            if (visuals == null || tex == null) return;
            try
            {
                // 1. Hide original Spine node
                var oldVisuals = visuals.GetNodeOrNull<Node2D>("%Visuals");
                if (oldVisuals != null)
                {
                    oldVisuals.Visible = false;
                    oldVisuals.ProcessMode = Node.ProcessModeEnum.Disabled;
                    oldVisuals.Name = "OriginalSpine_Disabled";
                    oldVisuals.UniqueNameInOwner = false;
                }

                // 2. Clear SpineBody so engine treats this as non-Spine 2D sprite
                typeof(NCreatureVisuals).GetField("<SpineBody>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(visuals, null);

                // 3. Attach custom Sprite2D node
                var sprite = new Sprite2D();
                sprite.Name = "Visuals";
                sprite.Texture = tex;
                sprite.Scale = scale;
                sprite.Position = pos;
                sprite.UniqueNameInOwner = true;
                visuals.AddChild(sprite);
                if (visuals.Owner != null) sprite.Owner = visuals.Owner;
                else sprite.Owner = visuals;

                // Dynamic idle breathing / floating animation once entered tree
                sprite.Ready += () =>
                {
                    try
                    {
                        var tween = sprite.CreateTween();
                        tween.SetLoops();

                        float floatY = isFloating ? 10f : 4f;
                        float breathScaleY = isFloating ? 1.025f : 1.015f;
                        float breathScaleX = isFloating ? 0.985f : 0.99f;
                        double duration = isFloating ? 2.2 : 2.6;

                        // Inhale / float up
                        tween.TweenProperty(sprite, "position:y", pos.Y - floatY, duration)
                             .SetTrans(Tween.TransitionType.Sine)
                             .SetEase(Tween.EaseType.InOut);
                        tween.Parallel().TweenProperty(sprite, "scale", new Vector2(scale.X * breathScaleX, scale.Y * breathScaleY), duration)
                             .SetTrans(Tween.TransitionType.Sine)
                             .SetEase(Tween.EaseType.InOut);

                        // Exhale / float down
                        tween.TweenProperty(sprite, "position:y", pos.Y, duration)
                             .SetTrans(Tween.TransitionType.Sine)
                             .SetEase(Tween.EaseType.InOut);
                        tween.Parallel().TweenProperty(sprite, "scale", scale, duration)
                             .SetTrans(Tween.TransitionType.Sine)
                             .SetEase(Tween.EaseType.InOut);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"[AnimeSurvivalMod] Error starting idle breathing tween: {ex}");
                    }
                };

                // Update _body field
                typeof(NCreatureVisuals).GetField("_body", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.SetValue(visuals, sprite);

                // 4. Update bounds (hitbox)
                var bounds = visuals.GetNodeOrNull<Control>("%Bounds");
                if (bounds != null)
                {
                    bounds.Position = boundsPos;
                    bounds.Size = boundsSize;
                }

                // 5. Update IntentPos
                var intentPos = visuals.GetNodeOrNull<Marker2D>("%IntentPos");
                if (intentPos != null)
                {
                    intentPos.Position = intentPosCoord;
                }

                // 6. Update CenterPos
                var centerPos = visuals.GetNodeOrNull<Marker2D>("%CenterPos");
                if (centerPos != null)
                {
                    centerPos.Position = centerPosCoord;
                }

                Log.Info("[AnimeSurvivalMod] Custom monster sprite attached successfully with breathing animation!");
            }
            catch (Exception ex)
            {
                Log.Error($"[AnimeSurvivalMod] Failed to attach custom sprite: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Nodes.NGame), "IsReleaseGame")]
    public static class Patch_BypassIsReleaseGame
    {
        public static bool Prefix(ref bool __result)
        {
            if (CommandLineHelper.HasArg("autoslay"))
            {
                Log.Info("[AnimeSurvivalMod] IsReleaseGame: Returning FALSE because autoslay is requested!");
                __result = false;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Nodes.NGame), "CheckShowModdedSaveFilePopup")]
    public static class Patch_BypassModdedSaveFilePopup
    {
        public static bool Prefix()
        {
            Log.Info("[AnimeSurvivalMod] CheckShowModdedSaveFilePopup: SUPPRESSED popup!");
            return false;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Nodes.NGame), "CheckShowSaveFileError")]
    public static class Patch_BypassSaveFileErrorPopup
    {
        public static bool Prefix()
        {
            Log.Info("[AnimeSurvivalMod] CheckShowSaveFileError: SUPPRESSED popup!");
            return false;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Nodes.NGame), "CheckShowLocalizationOverrideErrors")]
    public static class Patch_BypassLocalizationOverrideErrors
    {
        public static bool Prefix()
        {
            Log.Info("[AnimeSurvivalMod] CheckShowLocalizationOverrideErrors: SUPPRESSED popup!");
            return false;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.AutoSlay.AutoSlayer), "Start")]
    public static class Patch_AutoSlayerStart
    {
        public static void Prefix(string seed, string logFile)
        {
            Log.Info($"[AnimeSurvivalMod] AutoSlayer.Start called! seed='{seed}', logFile='{logFile}'");
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectButton), "Select")]
    public static class Patch_CharacterSelectButton
    {
        private static bool _redirecting = false;

        public static bool Prefix(MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectButton __instance)
        {
            if (_redirecting) return true;
            if (!CommandLineHelper.HasArg("autoslay")) return true;

            string target = CommandLineHelper.GetValue("autoslay");
            if (string.IsNullOrWhiteSpace(target) || target.Equals("true", StringComparison.OrdinalIgnoreCase))
                return true;

            if (__instance.Character != null && !__instance.Character.Id.Entry.Equals(target, StringComparison.OrdinalIgnoreCase))
            {
                var parent = __instance.GetParent();
                if (parent != null)
                {
                    var buttons = MegaCrit.Sts2.Core.AutoSlay.Helpers.UiHelper.FindAll<MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect.NCharacterSelectButton>(parent);
                    foreach (var btn in buttons)
                    {
                        if (btn.Character != null && btn.Character.Id.Entry.Equals(target, StringComparison.OrdinalIgnoreCase))
                        {
                            Log.Info($"[AnimeSurvivalMod] Redirecting character selection from {__instance.Character.Id.Entry} to {target}!");
                            _redirecting = true;
                            try { btn.Select(); } finally { _redirecting = false; }
                            return false;
                        }
                    }
                }
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Saves.UserDataPathProvider), "GetProfileDir", new Type[] { typeof(int), typeof(bool?) })]
    public static class Patch_WorkerProfileDir
    {
        public static bool Prefix(int profileId, bool? forceModState, ref string __result)
        {
            string? workerId = null;
            if (CommandLineHelper.TryGetValue("worker-id", out string? wId) && !string.IsNullOrWhiteSpace(wId))
            {
                workerId = wId;
            }
            else if (CommandLineHelper.TryGetValue("autoslay-worker", out string? awId) && !string.IsNullOrWhiteSpace(awId))
            {
                workerId = awId;
            }

            if (workerId != null)
            {
                string accountDir = MegaCrit.Sts2.Core.Saves.UserDataPathProvider.GetAccountDir(forceModState);
                __result = Godot.StringExtensions.PathJoin(accountDir, $"worker_{workerId}");
                Log.Info($"[AnimeSurvivalMod] Isolated profile path directed to '{__result}' for worker '{workerId}'");
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Platform.Steam.SteamRemoteSaveStore), "WriteFile", new Type[] { typeof(string), typeof(byte[]) })]
    public static class Patch_SkipSteamRemoteStoreWriteBytes
    {
        public static bool Prefix()
        {
            if (CommandLineHelper.HasArg("worker-id") || CommandLineHelper.HasArg("autoslay-worker"))
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Platform.Steam.SteamRemoteSaveStore), "WriteFile", new Type[] { typeof(string), typeof(string) })]
    public static class Patch_SkipSteamRemoteStoreWriteString
    {
        public static bool Prefix()
        {
            if (CommandLineHelper.HasArg("worker-id") || CommandLineHelper.HasArg("autoslay-worker"))
                return false;
            return true;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Platform.Steam.SteamRemoteSaveStore), "WriteFileAsync", new Type[] { typeof(string), typeof(byte[]) })]
    public static class Patch_SkipSteamRemoteStoreWriteBytesAsync
    {
        public static bool Prefix(ref Task __result)
        {
            if (CommandLineHelper.HasArg("worker-id") || CommandLineHelper.HasArg("autoslay-worker"))
            {
                __result = Task.CompletedTask;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Platform.Steam.SteamRemoteSaveStore), "WriteFileAsync", new Type[] { typeof(string), typeof(string) })]
    public static class Patch_SkipSteamRemoteStoreWriteStringAsync
    {
        public static bool Prefix(ref Task __result)
        {
            if (CommandLineHelper.HasArg("worker-id") || CommandLineHelper.HasArg("autoslay-worker"))
            {
                __result = Task.CompletedTask;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Commands.VfxCmd), "PlayOnCreatureCenter", new Type[] { typeof(Creature), typeof(string) })]
    public static class Patch_SafeVfxPlayOnCreatureCenter
    {
        public static bool Prefix(Creature target, string path)
        {
            if (target == null || target.IsDead || string.IsNullOrEmpty(path))
                return false;
            return true;
        }

        public static Exception? Finalizer(Exception? __exception)
        {
            return null;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Commands.VfxCmd), "PlayOnCreature", new Type[] { typeof(Creature), typeof(string) })]
    public static class Patch_SafeVfxPlayOnCreature
    {
        public static bool Prefix(Creature target, string path)
        {
            if (target == null || target.IsDead || string.IsNullOrEmpty(path))
                return false;
            return true;
        }

        public static Exception? Finalizer(Exception? __exception)
        {
            return null;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Commands.VfxCmd), "PlayOnCreatureCenters", new Type[] { typeof(IEnumerable<Creature>), typeof(string) })]
    public static class Patch_SafeVfxPlayOnCreatureCenters
    {
        public static bool Prefix(IEnumerable<Creature> targets, string path)
        {
            if (targets == null || string.IsNullOrEmpty(path))
                return false;
            return true;
        }

        public static Exception? Finalizer(Exception? __exception)
        {
            return null;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Commands.VfxCmd), "PlayVfx", new Type[] { typeof(Godot.Vector2), typeof(string), typeof(Godot.Control) })]
    public static class Patch_SafePlayVfx
    {
        public static Exception? Finalizer(Exception? __exception)
        {
            return null;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Helpers.HandPosHelper), "GetPosition", new Type[] { typeof(int), typeof(int) })]
    public static class Patch_SafeHandPosHelperGetPosition
    {
        public static void Prefix(ref int handSize, ref int cardIndex)
        {
            if (handSize > 10) handSize = 10;
            if (handSize < 1) handSize = 1;
            if (cardIndex >= handSize) cardIndex = handSize - 1;
            if (cardIndex < 0) cardIndex = 0;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Helpers.HandPosHelper), "GetAngle", new Type[] { typeof(int), typeof(int) })]
    public static class Patch_SafeHandPosHelperGetAngle
    {
        public static void Prefix(ref int handSize, ref int cardIndex)
        {
            if (handSize > 10) handSize = 10;
            if (handSize < 1) handSize = 1;
            if (cardIndex >= handSize) cardIndex = handSize - 1;
            if (cardIndex < 0) cardIndex = 0;
        }
    }

    [HarmonyPatch(typeof(MegaCrit.Sts2.Core.Helpers.HandPosHelper), "GetScale", new Type[] { typeof(int) })]
    public static class Patch_SafeHandPosHelperGetScale
    {
        public static void Prefix(ref int handSize)
        {
            if (handSize > 10) handSize = 10;
            if (handSize < 1) handSize = 1;
        }
    }
}
