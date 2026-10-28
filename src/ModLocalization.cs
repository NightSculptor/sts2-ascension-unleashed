#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;

namespace AnimeSurvivalMod
{
    public static class ModLocalization
    {
        private static readonly Dictionary<string, string> English = new Dictionary<string, string>
        {
            // --- Ironclad ---
            { "BLOOD_RETRIBUTION.title", "Blood Retribution" },
            { "BLOOD_RETRIBUTION.description", "Deal {Damage:diff()} damage for each missing HP." },
            { "DEEP_CONCENTRATION.title", "Deep Concentration" },
            { "DEEP_CONCENTRATION.description", "At the start of your turn: if you played no cards last turn, gain {Vigor:diff()} [gold]Vigor[/gold]." },
            { "EGOIST.title", "Egoist" },
            { "EGOIST.description", "Draw {Cards:diff()} card. This turn, you deal double damage. All allies take double damage until their next turn. [gold]Exhaust[/gold]." },
            { "SACRIFICIAL_CLEAVE.title", "Sacrificial Cleave" },
            { "SACRIFICIAL_CLEAVE.description", "Lose {HpLoss} HP.\nDeal {Damage:diff()} damage to ALL enemies.\nHeal {Heal:diff()} HP for each enemy damaged." },
            { "TITANIC_BASTION.title", "Titanic Bastion" },
            { "TITANIC_BASTION.description", "Gain {Block:diff()} [gold]Block[/gold]. At the start of your next turn: if you still have [gold]Block[/gold], gain {Strength:diff()} [gold]Strength[/gold]." },
            { "IRON_LOAN.title", "Iron Loan" },
            { "IRON_LOAN.description", "Gain {EnergyGain:diff()} [gold]Energy[/gold] now. At the end of this turn: lose 5 HP for each unspent Energy. Next turn: draw 1 fewer card." },
            { "MARTYRS_BRAND.title", "Martyr's Brand" },
            { "MARTYRS_BRAND.description", "For {Turns:diff()} turns: whenever you take unblocked damage from enemy attacks, reflect 100% of that damage back to the attacker." },
            { "ASHEN_REQUIEM.title", "Ashen Requiem" },
            { "ASHEN_REQUIEM.description", "Deal damage equal to the number of Exhausted cards this combat times {Damage:diff()}." },
            { "SIEGE_STANCE.title", "Siege Stance" },
            { "SIEGE_STANCE.description", "You can no longer play Attacks. All Block gained is doubled. At the end of your turn, deal damage to ALL enemies equal to 50% of your Block." },
            { "BLOOD_PARITY.title", "Blood Parity" },
            { "BLOOD_PARITY.description", "Compare your HP with the target. If target has more HP, deal the difference (up to {MaxCap:diff()}). If you have more, lose the difference and gain {Strength:diff()} [gold]Strength[/gold]. [gold]Exhaust[/gold]." },
            { "RED_HOT_ANVIL.title", "Red-Hot Anvil" },
            { "RED_HOT_ANVIL.description", "Deal {Damage:diff()} damage. Upgrade a random card in your hand. If no cards can be upgraded, deal {Damage:diff()} damage again." },
            { "DOOM_TOLL.title", "Doom Toll" },
            { "DOOM_TOLL.description", "Costs X. Exhaust up to X cards from your hand. For each card Exhausted, deal {Damage:diff()} damage to ALL enemies and gain 1 [gold]Strength[/gold]." },
            { "SECOND_HEART.title", "Second Heart" },
            { "SECOND_HEART.description", "The next time you would receive fatal damage: prevent death, heal 25% of your Max HP, gain 4 Strength and 20 Block." },
            { "UNSTOPPABLE_RAMPAGE.title", "Unstoppable Rampage" },
            { "UNSTOPPABLE_RAMPAGE.description", "Deal {Damage:diff()} damage. If this kills the target or breaks all of its Block, add a 0-cost copy of this card to your hand this turn." },
            { "COMBUSTION_PACT.title", "Combustion Pact" },
            { "COMBUSTION_PACT.description", "Exhaust your entire draw pile. For every 3 cards Exhausted, deal {Damage:diff()} damage to ALL enemies and gain 1 [gold]Energy[/gold]. [gold]Exhaust[/gold]." },

            // --- Silent ---
            { "DIVERT_AND_SCATTER.title", "Divert & Scatter" },
            { "DIVERT_AND_SCATTER.description", "Until your next turn: all damage you would take is reduced to 0 and divided evenly among all other allies." },
            { "DANCE_OF_POISON_BLADES.title", "Dance of Poison Blades" },
            { "DANCE_OF_POISON_BLADES.description", "Add {Cards:diff()} [gold]Poison Shivs[/gold] to your [gold]hand[/gold]." },
            { "POISON_SHIV.title", "Poison Shiv" },
            { "POISON_SHIV.description", "Deal {Damage:diff()} damage.\nApply {Poison:diff()} [gold]Poison[/gold].\n[gold]Exhaust[/gold]." },
            { "TOXIC_RICOCHET.title", "Toxic Ricochet" },
            { "TOXIC_RICOCHET.description", "Deal {Damage:diff()} damage. If the target has [gold]Poison[/gold], deal damage equal to its [gold]Poison[/gold] to all other enemies." },
            { "SHADOW_VEIL.title", "Shadow Veil" },
            { "SHADOW_VEIL.description", "Gain {Block:diff()} [gold]Block[/gold] plus {PerShiv:diff()} [gold]Block[/gold] for each [gold]Shiv[/gold] played this combat." },
            { "ENVENOMED_ARSENAL.title", "Envenomed Arsenal" },
            { "ENVENOMED_ARSENAL.description", "Whenever a [gold]Shiv[/gold] enters your [gold]hand[/gold], apply {Poison} [gold]Poison[/gold] to a random enemy. Whenever a card Exhausts, apply {ExhaustPoison} [gold]Poison[/gold] to ALL enemies." },
            { "PHANTOM_RICOCHET.title", "Phantom Ricochet" },
            { "PHANTOM_RICOCHET.description", "Deal {Damage:diff()} damage. Increases damage by 1 for each card discarded this combat." },
            { "WELL_POISONER.title", "Well Poisoner" },
            { "WELL_POISONER.description", "Whenever you receive a Debuff from an enemy, apply {Poison:diff()} [gold]Poison[/gold] to that enemy." },
            { "SHADOW_DOUBLE.title", "Shadow Double" },
            { "SHADOW_DOUBLE.description", "The next Attack you play this turn is played twice. At the start of your next turn, discard a random card." },
            { "SLEEVE_ARSENAL.title", "Sleeve Arsenal" },
            { "SLEEVE_ARSENAL.description", "Add 2 [gold]Shivs[/gold] into your hand (4 if you have 6 or more cards in hand). If you have 6 or more cards, gain {Accuracy:diff()} [gold]Accuracy[/gold]." },
            { "CATALYTIC_OUTBREAK.title", "Catalytic Outbreak" },
            { "CATALYTIC_OUTBREAK.description", "Remove all [gold]Poison[/gold] from target enemy and distribute 125% (150%) of it among ALL living enemies." },
            { "DEADLY_GAMBIT.title", "Deadly Gambit" },
            { "DEADLY_GAMBIT.description", "Discard all other cards in hand. For each discarded card, deal {Damage:diff()} damage and apply {Poison} [gold]Poison[/gold] to a random enemy." },
            { "SMOKESCREEN_FLURRY.title", "Smokescreen Flurry" },
            { "SMOKESCREEN_FLURRY.description", "Gain {Block:diff()} [gold]Block[/gold]. All enemies gain 2 [gold]Weak[/gold]. Next turn, start with 1 additional [gold]Energy[/gold] and draw 1 additional card. [gold]Exhaust[/gold]." },
            { "GLASS_STILETTO.title", "Glass Stiletto" },
            { "GLASS_STILETTO.description", "Deal {Damage:diff()} damage. Add {Cards} [gold]Glass Shards[/gold] into your hand." },
            { "GLASS_SHARD.title", "Glass Shard" },
            { "GLASS_SHARD.description", "Deal {Damage:diff()} damage. [gold]Exhaust[/gold]." },
            { "NEUROTOXIN_FLASH.title", "Neurotoxin Flash" },
            { "NEUROTOXIN_FLASH.description", "Deal {Damage:diff()} damage. Convert all [gold]Poison[/gold] on target into direct damage, then re-apply 50% of that [gold]Poison[/gold] and apply 2 [gold]Weak[/gold]." },

            // --- Necrobinder ---
            { "SKELETON_ARMY.title", "Skeleton Army" },
            { "SKELETON_ARMY.description", "Summon {SkeletonCount} mini-skeletons with 1 HP each. Whenever Osty attacks, they duplicate the strike for {DmgPercent}% of its damage. Perish if Osty dies. [gold]Exhaust[/gold]." },
            { "SOUL_HARVEST.title", "Soul Harvest" },
            { "SOUL_HARVEST.description", "Deal {Damage:diff()}*X damage. Grant Osty Max HP equal to unblocked damage dealt to enemy HP." },
            { "DOOM_SHROUD.title", "Doom Shroud" },
            { "DOOM_SHROUD.description", "Gain [gold]Block[/gold] equal to [gold]Doom[/gold] on target enemy. [gold]Exhaust[/gold]." },
            { "BONE_PURGE.title", "Bone Purge" },
            { "BONE_PURGE.description", "Destroy Osty. Remove ALL debuffs from yourself." },
            { "SHIFT_BURDEN.title", "Shift Burden" },
            { "SHIFT_BURDEN.description", "Transfer ALL your debuffs onto target ally. [gold]Exhaust[/gold]." },
            { "HEX_REFLECTION.title", "Hex Reflection" },
            { "HEX_REFLECTION.description", "Transfer ALL your debuffs onto target enemy. [gold]Exhaust[/gold]." },
            { "SOUL_TRANSFUSION.title", "Soul Transfusion" },
            { "SOUL_TRANSFUSION.description", "Costs {CostReduction} less whenever any creature dies this combat.\nLose half your current HP: heal or revive target ally for that amount. [gold]Exhaust[/gold]." },
            { "NECROTIC_TETHER.title", "Necrotic Tether" },
            { "NECROTIC_TETHER.description", "Apply Necrotic Tether for {Turns:diff()} turns. Whenever the player or Osty takes damage, 50% of that damage is dealt to this enemy." },
            { "HARVEST_OF_MALICE.title", "Harvest of Malice" },
            { "HARVEST_OF_MALICE.description", "Deal damage to ALL enemies equal to the total [gold]Doom[/gold] across all enemies plus {Damage:diff()}." },
            { "OSSUARY_FUSION.title", "Ossuary Fusion" },
            { "OSSUARY_FUSION.description", "Sacrifice Osty. Gain {Block:diff()} [gold]Block[/gold]. At the start of your next turn, summon a reinforced Osty with 20 Max HP." },
            { "CHARONS_OBOL.title", "Charon's Obol" },
            { "CHARONS_OBOL.description", "Gain {EnergyGain:diff()} [gold]Energy[/gold]. Add a [gold]Charon's Debt[/gold] into your discard pile." },
            { "CHARONS_DEBT.title", "Charon's Debt" },
            { "CHARONS_DEBT.description", "Retain. Exhaust. At the end of your turn while in hand: take 4 damage. When played: gain {Block:diff()} [gold]Block[/gold]." },
            { "PYRE_OF_THE_DAMNED.title", "Pyre of the Damned" },
            { "PYRE_OF_THE_DAMNED.description", "Exhaust up to 3 cards from your hand. For each card Exhausted, deal {Damage:diff()} damage and apply {Doom:diff()} [gold]Doom[/gold] to ALL enemies." },
            { "DEATH_ECHO.title", "Death Echo" },
            { "DEATH_ECHO.description", "Whenever any creature dies, gain 1 Energy and draw 1 card." },
            { "SHROUD_TEAR.title", "Shroud Tear" },
            { "SHROUD_TEAR.description", "Deal {Damage:diff()} damage. If target has [gold]Doom[/gold], double the [gold]Doom[/gold] on target." },
            { "ESSENCE_SIPHON.title", "Essence Siphon" },
            { "ESSENCE_SIPHON.description", "Target enemy loses {Strength:diff()} [gold]Strength[/gold]. You gain {Strength:diff()} [gold]Strength[/gold]." },
            { "WRAITH_STEP.title", "Wraith Step" },
            { "WRAITH_STEP.description", "Gain 1 Intangible. At the start of your next turn, gain 3 Doom on yourself. Exhaust." },
            { "SIGIL_OF_DECAY.title", "Sigil of Decay" },
            { "SIGIL_OF_DECAY.description", "At the end of your turn, all enemies take damage equal to their Doom count." },

            // --- Defect ---
            { "OVERCLOCK_FOCUS.title", "Overclock Focus" },
            { "OVERCLOCK_FOCUS.description", "Gain X [gold]Focus[/gold]. [gold]Exhaust[/gold]." },
            { "PLASMA_BEAM.title", "Plasma Beam" },
            { "PLASMA_BEAM.description", "Deal {Damage:diff()} damage to ALL enemies. Consumes 1 [gold]Plasma orb[/gold]. If you have no [gold]Plasma[/gold], does nothing." },
            { "SHORT_CIRCUIT.title", "Short Circuit" },
            { "SHORT_CIRCUIT.description", "Deal {Damage:diff()} damage. Evoke your front orb twice, then remove it from your orb queue." },
            { "NUCLEAR_REACTOR.title", "Nuclear Reactor" },
            { "NUCLEAR_REACTOR.description", "Whenever a Status card is created, gain {Energy:energyIcons()}." },
            { "MATRIX_CAPACITOR.title", "Matrix Capacitor" },
            { "MATRIX_CAPACITOR.description", "Gain {Block:diff()} [gold]Block[/gold]. This turn, whenever an orb is evoked, gain {EvokeBlock:diff()} [gold]Block[/gold]." },
            { "CORE_MELTDOWN.title", "Core Meltdown" },
            { "CORE_MELTDOWN.description", "Deal {Damage:diff()} damage. Permanently lose 1 Orb Slot this combat." },
            { "SUPERCONDUCTOR.title", "Superconductor" },
            { "SUPERCONDUCTOR.description", "This turn, your Orbs trigger their passive abilities 1 additional time." },
            { "QUANTUM_ENTANGLEMENT.title", "Quantum Entanglement" },
            { "QUANTUM_ENTANGLEMENT.description", "Apply Quantum Entanglement for {Turns:diff()} turns. Whenever another enemy takes damage from the player, this enemy takes the same damage." },
            { "PLASMA_OVERDRIVE.title", "Plasma Overdrive" },
            { "PLASMA_OVERDRIVE.description", "Whenever a Plasma Orb is Evoked, gain 1 additional Energy and take 3 damage." },
            { "RECURSIVE_COMPILE.title", "Recursive Compile" },
            { "RECURSIVE_COMPILE.description", "Shuffle all 0-cost cards from your discard pile into your draw pile. Draw {Draw:diff()} cards." },
            { "ABSOLUTE_ZERO.title", "Absolute Zero" },
            { "ABSOLUTE_ZERO.description", "At the end of your turn, apply Weak to ALL enemies equal to your Frost Orbs count. Whenever a Frost Orb is Evoked, gain 12 Block." },
            { "BIOS_PATCH.title", "BIOS Patch" },
            { "BIOS_PATCH.description", "Gain {Block:diff()} [gold]Block[/gold]. Heal {Heal:diff()} HP. Remove Vulnerable and Frail. Exhaust." },
            { "IMPULSE_CAPACITOR.title", "Impulse Capacitor" },
            { "IMPULSE_CAPACITOR.description", "Deal {Damage:diff()} damage. If you have 3 or more Orbs channeled, Evoke your next Orb twice." },
            { "MEMORY_LEAK.title", "Memory Leak" },
            { "MEMORY_LEAK.description", "Retain. At the start of your turn while in hand: draw 1 card, but increase the cost of a random card in hand by 1 until played. When played: gain {Block:diff()} [gold]Block[/gold]. Exhaust." },

            // --- Regent ---
            { "BLADE_ORBIT_RESONANCE.title", "Blade Orbit Resonance" },
            { "BLADE_ORBIT_RESONANCE.description", "Deal {Damage:diff()} damage. [gold]Forge[/gold] Sovereign Blade (+{Forge:diff()}). If Sovereign Blade is in [gold]hand[/gold], trigger a free strike from it." },
            { "GRAVITATIONAL_SINGULARITY.title", "Gravitational Singularity" },
            { "GRAVITATIONAL_SINGULARITY.description", "Until your next turn: {Redirect:diff()}% of damage from target enemy's attacks redirects to other enemies. Gain 1 [gold]Star[/gold] per attack." },
            { "CROWN_TRIBUTE.title", "Crown's Tribute" },
            { "CROWN_TRIBUTE.description", "Mark an enemy for {Repeat:diff()} turns. Attackers heal {Heal} HP and draw {Cards} card, you gain 1 [gold]Star[/gold]. On death, all players gain {Energy:energyIcons()} and {Block} [gold]Block[/gold]." },
            { "ASTRAL_ECLIPSE.title", "Astral Eclipse" },
            { "ASTRAL_ECLIPSE.description", "Costs 2 [gold]Stars[/gold]. Deal {Damage:diff()} damage. Steal 1 [gold]Strength[/gold] and up to {MaxSteal:diff()} [gold]Block[/gold] from target. If target has no [gold]Strength[/gold], apply {Weak} [gold]Weak[/gold] and [gold]Forge[/gold] Sovereign Blade (+{Forge:diff()})." },
            { "SUPERNOVA_CORONATION.title", "Supernova Coronation" },
            { "SUPERNOVA_CORONATION.description", "At the end of your turn: unspent [gold]Stars[/gold] deal {PerStar:diff()} damage each to ALL enemies and grant you half that much [gold]Block[/gold]. Every {StarThreshold} [gold]Stars[/gold] spent removes all enemy [gold]Block[/gold], applies 3 [gold]Vulnerable[/gold], and gives all players {Energy:energyIcons()}." },
            { "ROYAL_EDICT_EXECUTION.title", "Royal Edict: Execution" },
            { "ROYAL_EDICT_EXECUTION.description", "Deal {Damage:diff()} damage. If target has less HP than you, [gold]Forge[/gold] Sovereign Blade (+{Forge:diff()})." },
            { "GRAVITATIONAL_COLLAPSE.title", "Gravitational Collapse" },
            { "GRAVITATIONAL_COLLAPSE.description", "Spend ALL [gold]Stars[/gold]. Deal {PerStarDmg:diff()} damage to ALL enemies and gain {PerStarBlock:diff()} [gold]Block[/gold] per [gold]Star[/gold] spent." },
            { "AEGIS_OF_THE_CROWN.title", "Aegis of the Crown" },
            { "AEGIS_OF_THE_CROWN.description", "Whenever you Forge Sovereign Blade, gain Block equal to the Forge amount." },
            { "BLADE_SUPER_ORBIT.title", "Blade Super Orbit" },
            { "BLADE_SUPER_ORBIT.description", "Trigger Sovereign Blade's damage against ALL enemies. Forge Sovereign Blade by 2." },
            { "WEIGHT_OF_KINGS.title", "Weight of Kings" },
            { "WEIGHT_OF_KINGS.description", "Deal damage equal to your [gold]Block[/gold] plus {BaseBonus:diff()}." },
            { "WORMHOLE_SLASH.title", "Wormhole Slash" },
            { "WORMHOLE_SLASH.description", "Deal {Damage:diff()} damage. Move Sovereign Blade to your hand from your draw or discard pile; it costs 0 this turn." },
            { "ANTIMATTER_PULSE.title", "Antimatter Pulse" },
            { "ANTIMATTER_PULSE.description", "Costs 3 [gold]Stars[/gold]. Remove ALL [gold]Block[/gold] and [gold]Artifact[/gold] from ALL enemies, then apply {Vulnerable:diff()} [gold]Vulnerable[/gold] to all enemies." },
            { "VOID_ASCENSION.title", "Void Ascension" },
            { "VOID_ASCENSION.description", "At the start of your turn, gain 2 Stars. Whenever you play Sovereign Blade while having 6 or more Stars, it is played twice." },
            { "METEOR_BARRAGE.title", "Meteor Barrage" },
            { "METEOR_BARRAGE.description", "Deal {Damage:diff()} damage 3 times. Add 2 [gold]Stardust[/gold] cards to your draw pile." },
            { "COSMIC_STARDUST.title", "Cosmic Stardust" },
            { "COSMIC_STARDUST.description", "Retain. Exhaust. Gain {Stars:diff()} [gold]Star[/gold] and {Block:diff()} [gold]Block[/gold]." },

            // --- Regular Monsters & Encounters ---
            { "CHRONO_WARDEN.name", "Statue of Dionysus" },
            { "CHRONO_WARDEN.title", "Statue of Dionysus" },
            { "CHRONO_WARDEN_ENCOUNTER.name", "Statue of Dionysus" },
            { "CHRONO_WARDEN_ENCOUNTER.title", "Statue of Dionysus" },
            { "CHRONO_WARDEN.moves.CHRONO_1.title", "Smite" },
            { "CHRONO_WARDEN.moves.CHRONO_2.title", "Stone Stance" },
            { "CHRONO_WARDEN.moves.CHRONO_3.title", "Sunder" },
            { "CHRONO_WARDEN.moves.CHRONO_4.title", "Petrify" },
            { "CHRONO_WARDEN.moves.CHRONO_ESCAPE.title", "Ascension" },

            { "MIRROR_PHANTOM.name", "Mirror Phantom" },
            { "MIRROR_PHANTOM.title", "Mirror Phantom" },
            { "MIRROR_PHANTOM_ENCOUNTER.name", "Mirror Phantom" },
            { "MIRROR_PHANTOM_ENCOUNTER.title", "Mirror Phantom" },
            { "MIRROR_PHANTOM.moves.MIRROR_STANCE.title", "Mirror Stance" },
            { "MIRROR_PHANTOM.moves.MIRROR_SHARDS.title", "Shatter Shards" },
            { "MIRROR_PHANTOM.moves.MIRROR_BEAM.title", "Refraction Beam" },
            { "MIRROR_STANCE_POWER.name", "Mirror Stance" },
            { "MIRROR_STANCE_POWER.title", "Mirror Stance" },
            { "MIRROR_STANCE_POWER.description", "Reflects 30% of the damage from the first Attack received back to the attacker." },

            { "CHRONO_ALCHEMIST.name", "Chrono Alchemist" },
            { "CHRONO_ALCHEMIST.title", "Chrono Alchemist" },
            { "CHRONO_ALCHEMIST_ENCOUNTER.name", "Chrono Alchemist" },
            { "CHRONO_ALCHEMIST_ENCOUNTER.title", "Chrono Alchemist" },
            { "CHRONO_ALCHEMIST.moves.ALCHEMIST_TRAP.title", "Sand Trap" },
            { "CHRONO_ALCHEMIST.moves.ALCHEMIST_SHIFT.title", "Chrono Shift" },
            { "CHRONO_ALCHEMIST.moves.ALCHEMIST_TEMPEST.title", "Sand Tempest" },
            { "TIME_DISTORTION_POWER.name", "Time Distortion" },
            { "TIME_DISTORTION_POWER.title", "Time Distortion" },
            { "TIME_DISTORTION_POWER.description", "You may play at most 5 cards per turn." },

            { "SPORE_GOLIATH.name", "Spore Goliath" },
            { "SPORE_GOLIATH.title", "Spore Goliath" },
            { "SPORE_GOLIATH_ENCOUNTER.name", "Spore Goliath" },
            { "SPORE_GOLIATH_ENCOUNTER.title", "Spore Goliath" },
            { "SPORE_GOLIATH.moves.GOLIATH_SLAM.title", "Spore Slam" },
            { "SPORE_GOLIATH.moves.GOLIATH_EMISSION.title", "Toxic Emission" },
            { "SPORE_GOLIATH.moves.GOLIATH_BURST.title", "Spore Burst" },
            { "SPORE_CARD.name", "Mushroom Spores" },
            { "SPORE_CARD.title", "Mushroom Spores" },
            { "SPORE_CARD.description", "Exhaust. Take 2 unblockable damage when played." },

            { "SLAG_CINDER.name", "Slag Cinder" },
            { "SLAG_CINDER.title", "Slag Cinder" },
            { "SLAG_CINDER_ENCOUNTER.name", "Slag Cinder" },
            { "SLAG_CINDER_ENCOUNTER.title", "Slag Cinder" },
            { "SLAG_CINDER.moves.SLAG_HEAT_UP.title", "Molten Furnace" },
            { "SLAG_CINDER.moves.SLAG_HAMMER.title", "Hammer Slam" },
            { "SLAG_CINDER.moves.SLAG_VENT.title", "Vent & Cool" },
            { "SLAG_FURNACE_POWER.name", "Molten Furnace" },
            { "SLAG_FURNACE_POWER.title", "Molten Furnace" },
            { "SLAG_FURNACE_POWER.description", "Whenever the player plays an Attack, they take 1 damage." },

            { "THE_PUPPETEER.name", "The Puppeteer" },
            { "THE_PUPPETEER.title", "The Puppeteer" },
            { "THE_PUPPETEER_ENCOUNTER.name", "The Puppeteer" },
            { "THE_PUPPETEER_ENCOUNTER.title", "The Puppeteer" },
            { "THE_PUPPETEER.moves.PUPPET_STRINGS.title", "Puppet Strings" },
            { "THE_PUPPETEER.moves.PUPPET_NEEDLE.title", "Puppet Needle" },
            { "THE_PUPPETEER.moves.PUPPET_DANCE.title", "Danse Macabre" },
            { "PUPPET_MINION.name", "Puppet Minion" },
            { "PUPPET_MINION.title", "Puppet Minion" },
            { "PUPPET_MINION.moves.MINION_GUARD.title", "Wooden Guard" },
            { "PUPPET_MINION.moves.MINION_STRIKE.title", "Puppet Strike" },
            { "PUPPET_BONDS_POWER.name", "Puppet Bonds" },
            { "PUPPET_BONDS_POWER.title", "Puppet Bonds" },
            { "PUPPET_BONDS_POWER.description", "While a Puppet is alive, The Puppeteer takes 30% less damage. Enrages (+2 Strength) when the puppet dies." },

            // --- Act 1 Bosses ---
            { "CLOCKWORK_COLOSSUS.name", "Clockwork Colossus" },
            { "CLOCKWORK_COLOSSUS.title", "Clockwork Colossus" },
            { "CLOCKWORK_COLOSSUS_BOSS.name", "Clockwork Colossus" },
            { "CLOCKWORK_COLOSSUS_BOSS.title", "Clockwork Colossus" },
            { "CLOCKWORK_COLOSSUS.moves.COLOSSUS_WINDUP1.title", "Wind-Up Alpha" },
            { "CLOCKWORK_COLOSSUS.moves.COLOSSUS_STEAM.title", "Steam Vent" },
            { "CLOCKWORK_COLOSSUS.moves.COLOSSUS_WINDUP2.title", "Wind-Up Beta" },
            { "CLOCKWORK_COLOSSUS.moves.COLOSSUS_OVERDRIVE.title", "Overdrive Flurry" },
            { "CLOCKWORK_COLOSSUS.moves.COLOSSUS_MALFUNCTION.title", "Malfunction" },
            { "TENSION_POWER.name", "Spring Tension" },
            { "TENSION_POWER.title", "Spring Tension" },
            { "TENSION_POWER.description", "Accumulates tension. At max tension, transitions into Overclock Overdrive!" },

            { "MYCELIAL_BROODMOTHER.name", "Mycelial Broodmother" },
            { "MYCELIAL_BROODMOTHER.title", "Mycelial Broodmother" },
            { "MYCELIAL_BROODMOTHER_BOSS.name", "Mycelial Broodmother" },
            { "MYCELIAL_BROODMOTHER_BOSS.title", "Mycelial Broodmother" },
            { "MYCELIAL_BROODMOTHER.moves.BROOD_NURSERY.title", "Spore Nursery" },
            { "MYCELIAL_BROODMOTHER.moves.BROOD_LASH.title", "Hyphal Lash" },
            { "MYCELIAL_BROODMOTHER.moves.BROOD_ERUPTION.title", "Spore Eruption" },
            { "MYCELIAL_BROODMOTHER.moves.BROOD_SPIKE.title", "Chitin Spike" },
            { "SPORE_COCOON.name", "Spore Cocoon" },
            { "SPORE_COCOON.title", "Spore Cocoon" },
            { "SPORE_COCOON.moves.COCOON_PULSATE.title", "Pulsate" },
            { "COCOON_POWER.name", "Incubation" },
            { "COCOON_POWER.title", "Incubation" },
            { "COCOON_POWER.description", "Ticks down each turn. When it reaches 0, explodes for 12 unblockable damage to all players and adds 2 Mushroom Spores to their discard piles." },

            { "SANGUINE_INQUISITOR.name", "Sanguine Inquisitor" },
            { "SANGUINE_INQUISITOR.title", "Sanguine Inquisitor" },
            { "SANGUINE_INQUISITOR_BOSS.name", "Sanguine Inquisitor" },
            { "SANGUINE_INQUISITOR_BOSS.title", "Sanguine Inquisitor" },
            { "SANGUINE_INQUISITOR.moves.INQ_BRAND.title", "Sanguine Brand" },
            { "SANGUINE_INQUISITOR.moves.INQ_FLAGELLATION.title", "Flagellation" },
            { "SANGUINE_INQUISITOR.moves.INQ_JUDGMENT.title", "Crimson Judgment" },
            { "SANGUINE_INQUISITOR.moves.INQ_PURIFYING.title", "Purifying Rite" },
            { "MARTYRDOM_POWER.name", "Sanguine Zeal" },
            { "MARTYRDOM_POWER.title", "Sanguine Zeal" },
            { "MARTYRDOM_POWER.description", "Gains permanent Strength when losing HP below 75%, 50%, and 25%." },

            // --- Act 2 Bosses ---
            { "MIRROR_CLONE.name", "Mirror Clone" },
            { "MIRROR_CLONE.title", "Mirror Clone" },
            { "MIRROR_CLONE.moves.CLONE_SPLINTER.title", "Splinter" },
            { "MIRROR_CLONE.moves.CLONE_REFRACT.title", "Refract" },
            { "MIRROR_REFLECTION_POWER.name", "Mirror Reflection" },
            { "MIRROR_REFLECTION_POWER.title", "Mirror Reflection" },
            { "MIRROR_REFLECTION_POWER.description", "Reflects 100% of received attack damage back to the attacker." },
            { "MIRRORED_SOVEREIGN.name", "The Mirrored Sovereign" },
            { "MIRRORED_SOVEREIGN.title", "The Mirrored Sovereign" },
            { "MIRRORED_SOVEREIGN_BOSS.name", "The Mirrored Sovereign" },
            { "MIRRORED_SOVEREIGN_BOSS.title", "The Mirrored Sovereign" },
            { "MIRRORED_SOVEREIGN.moves.SOVEREIGN_WARD.title", "Prismatic Barrier" },
            { "MIRRORED_SOVEREIGN.moves.SOVEREIGN_BEAM.title", "Refraction Beam" },
            { "MIRRORED_SOVEREIGN.moves.SOVEREIGN_SPLINTER.title", "Glass Splinters" },
            { "MIRRORED_SOVEREIGN.moves.SOVEREIGN_RESONATOR.title", "Sovereign Echo" },
            { "PRISMATIC_WARD_POWER.name", "Prismatic Reflection" },
            { "PRISMATIC_WARD_POWER.title", "Prismatic Reflection" },
            { "PRISMATIC_WARD_POWER.description", "Reduces incoming damage by 50% while Mirror Clones are alive, and retaliates with 25% splinter damage." },

            { "ALCHEMICAL_CHIMERA.name", "The Alchemical Chimera" },
            { "ALCHEMICAL_CHIMERA.title", "The Alchemical Chimera" },
            { "ALCHEMICAL_CHIMERA_BOSS.name", "The Alchemical Chimera" },
            { "ALCHEMICAL_CHIMERA_BOSS.title", "The Alchemical Chimera" },
            { "ALCHEMICAL_CHIMERA.moves.CHIMERA_ACID1.title", "Caustic Saliva" },
            { "ALCHEMICAL_CHIMERA.moves.CHIMERA_ACID2.title", "Acidic Torrent" },
            { "ALCHEMICAL_CHIMERA.moves.CHIMERA_FIRE1.title", "Molten Breath" },
            { "ALCHEMICAL_CHIMERA.moves.CHIMERA_FIRE2.title", "Ignition Rush" },
            { "ALCHEMICAL_CHIMERA.moves.CHIMERA_CRYO1.title", "Glacial Roar" },
            { "ALCHEMICAL_CHIMERA.moves.CHIMERA_CRYO2.title", "Cryo Pounce" },
            { "CHIMERA_TRANSMUTATION_POWER.name", "Alchemical Transmutation" },
            { "CHIMERA_TRANSMUTATION_POWER.title", "Alchemical Transmutation" },
            { "CHIMERA_TRANSMUTATION_POWER.description", "Cycles between Acid, Flame, and Cryo reagents." },

            { "NULL_EXECUTIONER.name", "The Null Executioner" },
            { "NULL_EXECUTIONER.title", "The Null Executioner" },
            { "NULL_EXECUTIONER_BOSS.name", "The Null Executioner" },
            { "NULL_EXECUTIONER_BOSS.title", "The Null Executioner" },
            { "NULL_EXECUTIONER.moves.EXEC_EDICT_ATK.title", "Silence Attacks" },
            { "NULL_EXECUTIONER.moves.EXEC_CLEAVE.title", "Executioner's Cleave" },
            { "NULL_EXECUTIONER.moves.EXEC_EDICT_SKILL.title", "Silence Skills" },
            { "NULL_EXECUTIONER.moves.EXEC_DECAPITATE.title", "Decapitate" },
            { "NULL_EDICT_POWER.name", "Null Edict" },
            { "NULL_EDICT_POWER.title", "Null Edict" },
            { "NULL_EDICT_POWER.description", "Forbids playing either Attacks or Skills this turn. Playing forbidden cards deals 6 unblockable damage to you and gives Executioner +1 Strength." },
            { "NULL_EDICT_POWER_ATK.description", "Forbidden to play [gold]Attacks[/gold] this turn! Playing Attacks deals 6 unblockable damage to you and gives Executioner +1 Strength." },
            { "NULL_EDICT_POWER_SKILL.description", "Forbidden to play [gold]Skills[/gold] this turn! Playing Skills deals 6 unblockable damage to you and gives Executioner +1 Strength." },

            { "RUNIC_CUBE.name", "The Runic Cube" },
            { "RUNIC_CUBE.title", "The Runic Cube" },
            { "RUNIC_CUBE_BOSS.name", "The Runic Cube" },
            { "RUNIC_CUBE_BOSS.title", "The Runic Cube" },
            { "RUNIC_CUBE.moves.RUNIC_BASTION.title", "Runic Bastion" },
            { "RUNIC_CUBE.moves.BLADE_MATRIX.title", "Blade Matrix" },
            { "RUNIC_CUBE.moves.HEX_EXECUTION.title", "Hex Execution" },

            // --- Act 3 Bosses ---
            { "CHRONOS_TITAN.name", "The Chronos Titan" },
            { "CHRONOS_TITAN.title", "The Chronos Titan" },
            { "CHRONOS_TITAN_BOSS.name", "The Chronos Titan" },
            { "CHRONOS_TITAN_BOSS.title", "The Chronos Titan" },
            { "CHRONOS_TITAN.moves.TITAN_STASIS.title", "Temporal Stasis" },
            { "CHRONOS_TITAN.moves.TITAN_RIFT.title", "Chrono Rift" },
            { "CHRONOS_TITAN.moves.TITAN_PARADOX.title", "Grand Paradox" },
            { "CHRONOS_TITAN.moves.TITAN_REVERSAL.title", "Time Reversal" },
            { "CHRONOS_LOOP_POWER.name", "Time Warp" },
            { "CHRONOS_LOOP_POWER.title", "Time Warp" },
            { "CHRONOS_LOOP_POWER.description", "Whenever 6 cards are played in a turn, immediately ends the turn, gains 20 Block, and gains +2 Strength." },

            { "ASTRAL_STAR.name", "Astral Star" },
            { "ASTRAL_STAR.title", "Astral Star" },
            { "ASTRAL_STAR.moves.STAR_PULSE.title", "Stellar Pulse" },
            { "ASTRAL_STAR.moves.STAR_LIGHT.title", "Starlight" },
            { "ASTRAL_DEVOURER.name", "The Astral Devourer" },
            { "ASTRAL_DEVOURER.title", "The Astral Devourer" },
            { "ASTRAL_DEVOURER_BOSS.name", "The Astral Devourer" },
            { "ASTRAL_DEVOURER_BOSS.title", "The Astral Devourer" },
            { "ASTRAL_DEVOURER.moves.ASTRAL_CHARGE.title", "Singularity Charge" },
            { "ASTRAL_DEVOURER.moves.ASTRAL_DEVOUR.title", "Cosmic Devour" },
            { "ASTRAL_DEVOURER.moves.ASTRAL_COLLAPSE.title", "Stellar Collapse" },
            { "ASTRAL_DEVOURER.moves.ASTRAL_HORIZON.title", "Event Horizon" },
            { "COSMIC_HUNGER_POWER.name", "Cosmic Hunger" },
            { "COSMIC_HUNGER_POWER.title", "Cosmic Hunger" },
            { "COSMIC_HUNGER_POWER.description", "Gains +1 Strength at the end of each turn." },

            { "SPIRE_ARCHITECT.name", "The Spire Architect" },
            { "SPIRE_ARCHITECT.title", "The Spire Architect" },
            { "SPIRE_ARCHITECT_BOSS.name", "The Spire Architect" },
            { "SPIRE_ARCHITECT_BOSS.title", "The Spire Architect" },
            { "SPIRE_ARCHITECT.moves.ARCH_FOUNDATION.title", "Spire Foundation" },
            { "SPIRE_ARCHITECT.moves.ARCH_BEAM.title", "Geometric Laser" },
            { "SPIRE_ARCHITECT.moves.ARCH_PILLAR.title", "Monolithic Bastion" },
            { "SPIRE_ARCHITECT.moves.ARCH_COLLAPSE.title", "Cataclysmic Collapse" },
            { "ARCHITECT_GEOMETRY_POWER.name", "Spire Foundations" },
            { "ARCHITECT_GEOMETRY_POWER.title", "Spire Foundations" },
            { "ARCHITECT_GEOMETRY_POWER.description", "Shifts phases as HP decreases: gains 4 Block on hit at >66% HP, inflicts Frail at 33%-66% HP, and enters Total Ruin at <=33% HP (+8 Strength)." },

            // --- Card Powers (Status Bar) ---
            { "DEEP_CONCENTRATION_POWER.name", "Deep Concentration" },
            { "DEEP_CONCENTRATION_POWER.title", "Deep Concentration" },
            { "DEEP_CONCENTRATION_POWER.description", "At the start of your turn: if you played no cards last turn, gain {Amount} Vigor." },

            { "EGOIST_BUFF_POWER.name", "Egoist" },
            { "EGOIST_BUFF_POWER.title", "Egoist" },
            { "EGOIST_BUFF_POWER.description", "This turn, you deal double damage." },

            { "EGOIST_DEBUFF_POWER.name", "Collateral Strain" },
            { "EGOIST_DEBUFF_POWER.title", "Collateral Strain" },
            { "EGOIST_DEBUFF_POWER.description", "Until your next turn, you take double damage." },

            { "TITANIC_BASTION_POWER.name", "Titanic Bastion" },
            { "TITANIC_BASTION_POWER.title", "Titanic Bastion" },
            { "TITANIC_BASTION_POWER.description", "At the start of your next turn: if you still have Block, gain {Amount} Strength." },

            { "IRON_LOAN_POWER.name", "Iron Loan" },
            { "IRON_LOAN_POWER.title", "Iron Loan" },
            { "IRON_LOAN_POWER.description", "At the end of your turn, lose 5 HP for each unspent Energy. Next turn, draw 1 fewer card." },

            { "MARTYRS_BRAND_POWER.name", "Martyr's Brand" },
            { "MARTYRS_BRAND_POWER.title", "Martyr's Brand" },
            { "MARTYRS_BRAND_POWER.description", "For {Amount} turns: whenever you take unblocked attack damage, reflect 100% of that damage back to the attacker." },

            { "SIEGE_STANCE_POWER.name", "Siege Stance" },
            { "SIEGE_STANCE_POWER.title", "Siege Stance" },
            { "SIEGE_STANCE_POWER.description", "You can no longer play Attacks. All Block gained is doubled. At the end of your turn, deal damage to ALL enemies equal to 50% of your Block." },

            { "SECOND_HEART_POWER.name", "Second Heart" },
            { "SECOND_HEART_POWER.title", "Second Heart" },
            { "SECOND_HEART_POWER.description", "The next time you would receive fatal damage: prevent death, heal 25% of your Max HP, gain 4 Strength and 20 Block." },

            { "DIVERT_AND_SCATTER_POWER.name", "Divert & Scatter" },
            { "DIVERT_AND_SCATTER_POWER.title", "Divert & Scatter" },
            { "DIVERT_AND_SCATTER_POWER.description", "Until your next turn: all damage you would take is reduced to 0 and divided evenly among all allies (or enemies if solo)." },

            { "ENVENOMED_ARSENAL_POWER.name", "Envenomed Arsenal" },
            { "ENVENOMED_ARSENAL_POWER.title", "Envenomed Arsenal" },
            { "ENVENOMED_ARSENAL_POWER.description", "Whenever a Shiv enters your hand, apply {Amount} Poison to a random enemy. Whenever a card Exhausts, apply 1 Poison to ALL enemies." },

            { "WELL_POISONER_POWER.name", "Well Poisoner" },
            { "WELL_POISONER_POWER.title", "Well Poisoner" },
            { "WELL_POISONER_POWER.description", "Whenever you receive a debuff from an enemy, apply {Amount} Poison to that enemy in retaliation." },

            { "SHADOW_DOUBLE_POWER.name", "Shadow Double" },
            { "SHADOW_DOUBLE_POWER.title", "Shadow Double" },
            { "SHADOW_DOUBLE_POWER.description", "The next Attack you play this turn is played twice." },

            { "SKELETON_ARMY_POWER.name", "Skeleton Army" },
            { "SKELETON_ARMY_POWER.title", "Skeleton Army" },
            { "SKELETON_ARMY_POWER.description", "Whenever Osty attacks, mini-skeletons duplicate the strike twice for 50% damage each. Perishes if Osty dies." },

            { "NECROTIC_TETHER_POWER.name", "Necrotic Tether" },
            { "NECROTIC_TETHER_POWER.title", "Necrotic Tether" },
            { "NECROTIC_TETHER_POWER.description", "Whenever the player or Osty takes damage, 50% of that damage is dealt to this enemy. Lasts {Amount} turns." },

            { "OSSUARY_FUSION_POWER.name", "Ossuary Fusion" },
            { "OSSUARY_FUSION_POWER.title", "Ossuary Fusion" },
            { "OSSUARY_FUSION_POWER.description", "At the start of your next turn, summon a reinforced Osty with 20 Max HP." },

            { "DEATH_ECHO_POWER.name", "Death Echo" },
            { "DEATH_ECHO_POWER.title", "Death Echo" },
            { "DEATH_ECHO_POWER.description", "Whenever any creature dies, gain 1 Energy and draw 1 card." },

            { "WRAITH_STEP_DOOM_POWER.name", "Wraith Curse" },
            { "WRAITH_STEP_DOOM_POWER.title", "Wraith Curse" },
            { "WRAITH_STEP_DOOM_POWER.description", "At the start of your next turn, gain 3 Doom on yourself." },

            { "SIGIL_OF_DECAY_POWER.name", "Sigil of Decay" },
            { "SIGIL_OF_DECAY_POWER.title", "Sigil of Decay" },
            { "SIGIL_OF_DECAY_POWER.description", "At the end of your turn, all enemies take unblockable damage equal to their Doom." },

            { "NUCLEAR_REACTOR_POWER.name", "Nuclear Reactor" },
            { "NUCLEAR_REACTOR_POWER.title", "Nuclear Reactor" },
            { "NUCLEAR_REACTOR_POWER.description", "Whenever a Status card is generated during combat, gain 1 Energy." },

            { "MATRIX_CAPACITOR_POWER.name", "Matrix Capacitor" },
            { "MATRIX_CAPACITOR_POWER.title", "Matrix Capacitor" },
            { "MATRIX_CAPACITOR_POWER.description", "This turn, whenever an Orb is evoked, gain {Amount} Block." },

            { "SUPERCONDUCTOR_POWER.name", "Superconductor" },
            { "SUPERCONDUCTOR_POWER.title", "Superconductor" },
            { "SUPERCONDUCTOR_POWER.description", "This turn, your Orbs trigger their passive abilities 1 additional time." },

            { "QUANTUM_ENTANGLEMENT_POWER.name", "Quantum Entanglement" },
            { "QUANTUM_ENTANGLEMENT_POWER.title", "Quantum Entanglement" },
            { "QUANTUM_ENTANGLEMENT_POWER.description", "Whenever another enemy takes damage from the player, this enemy takes the same damage. Lasts {Amount} turns." },

            { "PLASMA_OVERDRIVE_POWER.name", "Plasma Overdrive" },
            { "PLASMA_OVERDRIVE_POWER.title", "Plasma Overdrive" },
            { "PLASMA_OVERDRIVE_POWER.description", "Whenever a Plasma Orb is Evoked, gain 1 additional Energy and take 3 damage." },

            { "ABSOLUTE_ZERO_POWER.name", "Absolute Zero" },
            { "ABSOLUTE_ZERO_POWER.title", "Absolute Zero" },
            { "ABSOLUTE_ZERO_POWER.description", "At the end of your turn, apply Weak to ALL enemies equal to your Frost Orbs count. Whenever a Frost Orb is Evoked, gain 12 Block." },

            { "GRAVITATIONAL_SINGULARITY_POWER.name", "Gravitational Singularity" },
            { "GRAVITATIONAL_SINGULARITY_POWER.title", "Gravitational Singularity" },
            { "GRAVITATIONAL_SINGULARITY_POWER.description", "This enemy redirects {Amount}% of damage dealt from its attacks to other enemies. You gain 1 Star per attack." },

            { "CROWN_TRIBUTE_POWER.name", "Crown's Tribute" },
            { "CROWN_TRIBUTE_POWER.title", "Crown's Tribute" },
            { "CROWN_TRIBUTE_POWER.description", "Marked for {Amount} turns. Attackers heal 2 HP, draw 1 card, and grant 1 Star. On death, all players gain 1 Energy and 10 Block." },

            { "SUPERNOVA_CORONATION_POWER.name", "Supernova Coronation" },
            { "SUPERNOVA_CORONATION_POWER.title", "Supernova Coronation" },
            { "SUPERNOVA_CORONATION_POWER.description", "At the end of your turn: unspent Stars deal {Amount} damage each to ALL enemies and grant half as much Block. Every 6 Stars spent removes all enemy Block, applies 3 Vulnerable, and gives all players 2 Energy." },

            { "AEGIS_OF_THE_CROWN_POWER.name", "Aegis of the Crown" },
            { "AEGIS_OF_THE_CROWN_POWER.title", "Aegis of the Crown" },
            { "AEGIS_OF_THE_CROWN_POWER.description", "Whenever you Forge Sovereign Blade, gain Block equal to the Forge amount." },

            { "VOID_ASCENSION_POWER.name", "Void Ascension" },
            { "VOID_ASCENSION_POWER.title", "Void Ascension" },
            { "VOID_ASCENSION_POWER.description", "At the start of your turn, gain 2 Stars. Whenever you play Sovereign Blade while having 6 or more Stars, it is played twice." },

            { "COCOON_INCUBATION_POWER.name", "Incubation" },
            { "COCOON_INCUBATION_POWER.title", "Incubation" },
            { "COCOON_INCUBATION_POWER.description", "Ticks down each turn. When it reaches 0, explodes for 12 unblockable damage to all players and adds 2 Mushroom Spores to their discard piles." },

            // --- Act 4 Boss: The Corrupted Heart ---
            { "SPIRE_HEART.name", "The Corrupted Heart" },
            { "SPIRE_HEART.title", "The Corrupted Heart" },
            { "SPIRE_HEART_BOSS.name", "The Corrupted Heart" },
            { "SPIRE_HEART_BOSS.title", "The Corrupted Heart" },
            { "SPIRE_HEART.moves.HEART_DEBILITATE.title", "Debilitate" },
            { "SPIRE_HEART.moves.HEART_BLOOD_SHOTS.title", "Blood Shots" },
            { "SPIRE_HEART.moves.HEART_ECHO_SLAM.title", "Echo Slam" },
            { "SPIRE_HEART.moves.HEART_CLEANSING.title", "Cleansing" },
            { "BEAT_OF_DEATH_POWER.name", "Beat of Death" },
            { "BEAT_OF_DEATH_POWER.title", "Beat of Death" },
            { "BEAT_OF_DEATH_POWER.description", "Whenever you play a card, take {Amount} unblockable damage." },
            { "SPIRE_INVINCIBLE_POWER.name", "Invincible" },
            { "SPIRE_INVINCIBLE_POWER.title", "Invincible" },
            { "SPIRE_INVINCIBLE_POWER.description", "Cannot take more than 300 damage per turn. Remaining this turn: {Amount}." },

            // --- Custom Relics ---
            { "CRIMSON_PACT.title", "Crimson Pact" },
            { "CRIMSON_PACT.description", "Whenever you take damage from your own cards or powers, gain 4 Block." },
            { "CRIMSON_PACT.flavor", "A sinister bond inked in fresh arterial blood." },

            { "SOUL_LANTERN.title", "Soul Lantern" },
            { "SOUL_LANTERN.description", "At the start of combat, gain 1 Energy and draw 1 card." },
            { "SOUL_LANTERN.flavor", "Its pale flame hungers for lingering spiritual essence." },

            { "OBSIDIAN_ANVIL.title", "Obsidian Anvil" },
            { "OBSIDIAN_ANVIL.description", "Whenever you play an Upgraded card, deal 3 unblockable damage to ALL enemies." },
            { "OBSIDIAN_ANVIL.flavor", "Forged in crushing depths where standard metal fractures." },

            { "CLOCKWORK_GEAR.title", "Clockwork Gear" },
            { "CLOCKWORK_GEAR.description", "Every 4th Attack you play deals 8 damage to ALL enemies." },
            { "CLOCKWORK_GEAR.flavor", "Ticks with uncanny mechanical exactitude." },

            { "CURSED_AMULET.title", "Cursed Amulet" },
            { "CURSED_AMULET.description", "Boss Relic. Gain 1 Energy at the start of each turn. At the start of combat, gain 2 Doom." },
            { "CURSED_AMULET.flavor", "Incalculable power, burdened by an inescapable dark destiny." },

            { "SPIRE_COMPASS.title", "Spire Compass" },
            { "SPIRE_COMPASS.description", "Shop Relic. Upon pickup, gain 75 Gold. At the start of combat, draw 1 card." },
            { "SPIRE_COMPASS.flavor", "The needle spins wildly, yet always guides to fortune." },

            { "WAR_DRUM.title", "War Drum" },
            { "WAR_DRUM.description", "Ironclad Relic. Whenever you gain Strength, gain 3 Block." },
            { "WAR_DRUM.flavor", "The deafening cadence quickens the pulse of true berserkers." },

            { "VIPER_FANG.title", "Viper Fang" },
            { "VIPER_FANG.description", "Silent Relic. Whenever you deal Attack damage to an enemy with Poison, apply 1 additional Poison." },
            { "VIPER_FANG.flavor", "Dripping with hyper-concentrated neurotoxic secretions." },

            { "CAPACITOR_CORE.title", "Capacitor Core" },
            { "CAPACITOR_CORE.description", "Defect Relic. At the start of combat, Channel 1 Plasma Orb." },
            { "CAPACITOR_CORE.flavor", "A miniaturized unstable singularity humming with clean power." },

            { "GRAVEYARD_SOIL.title", "Graveyard Soil" },
            { "GRAVEYARD_SOIL.description", "Necrobinder Relic. Whenever a card is Exhausted, deal 2 unblockable damage to ALL enemies." },
            { "GRAVEYARD_SOIL.flavor", "Cursed loam saturated with ancient necrotic memories." },

            { "CELESTIAL_PRISM.title", "Celestial Prism" },
            { "CELESTIAL_PRISM.description", "Regent Relic. Whenever you spend Stars, gain 4 Block for each Star spent." },
            { "CELESTIAL_PRISM.flavor", "Fractures celestial starlight into crystalline defensive shields." },

            { "HEART_OF_INFINITY.title", "Heart of Infinity" },
            { "HEART_OF_INFINITY.description", "Boss Relic. At the start of each turn, add a random Shiv, Glass Shard, or Cosmic Stardust to your hand." },
            { "HEART_OF_INFINITY.flavor", "A pulsating core throbbing with shards of manifold realities." }
        };

        private static readonly Dictionary<string, string> Russian = new Dictionary<string, string>
        {
            // --- Ironclad ---
            { "BLOOD_RETRIBUTION.title", "Кровавое возмездие" },
            { "BLOOD_RETRIBUTION.description", "Наносит {Damage:diff()} урона за каждую единицу недостающего ОЗ." },
            { "DEEP_CONCENTRATION.title", "Глубокая концентрация" },
            { "DEEP_CONCENTRATION.description", "В начале вашего хода: если в прошлый ход вы не разыграли ни одной карты, дает {Vigor:diff()} [gold]бодрости[/gold]." },
            { "EGOIST.title", "Эгоист" },
            { "EGOIST.description", "Вы добираете {Cards:diff()} {Cards:plural(ru):карту|карты|карт}.\nВ этом ходу вы наносите в 2 раза больше урона. Все союзники получают в 2 раза больше урона до своего следующего хода. [gold]Сжигается[/gold]." },
            { "SACRIFICIAL_CLEAVE.title", "Жертвенный рассекатель" },
            { "SACRIFICIAL_CLEAVE.description", "Вы теряете {HpLoss} ОЗ.\nНаносит {Damage:diff()} урона ВСЕМ врагам.\nВосстанавливает {Heal:diff()} ОЗ за каждого пораженного врага." },
            { "TITANIC_BASTION.title", "Титанический бастион" },
            { "TITANIC_BASTION.description", "Дает {Block:diff()} [gold]защиты[/gold]. В начале вашего следующего хода: если у вас осталась [gold]защита[/gold], дает {Strength:diff()} [gold]силы[/gold]." },
            { "IRON_LOAN.title", "Железный заём" },
            { "IRON_LOAN.description", "Дает {EnergyGain:diff()} [gold]энергии[/gold] сейчас. В конце хода: вы теряете 5 ОЗ за каждую непотраченную энергию. В следующем ходу: берете на 1 карту меньше." },
            { "MARTYRS_BRAND.title", "Клеймо мученика" },
            { "MARTYRS_BRAND.description", "На {Turns:diff()} {Turns:plural(ru):ход|хода|ходов}: при получении урона от атак врагов отражает 100% полученного урона обратно атакующему." },
            { "ASHEN_REQUIEM.title", "Пепельный реквием" },
            { "ASHEN_REQUIEM.description", "Наносит урон, равный количеству сожжённых карт в этом бою × {Damage:diff()}." },
            { "SIEGE_STANCE.title", "Осадная стойка" },
            { "SIEGE_STANCE.description", "Вы больше не можете разыгрывать Атаки. Весь получаемый Блок удваивается. В конце вашего хода наносит ВСЕМ врагам урон, равный 50% вашего Блока." },
            { "BLOOD_PARITY.title", "Кровавый паритет" },
            { "BLOOD_PARITY.description", "Сравнивает ваше ОЗ и ОЗ цели. Если у цели больше ОЗ — наносит ей разницу (до {MaxCap:diff()}). Если у вас больше — вы теряете разницу и получаете {Strength:diff()} [gold]силы[/gold]. [gold]Сжигается[/gold]." },
            { "RED_HOT_ANVIL.title", "Раскалённая наковальня" },
            { "RED_HOT_ANVIL.description", "Наносит {Damage:diff()} урона. Улучшает случайную карту в руке. Если улучшать нечего — наносит еще {Damage:diff()} урона." },
            { "DOOM_TOLL.title", "Колокол погибели" },
            { "DOOM_TOLL.description", "Стоит X. Сжигает до X карт из руки. За каждую сожжённую карту наносит {Damage:diff()} урона ВСЕМ врагам и дает 1 [gold]силы[/gold]." },
            { "SECOND_HEART.title", "Второе сердце" },
            { "SECOND_HEART.description", "При получении смертельного урона предотвращает гибель, восстанавливает 25% макс. ОЗ, дает 4 Силы и 20 Блока." },
            { "UNSTOPPABLE_RAMPAGE.title", "Неудержимый прорыв" },
            { "UNSTOPPABLE_RAMPAGE.description", "Наносит {Damage:diff()} урона. Если цель погибнет или весь ее Блок снят — добавляет в руку копию этой карты со стоимостью 0 в этом ходу." },
            { "COMBUSTION_PACT.title", "Пепельное пламя" },
            { "COMBUSTION_PACT.description", "Сжигает всю стопку добора. За каждые 3 сожжённые карты наносит {Damage:diff()} урона ВСЕМ врагам и дает 1 [gold]энергию[/gold]. [gold]Сжигается[/gold]." },

            // --- Silent ---
            { "DIVERT_AND_SCATTER.title", "Отвод и рассеивание" },
            { "DIVERT_AND_SCATTER.description", "До вашего следующего хода: весь получаемый вами урон снижается до 0 и поровну распределяется между остальными союзниками." },
            { "DANCE_OF_POISON_BLADES.title", "Танец ядовитых клинков" },
            { "DANCE_OF_POISON_BLADES.description", "Добавляет {Cards:diff()} [gold]«Ядовитые заточки»[/gold] в [gold]руку[/gold]." },
            { "POISON_SHIV.title", "Ядовитая заточка" },
            { "POISON_SHIV.description", "Наносит {Damage:diff()} урона.\nНакладывает {Poison:diff()} [gold]яда[/gold].\n[gold]Сжигается[/gold]." },
            { "TOXIC_RICOCHET.title", "Токсичный рикошет" },
            { "TOXIC_RICOCHET.description", "Наносит {Damage:diff()} урона. Если цель отравлена, наносит всем остальным врагам урон, равный ее [gold]яду[/gold]." },
            { "SHADOW_VEIL.title", "Теневой покров" },
            { "SHADOW_VEIL.description", "Дает {Block:diff()} [gold]защиты[/gold] плюс {PerShiv:diff()} [gold]защиты[/gold] за каждую сыгранную в этом бою [gold]«Заточку»[/gold]." },
            { "ENVENOMED_ARSENAL.title", "Отравленный арсенал" },
            { "ENVENOMED_ARSENAL.description", "Каждый раз, когда [gold]«Заточка»[/gold] попадает в [gold]руку[/gold], накладывает {Poison} [gold]яда[/gold] на случайного врага. Каждый раз, когда карта [gold]сжигается[/gold], накладывает {ExhaustPoison} [gold]яда[/gold] на ВСЕХ врагов." },
            { "PHANTOM_RICOCHET.title", "Призрачный рикошет" },
            { "PHANTOM_RICOCHET.description", "Наносит {Damage:diff()} урона. Увеличивает урон на 1 за каждую сброшенную в этом бою карту." },
            { "WELL_POISONER.title", "Отравитель колодца" },
            { "WELL_POISONER.description", "Каждый раз, когда вы получаете отрицательный эффект от врага, накладывает {Poison:diff()} [gold]яда[/gold] на этого врага." },
            { "SHADOW_DOUBLE.title", "Теневой двойник" },
            { "SHADOW_DOUBLE.description", "Следующая сыгранная в этом ходу Атака разыгрывается дважды. В начале следующего хода сбросьте случайную карту." },
            { "SLEEVE_ARSENAL.title", "Арсенал в рукаве" },
            { "SLEEVE_ARSENAL.description", "Добавляет 2 [gold]«Заточки»[/gold] в руку (4, если в руке 6+ карт). Если в руке 6+ карт, дает {Accuracy:diff()} [gold]Точности[/gold]." },
            { "CATALYTIC_OUTBREAK.title", "Каталитическая вспышка" },
            { "CATALYTIC_OUTBREAK.description", "Снимает весь [gold]Яд[/gold] с выбранного врага и поровну распределяет 125% (150%) этого яда между ВСЕМИ живыми врагами." },
            { "DEADLY_GAMBIT.title", "Смертельный гамбит" },
            { "DEADLY_GAMBIT.description", "Сбросьте все остальные карты из руки. За каждую сброшенную карту наносит {Damage:diff()} урона и накладывает {Poison} [gold]яда[/gold] на случайного врага." },
            { "SMOKESCREEN_FLURRY.title", "Дымовой шквал" },
            { "SMOKESCREEN_FLURRY.description", "Дает {Block:diff()} [gold]защиты[/gold]. Все враги получают 2 [gold]слабости[/gold]. В следующем ходу: +1 [gold]энергия[/gold] и добор 1 карты. [gold]Сжигается[/gold]." },
            { "GLASS_STILETTO.title", "Стеклянный стилет" },
            { "GLASS_STILETTO.description", "Наносит {Damage:diff()} урона. Добавляет {Cards} [gold]«Стеклянных осколка»[/gold] в руку." },
            { "GLASS_SHARD.title", "Стеклянный осколок" },
            { "GLASS_SHARD.description", "Наносит {Damage:diff()} урона. [gold]Сжигается[/gold]." },
            { "NEUROTOXIN_FLASH.title", "Вспышка нейротоксина" },
            { "NEUROTOXIN_FLASH.description", "Наносит {Damage:diff()} урона. Превращает весь [gold]Яд[/gold] на цели в мгновенный урон, затем повторно накладывает 50% этого [gold]Яда[/gold] и дает 2 [gold]слабости[/gold]." },

            // --- Necrobinder ---
            { "SKELETON_ARMY.title", "Армия скелетов" },
            { "SKELETON_ARMY.description", "Призывает {SkeletonCount} мини-скелетов с 1 ОЗ каждый. При каждой атаке Ости они повторяют ее с {DmgPercent}% его урона. Погибают вместе с Ости. [gold]Сжигается[/gold]." },
            { "SOUL_HARVEST.title", "Жатва душ" },
            { "SOUL_HARVEST.description", "Наносит {Damage:diff()}*X урона. Увеличивает максимальное ОЗ Ости (или призывает его) на величину незаблокированного урона по ОЗ цели." },
            { "DOOM_SHROUD.title", "Саван рока" },
            { "DOOM_SHROUD.description", "Дает [gold]защиту[/gold], равную количеству [gold]злого рока[/gold] на выбранном враге. [gold]Сжигается[/gold]." },
            { "BONE_PURGE.title", "Очищение костей" },
            { "BONE_PURGE.description", "Уничтожает Ости. Снимает с вас ВСЕ отрицательные эффекты." },
            { "SHIFT_BURDEN.title", "Перекладывание ноши" },
            { "SHIFT_BURDEN.description", "Переносит ВСЕ ваши отрицательные эффекты на выбранного союзника. [gold]Сжигается[/gold]." },
            { "HEX_REFLECTION.title", "Отражение проклятия" },
            { "HEX_REFLECTION.description", "Переносит ВСЕ ваши отрицательные эффекты на выбранного врага. [gold]Сжигается[/gold]." },
            { "SOUL_TRANSFUSION.title", "Переливание души" },
            { "SOUL_TRANSFUSION.description", "Стоит на {CostReduction} меньше за каждое погибшее в этом бою существо.\nПотеряйте половину ОЗ: исцеляет или воскрешает выбранного союзника на это же значение. [gold]Сжигается[/gold]." },
            { "NECROTIC_TETHER.title", "Некротическая привязь" },
            { "NECROTIC_TETHER.description", "Накладывает Некротическую привязь на {Turns:diff()} {Turns:plural(ru):ход|хода|ходов}. Каждый раз, когда игрок или Ости получают урон, 50% этого урона наносится этому врагу." },
            { "HARVEST_OF_MALICE.title", "Жатва злобы" },
            { "HARVEST_OF_MALICE.description", "Наносит ВСЕМ врагам урон, равный суммарному [gold]Злому року[/gold] на всех врагах плюс {Damage:diff()}." },
            { "OSSUARY_FUSION.title", "Слияние склепа" },
            { "OSSUARY_FUSION.description", "Жертвует Ости. Дает {Block:diff()} [gold]защиты[/gold]. В начале следующего хода призывает усиленного Ости с 20 макс. ОЗ." },
            { "CHARONS_OBOL.title", "Обол Харона" },
            { "CHARONS_OBOL.description", "Дает {EnergyGain:diff()} [gold]энергии[/gold]. Добавляет [gold]«Долг Харона»[/gold] в ваш сброс." },
            { "CHARONS_DEBT.title", "Долг Харона" },
            { "CHARONS_DEBT.description", "Сохраняется. [gold]Сжигается[/gold]. В конце хода, пока в руке: вы получаете 4 урона. При розыгрыше: дает {Block:diff()} [gold]защиты[/gold]." },
            { "PYRE_OF_THE_DAMNED.title", "Костёр проклятых" },
            { "PYRE_OF_THE_DAMNED.description", "Сжигает до 3 карт из руки. За каждую сожжённую карту наносит {Damage:diff()} урона и накладывает {Doom:diff()} [gold]Злого рока[/gold] ВСЕМ врагам." },
            { "DEATH_ECHO.title", "Эхо смерти" },
            { "DEATH_ECHO.description", "Каждый раз, когда погибает любое существо, дает 1 Энергию и берет 1 карту." },
            { "SHROUD_TEAR.title", "Разрыв савана" },
            { "SHROUD_TEAR.description", "Наносит {Damage:diff()} урона. Если у цели есть [gold]Злой рок[/gold], удваивает его значение." },
            { "ESSENCE_SIPHON.title", "Вытягивание сущности" },
            { "ESSENCE_SIPHON.description", "Выбранный враг теряет {Strength:diff()} [gold]силы[/gold]. Вы получаете {Strength:diff()} [gold]силы[/gold]." },
            { "WRAITH_STEP.title", "Призрачный шаг" },
            { "WRAITH_STEP.description", "Дает 1 [gold]Неосязаемость[/gold]. В начале следующего хода вы получаете 3 [gold]Злого рока[/gold]. [gold]Сжигается[/gold]." },
            { "SIGIL_OF_DECAY.title", "Символ тления" },
            { "SIGIL_OF_DECAY.description", "В конце вашего хода все враги получают урон, равный их [gold]Злому року[/gold]." },

            // --- Defect ---
            { "OVERCLOCK_FOCUS.title", "Разгон фокуса" },
            { "OVERCLOCK_FOCUS.description", "Дает X [gold]фокуса[/gold]. [gold]Сжигается[/gold]." },
            { "PLASMA_BEAM.title", "Плазмолуч" },
            { "PLASMA_BEAM.description", "Наносит {Damage:diff()} урона ВСЕМ врагам. Поглощает 1 [gold]сферу плазмы[/gold]. Если у вас нет [gold]сферы плазмы[/gold], ничего не происходит." },
            { "SHORT_CIRCUIT.title", "Короткое замыкание" },
            { "SHORT_CIRCUIT.description", "Наносит {Damage:diff()} урона. [gold]Активирует[/gold] переднюю сферу дважды, затем удаляет ее из очереди сфер." },
            { "NUCLEAR_REACTOR.title", "Ядерная энергетика" },
            { "NUCLEAR_REACTOR.description", "Каждый раз, когда создается [gold]карта-статус[/gold], дает {Energy:energyIcons()}." },
            { "MATRIX_CAPACITOR.title", "Матричный конденсатор" },
            { "MATRIX_CAPACITOR.description", "Дает {Block:diff()} [gold]защиты[/gold]. В этом ходу каждый раз при [gold]активации[/gold] сферы дает {EvokeBlock:diff()} [gold]защиты[/gold]." },
            { "CORE_MELTDOWN.title", "Расплавление ядра" },
            { "CORE_MELTDOWN.description", "Наносит {Damage:diff()} урона. Безвозвратно теряет 1 ячейку сфер в этом бою." },
            { "SUPERCONDUCTOR.title", "Сверхпроводник" },
            { "SUPERCONDUCTOR.description", "В этом ходу пассивные эффекты ваших сфер срабатывают на 1 раз больше." },
            { "QUANTUM_ENTANGLEMENT.title", "Квантовая запутанность" },
            { "QUANTUM_ENTANGLEMENT.description", "Накладывает Квантовую запутанность на {Turns:diff()} {Turns:plural(ru):ход|хода|ходов}. Каждый раз, когда другой враг получает урон от игрока, эта цель получает такой же урон." },
            { "PLASMA_OVERDRIVE.title", "Плазменный перегруз" },
            { "PLASMA_OVERDRIVE.description", "Каждый раз при [gold]активации[/gold] Сферы плазмы дает 1 дополнительную Энергию и наносит вам 3 урона." },
            { "RECURSIVE_COMPILE.title", "Рекурсивная компиляция" },
            { "RECURSIVE_COMPILE.description", "Замешивает все карты со стоимостью 0 из сброса в стопку добора. Возьмите {Draw:diff()} {Draw:plural(ru):карту|карты|карт}." },
            { "ABSOLUTE_ZERO.title", "Абсолютный ноль" },
            { "ABSOLUTE_ZERO.description", "В конце вашего хода накладывает [gold]Слабость[/gold] на ВСЕХ врагов по числу Сфер холода. При [gold]активации[/gold] Сферы холода дает 12 Блока." },
            { "BIOS_PATCH.title", "Заплатка BIOS" },
            { "BIOS_PATCH.description", "Дает {Block:diff()} [gold]защиты[/gold]. Восстанавливает {Heal:diff()} ОЗ. Снимает [gold]Уязвимость[/gold] и [gold]Хрупкость[/gold]. [gold]Сжигается[/gold]." },
            { "IMPULSE_CAPACITOR.title", "Импульсный конденсатор" },
            { "IMPULSE_CAPACITOR.description", "Наносит {Damage:diff()} урона. Если у вас 3 или более сфер, [gold]активирует[/gold] переднюю сферу дважды." },
            { "MEMORY_LEAK.title", "Утечка памяти" },
            { "MEMORY_LEAK.description", "Сохраняется. В начале хода, пока в руке: берет 1 карту, но увеличивает стоимость случайной карты в руке на 1 до розыгрыша. При розыгрыше: дает {Block:diff()} [gold]защиты[/gold]. Сжигается." },

            // --- Regent ---
            { "BLADE_ORBIT_RESONANCE.title", "Звездный резонанс клинка" },
            { "BLADE_ORBIT_RESONANCE.description", "Наносит {Damage:diff()} урона. [gold]Кует[/gold] [gold]«Парящий клинок»[/gold] (+{Forge:diff()}). Если [gold]«Парящий клинок»[/gold] в [gold]руке[/gold], совершает им бесплатный удар по цели." },
            { "GRAVITATIONAL_SINGULARITY.title", "Гравитационная сингулярность" },
            { "GRAVITATIONAL_SINGULARITY.description", "До вашего следующего хода: {Redirect:diff()}% урона от атак выбранного врага перенаправляется другим врагам. Дает 1 [gold]звезду[/gold] за атаку." },
            { "CROWN_TRIBUTE.title", "Королевская дань" },
            { "CROWN_TRIBUTE.description", "Накладывает Метку на врага на {Repeat:diff()} {Repeat:plural(ru):ход|хода|ходов}. Атакующие восстанавливают {Heal} ОЗ и добирают {Cards} [gold]карту[/gold], а вы получаете 1 [gold]звезду[/gold]. При гибели цели все союзники получают {Energy:energyIcons()} и {Block} [gold]защиты[/gold]." },
            { "ASTRAL_ECLIPSE.title", "Астральное затмение" },
            { "ASTRAL_ECLIPSE.description", "Стоит 2 [gold]звезды[/gold]. Наносит {Damage:diff()} урона. Крадет 1 [gold]силы[/gold] и до {MaxSteal:diff()} [gold]защиты[/gold] цели. Если у цели нет [gold]силы[/gold], накладывает {Weak} [gold]слабости[/gold] и [gold]кует[/gold] [gold]«Парящий клинок»[/gold] (+{Forge:diff()})." },
            { "SUPERNOVA_CORONATION.title", "Сверхновая коронация" },
            { "SUPERNOVA_CORONATION.description", "В конце вашего хода: неиспользованные [gold]звезды[/gold] наносят по {PerStar:diff()} урона ВСЕМ врагам и дают вам половину этого значения [gold]защиты[/gold]. За каждые {StarThreshold} потраченных [gold]звезд[/gold] снимает всю [gold]защиту[/gold] с врагов, накладывает 3 [gold]уязвимости[/gold] и дает всем союзникам {Energy:energyIcons()}." },
            { "ROYAL_EDICT_EXECUTION.title", "Королевский указ: Казнь" },
            { "ROYAL_EDICT_EXECUTION.description", "Наносит {Damage:diff()} урона. Если у цели меньше ОЗ, чем у вас, [gold]кует[/gold] [gold]«Парящий клинок»[/gold] (+{Forge:diff()})." },
            { "GRAVITATIONAL_COLLAPSE.title", "Гравитационный коллапс" },
            { "GRAVITATIONAL_COLLAPSE.description", "Тратит ВСЕ [gold]звезды[/gold]. Наносит {PerStarDmg:diff()} урона ВСЕМ врагам и дает {PerStarBlock:diff()} [gold]защиты[/gold] за каждую потраченную [gold]звезду[/gold]." },
            { "AEGIS_OF_THE_CROWN.title", "Эгида короны" },
            { "AEGIS_OF_THE_CROWN.description", "Каждый раз, когда вы [gold]куете[/gold] [gold]«Парящий клинок»[/gold], дает [gold]защиту[/gold], равную значению ковки." },
            { "BLADE_SUPER_ORBIT.title", "Сверхорбита клинка" },
            { "BLADE_SUPER_ORBIT.description", "Наносит урон [gold]«Парящего клинка»[/gold] ВСЕМ врагам. [gold]Кует[/gold] [gold]«Парящий клинок»[/gold] (+2)." },
            { "WEIGHT_OF_KINGS.title", "Вес королей" },
            { "WEIGHT_OF_KINGS.description", "Наносит урон, равный вашей [gold]защите[/gold] плюс {BaseBonus:diff()}." },
            { "WORMHOLE_SLASH.title", "Разрез червоточины" },
            { "WORMHOLE_SLASH.description", "Наносит {Damage:diff()} урона. Перемещает [gold]«Парящий клинок»[/gold] в руку из стопки добора или сброса; он стоит 0 в этом ходу." },
            { "ANTIMATTER_PULSE.title", "Антиматериальный импульс" },
            { "ANTIMATTER_PULSE.description", "Стоит 3 [gold]звезды[/gold]. Снимает ВСЮ [gold]защиту[/gold] и [gold]Артефакты[/gold] со ВСЕХ врагов, затем накладывает {Vulnerable:diff()} [gold]уязвимости[/gold] на всех врагов." },
            { "VOID_ASCENSION.title", "Вознесение пустоты" },
            { "VOID_ASCENSION.description", "В начале вашего хода дает 2 [gold]звезды[/gold]. Если у вас 6 или более [gold]звезд[/gold], сыгранный [gold]«Парящий клинок»[/gold] разыгрывается дважды." },
            { "METEOR_BARRAGE.title", "Метеоритный шквал" },
            { "METEOR_BARRAGE.description", "Наносит {Damage:diff()} урона 3 раза. Замешивает 2 карты [gold]«Звёздная пыль»[/gold] в стопку добора." },
            { "COSMIC_STARDUST.title", "Звёздная пыль" },
            { "COSMIC_STARDUST.description", "Сохраняется. [gold]Сжигается[/gold]. Дает {Stars:diff()} [gold]звезду[/gold] и {Block:diff()} [gold]защиты[/gold]." },

            // --- Regular Monsters & Encounters ---
            { "CHRONO_WARDEN.name", "Статуя Диониса" },
            { "CHRONO_WARDEN.title", "Статуя Диониса" },
            { "CHRONO_WARDEN_ENCOUNTER.name", "Статуя Диониса" },
            { "CHRONO_WARDEN_ENCOUNTER.title", "Статуя Диониса" },
            { "CHRONO_WARDEN.moves.CHRONO_1.title", "Сокрушение" },
            { "CHRONO_WARDEN.moves.CHRONO_2.title", "Каменная стойка" },
            { "CHRONO_WARDEN.moves.CHRONO_3.title", "Раскалывание" },
            { "CHRONO_WARDEN.moves.CHRONO_4.title", "Окаменение" },
            { "CHRONO_WARDEN.moves.CHRONO_ESCAPE.title", "Вознесение" },

            { "MIRROR_PHANTOM.name", "Зеркальный Фантом" },
            { "MIRROR_PHANTOM.title", "Зеркальный Фантом" },
            { "MIRROR_PHANTOM_ENCOUNTER.name", "Зеркальный Фантом" },
            { "MIRROR_PHANTOM_ENCOUNTER.title", "Зеркальный Фантом" },
            { "MIRROR_PHANTOM.moves.MIRROR_STANCE.title", "Зеркальная стойка" },
            { "MIRROR_PHANTOM.moves.MIRROR_SHARDS.title", "Осколочный залп" },
            { "MIRROR_PHANTOM.moves.MIRROR_BEAM.title", "Световой рефлекс" },
            { "MIRROR_STANCE_POWER.name", "Зеркальная стойка" },
            { "MIRROR_STANCE_POWER.title", "Зеркальная стойка" },
            { "MIRROR_STANCE_POWER.description", "Отражает 30% урона от первой полученной Атаки обратно в атакующего." },

            { "CHRONO_ALCHEMIST.name", "Песочный Хрономант" },
            { "CHRONO_ALCHEMIST.title", "Песочный Хрономант" },
            { "CHRONO_ALCHEMIST_ENCOUNTER.name", "Песочный Хрономант" },
            { "CHRONO_ALCHEMIST_ENCOUNTER.title", "Песочный Хрономант" },
            { "CHRONO_ALCHEMIST.moves.ALCHEMIST_TRAP.title", "Песчаная ловушка" },
            { "CHRONO_ALCHEMIST.moves.ALCHEMIST_SHIFT.title", "Хроно-сдвиг" },
            { "CHRONO_ALCHEMIST.moves.ALCHEMIST_TEMPEST.title", "Песчаный вихрь" },
            { "TIME_DISTORTION_POWER.name", "Замедление времени" },
            { "TIME_DISTORTION_POWER.title", "Замедление времени" },
            { "TIME_DISTORTION_POWER.description", "Вы можете разыграть не более 5 карт за ход." },

            { "SPORE_GOLIATH.name", "Споровый Симбиот" },
            { "SPORE_GOLIATH.title", "Споровый Симбиот" },
            { "SPORE_GOLIATH_ENCOUNTER.name", "Споровый Симбиот" },
            { "SPORE_GOLIATH_ENCOUNTER.title", "Споровый Симбиот" },
            { "SPORE_GOLIATH.moves.GOLIATH_SLAM.title", "Споровый удар" },
            { "SPORE_GOLIATH.moves.GOLIATH_EMISSION.title", "Токсичный выброс" },
            { "SPORE_GOLIATH.moves.GOLIATH_BURST.title", "Взрыв спор" },
            { "SPORE_CARD.name", "Споры гриба" },
            { "SPORE_CARD.title", "Споры гриба" },
            { "SPORE_CARD.description", "Сжигается. Наносит вам 2 неблокируемого урона при розыгрыше." },

            { "SLAG_CINDER.name", "Шлаковый Автоматон" },
            { "SLAG_CINDER.title", "Шлаковый Автоматон" },
            { "SLAG_CINDER_ENCOUNTER.name", "Шлаковый Автоматон" },
            { "SLAG_CINDER_ENCOUNTER.title", "Шлаковый Автоматон" },
            { "SLAG_CINDER.moves.SLAG_HEAT_UP.title", "Раскалённый корпус" },
            { "SLAG_CINDER.moves.SLAG_HAMMER.title", "Молотовой сокрушитель" },
            { "SLAG_CINDER.moves.SLAG_VENT.title", "Продувка и охлаждение" },
            { "SLAG_FURNACE_POWER.name", "Раскалённый корпус" },
            { "SLAG_FURNACE_POWER.title", "Раскалённый корпус" },
            { "SLAG_FURNACE_POWER.description", "Когда игрок разыгрывает Атаку, он получает 1 урона." },

            { "THE_PUPPETEER.name", "Теневой Марионеточник" },
            { "THE_PUPPETEER.title", "Теневой Марионеточник" },
            { "THE_PUPPETEER_ENCOUNTER.name", "Теневой Марионеточник" },
            { "THE_PUPPETEER_ENCOUNTER.title", "Теневой Марионеточник" },
            { "THE_PUPPETEER.moves.PUPPET_STRINGS.title", "Нити судьбы" },
            { "THE_PUPPETEER.moves.PUPPET_NEEDLE.title", "Игла марионетки" },
            { "THE_PUPPETEER.moves.PUPPET_DANCE.title", "Пляска смерти" },
            { "PUPPET_MINION.name", "Кукла-Марионетка" },
            { "PUPPET_MINION.title", "Кукла-Марионетка" },
            { "PUPPET_MINION.moves.MINION_GUARD.title", "Деревянный блок" },
            { "PUPPET_MINION.moves.MINION_STRIKE.title", "Удар куклы" },
            { "PUPPET_BONDS_POWER.name", "Кукольная связь" },
            { "PUPPET_BONDS_POWER.title", "Кукольная связь" },
            { "PUPPET_BONDS_POWER.description", "Пока жива Марионетка, Кукловод получает на 30% меньше урона. При гибели Марионетки впадает в ярость (+2 Силы)." },

            // --- Act 1 Bosses ---
            { "CLOCKWORK_COLOSSUS.name", "Колосс Часового Механизма" },
            { "CLOCKWORK_COLOSSUS.title", "Колосс Часового Механизма" },
            { "CLOCKWORK_COLOSSUS_BOSS.name", "Колосс Часового Механизма" },
            { "CLOCKWORK_COLOSSUS_BOSS.title", "Колосс Часового Механизма" },
            { "CLOCKWORK_COLOSSUS.moves.COLOSSUS_WINDUP1.title", "Завод механизма I" },
            { "CLOCKWORK_COLOSSUS.moves.COLOSSUS_STEAM.title", "Выброс пара" },
            { "CLOCKWORK_COLOSSUS.moves.COLOSSUS_WINDUP2.title", "Завод механизма II" },
            { "CLOCKWORK_COLOSSUS.moves.COLOSSUS_OVERDRIVE.title", "Шквал сверхразгона" },
            { "CLOCKWORK_COLOSSUS.moves.COLOSSUS_MALFUNCTION.title", "Сбой систем" },
            { "TENSION_POWER.name", "Завод пружины" },
            { "TENSION_POWER.title", "Завод пружины" },
            { "TENSION_POWER.description", "Накапливает напряжение. При максимуме пружины переходит в Сверхразгон!" },

            { "MYCELIAL_BROODMOTHER.name", "Королева Мицелия" },
            { "MYCELIAL_BROODMOTHER.title", "Королева Мицелия" },
            { "MYCELIAL_BROODMOTHER_BOSS.name", "Королева Мицелия" },
            { "MYCELIAL_BROODMOTHER_BOSS.title", "Королева Мицелия" },
            { "MYCELIAL_BROODMOTHER.moves.BROOD_NURSERY.title", "Питомник спор" },
            { "MYCELIAL_BROODMOTHER.moves.BROOD_LASH.title", "Удар гифами" },
            { "MYCELIAL_BROODMOTHER.moves.BROOD_ERUPTION.title", "Извержение спор" },
            { "MYCELIAL_BROODMOTHER.moves.BROOD_SPIKE.title", "Хитиновый шип" },
            { "SPORE_COCOON.name", "Кокон спор" },
            { "SPORE_COCOON.title", "Кокон спор" },
            { "SPORE_COCOON.moves.COCOON_PULSATE.title", "Пульсация" },
            { "COCOON_POWER.name", "Инкубация" },
            { "COCOON_POWER.title", "Инкубация" },
            { "COCOON_POWER.description", "Уменьшается каждый ход. При значении 0 взрывается, нанося 12 неблокируемого урона всем игрокам и добавляя 2 «Споры гриба» в их сброс." },

            { "SANGUINE_INQUISITOR.name", "Кровавый Инквизитор" },
            { "SANGUINE_INQUISITOR.title", "Кровавый Инквизитор" },
            { "SANGUINE_INQUISITOR_BOSS.name", "Кровавый Инквизитор" },
            { "SANGUINE_INQUISITOR_BOSS.title", "Кровавый Инквизитор" },
            { "SANGUINE_INQUISITOR.moves.INQ_BRAND.title", "Кровавое клеймо" },
            { "SANGUINE_INQUISITOR.moves.INQ_FLAGELLATION.title", "Самобичевание" },
            { "SANGUINE_INQUISITOR.moves.INQ_JUDGMENT.title", "Багровый суд" },
            { "SANGUINE_INQUISITOR.moves.INQ_PURIFYING.title", "Очищающий обряд" },
            { "MARTYRDOM_POWER.name", "Кровавое Мученичество" },
            { "MARTYRDOM_POWER.title", "Кровавое Мученичество" },
            { "MARTYRDOM_POWER.description", "Получает постоянную Силу при падении ОЗ ниже 75%, 50% и 25%." },

            // --- Act 2 Bosses ---
            { "MIRROR_CLONE.name", "Зеркальный клон" },
            { "MIRROR_CLONE.title", "Зеркальный клон" },
            { "MIRROR_CLONE.moves.CLONE_SPLINTER.title", "Осколок" },
            { "MIRROR_CLONE.moves.CLONE_REFRACT.title", "Преломление" },
            { "MIRROR_REFLECTION_POWER.name", "Зеркальное Отражение" },
            { "MIRROR_REFLECTION_POWER.title", "Зеркальное Отражение" },
            { "MIRROR_REFLECTION_POWER.description", "Отражает 100% полученного урона атак обратно в атакующего." },
            { "MIRRORED_SOVEREIGN.name", "Зеркальный Государь" },
            { "MIRRORED_SOVEREIGN.title", "Зеркальный Государь" },
            { "MIRRORED_SOVEREIGN_BOSS.name", "Зеркальный Государь" },
            { "MIRRORED_SOVEREIGN_BOSS.title", "Зеркальный Государь" },
            { "MIRRORED_SOVEREIGN.moves.SOVEREIGN_WARD.title", "Призматический барьер" },
            { "MIRRORED_SOVEREIGN.moves.SOVEREIGN_BEAM.title", "Преломляющий луч" },
            { "MIRRORED_SOVEREIGN.moves.SOVEREIGN_SPLINTER.title", "Стеклянные осколки" },
            { "MIRRORED_SOVEREIGN.moves.SOVEREIGN_RESONATOR.title", "Эхо государя" },
            { "PRISMATIC_WARD_POWER.name", "Призматический Щит" },
            { "PRISMATIC_WARD_POWER.title", "Призматический Щит" },
            { "PRISMATIC_WARD_POWER.description", "Снижает получаемый урон на 50%, пока живы Зеркальные клоны, и отражает 25% урона атак." },

            { "ALCHEMICAL_CHIMERA.name", "Алхимическая Химера" },
            { "ALCHEMICAL_CHIMERA.title", "Алхимическая Химера" },
            { "ALCHEMICAL_CHIMERA_BOSS.name", "Алхимическая Химера" },
            { "ALCHEMICAL_CHIMERA_BOSS.title", "Алхимическая Химера" },
            { "ALCHEMICAL_CHIMERA.moves.CHIMERA_ACID1.title", "Едкая слюна" },
            { "ALCHEMICAL_CHIMERA.moves.CHIMERA_ACID2.title", "Кислотный поток" },
            { "ALCHEMICAL_CHIMERA.moves.CHIMERA_FIRE1.title", "Дыхание магмы" },
            { "ALCHEMICAL_CHIMERA.moves.CHIMERA_FIRE2.title", "Воспламеняющий рывок" },
            { "ALCHEMICAL_CHIMERA.moves.CHIMERA_CRYO1.title", "Ледниковый рык" },
            { "ALCHEMICAL_CHIMERA.moves.CHIMERA_CRYO2.title", "Крио-наскок" },
            { "CHIMERA_TRANSMUTATION_POWER.name", "Трансмутация Реагентов" },
            { "CHIMERA_TRANSMUTATION_POWER.title", "Трансмутация Реагентов" },
            { "CHIMERA_TRANSMUTATION_POWER.description", "Циклически меняет реагенты: Кислота разъедает защиту, Огонь добавляет Ожоги, Лёд дает мощную броню." },

            { "NULL_EXECUTIONER.name", "Палач Забвения" },
            { "NULL_EXECUTIONER.title", "Палач Забвения" },
            { "NULL_EXECUTIONER_BOSS.name", "Палач Забвения" },
            { "NULL_EXECUTIONER_BOSS.title", "Палач Забвения" },
            { "NULL_EXECUTIONER.moves.EXEC_EDICT_ATK.title", "Запрет Атак" },
            { "NULL_EXECUTIONER.moves.EXEC_CLEAVE.title", "Секира палача" },
            { "NULL_EXECUTIONER.moves.EXEC_EDICT_SKILL.title", "Запрет Навыков" },
            { "NULL_EXECUTIONER.moves.EXEC_DECAPITATE.title", "Обезглавливание" },
            { "NULL_EDICT_POWER.name", "Указ Забвения" },
            { "NULL_EDICT_POWER.title", "Указ Забвения" },
            { "NULL_EDICT_POWER.description", "В текущем ходу запрещает разыгрывать карты определенного типа (Атаки или Навыки). Розыгрыш запрещенной карты наносит вам 6 неблокируемого урона и дает Палачу +1 Силы." },
            { "NULL_EDICT_POWER_ATK.description", "В этом ходу запрещены [gold]Атаки[/gold]! Розыгрыш Атак наносит вам 6 неблокируемого урона и дает Палачу +1 Силы." },
            { "NULL_EDICT_POWER_SKILL.description", "В этом ходу запрещены [gold]Навыки[/gold]! Розыгрыш Навыков наносит вам 6 неблокируемого урона и дает Палачу +1 Силы." },

            { "RUNIC_CUBE.name", "Рунический Куб" },
            { "RUNIC_CUBE.title", "Рунический Куб" },
            { "RUNIC_CUBE_BOSS.name", "Рунический Куб" },
            { "RUNIC_CUBE_BOSS.title", "Рунический Куб" },
            { "RUNIC_CUBE.moves.RUNIC_BASTION.title", "Бастион Рун" },
            { "RUNIC_CUBE.moves.BLADE_MATRIX.title", "Матрица Клинков" },
            { "RUNIC_CUBE.moves.HEX_EXECUTION.title", "Гексагон Казни" },

            // --- Act 3 Bosses ---
            { "CHRONOS_TITAN.name", "Титан Хроноса" },
            { "CHRONOS_TITAN.title", "Титан Хроноса" },
            { "CHRONOS_TITAN_BOSS.name", "Титан Хроноса" },
            { "CHRONOS_TITAN_BOSS.title", "Титан Хроноса" },
            { "CHRONOS_TITAN.moves.TITAN_STASIS.title", "Временной стазис" },
            { "CHRONOS_TITAN.moves.TITAN_RIFT.title", "Разлом эпох" },
            { "CHRONOS_TITAN.moves.TITAN_PARADOX.title", "Великий парадокс" },
            { "CHRONOS_TITAN.moves.TITAN_REVERSAL.title", "Обращение времени" },
            { "CHRONOS_LOOP_POWER.name", "Искажение Времени" },
            { "CHRONOS_LOOP_POWER.title", "Искажение Времени" },
            { "CHRONOS_LOOP_POWER.description", "Каждые 6 разыгранных карт за ход немедленно завершают ход игроков, давая Титану 20 брони и +2 Силы." },

            { "ASTRAL_STAR.name", "Астральная звезда" },
            { "ASTRAL_STAR.title", "Астральная звезда" },
            { "ASTRAL_STAR.moves.STAR_PULSE.title", "Звёздный пульс" },
            { "ASTRAL_STAR.moves.STAR_LIGHT.title", "Звёздный свет" },
            { "ASTRAL_DEVOURER.name", "Пожиратель Созвездий" },
            { "ASTRAL_DEVOURER.title", "Пожиратель Созвездий" },
            { "ASTRAL_DEVOURER_BOSS.name", "Пожиратель Созвездий" },
            { "ASTRAL_DEVOURER_BOSS.title", "Пожиратель Созвездий" },
            { "ASTRAL_DEVOURER.moves.ASTRAL_CHARGE.title", "Заряд сингулярности" },
            { "ASTRAL_DEVOURER.moves.ASTRAL_DEVOUR.title", "Пожирание звезды" },
            { "ASTRAL_DEVOURER.moves.ASTRAL_COLLAPSE.title", "Коллапс сверхновой" },
            { "ASTRAL_DEVOURER.moves.ASTRAL_HORIZON.title", "Горизонт событий" },
            { "COSMIC_HUNGER_POWER.name", "Космический Голод" },
            { "COSMIC_HUNGER_POWER.title", "Космический Голод" },
            { "COSMIC_HUNGER_POWER.description", "Получает +1 Силы в конце каждого хода." },

            { "SPIRE_ARCHITECT.name", "Архитектор Шпиля" },
            { "SPIRE_ARCHITECT.title", "Архитектор Шпиля" },
            { "SPIRE_ARCHITECT_BOSS.name", "Архитектор Шпиля" },
            { "SPIRE_ARCHITECT_BOSS.title", "Архитектор Шпиля" },
            { "SPIRE_ARCHITECT.moves.ARCH_FOUNDATION.title", "Фундамент Шпиля" },
            { "SPIRE_ARCHITECT.moves.ARCH_BEAM.title", "Геометрический лазер" },
            { "SPIRE_ARCHITECT.moves.ARCH_PILLAR.title", "Монолитный оплот" },
            { "SPIRE_ARCHITECT.moves.ARCH_COLLAPSE.title", "Катаклизм башни" },
            { "ARCHITECT_GEOMETRY_POWER.name", "Основы Шпиля" },
            { "ARCHITECT_GEOMETRY_POWER.title", "Основы Шпиля" },
            { "ARCHITECT_GEOMETRY_POWER.description", "Сменяет фазы по мере падения ОЗ: при >66% HP дает 4 защиты за удар, при 33%-66% накладывает Хрупкость, при <=33% дает +8 Силы." },

            // --- Card Powers (Status Bar) ---
            { "DEEP_CONCENTRATION_POWER.name", "Глубокая концентрация" },
            { "DEEP_CONCENTRATION_POWER.title", "Глубокая концентрация" },
            { "DEEP_CONCENTRATION_POWER.description", "В начале вашего хода: если в прошлый ход вы не разыграли ни одной карты, дает {Amount} Бодрости." },

            { "EGOIST_BUFF_POWER.name", "Эгоист" },
            { "EGOIST_BUFF_POWER.title", "Эгоист" },
            { "EGOIST_BUFF_POWER.description", "В этом ходу вы наносите удвоенный урон." },

            { "EGOIST_DEBUFF_POWER.name", "Побочный ущерб" },
            { "EGOIST_DEBUFF_POWER.title", "Побочный ущерб" },
            { "EGOIST_DEBUFF_POWER.description", "До вашего следующего хода вы получаете удвоенный урон." },

            { "TITANIC_BASTION_POWER.name", "Титанический бастион" },
            { "TITANIC_BASTION_POWER.title", "Титанический бастион" },
            { "TITANIC_BASTION_POWER.description", "В начале следующего хода: если у вас сохранился Блок, дает {Amount} Силы." },

            { "IRON_LOAN_POWER.name", "Железный заём" },
            { "IRON_LOAN_POWER.title", "Железный заём" },
            { "IRON_LOAN_POWER.description", "В конце хода вы теряете 5 ОЗ за каждую непотраченную Энергию. В следующем ходу добираете на 1 карту меньше." },

            { "MARTYRS_BRAND_POWER.name", "Клеймо мученика" },
            { "MARTYRS_BRAND_POWER.title", "Клеймо мученика" },
            { "MARTYRS_BRAND_POWER.description", "На {Amount} ходов: при получении урона от атак врагов отражает 100% полученного урона обратно атакующему." },

            { "SIEGE_STANCE_POWER.name", "Осадная стойка" },
            { "SIEGE_STANCE_POWER.title", "Осадная стойка" },
            { "SIEGE_STANCE_POWER.description", "Вы больше не можете разыгрывать Атаки. Весь получаемый Блок удваивается. В конце хода наносит ВСЕМ врагам урон в размере 50% вашего Блока." },

            { "SECOND_HEART_POWER.name", "Второе сердце" },
            { "SECOND_HEART_POWER.title", "Второе сердце" },
            { "SECOND_HEART_POWER.description", "При получении смертельного урона предотвращает гибель, восстанавливает 25% макс. ОЗ, дает 4 Силы и 20 Блока." },

            { "DIVERT_AND_SCATTER_POWER.name", "Отвод и рассеивание" },
            { "DIVERT_AND_SCATTER_POWER.title", "Отвод и рассеивание" },
            { "DIVERT_AND_SCATTER_POWER.description", "До вашего следующего хода: весь получаемый урон снижается до 0 и распределяется поровну между союзниками (или врагами, если вы в одиночку)." },

            { "ENVENOMED_ARSENAL_POWER.name", "Отравленный арсенал" },
            { "ENVENOMED_ARSENAL_POWER.title", "Отравленный арсенал" },
            { "ENVENOMED_ARSENAL_POWER.description", "Каждый раз, когда Заточка попадает в руку, накладывает {Amount} Яда на случайного врага. При сжигании любой карты накладывает 1 Яд на ВСЕХ врагов." },

            { "WELL_POISONER_POWER.name", "Отравитель колодца" },
            { "WELL_POISONER_POWER.title", "Отравитель колодца" },
            { "WELL_POISONER_POWER.description", "Каждый раз, когда вы получаете ослабление от врага, накладывает на него {Amount} Яда в ответ." },

            { "SHADOW_DOUBLE_POWER.name", "Теневой двойник" },
            { "SHADOW_DOUBLE_POWER.title", "Теневой двойник" },
            { "SHADOW_DOUBLE_POWER.description", "Следующая сыгранная в этом ходу Атака разыгрывается дважды." },

            { "SKELETON_ARMY_POWER.name", "Армия скелетов" },
            { "SKELETON_ARMY_POWER.title", "Армия скелетов" },
            { "SKELETON_ARMY_POWER.description", "При каждой атаке Ости мини-скелеты повторяют её 2 раза с 50% урона. Погибают вместе с Ости." },

            { "NECROTIC_TETHER_POWER.name", "Некротическая связь" },
            { "NECROTIC_TETHER_POWER.title", "Некротическая связь" },
            { "NECROTIC_TETHER_POWER.description", "Когда игрок или Ости получает урон, 50% этого урона наносится этому врагу. Длится {Amount} ходов." },

            { "OSSUARY_FUSION_POWER.name", "Слияние склепа" },
            { "OSSUARY_FUSION_POWER.title", "Слияние склепа" },
            { "OSSUARY_FUSION_POWER.description", "В начале следующего хода призывает усиленного Ости с 20 макс. ОЗ." },

            { "DEATH_ECHO_POWER.name", "Эхо смерти" },
            { "DEATH_ECHO_POWER.title", "Эхо смерти" },
            { "DEATH_ECHO_POWER.description", "При гибели любого существа вы получаете 1 Энергию и берете 1 карту." },

            { "WRAITH_STEP_DOOM_POWER.name", "Проклятие призрака" },
            { "WRAITH_STEP_DOOM_POWER.title", "Проклятие призрака" },
            { "WRAITH_STEP_DOOM_POWER.description", "В начале вашего следующего хода вы получаете 3 Погибели на себя." },

            { "SIGIL_OF_DECAY_POWER.name", "Символ тления" },
            { "SIGIL_OF_DECAY_POWER.title", "Символ тления" },
            { "SIGIL_OF_DECAY_POWER.description", "В конце вашего хода все враги получают непробиваемый урон, равный их Погибели." },

            { "NUCLEAR_REACTOR_POWER.name", "Ядерный реактор" },
            { "NUCLEAR_REACTOR_POWER.title", "Ядерный реактор" },
            { "NUCLEAR_REACTOR_POWER.description", "Каждый раз при создании карты Статуса в бою вы получаете 1 Энергию." },

            { "MATRIX_CAPACITOR_POWER.name", "Матричный конденсатор" },
            { "MATRIX_CAPACITOR_POWER.title", "Матричный конденсатор" },
            { "MATRIX_CAPACITOR_POWER.description", "В этом ходу при каждом проявлении сферы вы получаете {Amount} Блока." },

            { "SUPERCONDUCTOR_POWER.name", "Сверхпроводник" },
            { "SUPERCONDUCTOR_POWER.title", "Сверхпроводник" },
            { "SUPERCONDUCTOR_POWER.description", "В этом ходу ваши сферы активируют свои пассивные свойства на 1 раз больше." },

            { "QUANTUM_ENTANGLEMENT_POWER.name", "Квантовая запутанность" },
            { "QUANTUM_ENTANGLEMENT_POWER.title", "Квантовая запутанность" },
            { "QUANTUM_ENTANGLEMENT_POWER.description", "Когда другой враг получает урон от игрока, этот враг получает такой же урон. Длится {Amount} ходов." },

            { "PLASMA_OVERDRIVE_POWER.name", "Плазменный перегруз" },
            { "PLASMA_OVERDRIVE_POWER.title", "Плазменный перегруз" },
            { "PLASMA_OVERDRIVE_POWER.description", "При проявлении сферы Плазмы дает 1 дополнительную Энергию и наносит вам 3 урона." },

            { "ABSOLUTE_ZERO_POWER.name", "Абсолютный ноль" },
            { "ABSOLUTE_ZERO_POWER.title", "Абсолютный ноль" },
            { "ABSOLUTE_ZERO_POWER.description", "В конце хода накладывает Слабость на ВСЕХ врагов по числу ваших сфер Мороза. При проявлении сферы Мороза дает 12 Блока." },

            { "GRAVITATIONAL_SINGULARITY_POWER.name", "Гравитационная сингулярность" },
            { "GRAVITATIONAL_SINGULARITY_POWER.title", "Гравитационная сингулярность" },
            { "GRAVITATIONAL_SINGULARITY_POWER.description", "Этот враг перенаправляет {Amount}% урона своих атак в других врагов. Вы получаете 1 Звезду за каждую атаку." },

            { "CROWN_TRIBUTE_POWER.name", "Королевская дань" },
            { "CROWN_TRIBUTE_POWER.title", "Королевская дань" },
            { "CROWN_TRIBUTE_POWER.description", "Метка на {Amount} ходов. Атакующие восстанавливают 2 ОЗ, берут 1 карту и дают 1 Звезду. При гибели дает всем игрокам 1 Энергию и 10 Блока." },

            { "SUPERNOVA_CORONATION_POWER.name", "Коронация сверхновой" },
            { "SUPERNOVA_CORONATION_POWER.title", "Коронация сверхновой" },
            { "SUPERNOVA_CORONATION_POWER.description", "В конце хода непотраченные Звезды наносят по {Amount} урона ВСЕМ врагам и дают вдвое меньше Блока. Каждые 6 потраченных Звезд снимают вражеский Блок, накладывают 3 Уязвимости и дают всем игрокам 2 Энергии." },

            { "AEGIS_OF_THE_CROWN_POWER.name", "Эгида короны" },
            { "AEGIS_OF_THE_CROWN_POWER.title", "Эгида короны" },
            { "AEGIS_OF_THE_CROWN_POWER.description", "Каждый раз при ковке Клинка Владыки вы получаете Блок, равный значению ковки." },

            { "VOID_ASCENSION_POWER.name", "Вознесение пустоты" },
            { "VOID_ASCENSION_POWER.title", "Вознесение пустоты" },
            { "VOID_ASCENSION_POWER.description", "В начале вашего хода вы получаете 2 Звезды. При розыгрыше Клинка Владыки с 6+ Звездами он разыгрывается дважды." },

            { "COCOON_INCUBATION_POWER.name", "Инкубация" },
            { "COCOON_INCUBATION_POWER.title", "Инкубация" },
            { "COCOON_INCUBATION_POWER.description", "Уменьшается каждый ход. Достигнув 0, взрывается, нанося 12 непробиваемого урона всем игрокам и добавляя 2 Споры в сброс." },

            // --- Act 4 Boss: The Corrupted Heart ---
            { "SPIRE_HEART.name", "Осквернённое Сердце Шпиля" },
            { "SPIRE_HEART.title", "Осквернённое Сердце Шпиля" },
            { "SPIRE_HEART_BOSS.name", "Осквернённое Сердце Шпиля" },
            { "SPIRE_HEART_BOSS.title", "Осквернённое Сердце Шпиля" },
            { "SPIRE_HEART.moves.HEART_DEBILITATE.title", "Истощение" },
            { "SPIRE_HEART.moves.HEART_BLOOD_SHOTS.title", "Кровавые выстрелы" },
            { "SPIRE_HEART.moves.HEART_ECHO_SLAM.title", "Эхо-сокрушение" },
            { "SPIRE_HEART.moves.HEART_CLEANSING.title", "Очищение" },
            { "BEAT_OF_DEATH_POWER.name", "Пульс смерти" },
            { "BEAT_OF_DEATH_POWER.title", "Пульс смерти" },
            { "BEAT_OF_DEATH_POWER.description", "Каждый раз, когда вы разыгрываете карту, вы получаете {Amount} непробиваемого урона." },
            { "SPIRE_INVINCIBLE_POWER.name", "Несокрушимость" },
            { "SPIRE_INVINCIBLE_POWER.title", "Несокрушимость" },
            { "SPIRE_INVINCIBLE_POWER.description", "Не может получить более 300 урона за один ход. Осталось в этом ходу: {Amount}." },

            // --- Custom Relics ---
            { "CRIMSON_PACT.title", "Багровый пакт" },
            { "CRIMSON_PACT.description", "Когда вы получаете урон от собственных карт или способностей, дает 4 Защиты." },
            { "CRIMSON_PACT.flavor", "Зловещий пакт, скреплённый свежей артериальной кровью." },

            { "SOUL_LANTERN.title", "Фонарь душ" },
            { "SOUL_LANTERN.description", "В начале боя дает 1 Энергию и берет 1 карту." },
            { "SOUL_LANTERN.flavor", "Его бледное пламя жаждет заблудших духовных сущностей." },

            { "OBSIDIAN_ANVIL.title", "Обсидиановая наковальня" },
            { "OBSIDIAN_ANVIL.description", "Когда вы разыгрываете улучшенную карту, наносит 3 непробиваемого урона ВСЕМ врагам." },
            { "OBSIDIAN_ANVIL.flavor", "Выкована в бездонных недрах, где обычный металл обращается в прах." },

            { "CLOCKWORK_GEAR.title", "Заводная шестерня" },
            { "CLOCKWORK_GEAR.description", "Каждая 4-я разыгранная Атака наносит 8 урона ВСЕМ врагам." },
            { "CLOCKWORK_GEAR.flavor", "Тикает с пугающей механической точностью." },

            { "CURSED_AMULET.title", "Проклятый амулет" },
            { "CURSED_AMULET.description", "Реликвия босса. В начале каждого хода дает 1 Энергию. В начале каждого боя дает 2 Рока." },
            { "CURSED_AMULET.flavor", "Неизмеримая мощь, отягощенная неотвратимой темной судьбой." },

            { "SPIRE_COMPASS.title", "Компас шпиля" },
            { "SPIRE_COMPASS.description", "Реликвия магазина. При получении дает 75 Золота. В начале каждого боя берет 1 карту." },
            { "SPIRE_COMPASS.flavor", "Стрелка бешено крутится, но неизменно указывает путь к богатству." },

            { "WAR_DRUM.title", "Барабан войны" },
            { "WAR_DRUM.description", "Реликвия Латоносца. Когда вы получаете Силу, дает 3 Защиты." },
            { "WAR_DRUM.flavor", "Оглушительный бой барабана разжигает кровь истинных берсерков." },

            { "VIPER_FANG.title", "Клык гадюки" },
            { "VIPER_FANG.description", "Реликвия Безмолвной. Когда вы наносите урон Атакой врагу с Ядом, накладывает еще 1 Яд." },
            { "VIPER_FANG.flavor", "С клыка непрерывно сочится смертоносный нейротоксин." },

            { "CAPACITOR_CORE.title", "Ядро конденсатора" },
            { "CAPACITOR_CORE.description", "Реликвия Дефекта. В начале каждого боя создает 1 сферу Плазмы." },
            { "CAPACITOR_CORE.flavor", "Миниатюрная сингулярность, гудящая чистой термоядерной мощью." },

            { "GRAVEYARD_SOIL.title", "Земля погоста" },
            { "GRAVEYARD_SOIL.description", "Реликвия Некробиндера. Когда карта сгорает, наносит 2 непробиваемого урона ВСЕМ врагам." },
            { "GRAVEYARD_SOIL.flavor", "Проклятая почва, пропитанная памятью о тысячах павших." },

            { "CELESTIAL_PRISM.title", "Небесная призма" },
            { "CELESTIAL_PRISM.description", "Реликвия Регента. Когда вы тратите Звезды, дает 4 Защиты за каждую потраченную Звезду." },
            { "CELESTIAL_PRISM.flavor", "Преломляет звездный свет в непроницаемые кристаллические щиты." },

            { "HEART_OF_INFINITY.title", "Сердце бесконечности" },
            { "HEART_OF_INFINITY.description", "Реликвия босса. В начале каждого хода добавляет в руку случайную карту: Заточка, Осколок стекла или Космическая пыль." },
            { "HEART_OF_INFINITY.flavor", "Пульсирующее ядро, резонирующее с осколками бесчисленных реальностей." }
        };

        public static bool HasString(string key)
        {
            if (Russian.ContainsKey(key) || English.ContainsKey(key)) return true;
            if (TryGetAlias(key, out string aliasKey))
            {
                if (Russian.ContainsKey(aliasKey) || English.ContainsKey(aliasKey)) return true;
            }
            if (IsKnownModKey(key) && (key.EndsWith(".title") || key.EndsWith(".name"))) return true;
            return false;
        }

        public static bool TryGetString(string key, out string result)
        {
            string lang = LocManager.Instance?.Language ?? "eng";

            // 1. Direct dictionary match
            if (lang == "rus")
            {
                if (Russian.TryGetValue(key, out result)) return true;
            }
            else
            {
                if (English.TryGetValue(key, out result)) return true;
            }

            // 2. Direct match in secondary dictionary
            if (lang == "rus")
            {
                if (English.TryGetValue(key, out result)) return true;
            }
            else
            {
                if (Russian.TryGetValue(key, out result)) return true;
            }

            // 3. Suffix / alias swap (.title <-> .name, base monster lookup, smartDescription -> description)
            if (TryGetAlias(key, out string aliasKey))
            {
                if (lang == "rus" && Russian.TryGetValue(aliasKey, out result)) return true;
                if (English.TryGetValue(aliasKey, out result)) return true;
                if (Russian.TryGetValue(aliasKey, out result)) return true;
            }

            // 4. If key is a move title request, generate dynamic readable title
            if (key.Contains(".moves.") && (key.EndsWith(".title") || key.EndsWith(".name")))
            {
                string movePart = key.Substring(key.IndexOf(".moves.") + 7);
                int lastDot = movePart.LastIndexOf('.');
                if (lastDot >= 0) movePart = movePart.Substring(0, lastDot);
                result = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(movePart.Replace("_", " ").ToLowerInvariant());
                return true;
            }

            // 5. If key belongs to a known mod entity, generate clean fallback
            if (IsKnownModKey(key))
            {
                if (key.EndsWith(".description") || key.EndsWith(".smartDescription"))
                {
                    result = "";
                    return false;
                }
                string clean = key;
                int dot = clean.LastIndexOf('.');
                if (dot >= 0) clean = clean.Substring(0, dot);
                clean = clean.Replace("_BOSS", "").Replace("_ENCOUNTER", "").Replace("_POWER", "").Replace("_", " ");
                result = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(clean.ToLowerInvariant());
                return true;
            }

            result = "";
            return false;
        }

        private static bool TryGetAlias(string key, out string aliasKey)
        {
            if (key.EndsWith(".smartDescription"))
            {
                aliasKey = key.Substring(0, key.Length - 17) + ".description";
                return true;
            }
            if (key.StartsWith("COCOON_INCUBATION_POWER."))
            {
                aliasKey = "COCOON_POWER." + key.Substring("COCOON_INCUBATION_POWER.".Length);
                return true;
            }
            if (key.EndsWith(".title"))
            {
                aliasKey = key.Substring(0, key.Length - 6) + ".name";
                return true;
            }
            if (key.EndsWith(".name"))
            {
                aliasKey = key.Substring(0, key.Length - 5) + ".title";
                return true;
            }
            if (key.Contains("_BOSS."))
            {
                aliasKey = key.Replace("_BOSS.", ".");
                return true;
            }
            if (key.Contains("_ENCOUNTER."))
            {
                aliasKey = key.Replace("_ENCOUNTER.", ".");
                return true;
            }
            aliasKey = "";
            return false;
        }

        private static bool IsKnownModKey(string key)
        {
            return key.Contains("CHRONO_") || key.Contains("MIRROR_") || key.Contains("SPORE_") ||
                   key.Contains("SLAG_") || key.Contains("PUPPET") || key.Contains("COLOSSUS") ||
                   key.Contains("BROODMOTHER") || key.Contains("INQUISITOR") || key.Contains("SOVEREIGN") ||
                   key.Contains("CHIMERA") || key.Contains("EXECUTIONER") || key.Contains("TITAN") ||
                   key.Contains("DEVOURER") || key.Contains("ARCHITECT") || key.Contains("MARTYRDOM") ||
                   key.Contains("TENSION") || key.Contains("PRISMATIC");
        }

        public static Dictionary<string, string> GetTable(string lang)
        {
            return lang == "rus" ? Russian : English;
        }
    }

    [HarmonyPatch(typeof(LocTable), nameof(LocTable.GetRawText))]
    public static class LocTableGetRawTextPatch
    {
        public static bool Prefix(LocTable __instance, string key, ref string __result)
        {
            if (ModLocalization.TryGetString(key, out string val))
            {
                __result = val;
                return false;
            }
            return true;
        }

        public static Exception? Finalizer(Exception? __exception, string key, ref string __result)
        {
            if (__exception is LocException)
            {
                if (key.EndsWith(".smartDescription"))
                {
                    string descKey = key.Substring(0, key.Length - 17) + ".description";
                    if (ModLocalization.TryGetString(descKey, out string descVal))
                    {
                        __result = descVal;
                        return null;
                    }
                }
                Log.Warn($"[AnimeSurvivalMod] Suppressed missing LocTable key '{key}'; generating safe fallback.");
                string clean = key;
                int dot = clean.LastIndexOf('.');
                if (dot >= 0) clean = clean.Substring(0, dot);
                clean = clean.Replace("_BOSS", "").Replace("_ENCOUNTER", "").Replace("_POWER", "").Replace("_", " ");
                __result = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(clean.ToLowerInvariant());
                return null; // Exception suppressed!
            }
            return __exception;
        }
    }

    [HarmonyPatch(typeof(LocTable), nameof(LocTable.HasEntry))]
    public static class LocTableHasEntryPatch
    {
        public static bool Prefix(LocTable __instance, string key, ref bool __result)
        {
            if (ModLocalization.HasString(key))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
