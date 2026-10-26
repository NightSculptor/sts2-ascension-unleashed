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
    public static class PowerIconManager
    {
        public static bool Enabled = true;
        private static readonly Dictionary<string, Texture2D> _cache = new Dictionary<string, Texture2D>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, string>? _fileMap;
        private static string? _resolvedPowerDir;

        private static void SafeLog(string msg)
        {
            try { Log.Info(msg); } catch {}
        }

        public static string GetPowerDir()
        {
            if (_resolvedPowerDir != null) return _resolvedPowerDir;

            var candidates = new List<string>();

            // 1. Check STS2 ModManager loaded mods
            try
            {
                foreach (var mod in ModManager.GetLoadedMods())
                {
                    if (mod?.manifest?.id == "AnimeSurvivalMod" ||
                        (mod?.assemblies != null && mod.assemblies.Contains(Assembly.GetExecutingAssembly())))
                    {
                        if (!string.IsNullOrEmpty(mod.path))
                        {
                            candidates.Add(Path.Combine(mod.path, "powers"));
                            candidates.Add(Path.Combine(mod.path, "images", "powers"));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SafeLog($"[AnimeSurvivalMod] Error checking ModManager for powers: {ex.Message}");
            }

            // 2. Relative to executing assembly
            try
            {
                string loc = Assembly.GetExecutingAssembly().Location;
                if (!string.IsNullOrEmpty(loc))
                {
                    string dir = Path.GetDirectoryName(loc)!;
                    candidates.Add(Path.Combine(dir, "powers"));
                    candidates.Add(Path.Combine(dir, "images", "powers"));
                }
            }
            catch {}

            // 3. Relative to process executable path
            try
            {
                string exeDir = Path.GetDirectoryName(System.Environment.ProcessPath) ?? "";
                if (!string.IsNullOrEmpty(exeDir))
                {
                    candidates.Add(Path.Combine(exeDir, "mods", "AnimeSurvivalMod", "powers"));
                    candidates.Add(Path.Combine(exeDir, "mods", "AnimeSurvivalMod", "images", "powers"));
                }
            }
            catch {}

            // 4. Base directories & fallbacks
            candidates.Add(Path.Combine(AppContext.BaseDirectory, "mods", "AnimeSurvivalMod", "powers"));
            candidates.Add(Path.Combine(Directory.GetCurrentDirectory(), "mods", "AnimeSurvivalMod", "powers"));
            candidates.Add(@"D:\Slay The Spire 2\mods\AnimeSurvivalMod\powers");
            candidates.Add(@"D:\Slay The Spire 2\_mod_work\powers");

            foreach (var c in candidates)
            {
                if (Directory.Exists(c))
                {
                    _resolvedPowerDir = c;
                    SafeLog($"[AnimeSurvivalMod] Resolved custom power icons directory: {_resolvedPowerDir}");
                    return _resolvedPowerDir;
                }
            }

            _resolvedPowerDir = candidates[0];
            return _resolvedPowerDir;
        }

        public static string? FindPowerIconFile(string rawKey)
        {
            if (!Enabled) return null;
            string powerDir = GetPowerDir();
            if (string.IsNullOrEmpty(powerDir) || !Directory.Exists(powerDir)) return null;

            if (_fileMap == null)
            {
                _fileMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                try
                {
                    foreach (var file in Directory.GetFiles(powerDir, "*.png"))
                    {
                        string name = Path.GetFileNameWithoutExtension(file);
                        _fileMap[name] = file;

                        // Normalize aliases:
                        // e.g. "puppet_bonds" -> "PUPPET_BONDS_POWER", "puppet_bonds_power"
                        string lower = name.ToLowerInvariant();
                        _fileMap[lower] = file;

                        if (lower.EndsWith("_power"))
                        {
                            string stripped = lower.Substring(0, lower.Length - 6);
                            _fileMap[stripped] = file;
                        }
                        else
                        {
                            _fileMap[lower + "_power"] = file;
                        }
                    }
                    SafeLog($"[AnimeSurvivalMod] Indexed {_fileMap.Count} custom power icon keys from {powerDir}");
                }
                catch (Exception ex)
                {
                    SafeLog($"[AnimeSurvivalMod] Failed to index power icons in {powerDir}: {ex}");
                }
            }

            // Lookup variations
            if (_fileMap.TryGetValue(rawKey, out var p) && File.Exists(p)) return p;

            string keyLower = rawKey.ToLowerInvariant();
            if (_fileMap.TryGetValue(keyLower, out p) && File.Exists(p)) return p;

            if (keyLower.EndsWith("_power"))
            {
                string stripped = keyLower.Substring(0, keyLower.Length - 6);
                if (_fileMap.TryGetValue(stripped, out p) && File.Exists(p)) return p;
            }
            else
            {
                if (_fileMap.TryGetValue(keyLower + "_power", out p) && File.Exists(p)) return p;
            }

            return null;
        }

        public static bool TryGetIcon(string key, out Texture2D? texture)
        {
            texture = null;
            if (!Enabled) return false;

            if (_cache.TryGetValue(key, out var cachedTex))
            {
                texture = cachedTex;
                return true;
            }

            try
            {
                string? pngPath = FindPowerIconFile(key);
                if (pngPath != null)
                {
                    var img = Image.LoadFromFile(pngPath);
                    if (img != null)
                    {
                        var tex = ImageTexture.CreateFromImage(img);
                        if (tex != null)
                        {
                            _cache[key] = tex;
                            texture = tex;
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SafeLog($"[AnimeSurvivalMod] Error loading power icon for {key}: {ex}");
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(PowerModel), "get_Icon")]
    public static class PowerIconPatch
    {
        public static bool Prefix(PowerModel __instance, ref Texture2D __result)
        {
            if (__instance == null) return true;
            string key = __instance.Id.Entry;
            if (PowerIconManager.TryGetIcon(key, out var tex) && tex != null)
            {
                __result = tex;
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(PowerModel), "get_BigIcon")]
    public static class PowerBigIconPatch
    {
        public static bool Prefix(PowerModel __instance, ref Texture2D __result)
        {
            if (__instance == null) return true;
            string key = __instance.Id.Entry;
            if (PowerIconManager.TryGetIcon(key, out var tex) && tex != null)
            {
                __result = tex;
                return false;
            }
            return true;
        }
    }
}
