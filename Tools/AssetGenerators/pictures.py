"""Generate the game's pictures: sky backgrounds, the app icon, the Kamikaze button and the flag.

Everything is drawn from code, so the output is original work covered by
the project's own license. The plane in the icon and the menu background
is rendered from the same generated model the game uses (see models.py),
so the pictures always match the plane on screen.

    python Tools/AssetGenerators/pictures.py

Requires Python 3 with numpy and Pillow. Output is deterministic.
"""

from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

import models

TEXTURES = Path(__file__).resolve().parents[2] / "Assets" / "Textures"


# --------------------------------------------------------------------------
# Sky and clouds
# --------------------------------------------------------------------------

def periodic_noise(width, height, rng, largest=1.5, smallest=20.0, roughness=2.0):
    """Cloud-like fractal noise that tiles seamlessly in both directions.

    Built in the frequency domain: random phases with an amplitude that
    falls off with frequency. Only features between `largest` and
    `smallest` cycles per image height are kept, which sets how big the
    clouds are. Values are scaled to [0, 1].
    """
    fx = np.fft.fftfreq(width)[None, :] * height
    fy = np.fft.fftfreq(height)[:, None] * height
    frequency = np.sqrt(fx ** 2 + fy ** 2)
    keep = (frequency >= largest) & (frequency <= smallest)
    amplitude = np.where(keep, 1.0 / np.maximum(frequency, 1e-6) ** roughness, 0.0)
    phases = np.exp(2j * np.pi * rng.random((height, width)))
    noise = np.real(np.fft.ifft2(amplitude * phases))
    return (noise - noise.min()) / (noise.max() - noise.min())


def sky(width, height, seed, cloud_cover=0.42, horizon=0.85):
    """Blue sky gradient with soft white cumulus clouds; tiles left to right."""
    rng = np.random.default_rng(seed)
    y = np.linspace(0.0, 1.0, height)[:, None]  # 0 at the top
    top, bottom = np.array([45, 110, 200]), np.array([170, 210, 245])
    blend = np.clip(y / horizon, 0, 1) ** 1.3
    image = (top * (1 - blend[..., None]) + bottom * blend[..., None]) * np.ones((1, width, 1))

    density = periodic_noise(width, height, rng)
    # More clouds in the middle band, fewer at the very top and bottom.
    band = np.exp(-((y - 0.55) / 0.35) ** 2)
    clouds = np.clip((density * (0.6 + 0.6 * band) - (1 - cloud_cover)) / 0.12, 0, 1)
    # Shade: clouds get greyer towards their underside, where less cloud lies below.
    soft = np.asarray(Image.fromarray((clouds * 255).astype(np.uint8)).filter(ImageFilter.GaussianBlur(height / 80)), np.float32) / 255
    underside = np.clip(soft - np.roll(soft, -height // 25, axis=0), 0.0, 1.0)
    shade = 1.0 - 0.3 * underside
    white = np.array([255, 255, 255]) * shade[..., None]
    image = image * (1 - clouds[..., None]) + white * clouds[..., None]
    return Image.fromarray(np.clip(image, 0, 255).astype(np.uint8), "RGB")


# --------------------------------------------------------------------------
# Rendering the plane model
# --------------------------------------------------------------------------

PLANE_COLOURS = {
    "Silver": (196, 200, 208),
    "Red": (200, 32, 26),
    "Black": (38, 38, 42),
    "Glass": (160, 205, 235),
}


def plane_triangles():
    """All triangles of the plane in Aircraft space, with their part colour."""
    parts = [
        (models.plane_body, models.BODY),
        (models.plane_propeller, models.PROPELLER),
        (models.plane_elevator, models.ELEVATOR),
        (models.plane_aileron, models.AILERON),
    ]
    triangles = []
    for build, transform in parts:
        rotation = models.quaternion_matrix(*transform.get("rotation", (0, 0, 0, 1)))
        position = np.asarray(transform["position"])
        for part, items in build().triangles.items():
            colour = PLANE_COLOURS.get(part, (255, 0, 255))
            for points, _ in items:
                triangles.append((np.array([rotation @ p + position for p in points]), colour))
    # The red roundel on the fuselage side is a Unity cylinder in the prefab, not part of the mesh.
    roundel = models.MeshBuilder(["Red"])
    roundel.cylinder("Red", (models.CENTRE_X + 3, 1258, 2454), "x", 250, 840, sides=20)
    for points, _ in roundel.triangles["Red"]:
        triangles.append((np.array(points), PLANE_COLOURS["Red"]))
    return triangles


def render_plane(size, yaw, pitch, roll, supersample=3):
    """Flat-shaded render of the plane on a transparent background."""
    width, height = size[0] * supersample, size[1] * supersample
    rotation = (_axis_rotation("z", roll) @ _axis_rotation("x", pitch) @ _axis_rotation("y", yaw))
    triangles = [(points @ rotation.T, colour) for points, colour in plane_triangles()]
    every_point = np.concatenate([points for points, _ in triangles])
    low, high = every_point.min(axis=0), every_point.max(axis=0)
    scale = 0.92 * min(width / (high[0] - low[0]), height / (high[1] - low[1]))
    centre = (low + high) / 2
    light = np.array([0.3, 0.8, -0.5])
    light /= np.linalg.norm(light)

    image = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    # Painter's algorithm: draw far triangles first (camera looks along +Z).
    for points, colour in sorted(triangles, key=lambda item: -item[0][:, 2].mean()):
        normal = np.cross(points[1] - points[0], points[2] - points[0])
        length = np.linalg.norm(normal)
        if length == 0:
            continue
        brightness = 0.45 + 0.55 * abs(np.dot(normal / length, light))
        screen = [(width / 2 + (p[0] - centre[0]) * scale, height / 2 - (p[1] - centre[1]) * scale) for p in points]
        draw.polygon(screen, fill=tuple(int(c * brightness) for c in colour) + (255,))
    return image.resize(size, Image.LANCZOS)


def _axis_rotation(axis, degrees):
    angle = np.radians(degrees)
    c, s = np.cos(angle), np.sin(angle)
    if axis == "x":
        return np.array([[1, 0, 0], [0, c, -s], [0, s, c]])
    if axis == "y":
        return np.array([[c, 0, s], [0, 1, 0], [-s, 0, c]])
    return np.array([[c, -s, 0], [s, c, 0], [0, 0, 1]])


# Nose to the right, seen slightly from the front and above, banking a little.
HERO_VIEW = dict(yaw=-115, pitch=12, roll=6)


# --------------------------------------------------------------------------
# Individual pictures
# --------------------------------------------------------------------------

def game_clouds():
    """Scrolling sky behind the game; its left and right edges join seamlessly."""
    return sky(4096, 1150, seed=3001)


def menu_background():
    """Main menu and About screen: sky with the plane flying across."""
    background = sky(1920, 1136, seed=3002).convert("RGBA")
    plane = render_plane((1100, 700), **HERO_VIEW)
    shadow = Image.new("RGBA", plane.size, (0, 0, 0, 0))
    shadow.putalpha(plane.getchannel("A").point(lambda a: a * 0.25))
    shadow = shadow.filter(ImageFilter.GaussianBlur(18))
    background.alpha_composite(shadow, (420, 300))
    background.alpha_composite(plane, (400, 260))
    return background.convert("RGB")


def app_icon():
    """The plane on a transparent background, used as the application icon."""
    return render_plane((1280, 806), **HERO_VIEW)


def kamikaze_button(state):
    """Round Kamikaze weapon button: a white burst on a red disc.

    state is "normal", "pressed" (darker and slightly smaller) or
    "disabled" (grey).
    """
    supersample = 2
    size = 1280 * supersample
    disc = {"normal": (205, 30, 30), "pressed": (150, 18, 18), "disabled": (120, 120, 120)}[state]
    burst = {"normal": (255, 255, 255), "pressed": (235, 220, 220), "disabled": (190, 190, 190)}[state]
    inset = (0.04 if state != "pressed" else 0.07) * size
    image = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(image)
    draw.ellipse([inset, inset, size - inset, size - inset], fill=(20, 20, 24, 255))
    ring = inset + 0.05 * size
    draw.ellipse([ring, ring, size - ring, size - ring], fill=burst + (255,))
    face = ring + 0.035 * size
    draw.ellipse([face, face, size - face, size - face], fill=disc + (255,))

    # Twelve-point burst star in the middle.
    centre = size / 2
    outer, inner = 0.30 * size, 0.14 * size
    points = []
    for index in range(24):
        radius = outer if index % 2 == 0 else inner
        angle = np.pi / 12 * index - np.pi / 2
        points.append((centre + radius * np.cos(angle), centre + radius * np.sin(angle)))
    draw.polygon(points, fill=burst + (255,))
    core = 0.08 * size
    draw.ellipse([centre - core, centre - core, centre + core, centre + core], fill=disc + (255,))
    return image.resize((1280, 1280), Image.LANCZOS)


def us_flag():
    """United States flag drawn from its official proportions (a public domain design).

    It is painted on the straight missiles. The texture is stretched to
    1280 x 836, the size the material was set up for.
    """
    width, height, supersample = 1280, 836, 2
    w, h = width * supersample, height * supersample
    red, white, blue = (178, 34, 52), (255, 255, 255), (60, 59, 110)
    image = Image.new("RGB", (w, h), white)
    draw = ImageDraw.Draw(image)
    stripe = h / 13
    for index in range(0, 13, 2):
        draw.rectangle([0, index * stripe, w, (index + 1) * stripe], fill=red)
    canton_w, canton_h = w * 0.76 / 1.9, stripe * 7
    draw.rectangle([0, 0, canton_w, canton_h], fill=blue)
    star_radius = canton_h * 0.0616 / 0.5385 / 2 * 1.0
    for row in range(9):
        columns = 6 if row % 2 == 0 else 5
        for column in range(columns):
            x = canton_w / 12 * (2 * column + 1 + (row % 2))
            y = canton_h / 10 * (row + 1)
            _star(draw, x, y, star_radius, white)
    return image.resize((width, height), Image.LANCZOS)


def _star(draw, x, y, radius, colour):
    points = []
    for index in range(10):
        r = radius if index % 2 == 0 else radius * 0.382
        angle = np.pi / 5 * index - np.pi / 2
        points.append((x + r * np.cos(angle), y + r * np.sin(angle)))
    draw.polygon(points, fill=colour)


PICTURES = {
    "Clouds.png": game_clouds,
    "MenuBackground.png": menu_background,
    "AppIcon.png": app_icon,
    "KamikazeWeaponNormal.png": lambda: kamikaze_button("normal"),
    "KamikazeWeaponPressed.png": lambda: kamikaze_button("pressed"),
    "KamikazeWeaponDisabled.png": lambda: kamikaze_button("disabled"),
    "USFlag.png": us_flag,
}


def main():
    for file_name, build in PICTURES.items():
        build().save(TEXTURES / file_name, optimize=True)
        print(f"wrote {TEXTURES / file_name}")


if __name__ == "__main__":
    main()
