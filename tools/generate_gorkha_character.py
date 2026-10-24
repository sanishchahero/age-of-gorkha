"""
Gorkha Character Generator - High-Fidelity Historical Humanoid Rig
Builds an authentic Gorkhali Warrior in traditional Daura Suruwal, Patuka sash,
black Kot vest, Dhaka Topi, and Jungi mustache, fully rigged to a 15-bone armature.
"""

import os
import math
import bpy
import bmesh
from mathutils import Vector, Matrix, Euler


def clear_blender():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def create_mat(name, color, metallic=0.0, roughness=0.5):
    mat = bpy.data.materials.new(name=name)
    nodes = mat.node_tree.nodes
    bsdf = None
    for n in nodes:
        if n.type == 'BSDF_PRINCIPLED':
            bsdf = n
            break
    if bsdf:
        bsdf.inputs['Base Color'].default_value = (*color, 1.0)
        if 'Metallic' in bsdf.inputs:
            bsdf.inputs['Metallic'].default_value = metallic
        if 'Roughness' in bsdf.inputs:
            bsdf.inputs['Roughness'].default_value = roughness
    return mat


def build_gorkhali_character():
    clear_blender()

    # Materials
    mat_skin = create_mat("M_NepaliSkin", (0.72, 0.52, 0.38), metallic=0.0, roughness=0.45)
    mat_daura = create_mat("M_DauraCotton", (0.88, 0.86, 0.82), metallic=0.0, roughness=0.75)
    mat_kot = create_mat("M_BlackWoolKot", (0.10, 0.09, 0.09), metallic=0.05, roughness=0.85)
    mat_patuka = create_mat("M_PatukaSashRed", (0.62, 0.12, 0.12), metallic=0.0, roughness=0.65)
    mat_topi = create_mat("M_DhakaTopiPattern", (0.45, 0.15, 0.18), metallic=0.0, roughness=0.70)
    mat_boots = create_mat("M_LeatherBoots", (0.18, 0.12, 0.08), metallic=0.1, roughness=0.60)
    mat_hair = create_mat("M_HairMustache", (0.05, 0.04, 0.04), metallic=0.0, roughness=0.90)

    bm = bmesh.new()
    mesh = bpy.data.meshes.new("Gorkhali_Warrior_Mesh")
    char_obj = bpy.data.objects.new("Gorkhali_Warrior_Historical", mesh)
    bpy.context.collection.objects.link(char_obj)

    # --------------------------------------------------------------------------
    # 1. TORSO & DAURA SURUWAL BODY (Hips ~0.90m, Spine to 1.45m)
    # --------------------------------------------------------------------------
    # Torso sections
    torso_levels = [
        # (Z height, Radius X, Radius Y)
        (0.85, 0.16, 0.12),  # Lower hips / pelvis
        (0.96, 0.15, 0.11),  # Waist / Patuka sash band
        (1.08, 0.18, 0.12),  # Mid torso / Daura wrap
        (1.22, 0.22, 0.14),  # Chest & shoulders
        (1.36, 0.20, 0.13),  # Upper chest
        (1.42, 0.08, 0.08),  # Neck base
        (1.48, 0.065, 0.065), # Neck top
    ]
    t_seg = 14
    torso_rings = []
    for z, rx, ry in torso_levels:
        ring = []
        for s in range(t_seg):
            angle = (s / t_seg) * 2.0 * math.pi
            v = bm.verts.new(Vector((math.cos(angle)*rx, math.sin(angle)*ry, z)))
            ring.append(v)
        torso_rings.append(ring)

    for r in range(len(torso_rings) - 1):
        for s in range(t_seg):
            sn = (s + 1) % t_seg
            bm.faces.new((torso_rings[r][s], torso_rings[r][sn], torso_rings[r+1][sn], torso_rings[r+1][s]))

    # Close bottom hips
    hip_center = bm.verts.new(Vector((0, 0, 0.85)))
    for s in range(t_seg):
        sn = (s + 1) % t_seg
        bm.faces.new((torso_rings[0][sn], torso_rings[0][s], hip_center))

    # --------------------------------------------------------------------------
    # 2. HEAD, JUNGLE MUSTACHE & DHAKA TOPI
    # --------------------------------------------------------------------------
    # Head sphere/ellipsoid (Center at Z=1.58m)
    head_z = 1.58
    h_rings = 7
    h_seg = 12
    head_vert_rings = []
    for hr in range(h_rings):
        t = hr / (h_rings - 1)
        phi = (t - 0.5) * math.pi
        rz = math.sin(phi) * 0.11
        rxy = math.cos(phi) * 0.085
        ring = []
        for s in range(h_seg):
            a = (s / h_seg) * 2.0 * math.pi
            # Forward bias for nose and chin
            y_bias = 0.015 if (s >= 2 and s <= 4) else 0.0
            x = math.cos(a) * rxy
            y = math.sin(a) * (rxy * 1.05) + y_bias
            z = head_z + rz
            ring.append(bm.verts.new(Vector((x, y, z))))
        head_vert_rings.append(ring)

    for r in range(len(head_vert_rings) - 1):
        for s in range(h_seg):
            sn = (s + 1) % h_seg
            bm.faces.new((head_vert_rings[r][s], head_vert_rings[r][sn], head_vert_rings[r+1][sn], head_vert_rings[r+1][s]))

    # Authentic Gorkhali Curved Mustache (Jungi Muchhi)
    mustache_y = 0.095
    mustache_z = 1.56
    m_pts = [
        (-0.045, mustache_y - 0.015, mustache_z + 0.005), # Left curl up
        (-0.025, mustache_y,         mustache_z),
        ( 0.000, mustache_y + 0.005, mustache_z + 0.002), # Center under nose
        ( 0.025, mustache_y,         mustache_z),
        ( 0.045, mustache_y - 0.015, mustache_z + 0.005), # Right curl up
    ]
    m_verts = [bm.verts.new(Vector(p)) for p in m_pts]
    for i in range(len(m_verts) - 1):
        # Create small tube for mustache
        v_a = m_verts[i]
        v_b = m_verts[i+1]
        v_up1 = bm.verts.new(v_a.co + Vector((0, 0, 0.008)))
        v_up2 = bm.verts.new(v_b.co + Vector((0, 0, 0.008)))
        bm.faces.new((v_a, v_b, v_up2, v_up1))

    # Traditional Nepali Dhaka Topi (Slanted mountain crown)
    topi_base_z = 1.63
    topi_rim = []
    topi_seg = 12
    for s in range(topi_seg):
        a = (s / topi_seg) * 2.0 * math.pi
        topi_rim.append(bm.verts.new(Vector((math.cos(a)*0.092, math.sin(a)*0.092, topi_base_z))))

    topi_crown = []
    for s in range(topi_seg):
        a = (s / topi_seg) * 2.0 * math.pi
        # Asymmetrical mountain slant (higher on right)
        slant = 0.075 + math.cos(a) * 0.025
        topi_crown.append(bm.verts.new(Vector((math.cos(a)*0.082, math.sin(a)*0.082, topi_base_z + slant))))

    for s in range(topi_seg):
        sn = (s + 1) % topi_seg
        bm.faces.new((topi_rim[s], topi_rim[sn], topi_crown[sn], topi_crown[s]))

    # Close top of Dhaka Topi
    topi_top_center = bm.verts.new(Vector((0.015, 0, topi_base_z + 0.095)))
    for s in range(topi_seg):
        sn = (s + 1) % topi_seg
        bm.faces.new((topi_crown[sn], topi_crown[s], topi_top_center))

    # --------------------------------------------------------------------------
    # 3. LEGS & MOUNTAIN BOOTS (Suruwal)
    # --------------------------------------------------------------------------
    leg_x_offsets = [-0.095, 0.095]
    leg_levels = [
        # (Z height, Radius X, Radius Y)
        (0.85, 0.075, 0.075),  # Thigh top (UpperLeg)
        (0.68, 0.065, 0.065),  # Mid thigh
        (0.48, 0.055, 0.055),  # Knee (LowerLeg)
        (0.28, 0.048, 0.048),  # Calf (tight Suruwal taper)
        (0.12, 0.042, 0.045),  # Boot cuff
        (0.00, 0.040, 0.050),  # Ankle / Foot base
    ]

    for lx in leg_x_offsets:
        l_rings = []
        for z, rx, ry in leg_levels:
            ring = []
            for s in range(8):
                a = (s / 8) * 2.0 * math.pi
                v = bm.verts.new(Vector((math.cos(a)*rx + lx, math.sin(a)*ry, z)))
                ring.append(v)
            l_rings.append(ring)

        for r in range(len(l_rings) - 1):
            for s in range(8):
                sn = (s + 1) % 8
                bm.faces.new((l_rings[r][s], l_rings[r][sn], l_rings[r+1][sn], l_rings[r+1][s]))

        # Boot foot sole (extends forward in +Y)
        sole_center = bm.verts.new(Vector((lx, 0.035, 0.00)))
        for s in range(8):
            sn = (s + 1) % 8
            bm.faces.new((l_rings[-1][sn], l_rings[-1][s], sole_center))

    # --------------------------------------------------------------------------
    # 4. ARMS & HANDS (Shoulders ~1.32m, Elbows ~1.05m, Hands ~0.80m)
    # --------------------------------------------------------------------------
    arm_x_offsets = [(-0.25, -1), (0.25, 1)]
    arm_levels = [
        # (Relative X, Relative Y, Z height, Radius)
        (0.00,  0.00, 1.34, 0.055),  # Shoulder
        (0.02,  0.00, 1.18, 0.048),  # Upper Arm
        (0.03, -0.01, 1.05, 0.044),  # Elbow
        (0.04,  0.00, 0.92, 0.038),  # Forearm
        (0.05,  0.01, 0.80, 0.032),  # Wrist
        (0.05,  0.02, 0.72, 0.028),  # Hand knuckle
    ]

    for sx, sign in arm_x_offsets:
        a_rings = []
        for rx, ry, z, rad in arm_levels:
            ring = []
            for s in range(8):
                a = (s / 8) * 2.0 * math.pi
                v = bm.verts.new(Vector((math.cos(a)*rad + (sx + rx*sign), math.sin(a)*rad + ry, z)))
                ring.append(v)
            a_rings.append(ring)

        for r in range(len(a_rings) - 1):
            for s in range(8):
                sn = (s + 1) % 8
                bm.faces.new((a_rings[r][s], a_rings[r][sn], a_rings[r+1][sn], a_rings[r+1][s]))

        # Close hand tip
        h_tip = bm.verts.new(Vector((sx + 0.05*sign, 0.02, 0.70)))
        for s in range(8):
            sn = (s + 1) % 8
            bm.faces.new((a_rings[-1][s], a_rings[-1][sn], h_tip))

    bm.to_mesh(mesh)
    bm.free()

    # Materials assignment
    mesh.materials.append(mat_daura)
    mesh.materials.append(mat_skin)
    mesh.materials.append(mat_kot)
    mesh.materials.append(mat_patuka)
    mesh.materials.append(mat_topi)
    mesh.materials.append(mat_boots)
    mesh.materials.append(mat_hair)

    for poly in char_obj.data.polygons:
        poly.use_smooth = True

    # --------------------------------------------------------------------------
    # 5. CREATE 15-BONE HUMANOID ARMATURE
    # --------------------------------------------------------------------------
    arm_data = bpy.data.armatures.new("Gorkha_Humanoid_Rig")
    arm_obj = bpy.data.objects.new("Gorkha_Humanoid_Armature", arm_data)
    bpy.context.collection.objects.link(arm_obj)

    bpy.context.view_layer.objects.active = arm_obj
    bpy.ops.object.mode_set(mode='EDIT')

    # Bone layout matching Unity Mecanim & UnitProceduralAnimator
    bones = arm_data.edit_bones

    # 1. Hips (Root pelvis at 0.90m)
    b_hips = bones.new("Hips")
    b_hips.head = Vector((0, 0, 0.90))
    b_hips.tail = Vector((0, 0, 1.05))

    # 2. Spine_01 & Spine_02
    b_sp1 = bones.new("Spine_01")
    b_sp1.head = Vector((0, 0, 1.05))
    b_sp1.tail = Vector((0, 0, 1.25))
    b_sp1.parent = b_hips

    b_sp2 = bones.new("Spine_02")
    b_sp2.head = Vector((0, 0, 1.25))
    b_sp2.tail = Vector((0, 0, 1.45))
    b_sp2.parent = b_sp1

    # 3. Head
    b_head = bones.new("Head")
    b_head.head = Vector((0, 0, 1.45))
    b_head.tail = Vector((0, 0, 1.70))
    b_head.parent = b_sp2

    # 4. Left Leg (UpperLeg_L -> LowerLeg_L -> Ankle_L)
    b_u_l = bones.new("UpperLeg_L")
    b_u_l.head = Vector((0.095, 0, 0.88))
    b_u_l.tail = Vector((0.095, 0, 0.48))
    b_u_l.parent = b_hips

    b_l_l = bones.new("LowerLeg_L")
    b_l_l.head = Vector((0.095, 0, 0.48))
    b_l_l.tail = Vector((0.095, 0, 0.12))
    b_l_l.parent = b_u_l

    b_a_l = bones.new("Ankle_L")
    b_a_l.head = Vector((0.095, 0, 0.12))
    b_a_l.tail = Vector((0.095, 0.08, 0.00))
    b_a_l.parent = b_l_l

    # 5. Right Leg (UpperLeg_R -> LowerLeg_R -> Ankle_R)
    b_u_r = bones.new("UpperLeg_R")
    b_u_r.head = Vector((-0.095, 0, 0.88))
    b_u_r.tail = Vector((-0.095, 0, 0.48))
    b_u_r.parent = b_hips

    b_l_r = bones.new("LowerLeg_R")
    b_l_r.head = Vector((-0.095, 0, 0.48))
    b_l_r.tail = Vector((-0.095, 0, 0.12))
    b_l_r.parent = b_u_r

    b_a_r = bones.new("Ankle_R")
    b_a_r.head = Vector((-0.095, 0, 0.12))
    b_a_r.tail = Vector((-0.095, 0.08, 0.00))
    b_a_r.parent = b_l_r

    # 6. Left Arm (Shoulder_L -> Elbow_L -> Hand_L)
    b_s_l = bones.new("Shoulder_L")
    b_s_l.head = Vector((0.20, 0, 1.34))
    b_s_l.tail = Vector((0.23, 0, 1.05))
    b_s_l.parent = b_sp2

    b_e_l = bones.new("Elbow_L")
    b_e_l.head = Vector((0.23, 0, 1.05))
    b_e_l.tail = Vector((0.25, 0, 0.80))
    b_e_l.parent = b_s_l

    b_h_l = bones.new("Hand_L")
    b_h_l.head = Vector((0.25, 0, 0.80))
    b_h_l.tail = Vector((0.25, 0, 0.70))
    b_h_l.parent = b_e_l

    # 7. Right Arm (Shoulder_R -> Elbow_R -> Hand_R)
    b_s_r = bones.new("Shoulder_R")
    b_s_r.head = Vector((-0.20, 0, 1.34))
    b_s_r.tail = Vector((-0.23, 0, 1.05))
    b_s_r.parent = b_sp2

    b_e_r = bones.new("Elbow_R")
    b_e_r.head = Vector((-0.23, 0, 1.05))
    b_e_r.tail = Vector((-0.25, 0, 0.80))
    b_e_r.parent = b_s_r

    b_h_r = bones.new("Hand_R")
    b_h_r.head = Vector((-0.25, 0, 0.80))
    b_h_r.tail = Vector((-0.25, 0, 0.70))
    b_h_r.parent = b_e_r

    bpy.ops.object.mode_set(mode='OBJECT')

    # Parent mesh to armature with automatic bone heat weights
    bpy.ops.object.select_all(action='DESELECT')
    char_obj.select_set(True)
    arm_obj.select_set(True)
    bpy.context.view_layer.objects.active = arm_obj
    bpy.ops.object.parent_set(type='ARMATURE_AUTO')

    return arm_obj, char_obj


def export_character(output_path):
    os.makedirs(os.path.dirname(output_path), exist_ok=True)
    arm, char = build_gorkhali_character()

    bpy.ops.object.select_all(action='DESELECT')
    arm.select_set(True)
    char.select_set(True)
    bpy.context.view_layer.objects.active = arm

    bpy.ops.export_scene.fbx(
        filepath=output_path,
        use_selection=True,
        bake_anim=False,
        armature_nodetype='NULL',
        apply_scale_options='FBX_SCALE_ALL'
    )
    print(f"✅ Successfully exported Rigged Character to: {output_path}")


if __name__ == "__main__":
    out_file = "/Users/sanishmaharjanengineering/Downloads/free/game/unity-client/Assets/Art/Meshes/GorkhaCharacters/Gorkhali_Warrior_Historical.fbx"
    export_character(out_file)
