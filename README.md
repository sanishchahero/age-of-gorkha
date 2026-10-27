# Gorkha: Rise of the Kingdom (Unity 3D Client)

Historically grounded Nepali 3D grand-strategy settlement builder, Age of Empires style RTS combat, and Clash of Clans base-building simulation.

---

## 🚀 Quick Setup in Unity Hub

1. Open **Unity Hub** (installed in `/Applications/Unity Hub.app`).
2. Log in with your Unity ID.
3. Under **Installs**, ensure you have **Unity 6** (recommended: `6000.0.x`) or **Unity 2022.3 LTS**.
4. In **Projects**, click **Add** -> **Add project from disk**.
5. Select this directory:  
   `/Users/sanishmaharjanengineering/Downloads/free/game/unity-client`
6. Click **Open**!
7. **One-Click 3D Scene Setup:**
   In the Unity menu bar, select:  
   **`Gorkha` -> `Generate 3D Settlement Scene & Prefabs`**  
   This constructs all 3D building prefabs, terrain, lighting, materials, and saves `Assets/Scenes/MainSettlementScene.unity`.
8. Click **Play (▶️)** to experience the 3D Himalayan kingdom immediately!

---

## 🏛️ Key Gameplay Systems

### 1. Four Historical Eras of Gorkha (Age Progression)
- **Era I: Baise-Chaubisi Principality (1559–1740):** Founding of Gorkha atop the ridge by King Dravya Shah. Single-tier timber/thatch lodge, basic spear militia, and agrarian gathering economy.
- **Era II: Fortified Kot Era (1740–1768):** Kalu Pande military reforms. Two-tier red brick pagoda keep, slate stone plinth, Khukuri shock troops, Watchtowers, and Granaries.
- **Era III: Unification Era (1768–1814):** Prithvi Narayan Shah & Kathmandu Valley conquest. Three-tier grand royal palace, golden Gajur spire, waving royal standard, bronze Kaman cannons, and Mountain Archers.
- **Era IV: Himalayan Empire Era (1814+):** Grand imperial citadel with 4 tiers, 4 corner fortress bastions, dual battle standards, and Wonder of the World foundation.
- **Town Center Visual Morphing:** Upgrading the Town Center instantly morphs the 3D architecture to the new era tier with celebration fanfare!

### 2. RTS Tactical Combat & Control System (AoE Style)
- **Marquee Selection:** Click-and-drag green selection box to select multiple units simultaneously. Double-click any unit to select all visible units of that type.
- **Control Groups (`Ctrl + 1..9`):** Assign military squads to number keys; single-tap recalls group; double-tap centers camera.
- **Tactical Stances & Hotkeys:**
  - `[A]` **Attack-Move:** March in formation towards destination, automatically scanning and engaging any hostile enemies or structures encountered along the path.
  - `[H]` **Hold Position:** Units anchor to their ground spot, refusing to break formation or chase fleeing targets, engaging only enemies within weapon reach.
  - `[P]` **Patrol:** Units march continuously between two waypoints, engaging enemies encountered and resuming their patrol loop after combat.
  - `[S]` **Halt / Stop:** Instantly cancels current movement, stances, and targeting.
  - `[Z/X/C/V]` **Formations:** Line [Z], Box [X], Wedge [C], and Staggered [V] with dynamic slot calculation.
- **Authentic Combat Triangle:**
  - *Spearmen* counter Khukuri Warriors (+35% damage, Spear Wall).
  - *Khukuri Warriors* flank and shred Archers (+50% damage, Flank Slash).
  - *Mountain Archers* counter Spearmen (+40% damage, Ranged Standoff).

### 3. Clash of Clans Style Raid Battle Mode
- **Outpost Raid Expeditions:** Launch invasions against 3 distinct difficulty outposts:
  - *Tier 1: Rival Chaubisi Kot (Easy)* — 600 Food, 450 Gold, 5 Gems, 4 Bandit defenders.
  - *Tier 2: Bandit Mountain Stronghold (Medium)* — 1,600 Food, 1,200 Gold, 12 Gems, Watchtowers & Cannons.
  - *Tier 3: Valley Warlord Citadel (Hard)* — 3,800 Food, 3,000 Gold, 25 Gems, 14 Elite defenders.
- **CoC 3-Star Victory Evaluation:**
  - ⭐ **Star 1:** Awarded upon reaching 50% Outpost Destruction.
  - ⭐ **Star 2:** Awarded upon destroying the Enemy Town Center Keep.
  - ⭐ **Star 3:** Awarded upon 100% Total Obliteration of the outpost.
- **Real-time Plundering:** Plunder loot from damaged/destroyed enemy storages. Return to village to deposit spoils directly into kingdom resources.

### 4. Audio & Cultural Immersion
- **Spoken Nepali Voice Barks:** 18 synthesized authentic spoken Nepali WAV clips (*"Hajur, aadesh dinuhos"*, *"Aakraman!"*, *"Rokkinuhos"*, *"Gorkhali aayo!"*).
- **Himalayan Soundscape:** Ambient mountain breezes, spatial pine forest bird calls, distant monastery prayer chimes, pastoral farm audio, and dynamic 110 BPM Madal war drum combat cadence.

---

## 🏗️ Architecture & Scripts

```text
Assets/
├── Audio/
│   └── VoiceBarks/                          # 18 Spoken Nepali voice clips (.wav + .meta)
├── Prefabs/                                 # 3D Unit and building prefabs
├── Scripts/
│   ├── Audio/
│   │   ├── GorkhaAudio.cs                   # Master procedural audio synthesizer & sound effects
│   │   ├── GorkhaVoiceBarks.cs              # Voice bark dispatch with fallback direct WAV parser
│   │   └── HimalayanSoundscape.cs           # Living procedural soundscape & dynamic war drums
│   ├── Buildings/
│   │   ├── BuildingData.cs                  # ScriptableObject blueprints (costs, footprint, timers)
│   │   ├── BuildingInstance.cs              # 3D building lifecycle, health, harvest bubbles, levels
│   │   ├── BuildingPlacementController.cs   # CoC-style 3D drag-to-place with green/red footprint
│   │   └── WallInstance.cs                  # Connectable dynamic 1x1 stone/timber fortress walls
│   ├── Camera/
│   │   └── IsometricCameraController.cs     # 45° smooth isometric pan, drag, and pinch-zoom
│   ├── Combat/
│   │   └── GorkhaRaidBattleController.cs    # CoC Outpost Raid Battles, 3-star rating, plunder tally
│   ├── Core/
│   │   ├── GorkhaGameInitializer.cs         # Master scene bootstrapper & manager lifecycle
│   │   ├── IsometricGrid.cs                 # 3D Grid mapping, tile queries, and occupancy tracking
│   │   └── GorkhaInput.cs                   # Cross-platform unified mouse & keyboard input
│   ├── Economy/
│   │   ├── BuilderManager.cs                # Craftsman (Dakshya Karmi) queue & hut assignment
│   │   ├── ResourceManager.cs               # Live resource ticking, capacities, peace treaty shields
│   │   ├── ResourceNode.cs                  # Harvestable SpeedTree pine trees and slate boulders
│   │   └── ResourceType.cs                  # Food, Wood, Stone, Iron, Gold, Gems
│   ├── Graphics/
│   │   ├── BallisticProjectile.cs           # Gravitational parabolic arrow flight & sticking impact
│   │   ├── FantasyKingdomAssetRegistry.cs   # Modular 3D asset bindings & calibration
│   │   ├── FlagClothAnimator.cs             # Real-time cloth wind simulation for double-triangle flag
│   │   ├── GorkhaMaterials.cs               # Centralized PBR material registry (URP Lit & Standard)
│   │   ├── GorkhaVFX.cs                     # Particle systems (chimney smoke, torches, placement dust)
│   │   ├── HimalayanAtmosphericVFX.cs       # Atmospheric falling leaves, wind motes, soaring eagles
│   │   ├── HimalayanEnvironment3D.cs        # 3D terraced terrain, golden sun, fog, and water stream
│   │   ├── Nepali3DMeshBuilder.cs           # 4-tier procedural Town Center, defenses & character rigs
│   │   └── WorldCombatHealthBar.cs          # Billboarded floating combat health bars with auto-fade
│   ├── Progression/
│   │   ├── AgeManager.cs                    # 4 Historical Eras of Gorkha & advancement fanfare
│   │   └── BuildingGatingManager.cs         # CoC Town Hall style building caps & unit gating
│   ├── UI/
│   │   ├── GorkhaMiniMap.cs                 # Radar mini-map with camera viewport and click-to-pan
│   │   └── GorkhaPlayableHUD.cs             # Studio HUD, era banner, build dock, tactical unit inspector
│   └── Units/
│       ├── ArmyFormationController.cs       # Line, Box, Wedge, Staggered formation slot math
│       ├── RTSSelectionManager.cs           # Marquee drag selection, control groups, tactical hotkeys
│       ├── UnitController.cs                # Tactical stances (Attack-Move, Hold, Patrol, Stop)
│       └── UnitProceduralAnimator.cs        # Rigged walk cycles, slashes, spear lunges, bow releases
```

---

## 🧪 Headless Build Verification

To verify C# script compilation without opening the Unity Editor:
```bash
bash tools/compile_check.sh
```
Guaranteed `0 errors, 0 warnings` at all times.
