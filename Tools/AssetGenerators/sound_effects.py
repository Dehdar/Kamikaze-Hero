"""Generate Kamikaze Hero's sound effects.

Every sound is synthesised from noise and simple waveforms, so the output
is original work covered by the project's own license. Re-run the script
after changing a parameter to rebuild the files:

    python Tools/AssetGenerators/sound_effects.py

Requires Python 3 with numpy. Output is deterministic: the same seed always
gives the same file. Looping sounds are built from periodic noise, so they
loop without a click.
"""

import wave
from pathlib import Path

import numpy as np

OUTPUT_DIR = Path(__file__).resolve().parents[2] / "Assets" / "Sounds"
SAMPLE_RATE = 44100


# --------------------------------------------------------------------------
# Building blocks
# --------------------------------------------------------------------------

def timeline(seconds):
    """Sample times in seconds for a clip of the given length."""
    return np.arange(int(seconds * SAMPLE_RATE)) / SAMPLE_RATE


def shaped_noise(seconds, rng, low_hz, high_hz, tilt=1.0):
    """Noise limited to a frequency band, made in the frequency domain.

    Because it is built from whole frequency bins it repeats exactly after
    `seconds`, which makes it safe to loop. `tilt` > 0 makes the noise
    darker (more bass), like brown noise.
    """
    samples = int(seconds * SAMPLE_RATE)
    frequencies = np.fft.rfftfreq(samples, 1.0 / SAMPLE_RATE)
    magnitude = np.zeros_like(frequencies)
    band = (frequencies >= low_hz) & (frequencies <= high_hz)
    magnitude[band] = 1.0 / np.maximum(frequencies[band], 1.0) ** (tilt * 0.5)
    phases = np.exp(2j * np.pi * rng.random(len(frequencies)))
    noise = np.fft.irfft(magnitude * phases, n=samples)
    return noise / np.max(np.abs(noise))


def decay(t, seconds):
    """Exponential fade that has dropped to about 1 % after `seconds`."""
    return np.exp(-t * 4.6 / seconds)


def attack(t, seconds):
    """Short linear fade-in that avoids a click at the very start."""
    return np.clip(t / seconds, 0.0, 1.0)


def sweep(t, start_hz, end_hz, seconds):
    """Sine wave whose pitch glides exponentially from start_hz to end_hz."""
    ratio = end_hz / start_hz
    frequency = start_hz * ratio ** np.clip(t / seconds, 0.0, 1.0)
    return np.sin(2 * np.pi * np.cumsum(frequency) / SAMPLE_RATE)


def one_pole_lowpass(signal, cutoff_hz):
    """Simple low-pass filter that softens harsh highs."""
    alpha = 1.0 - np.exp(-2 * np.pi * cutoff_hz / SAMPLE_RATE)
    output = np.empty_like(signal)
    state = 0.0
    for index, sample in enumerate(signal):
        state += alpha * (sample - state)
        output[index] = state
    return output


def normalise(signal, peak=0.9):
    return signal * (peak / np.max(np.abs(signal)))


def write_wav(path, signal):
    pcm = (np.clip(signal, -1.0, 1.0) * 32767).astype("<i2")
    with wave.open(str(path), "wb") as file:
        file.setnchannels(1)
        file.setsampwidth(2)
        file.setframerate(SAMPLE_RATE)
        file.writeframes(pcm.tobytes())


# --------------------------------------------------------------------------
# Individual sounds
# --------------------------------------------------------------------------

def explosion(seconds, rng, boom_hz=70.0, brightness=3000.0):
    """Generic explosion: a noisy blast, a falling low boom and a rumbling tail."""
    t = timeline(seconds)
    blast = shaped_noise(seconds, rng, 20, brightness, tilt=1.2) * decay(t, seconds * 0.6)
    crack = shaped_noise(seconds, rng, 1500, 9000) * decay(t, 0.12)
    boom = sweep(t, boom_hz * 1.8, boom_hz * 0.45, seconds * 0.5) * decay(t, seconds * 0.45)
    return (0.9 * blast + 0.35 * crack + 0.8 * boom) * attack(t, 0.004)


def gunshot():
    """Short, punchy machine-gun shot."""
    rng = np.random.default_rng(2001)
    seconds = 0.5
    t = timeline(seconds)
    crack = shaped_noise(seconds, rng, 400, 12000, tilt=0.4) * decay(t, 0.08)
    body = one_pole_lowpass(shaped_noise(seconds, rng, 40, 2500, tilt=1.0), 1800) * decay(t, 0.25)
    thump = sweep(t, 180, 55, 0.12) * decay(t, 0.15)
    return normalise((crack + 0.8 * body + 0.7 * thump) * attack(t, 0.001))


def missile_explosion():
    """Explosion when a missile hits."""
    rng = np.random.default_rng(2002)
    return normalise(explosion(1.8, rng))


def kamikaze_blast():
    """The Kamikaze weapon: a rushing whoosh into a big fiery blast."""
    rng = np.random.default_rng(2003)
    seconds = 2.0
    t = timeline(seconds)
    whoosh = shaped_noise(seconds, rng, 300, 6000, tilt=0.6) * np.exp(-((t - 0.08) / 0.06) ** 2)
    blast = explosion(seconds, rng, boom_hz=55.0, brightness=4500.0)
    crackle_points = (rng.random(len(t)) > 0.9993) * rng.random(len(t))
    crackle = np.convolve(crackle_points, np.exp(-np.arange(200) / 25.0), mode="same")
    crackle *= decay(t, 1.4) * (t > 0.1)
    return normalise(0.6 * whoosh + blast + 0.4 * crackle)


def player_explosion():
    """The player's plane blowing up: a deep explosion with falling metal debris."""
    rng = np.random.default_rng(2004)
    seconds = 2.7
    t = timeline(seconds)
    blast = explosion(seconds, rng, boom_hz=45.0, brightness=2500.0)
    debris = np.zeros_like(t)
    for _ in range(14):
        start = 0.15 + rng.random() * 1.8
        pitch = 900 + rng.random() * 2600
        local = np.clip(t - start, 0.0, None)
        ring = np.sin(2 * np.pi * pitch * local) + 0.5 * np.sin(2 * np.pi * pitch * 2.76 * local)
        debris += (t >= start) * ring * decay(local, 0.12) * (0.3 + 0.7 * rng.random())
    # Soft clipping makes the final explosion feel louder and heavier than the others.
    return normalise(np.tanh(3.0 * normalise(blast + 0.18 * debris)))


def propeller_engine():
    """Looping propeller engine drone (4 seconds, loops seamlessly)."""
    rng = np.random.default_rng(2005)
    seconds = 4.0
    t = timeline(seconds)
    # Whole-number frequencies repeat exactly every second, so the loop has no seam.
    engine = sum(np.sin(2 * np.pi * 62 * harmonic * t) / harmonic for harmonic in range(1, 9))
    blade_rate = np.abs(np.sin(2 * np.pi * 24 * t)) ** 0.5
    air = shaped_noise(seconds, rng, 80, 1200, tilt=1.0)
    return normalise(0.6 * engine * (0.6 + 0.4 * blade_rate) + 0.5 * air * blade_rate, peak=0.7)


def rocket_loop(seconds, rng, low_hz, high_hz, whine_hz):
    """Looping rocket motor: band-limited roar with a steady whine on top."""
    t = timeline(seconds)
    roar = shaped_noise(seconds, rng, low_hz, high_hz, tilt=0.8)
    flutter = 0.85 + 0.15 * np.sin(2 * np.pi * 7 * t)
    whine = np.sin(2 * np.pi * whine_hz * t)
    return normalise(roar * flutter + 0.08 * whine, peak=0.7)


def missile_motor():
    """Loop for the straight-flying missile."""
    return rocket_loop(4.0, np.random.default_rng(2006), 60, 2500, 440)


def guided_missile_motor():
    """Loop for guided missiles: higher and hissier than the plain missile."""
    return rocket_loop(10.0, np.random.default_rng(2007), 200, 7000, 880)


SOUNDS = {
    "MachineGunShot.wav": gunshot,
    "MissileExplosion.wav": missile_explosion,
    "KamikazeBlast.wav": kamikaze_blast,
    "PlaneExplosion.wav": player_explosion,
    "PropellerEngine.wav": propeller_engine,
    "MissileMotor.wav": missile_motor,
    "GuidedMissileMotor.wav": guided_missile_motor,
}


def main():
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)
    for file_name, build in SOUNDS.items():
        write_wav(OUTPUT_DIR / file_name, build())
        print(f"wrote {OUTPUT_DIR / file_name}")


if __name__ == "__main__":
    main()
