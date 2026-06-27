# Changelog

All notable changes to **BanglaType** are documented here.
The format is based on [Keep a Changelog](https://keepachangelog.com/),
and this project adheres to [Semantic Versioning](https://semver.org/).

## [1.0.2] - 2026-06-27

### Added
- **BanglaType Notepad — full rewrite** with a built-in, self-contained
  Banglish (Avro Phonetic) typing engine that transliterates Roman input to
  Bangla word-by-word, independent of the global keyboard hook.
- Live **word suggestions** in the Notepad — both Bangla (phonetic) and plain
  **English** words; ↑/↓ to choose, Tab to accept, Esc to dismiss.
- **Auto-correct** and **text-expansion macros** applied on word commit.
- **Persistent Notepad preferences** — phonetic mode, suggestions, auto-correct,
  macros, word wrap, dark mode, font and zoom are remembered across sessions.
- **Auto-save & crash recovery** — drafts are saved every 30 s and offered for
  recovery on the next launch after an unclean exit.
- **Emoji & Bangla symbol picker** (Insert menu) for quick insertion of emojis
  and symbols (।, ৳, ঃ, ং, ঁ, ়, ্, ৎ, quotes, dash).
- **Bangla date/time insertion** and **number conversion** (123 ↔ ১২৩) over the
  selection or whole document.
- **Spell check** (F7) that flags unknown Bangla words and offers suggestions.
- **Bijoy ANSI ↔ Unicode** in the Notepad: copy/save as Bijoy, plus
  **paste from** and **import** legacy Bijoy ANSI text/files into Unicode.
- **Find & Replace** dialog, **zoom** (with status-bar percentage), **print** and
  **print preview**, **recent files**, and **dark mode** in the Notepad.
- AI assist (Gemini) seeded from the current selection, and a voice-typing hook.
- `SuggestionEngine.IsKnownWord` public helper backing the spell-checker.

### Changed
- Installer (`BanglaType.iss`) bumped to **1.0.2**: x64 setup mode, grouped Start
  Menu shortcuts, versioned output filename, and cleaner VC++ redistributable
  detection.
- Notepad files now save as UTF-8 (with BOM).

## [1.0.1] - 2026-06

### Added
- Silent Visual C++ 2015–2022 redistributable detection/installation.
- New layout data (Avro, Borno, Munir Optima, Jatiya) and updated parser/UI.
- Glassmorphic landing page with live transliteration sandbox, theme switcher,
  and FAQ.

### Changed
- EULA updated to 1.0.1; project structure reorganised; uninstall cleanup.

## [1.0.0] - 2026

### Added
- Initial release: phonetic & fixed Bangla layouts, suggestions, voice typing,
  clipboard manager, custom layout builder, and Windows installer.

[1.0.2]: https://github.com/mohammad-sheikh-shahinur-rahman/BanglaType/releases/tag/v1.0.2
[1.0.1]: https://github.com/mohammad-sheikh-shahinur-rahman/BanglaType/releases/tag/v1.0.1
[1.0.0]: https://github.com/mohammad-sheikh-shahinur-rahman/BanglaType/releases/tag/v1.0.0
