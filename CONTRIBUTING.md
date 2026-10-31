# Contributing to Kamikaze Hero

Thanks for your interest. This is a small hobby project kept public for
learning and reuse, so contributions of any size are welcome: bug fixes,
cleanup, documentation, or new gameplay.

## Before you start

- For anything bigger than a small fix, open an issue first to describe what
  you want to change. That avoids work on something that may not fit.
- Read [docs/PROJECT_STRUCTURE.md](docs/PROJECT_STRUCTURE.md) to get oriented.
- Use the Unity version in `ProjectSettings/ProjectVersion.txt`
  (currently 6000.6.7f1). Opening the project in another version rewrites many
  files and makes your pull request hard to review.

## Making changes

1. Fork the repository and create a branch from `main`.
2. Make your change in Unity, play it in the editor, and if it affects mobile
   input, try it on an Android device if you can.
3. Commit the `.meta` file with every asset you add, move or rename.
4. Do not commit `Library/`, `Temp/`, `Logs/`, `UserSettings/` or build output.
   The `.gitignore` already excludes them.

## Commits

Keep commits small and focused, one logical change each, so the history
explains itself:

- The **subject line** says what changed, in the imperative
  ("Add pause button", not "Added pause button"), in about 50 characters.
- The **body** explains why the change was needed and anything a reviewer
  would not see from the diff.
- Keep unrelated scene or prefab re-serialization out of your commits. If Unity
  touched files you did not mean to change, discard those before committing.

## Code style

Follow the style of the surrounding code:

- C# with four-space indentation (some older scripts use two; use four for new
  code) and braces on their own line.
- `PascalCase` for public fields, properties and methods; `camelCase` for
  private fields and locals.
- Prefer `GetComponent<T>()` and serialized references over string lookups.
- Keep tuning values (speeds, delays) as Inspector fields rather than
  hard-coded numbers.

## Pull requests

- Describe what you changed and why, and how you tested it.
- Add screenshots or a short clip for visual changes.
- Update `CHANGELOG.md` under **Unreleased** for anything a player or
  developer would notice.

## Licensing of contributions

By contributing, you agree that your code is licensed under the
[MIT License](LICENSE) and your assets under
[CC BY 4.0](LICENSE-ASSETS.md), the same as the rest of the project. Only
contribute assets you made yourself or that are under a license compatible
with reuse, and add third-party assets to [CREDITS.md](CREDITS.md).
