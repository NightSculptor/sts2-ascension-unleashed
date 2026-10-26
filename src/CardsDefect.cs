#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace AnimeSurvivalMod
{
    // ==========================================
    // 1. Overclock Focus / Разгон фокуса
    // ==========================================
    public sealed class OverclockFocus : CardModel
    {
        protected override bool HasEnergyCostX => true;

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
        {
            HoverTipFactory.FromPower<FocusPower>()
        };

        public OverclockFocus()
            : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int x = ResolveEnergyXValue();
            if (x > 0)
            {
                await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
                await PowerCmd.Apply<FocusPower>(choiceContext, Owner.Creature, x, Owner.Creature, this);
            }
        }

        protected override void OnUpgrade()
        {
            RemoveKeyword(CardKeyword.Exhaust);
        }
    }

    // ==========================================
    // 2. Plasma Beam / Плазменный луч
    // ==========================================
    public sealed class PlasmaBeam : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(30m, ValueProp.Move)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
        {
            HoverTipFactory.FromOrb<PlasmaOrb>()
        };

        public PlasmaBeam()
            : base(3, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var plasma = Owner.PlayerCombatState.OrbQueue.Orbs.OfType<PlasmaOrb>().FirstOrDefault();
            if (plasma == null)
            {
                return;
            }

            Owner.PlayerCombatState.OrbQueue.Remove(plasma);
            NCombatRoom.Instance?.GetCreatureNode(Owner.Creature)?.OrbManager?.EvokeOrbAnim(plasma);
            plasma.RemoveInternal();

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .TargetingAllOpponents(CombatState!)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(10m);
        }
    }

    // ==========================================
    // 3. Short Circuit / Короткое замыкание
    // ==========================================
    public sealed class ShortCircuit : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(9m, ValueProp.Move)
        };

        public ShortCircuit()
            : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

            if (Owner.PlayerCombatState.OrbQueue.Orbs.Count > 0)
            {
                await OrbCmd.EvokeNext(choiceContext, Owner, dequeue: false);
                await OrbCmd.EvokeNext(choiceContext, Owner, dequeue: true);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
        }
    }

    // ==========================================
    // 4. Nuclear Reactor / Ядерная энергетика
    // ==========================================
    public sealed class NuclearReactor : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new EnergyVar(1)
        };

        public NuclearReactor()
            : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<NuclearReactorPower>(choiceContext, Owner.Creature, DynamicVars.Energy.BaseValue, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public sealed class NuclearReactorPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
        {
            if (card.Type == CardType.Status && (creator == Owner?.Player || (creator == null && card.Owner == Owner?.Player)))
            {
                Flash();
                if (Owner?.Player != null)
                {
                    await PlayerCmd.GainEnergy(1, Owner.Player);
                }
            }
        }
    }

    // ==========================================
    // 5. Matrix Capacitor / Матричный конденсатор
    // ==========================================
    public sealed class MatrixCapacitor : CardModel
    {
        public override bool GainsBlock => true;

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new BlockVar(5m, ValueProp.Move),
            new DynamicVar("EvokeBlock", 4m)
        };

        public MatrixCapacitor()
            : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
            decimal blockPerEvoke = DynamicVars["EvokeBlock"].BaseValue;
            await PowerCmd.Apply<MatrixCapacitorPower>(choiceContext, Owner.Creature, blockPerEvoke, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Block.UpgradeValueBy(3m);
            DynamicVars["EvokeBlock"].UpgradeValueBy(2m);
        }
    }

    public sealed class MatrixCapacitorPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
        {
            if (orb.Owner == Owner?.Player && Owner != null)
            {
                Flash();
                await CreatureCmd.GainBlock(Owner, Amount, ValueProp.Move, null);
            }
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == Owner?.Player)
            {
                await PowerCmd.Remove(this);
            }
        }
    }

    // ==========================================
    // 6. Core Meltdown / Расплавление ядра
    // ==========================================
    public sealed class CoreMeltdown : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(26m, ValueProp.Move)
        };

        public CoreMeltdown()
            : base(2, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

            if (Owner?.PlayerCombatState?.OrbQueue != null && Owner.PlayerCombatState.OrbQueue.Capacity > 0)
            {
                OrbCmd.RemoveSlots(Owner, 1);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(8m);
        }
    }

    // ==========================================
    // 7. Superconductor / Сверхпроводник
    // ==========================================
    public sealed class Superconductor : CardModel
    {
        public Superconductor()
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<SuperconductorPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public sealed class SuperconductorPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override int ModifyOrbPassiveTriggerCounts(OrbModel orb, int triggerCount)
        {
            if (orb.Owner == Owner?.Player)
            {
                return triggerCount + 1;
            }
            return triggerCount;
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == Owner?.Player)
            {
                await PowerCmd.Remove(this);
            }
        }
    }

    // ==========================================
    // 8. Quantum Entanglement / Квантовая запутанность
    // ==========================================
    public sealed class QuantumEntanglement : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("Turns", 2m)
        };

        public QuantumEntanglement()
            : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
            await PowerCmd.Apply<QuantumEntanglementPower>(choiceContext, cardPlay.Target, DynamicVars["Turns"].BaseValue, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["Turns"].UpgradeValueBy(1m);
        }
    }

    public sealed class QuantumEntanglementPower : PowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
        {
            if (target != Owner && dealer != null && dealer.IsPlayer && result.TotalDamage > 0 && Owner != null && Owner.IsAlive)
            {
                Flash();
                await CreatureCmd.Damage(choiceContext, Owner, result.TotalDamage, ValueProp.Unpowered, dealer);
            }
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            await PowerCmd.Decrement(this);
        }
    }

    // ==========================================
    // 9. Plasma Overdrive / Плазменный перегруз
    // ==========================================
    public sealed class PlasmaOverdrive : CardModel
    {
        public PlasmaOverdrive()
            : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<PlasmaOverdrivePower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public sealed class PlasmaOverdrivePower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
        {
            if (orb is PlasmaOrb && orb.Owner == Owner?.Player && Owner?.Player != null)
            {
                Flash();
                await PlayerCmd.GainEnergy(1, Owner.Player);
                await CreatureCmd.Damage(choiceContext, Owner, 3, ValueProp.Unpowered, Owner);
            }
        }
    }

    // ==========================================
    // 10. Recursive Compile / Рекурсивная компиляция
    // ==========================================
    public sealed class RecursiveCompile : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("Draw", 2m)
        };

        public RecursiveCompile()
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner?.PlayerCombatState != null)
            {
                var zeros = Owner.PlayerCombatState.DiscardPile.Cards.Where(c => c.EnergyCost.Canonical == 0).ToList();
                if (zeros.Count > 0)
                {
                    await CardPileCmd.Add(zeros, PileType.Draw, CardPilePosition.Random, this, false);
                }
                await CardPileCmd.Draw(choiceContext, DynamicVars["Draw"].BaseValue, Owner, false);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars["Draw"].UpgradeValueBy(1m);
        }
    }

    // ==========================================
    // 11. Absolute Zero / Абсолютный ноль
    // ==========================================
    public sealed class AbsoluteZero : CardModel
    {
        public AbsoluteZero()
            : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<AbsoluteZeroPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public sealed class AbsoluteZeroPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side == CombatSide.Player && Owner?.CombatState != null && Owner.Player != null)
            {
                int frostCount = Owner.Player.PlayerCombatState.OrbQueue.Orbs.OfType<FrostOrb>().Count();
                if (frostCount > 0)
                {
                    Flash();
                    foreach (var enemy in Owner.CombatState.Enemies.Where(e => e.IsAlive).ToList())
                    {
                        await PowerCmd.Apply<WeakPower>(choiceContext, enemy, frostCount, Owner, null);
                    }
                }
            }
        }

        public override async Task AfterOrbEvoked(PlayerChoiceContext choiceContext, OrbModel orb, IEnumerable<Creature> targets)
        {
            if (orb is FrostOrb && orb.Owner == Owner?.Player && Owner != null)
            {
                Flash();
                await CreatureCmd.GainBlock(Owner, 12, ValueProp.Move, null);
            }
        }
    }

    // ==========================================
    // 12. Bios Patch / Заплатка BIOS
    // ==========================================
    public sealed class BiosPatch : CardModel
    {
        public override bool GainsBlock => true;
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new BlockVar(6m, ValueProp.Move),
            new HealVar(4m)
        };

        public BiosPatch()
            : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
            await CreatureCmd.Heal(Owner.Creature, DynamicVars.Heal.BaseValue);

            var debuffs = Owner.Creature.Powers.Where(p => p is VulnerablePower || p is FrailPower).ToList();
            foreach (var p in debuffs)
            {
                await PowerCmd.Remove(p);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Block.UpgradeValueBy(3m);
            DynamicVars.Heal.UpgradeValueBy(3m);
        }
    }

    // ==========================================
    // 13. Impulse Capacitor / Импульсный конденсатор
    // ==========================================
    public sealed class ImpulseCapacitor : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(8m, ValueProp.Move)
        };

        public ImpulseCapacitor()
            : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

            if (Owner?.PlayerCombatState?.OrbQueue != null && Owner.PlayerCombatState.OrbQueue.Orbs.Count >= 3)
            {
                await OrbCmd.EvokeNext(choiceContext, Owner, dequeue: false);
                await OrbCmd.EvokeNext(choiceContext, Owner, dequeue: true);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
        }
    }

    // ==========================================
    // 14. Memory Leak / Утечка памяти
    // ==========================================
    public sealed class MemoryLeak : CardModel
    {
        public override bool GainsBlock => true;
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain, CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new BlockVar(10m, ValueProp.Move)
        };

        public MemoryLeak()
            : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == Owner && Owner?.PlayerCombatState?.Hand != null && Owner.PlayerCombatState.Hand.Cards.Contains(this))
            {
                await CardPileCmd.Draw(choiceContext, 1, Owner, false);
                var otherCards = Owner.PlayerCombatState.Hand.Cards.Where(c => c != this).ToList();
                if (otherCards.Count > 0)
                {
                    var card = otherCards[new Random().Next(otherCards.Count)];
                    card.EnergyCost.AddUntilPlayed(1);
                }
            }
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Block.UpgradeValueBy(4m);
        }
    }
}

