# 📋 Gorkha: Rise of the Kingdom — Feature Roadmap & Task Backlog
## Aligning with Clash of Clans (CoC) & Age of Empires (AoE) Mechanics

**Document Version:** 1.0  
**Target Platforms:** Unity 3D Client (`unity-client/`) & Flutter/Flame Client (`game-client/`)  
**Design Philosophy:** Combine the intuitive base-building, tactile harvesting, and defensive base-layout strategy of **Clash of Clans** with the deep historical era progression, villager resource harvesting, RTS unit formations, and tactical counter-combat of **Age of Empires**.

---

## 🧭 Milestone Overview

```mermaid
flowchart LR
    M0["Phase 0: Graphics & Professional UX<br/>(Studio Visuals & Tactile Polish)"] --> M1["Phase 1: Economy & Village Life<br/>(CoC + AoE)"]
    M1 --> M2["Phase 2: Fortifications & Defense<br/>(CoC Style)"]
    M2 --> M3["Phase 3: Age Progression & Tech Tree<br/>(AoE Style)"]
    M3 --> M4["Phase 4: RTS Tactical Combat<br/>(AoE Formations + CoC Raids)"]
    M4 --> M5["Phase 5: Guthi / Clan System & Map<br/>(Campaign & CoC Alliances)"]
```

---

## 🎨 Phase 0: Graphics & Professional User Experience (UX/UI Polish)

| Task ID | Feature Title | Inspiration | Priority | Status |
| :--- | :--- | :--- | :--- | :--- |
| **GFX-01** | **URP Himalayan Lighting & Atmospheric Mist** | Studio Standard | **P0** | ✅ Completed |
| **GFX-02** | **Ground Terrain Shading, Cobblestone Walkways & Foliage** | AoE / CoC | **P0** | ✅ Completed |
| **GFX-03** | **Dynamic Particle Systems (Chimney Smoke, Torches, Dust Puffs)** | CoC | **P0** | ✅ Completed |
| **GFX-04** | **Procedural Himalayan Soundscape (Mountain Wind & Melodic Chimes)** | AoE | **P1** | ✅ Completed |
| **GFX-05** | **Fantasy Kingdom Unity 6 3D Asset Integration (Modular Buildings & SpeedTree Pines)** | Unity 6 / Synty | **P0** | ✅ Completed |
| **GFX-06** | **Bloom Glare Clamp & Lighting Shading Calibration (No Glare, 3D Medallions)** | Studio Standard | **P0** | ✅ Completed |
| **GFX-07** | **Himalayan Army Archetypes & Stylized Weapons (Khukuris, Spears, Bows, Shields)** | Synty / AoE | **P0** | ✅ Completed |
| **GFX-08** | **Kinetic Machinery & Armory Outpost Dressing (Spinning Windmill & Armory Props)** | Synty / CoC | **P0** | ✅ Completed |
| **GFX-09** | **Fantasy Kingdom Environment Parity (Tiled Ground, Lake, Grass, Walls, SSAO, VFX)** | Fantasy Kingdom | **P0** | ✅ Completed |
| **GFX-10** | **Articulated Procedural Skeletal Animations (Walk Cycles, Weapon Strikes, Flinch)** | AoE / Synty | **P0** | ✅ Completed |
| **GFX-11** | **Parabolic Ballistic Projectiles (Gravitational Flight & Sticking Arrow Impact)** | AoE / CoC | **P0** | ✅ Completed |
| **GFX-12** | **Synty POLYGON Humanoid 3D Character Models (`SM_Chr_Male_01` / `Female_01`)** | Synty Studios | **P0** | ✅ Completed |
| **UX-01** | **Clash of Clans Studio HUD Redesign (Resource Bars & Dock)** | CoC | **P0** | ✅ Completed |
| **UX-02** | **Tactile 3D Building Selection Ring & Bounce Hop** | CoC | **P0** | ✅ Completed |
| **UX-03** | **Signature CoC Harvest Bubbles & Floating Popups** | CoC | **P0** | ✅ Completed |
| **UX-04** | **Precision 3D Building Placement Grid & Snap Audio** | CoC / AoE | **P0** | ✅ Completed |
| **UX-05** | **Cinematic RTS Camera Inertia & Edge Panning** | AoE | **P1** | ✅ Completed |
| **UX-06** | **World-Space Dynamic Combat Health Bars (Billboarded Gradient & Auto-Fade)** | CoC / AoE | **P0** | ✅ Completed |

### Detailed Specifications (Phase 0)

#### GFX-01: URP Himalayan Lighting & Atmospheric Mist
- Golden-hour directional sun (`#FFF4E6`, 1.35x intensity) cast at $50^\circ$ elevation for long dramatic shadows.
- Multi-tier trilight ambient lighting: Alpine Sky Blue overhead, warm earth terraced midpoint, and rich forest shadow ground.
- Realistic distance fog (`FogMode.ExponentialSquared`) softly blending distant Himalayan peaks into the sky.

#### GFX-02: Ground Terrain Shading, Cobblestone Walkways & Foliage
- Tiled alpine grass with subtle per-tile variations and stone edge trims.
- Procedural cobblestone pathways radiating from Gorkha Durbar to outward workshops and military camps.
- Perimeter alpine pine trees with layered branches and mountain slate boulders.

#### GFX-03: Dynamic Particle Systems
- Gently curling chimney smoke rising from Town Center, cottage huts, and barracks forge.
- Warm flickering torch flames on Kot barracks gateposts and Watchtower corners.
- Ground dust puff effect triggering when placing buildings or completing construction.

#### GFX-04: Procedural Himalayan Soundscape
- Dynamic audio synthesizer enhancements: tactile wooden UI clicks, harp arpeggio harvest chimes, deep stone building thuds, and victory fanfare.
- Ambient mountain breeze loop with subtle temple bell resonance.

#### GFX-05: Fantasy Kingdom Unity 6 3D Asset Integration
- Seamless binding of Unity 6 Fantasy Kingdom modular 3D prefabs via `FantasyKingdomAssetRegistry`:
  - **Town Center (Durbar):** `Preset_Church_01_A.prefab` (grand stone foundation & soaring timber keep).
  - **Barracks (Kot):** `Preset_Blacksmith_01.prefab` (fortified armory with outdoor training compound).
  - **Farm (Krishi Kshetra):** `Preset_House_Windmill_01.prefab` (wind-driven mill with stone base).
  - **Woodcutter Camp:** `Preset_Shelter_02.prefab` (timber logging shelter).
  - **Stone Quarry:** `Preset_Hut_01.prefab` (stone masons' hut).
  - **Residential Cottages:** `Preset_House_01_A.prefab` (multi-tier Newari/medieval cottage).
  - **Watchtower (Suraksha Minar):** `Preset_Tower_01.prefab` (high defensive stone bastion).
  - **Perimeter & Harvest Scenery:** SpeedTree 9 wind-animated pine trees (`TreePine_VarA/B/C.prefab`) and granite boulders (`Rocks_01/11.prefab`).
- Exact mathematical bounding offsets and proportional RTS scaling so all models sit perfectly grounded on 2x2 grid footprints without floating or misalignment.
- Automatic procedural mesh generation fallback (`Nepali3DMeshBuilder`) whenever a prefab is unassigned.

#### GFX-06: Bloom Glare Clamp & Lighting Shading Calibration
- Clamped URP Bloom maximum threshold to $1.25$ (previously unclamped at $65472$) with softened golden-hour tint in `SampleSceneProfile.asset`, eliminating the blinding white glare over the Town Hall.
- Reduced Directional Sun intensity from $2.0$ down to natural Himalayan mountain ambient $1.15$ with softened shadow bias.
- Updated Royal Red Flag material (`GorkhaMaterials.RoyalRedFlag`) with double-sided rendering (`_Cull 0`) and vibrant crimson non-specular diffuse, fixing the shadowed black backface silhouette bug.
- Upgraded harvest floating bubbles from raw tilted yellow cubes to 3D golden coin medallions (`Nepali3DMeshBuilder.BuildHarvestMedallion`) with rim-glow and smooth collection popping animation.

#### GFX-07: Himalayan Army Archetypes & Stylized Weapons
- Multi-class 3D character generator (`Nepali3DMeshBuilder.BuildUnit`) matching Synty low-poly proportions:
  - **Khukuri Warrior:** Daura Suruwal tunic, Patuka crimson sash, Dhaka Topi with golden crest, curved steel Khukuri with Kauda cho notch, and round wooden buckler shield with brass rim and iron center boss.
  - **Gorkha Spearman:** Conical iron combat helmet with golden spire, 2.3m Salwood spear with steel leaf head and crimson pennant, and heavy infantry rectangular pavise tower shield with royal sun crest.
  - **Mountain Archer:** Scout headband with crimson feather plume, carved horn/wood reflex recurve bow, and leather back quiver containing 3 fletched arrows.
  - **Civilian Villager:** Woven bamboo straw sun hat, double-bitted lumber axe / mining pickaxe, and dynamic resource sack that visibly straps to the villager's back when harvesting timber or slate and disappears upon drop-off.
  - **Hostile Raider Bandit:** Slate-iron war vest, crimson war bandana with spikes, and dual-wielded raiding blades.

#### GFX-08: Kinetic Machinery & Armory Outpost Dressing
- **Kinetic Windmill Sails (`SmoothRotator`):** Continuous frame-rate independent rotation (28 deg/sec around local Z axis) operating seamlessly in both Edit Mode and Play Mode, mounted precisely at the hub axle of `Preset_House_Windmill_01`.
- **Kot Barracks Armory Yard:** Dressed with heavy forged blacksmith anvil on tree stump block, A-frame wooden weapon racks displaying upright spears, and straw concentric archery targets with stuck practice arrows.
- **Outpost Supply Stacks:** Timber cargo crates and iron-banded storage barrels clustered outside logging camps, stone quarries, farms, and Town Center command posts.

#### GFX-09: Fantasy Kingdom Environment Parity
- **Textured Tiled Alpine Ground:** Replaced flat single-color boxes with a high-resolution 16x16 tiled terrain mesh utilizing `Atlas_Ground_Rocks.mat` (`Atlas_Ground_Base_Color.png` & `Atlas_Ground_Normal.png`), rendering sharp pebbles, grass tufts, and mountain loam.
- **Living Alpine Lake & Stream:** Scenic western water plane (`WaterLake.mat`) with animated depth wave displacement, sun glints, depth foam shorelines, submerged lakebed, and shoreline granite boulders.
- **Waving Meadow Grass Scatter:** Clustered `Scatter_Grass` prefabs with vertex wind sway (`Atlas_Foliage`) lining the road shoulders, boulders, and perimeter.
- **Fortified Town Wall Perimeter:** Modular stone fortress battlements (`SM_Town_Wall_Border_01`), corner bastion watchtowers (`SM_Town_Wall_Border_04`), and fortified stone gatehouse archways (`SM_Town_Wall_Border_10`) enclosing the kingdom.
- **Desktop Volume Profile & SSAO Contact Shadows:** Upgraded active scenes to `Global Volume Profile Desktop.asset` with Screen Space Ambient Occlusion (SSAO) eliminating floating artifacts, ACES Tonemapping, Color Adjustments, White Balance, and Vignette.
- **Living Himalayan Atmospheric VFX (`HimalayanAtmosphericVFX`):** Integrated Visual Effect Graph systems for curling chimney smoke plumes, windblown autumn leaves tumbling through village streets, sunlit pollen dust motes, and soaring eagle/crow flocks.

#### UX-01: Clash of Clans Studio HUD Redesign
- Custom high-contrast slate cards with golden filigree borders for top resource trackers (Food, Wood, Stone, Gold, Gems).
- Resource capacity progress bars and live production rate badges (+/sec).
- Bottom construction dock featuring categorized building blueprint cards with cost indicators and hover glow.

#### UX-02: Tactile 3D Building Selection Ring & Bounce Hop
- Animated glowing selection baseplate/ring on ground beneath active building.
- Spring-physics "bounce hop" when clicking buildings or harvesting.
- Quick action radial/context dock: Upgrade, Info, Collect, Close.

#### UX-03: Signature CoC Harvest Bubbles & Floating Popups
- 3D floating orb floating above resource buildings (coin, sheaf, timber, ore).
- One-click tap with spring-pop collection animation and rising gold floating text.

#### UX-04: Precision 3D Building Placement Grid & Snap Audio
- Real-time grid projector under cursor showing emerald-green footprint for valid placement, ruby-red for blocked tiles.
- Grid-cell audio snapping sound as player sweeps blueprint across the terrain.

#### UX-05: Cinematic RTS Camera Inertia & Edge Panning
- Smooth damping inertia for keyboard WASD and mouse-drag panning.
- Optional RTS edge panning (moving mouse to viewport boundary pans the view).
- Smooth exponential zoom and double-click building focus.

#### GFX-10: Articulated Procedural Skeletal Animations
- Articulated joint hierarchy (`HipLeft`, `HipRight`, `ShoulderLeft`, `ShoulderRight`, `Torso`, `Head`) driven by `UnitProceduralAnimator`.
- Opposing harmonic hip walk cycles ($\pm 32^\circ$) and counter-phase shoulder swings ($\pm 24^\circ$) matching real human locomotion.
- True weapon combat routines:
  - **Khukuri Warrior:** High windup raise, downward cross slash across chest, and recovery.
  - **Spearman:** Rear-foot plant and two-handed forward spear lunge ($0.45\text{ m}$ forward drive).
  - **Mountain Archer:** Left-arm bow elevation, right-hand bowstring draw to cheek, and snap release with bow vibration recoil.
  - **Villager:** Overhead two-handed axe/pickaxe power chop rhythmically striking trees and stone boulders.
  - **Hit Flinch:** Knockback torso and head recoil with quick recovery.

#### GFX-11: Parabolic Ballistic Projectiles
- True gravitational parabolic flight paths (`BallisticProjectile`) for physical arrows fired by Watchtowers and Archers.
- Real-time orientation matching trajectory velocity vector $\vec{v}(t)$ with dynamic target leading.
- Sticking impact realism: arrows stick directly into targeted units or buildings upon impact for 2.8 seconds.

#### UX-06: World-Space Dynamic Combat Health Bars
- Camera-billboarded, high-contrast slate-bordered health bars (`WorldCombatHealthBar`) hovering above damaged units ($1.85\text{ m}$) and buildings ($3.2\text{ m}$ / $4.8\text{ m}$).
- Smooth color gradient transition: Emerald Green ($>55\%$) $\to$ Golden Amber ($55\% - 25\%$) $\to$ Ruby Crimson ($<25\%$).
- Auto-hides when units/buildings are at full health during peace, appearing instantly upon taking damage and fading after combat resolves.

#### GFX-12: Synty POLYGON Humanoid 3D Character Integration
- Replaced blocky procedural character models with Synty Studios **POLYGON - Starter Pack** rigged humanoid 3D character meshes (`SM_Chr_Male_01` and `SM_Chr_Female_01`).
- Dynamically rigged and accessorized with authentic historic Nepali gear via `Nepali3DMeshBuilder`:
  - **Khukuri Warrior (*Asal Sipahi*):** Traditional geometric Dhaka Topi with golden sun crest, curved steel Khukuri blade in right hand, round brass-rim buckler shield on left arm.
  - **Gorkha Spearman (*Bhaledar*):** Conical iron combat helmet with golden Gajur spire, 2.3m Salwood spear with royal red pennant in right hand, heavy infantry rectangular pavise shield on left arm.
  - **Mountain Archer (*Dhanurdhari*):** Scout headband with crimson feather plume, carved horn/wood reflex recurve bow in left hand, back leather quiver with fletched arrows.
  - **Civilian Villager:** Woven bamboo straw sun hat (*Ghum*), lumberjack axe / mining pickaxe, dynamic carried resource back-sack.
  - **Hostile Bandit Raider:** Spiked bandit war bandana, dark slate iron palette (`PolygonStarter_03.mat`), dual blades.
- Sockets weapons directly into hand bones (`Hand_R`, `Hand_L`), headwear onto `Head`, and quivers/sacks onto `Spine_02`.
- Hair mesh (`Character_Hair_Male_01` / `Female_01`) dynamically hidden when wearing enclosed helmets or hats to prevent clipping.
- Procedural animator (`UnitProceduralAnimator`) upgraded to automatically bind Synty bone hierarchies (`UpperLeg_L/R`, `Shoulder_L/R`, `Hips`, `Head`) and multiply relative rotations onto rest poses, supporting both Synty humanoid and procedural rigs.
- Auto-upgrades existing units in scenes upon launch via `UnitController.EnsureVisuals()`.

---

## 🌾 Phase 1: Economy, Villager Work & Base Management (CoC + AoE)

| Task ID | Feature Title | Inspiration | Priority | Status |
| :--- | :--- | :--- | :--- | :--- |
| **ECO-01** | **Resource Storage & Capacity Cap System** | CoC | **P0** | ✅ Completed |
| **ECO-02** | **Villager Work Loop (Tree Chopping & Stone Mining)** | AoE | **P0** | ✅ Completed |
| **ECO-03** | **Builder / Craftsman Management (Dakshya Karmi)** | CoC | **P1** | ✅ Completed |
| **ECO-04** | **Building Relocation & Smooth Drag-to-Move** | CoC | **P1** | ✅ Completed |
| **ECO-05** | **Mini-Map Navigation & Radar View** | AoE | **P1** | ✅ Completed |

### Detailed Specifications

#### ECO-01: Resource Storage & Capacity Cap System (CoC)
- **Concept:** Differentiate between **Production Collectors** (Farms, Lumber Camps, Quarries) and **Primary Storages** (Granary for Food, Treasury/Vault for Gold, Timber Yard for Wood, Slate Depot for Stone).
- **Gameplay Mechanics:**
  - Resource buildings slowly accumulate uncollected yields up to their internal buffer.
  - Clicking the floating 3D bubble or building transfers resources into the kingdom's primary storages.
  - Primary storages have capacity caps that can be increased by upgrading them.
  - Visual gauge on storage buildings showing 0%, 25%, 50%, 75%, 100% fill levels.

#### ECO-02: Villager Work Loop (Tree Chopping, Stone Mining, Farming) (AoE)
- **Concept:** Bring the settlement to life with functional civilian villagers (*Kisan* & *Karmi*) who physically gather resources.
- **Gameplay Mechanics:**
  - Clicking a tree orders an idle villager to chop it with an axe, carrying wood bundles back to the nearest Lumber Camp or Town Center.
  - Clicking a mountain slate boulder orders a villager to mine stone with a pickaxe.
  - Villagers working on terraced farms cultivate and harvest wheat sheaves.
  - Trees and boulders slowly deplete and disappear, allowing players to clear the forest to expand their village layout (just like AoE & CoC obstacles).

#### ECO-03: Builder / Craftsman Queue (Dakshya Karmi) (CoC)
- **Concept:** Limit simultaneous construction and upgrades by the number of active master builders.
- **Gameplay Mechanics:**
  - Start with 2 Builder Huts (*Karmi Chhauni*), upgradable/purchasable up to 5 with Gems.
  - When an upgrade or construction begins, an animated builder walks to the site and works with scaffolding.
  - If all builders are busy, construction queue warns: *"All builders are busy!"* with option to free one using Gems.

#### ECO-04: Building Relocation & Edit Mode (CoC)
- **Concept:** Allow players to reorganize their settlement layout without demolishing buildings.
- **Gameplay Mechanics:**
  - Press-and-hold or click "Move" on any building to lift it.
  - Shows green/red footprint preview as it drags across the 3D grid.
  - Releasing places the building in the new valid location.
  - Dedicated "Layout Editor" mode allowing one-click clearing and restructuring of village walls and defenses.

#### ECO-05: Mini-Map with Live Radar (AoE)
- **Concept:** Corner overview map displaying terrain boundaries, friendly buildings, units, and resource deposits.
- **Gameplay Mechanics:**
  - Renders 2D top-down isometric diamond projection of the 24x24 / 48x48 world.
  - Color-coded blips: Blue (Town Center/Buildings), Yellow (Resource Collectors), Green (Friendly Units), Red (Hostile Patrols).
  - White rectangle shows current camera viewport.
  - Clicking any point on the mini-map smoothly pans the camera there instantly.

---

## 🏰 Phase 2: Fortifications, Defenses & Base Layout Strategy (CoC Style)

| Task ID | Feature Title | Inspiration | Priority | Status |
| :--- | :--- | :--- | :--- | :--- |
| **DEF-01** | **Placeable & Connectable Fortification Walls** | CoC / AoE | **P0** | ✅ Completed |
| **DEF-02** | **Archer Watchtowers with Auto-Targeting Radius** | CoC / AoE | **P0** | ✅ Completed |
| **DEF-03** | **Kaman (Mountain Cannon) Heavy Defense** | CoC | **P1** | ✅ Completed |
| **DEF-04** | **Himalayan Concealed Traps (Pit Traps & Rolling Logs)** | CoC | **P1** | ✅ Completed |
| **DEF-05** | **Settlement Shield & Peace Treaty Timer** | CoC | **P2** | ✅ Completed |

### Detailed Specifications

#### DEF-01: Placeable & Connectable Fortification Walls (CoC / AoE)
- **Concept:** 1x1 tile stone and timber walls that dynamically connect to adjacent wall segments.
- **Gameplay Mechanics:**
  - Players drag lines of walls to enclose their Gorkha Durbar and resource storages.
  - Automatic corner posts, straight connectors, and T-junctions.
  - Level progression:
    - **Lv 1:** Bamboo & Sal Wood Stakes (*Kandeghari*)
    - **Lv 2:** Clay & Terracotta Brick Ramparts (*Mato Killa*)
    - **Lv 3:** Dressed Himalayan Slate Stone Wall (*Dhungo Killa*)
    - **Lv 4:** Iron-Reinforced Fortress Bastions with crenellations.
  - Wall Gates (*Dhoka*) allowing friendly units to pass through while keeping enemies out.

#### DEF-02: Archer Watchtowers with Auto-Targeting Radius (CoC / AoE)
- **Concept:** Defensive towers with garrisons that attack hostile units entering their perimeter.
- **Gameplay Mechanics:**
  - Selecting a tower reveals a translucent circular range radius (e.g. 10 meters).
  - 3D archer stationed on top aims and fires animated physical arrows with parabolic arcs at the closest enemy unit.
  - High ground advantage: Towers placed on mountain ridges gain +15% range and damage bonus.

#### DEF-03: Kaman (Himalayan Mountain Cannon) Heavy Defense (CoC Mortar Style)
- **Concept:** Long-range, slow-firing heavy mortar battery based on historical Gorkhali bronze cannons.
- **Gameplay Mechanics:**
  - Has a minimum blind-spot radius (2m) and long maximum range (16m).
  - Fires explosive cannonballs with splash damage that knock down clusters of light infantry.
  - Deep percussive cannon blast audio with smoke particle effects.

#### DEF-04: Himalayan Concealed Traps (CoC Spring Trap Style)
- **Concept:** Hidden traps placed in pathways between walls that trigger when enemy troops walk over them.
- **Gameplay Mechanics:**
  - Invisible to enemy players in battle view; visible to defending player in village view.
  - **Spike Pit (*Khopte Pasol*):** Deals burst puncture damage to first 3 ground units.
  - **Rolling Mountain Logs (*Pahadi Dhunga*):** Triggers a cascade of boulders down mountain terraces, crushing units in a directional line.

---

## 🏛️ Phase 3: Age Progression & Historical Tech Tree (AoE Style)

| Task ID | Feature Title | Inspiration | Priority | Status |
| :--- | :--- | :--- | :--- | :--- |
| **AGE-01** | **Four Historical Ages of Gorkha** | AoE | **P0** | ✅ Completed |
| **AGE-02** | **Town Center Upgrade Gating & Unlocks** | CoC / AoE | **P0** | ✅ Completed |
| **AGE-03** | **Academy of Arts & Warfare (Research Tech Tree)** | AoE | **P1** | ⏳ Planned |
| **AGE-04** | **Wonder of the World Construction Victory** | AoE | **P2** | ⏳ Planned |

### Detailed Specifications

#### AGE-01: Four Historical Ages of Gorkha (AoE Era Progression)
- **Era I: Baise-Chaubisi Principality Era (Founding of Gorkha, 1559–1740)**
  - Rustic wood and thatch huts, mud brick walls, basic spear militia, and gathering economies.
- **Era II: Fortified Kot Era (Kalu Pande Military Reforms, 1740–1768)**
  - Dressed stone fortifications, Newari brick pagoda durbar, Khukuri shock troops, composite archers.
- **Era III: Unification Era (Prithvi Narayan Shah & Valley Conquest, 1768–1814)**
  - Tier-3 Gorkha Durbar palace, matchlock musket infantry (*Bandukdhari*), bronze cannons, paved stone paths.
- **Era IV: Himalayan Empire Era (Anglo-Gorkha & Tibetan Expeditions, 1814+)**
  - Fortresses (*Nalapani style*), Gurkha rifle regiments, iron foundries, grand mountain citadels.
- **Visual Transformation:** Upgrading the Town Center transforms all settlement buildings with richer materials, tile roofs, and larger footprints.

#### AGE-02: Town Center Gating & Unlocks (CoC Town Hall Style)
- **Concept:** Gorkha Durbar level defines the maximum building count and unlocks new structures.
- **Progression Table:**
  - **Durbar Lv 1:** 1 Farm, 1 Wood Camp, 1 Barracks, 25 Walls, Spearmen.
  - **Durbar Lv 2:** +1 Farm, +1 Quarry, +1 Watchtower, +25 Walls, Khukuri Warriors, Granary.
  - **Durbar Lv 3:** +1 Barracks, +1 Cannon (Kaman), +50 Walls, Archers, Blacksmith.
  - **Durbar Lv 4:** Mountain Citadel, Musket Infantry, Wonder Foundation.

#### AGE-03: Academy of Warfare (Vidya Kendra / Blacksmith Tech Tree) (AoE)
- **Concept:** Dedicated research building to unlock passive buffs and upgrades:
  - **Forged Khukuri Blades:** +20% melee damage for all Khukuri warriors.
  - **Sal Wood Arrow Shafts:** +15% range and projectile velocity for Archers.
  - **Himalayan Terracing:** +25% Food production rate across all Farms.
  - **Paved Slate Roads:** +20% movement speed for villagers and units walking on paths.

#### AGE-04: Himalayan Wonder Construction (AoE Victory Condition)
- **Concept:** Massive, multi-stage mega-structure (*e.g. Swayambhunath Stupa or Kasthamandap Citadel*).
- **Gameplay Mechanics:**
  - Requires immense resource investments (5,000 Wood, 5,000 Stone, 2,000 Gold).
  - Built across 4 visual construction stages.
  - Once completed, triggers a defensive countdown timer: survive enemy onslaughts to achieve Victory!

---

## ⚔️ Phase 4: RTS Tactical Combat & Raiding (AoE Formations + CoC Raids)

| Task ID | Feature Title | Inspiration | Priority | Status |
| :--- | :--- | :--- | :--- | :--- |
| **CBT-00** | **Living Garrison Active Defense & Raid AI (Melee & Ranged Combat)** | AoE / CoC | **P0** | ✅ Completed |
| **CBT-01** | **Multi-Unit Drag-Box Selection & Group Hotkeys** | AoE | **P0** | ✅ Completed |
| **CBT-02** | **Tactical Unit Formations (Line, Box, Wedge, Staggered)** | AoE | **P1** | ✅ Completed |
| **CBT-03** | **Rock-Paper-Scissors Combat Triangle** | AoE | **P0** | ✅ Completed |
| **CBT-04** | **Attack-Move, Hold Position & Patrol Commands** | AoE | **P1** | ✅ Completed |
| **CBT-05** | **CoC-Style Asynchronous Village Raid Mode** | CoC | **P1** | ✅ Completed |
| **CBT-06** | **Hero Generals (Prithvi Narayan Shah, Kalu Pande)** | CoC / AoE | **P2** | ⏳ Planned |

### Detailed Specifications

#### CBT-00: Living Garrison Active Defense & Raid AI
- **Garrison Defense AI:** Patrolling Gorkha military units (Khukuri Warriors, Spearmen, Archers) automatically detect hostile bandits within a 15-meter alert radius.
- **Class-Specific Engagement Ranges:**
  - *Khukuri Warriors:* Close into $1.35\text{ m}$ melee range and unleash swift downward cross slashes.
  - *Gorkha Spearmen:* Stand at $1.9\text{ m}$ spear reach to pierce attackers with two-handed forward lunges.
  - *Mountain Archers:* Hold position at $7.5\text{ m}$ standoff range, draw recurve bowstrings to cheek, and fire physical parabolic ballistic arrows.
- **Bandit Raider AI:** Bandits prioritize attacking nearby defenders or siege village structures, retaliating with dual blade slashes.
- **Watchtower Integration:** Archer Watchtowers fire parabolic ballistic arrows with arc trajectories and target leading, sticking into struck targets.

#### CBT-01: Drag-Box Selection & Control Groups (AoE RTS Controls)
- **Concept:** Click-and-drag green selection box to highlight multiple units simultaneously.
- **Controls:**
  - Left-click drag creates marquee selection box.
  - Double-click a unit selects all units of that type currently on screen.
  - `Ctrl + 1-9` assigns units to a Control Group; pressing `1-9` re-selects them; double-tapping centers camera.
  - Right-click commands the selected group to move or attack target.

#### CBT-02: Tactical Formations (AoE Style)
- **Formations:**
  - **Line Formation:** Front-line wall of Spearmen to intercept charges.
  - **Box Formation:** Spearmen and swordsmen surround fragile Archers and monks in the center.
  - **Wedge Formation:** Khukuri shock troops break through enemy lines.
  - **Staggered Formation:** Spreads units apart to minimize damage from Cannon/Catapult splash explosions.

#### CBT-03: Authentic Unit Combat Triangle (AoE Counter System)
```
          🗡️ Spearmen
          /         \
   (Counters)     (Countered by)
        /             \
       v               v
🐎 Cavalry / Beasts <--- 🏹 Archers
       ^               |
        \   (Counters) |
    (Countered by)     |
          \            v
         ⚔️ Khukuri Warriors
```
- **Spearmen (*Bhaledar*):** Bonus damage vs mounted cavalry and heavy armored units; weak to ranged archers.
- **Khukuri Warriors (*Asal Sipahi*):** High movement speed and rapid melee attack; flanks and shreds archers; vulnerable to defensive walls and cannon splash.
- **Archers (*Dhanurdhari*):** High range; picks off spearmen and unarmored targets; vulnerable to fast flankers.
- **Musketeers (*Bandukdhari* - Era III):** High single-target armor piercing damage with slow reload time.

#### CBT-05: Village Raid / Siege Mode (CoC Style)
- **Concept:** Deploy recruited armies onto the perimeter of an enemy settlement to destroy their defenses and loot resources.
- **Gameplay Mechanics:**
  - Red deployment border around the perimeter of the defending village.
  - Deploy units by clicking on the boundary.
  - Units automatically seek targets according to their AI preference:
    - Spearmen attack nearest structure.
    - Khukuri warriors target enemy defense towers first.
    - Villagers/Looters prioritize resource storages and granaries.
  - 3-Star Victory Metric:
    - ⭐ 1 Star: 50% settlement destruction OR Town Center destroyed.
    - ⭐ 2 Stars: 50% destruction AND Town Center destroyed.
    - ⭐ 3 Stars: 100% total eradication of enemy base.

#### CBT-06: Historic Hero Generals (CoC Hero / AoE Hero Units)
- **Prithvi Narayan Shah (The Unifier):** Aura increases nearby unit attack damage by +20%; Special ability: *Call to Arms* (instantly summons 4 royal bodyguards).
- **Kalu Pande (The Grand Commander):** Passive armor boost to infantry; Special ability: *Shield Wall* (reduces incoming ranged damage by 50% for 8 seconds).
- **Bhakti Thapa (The Lion of Nalapani):** Heroic last stand; gains attack speed as health drops.

---

## 🤝 Phase 5: Guthi (Clan) System, Alliances & Regional Campaign Map

| Task ID | Feature Title | Inspiration | Priority | Status |
| :--- | :--- | :--- | :--- | :--- |
| **SOC-01** | **Guthi Brotherhood (Clan System)** | CoC | **P1** | ⏳ Planned |
| **SOC-02** | **Troop Reinforcement Donations (Guthi Durbar)** | CoC | **P1** | ⏳ Planned |
| **SOC-03** | **Historical Himalayan Campaign Map (Nuwakot, Makwanpur)** | AoE | **P1** | ⏳ Planned |
| **SOC-04** | **Clan Wars (Guthi vs Guthi Territory Battles)** | CoC | **P2** | ⏳ Planned |

### Detailed Specifications

#### SOC-01: Guthi Brotherhood (Clan System) (CoC)
- **Concept:** Based on Nepal’s historic *Guthi* socio-military cooperatives.
- **Features:**
  - Create or join a Guthi with custom emblems (Double-Triangle flag, crossed Khukuris, mountain peak, golden bell).
  - In-game clan chat and shared achievements.
  - Leaderboards ranking Guthis by total conquered territories and trophies.

#### SOC-02: Troop Donations & Guthi Castle (CoC Clan Castle Style)
- **Concept:** Construct a *Guthi Durbar* in your settlement to receive elite reinforcements from clan members.
- **Mechanics:**
  - Request troops (e.g. 15 troop housing capacity).
  - Clan members donate elite warriors who live in the Guthi Durbar.
  - In defensive raids, donated troops march out of the Guthi Durbar to defend your settlement against attackers!

#### SOC-03: Historical Himalayan Campaign Map (AoE Campaign)
- **Campaign Chapters:**
  1. **Act I: The Strategic Gate:** Conquest of Nuwakot (1744) — Master mountain pass logistics and flank fortified ridges.
  2. **Act II: The Southern Trade Route:** Battle of Makwanpur (1762) — Counter heavy cavalry in valley terrain.
  3. **Act III: Valley of the Gods:** Unification of Kathmandu, Patan, and Bhaktapur (1768–1769) — Infiltrate and siege ancient brick-walled pagoda cities during Indra Jatra.
  4. **Act IV: The Southern Storm:** Defense of Sindhuligadhi (1767) — Repel British Captain Kinloch’s expedition through mountain guerilla warfare and hornet nests!

---

## 🎨 Phase 6: Visual & Audio Polish Checklist

- [ ] **Dynamic Day-Night Cycle:** Himalayan sunrise, golden afternoon, and lantern-lit mountain night with glowing paper windows.
- [ ] **Ambient Himalayan Wildlife:** Mountain yaks grazing in pastures, Himalayan eagles soaring overhead, and musk deer in forests.
- [ ] **Weather & Seasons:** Mountain mist rising from valleys, monsoon rain effects on terracotta roofs, and winter snow caps on pagoda eaves.
- [x] **Authentic Traditional Audio (AUD-01, AUD-02, AUD-03):** ✅ Completed!
  - **`AUD-01` Procedural Himalayan Instruments & Combat SFX:** Real-time synthesis of Bansuri bamboo flute (Raga Pahadi), Madal hand drums (Dayan treble slap & Bayan sub thud), Dhimay war drums, Sarangi strings, steel Khukuri clashes, pavise shield blocks, bowstring twangs, and structure breaches in `GorkhaAudio`.
  - **`AUD-02` Authentic Nepali Voice Barks & Speech Callouts:** Real-time formant vocal synthesizer generating localized barks (*"हजुर! (Hajur!)"*, *"आदेश दिनुहोस्! (Aadesh!)"*, *"हुन्छ! (Hunchha!)"*, *"आक्रमण! (Aakraman!)"*, *"जय महाकाली! (Jai Mahakali!)"*) paired with floating speech balloons in `GorkhaVoiceBarks`.
  - **`AUD-03` Dynamic Multi-Tier Himalayan Soundscape:** Real-time environmental soundscape blending village calm/busy murmurs, watershore lake waves, altitude wind, peace flute melodies, and 110 BPM Madal war drum battle cadence during raids in `HimalayanSoundscape`.

---

## 🛠️ Sprint 1 Recommended Tasks (Starting Now)

To immediately make the game feel like Clash of Clans & Age of Empires, here are the **Top 4 Sprint Tasks**:

1. **`DEF-01` Placeable Walls & Gates:** Add a 1x1 wall blueprint to the bottom dock so players can wall in their Town Center and create fortresses.
2. **`ECO-01` Resource Storage Caps & Granary:** Add Granary and Treasury buildings so storage capacity matters and players upgrade them.
3. **`CBT-01` Multi-Unit Box Selection & March Command:** Implement click-and-drag box selection to command groups of warriors to march and guard together.
4. **`DEF-02` Archer Watchtower Auto-Attack:** Make the watchtower actively detect and fire arrows at wandering enemy patrols within radius.
