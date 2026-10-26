#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;

namespace AnimeSurvivalMod
{
    // ==========================================
    // 1. Blood Retribution / Кровавое возмездие
    // ==========================================
    public sealed class BloodRetribution : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(1m, ValueProp.Move)
        };

        public BloodRetribution()
            : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            int missingHp = Math.Max(0, Owner.Creature.MaxHp - Owner.Creature.CurrentHp);
            decimal damage = missingHp * DynamicVars.Damage.BaseValue;

            await DamageCmd.Attack(damage)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(1m);
        }
    }

    // ==========================================
    // 2. Deep Concentration / Глубокая концентрация
    // ==========================================
    public sealed class DeepConcentration : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new PowerVar<VigorPower>("Vigor", 20m)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
        {
            HoverTipFactory.FromPower<VigorPower>()
        };

        public DeepConcentration()
            : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            decimal vigorAmt = DynamicVars["Vigor"].BaseValue;
            await PowerCmd.Apply<DeepConcentrationPower>(choiceContext, Owner.Creature, vigorAmt, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
            DynamicVars["Vigor"].UpgradeValueBy(10m);
        }
    }

    public sealed class DeepConcentrationPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        private int _cardsPlayedThisTurn = 0;

        public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner == Owner?.Player)
            {
                _cardsPlayedThisTurn++;
            }
            return Task.CompletedTask;
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == Owner?.Player)
            {
                if (_cardsPlayedThisTurn == 0)
                {
                    Flash();
                    await PowerCmd.Apply<VigorPower>(choiceContext, Owner, Amount, Owner, null);
                }
                _cardsPlayedThisTurn = 0;
            }
        }
    }

    // ==========================================
    // 3. Egoist / Эгоист
    // ==========================================
    public sealed class Egoist : CardModel
    {
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new CardsVar(1)
        };

        public Egoist()
            : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<EgoistBuffPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);

            foreach (var teammate in CombatState.GetTeammatesOf(Owner.Creature))
            {
                if (teammate != null && teammate != Owner.Creature && teammate.IsAlive && teammate.IsPlayer)
                {
                    await PowerCmd.Apply<EgoistDebuffPower>(choiceContext, teammate, 1m, Owner.Creature, this);
                }
            }

            await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.BaseValue, Owner);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }

    public sealed class EgoistBuffPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
        {
            if (dealer == Owner)
            {
                return 2m;
            }
            return 1m;
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == Owner?.Player)
            {
                await PowerCmd.Remove(this);
            }
        }
    }

    public sealed class EgoistDebuffPower : PowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
        {
            if (target == Owner)
            {
                return 2m;
            }
            return 1m;
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
    // 4. Sacrificial Cleave / Жертвенный рассекатель
    // ==========================================
    public sealed class SacrificialCleave : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(14m, ValueProp.Move),
            new HpLossVar(4),
            new HealVar(1m)
        };

        public SacrificialCleave()
            : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.Damage(choiceContext, Owner.Creature, DynamicVars.HpLoss.BaseValue, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);

            var cmd = await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .TargetingAllOpponents(CombatState!)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

            int damagedEnemies = cmd.Results.SelectMany(r => r).Count(dr => dr.UnblockedDamage > 0);
            if (damagedEnemies > 0)
            {
                await CreatureCmd.Heal(Owner.Creature, damagedEnemies * DynamicVars.Heal.BaseValue);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(4m);
            DynamicVars.Heal.UpgradeValueBy(1m);
        }
    }

    // ==========================================
    // 5. Titanic Bastion / Титанический бастион
    // ==========================================
    public sealed class TitanicBastion : CardModel
    {
        public override bool GainsBlock => true;

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new BlockVar(16m, ValueProp.Move),
            new PowerVar<StrengthPower>("Strength", 2m)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
        {
            HoverTipFactory.FromPower<StrengthPower>()
        };

        public TitanicBastion()
            : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
            decimal str = DynamicVars["Strength"].BaseValue;
            await PowerCmd.Apply<TitanicBastionPower>(choiceContext, Owner.Creature, str, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Block.UpgradeValueBy(6m);
            DynamicVars["Strength"].UpgradeValueBy(1m);
        }
    }

    public sealed class TitanicBastionPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == Owner?.Player && Owner != null)
            {
                if (Owner.Block > 0)
                {
                    Flash();
                    await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
                }
                await PowerCmd.Remove(this);
            }
        }
    }

    // ==========================================
    // 6. Iron Loan / Железный заём
    // ==========================================
    public sealed class IronLoan : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("EnergyGain", 2m)
        };

        public IronLoan()
            : base(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int energyGain = (int)DynamicVars["EnergyGain"].BaseValue;
            await PlayerCmd.GainEnergy(energyGain, Owner);
            await PowerCmd.Apply<IronLoanPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["EnergyGain"].UpgradeValueBy(1m);
        }
    }

    public sealed class IronLoanPower : PowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side == CombatSide.Player && Owner?.Player != null)
            {
                int unspent = Owner.Player.PlayerCombatState.Energy;
                if (unspent > 0)
                {
                    Flash();
                    int dmg = unspent * 5;
                    await CreatureCmd.Damage(choiceContext, Owner, dmg, ValueProp.Unblockable | ValueProp.Unpowered, Owner);
                }
            }
        }

        public override decimal ModifyHandDraw(Player player, decimal count)
        {
            if (player == Owner?.Player)
            {
                return Math.Max(0m, count - 1m);
            }
            return count;
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == Owner?.Player && Owner != null)
            {
                Flash();
                await PowerCmd.Remove(this);
            }
        }
    }

    // ==========================================
    // 7. Martyr's Brand / Клеймо мученика
    // ==========================================
    public sealed class MartyrsBrand : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("Turns", 2m)
        };

        public MartyrsBrand()
            : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            decimal turns = DynamicVars["Turns"].BaseValue;
            await PowerCmd.Apply<MartyrsBrandPower>(choiceContext, Owner.Creature, turns, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["Turns"].UpgradeValueBy(1m);
        }
    }

    public sealed class MartyrsBrandPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target == Owner && dealer != null && dealer != Owner && props.IsPoweredAttack() && result.TotalDamage > 0)
            {
                Flash();
                await CreatureCmd.Damage(choiceContext, dealer, result.TotalDamage, ValueProp.Unblockable | ValueProp.Unpowered, Owner);
            }
        }

        public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side == CombatSide.Player)
            {
                SetAmount(Amount - 1);
                if (Amount <= 0)
                {
                    await PowerCmd.Remove(this);
                }
            }
        }
    }

    // ==========================================
    // 8. Ashen Requiem / Пепельный реквием
    // ==========================================
    public sealed class AshenRequiem : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(4m, ValueProp.Move)
        };

        public AshenRequiem()
            : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            int exhaustCount = Owner.PlayerCombatState.ExhaustPile?.Cards.Count ?? 0;
            decimal totalDmg = exhaustCount * DynamicVars.Damage.BaseValue;

            await DamageCmd.Attack(totalDmg)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_big_slash")
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(1m);
        }
    }

    // ==========================================
    // 9. Siege Stance / Осадная стойка
    // ==========================================
    public sealed class SiegeStance : CardModel
    {
        public SiegeStance()
            : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<SiegeStancePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public sealed class SiegeStancePower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel cardSource, CardPlay cardPlay)
        {
            if (target == Owner)
            {
                return 2m; // Doubled block!
            }
            return 1m;
        }

        public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side == CombatSide.Player && Owner != null && Owner.Block > 0 && CombatState != null)
            {
                Flash();
                decimal dmg = Math.Floor(Owner.Block * 0.5m);
                if (dmg > 0)
                {
                    foreach (var e in CombatState.Enemies.Where(e => e.IsAlive).ToList())
                    {
                        await CreatureCmd.Damage(choiceContext, e, dmg, ValueProp.Unpowered | ValueProp.Move, Owner);
                    }
                }
            }
        }
    }

    [HarmonyPatch(typeof(CardModel), "get_IsPlayable")]
    public static class SiegeStancePlayablePatch
    {
        public static void Postfix(CardModel __instance, ref bool __result)
        {
            if (__result && __instance.Type == CardType.Attack && __instance.Owner?.Creature?.HasPower<SiegeStancePower>() == true)
            {
                __result = false;
            }
        }
    }

    // ==========================================
    // 10. Blood Parity / Кровавый паритет
    // ==========================================
    public sealed class BloodParity : CardModel
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("MaxCap", 40m),
            new DynamicVar("Strength", 3m)
        };

        public BloodParity()
            : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            int ownerHp = Owner.Creature.CurrentHp;
            int targetHp = cardPlay.Target.CurrentHp;
            int maxCap = (int)DynamicVars["MaxCap"].BaseValue;

            if (targetHp > ownerHp)
            {
                int diff = Math.Min(maxCap, targetHp - ownerHp);
                await CreatureCmd.Damage(choiceContext, cardPlay.Target, diff, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);
            }
            else if (ownerHp > targetHp)
            {
                int diff = Math.Min(maxCap, ownerHp - targetHp);
                await CreatureCmd.Damage(choiceContext, Owner.Creature, diff, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);
                decimal str = DynamicVars["Strength"].BaseValue;
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, str, Owner.Creature, this);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars["MaxCap"].UpgradeValueBy(10m);
            DynamicVars["Strength"].UpgradeValueBy(1m);
        }
    }

    // ==========================================
    // 11. Red-Hot Anvil / Раскалённая наковальня
    // ==========================================
    public sealed class RedHotAnvil : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(9m, ValueProp.Move)
        };

        public RedHotAnvil()
            : base(1, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            decimal dmg = DynamicVars.Damage.BaseValue;
            await DamageCmd.Attack(dmg)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(choiceContext);

            var upgradable = Owner.PlayerCombatState.Hand.Cards.Where(c => c != this && c.IsUpgradable).ToList();
            if (upgradable.Count > 0)
            {
                var rng = new Random();
                var cardToUpgrade = upgradable[rng.Next(upgradable.Count)];
                try
                {
                    CardCmd.Upgrade(cardToUpgrade, MegaCrit.Sts2.Core.Nodes.CommonUi.CardPreviewStyle.None);
                }
                catch
                {
                    cardToUpgrade.UpgradeInternal();
                    cardToUpgrade.FinalizeUpgradeInternal();
                }
            }
            else if (cardPlay.Target.IsAlive)
            {
                await DamageCmd.Attack(dmg)
                    .FromCard(this, cardPlay)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_attack_blunt")
                    .Execute(choiceContext);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
        }
    }

    // ==========================================
    // 12. Doom Toll / Колокол погибели
    // ==========================================
    public sealed class DoomToll : CardModel
    {
        protected override bool HasEnergyCostX => true;

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(8m, ValueProp.Move)
        };

        public DoomToll()
            : base(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int x = ResolveEnergyXValue();
            var otherCards = Owner.PlayerCombatState.Hand.Cards.Where(c => c != this).Take(x).ToList();
            foreach (var c in otherCards)
            {
                await CardCmd.Exhaust(choiceContext, c, causedByEthereal: false, skipVisuals: true);
            }

            int actualExhausted = otherCards.Count;
            if (actualExhausted > 0 && CombatState != null)
            {
                decimal dmgPer = DynamicVars.Damage.BaseValue;
                await DamageCmd.Attack(dmgPer)
                    .FromCard(this, cardPlay)
                    .WithHitCount(actualExhausted)
                    .TargetingAllOpponents(CombatState)
                    .WithHitFx("vfx/vfx_big_slash")
                    .Execute(choiceContext);

                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, (decimal)actualExhausted, Owner.Creature, this);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
        }
    }

    // ==========================================
    // 13. Second Heart / Второе сердце
    // ==========================================
    public sealed class SecondHeart : CardModel
    {
        public SecondHeart()
            : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<SecondHeartPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public sealed class SecondHeartPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override bool ShouldDie(Creature creature)
        {
            if (creature == Owner) return false;
            return base.ShouldDie(creature);
        }

        public override async Task AfterPreventingDeath(Creature creature)
        {
            if (creature == Owner && Owner != null)
            {
                Flash();
                int healAmt = Math.Max(1, (int)(Owner.MaxHp * 0.25f));
                await CreatureCmd.Heal(Owner, healAmt);
                await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Owner, 4m, Owner, null);
                await CreatureCmd.GainBlock(Owner, 20m, ValueProp.Move, null);
                await PowerCmd.Remove(this);
            }
        }
    }

    // ==========================================
    // 14. Unstoppable Rampage / Неудержимый прорыв
    // ==========================================
    public sealed class UnstoppableRampage : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(10m, ValueProp.Move)
        };

        public UnstoppableRampage()
            : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            int targetOldBlock = cardPlay.Target.Block;
            decimal dmg = DynamicVars.Damage.BaseValue;

            await DamageCmd.Attack(dmg)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

            if (CombatState != null && Owner != null && (!cardPlay.Target.IsAlive || (targetOldBlock > 0 && cardPlay.Target.Block == 0)))
            {
                var copy = CombatState.CreateCard<UnstoppableRampage>(Owner);
                if (IsUpgraded)
                {
                    CardCmd.Upgrade(copy);
                }
                copy.EnergyCost.SetThisTurn(0, reduceOnly: true);
                await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, Owner);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
        }
    }

    // ==========================================
    // 15. Combustion Pact / Пепельное пламя
    // ==========================================
    public sealed class CombustionPact : CardModel
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(14m, ValueProp.Move)
        };

        public CombustionPact()
            : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var drawCards = Owner.PlayerCombatState.DrawPile.Cards.ToList();
            foreach (var c in drawCards)
            {
                await CardCmd.Exhaust(choiceContext, c, causedByEthereal: false, skipVisuals: true);
            }

            int bundles = drawCards.Count / 3;
            if (bundles > 0)
            {
                decimal dmg = DynamicVars.Damage.BaseValue * bundles;
                await DamageCmd.Attack(dmg)
                    .FromCard(this, cardPlay)
                    .TargetingAllOpponents(CombatState!)
                    .WithHitFx("vfx/vfx_fire_burst")
                    .Execute(choiceContext);
                await PlayerCmd.GainEnergy(bundles, Owner);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(4m);
        }
    }
}
