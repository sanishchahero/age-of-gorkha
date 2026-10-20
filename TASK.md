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
| **UX-01** | **Clash of Clans Studio HUD Redesign (Resource Bars & Dock)** | CoC | **P0** | ✅ Completed |
| **UX-02** | **Tactile 3D Building Selection Ring & Bounce Hop** | CoC | **P0** | ✅ Completed |
| **UX-03** | **Signature CoC Harvest Bubbles & Floating Popups** | CoC | **P0** | ✅ Completed |
| **UX-04** | **Precision 3D Building Placement Grid & Snap Audio** | CoC / AoE | **P0** | ✅ Completed |
| **UX-05** | **Cinematic RTS Camera Inertia & Edge Panning** | AoE | **P1** | ✅ Completed |

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

---

## 🌾 Phase 1: Economy, Villager Work & Base Management (CoC + AoE)

| Task ID | Feature Title | Inspiration | Priority | Status |
| :--- | :--- | :--- | :--- | :--- |
| **ECO-01** | **Resource Storage & Capacity Cap System** | CoC | **P0** | ⏳ Planned |
| **ECO-02** | **Villager Work Loop (Tree Chopping & Stone Mining)** | AoE | **P0** | ⏳ Planned |
| **ECO-03** | **Builder / Craftsman Management (Dakshya Karmi)** | CoC | **P1** | ⏳ Planned |
| **ECO-04** | **Building Relocation & Smooth Drag-to-Move** | CoC | **P1** | ⏳ Planned |
| **ECO-05** | **Mini-Map Navigation & Radar View** | AoE | **P1** | ⏳ Planned |

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
| **DEF-01** | **Placeable & Connectable Fortification Walls** | CoC / AoE | **P0** | ⏳ Planned |
| **DEF-02** | **Archer Watchtowers with Auto-Targeting Radius** | CoC / AoE | **P0** | ⏳ Planned |
| **DEF-03** | **Kaman (Mountain Cannon) Heavy Defense** | CoC | **P1** | ⏳ Planned |
| **DEF-04** | **Himalayan Concealed Traps (Pit Traps & Rolling Logs)** | CoC | **P1** | ⏳ Planned |
| **DEF-05** | **Settlement Shield & Peace Treaty Timer** | CoC | **P2** | ⏳ Planned |

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
| **AGE-01** | **Four Historical Ages of Gorkha** | AoE | **P0** | ⏳ Planned |
| **AGE-02** | **Town Center Upgrade Gating & Unlocks** | CoC / AoE | **P0** | ⏳ Planned |
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
| **CBT-01** | **Multi-Unit Drag-Box Selection & Group Hotkeys** | AoE | **P0** | ⏳ Planned |
| **CBT-02** | **Tactical Unit Formations (Line, Box, Wedge)** | AoE | **P1** | ⏳ Planned |
| **CBT-03** | **Rock-Paper-Scissors Combat Triangle** | AoE | **P0** | ⏳ Planned |
| **CBT-04** | **Attack-Move, Hold Position & Patrol Commands** | AoE | **P1** | ⏳ Planned |
| **CBT-05** | **CoC-Style Asynchronous Village Raid Mode** | CoC | **P1** | ⏳ Planned |
| **CBT-06** | **Hero Generals (Prithvi Narayan Shah, Kalu Pande)** | CoC / AoE | **P2** | ⏳ Planned |

### Detailed Specifications

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
- [ ] **Authentic Traditional Audio:**
  - Background music blending Bansuri (bamboo flute), Sarangi (strings), Madal (drums), and Dhimay percussion.
  - Voice barks in authentic Nepali for unit orders (*"Hajur!"*, *"Aadesh garnuhos!"*, *"Aakraman!"*).

---

## 🛠️ Sprint 1 Recommended Tasks (Starting Now)

To immediately make the game feel like Clash of Clans & Age of Empires, here are the **Top 4 Sprint Tasks**:

1. **`DEF-01` Placeable Walls & Gates:** Add a 1x1 wall blueprint to the bottom dock so players can wall in their Town Center and create fortresses.
2. **`ECO-01` Resource Storage Caps & Granary:** Add Granary and Treasury buildings so storage capacity matters and players upgrade them.
3. **`CBT-01` Multi-Unit Box Selection & March Command:** Implement click-and-drag box selection to command groups of warriors to march and guard together.
4. **`DEF-02` Archer Watchtower Auto-Attack:** Make the watchtower actively detect and fire arrows at wandering enemy patrols within radius.
