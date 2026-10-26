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
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.ValueProps;

namespace AnimeSurvivalMod
{
    // ==========================================
    // 1. Blade Orbit Resonance / Звёздный резонанс клинка
    // ==========================================
    public sealed class BladeOrbitResonance : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(9m, ValueProp.Move),
            new ForgeVar(4)
        };

        public BladeOrbitResonance()
            : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
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

            decimal forgeAmt = DynamicVars.Forge.BaseValue;
            await ForgeCmd.Forge(forgeAmt, Owner, this);

            var blade = Owner.PlayerCombatState.Hand.Cards.OfType<SovereignBlade>().FirstOrDefault();
            if (blade != null)
            {
                await DamageCmd.Attack(blade.DynamicVars.Damage.BaseValue)
                    .FromCard(blade, cardPlay)
                    .Targeting(cardPlay.Target)
                    .WithHitFx("vfx/vfx_giant_horizontal_slash", null, "slash_attack.mp3")
                    .Execute(choiceContext);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
            DynamicVars.Forge.UpgradeValueBy(2m);
        }
    }

    // ==========================================
    // 2. Gravitational Singularity / Гравитационная сингулярность
    // ==========================================
    public sealed class GravitationalSingularity : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("Redirect", 50m)
        };

        public GravitationalSingularity()
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            decimal redirectPct = DynamicVars["Redirect"].BaseValue;
            await PowerCmd.Apply<GravitationalSingularityPower>(choiceContext, cardPlay.Target, redirectPct, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
            DynamicVars["Redirect"].UpgradeValueBy(25m);
        }
    }

    public sealed class GravitationalSingularityPower : PowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Single;

        private bool _isRedirecting = false;

        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
        {
            if (dealer == Owner && target != null && target.Side == CombatSide.Player && props.IsPoweredAttack())
            {
                decimal reductionPct = Math.Min(Amount, 75m) / 100m;
                return Math.Max(0.2m, 1m - reductionPct);
            }
            return 1m;
        }

        public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
        {
            if (_isRedirecting) return;
            if (dealer == Owner && dealer != null && dealer.IsMonster && target != null && target.Side == CombatSide.Player && props.IsPoweredAttack())
            {
                _isRedirecting = true;
                try
                {
                    Flash();
                    decimal pct = Math.Min(Amount, 75m) / 100m;
                    decimal redirectedDmg = 0m;
                    if (result.TotalDamage > 0)
                    {
                        decimal orig = result.TotalDamage / Math.Max(0.25m, 1m - pct);
                        redirectedDmg = Math.Min(150m, Math.Max(1m, Math.Floor(orig * pct)));
                    }

                    var others = CombatState?.Creatures?.Where(c => c != null && c.IsMonster && c != Owner && c.IsAlive).ToList() ?? new List<Creature>();
                    if (others.Count > 0 && redirectedDmg > 0m)
                    {
                        decimal perEnemy = Math.Max(1m, Math.Floor(redirectedDmg / others.Count));
                        foreach (var o in others)
                        {
                            if (o.IsAlive)
                            {
                                await CreatureCmd.Damage(choiceContext, o, perEnemy, ValueProp.Unpowered, Owner);
                            }
                        }
                    }

                    var player = Applier?.Player ?? CombatState?.Players.FirstOrDefault();
                    if (player != null)
                    {
                        await PlayerCmd.GainStars(1m, player);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"[AnimeSurvivalMod] Error in GravitationalSingularityPower: {ex}");
                }
                finally
                {
                    _isRedirecting = false;
                }
            }
        }

        public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side == CombatSide.Enemy)
            {
                await PowerCmd.Remove(this);
            }
        }
    }

    // ==========================================
    // 3. Crown's Tribute / Королевская дань
    // ==========================================
    public sealed class CrownTribute : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new RepeatVar(2),
            new HealVar(2m),
            new CardsVar(1),
            new EnergyVar(1),
            new BlockVar(10m, ValueProp.Move)
        };

        public CrownTribute()
            : base(1, CardType.Skill, CardRarity.Rare, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            decimal turns = DynamicVars.Repeat.BaseValue;
            await PowerCmd.Apply<CrownTributePower>(choiceContext, cardPlay.Target, turns, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
            DynamicVars.Repeat.UpgradeValueBy(1m);
        }
    }

    public sealed class CrownTributePower : PowerModel
    {
        public override PowerType Type => PowerType.Debuff;
        public override PowerStackType StackType => PowerStackType.Counter;

        private bool _isProcessing = false;

        public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (_isProcessing) return;
            if (target == Owner && dealer != null && dealer.IsPlayer && props.IsPoweredAttack() && result.TotalDamage > 0)
            {
                _isProcessing = true;
                try
                {
                    Flash();
                    await CreatureCmd.Heal(dealer, 2m);
                    if (dealer.Player != null)
                    {
                        await CardPileCmd.Draw(choiceContext, 1m, dealer.Player);
                    }
                    if (Applier?.Player != null)
                    {
                        await PlayerCmd.GainStars(1m, Applier.Player);
                    }
                }
                finally
                {
                    _isProcessing = false;
                }
            }
        }

        public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature creature, bool wasRemovalPrevented, float deathAnimLength)
        {
            if (creature == Owner)
            {
                Flash();
                foreach (var p in CombatState.Players)
                {
                    await PlayerCmd.GainEnergy(1, p);
                    await CreatureCmd.GainBlock(p.Creature, 10m, ValueProp.Move, null);
                }
            }
        }

        public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side == CombatSide.Enemy)
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
    // 4. Astral Eclipse / Астральное затмение
    // ==========================================
    public sealed class AstralEclipse : CardModel
    {
        public override int CanonicalStarCost => 2;

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(14m, ValueProp.Move),
            new DynamicVar("MaxSteal", 15m),
            new PowerVar<WeakPower>("Weak", 2m),
            new ForgeVar(6)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
        {
            HoverTipFactory.FromPower<WeakPower>()
        };

        public AstralEclipse()
            : base(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
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

            decimal enemyStr = cardPlay.Target.GetPowerAmount<StrengthPower>();
            if (enemyStr > 0)
            {
                await PowerCmd.Apply<StrengthPower>(choiceContext, cardPlay.Target, -1m, Owner.Creature, this);
                await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
            }
            else
            {
                await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, DynamicVars["Weak"].BaseValue, Owner.Creature, this);
                decimal forgeBonus = DynamicVars.Forge.BaseValue;
                await ForgeCmd.Forge(forgeBonus, Owner, this);
            }

            if (cardPlay.Target.Block > 0)
            {
                decimal maxSteal = DynamicVars["MaxSteal"].BaseValue;
                decimal stealBlock = Math.Min(maxSteal, cardPlay.Target.Block);
                await CreatureCmd.LoseBlock(choiceContext, cardPlay.Target, stealBlock, Owner.Creature);
                await CreatureCmd.GainBlock(Owner.Creature, stealBlock, ValueProp.Move, cardPlay);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(4m);
            DynamicVars["MaxSteal"].UpgradeValueBy(10m);
            DynamicVars.Forge.UpgradeValueBy(3m);
        }
    }

    // ==========================================
    // 5. Supernova Coronation / Сверхновая коронация
    // ==========================================
    public sealed class SupernovaCoronation : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("PerStar", 4m),
            new DynamicVar("StarThreshold", 6m),
            new EnergyVar(2)
        };

        public SupernovaCoronation()
            : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            decimal basePerStar = DynamicVars["PerStar"].BaseValue;
            await PowerCmd.Apply<SupernovaCoronationPower>(choiceContext, Owner.Creature, basePerStar, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
            DynamicVars["PerStar"].UpgradeValueBy(2m);
        }
    }

    public sealed class SupernovaCoronationPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        private int _totalStarsSpent = 0;

        public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
        {
            if (side == CombatSide.Player && Owner?.Player != null)
            {
                int unspentStars = (int)Owner.Player.PlayerCombatState.Stars;
                if (unspentStars > 0)
                {
                    Flash();
                    decimal totalDmg = Amount * unspentStars;
                    decimal totalBlock = Math.Max(1m, Math.Floor(Amount / 2m)) * unspentStars;

                    if (CombatState != null)
                    {
                        foreach (var enemy in CombatState.HittableEnemies)
                        {
                            if (enemy.IsAlive)
                            {
                                await CreatureCmd.Damage(choiceContext, enemy, totalDmg, ValueProp.Unpowered, Owner);
                            }
                        }
                    }

                    await CreatureCmd.GainBlock(Owner, totalBlock, ValueProp.Move, null);
                }
            }
        }

        public override async Task AfterStarsSpent(int amount, Player spender)
        {
            if (spender == Owner?.Player && Owner != null)
            {
                _totalStarsSpent += amount;
                if (_totalStarsSpent >= 6)
                {
                    _totalStarsSpent -= 6;
                    Flash();

                    foreach (var enemy in CombatState.HittableEnemies)
                    {
                        if (enemy.Block > 0)
                        {
                            await CreatureCmd.LoseBlock(new BlockingPlayerChoiceContext(), enemy, enemy.Block, Owner);
                        }
                        await PowerCmd.Apply<VulnerablePower>(new BlockingPlayerChoiceContext(), enemy, 3m, Owner, null);
                    }

                    foreach (var player in CombatState.Players)
                    {
                        await PlayerCmd.GainEnergy(2, player);
                    }
                }
            }
        }
    }

    // ==========================================
    // 6. Royal Edict: Execution / Королевский указ: Казнь
    // ==========================================
    public sealed class RoyalEdictExecution : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(18m, ValueProp.Move),
            new ForgeVar(4)
        };

        public RoyalEdictExecution()
            : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
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

            if (cardPlay.Target.CurrentHp < Owner.Creature.CurrentHp)
            {
                await ForgeCmd.Forge(DynamicVars.Forge.BaseValue, Owner, this);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(6m);
            DynamicVars.Forge.UpgradeValueBy(2m);
        }
    }

    // ==========================================
    // 7. Gravitational Collapse / Гравитационный коллапс
    // ==========================================
    public sealed class GravitationalCollapse : CardModel
    {
        public override bool GainsBlock => true;

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("PerStarDmg", 10m),
            new DynamicVar("PerStarBlock", 5m)
        };

        public GravitationalCollapse()
            : base(1, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int stars = (int)Owner.PlayerCombatState.Stars;
            if (stars > 0 && CombatState != null)
            {
                await PlayerCmd.LoseStars(stars, Owner);
                decimal dmg = stars * DynamicVars["PerStarDmg"].BaseValue;
                decimal blk = stars * DynamicVars["PerStarBlock"].BaseValue;

                await DamageCmd.Attack(dmg)
                    .FromCard(this, cardPlay)
                    .TargetingAllOpponents(CombatState)
                    .WithHitFx("vfx/vfx_attack_slash")
                    .Execute(choiceContext);

                await CreatureCmd.GainBlock(Owner.Creature, blk, ValueProp.Move, cardPlay);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars["PerStarDmg"].UpgradeValueBy(4m);
            DynamicVars["PerStarBlock"].UpgradeValueBy(2m);
        }
    }

    // ==========================================
    // 8. Aegis of the Crown / Эгида короны
    // ==========================================
    public sealed class AegisOfTheCrown : CardModel
    {
        public AegisOfTheCrown()
            : base(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<AegisOfTheCrownPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public sealed class AegisOfTheCrownPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task AfterForge(decimal amount, Player forger, AbstractModel source)
        {
            if (forger == Owner?.Player && Owner != null && amount > 0)
            {
                Flash();
                await CreatureCmd.GainBlock(Owner, amount, ValueProp.Move, null);
            }
        }
    }

    // ==========================================
    // 9. Blade Super Orbit / Сверхорбита клинка
    // ==========================================
    public sealed class BladeSuperOrbit : CardModel
    {
        public BladeSuperOrbit()
            : base(1, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var blade = Owner.PlayerCombatState.Hand.Cards.OfType<SovereignBlade>().FirstOrDefault()
                ?? Owner.PlayerCombatState.DrawPile.Cards.OfType<SovereignBlade>().FirstOrDefault()
                ?? Owner.PlayerCombatState.DiscardPile.Cards.OfType<SovereignBlade>().FirstOrDefault();

            if (blade != null && CombatState != null)
            {
                await DamageCmd.Attack(blade.DynamicVars.Damage.BaseValue)
                    .FromCard(blade, cardPlay)
                    .TargetingAllOpponents(CombatState)
                    .WithHitFx("vfx/vfx_giant_horizontal_slash", null, "slash_attack.mp3")
                    .Execute(choiceContext);

                await ForgeCmd.Forge(2m, Owner, this);
            }
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    // ==========================================
    // 10. Weight of Kings / Вес королей
    // ==========================================
    public sealed class WeightOfKings : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("BaseBonus", 10m)
        };

        public WeightOfKings()
            : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            decimal dmg = Owner.Creature.Block + DynamicVars["BaseBonus"].BaseValue;
            await DamageCmd.Attack(dmg)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_blunt")
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            DynamicVars["BaseBonus"].UpgradeValueBy(5m);
        }
    }

    // ==========================================
    // 11. Wormhole Slash / Разрез червоточины
    // ==========================================
    public sealed class WormholeSlash : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(12m, ValueProp.Move)
        };

        public WormholeSlash()
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

            var blade = Owner.PlayerCombatState.DrawPile.Cards.OfType<SovereignBlade>().FirstOrDefault()
                ?? Owner.PlayerCombatState.DiscardPile.Cards.OfType<SovereignBlade>().FirstOrDefault();

            if (blade != null)
            {
                await CardPileCmd.Add(blade, PileType.Hand, CardPilePosition.Top, this, false);
                blade.EnergyCost.SetThisTurn(0, true);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(4m);
        }
    }

    // ==========================================
    // 12. Antimatter Pulse / Антиматериальный импульс
    // ==========================================
    public sealed class AntimatterPulse : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("Vulnerable", 2m)
        };

        public AntimatterPulse()
            : base(1, CardType.Skill, CardRarity.Rare, TargetType.AllEnemies)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (Owner.PlayerCombatState.Stars >= 3 && CombatState != null)
            {
                await PlayerCmd.LoseStars(3, Owner);
                decimal vuln = DynamicVars["Vulnerable"].BaseValue;

                foreach (var enemy in CombatState.HittableEnemies)
                {
                    if (enemy.Block > 0)
                    {
                        await CreatureCmd.LoseBlock(choiceContext, enemy, enemy.Block, Owner.Creature);
                    }
                    var art = enemy.Powers.OfType<ArtifactPower>().FirstOrDefault();
                    if (art != null)
                    {
                        await PowerCmd.Remove(art);
                    }
                    await PowerCmd.Apply<VulnerablePower>(choiceContext, enemy, vuln, Owner.Creature, this);
                }
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars["Vulnerable"].UpgradeValueBy(1m);
        }
    }

    // ==========================================
    // 13. Void Ascension / Вознесение пустоты
    // ==========================================
    public sealed class VoidAscension : CardModel
    {
        public VoidAscension()
            : base(3, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<VoidAscensionPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public sealed class VoidAscensionPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == Owner?.Player && Owner?.Player != null)
            {
                Flash();
                await PlayerCmd.GainStars(2, Owner.Player);
            }
        }

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card is SovereignBlade && !cardPlay.IsAutoPlay && cardPlay.Target != null && Owner?.Player?.PlayerCombatState.Stars >= 6)
            {
                Flash();
                await CardCmd.AutoPlay(choiceContext, cardPlay.Card, cardPlay.Target, AutoPlayType.Default, false, false);
            }
        }
    }

    // ==========================================
    // 14. Meteor Barrage / Метеоритный шквал
    // ==========================================
    public sealed class MeteorBarrage : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(7m, ValueProp.Move)
        };

        public MeteorBarrage()
            : base(2, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            for (int i = 0; i < 3; i++)
            {
                if (cardPlay.Target.IsAlive)
                {
                    await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                        .FromCard(this, cardPlay)
                        .Targeting(cardPlay.Target)
                        .WithHitFx("vfx/vfx_attack_blunt")
                        .Execute(choiceContext);
                }
            }

            if (CombatState != null)
            {
                for (int i = 0; i < 2; i++)
                {
                    var star = CombatState.CreateCard<CosmicStardust>(Owner);
                    await CardPileCmd.AddGeneratedCardToCombat(star, PileType.Draw, Owner);
                }
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(2m);
        }
    }

    // ==========================================
    // 15. CosmicStardust / Звёздная пыль (Token)
    // ==========================================
    public sealed class CosmicStardust : CardModel
    {
        public override bool GainsBlock => true;
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Retain, CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new BlockVar(4m, ValueProp.Move),
            new DynamicVar("Stars", 1m)
        };

        public CosmicStardust()
            : base(0, CardType.Skill, CardRarity.Token, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
            await PlayerCmd.GainStars(DynamicVars["Stars"].BaseValue, Owner);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Block.UpgradeValueBy(2m);
            DynamicVars["Stars"].UpgradeValueBy(1m);
        }
    }
}

