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
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.ValueProps;

namespace AnimeSurvivalMod
{
    // ==========================================
    // 1. Skeleton Army / Армия скелетов
    // ==========================================
    public sealed class SkeletonArmy : CardModel
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("SkeletonCount", 2m),
            new DynamicVar("DmgPercent", 50m)
        };

        public SkeletonArmy()
            : base(5, CardType.Skill, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (!Owner.IsOstyAlive)
            {
                await OstyCmd.Summon(choiceContext, Owner, 1m, this);
            }

            await PowerCmd.Apply<SkeletonArmyPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public sealed class SkeletonArmyPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        private bool _isDuplicating = false;

        public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
        {
            if (dealer != null && dealer == Owner?.Player?.Osty && result.TotalDamage > 0 && target.IsAlive && !_isDuplicating)
            {
                _isDuplicating = true;
                try
                {
                    Flash();
                    decimal halfDamage = Math.Max(1m, Math.Floor(result.TotalDamage / 2m));
                    for (int i = 0; i < 2; i++)
                    {
                        if (target.IsAlive)
                        {
                            await CreatureCmd.Damage(choiceContext, target, halfDamage, ValueProp.Unpowered, dealer);
                        }
                    }
                }
                finally
                {
                    _isDuplicating = false;
                }
            }
        }

        public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            if (creature == Owner?.Player?.Osty)
            {
                await PowerCmd.Remove(this);
            }
        }
    }

    // ==========================================
    // 2. Soul Harvest / Жатва душ
    // ==========================================
    public sealed class SoulHarvest : CardModel
    {
        protected override bool HasEnergyCostX => true;

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(5m, ValueProp.Move)
        };

        public SoulHarvest()
            : base(0, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            int x = ResolveEnergyXValue();
            decimal totalDmg = DynamicVars.Damage.BaseValue * x;

            var cmd = await DamageCmd.Attack(totalDmg)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

            int hpDamage = cmd.Results.SelectMany(r => r).Sum(dr => dr.UnblockedDamage);
            if (hpDamage > 0)
            {
                await OstyCmd.Summon(choiceContext, Owner, hpDamage, this);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(2m);
        }
    }

    // ==========================================
    // 3. Doom Shroud / Саван рока
    // ==========================================
    public sealed class DoomShroud : CardModel
    {
        public override bool GainsBlock => true;

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
        {
            HoverTipFactory.FromPower<DoomPower>()
        };

        public DoomShroud()
            : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            decimal doomAmount = cardPlay.Target.GetPowerAmount<DoomPower>();
            if (doomAmount > 0)
            {
                await CreatureCmd.GainBlock(Owner.Creature, doomAmount, ValueProp.Move, cardPlay);
            }
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    // ==========================================
    // 4. Bone Purge / Очищение костей
    // ==========================================
    public sealed class BonePurge : CardModel
    {
        public BonePurge()
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner.IsOstyAlive && Owner.Osty != null)
            {
                await CreatureCmd.Kill(Owner.Osty);
            }

            var debuffs = Owner.Creature.Powers.Where(p => p.Type == PowerType.Debuff).ToList();
            foreach (var d in debuffs)
            {
                await PowerCmd.Remove(d);
            }
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    // ==========================================
    // 5. Shift Burden / Перекладывание ноши
    // ==========================================
    public sealed class ShiftBurden : CardModel
    {
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

        public override TargetType TargetType => (CombatState != null && CombatState.PlayerCreatures.Count(c => c.IsAlive) > 1) 
            ? TargetType.AnyAlly 
            : TargetType.Self;

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        public ShiftBurden()
            : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
        {
        }

        protected override bool IsPlayable
        {
            get
            {
                if (!base.IsPlayable) return false;
                return Owner?.Creature?.Powers.Any(p => p.Type == PowerType.Debuff) == true;
            }
        }

        public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
        {
            if (card == this && Owner?.Creature?.Powers.Any(p => p.Type == PowerType.Debuff) != true)
            {
                return false;
            }
            return base.ShouldPlay(card, autoPlayType);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            Creature? ally = (cardPlay.Target != null && cardPlay.Target != Owner.Creature) ? cardPlay.Target : null;
            if (ally == null)
            {
                ally = CombatState.PlayerCreatures.FirstOrDefault(c => c != Owner.Creature && c.IsAlive)
                    ?? (Owner.IsOstyAlive ? Owner.Osty : Owner.Creature);
            }

            var debuffs = Owner.Creature.Powers.Where(p => p.Type == PowerType.Debuff).ToList();
            if (debuffs.Count == 0)
            {
                return;
            }

            foreach (var d in debuffs)
            {
                decimal amt = d.Amount;
                await PowerCmd.Remove(d);
                if (ally != Owner.Creature && ally != null)
                {
                    try
                    {
                        var canonical = ModelDb.GetById<PowerModel>(d.Id);
                        var newPower = (PowerModel)canonical.MutableClone();
                        await PowerCmd.Apply(choiceContext, newPower, ally, amt, Owner.Creature, this);
                    }
                    catch (Exception ex)
                    {
                        Log.Warn($"[AnimeSurvivalMod] Error shifting debuff {d.Id}: {ex.Message}");
                    }
                }
            }
            await Cmd.Wait(0.15f);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    // ==========================================
    // 6. Hex Reflection / Отражение проклятия
    // ==========================================
    public sealed class HexReflection : CardModel
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        public HexReflection()
            : base(3, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
        }

        protected override bool IsPlayable
        {
            get
            {
                if (!base.IsPlayable) return false;
                return Owner?.Creature?.Powers.Any(p => p.Type == PowerType.Debuff) == true;
            }
        }

        public override bool ShouldPlay(CardModel card, AutoPlayType autoPlayType)
        {
            if (card == this && Owner?.Creature?.Powers.Any(p => p.Type == PowerType.Debuff) != true)
            {
                return false;
            }
            return base.ShouldPlay(card, autoPlayType);
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            var debuffs = Owner.Creature.Powers.Where(p => p.Type == PowerType.Debuff).ToList();
            if (debuffs.Count == 0)
            {
                return;
            }

            foreach (var d in debuffs)
            {
                decimal amt = d.Amount;
                await PowerCmd.Remove(d);
                try
                {
                    var canonical = ModelDb.GetById<PowerModel>(d.Id);
                    var newPower = (PowerModel)canonical.MutableClone();
                    await PowerCmd.Apply(choiceContext, newPower, cardPlay.Target, amt, Owner.Creature, this);
                }
                catch (Exception ex)
                {
                    Log.Warn($"[AnimeSurvivalMod] Error reflecting debuff {d.Id}: {ex.Message}");
                }
            }
            await Cmd.Wait(0.15f);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    // ==========================================
    // 7. Soul Transfusion / Переливание души
    // ==========================================
    public sealed class SoulTransfusion : CardModel
    {
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

        public override TargetType TargetType => (CombatState != null && CombatState.PlayerCreatures.Count(c => c.IsAlive) > 1) 
            ? TargetType.AnyAlly 
            : TargetType.Self;

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("CostReduction", 1m)
        };

        private int _deathsThisCombat = 0;

        public SoulTransfusion()
            : base(10, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
        {
        }

        public override Task BeforeCombatStart()
        {
            _deathsThisCombat = 0;
            return base.BeforeCombatStart();
        }

        public override Task AfterCombatEnd(MegaCrit.Sts2.Core.Rooms.CombatRoom room)
        {
            _deathsThisCombat = 0;
            return base.AfterCombatEnd(room);
        }

        public override Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            if (wasRemovalPrevented) return Task.CompletedTask;
            if (CombatState == null) return Task.CompletedTask;

            _deathsThisCombat++;
            InvokeEnergyCostChanged();
            return Task.CompletedTask;
        }

        public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
        {
            if (card == this)
            {
                modifiedCost = Math.Max(0m, originalCost - _deathsThisCombat);
                return true;
            }
            modifiedCost = originalCost;
            return false;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int currentHp = Owner.Creature.CurrentHp;
            int allyGain = currentHp <= 1 ? 1 : currentHp / 2;
            int ownerLose = currentHp <= 1 ? 0 : allyGain;

            if (ownerLose > 0)
            {
                await CreatureCmd.Damage(choiceContext, Owner.Creature, ownerLose, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, this, cardPlay);
            }

            Creature? ally = (cardPlay.Target != null && cardPlay.Target != Owner.Creature) ? cardPlay.Target : null;
            if (ally == null)
            {
                ally = CombatState.PlayerCreatures.FirstOrDefault(c => c != Owner.Creature && c.IsAlive);
            }

            if (ally != null)
            {
                await CreatureCmd.Heal(ally, allyGain, playAnim: true);
            }
            else if (Owner.IsOstyAlive && Owner.Osty != null)
            {
                await CreatureCmd.Heal(Owner.Osty, allyGain, playAnim: true);
            }
            await Cmd.Wait(0.15f);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-2);
        }
    }

    // ==========================================
    // 8. Necrotic Tether / Некротическая привязь
    // ==========================================
    public sealed class NecroticTether : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("Turns", 2m)
        };

        public NecroticTether()
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
            await PowerCmd.Apply<NecroticTetherPower>(choiceContext, cardPlay.Target, DynamicVars["Turns"].BaseValue, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["Turns"].UpgradeValueBy(1m);
        }
    }

    public sealed class NecroticTetherPower : PowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
        {
            if (target != Owner && (target.IsPlayer || (Owner?.CombatState?.PlayerCreatures.Contains(target) == true)) && result.TotalDamage > 0 && Owner != null && Owner.IsAlive)
            {
                Flash();
                decimal sharedDmg = Math.Ceiling(result.TotalDamage * 0.5m);
                await CreatureCmd.Damage(choiceContext, Owner, sharedDmg, ValueProp.Unpowered, target);
            }
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            await PowerCmd.Decrement(this);
        }
    }

    // ==========================================
    // 9. Harvest of Malice / Жатва злобы
    // ==========================================
    public sealed class HarvestOfMalice : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(0m, ValueProp.Move)
        };

        public HarvestOfMalice()
            : base(2, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            decimal totalDoom = 0m;
            if (CombatState != null)
            {
                totalDoom = CombatState.Enemies.Where(e => e.IsAlive).Sum(e => e.GetPowerAmount<DoomPower>());
            }
            decimal dmg = totalDoom + DynamicVars.Damage.BaseValue;

            await DamageCmd.Attack(dmg)
                .FromCard(this, cardPlay)
                .TargetingAllOpponents(CombatState!)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(6m);
        }
    }

    // ==========================================
    // 10. Ossuary Fusion / Слияние склепа
    // ==========================================
    public sealed class OssuaryFusion : CardModel
    {
        public override bool GainsBlock => true;

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new BlockVar(20m, ValueProp.Move)
        };

        public OssuaryFusion()
            : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner.IsOstyAlive && Owner.Osty != null)
            {
                await CreatureCmd.Kill(Owner.Osty);
            }

            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
            await PowerCmd.Apply<OssuaryFusionPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Block.UpgradeValueBy(8m);
        }
    }

    public sealed class OssuaryFusionPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == Owner?.Player && Owner?.Player != null)
            {
                Flash();
                await PowerCmd.Remove(this);
                await OstyCmd.Summon(choiceContext, Owner.Player, 20m, null);
            }
        }
    }

    // ==========================================
    // 11. Charon's Obol / Обол Харона
    // ==========================================
    public sealed class CharonsObol : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("EnergyGain", 3m)
        };

        public CharonsObol()
            : base(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            decimal energy = DynamicVars["EnergyGain"].BaseValue;
            await PlayerCmd.GainEnergy(energy, Owner);

            if (CombatState != null)
            {
                var debt = CombatState.CreateCard<CharonsDebt>(Owner);
                await CardPileCmd.AddGeneratedCardToCombat(debt, PileType.Discard, Owner);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars["EnergyGain"].UpgradeValueBy(1m);
        }
    }

    public sealed class CharonsDebt : CardModel
    {
        public override bool GainsBlock => true;
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain, CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new BlockVar(4m, ValueProp.Move)
        };

        public CharonsDebt()
            : base(1, CardType.Skill, CardRarity.Token, TargetType.Self)
        {
        }

        public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side == CombatSide.Player && Owner?.PlayerCombatState?.Hand != null && Owner.PlayerCombatState.Hand.Cards.Contains(this))
            {
                await CreatureCmd.Damage(choiceContext, Owner.Creature, 4, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);
            }
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Block.UpgradeValueBy(3m);
        }
    }

    // ==========================================
    // 12. Pyre of the Damned / Костёр проклятых
    // ==========================================
    public sealed class PyreOfTheDamned : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(8m, ValueProp.Move),
            new DynamicVar("Doom", 3m)
        };

        public PyreOfTheDamned()
            : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var otherCards = Owner.PlayerCombatState.Hand.Cards.Where(c => c != this).Take(3).ToList();
            foreach (var card in otherCards)
            {
                await CardCmd.Exhaust(choiceContext, card);
            }

            int count = otherCards.Count;
            if (count > 0 && CombatState != null)
            {
                decimal totalDmg = DynamicVars.Damage.BaseValue * count;
                await DamageCmd.Attack(totalDmg)
                    .FromCard(this, cardPlay)
                    .TargetingAllOpponents(CombatState)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);

                decimal doomAmt = DynamicVars["Doom"].BaseValue * count;
                foreach (var enemy in CombatState.Enemies.Where(e => e.IsAlive).ToList())
                {
                    await PowerCmd.Apply<DoomPower>(choiceContext, enemy, doomAmt, Owner.Creature, this);
                }
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
            DynamicVars["Doom"].UpgradeValueBy(1m);
        }
    }

    // ==========================================
    // 13. Death Echo / Эхо смерти
    // ==========================================
    public sealed class DeathEcho : CardModel
    {
        public DeathEcho()
            : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<DeathEchoPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public sealed class DeathEchoPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            if (!wasRemovalPrevented && Owner?.Player != null)
            {
                Flash();
                await PlayerCmd.GainEnergy(1, Owner.Player);
                await CardPileCmd.Draw(choiceContext, 1, Owner.Player, false);
            }
        }
    }

    // ==========================================
    // 14. Shroud Tear / Разрыв савана
    // ==========================================
    public sealed class ShroudTear : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(14m, ValueProp.Move)
        };

        public ShroudTear()
            : base(2, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
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

            decimal currentDoom = cardPlay.Target.GetPowerAmount<DoomPower>();
            if (currentDoom > 0)
            {
                await PowerCmd.Apply<DoomPower>(choiceContext, cardPlay.Target, currentDoom, Owner.Creature, this);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(4m);
        }
    }

    // ==========================================
    // 15. Essence Siphon / Вытягивание сущности
    // ==========================================
    public sealed class EssenceSiphon : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("Strength", 2m)
        };

        public EssenceSiphon()
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));
            decimal str = DynamicVars["Strength"].BaseValue;

            await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Target, -str, Owner.Creature, this);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, str, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["Strength"].UpgradeValueBy(1m);
        }
    }

    // ==========================================
    // 16. Wraith Step / Призрачный шаг
    // ==========================================
    public sealed class WraithStep : CardModel
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        public WraithStep()
            : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<IntangiblePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
            await PowerCmd.Apply<WraithStepDoomPower>(choiceContext, Owner.Creature, 3m, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public sealed class WraithStepDoomPower : PowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == Owner?.Player && Owner != null)
            {
                Flash();
                await PowerCmd.Remove(this);
                await PowerCmd.Apply<DoomPower>(choiceContext, Owner, 3m, Owner, null);
            }
        }
    }

    // ==========================================
    // 17. Sigil of Decay / Символ тления
    // ==========================================
    public sealed class SigilOfDecay : CardModel
    {
        public SigilOfDecay()
            : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<SigilOfDecayPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public sealed class SigilOfDecayPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side == CombatSide.Player && Owner?.CombatState != null)
            {
                Flash();
                foreach (var enemy in Owner.CombatState.Enemies.Where(e => e.IsAlive).ToList())
                {
                    decimal doom = enemy.GetPowerAmount<DoomPower>();
                    if (doom > 0)
                    {
                        await CreatureCmd.Damage(choiceContext, enemy, doom, ValueProp.Unblockable | ValueProp.Unpowered, Owner);
                    }
                }
            }
        }
    }
}

