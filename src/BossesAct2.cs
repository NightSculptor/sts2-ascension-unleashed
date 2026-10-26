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
    // 4. MIRRORED SOVEREIGN (ACT 2 BOSS) & MIRROR CLONE
    // ==========================================

    public class MirrorReflectionPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;
        public override LocString Title => new LocString("powers", "MIRROR_REFLECTION_POWER.name");
        public override LocString Description => new LocString("powers", "MIRROR_REFLECTION_POWER.description");

        public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target != Owner) return;
            if (dealer == null || dealer == Owner) return;
            if (!props.IsPoweredAttack()) return;
            if (result.TotalDamage <= 0) return;

            Flash();
            VfxCmd.PlayOnCreatureCenter(dealer, "vfx/vfx_attack_blunt");
            await CreatureCmd.Damage(choiceContext, dealer, result.TotalDamage, ValueProp.Unpowered | ValueProp.Unblockable, Owner);
        }
    }

    public class MirrorClone : MonsterModel
    {
        public override int MinInitialHp => 22;
        public override int MaxInitialHp => 26;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");
        public override LocString Title => new LocString("monsters", "MIRROR_CLONE.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        public override async Task AfterAddedToRoom()
        {
            await base.AfterAddedToRoom();
            await PowerCmd.Apply<MirrorReflectionPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateSplinter = new MoveState("CLONE_SPLINTER", MoveSplinter, new SingleAttackIntent(5));
            var stateRefract = new MoveState("CLONE_REFRACT", MoveRefract, new DefendIntent());

            stateSplinter.FollowUpState = stateRefract;
            stateRefract.FollowUpState = stateSplinter;

            return new MonsterMoveStateMachine(new MonsterState[] { stateSplinter, stateRefract }, stateSplinter);
        }

        private async Task MoveSplinter(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -40f, 0.12f);
            await DamageCmd.Attack(5).FromMonster(this)
                .WithAttackerAnim("Attack", 0.12f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.1f);
        }

        private async Task MoveRefract(IReadOnlyList<Creature> targets)
        {
            var sovereign = CombatState?.Enemies.FirstOrDefault(e => e.IsAlive && e.Monster is MirroredSovereign);
            if (sovereign != null)
            {
                await CreatureCmd.GainBlock(sovereign, 12, ValueProp.Move, null);
            }
            else
            {
                await CreatureCmd.GainBlock(Creature, 8, ValueProp.Move, null);
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

    public class PrismaticWardPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override LocString Title => new LocString("powers", "PRISMATIC_WARD_POWER.name");
        public override LocString Description => new LocString("powers", "PRISMATIC_WARD_POWER.description");

        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
        {
            if (target == Owner)
            {
                bool clonesAlive = CombatState?.Enemies.Any(e => e.IsAlive && e.Monster is MirrorClone) ?? false;
                if (clonesAlive)
                {
                    return 0.5m; // 50% damage reduction while mirror clones protect Sovereign!
                }
            }
            return 1m;
        }

        public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target != Owner) return;
            if (dealer == null || dealer == Owner) return;
            if (!props.IsPoweredAttack()) return;
            if (result.TotalDamage <= 0) return;

            // Retaliate with 25% splinter damage
            int reflectDmg = Math.Max(1, (int)(result.TotalDamage * 0.25f));
            VfxCmd.PlayOnCreatureCenter(dealer, "vfx/vfx_attack_blunt");
            await CreatureCmd.Damage(choiceContext, dealer, reflectDmg, ValueProp.Unpowered, Owner);
        }
    }

    public class MirroredSovereign : MonsterModel
    {
        public override int MinInitialHp => 340;
        public override int MaxInitialHp => 380;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");
        public override LocString Title => new LocString("monsters", "MIRRORED_SOVEREIGN.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Magic;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/the_forgotten/the_forgotten_die";

        public override async Task AfterAddedToRoom()
        {
            await base.AfterAddedToRoom();
            await PowerCmd.Apply<PrismaticWardPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateWard = new MoveState("SOVEREIGN_WARD", MoveWard, new DefendIntent(), new StatusIntent(1));
            var stateBeam = new MoveState("SOVEREIGN_BEAM", MoveShatterBeam, new SingleAttackIntent(22));
            var stateSplinter = new MoveState("SOVEREIGN_SPLINTER", MoveSplinter, new MultiAttackIntent(4, 5));
            var stateResonator = new MoveState("SOVEREIGN_RESONATOR", MoveResonator, new DefendIntent(), new BuffIntent());

            stateWard.FollowUpState = stateBeam;
            stateBeam.FollowUpState = stateSplinter;
            stateSplinter.FollowUpState = stateResonator;
            stateResonator.FollowUpState = stateWard;

            return new MonsterMoveStateMachine(new MonsterState[] { stateWard, stateBeam, stateSplinter, stateResonator }, stateWard);
        }

        private async Task MoveWard(IReadOnlyList<Creature> targets)
        {
            bool clonesAlive = CombatState?.Enemies.Any(e => e.IsAlive && e.Monster is MirrorClone) ?? false;
            int blockAmt = clonesAlive ? 25 : 35;
            await CreatureCmd.GainBlock(Creature, blockAmt, ValueProp.Move, null);

            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null && CombatState != null)
            {
                var dazed = new List<CardModel> { CombatState.CreateCard<MegaCrit.Sts2.Core.Models.Cards.Dazed>(player) };
                await CardPileCmd.AddGeneratedCardsToCombat(dazed, PileType.Discard, player);
            }
            await Cmd.Wait(0.2f);
        }

        private async Task MoveShatterBeam(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -70f, 0.16f);
            await DamageCmd.Attack(22).FromMonster(this)
                .WithAttackerAnim("Attack", 0.18f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.2f);
        }

        private async Task MoveSplinter(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -50f, 0.12f);
            for (int i = 0; i < 5; i++)
            {
                await DamageCmd.Attack(4).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.10f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_attack_blunt")
                    .Execute(null);
                await Cmd.Wait(0.08f);
            }
        }

        private async Task MoveResonator(IReadOnlyList<Creature> targets)
        {
            await CreatureCmd.GainBlock(Creature, 18, ValueProp.Move, null);
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 3m, Creature, null);
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

    public class MirroredSovereignBoss : EncounterModel
    {
        public override RoomType RoomType => RoomType.Boss;
        public override string BossNodePath => "res://images/map/placeholder/kaiser_crab_boss_icon";
        public override MegaSkeletonDataResource? BossNodeSpineResource => null;
        public override IEnumerable<MonsterModel> AllPossibleMonsters => new MonsterModel[]
        {
            ModelDb.Monster<MirroredSovereign>(),
            ModelDb.Monster<MirrorClone>()
        };
        public override float GetCameraScaling() => 0.82f;
        public override Vector2 GetCameraOffset() => Vector2.Down * 60f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<MirroredSovereign>().ToMutable(), null),
                (ModelDb.Monster<MirrorClone>().ToMutable(), null),
                (ModelDb.Monster<MirrorClone>().ToMutable(), null)
            };
        }
    }

    // ==========================================
    // 5. ALCHEMICAL CHIMERA (ACT 2 BOSS)
    // ==========================================

    public class ChimeraTransmutationPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override LocString Title => new LocString("powers", "CHIMERA_TRANSMUTATION_POWER.name");
        public override LocString Description => new LocString("powers", "CHIMERA_TRANSMUTATION_POWER.description");

        public int CurrentElement => Amount; // 1: Acid, 2: Flame, 3: Cryo

        public void SetElement(int element)
        {
            SetAmount(element);
            Flash();
        }
    }

    public class AlchemicalChimera : MonsterModel
    {
        public override int MinInitialHp => 360;
        public override int MaxInitialHp => 400;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/the_forgotten");
        public override LocString Title => new LocString("monsters", "ALCHEMICAL_CHIMERA.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Fur;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/axebot/axebot_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/axebot/axebot_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/axebot/axebot_die";

        public override async Task AfterAddedToRoom()
        {
            await base.AfterAddedToRoom();
            await PowerCmd.Apply<ChimeraTransmutationPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateAcidSaliva = new MoveState("CHIMERA_ACID1", MoveAcidSaliva, new SingleAttackIntent(16), new DebuffIntent());
            var stateAcidTorrent = new MoveState("CHIMERA_ACID2", MoveAcidTorrent, new SingleAttackIntent(24));
            var stateMoltenBreath = new MoveState("CHIMERA_FIRE1", MoveMoltenBreath, new SingleAttackIntent(18), new StatusIntent(2));
            var stateIgnitionRush = new MoveState("CHIMERA_FIRE2", MoveIgnitionRush, new MultiAttackIntent(8, 3), new BuffIntent());
            var stateGlacialRoar = new MoveState("CHIMERA_CRYO1", MoveGlacialRoar, new DefendIntent(), new DebuffIntent());
            var stateCryoPounce = new MoveState("CHIMERA_CRYO2", MoveCryoPounce, new SingleAttackIntent(30));

            stateAcidSaliva.FollowUpState = stateAcidTorrent;
            stateAcidTorrent.FollowUpState = stateMoltenBreath;
            stateMoltenBreath.FollowUpState = stateIgnitionRush;
            stateIgnitionRush.FollowUpState = stateGlacialRoar;
            stateGlacialRoar.FollowUpState = stateCryoPounce;
            stateCryoPounce.FollowUpState = stateAcidSaliva;

            return new MonsterMoveStateMachine(new MonsterState[] { stateAcidSaliva, stateAcidTorrent, stateMoltenBreath, stateIgnitionRush, stateGlacialRoar, stateCryoPounce }, stateAcidSaliva);
        }

        private async Task MoveAcidSaliva(IReadOnlyList<Creature> targets)
        {
            var power = Creature.GetPower<ChimeraTransmutationPower>();
            if (power != null) { power.SetElement(1); }

            await ModVfx.AttackLunge(Creature, -60f, 0.14f);
            await DamageCmd.Attack(16).FromMonster(this)
                .WithAttackerAnim("Attack", 0.15f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);

            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null)
            {
                await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), player.Creature, 2m, Creature, null);
            }
            await Cmd.Wait(0.2f);
        }

        private async Task MoveAcidTorrent(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -70f, 0.16f);
            
            // Erode half of player's block before attack
            var target = (targets != null && targets.Count > 0) ? targets[0] : CombatState?.Players.FirstOrDefault()?.Creature;
            if (target != null && target.Block > 0)
            {
                await CreatureCmd.LoseBlock(new ThrowingPlayerChoiceContext(), target, target.Block / 2, Creature);
            }

            await DamageCmd.Attack(24).FromMonster(this)
                .WithAttackerAnim("Attack", 0.18f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.2f);
        }

        private async Task MoveMoltenBreath(IReadOnlyList<Creature> targets)
        {
            var power = Creature.GetPower<ChimeraTransmutationPower>();
            if (power != null) { power.SetElement(2); }

            await ModVfx.AttackLunge(Creature, -60f, 0.14f);
            await DamageCmd.Attack(18).FromMonster(this)
                .WithAttackerAnim("Attack", 0.15f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);

            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null && CombatState != null)
            {
                var burns = new List<CardModel>
                {
                    CombatState.CreateCard<MegaCrit.Sts2.Core.Models.Cards.Burn>(player),
                    CombatState.CreateCard<MegaCrit.Sts2.Core.Models.Cards.Burn>(player)
                };
                await CardPileCmd.AddGeneratedCardsToCombat(burns, PileType.Discard, player);
            }
            await Cmd.Wait(0.2f);
        }

        private async Task MoveIgnitionRush(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -50f, 0.12f);
            for (int i = 0; i < 3; i++)
            {
                await DamageCmd.Attack(8).FromMonster(this)
                    .WithAttackerAnim("Attack", 0.12f)
                    .OnlyPlayAnimOnce()
                    .WithAttackerFx(null, AttackSfx)
                    .WithHitFx("vfx/vfx_attack_blunt")
                    .Execute(null);
                await Cmd.Wait(0.1f);
            }

            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 2m, Creature, null);
            await Cmd.Wait(0.15f);
        }

        private async Task MoveGlacialRoar(IReadOnlyList<Creature> targets)
        {
            var power = Creature.GetPower<ChimeraTransmutationPower>();
            if (power != null) { power.SetElement(3); }

            await CreatureCmd.GainBlock(Creature, 40, ValueProp.Move, null);
            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null)
            {
                await PowerCmd.Apply<WeakPower>(new ThrowingPlayerChoiceContext(), player.Creature, 2m, Creature, null);
            }
            await Cmd.Wait(0.2f);
        }

        private async Task MoveCryoPounce(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -80f, 0.18f);
            await DamageCmd.Attack(30).FromMonster(this)
                .WithAttackerAnim("Attack", 0.20f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
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

    public class AlchemicalChimeraBoss : EncounterModel
    {
        public override RoomType RoomType => RoomType.Boss;
        public override string BossNodePath => "res://images/map/placeholder/knowledge_demon_boss_icon";
        public override MegaSkeletonDataResource? BossNodeSpineResource => null;
        public override IEnumerable<MonsterModel> AllPossibleMonsters => new[] { ModelDb.Monster<AlchemicalChimera>() };
        public override float GetCameraScaling() => 0.82f;
        public override Vector2 GetCameraOffset() => Vector2.Down * 60f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<AlchemicalChimera>().ToMutable(), null)
            };
        }
    }

    // ==========================================
    // 6. NULL EXECUTIONER (ACT 2 BOSS)
    // ==========================================

    public class NullEdictPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;
        public override LocString Title => new LocString("powers", "NULL_EDICT_POWER.name");
        public override LocString Description => Amount == 1 
            ? new LocString("powers", "NULL_EDICT_POWER_ATK.description") 
            : new LocString("powers", "NULL_EDICT_POWER_SKILL.description");

        public CardType ForbiddenType => Amount == 1 ? CardType.Attack : CardType.Skill;

        public void SetForbidden(CardType cardType)
        {
            SetAmount(cardType == CardType.Attack ? 1 : 2);
            Flash();
        }

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var card = cardPlay.Card;
            if (card != null && card.Owner != null && card.Type == ForbiddenType && Owner != null && Owner.IsAlive)
            {
                Flash();
                VfxCmd.PlayOnCreatureCenter(card.Owner.Creature, "vfx/vfx_attack_blunt");
                await CreatureCmd.Damage(choiceContext, card.Owner.Creature, 6m, ValueProp.Unblockable | ValueProp.Unpowered, Owner);
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, 1m, Owner, null);
            }
        }
    }

    public class NullExecutioner : MonsterModel
    {
        public override int MinInitialHp => 380;
        public override int MaxInitialHp => 420;

        protected override string VisualsPath => SceneHelper.GetScenePath("creature_visuals/spectral_knight");
        public override LocString Title => new LocString("monsters", "NULL_EXECUTIONER.name");

        public override DamageSfxType TakeDamageSfxType => DamageSfxType.Armor;
        protected override string AttackSfx => "event:/sfx/enemy/enemy_attacks/axebot/axebot_attack";
        protected override string CastSfx => "event:/sfx/enemy/enemy_attacks/axebot/axebot_cast";
        public override string DeathSfx => "event:/sfx/enemy/enemy_attacks/axebot/axebot_die";

        public override async Task AfterAddedToRoom()
        {
            await base.AfterAddedToRoom();
            await PowerCmd.Apply<NullEdictPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
        }

        protected override MonsterMoveStateMachine GenerateMoveStateMachine()
        {
            var stateEdictAttacks = new MoveState("EXEC_EDICT_ATK", MoveEdictAttacks, new DefendIntent(), new BuffIntent());
            var stateCleave = new MoveState("EXEC_CLEAVE", MoveCleave, new SingleAttackIntent(26));
            var stateEdictSkills = new MoveState("EXEC_EDICT_SKILL", MoveEdictSkills, new DebuffIntent(), new BuffIntent());
            var stateDecapitate = new MoveState("EXEC_DECAPITATE", MoveDecapitate, new SingleAttackIntent(36));

            stateEdictAttacks.FollowUpState = stateCleave;
            stateCleave.FollowUpState = stateEdictSkills;
            stateEdictSkills.FollowUpState = stateDecapitate;
            stateDecapitate.FollowUpState = stateEdictAttacks;

            return new MonsterMoveStateMachine(new MonsterState[] { stateEdictAttacks, stateCleave, stateEdictSkills, stateDecapitate }, stateEdictAttacks);
        }

        private async Task MoveEdictAttacks(IReadOnlyList<Creature> targets)
        {
            var power = Creature.GetPower<NullEdictPower>();
            if (power != null) { power.SetForbidden(CardType.Attack); }
            await CreatureCmd.GainBlock(Creature, 25, ValueProp.Move, null);
            await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, Creature, null);
            await Cmd.Wait(0.2f);
        }

        private async Task MoveCleave(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -80f, 0.15f);
            await DamageCmd.Attack(26).FromMonster(this)
                .WithAttackerAnim("Attack", 0.18f)
                .OnlyPlayAnimOnce()
                .WithAttackerFx(null, AttackSfx)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(null);
            await Cmd.Wait(0.2f);
        }

        private async Task MoveEdictSkills(IReadOnlyList<Creature> targets)
        {
            var power = Creature.GetPower<NullEdictPower>();
            if (power != null) { power.SetForbidden(CardType.Skill); }
            var player = (targets != null && targets.Count > 0) ? targets[0].Player : CombatState?.Players.FirstOrDefault();
            if (player != null)
            {
                await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), player.Creature, 2m, Creature, null);
                await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), player.Creature, 2m, Creature, null);
            }
            await Cmd.Wait(0.2f);
        }

        private async Task MoveDecapitate(IReadOnlyList<Creature> targets)
        {
            await ModVfx.AttackLunge(Creature, -100f, 0.18f);
            await DamageCmd.Attack(36).FromMonster(this)
                .WithAttackerAnim("Attack", 0.22f)
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
                return ModelDb.Monster<MegaCrit.Sts2.Core.Models.Monsters.SpectralKnight>().GenerateAnimator(controller);
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

    public class NullExecutionerBoss : EncounterModel
    {
        public override RoomType RoomType => RoomType.Boss;
        public override string BossNodePath => "res://images/map/placeholder/soul_fysh_boss_icon";
        public override MegaSkeletonDataResource? BossNodeSpineResource => null;
        public override IEnumerable<MonsterModel> AllPossibleMonsters => new[] { ModelDb.Monster<NullExecutioner>() };
        public override float GetCameraScaling() => 0.82f;
        public override Vector2 GetCameraOffset() => Vector2.Down * 60f;

        protected override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
        {
            return new List<(MonsterModel, string?)>
            {
                (ModelDb.Monster<NullExecutioner>().ToMutable(), null)
            };
        }
    }
}
