# Ascension Unleashed — Slay the Spire 2 Mod

[![Game](https://img.shields.io/badge/Game-Slay%20the%20Spire%202-red.svg)](https://store.steampowered.com/app/2868840/Slay_the_Spire_2/)
[![Engine](https://img.shields.io/badge/Engine-Godot%204%20(C%23)-blue.svg)](https://godotengine.org)
[![Version](https://img.shields.io/badge/Version-1.0.0-green.svg)]()
[![Multiplayer](https://img.shields.io/badge/Multiplayer-Co--Op%20Compatible-brightgreen.svg)]()

**Ascension Unleashed** is a massive expansion mod for **Slay the Spire 2**, adding **79 new cards**, **12 unique relics**, **10 custom act bosses** (including the Act 4 Spire Heart), and full multiplayer co-op support!

---

## 🌟 Key Features

* ⚔️ **79 Custom Cards**: Deep tactical cards across all 5 characters:
  * **Ironclad**: High-risk health sacrifice, posture, and unstoppable momentum.
  * **Silent**: Poison cascades, blade dances, and damage-diversion defensive tricks.
  * **Defect**: Plasma reactor overcharging, matrix capacitors, and zero-energy compiler loops.
  * **Necrobinder**: Osty summon synergies, soul harvests, and negative effect redirection.
  * **Regent**: Star-forging, hovering blade orbital mechanics, and royal edicts.
* 🏺 **12 Custom Relics**: Unique game-changing relics supporting defensive, offensive, and combo builds.
* 👹 **10 Custom Act Bosses**:
  * **Act 1**: Alchemical Chimera, Spire Architect.
  * **Act 2**: Clockwork Colossus, Sanguine Inquisitor, The Puppeteer.
  * **Act 3**: Chronos Titan, Astral Devourer, Mirrored Sovereign.
  * **Act 4**: **The Spire Heart** (900 HP, Beat of Death, Invincible phase, custom mechanics).
* 👥 **Full Multiplayer Co-Op Support**: Completely synchronized combat, status scaling, and specialized co-op cards (e.g. *Egoist*, *Divert & Scatter*, *Soul Transfusion*).
* 🌐 **Bilingual**: Full **Russian** and **English** localization with dynamic variable math formulas (`{Damage:diff()}`, `{Block:diff()}`).

---

## 📥 Installation

1. Download the latest `mods.zip` from [Releases](https://github.com/NightSculptor/sts2-ascension-unleashed/releases/latest) (or direct link: [mods.zip](https://github.com/NightSculptor/sts2-ascension-unleashed/releases/download/v1.0.0/mods.zip)).
2. Extract the archive into your game's root directory:
   ```
   <Slay The Spire 2 Directory>/mods/AnimeSurvivalMod/
   ```
3. Make sure the following files are inside `mods/AnimeSurvivalMod/`:
   * `AnimeSurvivalMod.dll`
   * `AnimeSurvivalMod.pck`
   * `manifest.json`
   * `images/`, `localization/`, `portraits/`, `powers/`
4. Launch the game and click **Accept / Enable Mods** in the main menu prompt.

---

## 🛠️ Building from Source

This mod is compiled using the in-game Roslyn compiler:
```powershell
powershell.exe -ExecutionPolicy Bypass -File "tools/compile_mod.ps1"
```
Or run `COMPILE_MOD.bat`.

---

## 📜 License & Credits

* **Author**: White
* **Special Thanks**: Mega Crit for Slay the Spire 2 and the modding community.
