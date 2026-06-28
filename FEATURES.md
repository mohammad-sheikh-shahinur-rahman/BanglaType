# ⌨️ BanglaType — Product Features

> A free, intelligent, offline-first **Bangla / English Input Method Editor (IME)** for Windows.
> Type Bangla anywhere — phonetically or with fixed layouts — with AI suggestions, voice typing,
> a full Notepad, and a privacy-first, telemetry-free design.

**Current release:** v1.0.5 · Windows 10/11 (x86 & x64) · .NET Framework 4.8

---

## 1. Typing Engines & Layouts

| Feature | What it does |
| --- | --- |
| **BanglaType Phonetic** | Built-in transliteration engine — type `ami` → `আমি`. |
| **Avro Phonetic (Open)** | Full Avro-compatible phonetic mode for users migrating from Avro. |
| **Fixed Layouts** | Bijoy, National (Jatiya), Borno, Munir Optima, Probhat, Avro Easy — traditional key-mapped typing. |
| **Custom Layout Builder** | Create, compile and share your own layouts as `.kbl` XML files. |
| **System-wide input** | A global keyboard hook injects Bangla into any application — browsers, Office, design tools. |
| **Mode indicator** | Colour-coded topbar label — 🟢 Bangla · 🔴 Banglish · ⚪ English — with a toggle hotkey (`F12`/`F10`). |

---

## 2. AI-Powered Suggestions & Prediction

- **Real-time word suggestions** for both Bangla (phonetic) and English.
- **Next-word prediction** that learns from an offline vocabulary as you type.
- **Google recommendation API** integration for richer online suggestions.
- **Gemini AI assistant** — rewrite, format, and tone-adapt text (Formal / Casual / Friendly), with emoji insertion.
- **Variation customizer** — teach the parser your own Romanized spellings so it matches how *you* type.

---

## 3. Voice Typing (Speech-to-Text)

- Local, browser-based **speech-typing portal** using Google speech recognition for Bangla and English.
- **Auto-paste** of recognised speech straight into the active application, with single-click visual feedback.

---

## 4. BanglaType Notepad (Built-in Editor)

A full editor with its own self-contained Avro-phonetic engine, independent of the global hook:

- **Live word suggestions** (Bangla + English) — ↑/↓ to choose, Tab to accept, Esc to dismiss.
- **Auto-correct** and **text-expansion macros** applied on each word commit.
- **Spell check (F7)** flagging unknown Bangla words with suggestions.
- **Bijoy ANSI ↔ Unicode** — copy/save as Bijoy, paste-from and import legacy Bijoy text/files.
- **Emoji & Bangla symbol picker** (।, ৳, ঃ, ং, ঁ, ্, ৎ, quotes, dash…).
- **Bangla date/time insertion** and **number conversion** (123 ↔ ১২৩).
- **Find & Replace**, **zoom** with status-bar %, **print / print preview**, **recent files**, **dark mode**, word wrap.
- **Auto-save & crash recovery** — drafts saved every 30 s and offered for recovery after an unclean exit.
- **Persistent preferences** — phonetic mode, suggestions, auto-correct, macros, font, zoom remembered across sessions.
- Files saved as **UTF-8 (with BOM)**.

---

## 5. Productivity Tools

| Tool | Description |
| --- | --- |
| **Text-Expansion Macros** | Abbreviations expand to full text — e.g. `!th` → `ধন্যবাদ`, `!as` → `আসসালামু আলাইকুম`, `;ok` → `ঠিক আছে 👍`. Fully editable. |
| **Clipboard Manager** | Keeps your last **10 copied texts** for instant reuse. |
| **Text Converter** | Standalone **Bijoy ANSI ↔ Unicode** converter for legacy graphic-design workflows (Illustrator, Photoshop). |
| **Floating On-Screen Keyboard** | Click-to-type virtual keyboard for touch and mouse input. |
| **Dictionary Manager** | View, add, and prune the personal word dictionary that powers suggestions. |
| **Stickers & Emoji** | Quick-insert sticker and emoji panel. |
| **Typing Analytics** | Tracks total keystrokes, backspaces, live **WPM**, and most-used words — all stored locally. |

---

## 6. Migration & Onboarding

- **Switching from Avro?** Toggle key defaults to `F12`; pick **Avro Phonetic (Open)** and keep typing as before.
- **Switching from Bijoy?** Toggle key set to `F10`, National layout active, `Ctrl+Alt+B` (ANSI) / `Ctrl+Alt+V` (Unicode) shortcuts enabled for legacy design tools.
- **First-Run Setup Wizard** guides theme, interface language, and privacy choices on first launch.

---

## 7. Appearance & Interface

- **16 built-in themes** applied live to the topbar, suggestion popup, and dialogs — Cream, Light, Dark, Ocean, Midnight Purple, Neon Cyberpunk, Amoled, Synthwave Dracula, Sakura, Forest Moss, Nordic Frost, Solarized Light/Dark, Monokai Pro, Retro Terminal.
- **Shared `UiTheme` styler** for consistent backgrounds, flat hover/press buttons, and accent actions across every secondary window.
- **Colour-coded mode label**, theme-tinted hover feedback, and a subtle gradient sheen on the topbar.

---

## 8. Reliability & Deployment

- **Silent VC++ runtime install** — the setup detects and installs Microsoft Visual C++ 2015–2022 (x86 & x64), resolving `msvcp140.dll` errors automatically.
- **Safe Mode** — launch `BanglaType.exe --safe` to bypass the advanced hook/suggestion startup for guaranteed typing recovery and stable debugging.
- **Two installers** — a professional setup `.exe` (bundles VC++ runtimes) and a per-machine **MSI** package.
- Clean install **and** complete uninstall profiles.

---

## 9. Privacy & Safety

- **100% offline-first** — transliteration, local suggestions, layout compilation, and clipboard run entirely on your machine.
- **No telemetry** — keystrokes are never logged, stored, or sent to external servers.
- Online features (Google suggestions, Gemini AI, voice) are **opt-in** and only active when you use them.

---

*Developed by **Mohammad Sheikh Shahinur Rahman** — [shahinurrahman.com](https://shahinurrahman.com/)*
