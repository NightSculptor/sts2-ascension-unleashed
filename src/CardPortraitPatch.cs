#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using MegaCrit.Sts2.Core.Models;

namespace AnimeSurvivalMod
{
    [HarmonyPatch(typeof(CardModel), "get_Portrait")]
    public static class CardPortraitPatch
    {
        public static bool Enabled = true;
        private static readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string>? _fileMap;
        private static string? _resolvedPortraitDir;

        public static string GetPortraitDir()
        {
            if (_resolvedPortraitDir != null) return _resolvedPortraitDir;

            var candidates = new List<string>();

            // 1. Ask STS2's ModManager for the loaded mod's exact path (Works natively on Linux, Steam Deck, Proton, Windows)
            try
            {
                foreach (var mod in ModManager.GetLoadedMods())
                {
                    if (mod?.manifest?.id == "AnimeSurvivalMod" || 
                        (mod?.assemblies != null && mod.assemblies.Contains(Assembly.GetExecutingAssembly())))
                    {
                        if (!string.IsNullOrEmpty(mod.path))
                        {
                            candidates.Add(Path.Combine(mod.path, "portraits"));
                            candidates.Add(Path.Combine(mod.path, "Portraits"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Warn($"[AnimeSurvivalMod] Error checking ModManager.GetLoadedMods: {ex.Message}");
            }

            // 2. Relative to executing assembly
            try
            {
                string loc = Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(loc))
                {
                    string dir = Path.GetDirectoryName(loc)!;
                    candidates.Add(Path.Combine(dir, "portraits"));
                    candidates.Add(Path.Combine(dir, "Portraits"));
                }
            }
            catch {}

            // 3. Relative to process executable path
            try
            {
                string exeDir = Path.GetDirectoryName(System.Environment.ProcessPath) ?? "";
                if (!string.IsNullOrEmpty(exeDir))
                {
                    candidates.Add(Path.Combine(exeDir, "mods", "AnimeSurvivalMod", "portraits"));
                    candidates.Add(Path.Combine(exeDir, "mods", "AnimeSurvivalMod", "Portraits"));
                }
            }
            catch {}

            // 4. AppContext and CurrentDirectory
            candidates.Add(Path.Combine(AppContext.BaseDirectory, "mods", "AnimeSurvivalMod", "portraits"));
            candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "mods", "AnimeSurvivalMod", "portraits"));
            candidates.Add(@"D:\Slay The Spire 2\mods\AnimeSurvivalMod\portraits");
            candidates.Add(@"D:\Slay The Spire 2\_mod_work\portraits");

            foreach (var c in candidates)
            {
                if (Directory.Exists(c))
                {
                    _resolvedPortraitDir = c;
                    Log.Info($"[AnimeSurvivalMod] Resolved custom card portraits directory: {_resolvedPortraitDir}");
                    return _resolvedPortraitDir;
                }
            }

            _resolvedPortraitDir = candidates[0];
            return _resolvedPortraitDir;
        }

        public static string? FindPortraitFile(string key)
        {
            if (!Enabled) return null;
            string portraitDir = GetPortraitDir();
            if (string.IsNullOrEmpty(portraitDir) || !Directory.Exists(portraitDir)) return null;

            if (_fileMap == null)
            {
                _fileMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    foreach (var file in Directory.GetFiles(portraitDir, "*.png"))
                    {
                        string name = Path.GetFileNameWithoutExtension(file);
                        _fileMap[name] = file;
                    }
                    Log.Info($"[AnimeSurvivalMod] Indexed {_fileMap.Count} custom card portraits from {portraitDir}");
                }
                catch (Exception ex)
                {
                    Log.Error($"[AnimeSurvivalMod] Failed to index portraits in {portraitDir}: {ex}");
                }
            }

            if (_fileMap.TryGetValue(key, out var path) && File.Exists(path))
            {
                return path;
            }

            return null;
        }

        public static bool Prefix(CardModel __instance, ref Texture2D __result)
        {
            if (!Enabled || __instance == null) return true;
            string key = __instance.Id.Entry;
            if (_cache.TryGetValue(key, out var cachedTex))
            {
                __result = cachedTex;
                return false;
            }

            try
            {
                string? pngPath = FindPortraitFile(key);
                if (pngPath != null)
                {
                    var img = Image.LoadFromFile(pngPath);
                    if (img != null)
                    {
                        var tex = ImageTexture.CreateFromImage(img);
                        if (tex != null)
                        {
                            _cache[key] = tex;
                            __result = tex;
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"[AnimeSurvivalMod] Error loading portrait for {key}: {ex}");
            }

            return true;
        }

        public static bool HasCustomPortrait(string key)
        {
            if (!Enabled) return false;
            return FindPortraitFile(key) != null;
        }
    }

    [HarmonyPatch(typeof(CardModel), "get_HasPortrait")]
    public static class CardHasPortraitPatch
    {
        public static bool Prefix(CardModel __instance, ref bool __result)
        {
            if (__instance != null && CardPortraitPatch.HasCustomPortrait(__instance.Id.Entry))
            {
                __result = true;
                return false;
            }
            return true;
        }
    }
}
