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
using MegaCrit.Sts2.Core.ValueProps;

namespace AnimeSurvivalMod
{
    // ==========================================
    // 1. Divert & Scatter / Отвод и рассеивание
    // ==========================================
    public sealed class DivertAndScatter : CardModel
    {
        public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

        public DivertAndScatter()
            : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<DivertAndScatterPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public sealed class DivertAndScatterPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
        {
            if (target == Owner && dealer != null && dealer != Owner)
            {
                return 0m;
            }
            return 1m;
        }

        public override async Task BeforeDamageReceived(PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target == Owner && dealer != null && dealer != Owner && amount > 0)
            {
                Flash();
                var otherAllies = CombatState.PlayerCreatures.Where(c => c != Owner && c.IsAlive).ToList();
                if (otherAllies.Count > 0)
                {
                    decimal splitDamage = Math.Max(1m, Math.Floor(amount / otherAllies.Count));
                    foreach (var ally in otherAllies)
                    {
                        await CreatureCmd.Damage(choiceContext, ally, splitDamage, ValueProp.Unblockable | ValueProp.Unpowered | ValueProp.Move, cardSource, null);
                    }
                }
                else
                {
                    var enemies = CombatState.HittableEnemies.Where(e => e.IsAlive).ToList();
                    if (enemies.Count > 0)
                    {
                        decimal splitDamage = Math.Max(1m, Math.Floor(amount / enemies.Count));
                        foreach (var enemy in enemies)
                        {
                            await CreatureCmd.Damage(choiceContext, enemy, splitDamage, ValueProp.Unpowered, Owner);
                        }
                    }
                }
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
    // 2. Dance of Poison Blades / Танец ядовитых клинков
    // ==========================================
    public sealed class DanceOfPoisonBlades : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new CardsVar(3)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
        {
            HoverTipFactory.FromCard<PoisonShiv>()
        };

        public DanceOfPoisonBlades()
            : base(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await CreatureCmd.TriggerAnim(Owner.Creature, "Cast", Owner.Character.CastAnimDelay);
            await PoisonShiv.CreateInHand(Owner, DynamicVars.Cards.IntValue, CombatState!, IsUpgraded);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Cards.UpgradeValueBy(1m);
        }
    }

    // ==========================================
    // Token: Poison Shiv / Ядовитая заточка
    // ==========================================
    public sealed class PoisonShiv : CardModel
    {
        protected override HashSet<CardTag> CanonicalTags => new HashSet<CardTag> { CardTag.Shiv };

        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(4m, ValueProp.Move),
            new PowerVar<PoisonPower>("Poison", 1m)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
        {
            HoverTipFactory.FromPower<PoisonPower>()
        };

        public PoisonShiv()
            : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithAttackerAnim("Shiv", 0.2f)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

            decimal poisonAmount = DynamicVars["Poison"].BaseValue;
            await PowerCmd.Apply<PoisonPower>(choiceContext, cardPlay.Target, poisonAmount, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(2m);
            DynamicVars["Poison"].UpgradeValueBy(1m);
        }

        public static async Task<IEnumerable<CardModel>> CreateInHand(Player owner, int count, ICombatState combatState, bool upgraded = false, Player? creator = null)
        {
            if (count == 0 || CombatManager.Instance.IsOverOrEnding)
            {
                return Array.Empty<CardModel>();
            }
            var shivs = new List<CardModel>();
            for (int i = 0; i < count; i++)
            {
                var shiv = combatState.CreateCard<PoisonShiv>(owner);
                if (upgraded)
                {
                    CardCmd.Upgrade(shiv);
                }
                shivs.Add(shiv);
            }
            await CardPileCmd.AddGeneratedCardsToCombat(shivs, PileType.Hand, creator ?? owner);
            return shivs;
        }
    }

    // ==========================================
    // 3. Toxic Ricochet / Токсичный рикошет
    // ==========================================
    public sealed class ToxicRicochet : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(7m, ValueProp.Move)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
        {
            HoverTipFactory.FromPower<PoisonPower>()
        };

        public ToxicRicochet()
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

            decimal targetPoison = cardPlay.Target.GetPowerAmount<PoisonPower>();
            if (targetPoison > 0)
            {
                var others = CombatState.HittableEnemies.Where(e => e != cardPlay.Target).ToList();
                foreach (var other in others)
                {
                    await DamageCmd.Attack(targetPoison)
                        .FromCard(this, cardPlay)
                        .Targeting(other)
                        .WithHitFx("vfx/vfx_poison_impact")
                        .Execute(choiceContext);
                }
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
        }
    }

    // ==========================================
    // 4. Shadow Veil / Теневой покров
    // ==========================================
    public sealed class ShadowVeil : CardModel
    {
        public override bool GainsBlock => true;

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new BlockVar(6m, ValueProp.Move),
            new DynamicVar("PerShiv", 2m)
        };

        public ShadowVeil()
            : base(1, CardType.Skill, CardRarity.Common, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int shivsPlayed = CombatManager.Instance.History.CardPlaysFinished
                .Count(e => e.CardPlay.Player == Owner && (e.CardPlay.Card is PoisonShiv || e.CardPlay.Card.Id.Entry.Contains("SHIV")));

            decimal perShiv = DynamicVars["PerShiv"].BaseValue;
            decimal totalBlock = DynamicVars.Block.BaseValue + (shivsPlayed * perShiv);

            await CreatureCmd.GainBlock(Owner.Creature, totalBlock, ValueProp.Move, cardPlay);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Block.UpgradeValueBy(3m);
            DynamicVars["PerShiv"].UpgradeValueBy(1m);
        }
    }

    // ==========================================
    // 5. Envenomed Arsenal / Отравленный арсенал
    // ==========================================
    public sealed class EnvenomedArsenal : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new PowerVar<PoisonPower>("Poison", 2m),
            new DynamicVar("ExhaustPoison", 1m)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
        {
            HoverTipFactory.FromPower<PoisonPower>()
        };

        public EnvenomedArsenal()
            : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            decimal poisonPerShiv = DynamicVars["Poison"].BaseValue;
            decimal poisonOnExhaust = DynamicVars["ExhaustPoison"].BaseValue;
            await PowerCmd.Apply<EnvenomedArsenalPower>(choiceContext, Owner.Creature, poisonPerShiv, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public sealed class EnvenomedArsenalPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task AfterCardGeneratedForCombat(CardModel card, Player? creator)
        {
            if (card is PoisonShiv || card.Id.Entry.Contains("SHIV"))
            {
                var target = CombatState.HittableEnemies.OrderBy(_ => Guid.NewGuid()).FirstOrDefault();
                if (target != null)
                {
                    Flash();
                    await PowerCmd.Apply<PoisonPower>(new BlockingPlayerChoiceContext(), target, Amount, Owner, null);
                }
            }
        }

        public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
        {
            Flash();
            foreach (var enemy in CombatState.HittableEnemies)
            {
                await PowerCmd.Apply<PoisonPower>(choiceContext, enemy, 1m, Owner, null);
            }
        }
    }

    // ==========================================
    // 6. Phantom Ricochet / Фантомный рикошет
    // ==========================================
    public sealed class PhantomRicochet : CardModel
    {
        public override bool ShouldReceiveCombatHooks => true;
        private int _combatDiscards = 0;

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(5m, ValueProp.Move)
        };

        public PhantomRicochet()
            : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
        }

        public override Task AfterCardDiscarded(PlayerChoiceContext choiceContext, CardModel card)
        {
            _combatDiscards++;
            return Task.CompletedTask;
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            decimal totalDmg = DynamicVars.Damage.BaseValue + _combatDiscards;

            await DamageCmd.Attack(totalDmg)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
        }
    }

    // ==========================================
    // 7. Well Poisoner / Отравитель колодцев
    // ==========================================
    public sealed class WellPoisoner : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new PowerVar<PoisonPower>("Poison", 6m)
        };

        public WellPoisoner()
            : base(2, CardType.Power, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            decimal poisonAmt = DynamicVars["Poison"].BaseValue;
            await PowerCmd.Apply<WellPoisonerPower>(choiceContext, Owner.Creature, poisonAmt, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
            DynamicVars["Poison"].UpgradeValueBy(3m);
        }
    }

    public sealed class WellPoisonerPower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Counter;

        public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature applier, CardModel cardSource)
        {
            if (power.Owner == Owner && power.Type == PowerType.Debuff && applier != null && applier != Owner && applier.IsAlive)
            {
                Flash();
                await PowerCmd.Apply<PoisonPower>(choiceContext, applier, Amount, Owner, null);
            }
        }
    }

    // ==========================================
    // 8. Shadow Double / Теневой дублёр
    // ==========================================
    public sealed class ShadowDouble : CardModel
    {
        public ShadowDouble()
            : base(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            await PowerCmd.Apply<ShadowDoublePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
        }

        protected override void OnUpgrade()
        {
            EnergyCost.UpgradeBy(-1);
        }
    }

    public sealed class ShadowDoublePower : PowerModel
    {
        public override PowerType Type => PowerType.Buff;
        public override PowerStackType StackType => PowerStackType.Single;

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card.Owner == Owner?.Player && cardPlay.Card.Type == CardType.Attack && !cardPlay.IsAutoPlay && cardPlay.Target != null)
            {
                Flash();
                await PowerCmd.Remove(this);
                await CardCmd.AutoPlay(choiceContext, cardPlay.Card, cardPlay.Target, AutoPlayType.Default, skipXCapture: false, skipCardPileVisuals: false);
            }
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == Owner?.Player && Owner?.Player != null)
            {
                var hand = Owner.Player.PlayerCombatState.Hand.Cards.ToList();
                if (hand.Count > 0)
                {
                    var cardToDiscard = hand[new Random().Next(hand.Count)];
                    await CardCmd.Discard(choiceContext, cardToDiscard);
                }
            }
        }
    }

    // ==========================================
    // 9. Sleeve Arsenal / Веер из рукава
    // ==========================================
    public sealed class SleeveArsenal : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("Accuracy", 3m)
        };

        public SleeveArsenal()
            : base(0, CardType.Skill, CardRarity.Common, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            int handCount = Owner.PlayerCombatState.Hand.Cards.Count;
            int shivCount = handCount >= 6 ? 4 : 2;

            var shivs = new List<CardModel>();
            for (int i = 0; i < shivCount; i++)
            {
                shivs.Add(CombatState!.CreateCard<MegaCrit.Sts2.Core.Models.Cards.Shiv>(Owner));
            }
            await CardPileCmd.AddGeneratedCardsToCombat(shivs, PileType.Hand, Owner);

            if (handCount >= 6)
            {
                decimal accAmt = DynamicVars["Accuracy"].BaseValue;
                await PowerCmd.Apply<AccuracyPower>(choiceContext, Owner.Creature, accAmt, Owner.Creature, this);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars["Accuracy"].UpgradeValueBy(2m);
        }
    }

    // ==========================================
    // 10. Catalytic Outbreak / Эпидемия
    // ==========================================
    public sealed class CatalyticOutbreak : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DynamicVar("Multiplier", 1.25m)
        };

        public CatalyticOutbreak()
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            var poisonPower = cardPlay.Target.GetPower<PoisonPower>();
            if (poisonPower != null && poisonPower.Amount > 0)
            {
                decimal total = poisonPower.Amount;
                await PowerCmd.Remove(poisonPower);

                var livingEnemies = CombatState?.Enemies.Where(e => e.IsAlive).ToList();
                if (livingEnemies != null && livingEnemies.Count > 0)
                {
                    decimal bonus = DynamicVars["Multiplier"].BaseValue;
                    decimal perEnemy = Math.Max(1m, Math.Floor((total * bonus) / livingEnemies.Count));
                    foreach (var e in livingEnemies)
                    {
                        await PowerCmd.Apply<PoisonPower>(choiceContext, e, perEnemy, Owner.Creature, this);
                    }
                }
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars["Multiplier"].UpgradeValueBy(0.25m);
            EnergyCost.UpgradeBy(-1);
        }
    }

    // ==========================================
    // 11. Deadly Gambit / Смертоносный гамбит
    // ==========================================
    public sealed class DeadlyGambit : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(4m, ValueProp.Move),
            new PowerVar<PoisonPower>("Poison", 1m)
        };

        public DeadlyGambit()
            : base(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var handCards = Owner.PlayerCombatState.Hand.Cards.Where(c => c != this).ToList();
            foreach (var c in handCards)
            {
                await CardCmd.Discard(choiceContext, c);
            }

            int discarded = handCards.Count;
            decimal dmg = DynamicVars.Damage.BaseValue;
            if (discarded > 0 && CombatState != null)
            {
                var rng = new Random();
                for (int i = 0; i < discarded; i++)
                {
                    if (CombatManager.Instance.IsOverOrEnding) break;
                    var livingEnemies = CombatState.HittableEnemies.Where(e => e.IsAlive).ToList();
                    if (livingEnemies.Count == 0) break;

                    var targetEnemy = livingEnemies[rng.Next(livingEnemies.Count)];
                    await DamageCmd.Attack(dmg)
                        .FromCard(this, cardPlay)
                        .Targeting(targetEnemy)
                        .WithHitFx("vfx/vfx_attack_slash")
                        .Execute(choiceContext);

                    if (targetEnemy.IsAlive && !CombatManager.Instance.IsOverOrEnding)
                    {
                        await PowerCmd.Apply<PoisonPower>(choiceContext, targetEnemy, 1m, Owner.Creature, this);
                    }
                    await Cmd.Wait(0.06f);
                }
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(2m);
        }
    }

    // ==========================================
    // 12. Smokescreen Flurry / Дымовая завеса
    // ==========================================
    public sealed class SmokescreenFlurry : CardModel
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new BlockVar(7m, ValueProp.Move)
        };

        public SmokescreenFlurry()
            : base(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            decimal blockAmt = DynamicVars.Block.BaseValue;
            await CreatureCmd.GainBlock(Owner.Creature, blockAmt, ValueProp.Move, cardPlay);

            if (cardPlay.PlayIndex >= 3)
            {
                await PowerCmd.Apply<IntangiblePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Block.UpgradeValueBy(3m);
        }
    }

    // ==========================================
    // 13. Glass Shard / Осколок стекла (Токен)
    // ==========================================
    public sealed class GlassShard : CardModel
    {
        public override IEnumerable<CardKeyword> CanonicalKeywords => new[] { CardKeyword.Exhaust };

        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(4m, ValueProp.Move)
        };

        public GlassShard()
            : base(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
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
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(2m);
        }
    }

    // ==========================================
    // 14. Glass Stiletto / Стеклянный стилет
    // ==========================================
    public sealed class GlassStiletto : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(16m, ValueProp.Move),
            new CardsVar(2)
        };

        protected override IEnumerable<IHoverTip> ExtraHoverTips => new IHoverTip[]
        {
            HoverTipFactory.FromCard<GlassShard>()
        };

        public GlassStiletto()
            : base(0, CardType.Attack, CardRarity.Common, TargetType.AnyEnemy)
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

            if (CombatState != null)
            {
                var shards = new List<CardModel>
                {
                    CombatState.CreateCard<GlassShard>(Owner),
                    CombatState.CreateCard<GlassShard>(Owner)
                };
                await CardPileCmd.AddGeneratedCardsToCombat(shards, PileType.Hand, Owner);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(4m);
        }
    }

    // ==========================================
    // 15. Neurotoxin Flash / Нейротоксический шок
    // ==========================================
    public sealed class NeurotoxinFlash : CardModel
    {
        protected override IEnumerable<DynamicVar> CanonicalVars => new DynamicVar[]
        {
            new DamageVar(7m, ValueProp.Move)
        };

        public NeurotoxinFlash()
            : base(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
        {
        }

        protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            ArgumentNullException.ThrowIfNull(cardPlay.Target, nameof(cardPlay.Target));

            decimal dmg = DynamicVars.Damage.BaseValue;
            await DamageCmd.Attack(dmg)
                .FromCard(this, cardPlay)
                .Targeting(cardPlay.Target)
                .WithHitFx("vfx/vfx_attack_slash")
                .Execute(choiceContext);

            var poison = cardPlay.Target.GetPower<PoisonPower>();
            if (poison != null && poison.Amount > 0)
            {
                await CreatureCmd.Damage(choiceContext, cardPlay.Target, poison.Amount, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);
            }
        }

        protected override void OnUpgrade()
        {
            DynamicVars.Damage.UpgradeValueBy(3m);
        }
    }
}
