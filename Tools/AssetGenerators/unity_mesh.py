"""Tiny toolkit for building low-poly meshes and saving them as Unity mesh assets.

A mesh is built from simple solids (boxes, lofts, cylinders, cones). Each
solid is added to a named *part*; every part becomes one sub-mesh, so it can
get its own material in Unity. Faces are flat shaded, which gives the game
its toy-like low-poly look.

Meshes are written as Unity's own text mesh format (``.asset``), so Unity
loads them directly without a model importer and the asset GUIDs stay
stable between runs.

Coordinates use Unity's convention: X right, Y up, Z forward, left-handed.
"""

import hashlib
from pathlib import Path

import numpy as np


class MeshBuilder:
    """Collects flat-shaded triangles, grouped into named parts (sub-meshes)."""

    def __init__(self, part_names):
        self.part_names = list(part_names)
        self.triangles = {name: [] for name in self.part_names}

    # ---------------------------------------------------------------- core

    def triangle(self, part, a, b, c, outward_from, uv=((0, 0), (0, 0), (0, 0))):
        """Add one triangle, flipped if needed so its front faces away from `outward_from`."""
        a, b, c = (np.asarray(p, np.float64) for p in (a, b, c))
        normal = np.cross(b - a, c - a)
        if np.dot(normal, (a + b + c) / 3.0 - np.asarray(outward_from, np.float64)) < 0:
            b, c = c, b
            uv = (uv[0], uv[2], uv[1])
        self.triangles[part].append(((a, b, c), uv))

    def quad(self, part, a, b, c, d, outward_from, uv=((0, 0), (1, 0), (1, 1), (0, 1))):
        """Add a quad a-b-c-d (in order around its edge) as two triangles."""
        self.triangle(part, a, b, c, outward_from, (uv[0], uv[1], uv[2]))
        self.triangle(part, a, c, d, outward_from, (uv[0], uv[2], uv[3]))

    # ---------------------------------------------------------- primitives

    def loft(self, part, sections, sides=8, cap_start=True, cap_end=True, uv_mode=None):
        """Skin a tube through a list of cross-sections.

        Each section is (z, centre_y, half_width, half_height): an ellipse-like
        ring around the Z axis. Used for fuselages and missile bodies.
        `uv_mode="wrap"` maps U along the tube and V around it.
        """
        rings = []
        for z, centre_y, half_width, half_height in sections:
            ring = []
            for side in range(sides):
                angle = 2 * np.pi * (side + 0.5) / sides
                ring.append((half_width * np.cos(angle), centre_y + half_height * np.sin(angle), z))
            rings.append(ring)
        length = sections[-1][0] - sections[0][0]
        for index in range(len(rings) - 1):
            z0, y0 = sections[index][0], sections[index][1]
            z1, y1 = sections[index + 1][0], sections[index + 1][1]
            centre = (0.0, (y0 + y1) / 2, (z0 + z1) / 2)
            for side in range(sides):
                nxt = (side + 1) % sides
                if uv_mode == "wrap":
                    u0, u1 = (z0 - sections[0][0]) / length, (z1 - sections[0][0]) / length
                    v0, v1 = side / sides, (side + 1) / sides
                    uv = ((u0, v0), (u1, v0), (u1, v1), (u0, v1))
                else:
                    uv = ((0, 0), (0, 0), (0, 0), (0, 0))
                a, b = rings[index][side], rings[index + 1][side]
                c, d = rings[index + 1][nxt], rings[index][nxt]
                self.quad(part, a, b, c, d, centre, uv)
        if cap_start:
            self._cap(part, rings[0], sections[0], direction=-1)
        if cap_end:
            self._cap(part, rings[-1], sections[-1], direction=+1)

    def _cap(self, part, ring, section, direction):
        z, centre_y = section[0], section[1]
        middle = (0.0, centre_y, z)
        outside = (0.0, centre_y, z - direction)
        for side in range(len(ring)):
            self.triangle(part, middle, ring[side], ring[(side + 1) % len(ring)], outside)

    def box(self, part, centre, size):
        """Axis-aligned box."""
        cx, cy, cz = centre
        hx, hy, hz = (s / 2 for s in size)
        corners = [(cx + sx * hx, cy + sy * hy, cz + sz * hz)
                   for sx in (-1, 1) for sy in (-1, 1) for sz in (-1, 1)]
        faces = [(0, 1, 3, 2), (4, 6, 7, 5), (0, 4, 5, 1), (2, 3, 7, 6), (0, 2, 6, 4), (1, 5, 7, 3)]
        for face in faces:
            self.quad(part, *(corners[i] for i in face), centre)

    def slab(self, part, outline, y_bottom, y_top):
        """Extrude a convex outline in the XZ plane between two heights (wings, fins)."""
        self.prism(part, [(x, y_bottom, z) for x, z in outline], (0, y_top - y_bottom, 0))

    def prism(self, part, outline, extrusion):
        """Extrude a convex polygon (list of 3D points) along a vector."""
        bottom = [np.asarray(p, np.float64) for p in outline]
        top = [p + np.asarray(extrusion, np.float64) for p in bottom]
        centre = (sum(bottom) + sum(top)) / (2 * len(bottom))
        for index in range(len(bottom)):
            nxt = (index + 1) % len(bottom)
            self.quad(part, bottom[index], bottom[nxt], top[nxt], top[index], centre)
        for index in range(1, len(bottom) - 1):
            self.triangle(part, bottom[0], bottom[index], bottom[index + 1], centre)
            self.triangle(part, top[0], top[index], top[index + 1], centre)

    def beam(self, part, start, end, thickness):
        """Square rod from start to end, for struts and gun barrels."""
        start, end = np.asarray(start, np.float64), np.asarray(end, np.float64)
        axis = end - start
        axis /= np.linalg.norm(axis)
        helper = np.array([0, 1, 0]) if abs(axis[1]) < 0.9 else np.array([1, 0, 0])
        side_a = np.cross(axis, helper)
        side_a *= thickness / 2 / np.linalg.norm(side_a)
        side_b = np.cross(axis, side_a)
        side_b *= thickness / 2 / np.linalg.norm(side_b)
        outline = [start + side_a + side_b, start - side_a + side_b, start - side_a - side_b, start + side_a - side_b]
        self.prism(part, outline, end - start)

    def cylinder(self, part, centre, axis, radius, length, sides=10):
        """Cylinder around `axis` ("x", "y" or "z"), used for wheels and hubs."""
        self.cone(part, centre, axis, radius, radius, length, sides)

    def cone(self, part, centre, axis, radius_start, radius_end, length, sides=10):
        """Truncated cone along an axis; a radius of 0 makes a point."""
        axis_index = "xyz".index(axis)
        other = [i for i in range(3) if i != axis_index]
        centre = np.asarray(centre, np.float64)

        def ring(radius, offset):
            points = []
            for side in range(sides):
                angle = 2 * np.pi * (side + 0.5) / sides
                point = centre.copy()
                point[axis_index] += offset
                point[other[0]] += radius * np.cos(angle)
                point[other[1]] += radius * np.sin(angle)
                points.append(point)
            return points

        start, end = ring(radius_start, -length / 2), ring(radius_end, length / 2)
        cap_start, cap_end = centre.copy(), centre.copy()
        cap_start[axis_index] -= length / 2
        cap_end[axis_index] += length / 2
        for side in range(sides):
            nxt = (side + 1) % sides
            if radius_start == 0:
                self.triangle(part, cap_start, end[side], end[nxt], centre)
            elif radius_end == 0:
                self.triangle(part, start[side], cap_end, start[nxt], centre)
            else:
                self.quad(part, start[side], end[side], end[nxt], start[nxt], centre)
            if radius_start > 0:
                self.triangle(part, cap_start, start[side], start[nxt], centre)
            if radius_end > 0:
                self.triangle(part, cap_end, end[side], end[nxt], centre)

    # ------------------------------------------------------------ helpers

    def transformed(self, offset=(0, 0, 0), rotation=None):
        """Return a copy moved into another local space: v' = rotation^-1 * (v - offset).

        Use it to express geometry designed in a parent's space in the local
        space of the GameObject that will show it.
        """
        inverse = np.eye(3) if rotation is None else np.linalg.inv(rotation)
        offset = np.asarray(offset, np.float64)
        copy = MeshBuilder(self.part_names)
        for part, triangles in self.triangles.items():
            copy.triangles[part] = [
                (tuple(inverse @ (p - offset) for p in points), uv) for points, uv in triangles
            ]
        return copy


# ---------------------------------------------------------------- writing

def _aabb(points):
    low, high = points.min(axis=0), points.max(axis=0)
    return (low + high) / 2, (high - low) / 2


def _vec(v):
    return "{x: %s, y: %s, z: %s}" % tuple(_num(c) for c in v)


def _num(value):
    value = float(value)
    if abs(value) < 1e-6:
        return "0"
    text = "%.6g" % value
    return "0" if text in ("-0", "0") else text


def stable_guid(name):
    """Deterministic GUID from a name, so re-running the generator never changes .meta files."""
    return hashlib.md5(("KamikazeHero/" + name).encode()).hexdigest()


def write_mesh_asset(path, name, builder):
    """Save a MeshBuilder as a Unity text mesh asset plus its .meta file."""
    positions, normals, uvs, indices, submeshes = [], [], [], [], []
    for part in builder.part_names:
        first_index, first_vertex = len(indices), len(positions)
        for (a, b, c), uv in builder.triangles[part]:
            normal = np.cross(b - a, c - a)
            length = np.linalg.norm(normal)
            normal = normal / length if length > 0 else np.array([0.0, 1.0, 0.0])
            for point, coordinate in zip((a, b, c), uv):
                indices.append(len(positions))
                positions.append(point)
                normals.append(normal)
                uvs.append(coordinate)
        part_points = np.array(positions[first_vertex:]) if len(positions) > first_vertex else np.zeros((1, 3))
        submeshes.append((first_index, len(indices) - first_index, first_vertex,
                          len(positions) - first_vertex, _aabb(part_points)))

    positions, normals, uvs = (np.array(a, np.float32) for a in (positions, normals, uvs))
    vertex_data = np.hstack([positions, normals, uvs]).astype("<f4").tobytes()
    use_32bit = len(positions) > 65535
    index_data = np.array(indices, "<u4" if use_32bit else "<u2").tobytes()
    centre, extent = _aabb(positions)

    channels = [(0, 3), (12, 3), (0, 0), (0, 0), (24, 2)] + [(0, 0)] * 9
    lines = [
        "%YAML 1.1",
        "%TAG !u! tag:unity3d.com,2011:",
        "--- !u!43 &4300000",
        "Mesh:",
        "  m_ObjectHideFlags: 0",
        "  m_CorrespondingSourceObject: {fileID: 0}",
        "  m_PrefabInstance: {fileID: 0}",
        "  m_PrefabAsset: {fileID: 0}",
        f"  m_Name: {name}",
        "  serializedVersion: 11",
        "  m_SubMeshes:",
    ]
    for first_index, index_count, first_vertex, vertex_count, (sub_centre, sub_extent) in submeshes:
        lines += [
            "  - serializedVersion: 2",
            f"    firstByte: {first_index * (4 if use_32bit else 2)}",
            f"    indexCount: {index_count}",
            "    topology: 0",
            "    baseVertex: 0",
            f"    firstVertex: {first_vertex}",
            f"    vertexCount: {vertex_count}",
            "    localAABB:",
            f"      m_Center: {_vec(sub_centre)}",
            f"      m_Extent: {_vec(sub_extent)}",
        ]
    lines += [
        "  m_Shapes:",
        "    vertices: []",
        "    shapes: []",
        "    channels: []",
        "    fullWeights: []",
        "  m_BindPose: []",
        "  m_BoneNameHashes: ",
        "  m_RootBoneNameHash: 0",
        "  m_BonesAABB: []",
        "  m_VariableBoneCountWeights:",
        "    m_Data: ",
        "  m_MeshCompression: 0",
        "  m_IsReadable: 1",
        "  m_KeepVertices: 1",
        "  m_KeepIndices: 1",
        f"  m_IndexFormat: {1 if use_32bit else 0}",
        f"  m_IndexBuffer: {index_data.hex()}",
        "  m_VertexData:",
        "    serializedVersion: 3",
        f"    m_VertexCount: {len(positions)}",
        "    m_Channels:",
    ]
    for offset, dimension in channels:
        lines += [
            "    - stream: 0",
            f"      offset: {offset}",
            "      format: 0",
            f"      dimension: {dimension}",
        ]
    lines += [
        f"    m_DataSize: {len(vertex_data)}",
        f"    _typelessdata: {vertex_data.hex()}",
        "  m_CompressedMesh:",
        "    m_Vertices:",
        "      m_NumItems: 0",
        "      m_Range: 0",
        "      m_Start: 0",
        "      m_Data: ",
        "      m_BitSize: 0",
        "  m_LocalAABB:",
        f"    m_Center: {_vec(centre)}",
        f"    m_Extent: {_vec(extent)}",
        "  m_MeshUsageFlags: 0",
        "  m_CookingOptions: 30",
        "  m_BakedConvexCollisionMesh: ",
        "  m_BakedTriangleCollisionMesh: ",
        "  'm_MeshMetrics[0]': 1",
        "  'm_MeshMetrics[1]': 1",
        "  m_MeshOptimizationFlags: 1",
        "  m_StreamData:",
        "    serializedVersion: 2",
        "    offset: 0",
        "    size: 0",
        "    path: ",
    ]
    path = Path(path)
    path.write_text("\n".join(lines) + "\n", newline="\n")
    meta = Path(str(path) + ".meta")
    meta.write_text(
        "fileFormatVersion: 2\n"
        f"guid: {stable_guid(path.name)}\n"
        "NativeFormatImporter:\n"
        "  externalObjects: {}\n"
        "  mainObjectFileID: 4300000\n"
        "  userData: \n"
        "  assetBundleName: \n"
        "  assetBundleVariant: \n",
        newline="\n",
    )
    return stable_guid(path.name), [len(builder.triangles[p]) for p in builder.part_names]
