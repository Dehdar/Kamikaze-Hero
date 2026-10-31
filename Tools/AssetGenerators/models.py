"""Generate the low-poly 3D models for the player's plane, the missiles and the bullet.

Each model is designed in the coordinate space of its prefab (see the
`*_SPACE` notes below) and then moved into the local space of the
GameObject that shows it, so the prefabs keep their existing transforms,
colliders and scripts. Every mesh is split into parts, one per material
slot, in the order listed in the `parts` argument.

    python Tools/AssetGenerators/models.py

Requires Python 3 with numpy. Writes Unity mesh assets to Assets/Models.
"""

from pathlib import Path

import numpy as np

from unity_mesh import MeshBuilder, write_mesh_asset

OUTPUT_DIR = Path(__file__).resolve().parents[2] / "Assets" / "Models"


def quaternion_matrix(x, y, z, w):
    """Rotation matrix for a Unity quaternion (as stored in prefab transforms)."""
    return np.array([
        [1 - 2 * (y * y + z * z), 2 * (x * y - z * w), 2 * (x * z + y * w)],
        [2 * (x * y + z * w), 1 - 2 * (x * x + z * z), 2 * (y * z - x * w)],
        [2 * (x * z - y * w), 2 * (y * z + x * w), 1 - 2 * (x * x + y * y)],
    ])


def into_local(builder, position=(0, 0, 0), rotation=(0, 0, 0, 1), scale=(1, 1, 1)):
    """Move geometry from a parent's space into a child's local space.

    position, rotation and scale are the child's Transform values as they
    appear in the prefab (m_LocalPosition, m_LocalRotation, m_LocalScale).
    """
    matrix = quaternion_matrix(*rotation) @ np.diag(scale)
    return builder.transformed(offset=position, rotation=matrix)


def design_to_parent(builder, axes, origin):
    """Place a model designed along +Z into its parent's space.

    `axes` is the 3x3 matrix whose columns say where the design X, Y and Z
    axes point in the parent; `origin` is where the design origin lands.
    """
    axes = np.asarray(axes, np.float64)
    return builder.transformed(
        offset=-np.linalg.inv(axes) @ np.asarray(origin, np.float64), rotation=np.linalg.inv(axes)
    )


# --------------------------------------------------------------------------
# Player plane
#
# AIRCRAFT_SPACE: local space of Player/Aircraft, roughly millimetres.
# Forward (the nose) is -Z, up is +Y, the wings span X. The plane's centre
# line sits at X = 45.7, a leftover from the original model that the child
# transforms and colliders still expect.
# --------------------------------------------------------------------------

CENTRE_X = 45.703217

# Transforms of the Player.prefab objects that show the plane.
BODY = dict(position=(45.703217, 1175.0973, 1461.6455))
PROPELLER = dict(position=(45.703217, 1324.0973, -1280.3542), rotation=(0, 0, 0.7069834, 0.70723015))
ELEVATOR = dict(position=(-598.2967, 1560.0973, 5152.645))
AILERON = dict(position=(-3228.2966, 2282.0972, 1209.6455))

PLANE_PARTS = ["Silver", "Red", "Black", "Glass"]


def plane_body():
    """Everything on the plane that does not move: fuselage, wings, tail, struts and wheels."""
    plane = MeshBuilder(PLANE_PARTS)

    # Fuselage: an eight-sided tube from the engine back to the tail.
    plane.loft("Red", [(-1150, 1300, 540, 580), (-700, 1300, 570, 610)], cap_end=False)
    plane.loft("Silver", [
        (-700, 1300, 560, 600),
        (0, 1320, 520, 630),
        (1800, 1400, 430, 560),
        (3500, 1500, 250, 330),
        (5650, 1560, 60, 110),
    ], cap_start=False)

    # Cockpit: dark opening with a windscreen in front of it.
    plane.box("Black", (0, 1975, 2050), (420, 50, 560))
    plane.prism("Glass", [(-230, 1940, 1700), (230, 1940, 1700), (200, 2170, 1780), (-200, 2170, 1780)], (0, 0, 60))

    # Engine details: guns on top of the cowling and exhaust stubs on the sides.
    for x in (-300, 300):
        plane.beam("Black", (x, 1860, -1250), (x, 1860, 200), 70)
    for x in (-575, 575):
        plane.box("Black", (x, 1120, -350), (60, 70, 420))

    # Upper wing with a fixed left aileron; the right aileron moves (see plane_aileron).
    plane.slab("Silver", [(-3900, 380), (-3780, 300), (3780, 300), (3900, 380), (3900, 1210), (-3900, 1210)], 2200, 2300)
    plane.slab("Silver", [(2500, 1210), (3800, 1210), (3800, 1460), (2500, 1460)], 2210, 2290)
    plane.slab("Red", [(-3990, 380), (-3900, 380), (-3900, 1210), (-3990, 1150)], 2200, 2300)
    plane.slab("Red", [(3900, 380), (3990, 380), (3990, 1150), (3900, 1210)], 2200, 2300)

    # Lower wing.
    plane.slab("Silver", [(-3500, 480), (-3400, 400), (3400, 400), (3500, 480), (3500, 1250), (-3500, 1250)], 820, 900)

    # Struts between the wings and up from the fuselage.
    for side in (-1, 1):
        plane.beam("Black", (side * 2600, 900, 600), (side * 2700, 2200, 500), 60)
        plane.beam("Black", (side * 2600, 900, 1100), (side * 2700, 2200, 1000), 60)
        plane.beam("Black", (side * 300, 1850, 400), (side * 500, 2200, 450), 50)
        plane.beam("Black", (side * 300, 1850, 1000), (side * 500, 2200, 1050), 50)

    # Landing gear: two legs per wheel plus a tail skid.
    for side in (-1, 1):
        hub = (side * 1030, 380, -60)
        plane.beam("Black", (side * 400, 820, -100), hub, 70)
        plane.beam("Black", (side * 400, 820, 500), hub, 70)
        plane.cylinder("Black", hub, "x", 340, 160, sides=12)
        plane.cylinder("Silver", (side * 1030 + side * 85, 380, -60), "x", 120, 20, sides=12)
    plane.beam("Black", (0, 1350, 5300), (0, 1050, 5500), 60)

    # Tail: red fin and a fixed horizontal stabiliser; the elevator moves (see plane_elevator).
    plane.prism("Red", [(-30, 1600, 4650), (-30, 2450, 5250), (-30, 2550, 5550), (-30, 1600, 5700)], (60, 0, 0))
    plane.slab("Silver", [(-1300, 4900), (-900, 4650), (900, 4650), (1300, 4900), (1300, 5150), (-1300, 5150)], 1530, 1590)

    shifted = plane.transformed(offset=(-CENTRE_X, 0, 0))
    return into_local(shifted, **BODY)


def plane_propeller():
    """Two-bladed propeller with a red spinner, spun by PropelSpinner around its local Z axis."""
    prop = MeshBuilder(["Black", "Red"])
    hub_x, hub_y, hub_z = CENTRE_X, 1324.0973, -1280.3542
    for direction in (1, -1):
        blade = [
            (hub_x - 90, hub_y, hub_z - 20),
            (hub_x + 90, hub_y, hub_z - 20),
            (hub_x + 60, hub_y + direction * 1250, hub_z - 20),
            (hub_x - 60, hub_y + direction * 1250, hub_z - 20),
        ]
        prop.prism("Black", blade, (0, 0, 40))
    prop.cone("Red", (hub_x, hub_y, hub_z - 150), "z", 0, 260, 320, sides=12)
    return into_local(prop, **PROPELLER)


def plane_elevator():
    """Elevator on the tail, tilted by WingMovement when the player climbs or dives."""
    elevator = MeshBuilder(["Silver"])
    elevator.slab("Silver", [(-1250, 5152), (1250, 5152), (1200, 5450), (-1200, 5450)], 1535, 1585)
    return into_local(elevator.transformed(offset=(-CENTRE_X, 0, 0)), **ELEVATOR)


def plane_aileron():
    """Right aileron on the upper wing, tilted by WingMovement."""
    aileron = MeshBuilder(["Silver"])
    aileron.slab("Silver", [(-3800, 1210), (-2500, 1210), (-2500, 1460), (-3800, 1460)], 2210, 2290)
    return into_local(aileron.transformed(offset=(-CENTRE_X, 0, 0)), **AILERON)


# --------------------------------------------------------------------------
# Missiles and bullet
#
# Each is designed along +Z, from the tail (z = 0) to the nose (z = length),
# then placed in the prefab root's space to fit its colliders, with the
# nose pointing the way the missile flies.
# --------------------------------------------------------------------------

def missile(length, radius, nose_length, fin_span, fin_start, fin_end, parts, wrap_body_uv=False):
    """Generic missile: body tube, pointed nose cone and four tail fins."""
    body_part, nose_part, fin_part = parts
    shape = MeshBuilder(parts)
    body_end = length - nose_length
    shape.loft(body_part, [
        (0, 0, radius * 0.85, radius * 0.85),
        (length * 0.05, 0, radius, radius),
        (body_end, 0, radius, radius),
    ], sides=12, uv_mode="wrap" if wrap_body_uv else None)
    shape.cone(nose_part, (0, 0, body_end + nose_length / 2), "z", radius, 0, nose_length, sides=12)
    thickness = radius * 0.15
    fin = [(0, fin_start), (fin_span, fin_start + (fin_end - fin_start) * 0.35), (fin_span, fin_end - (fin_end - fin_start) * 0.25), (0, fin_end)]
    for angle in (0, 90, 180, 270):
        rotation = np.radians(angle)
        cos, sin = np.cos(rotation), np.sin(rotation)
        outline = [(r * cos - thickness / 2 * sin, r * sin + thickness / 2 * cos, z) for r, z in fin]
        offset = (thickness * sin, -thickness * cos, 0)
        shape.prism(fin_part, outline, offset)
    return shape


# Design Z axis pointing along -X of the parent: for missiles that fly left.
POINT_LEFT = [[0, 0, -1], [0, 1, 0], [1, 0, 0]]
# Design axes equal the parent's axes.
SAME_AXES = np.eye(3)


def plain_missile():
    """Missile.prefab: long missile wrapped in the flag texture. Shown by Missile/Content/Body."""
    shape = missile(605, 32, 130, 95, 15, 160, ["Flag", "Red", "Fins"], wrap_body_uv=True)
    in_root = design_to_parent(shape, POINT_LEFT, origin=(313, 9, -38))
    in_content = into_local(in_root, rotation=(0, -0.7071068, 0, 0.7071068))
    return into_local(in_content, position=(299.0001, -185.00006, 0))


def guided_missile():
    """Guided Missile.prefab: short, fat missile with long side fins. Shown by Body/Part1."""
    shape = missile(1.8, 0.12, 0.35, 0.36, 0.02, 0.45, ["Body", "Red", "Black"])
    in_root = design_to_parent(shape, POINT_LEFT, origin=(10.17, 0.8, 0))
    return into_local(in_root, position=(9.24, 0.8, 0), rotation=(0, -0.7071068, 0, 0.7071068), scale=(0.3, 0.3, 0.2))


def multi_missile():
    """One rocket of MultiMissiles.prefab, shared by MultiMissileA, B and C (each one's Body)."""
    shape = missile(436, 8, 60, 25, 10, 95, ["Body", "Nose", "Fins"])
    in_rocket = design_to_parent(shape, SAME_AXES, origin=(2.1, -0.08, 0))
    return into_local(
        in_rocket,
        rotation=(-0.000000053385083, 0.7071068, -0.7071068, -0.00000004371139),
        scale=(0.78684396, 16.803078, 0.78684396),
    )


def bullet():
    """Bullet.prefab: slim tracer round. The prefab's root shows it directly.

    The bullet spawns rotated 180 degrees around Y, so its nose points along local -X.
    """
    shape = MeshBuilder(["Casing", "Tip"])
    shape.loft("Casing", [(0, 0, 0.5, 0.5), (3.6, 0, 0.55, 0.55)], sides=8)
    shape.cone("Tip", (0, 0, 4.5), "z", 0.55, 0, 1.8, sides=8)
    return design_to_parent(shape, POINT_LEFT, origin=(1.4, 0, 0))


MODELS = {
    "PlaneBody": plane_body,
    "PlanePropeller": plane_propeller,
    "PlaneElevator": plane_elevator,
    "PlaneAileron": plane_aileron,
    "Missile": plain_missile,
    "GuidedMissile": guided_missile,
    "MultiMissile": multi_missile,
    "Bullet": bullet,
}


def main():
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    for name, build in MODELS.items():
        guid, triangles = write_mesh_asset(OUTPUT_DIR / f"{name}.asset", name, build())
        print(f"wrote {name}.asset  guid {guid}  triangles per part {triangles}")


if __name__ == "__main__":
    main()
