"""
Save native Blender (.blend) project files for all Gorkha historical weapons and character models.
Saves to /Users/sanishmaharjanengineering/Downloads/free/game/blender_models/
and /Users/sanishmaharjanengineering/Downloads/free/game/unity-client/Assets/Art/Meshes/GorkhaWeapons/
"""

import sys
import os

# Add tools directory to path
sys.path.append("/Users/sanishmaharjanengineering/Downloads/free/game/unity-client/tools")

import bpy
from generate_gorkha_assets import clear_blender_scene, build_khukuri, build_dhal_shield, build_spear, build_recurve_bow, build_sickle
from generate_gorkha_character import build_gorkhali_character

out_dirs = [
    "/Users/sanishmaharjanengineering/Downloads/free/game/blender_models",
    "/Users/sanishmaharjanengineering/Downloads/free/game/unity-client/Assets/Art/Meshes/GorkhaWeapons"
]

for d in out_dirs:
    os.makedirs(d, exist_ok=True)

models = [
    ("Gorkha_Khukuri_Historical", build_khukuri),
    ("Gorkha_Dhal_Shield", build_dhal_shield),
    ("Gorkha_Bhala_Spear", build_spear),
    ("Gorkha_Recurve_Bow", build_recurve_bow),
    ("Gorkha_Hasiya_Sickle", build_sickle),
]

for name, builder in models:
    clear_blender_scene()
    obj = builder()
    if obj:
        for d in out_dirs:
            blend_path = os.path.join(d, f"{name}.blend")
            bpy.ops.wm.save_as_mainfile(filepath=blend_path)
            print(f"✅ Saved Blender Project: {blend_path}")

# Character
char_dirs = [
    "/Users/sanishmaharjanengineering/Downloads/free/game/blender_models",
    "/Users/sanishmaharjanengineering/Downloads/free/game/unity-client/Assets/Art/Meshes/GorkhaCharacters"
]
for d in char_dirs:
    os.makedirs(d, exist_ok=True)

clear_blender_scene()
arm, char = build_gorkhali_character()
for d in char_dirs:
    blend_path = os.path.join(d, "Gorkhali_Warrior_Historical.blend")
    bpy.ops.wm.save_as_mainfile(filepath=blend_path)
    print(f"✅ Saved Character Project: {blend_path}")

print("=== All .blend Project Files Successfully Saved! ===")
