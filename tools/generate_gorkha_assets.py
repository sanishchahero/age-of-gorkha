"""
Gorkha 3D Asset Generator - High-Fidelity Historical Weapons & Equipment
Runs inside Blender headless (blender -b -P generate_gorkha_assets.py)
to generate production-quality FBX models directly for Unity.
"""

import sys
import os
import math

import bpy
import bmesh
from mathutils import Vector, Matrix, Euler


def clear_blender_scene():
    bpy.ops.wm.read_factory_settings(use_empty=True)


def create_pbr_material(name, base_color, metallic=0.0, roughness=0.5):
    mat = bpy.data.materials.new(name=name)
    nodes = mat.node_tree.nodes
    bsdf = None
    for n in nodes:
        if n.type == 'BSDF_PRINCIPLED':
            bsdf = n
            break
    if bsdf:
        bsdf.inputs['Base Color'].default_value = (*base_color, 1.0)
        if 'Metallic' in bsdf.inputs:
            bsdf.inputs['Metallic'].default_value = metallic
        if 'Roughness' in bsdf.inputs:
            bsdf.inputs['Roughness'].default_value = roughness
    return mat


# ==============================================================================
# 1. AUTHENTIC GORKHALI KHUKURI GENERATOR
# ==============================================================================
def build_khukuri():
    mat_steel = create_pbr_material("M_ForgedCarbonSteel", (0.28, 0.30, 0.32), metallic=0.92, roughness=0.22)
    mat_wood = create_pbr_material("M_CarvedRosewood", (0.16, 0.08, 0.04), metallic=0.0, roughness=0.55)
    mat_brass = create_pbr_material("M_PolishedBrass", (0.82, 0.65, 0.22), metallic=0.88, roughness=0.28)

    # 1. Handle (Bhid)
    bm_handle = bmesh.new()
    handle_mesh = bpy.data.meshes.new("Khukuri_Handle")
    handle_obj = bpy.data.objects.new("Khukuri_Handle", handle_mesh)
    bpy.context.collection.objects.link(handle_obj)

    segments = 14
    handle_points = [
        (0.000, 0.016, 0.013),
        (0.015, 0.017, 0.014),
        (0.035, 0.015, 0.012),
        (0.055, 0.018, 0.015),
        (0.060, 0.021, 0.017),  # Kharpa raised center ring
        (0.065, 0.018, 0.015),
        (0.085, 0.015, 0.012),
        (0.105, 0.017, 0.014),
        (0.120, 0.022, 0.016),  # Flared butt
    ]

    vert_rings = []
    for z, rx, ry in handle_points:
        ring_verts = []
        for s in range(segments):
            angle = (s / segments) * 2.0 * math.pi
            v = bm_handle.verts.new(Vector((math.cos(angle) * rx, math.sin(angle) * ry, z)))
            ring_verts.append(v)
        vert_rings.append(ring_verts)

    for r in range(len(vert_rings) - 1):
        for s in range(segments):
            s_next = (s + 1) % segments
            bm_handle.faces.new((vert_rings[r][s], vert_rings[r][s_next], vert_rings[r+1][s_next], vert_rings[r+1][s]))

    butt_center = bm_handle.verts.new(Vector((0, 0, 0)))
    for s in range(segments):
        s_next = (s + 1) % segments
        bm_handle.faces.new((vert_rings[0][s_next], vert_rings[0][s], butt_center))

    bm_handle.to_mesh(handle_mesh)
    bm_handle.free()
    handle_mesh.materials.append(mat_wood)

    # 2. Brass Bolster & Pommel Plate
    bm_brass = bmesh.new()
    brass_mesh = bpy.data.meshes.new("Khukuri_BrassFittings")
    brass_obj = bpy.data.objects.new("Khukuri_BrassFittings", brass_mesh)
    bpy.context.collection.objects.link(brass_obj)

    bolster_pts = [(0.120, 0.020, 0.016), (0.135, 0.018, 0.014)]
    b_rings = []
    for z, rx, ry in bolster_pts:
        ring = []
        for s in range(segments):
            a = (s / segments) * 2.0 * math.pi
            ring.append(bm_brass.verts.new(Vector((math.cos(a)*rx, math.sin(a)*ry, z))))
        b_rings.append(ring)
    for s in range(segments):
        s_next = (s + 1) % segments
        bm_brass.faces.new((b_rings[0][s], b_rings[0][s_next], b_rings[1][s_next], b_rings[1][s]))

    diamond_h = -0.005
    d1 = bm_brass.verts.new(Vector((0.022, 0, diamond_h)))
    d2 = bm_brass.verts.new(Vector((0, 0.018, diamond_h)))
    d3 = bm_brass.verts.new(Vector((-0.022, 0, diamond_h)))
    d4 = bm_brass.verts.new(Vector((0, -0.018, diamond_h)))
    bm_brass.faces.new((d1, d2, d3, d4))

    bm_brass.to_mesh(brass_mesh)
    bm_brass.free()
    brass_mesh.materials.append(mat_brass)

    # 3. Forged Curved Blade
    bm_blade = bmesh.new()
    blade_mesh = bpy.data.meshes.new("Khukuri_Blade")
    blade_obj = bpy.data.objects.new("Khukuri_Blade", blade_mesh)
    bpy.context.collection.objects.link(blade_obj)

    blade_profile = [
        (0.135, 0.000,  0.030,  0.007, 0.005, 0.001),
        (0.155, 0.002,  0.034,  0.007, 0.005, 0.001),  # Kauda notch
        (0.190, 0.008,  0.046,  0.006, 0.004, 0.0008),
        (0.240, 0.020,  0.068,  0.006, 0.004, 0.0007), # Mid belly
        (0.290, 0.028,  0.078,  0.005, 0.003, 0.0007), # Peak belly drop
        (0.340, 0.025,  0.068,  0.004, 0.002, 0.0006),
        (0.390, 0.014,  0.042,  0.003, 0.001, 0.0005),
        (0.435, 0.000,  0.000,  0.001, 0.000, 0.0003), # Tip (Aara)
    ]

    blade_sections = []
    for z, sx, ex, sthick, fthick, ethick in blade_profile:
        v_spine_l = bm_blade.verts.new(Vector((sx, -sthick*0.5, z)))
        v_spine_r = bm_blade.verts.new(Vector((sx,  sthick*0.5, z)))
        mid_x = sx + (ex - sx) * 0.45
        v_fuller_l = bm_blade.verts.new(Vector((mid_x, -fthick*0.5, z)))
        v_fuller_r = bm_blade.verts.new(Vector((mid_x,  fthick*0.5, z)))
        v_edge_l = bm_blade.verts.new(Vector((ex, -ethick*0.5, z)))
        v_edge_r = bm_blade.verts.new(Vector((ex,  ethick*0.5, z)))
        blade_sections.append((v_spine_l, v_spine_r, v_fuller_l, v_fuller_r, v_edge_l, v_edge_r))

    for i in range(len(blade_sections) - 1):
        s1 = blade_sections[i]
        s2 = blade_sections[i+1]
        bm_blade.faces.new((s1[0], s2[0], s2[1], s1[1]))
        bm_blade.faces.new((s1[0], s1[2], s2[2], s2[0]))
        bm_blade.faces.new((s1[1], s2[1], s2[3], s1[3]))
        bm_blade.faces.new((s1[2], s1[4], s2[4], s2[2]))
        bm_blade.faces.new((s1[3], s2[3], s2[5], s1[5]))
        bm_blade.faces.new((s1[4], s1[5], s2[5], s2[4]))

    b0 = blade_sections[0]
    bm_blade.faces.new((b0[0], b0[1], b0[3], b0[5], b0[4], b0[2]))

    bm_blade.to_mesh(blade_mesh)
    bm_blade.free()
    blade_mesh.materials.append(mat_steel)

    bpy.ops.object.select_all(action='DESELECT')
    handle_obj.select_set(True)
    brass_obj.select_set(True)
    blade_obj.select_set(True)
    bpy.context.view_layer.objects.active = blade_obj
    bpy.ops.object.join()
    blade_obj.name = "Gorkha_Khukuri_Historical"

    for poly in blade_obj.data.polygons:
        poly.use_smooth = True

    return blade_obj


# ==============================================================================
# 2. AUTHENTIC GORKHALI DHAL BUCKLER SHIELD
# ==============================================================================
def build_dhal_shield():
    mat_leather = create_pbr_material("M_HardenedBuffaloHide", (0.18, 0.14, 0.11), metallic=0.08, roughness=0.62)
    mat_brass = create_pbr_material("M_PolishedBrass", (0.82, 0.65, 0.22), metallic=0.88, roughness=0.28)

    bm = bmesh.new()
    mesh = bpy.data.meshes.new("Gorkha_Dhal_Shield")
    obj = bpy.data.objects.new("Gorkha_Dhal_Shield", mesh)
    bpy.context.collection.objects.link(obj)

    radius = 0.22
    rings = 8
    segments = 24
    depth = 0.055

    ring_verts = []
    for r in range(rings + 1):
        t = r / rings
        cur_r = radius * math.sin(t * math.pi * 0.5)
        cur_z = depth * math.cos(t * math.pi * 0.5) - depth
        rv = []
        for s in range(segments):
            angle = (s / segments) * 2.0 * math.pi
            v = bm.verts.new(Vector((math.cos(angle) * cur_r, math.sin(angle) * cur_r, cur_z)))
            rv.append(v)
        ring_verts.append(rv)

    for r in range(rings):
        for s in range(segments):
            s_next = (s + 1) % segments
            bm.faces.new((ring_verts[r][s], ring_verts[r][s_next], ring_verts[r+1][s_next], ring_verts[r+1][s]))

    rim_r = radius * 1.05
    rim_verts = [bm.verts.new(Vector((math.cos(s*2*math.pi/segments)*rim_r, math.sin(s*2*math.pi/segments)*rim_r, -depth - 0.008))) for s in range(segments)]
    for s in range(segments):
        s_next = (s + 1) % segments
        bm.faces.new((ring_verts[-1][s], ring_verts[-1][s_next], rim_verts[s_next], rim_verts[s]))

    bm.to_mesh(mesh)
    bm.free()
    mesh.materials.append(mat_leather)

    for dx, dy in [(0.075, 0.075), (-0.075, 0.075), (0.075, -0.075), (-0.075, -0.075)]:
        bm_boss = bmesh.new()
        b_mesh = bpy.data.meshes.new("Boss")
        b_obj = bpy.data.objects.new("Boss", b_mesh)
        bpy.context.collection.objects.link(b_obj)

        b_rad = 0.024
        b_seg = 12
        b_rings = []
        for b_r in range(4):
            frac = b_r / 3.0
            r_cur = b_rad * (1.0 - frac)
            z_cur = 0.016 * frac
            ring = [bm_boss.verts.new(Vector((math.cos(s*2*math.pi/b_seg)*r_cur + dx, math.sin(s*2*math.pi/b_seg)*r_cur + dy, z_cur - 0.015))) for s in range(b_seg)]
            b_rings.append(ring)
        for r in range(3):
            for s in range(b_seg):
                sn = (s + 1) % b_seg
                bm_boss.faces.new((b_rings[r][s], b_rings[r][sn], b_rings[r+1][sn], b_rings[r+1][s]))
        tip = bm_boss.verts.new(Vector((dx, dy, 0.016 - 0.015)))
        for s in range(b_seg):
            sn = (s + 1) % b_seg
            bm_boss.faces.new((b_rings[-1][s], b_rings[-1][sn], tip))

        bm_boss.to_mesh(b_mesh)
        bm_boss.free()
        b_mesh.materials.append(mat_brass)

        bpy.ops.object.select_all(action='DESELECT')
        b_obj.select_set(True)
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.join()

    for poly in obj.data.polygons:
        poly.use_smooth = True

    return obj


# ==============================================================================
# 3. AUTHENTIC HIMALAYAN SPEAR (BHALA)
# ==============================================================================
def build_spear():
    mat_steel = create_pbr_material("M_ForgedCarbonSteel", (0.28, 0.30, 0.32), metallic=0.92, roughness=0.22)
    mat_wood = create_pbr_material("M_SalWoodShaft", (0.22, 0.12, 0.06), metallic=0.0, roughness=0.60)
    mat_tassel = create_pbr_material("M_YakHairTassel", (0.65, 0.08, 0.08), metallic=0.0, roughness=0.90)

    bm_shaft = bmesh.new()
    shaft_mesh = bpy.data.meshes.new("Spear_Shaft")
    shaft_obj = bpy.data.objects.new("Spear_Shaft", shaft_mesh)
    bpy.context.collection.objects.link(shaft_obj)

    shaft_len = 1.90
    shaft_rad = 0.014
    shaft_seg = 10
    v_bottom = [bm_shaft.verts.new(Vector((math.cos(i*2*math.pi/shaft_seg)*shaft_rad, math.sin(i*2*math.pi/shaft_seg)*shaft_rad, -0.60))) for i in range(shaft_seg)]
    v_top    = [bm_shaft.verts.new(Vector((math.cos(i*2*math.pi/shaft_seg)*shaft_rad, math.sin(i*2*math.pi/shaft_seg)*shaft_rad, shaft_len - 0.60))) for i in range(shaft_seg)]
    for i in range(shaft_seg):
        inxt = (i + 1) % shaft_seg
        bm_shaft.faces.new((v_bottom[i], v_bottom[inxt], v_top[inxt], v_top[i]))

    bm_shaft.to_mesh(shaft_mesh)
    bm_shaft.free()
    shaft_mesh.materials.append(mat_wood)

    bm_head = bmesh.new()
    head_mesh = bpy.data.meshes.new("Spear_Head")
    head_obj = bpy.data.objects.new("Spear_Head", head_mesh)
    bpy.context.collection.objects.link(head_obj)

    base_z = shaft_len - 0.60
    head_profile = [
        (0.00, 0.016, 0.016, 0.016),
        (0.08, 0.018, 0.016, 0.016),
        (0.12, 0.028, 0.010, 0.002),
        (0.24, 0.048, 0.008, 0.001),
        (0.36, 0.038, 0.006, 0.001),
        (0.48, 0.018, 0.004, 0.001),
        (0.56, 0.000, 0.001, 0.000),
    ]

    h_sections = []
    for z_off, wx, ty, ey in head_profile:
        z = base_z + z_off
        v_sp_l = bm_head.verts.new(Vector((0, -ty*0.5, z)))
        v_sp_r = bm_head.verts.new(Vector((0,  ty*0.5, z)))
        v_ed_l = bm_head.verts.new(Vector((-wx*0.5, -ey*0.5, z)))
        v_ed_r = bm_head.verts.new(Vector(( wx*0.5, -ey*0.5, z)))
        h_sections.append((v_sp_l, v_sp_r, v_ed_l, v_ed_r))

    for i in range(len(h_sections) - 1):
        s1 = h_sections[i]
        s2 = h_sections[i+1]
        bm_head.faces.new((s1[2], s1[1], s2[1], s2[2]))
        bm_head.faces.new((s1[1], s1[3], s2[3], s2[1]))
        bm_head.faces.new((s1[2], s2[2], s2[0], s1[0]))
        bm_head.faces.new((s1[0], s2[0], s2[3], s1[3]))

    bm_head.to_mesh(head_mesh)
    bm_head.free()
    head_mesh.materials.append(mat_steel)

    bm_tassel = bmesh.new()
    tassel_mesh = bpy.data.meshes.new("Spear_Tassel")
    tassel_obj = bpy.data.objects.new("Spear_Tassel", tassel_mesh)
    bpy.context.collection.objects.link(tassel_obj)

    tassel_z = base_z + 0.04
    t_seg = 12
    v_t1 = [bm_tassel.verts.new(Vector((math.cos(i*2*math.pi/t_seg)*0.024, math.sin(i*2*math.pi/t_seg)*0.024, tassel_z))) for i in range(t_seg)]
    v_t2 = [bm_tassel.verts.new(Vector((math.cos(i*2*math.pi/t_seg)*0.038, math.sin(i*2*math.pi/t_seg)*0.038, tassel_z - 0.09))) for i in range(t_seg)]
    for i in range(t_seg):
        inxt = (i + 1) % t_seg
        bm_tassel.faces.new((v_t1[i], v_t1[inxt], v_t2[inxt], v_t2[i]))

    bm_tassel.to_mesh(tassel_mesh)
    bm_tassel.free()
    tassel_mesh.materials.append(mat_tassel)

    bpy.ops.object.select_all(action='DESELECT')
    shaft_obj.select_set(True)
    head_obj.select_set(True)
    tassel_obj.select_set(True)
    bpy.context.view_layer.objects.active = shaft_obj
    bpy.ops.object.join()
    shaft_obj.name = "Gorkha_Bhala_Spear"

    for poly in shaft_obj.data.polygons:
        poly.use_smooth = True

    return shaft_obj


# ==============================================================================
# 4. AUTHENTIC GORKHALI RECURVE COMPOSITE BOW (DHANU)
# ==============================================================================
def build_recurve_bow():
    mat_horn = create_pbr_material("M_HornSinewComposite", (0.18, 0.12, 0.08), metallic=0.1, roughness=0.45)
    mat_leather = create_pbr_material("M_GripLeatherWrap", (0.35, 0.18, 0.10), metallic=0.0, roughness=0.70)
    mat_string = create_pbr_material("M_TwistedSilkString", (0.85, 0.82, 0.75), metallic=0.0, roughness=0.80)

    bm = bmesh.new()
    mesh = bpy.data.meshes.new("Gorkha_Recurve_Bow")
    obj = bpy.data.objects.new("Gorkha_Recurve_Bow", mesh)
    bpy.context.collection.objects.link(obj)

    # 1. Recurve Limbs (Upper & Lower)
    bow_pts = [
        # (X offset, Z height, Limb width, Limb thickness)
        (0.000,  0.00, 0.024, 0.020),  # Handle center
        (0.015,  0.12, 0.022, 0.018),  # Main working limb starts
        (0.040,  0.26, 0.020, 0.016),  # Forward bow curve
        (0.065,  0.40, 0.018, 0.014),  # Peak forward belly
        (0.050,  0.54, 0.016, 0.012),  # Tapering back
        (0.015,  0.64, 0.014, 0.010),  # Reflex recurve zone
        (-0.025, 0.72, 0.012, 0.008),  # Recurved ear / tip
    ]

    all_rings = []
    # Lower limb (negative Z) to Upper limb (positive Z)
    full_profile = []
    for x, z, w, t in reversed(bow_pts[1:]):
        full_profile.append((x, -z, w, t))
    full_profile.extend(bow_pts)

    for x, z, w, t in full_profile:
        # Cross section box verts
        v1 = bm.verts.new(Vector((x, -t*0.5, z - w*0.5)))
        v2 = bm.verts.new(Vector((x,  t*0.5, z - w*0.5)))
        v3 = bm.verts.new(Vector((x,  t*0.5, z + w*0.5)))
        v4 = bm.verts.new(Vector((x, -t*0.5, z + w*0.5)))
        all_rings.append((v1, v2, v3, v4))

    for i in range(len(all_rings) - 1):
        r1 = all_rings[i]
        r2 = all_rings[i+1]
        bm.faces.new((r1[0], r2[0], r2[1], r1[1]))
        bm.faces.new((r1[1], r2[1], r2[2], r1[2]))
        bm.faces.new((r1[2], r2[2], r2[3], r1[3]))
        bm.faces.new((r1[3], r2[3], r2[0], r1[0]))

    # Close top and bottom tips
    bm.faces.new(all_rings[0])
    bm.faces.new(all_rings[-1])

    # 2. Taut Bowstring (String connects ear tips)
    tip_lower = Vector((-0.025, 0, -0.72))
    tip_upper = Vector((-0.025, 0,  0.72))
    s_r = 0.002
    v_s_bot = [bm.verts.new(Vector((-0.025 + math.cos(i*math.pi/2)*s_r, math.sin(i*math.pi/2)*s_r, -0.72))) for i in range(4)]
    v_s_top = [bm.verts.new(Vector((-0.025 + math.cos(i*math.pi/2)*s_r, math.sin(i*math.pi/2)*s_r,  0.72))) for i in range(4)]
    for i in range(4):
        inxt = (i + 1) % 4
        bm.faces.new((v_s_bot[i], v_s_bot[inxt], v_s_top[inxt], v_s_top[i]))

    bm.to_mesh(mesh)
    bm.free()
    mesh.materials.append(mat_horn)

    for poly in obj.data.polygons:
        poly.use_smooth = True

    return obj


# ==============================================================================
# 5. AUTHENTIC CURVED SICKLE (HASIYA / KHURPA)
# ==============================================================================
def build_sickle():
    mat_steel = create_pbr_material("M_ForgedCarbonSteel", (0.28, 0.30, 0.32), metallic=0.92, roughness=0.22)
    mat_wood = create_pbr_material("M_CarvedRosewood", (0.16, 0.08, 0.04), metallic=0.0, roughness=0.55)

    bm = bmesh.new()
    mesh = bpy.data.meshes.new("Gorkha_Hasiya_Sickle")
    obj = bpy.data.objects.new("Gorkha_Hasiya_Sickle", mesh)
    bpy.context.collection.objects.link(obj)

    # 1. Turned Wooden Handle
    h_seg = 10
    h_len = 0.16
    h_rad = 0.016
    v_hbot = [bm.verts.new(Vector((math.cos(i*2*math.pi/h_seg)*h_rad, math.sin(i*2*math.pi/h_seg)*h_rad, 0))) for i in range(h_seg)]
    v_htop = [bm.verts.new(Vector((math.cos(i*2*math.pi/h_seg)*h_rad, math.sin(i*2*math.pi/h_seg)*h_rad, h_len))) for i in range(h_seg)]
    for i in range(h_seg):
        inxt = (i + 1) % h_seg
        bm.faces.new((v_hbot[i], v_hbot[inxt], v_htop[inxt], v_htop[i]))
    c_bot = bm.verts.new(Vector((0, 0, 0)))
    for i in range(h_seg):
        inxt = (i + 1) % h_seg
        bm.faces.new((v_hbot[inxt], v_hbot[i], c_bot))

    # 2. Crescent Curved Blade (Inner cutting radius)
    blade_steps = 10
    arc_rad = 0.18
    blade_sections = []
    for step in range(blade_steps + 1):
        angle = (step / blade_steps) * (math.pi * 0.75)  # 135 deg arc
        cx = math.sin(angle) * arc_rad
        cz = h_len + (1.0 - math.cos(angle)) * arc_rad

        # Tang / width narrows toward tip
        frac = step / blade_steps
        width = 0.032 * (1.0 - frac * 0.70)
        thick = 0.004 * (1.0 - frac * 0.50)

        # Inner razor edge and outer reinforced spine
        v_inner_l = bm.verts.new(Vector((cx, -0.0005, cz)))
        v_inner_r = bm.verts.new(Vector((cx,  0.0005, cz)))
        v_outer_l = bm.verts.new(Vector((cx + width, -thick*0.5, cz)))
        v_outer_r = bm.verts.new(Vector((cx + width,  thick*0.5, cz)))
        blade_sections.append((v_inner_l, v_inner_r, v_outer_l, v_outer_r))

    for i in range(len(blade_sections) - 1):
        s1 = blade_sections[i]
        s2 = blade_sections[i+1]
        bm.faces.new((s1[0], s2[0], s2[1], s1[1]))
        bm.faces.new((s1[1], s2[1], s2[3], s1[3]))
        bm.faces.new((s1[3], s2[3], s2[2], s1[2]))
        bm.faces.new((s1[2], s2[2], s2[0], s1[0]))

    bm.to_mesh(mesh)
    bm.free()
    mesh.materials.append(mat_steel)

    for poly in obj.data.polygons:
        poly.use_smooth = True

    return obj


# ==============================================================================
# 6. EXPORT ALL WEAPONS & EQUIPMENT
# ==============================================================================
def export_all(output_dir):
    os.makedirs(output_dir, exist_ok=True)
    print(f"=== Starting Gorkha 3D Asset Pipeline to: {output_dir} ===")

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
            out_file = os.path.join(output_dir, f"{name}.fbx")
            bpy.ops.export_scene.fbx(filepath=out_file, use_selection=True, apply_scale_options='FBX_SCALE_ALL')
            print(f"✅ Exported: {out_file}")

    print("=== All High-Fidelity Gorkha Equipment Successfully Exported! ===")


if __name__ == "__main__":
    out_dir = "/Users/sanishmaharjanengineering/Downloads/free/game/unity-client/Assets/Art/Meshes/GorkhaWeapons"
    export_all(out_dir)
