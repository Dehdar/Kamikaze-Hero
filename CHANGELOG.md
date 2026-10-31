# Changelog

All notable changes to Kamikaze Hero are recorded here, newest first. The
format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

The game was never tagged with release numbers in git, so entries are named
after what they did. The in-game bundle version is still `1.0.1.1`.

## [Unreleased] – Open-source release

Preparing the repository so anyone can learn from it or build on it.

### Added
- MIT license (`LICENSE`): free to use in any context, with credit to me.
- `CREDITS.md` listing third-party content and its licenses.
- `README.md` explaining why the project is public, how to play, and how to
  open and build it.
- Project structure guide (`docs/PROJECT_STRUCTURE.md`).
- `CONTRIBUTING.md`, `CODE_OF_CONDUCT.md` and `SECURITY.md`.
- This changelog.

### Removed
- `License.txt`, which only held two credit lines. They now live in
  `CREDITS.md`.

## Unity 6 migration – 2026-10-21

Brought the project from Unity 5.4.1f1 to Unity 6000.6.7f1 (Unity 6.6) and
removed features whose online services no longer work
([pull request #1](https://github.com/Dehdar/Kamikaze-Hero/pull/1)).

### Added
- `AdsManager`, which shows interstitial ads through the Unity Ads 4.x
  initialize/load/show API instead of the removed built-in Ads service.
- `Packages/manifest.json` and `packages-lock.json`, so Unity resolves the
  same package versions for everyone.
- Unity 6 `.gitignore` entries.
- Per-scene lighting settings assets created by the upgrade.

### Changed
- Upgraded the project and all assets to Unity 6000.6.7f1 and switched asset
  serialization to Force Text, so scenes and prefabs can be diffed in git.
- Android builds now use IL2CPP for ARMv7 and ARM64, with the target API level
  chosen automatically.
- Replaced obsolete APIs: `WWW.EscapeURL` with `UnityWebRequest.EscapeURL`,
  `Rigidbody.velocity`, and `ParticleSystem.startDelay` in the fire effects.
- Re-centred the three main-menu buttons and the Restart button after the
  leaderboard buttons were removed.
- Trimmed default Unity packages the game does not use.

### Removed
- **Facebook integration**: Facebook SDK 7.9.0, the Facebook login and score
  posting, and the `FacebookController` script and prefab. Facebook's Scores
  API stopped returning data in 2018, so these features no longer worked.
- **Leaderboard**: the Leaderboard scene and the Leaderboard and Save Score
  buttons, which depended on Facebook.
- The hand-written Android manifest and the bundled `Newtonsoft.Json.dll`,
  which only the Facebook code used.

## Original game – imported 2023-11-22

The game as first released for Android (bundle version `1.0.1.1`, built with
Unity 5.4). It was moved into this repository in one commit, without its
earlier git history.

### Features
- Side-scrolling plane shooter with a scrolling cloud background and music.
- 20 waves with rising difficulty: more enemies per wave, shorter spawn delays,
  paired spawns from wave 12, and 200 enemies in the final wave.
- Three enemy types: straight missiles, guided missiles that follow the
  player's height, and multi-missile formations.
- Three-shot burst machine gun with muzzle flash.
- Kamikaze special weapon with a radial cooldown button and brief
  invulnerability.
- Touch controls for movement, fire and kamikaze, with multi-touch support.
- In-game score and timer, and an end-of-game report with kills, misses,
  accuracy, wave reached and a nine-level verdict from "n00b" to "GOD".
- Main menu, About screen with a message from me, credits and an email link.
- Facebook login, score posting and a friends leaderboard.
- Unity Ads interstitial every third game, and a "no internet" dialog.
