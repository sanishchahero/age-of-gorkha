# Gorkha: Rise of the Kingdom (Unity 3D Client)

Historically grounded Nepali 3D grand-strategy settlement builder and tactical battle simulation.

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
   This automatically constructs all 3D building prefabs, terrain, lighting, materials, and saves `Assets/Scenes/MainSettlementScene.unity`.
8. Click **Play (▶️)** to experience the 3D Himalayan kingdom immediately!

---

## 🎨 3D Graphics Architecture

The Unity client includes an authentic Nepali 3D procedural graphics engine:

| Module | Description |
| :--- | :--- |
| **[`GorkhaMaterials.cs`](file:///Users/sanishmaharjanengineering/Downloads/free/game/unity-client/Assets/Scripts/Graphics/GorkhaMaterials.cs)** | Centralized PBR material registry (URP Lit & Standard shader compatible). Includes Terracotta Red Brick, Sal Wood, Metallic Gold (*Gajur*), Slate Stone, Alpine Grass, Wheat Fields, and glowing translucent placement grids. |
| **[`Nepali3DMeshBuilder.cs`](file:///Users/sanishmaharjanengineering/Downloads/free/game/unity-client/Assets/Scripts/Graphics/Nepali3DMeshBuilder.cs)** | Procedural 3D model generator for 3-tiered pagoda Durbar roofs, carved timber columns, stone plinths, golden spires, terraced farms, Kot fortress bastions, timber lodges, and stylized Gorkha soldiers. |
| **[`FlagClothAnimator.cs`](file:///Users/sanishmaharjanengineering/Downloads/free/game/unity-client/Assets/Scripts/Graphics/FlagClothAnimator.cs)** | Simulates real-time Himalayan mountain wind physics on the iconic double-triangle Nepali royal standard banner via vertex displacement. |
| **[`HimalayanEnvironment3D.cs`](file:///Users/sanishmaharjanengineering/Downloads/free/game/unity-client/Assets/Scripts/Graphics/HimalayanEnvironment3D.cs)** | Builds stepped terraced valley terrain, golden-hour sunlight with soft shadows, atmospheric mountain mist fog, alpine pine trees, and slate boulders. |
| **[`ProceduralUnitBob.cs`](file:///Users/sanishmaharjanengineering/Downloads/free/game/unity-client/Assets/Scripts/Graphics/ProceduralUnitBob.cs)** | Procedural walk-bobbing, tilt, and idle breathing animations for 3D Gorkha warriors and villagers. |
| **[`GorkhaGameInitializer.cs`](file:///Users/sanishmaharjanengineering/Downloads/free/game/unity-client/Assets/Scripts/Core/GorkhaGameInitializer.cs)** | Self-bootstrapping runtime manager that builds the 3D world, spawns the Town Center, Farm, Barracks, Wood Camp, and ambient units, with an interactive on-screen construction HUD. |
| **[`GorkhaVisualSceneBuilder.cs`](file:///Users/sanishmaharjanengineering/Downloads/free/game/unity-client/Assets/Scripts/Editor/GorkhaVisualSceneBuilder.cs)** | Editor automation tool that generates prefab assets and saves the complete 3D scene file with one click. |

---

## 🏗️ Architecture & Scripts

```text
Assets/
├── Scripts/
│   ├── Core/
│   │   ├── GorkhaGameInitializer.cs         # Master scene bootstrapper & runtime 3D HUD
│   │   └── IsometricGrid.cs                 # 3D Grid mapping, tile queries, and occupancy tracking
│   ├── Graphics/
│   │   ├── GorkhaMaterials.cs               # Centralized PBR materials (Brick, Wood, Gold, Slate, Grass)
│   │   ├── Nepali3DMeshBuilder.cs           # Procedural 3D model & mesh builder for Nepali architecture
│   │   ├── FlagClothAnimator.cs             # Real-time cloth wind simulation for Nepal flag
│   │   ├── HimalayanEnvironment3D.cs        # 3D valley terrain, golden hour sun, mist, and pine forests
│   │   └── ProceduralUnitBob.cs             # Procedural step-walking bobbing & idle breathing animations
│   ├── Economy/
│   │   ├── ResourceType.cs                  # Food, Wood, Stone, Iron, Gold, Gems
│   │   └── ResourceManager.cs               # Live ticking production, transactions, capacity
│   ├── Buildings/
│   │   ├── BuildingData.cs                  # ScriptableObject blueprints (costs, footprint, timers)
│   │   ├── BuildingInstance.cs              # 3D world building, levels, scaffolding, harvest bubble
│   │   └── BuildingPlacementController.cs   # Clash of Clans style 3D drag-to-place with green/red preview
│   ├── Camera/
│   │   └── IsometricCameraController.cs     # 45° smooth isometric pan, drag, and pinch-zoom
│   ├── Units/
│   │   └── UnitController.cs                # Ambient villagers & Gorkha military patrols with Dhaka topi & Khukuri
│   ├── UI/
│   │   └── HUDController.cs                 # Top resource bar, population cap, shop controls
│   └── Editor/
│       └── GorkhaVisualSceneBuilder.cs      # One-click 3D scene & prefab generator menu item
└── Textures/
    ├── buildings/                           # Reference building sprites & textures
    ├── terrain/                             # Mountain & valley terrain textures
    └── units/                               # Unit textures
```

---

## 🎮 Gameplay Features Ready in 3D
- **45° Isometric Camera:** Smooth panning with WASD / Mouse drag and mouse wheel zooming.
- **Clash of Clans Grid Placement:** Snaps to integer grid coordinates, shows green/red boundary preview, and verifies terrain collision.
- **Kingdom Economy Loop:** Continuous per-second resource accumulation (Food, Wood, Stone, Gold).
- **Construction & Upgrades:** Timed construction with 3D timber scaffolding state and progression.
- **Floating Harvest Bubbles:** Tap-to-collect 3D resource orbs directly above active farms and camps.
- **Ambient Life:** Roaming 3D Gorkha warriors and villagers with walking animations and pathing.
