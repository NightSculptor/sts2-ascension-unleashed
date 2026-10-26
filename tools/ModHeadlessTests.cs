using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using AnimeSurvivalMod;

namespace ModTesting
{
    public static class Program
    {
        private static int _passed = 0;
        private static int _failed = 0;

        public static int Main(string[] args)
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
            {
                var assemblyName = new AssemblyName(eventArgs.Name).Name;
                string gameDir = @"D:\Slay The Spire 2\data_sts2_windows_x86_64";
                string candidate = Path.Combine(gameDir, assemblyName + ".dll");
                if (File.Exists(candidate)) return Assembly.LoadFrom(candidate);
                string buildCandidate = Path.Combine(@"D:\Slay The Spire 2\_mod_work\build", assemblyName + ".dll");
                if (File.Exists(buildCandidate)) return Assembly.LoadFrom(buildCandidate);
                return null;
            };

            return RunTests();
        }

        private static int RunTests()
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("  ASCENSION UNLEASHED - HEADLESS TEST RUNNER   ");
            Console.WriteLine("=================================================\n");

            TestMultiplayerConstraints();
            TestCardInstantiationAndUpgrades();
            TestBossStateMachineAndIntents();
            TestRelicInstantiation();
            TestLocalizationCompleteness();

            Console.WriteLine("\n-------------------------------------------------");
            Console.WriteLine($"RESULTS: {_passed} Passed, {_failed} Failed");
            Console.WriteLine("=================================================");

            return _failed > 0 ? 1 : 0;
        }

        private static void Assert(bool condition, string testName, string? errorMsg = null)
        {
            if (condition)
            {
                _passed++;
                Console.WriteLine($"[PASS] {testName}");
            }
            else
            {
                _failed++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[FAIL] {testName}: {errorMsg ?? "Assertion failed"}");
                Console.ResetColor();
            }
        }

        private static void TestMultiplayerConstraints()
        {
            Console.WriteLine(">>> 1. Testing Solo/Multiplayer Constraints...");

            var coOpCards = new (Type CardType, string Name)[]
            {
                (typeof(Egoist), "Egoist"),
                (typeof(DivertAndScatter), "DivertAndScatter"),
                (typeof(ShiftBurden), "ShiftBurden"),
                (typeof(SoulTransfusion), "SoulTransfusion")
            };

            foreach (var (cardType, name) in coOpCards)
            {
                var card = (CardModel)Activator.CreateInstance(cardType)!;
                Assert(card.MultiplayerConstraint == CardMultiplayerConstraint.MultiplayerOnly,
                    $"Card '{name}' has MultiplayerOnly constraint",
                    $"Expected MultiplayerOnly, got {card.MultiplayerConstraint}");
            }
        }

        private static void TestCardInstantiationAndUpgrades()
        {
            Console.WriteLine("\n>>> 2. Testing All Custom Cards (Instantiation & Upgrade Logic)...");

            var modAssembly = typeof(ClockworkColossus).Assembly;
            var cardTypes = modAssembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(CardModel).IsAssignableFrom(t))
                .ToList();

            Assert(cardTypes.Count >= 70, $"Found {cardTypes.Count} custom cards in mod assembly (expected >= 70)");

            foreach (var cardType in cardTypes)
            {
                try
                {
                    var card = (CardModel)Activator.CreateInstance(cardType)!;
                    
                    // Verify base cost is valid (>= 0 or X)
                    bool validCost = card.EnergyCost.Canonical >= 0;
                    Assert(validCost, $"Card '{cardType.Name}' canonical cost is valid ({card.EnergyCost.Canonical})");

                    // Verify upgrade logic on a mutable instance
                    bool upgradeOk = true;
                    try
                    {
                        var field = typeof(AbstractModel).GetField("<IsMutable>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                        field?.SetValue(card, true);

                        var method = cardType.GetMethod("OnUpgrade", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        method?.Invoke(card, null);
                    }
                    catch (Exception ex)
                    {
                        upgradeOk = false;
                        Assert(false, $"Card '{cardType.Name}' OnUpgrade() threw: {ex.InnerException?.Message ?? ex.Message}");
                    }
                    if (upgradeOk)
                    {
                        Assert(true, $"Card '{cardType.Name}' OnUpgrade() runs cleanly");
                    }
                }
                catch (Exception ex)
                {
                    Assert(false, $"Instantiating card '{cardType.Name}'", ex.Message);
                }
            }
        }

        private static void TestBossStateMachineAndIntents()
        {
            Console.WriteLine("\n>>> 3. Testing Boss Move State Machines & HP Configurations...");

            var bossTypes = new Type[]
            {
                typeof(ClockworkColossus),
                typeof(MycelialBroodmother),
                typeof(SanguineInquisitor),
                typeof(MirroredSovereign),
                typeof(AlchemicalChimera),
                typeof(NullExecutioner),
                typeof(ChronosTitan),
                typeof(AstralDevourer),
                typeof(SpireArchitect),
                typeof(SpireHeart)
            };

            foreach (var bType in bossTypes)
            {
                try
                {
                    var monster = (MonsterModel)Activator.CreateInstance(bType)!;
                    
                    Assert(monster.MinInitialHp > 0 && monster.MaxInitialHp >= monster.MinInitialHp,
                        $"Boss '{bType.Name}' has valid HP range [{monster.MinInitialHp} - {monster.MaxInitialHp}]");

                    var smMethod = bType.GetMethod("GenerateMoveStateMachine", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    var sm = smMethod?.Invoke(monster, null) as MonsterMoveStateMachine;

                    Assert(sm != null, $"Boss '{bType.Name}' generated valid MonsterMoveStateMachine");
                }
                catch (Exception ex)
                {
                    Assert(false, $"Boss '{bType.Name}' validation", ex.InnerException?.Message ?? ex.Message);
                }
            }
        }

        private static void TestRelicInstantiation()
        {
            Console.WriteLine("\n>>> 4. Testing All 12 Custom Relics (Instantiation, Rarity, Icon)...");

            var relicTypes = new Type[]
            {
                typeof(CrimsonPact),
                typeof(SoulLantern),
                typeof(ObsidianAnvil),
                typeof(ClockworkGear),
                typeof(CursedAmulet),
                typeof(SpireCompass),
                typeof(WarDrum),
                typeof(ViperFang),
                typeof(CapacitorCore),
                typeof(GraveyardSoil),
                typeof(CelestialPrism),
                typeof(HeartOfInfinity)
            };

            foreach (var rType in relicTypes)
            {
                try
                {
                    var relic = (RelicModel)Activator.CreateInstance(rType)!;
                    Assert(relic != null, $"Relic '{rType.Name}' instantiated cleanly");

                    Assert(relic.Rarity != RelicRarity.None, $"Relic '{rType.Name}' has valid rarity ({relic.Rarity})");

                    var iconProp = rType.GetProperty("IconBaseName", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                    string? icon = iconProp?.GetValue(relic) as string;
                    Assert(!string.IsNullOrWhiteSpace(icon), $"Relic '{rType.Name}' has valid icon base name ('{icon}')");
                }
                catch (Exception ex)
                {
                    Assert(false, $"Relic '{rType.Name}' validation", ex.InnerException?.Message ?? ex.Message);
                }
            }
        }

        private static void TestLocalizationCompleteness()
        {
            Console.WriteLine("\n>>> 5. Testing Localization Completeness for Key Entities...");

            var locType = typeof(ModLocalization);
            var engField = locType.GetField("English", BindingFlags.Static | BindingFlags.NonPublic);
            var ruField = locType.GetField("Russian", BindingFlags.Static | BindingFlags.NonPublic);

            var eng = engField?.GetValue(null) as Dictionary<string, string>;
            var ru = ruField?.GetValue(null) as Dictionary<string, string>;

            Assert(eng != null && eng.Count > 100, $"English localization table loaded ({eng?.Count ?? 0} entries)");
            Assert(ru != null && ru.Count > 100, $"Russian localization table loaded ({ru?.Count ?? 0} entries)");

            if (eng != null && ru != null)
            {
                string[] criticalKeys = new string[]
                {
                    "DEADLY_GAMBIT.title", "DEADLY_GAMBIT.description",
                    "GLASS_STILETTO.title", "GLASS_STILETTO.description",
                    "GLASS_SHARD.title", "GLASS_SHARD.description",
                    "DOOM_TOLL.title", "DOOM_TOLL.description",
                    "RED_HOT_ANVIL.title", "RED_HOT_ANVIL.description",
                    "CROWN_TRIBUTE.title", "CROWN_TRIBUTE.description",
                    "GRAVITATIONAL_SINGULARITY.title", "GRAVITATIONAL_SINGULARITY.description",
                    "CHRONOS_TITAN.name", "CHRONOS_LOOP_POWER.name", "CHRONOS_LOOP_POWER.description",
                    "NULL_EDICT_POWER_ATK.description", "NULL_EDICT_POWER_SKILL.description",
                    // Act 4 Boss
                    "SPIRE_HEART.name", "BEAT_OF_DEATH_POWER.name", "BEAT_OF_DEATH_POWER.description",
                    "SPIRE_INVINCIBLE_POWER.name", "SPIRE_INVINCIBLE_POWER.description",
                    // Relics
                    "CRIMSON_PACT.title", "CRIMSON_PACT.description",
                    "SOUL_LANTERN.title", "SOUL_LANTERN.description",
                    "OBSIDIAN_ANVIL.title", "OBSIDIAN_ANVIL.description",
                    "CLOCKWORK_GEAR.title", "CLOCKWORK_GEAR.description",
                    "CURSED_AMULET.title", "CURSED_AMULET.description",
                    "SPIRE_COMPASS.title", "SPIRE_COMPASS.description",
                    "WAR_DRUM.title", "WAR_DRUM.description",
                    "VIPER_FANG.title", "VIPER_FANG.description",
                    "CAPACITOR_CORE.title", "CAPACITOR_CORE.description",
                    "GRAVEYARD_SOIL.title", "GRAVEYARD_SOIL.description",
                    "CELESTIAL_PRISM.title", "CELESTIAL_PRISM.description",
                    "HEART_OF_INFINITY.title", "HEART_OF_INFINITY.description"
                };

                foreach (var k in criticalKeys)
                {
                    bool inEng = eng.ContainsKey(k) && !string.IsNullOrWhiteSpace(eng[k]);
                    bool inRu = ru.ContainsKey(k) && !string.IsNullOrWhiteSpace(ru[k]);

                    Assert(inEng, $"English key '{k}' present and non-empty");
                    Assert(inRu, $"Russian key '{k}' present and non-empty");
                }
            }
        }
    }
}
