#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Orbs;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.ValueProps;

namespace AnimeSurvivalMod
{
    // ==========================================
    // 1. Crimson Pact / Багровый пакт (Common, Shared)
    // ==========================================
    public sealed class CrimsonPact : RelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Common;
        protected override string IconBaseName => "blood_soaked_rose";

        public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
        {
            if (target == Owner?.Creature && dealer == Owner?.Creature && result.TotalDamage > 0)
            {
                Flash();
                await CreatureCmd.GainBlock(Owner.Creature, 4m, ValueProp.Move, null);
            }
        }
    }

    // ==========================================
    // 2. Soul Lantern / Фонарь душ (Uncommon, Shared)
    // ==========================================
    public sealed class SoulLantern : RelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Uncommon;
        protected override string IconBaseName => "amphorae";

        public override async Task BeforeCombatStart()
        {
            if (Owner != null)
            {
                Flash();
                await PlayerCmd.GainEnergy(1, Owner);
                await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), 1, Owner);
            }
        }
    }

    // ==========================================
    // 3. Obsidian Anvil / Обсидиановая наковальня (Rare, Shared)
    // ==========================================
    public sealed class ObsidianAnvil : RelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Rare;
        protected override string IconBaseName => "anchor";

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            var combatState = Owner?.Creature?.CombatState;
            if (cardPlay.Card?.Owner == Owner && cardPlay.Card.IsUpgraded && combatState != null && Owner?.Creature != null)
            {
                Flash();
                foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive).ToList())
                {
                    await CreatureCmd.Damage(choiceContext, enemy, 3m, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);
                }
            }
        }
    }

    // ==========================================
    // 4. Clockwork Gear / Заводная шестерня (Common, Shared)
    // ==========================================
    public sealed class ClockworkGear : RelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Common;
        protected override string IconBaseName => "akabeko";
        public override bool ShowCounter => true;

        private int _counter = 0;
        public override int DisplayAmount => _counter;

        public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
        {
            if (cardPlay.Card?.Owner == Owner && cardPlay.Card.Type == CardType.Attack)
            {
                _counter = (_counter + 1) % 4;
                InvokeDisplayAmountChanged();

                var combatState = Owner?.Creature?.CombatState;
                if (_counter == 0 && combatState != null && Owner?.Creature != null)
                {
                    Flash();
                    foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive).ToList())
                    {
                        await CreatureCmd.Damage(choiceContext, enemy, 8m, ValueProp.Unpowered, Owner.Creature);
                    }
                }
            }
        }

        public override Task AfterCombatVictory(CombatRoom room)
        {
            _counter = 0;
            InvokeDisplayAmountChanged();
            return Task.CompletedTask;
        }
    }

    // ==========================================
    // 5. Cursed Amulet / Проклятый амулет (Ancient / Boss, Shared)
    // ==========================================
    public sealed class CursedAmulet : RelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Ancient;
        protected override string IconBaseName => "black_blood";

        public override async Task BeforeCombatStart()
        {
            if (Owner?.Creature != null)
            {
                Flash();
                await PowerCmd.Apply<DoomPower>(new ThrowingPlayerChoiceContext(), Owner.Creature, 2m, Owner.Creature, null);
            }
        }

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            if (player == Owner)
            {
                Flash();
                await PlayerCmd.GainEnergy(1, Owner);
            }
        }
    }

    // ==========================================
    // 6. Spire Compass / Компас шпиля (Shop, Shared)
    // ==========================================
    public sealed class SpireCompass : RelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Shop;
        protected override string IconBaseName => "astrolabe";

        public override async Task AfterObtained()
        {
            if (Owner != null)
            {
                await PlayerCmd.GainGold(75, Owner);
            }
        }

        public override async Task BeforeCombatStart()
        {
            if (Owner != null)
            {
                Flash();
                await CardPileCmd.Draw(new ThrowingPlayerChoiceContext(), 1, Owner);
            }
        }
    }

    // ==========================================
    // 7. War Drum / Барабан войны (Common, Ironclad)
    // ==========================================
    public sealed class WarDrum : RelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Common;
        protected override string IconBaseName => "battle_horn";

        public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
        {
            if (power is StrengthPower && power.Owner == Owner?.Creature && amount > 0 && Owner?.Creature != null)
            {
                Flash();
                await CreatureCmd.GainBlock(Owner.Creature, 3m, ValueProp.Move, null);
            }
        }
    }

    // ==========================================
    // 8. Viper Fang / Клык гадюки (Uncommon, Silent)
    // ==========================================
    public sealed class ViperFang : RelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Uncommon;
        protected override string IconBaseName => "bagh_nakh";

        public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
        {
            if (dealer == Owner?.Creature && target.IsAlive && props.IsPoweredAttack() && result.TotalDamage > 0)
            {
                var poison = target.GetPower<PoisonPower>();
                if (poison != null && poison.Amount > 0)
                {
                    Flash();
                    await PowerCmd.Apply<PoisonPower>(choiceContext, target, 1m, Owner.Creature, null);
                }
            }
        }
    }

    // ==========================================
    // 9. Capacitor Core / Ядро конденсатора (Rare, Defect)
    // ==========================================
    public sealed class CapacitorCore : RelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Rare;
        protected override string IconBaseName => "crystal_sphere";

        public override async Task BeforeCombatStart()
        {
            if (Owner != null)
            {
                Flash();
                await OrbCmd.Channel<PlasmaOrb>(new ThrowingPlayerChoiceContext(), Owner);
            }
        }
    }

    // ==========================================
    // 10. Graveyard Soil / Земля погоста (Uncommon, Necrobinder)
    // ==========================================
    public sealed class GraveyardSoil : RelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Uncommon;
        protected override string IconBaseName => "bone_flute";

        public override async Task AfterCardExhausted(PlayerChoiceContext choiceContext, CardModel card, bool causedByEthereal)
        {
            var combatState = Owner?.Creature?.CombatState;
            if (card?.Owner == Owner && combatState != null && Owner?.Creature != null)
            {
                Flash();
                foreach (var enemy in combatState.Enemies.Where(e => e.IsAlive).ToList())
                {
                    await CreatureCmd.Damage(choiceContext, enemy, 2m, ValueProp.Unblockable | ValueProp.Unpowered, Owner.Creature);
                }
            }
        }
    }

    // ==========================================
    // 11. Celestial Prism / Небесная призма (Rare, Regent)
    // ==========================================
    public sealed class CelestialPrism : RelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Rare;
        protected override string IconBaseName => "amethyst_aubergine";

        public override async Task AfterStarsSpent(int amount, Player spender)
        {
            if (spender == Owner && amount > 0 && Owner?.Creature != null)
            {
                Flash();
                decimal block = amount * 4m;
                await CreatureCmd.GainBlock(Owner.Creature, block, ValueProp.Move, null);
            }
        }
    }

    // ==========================================
    // 12. Heart of Infinity / Сердце бесконечности (Ancient / Boss, Shared)
    // ==========================================
    public sealed class HeartOfInfinity : RelicModel
    {
        public override RelicRarity Rarity => RelicRarity.Ancient;
        protected override string IconBaseName => "beating_remnant";

        public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
        {
            var combatState = Owner?.Creature?.CombatState;
            if (player == Owner && combatState != null && Owner.PlayerCombatState.Hand.Cards.Count < 10)
            {
                Flash();
                var rng = new Random();
                int roll = rng.Next(3);
                CardModel token;
                if (roll == 0) token = combatState.CreateCard<MegaCrit.Sts2.Core.Models.Cards.Shiv>(Owner);
                else if (roll == 1) token = combatState.CreateCard<GlassShard>(Owner);
                else token = combatState.CreateCard<CosmicStardust>(Owner);

                await CardPileCmd.AddGeneratedCardToCombat(token, PileType.Hand, Owner);
            }
        }
    }
}
