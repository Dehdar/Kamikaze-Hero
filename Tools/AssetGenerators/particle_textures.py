"""Generate the particle textures used by Kamikaze Hero's effects.

Every texture is drawn from code (noise, gradients and simple shapes), so
the output is original work covered by the project's own license. Re-run
the script after changing a parameter to rebuild the images:

    python Tools/AssetGenerators/particle_textures.py

Requires Python 3 with numpy and Pillow. Output is deterministic: the same
seed always gives the same image, so re-running does not create git noise.
"""

from pathlib import Path

import numpy as np
from PIL import Image

OUTPUT_DIR = Path(__file__).resolve().parents[2] / "Assets" / "Textures" / "Particles"


# --------------------------------------------------------------------------
# Building blocks
# --------------------------------------------------------------------------

def value_noise(size, cells, rng):
    """Smooth random noise in [0, 1]: random values on a grid, bicubic upscaled."""
    grid = rng.random((cells + 1, cells + 1))
    image = Image.fromarray((grid * 255).astype(np.uint8)).resize((size, size), Image.BICUBIC)
    return np.asarray(image, dtype=np.float32) / 255.0


def fbm(size, rng, octaves=5, base_cells=4):
    """Fractal noise: several octaves of value noise, each finer and fainter."""
    total = np.zeros((size, size), np.float32)
    amplitude, weight = 1.0, 0.0
    for octave in range(octaves):
        total += amplitude * value_noise(size, base_cells * 2 ** octave, rng)
        weight += amplitude
        amplitude *= 0.5
    return total / weight


def polar_grid(size):
    """Distance from the centre (0 at centre, 1 at the edge) and angle for every pixel."""
    axis = (np.arange(size, dtype=np.float32) + 0.5) / size * 2.0 - 1.0
    x, y = np.meshgrid(axis, axis)
    return np.sqrt(x * x + y * y), np.arctan2(y, x)


def billow(size, rng, puffs, spread, puff_radius, softness=0.35):
    """A cloud-like blob: several overlapping soft circles, roughened by noise.

    Returns the blob mask in [0, 1] and the noise field used to roughen it,
    so callers can reuse the noise for shading.
    """
    axis = (np.arange(size, dtype=np.float32) + 0.5) / size * 2.0 - 1.0
    x, y = np.meshgrid(axis, axis)
    density = np.zeros((size, size), np.float32)
    for _ in range(puffs):
        cx, cy = (rng.random(2) * 2.0 - 1.0) * spread
        radius = puff_radius * (0.6 + 0.4 * rng.random())
        density += np.exp(-((x - cx) ** 2 + (y - cy) ** 2) / (radius * radius))
    noise = fbm(size, rng)
    density *= 0.3 + 1.4 * noise
    # Fade to zero towards the border so nothing gets cut off by the quad edge.
    distance, _ = polar_grid(size)
    density *= np.clip((1.0 - distance) / 0.25, 0.0, 1.0)
    return np.clip((density - 0.25) / softness, 0.0, 1.0), noise


def fire_ramp(heat):
    """Map heat in [0, 1] to black -> deep red -> orange -> yellow -> white."""
    stops = np.array([0.0, 0.25, 0.5, 0.75, 1.0])
    colours = np.array([
        [0, 0, 0],
        [120, 20, 5],
        [235, 105, 20],
        [255, 200, 70],
        [255, 250, 225],
    ], np.float32)
    rgb = [np.interp(heat, stops, colours[:, channel]) for channel in range(3)]
    return np.stack(rgb, axis=-1)


def to_rgb_image(rgb):
    return Image.fromarray(np.clip(rgb, 0, 255).astype(np.uint8), "RGB")


def to_rgba_image(rgb, alpha):
    rgba = np.dstack([np.clip(rgb, 0, 255), np.clip(alpha * 255, 0, 255)])
    return Image.fromarray(rgba.astype(np.uint8), "RGBA")


# --------------------------------------------------------------------------
# Individual textures
# --------------------------------------------------------------------------

def fireball(size, rng):
    """One additive fireball: bright core, noisy orange body, dark red rim."""
    shape, noise = billow(size, rng, puffs=7, spread=0.3, puff_radius=0.4, softness=0.9)
    distance, _ = polar_grid(size)
    core = np.clip(1.0 - distance / 0.4, 0.0, 1.0) ** 2
    detail = fbm(size, rng, octaves=6, base_cells=8)
    heat = np.clip(shape * (0.25 + 0.75 * detail ** 1.5) + 0.5 * core * shape, 0.0, 1.0)
    return fire_ramp(heat)


def spark(size, rng, rays):
    """A small star burst: bright dot with a few thin rays."""
    distance, angle = polar_grid(size)
    dot = np.clip(1.0 - distance / (0.35 + 0.25 * rng.random()), 0.0, 1.0) ** 1.5
    ray_angles = rng.random(rays) * 2 * np.pi
    streaks = np.zeros_like(distance)
    for ray_angle in ray_angles:
        offset = np.abs(np.angle(np.exp(1j * (angle - ray_angle))))
        streaks = np.maximum(streaks, np.clip(1.0 - offset / 0.12, 0, 1) * np.clip(1.0 - distance, 0, 1) ** 2)
    return fire_ramp(np.clip(dot + 0.7 * streaks, 0.0, 1.0))


def kamikaze_fireball_sheet():
    """Four fireball frames stacked vertically (texture sheet 1 x 4)."""
    rng = np.random.default_rng(1001)
    frames = [fireball(256, rng) for _ in range(4)]
    return to_rgb_image(np.concatenate(frames, axis=0))


def kamikaze_spark_sheet():
    """Sixteen spark variations on a 4 x 4 grid of 64 px tiles."""
    rng = np.random.default_rng(1002)
    sheet = np.zeros((256, 256, 3), np.float32)
    for index in range(16):
        tile = np.zeros((64, 64, 3), np.float32)
        spark_size = int(rng.integers(28, 64)) // 2 * 2
        start = (64 - spark_size) // 2
        tile[start:start + spark_size, start:start + spark_size] = spark(spark_size, rng, int(rng.integers(3, 7)))
        row, column = divmod(index, 4)
        sheet[row * 64:(row + 1) * 64, column * 64:(column + 1) * 64] = tile
    return to_rgb_image(sheet)


def spark_dot():
    """Tiny round glow for spark particles."""
    distance, _ = polar_grid(32)
    glow = np.clip(1.0 - distance, 0.0, 1.0) ** 2
    return to_rgb_image(np.dstack([glow, glow * 0.95, glow * 0.85]) * 255)


def fire_cloud():
    """Big billowing additive explosion cloud."""
    rng = np.random.default_rng(1003)
    shape, noise = billow(256, rng, puffs=12, spread=0.4, puff_radius=0.35, softness=0.8)
    detail = fbm(256, rng, octaves=6, base_cells=6)
    heat = np.clip(shape * (0.2 + 0.3 * noise + 0.7 * detail ** 1.5), 0.0, 1.0)
    return to_rgb_image(fire_ramp(heat))


def smoke_puff(size, rng):
    """Alpha mask for one soft, lumpy smoke puff."""
    shape, noise = billow(size, rng, puffs=10, spread=0.4, puff_radius=0.3, softness=1.0)
    detail = fbm(size, rng, octaves=6, base_cells=8)
    return np.clip(shape * (0.2 + 1.0 * detail ** 2), 0.0, 1.0)


def white_cloud():
    """White smoke puff with soft alpha, tinted by the material."""
    rng = np.random.default_rng(1004)
    alpha = smoke_puff(256, rng)
    shade = 200 + 55 * fbm(256, rng)
    return to_rgba_image(np.dstack([shade] * 3), alpha)


def smoke_pair():
    """Two smoke puffs side by side, the layout the explosion smoke material expects."""
    rng = np.random.default_rng(1005)
    puffs = [smoke_puff(256, rng) for _ in range(2)]
    alpha = np.concatenate(puffs, axis=1)
    shade = 225 + 30 * np.concatenate([fbm(256, rng), fbm(256, rng)], axis=1)
    return to_rgba_image(np.dstack([shade] * 3), alpha)


def metal_shard_sheet():
    """Sixteen grey metal shards on a 4 x 4 grid, used for debris particles."""
    rng = np.random.default_rng(1006)
    tile_size = 128
    sheet = np.zeros((tile_size * 4, tile_size * 4, 4), np.float32)
    y, x = np.mgrid[0:tile_size, 0:tile_size].astype(np.float32) - tile_size / 2
    for index in range(16):
        # A random convex-ish polygon: points around a circle with random radii.
        corners = int(rng.integers(3, 6))
        angles = np.sort(rng.random(corners) * 2 * np.pi)
        radii = tile_size * (0.18 + 0.25 * rng.random(corners))
        px, py = radii * np.cos(angles), radii * np.sin(angles)
        inside = np.ones_like(x, bool)
        for i in range(corners):
            j = (i + 1) % corners
            inside &= (px[j] - px[i]) * (y - py[i]) - (py[j] - py[i]) * (x - px[i]) >= 0
        # Light from the top left gives each shard a little shine.
        light = 0.55 + 0.45 * np.clip(-(x + y) / tile_size + 0.5 * rng.random(), 0, 1)
        grey = (110 + 90 * light)[..., None] * np.array([1.0, 1.0, 1.05])
        row, column = divmod(index, 4)
        block = sheet[row * tile_size:(row + 1) * tile_size, column * tile_size:(column + 1) * tile_size]
        block[..., :3] = grey
        block[..., 3] = inside
    return to_rgba_image(sheet[..., :3], sheet[..., 3])


def rocket_flame():
    """Tall flame for the missile exhaust: hot at the bottom, flickering to black at the top."""
    rng = np.random.default_rng(1007)
    width, height = 400, 512
    y, x = np.mgrid[0:height, 0:width].astype(np.float32)
    u = (x / width - 0.5) * 2.0
    v = 1.0 - y / height  # 0 at the bottom, 1 at the top
    noise = np.asarray(Image.fromarray((fbm(512, rng) * 255).astype(np.uint8)).resize((width, height)), np.float32) / 255
    flame_width = 0.75 * (1.0 - v) ** 0.7 + 0.05
    body = np.clip(1.0 - np.abs(u + 0.25 * (noise - 0.5)) / flame_width, 0.0, 1.0)
    heat = np.clip(body * (1.15 - v) * (0.6 + 0.6 * noise), 0.0, 1.0)
    return to_rgb_image(fire_ramp(heat))


TEXTURES = {
    "KamikazeFireballSheet.png": kamikaze_fireball_sheet,
    "KamikazeSparkSheet.png": kamikaze_spark_sheet,
    "SparkDot.png": spark_dot,
    "FireCloud.png": fire_cloud,
    "WhiteCloud.png": white_cloud,
    "SmokePair.png": smoke_pair,
    "MetalShardSheet.png": metal_shard_sheet,
    "RocketFlame.png": rocket_flame,
}


def main():
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    for file_name, build in TEXTURES.items():
        build().save(OUTPUT_DIR / file_name, optimize=True)
        print(f"wrote {OUTPUT_DIR / file_name}")


if __name__ == "__main__":
    main()
