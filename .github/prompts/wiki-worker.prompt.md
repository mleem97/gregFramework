---
mode: 'agent'
tools: ['terminal', 'codebase']
description: 'Worker-Instanz: Beansprucht eine freie Wiki-Seite, generiert sie vollständig und markiert sie als done. Kann parallel in mehreren Chat-Panels laufen.'
---

# Wiki-Worker — Parallele Instanz

Du bist ein **Worker** im parallelen Ralph Loop. Mehrere Instanzen von dir
laufen gleichzeitig in verschiedenen Chat-Panels. Koordination über Dateisperren.

## Schritt 1 — Freie Seite beanspruchen (atomic lock)

```bash
# Generiere eindeutige Worker-ID
WORKER_ID="worker-$$-$(date +%s%N)"

# Finde erste freie .todo Seite und lock sie
CLAIMED=""
for todo_file in .wiki-state/pages/*.todo; do
  [ -f "$todo_file" ] || continue
  base="${todo_file%.todo}"
  lock_file="${base}.lock"
  # Atomic: nur wenn lock noch nicht existiert
  if ( set -o noclobber; echo "$WORKER_ID" > "$lock_file" ) 2>/dev/null; then
    rm "$todo_file"
    CLAIMED="$base"
    echo "✓ Claimed: $(basename "$CLAIMED") als $WORKER_ID"
    break
  fi
done

if [ -z "$CLAIMED" ]; then
  # Prüfe ob noch Feedback-Seiten warten
  FEEDBACK=$(ls .wiki-state/pages/*.feedback 2>/dev/null | head -1)
  if [ -n "$FEEDBACK" ]; then
    base="${FEEDBACK%.feedback}"
    lock_file="${base}.lock"
    if ( set -o noclobber; echo "$WORKER_ID" > "$lock_file" ) 2>/dev/null; then
      rm "$FEEDBACK"
      CLAIMED="$base"
      echo "↩ Revision claimed: $(basename "$CLAIMED")"
    fi
  fi
fi

if [ -z "$CLAIMED" ]; then
  echo "ℹ Keine freien Seiten verfügbar. Alle Seiten sind in Bearbeitung oder fertig."
  echo "→ Starte /wiki-orchestrator um blockierte Locks aufzuräumen."
  exit 0
fi

echo "Bearbeite: $(basename "$CLAIMED")"
```

## Schritt 2 — Feedback lesen (falls Revision)

```bash
PAGE_NAME=$(basename "$CLAIMED")
FEEDBACK_FILE=".wiki-state/review/${PAGE_NAME}.md.feedback"
if [ -f "$FEEDBACK_FILE" ]; then
  echo "=== REVISION: Feedback lesen ==="
  cat "$FEEDBACK_FILE"
fi
```

## Schritt 3 — Wiki-Seite generieren

**Jetzt generierst du den vollständigen Inhalt der Seite.**

Ermittle zuerst welche Seite du bearbeiten sollst:
```bash
echo "Seite: $PAGE_NAME"
# z.B. "moddev__setup-csharp" → docs/wiki/moddev/setup-csharp.md
OUTPUT_PATH=$(echo "$PAGE_NAME" | sed 's/__/\//g')
echo "Output: docs/wiki/${OUTPUT_PATH}.md"
```

Generiere jetzt die Seite basierend auf diesem Mapping:

| Page-Name | Datei | Inhalt |
|---|---|---|
| `index` | `docs/wiki/index.md` | Startseite: Ein-Satz-Erklärung, 4 Buttons, Architektur, Inoffiziell-Badge |
| `glossar` | `docs/wiki/glossar.md` | IL2CPP, Interop-Assembly (leere Methodenbodies!), C-ABI, HarmonyX, blittable, GameContext |
| `changelog` | `docs/wiki/changelog.md` | Format, SemVer, Breaking Changes |
| `known-issues` | `docs/wiki/known-issues.md` | Inkompatibilitäten, Workarounds |
| `legal` | `docs/wiki/legal.md` | §69e UrhG, EU Directive Art.6 |
| `enduser__index` | `docs/wiki/enduser/index.md` | Übersicht End-User |
| `enduser__installation` | `docs/wiki/enduser/installation.md` | MelonLoader + FrikaMF, Schritt-für-Schritt, kein Coding-Wissen nötig |
| `enduser__update` | `docs/wiki/enduser/update.md` | Update-Prozess |
| `enduser__deinstallation` | `docs/wiki/enduser/deinstallation.md` | Vollständige Deinstallation |
| `enduser__faq` | `docs/wiki/enduser/faq.md` | Häufige Fehler, Troubleshooting |
| `moddev__index` | `docs/wiki/moddev/index.md` | Rust vs C# Entscheidungstabelle |
| `moddev__setup-csharp` | `docs/wiki/moddev/setup-csharp.md` | C# Getting Started, echtes Beispiel-Mod |
| `moddev__setup-rust` | `docs/wiki/moddev/setup-rust.md` | Rust FFI Getting Started, echtes Beispiel-Mod |
| `moddev__api-reference` | `docs/wiki/moddev/api-reference.md` | Hook-Klassen (Server, NetworkSwitch, etc.) |
| `moddev__reverse-engineering` | `docs/wiki/moddev/reverse-engineering.md` | dnSpy/dotPeek, leere Methodenbodies erklären |
| `moddev__architecture` | `docs/wiki/moddev/architecture.md` | Architektur-Diagramm, GameContext, C-ABI |
| `contributor__index` | `docs/wiki/contributor/index.md` | Contributor-Übersicht |
| `contributor__dev-setup` | `docs/wiki/contributor/dev-setup.md` | Build-Voraussetzungen |
| `contributor__project-structure` | `docs/wiki/contributor/project-structure.md` | Ordnerstruktur erklärt |
| `contributor__add-hook` | `docs/wiki/contributor/add-hook.md` | Hook hinzufügen, Schritt-für-Schritt |
| `contributor__conventions` | `docs/wiki/contributor/conventions.md` | Naming, blittable, Conventional Commits |
| `contributor__pitfalls` | `docs/wiki/contributor/pitfalls.md` | b###-Methoden, Coroutinen, Prefix/Postfix |
| `contributor__ci` | `docs/wiki/contributor/ci.md` | CI ohne Spielinstallation |
| `sponsor__index` | `docs/wiki/sponsor/index.md` | Sponsoring-Optionen, Hall of Fame |
| `gamedev__index` | `docs/wiki/gamedev/index.md` | Brief an WASEKU, §69e, Kooperationsangebot |

### Pflichtformat jeder Seite:

```markdown
---
title: "Seitentitel"
description: "Kurze Beschreibung (max. 160 Zeichen)"
sidebar_position: [Nummer]
tags:
  - audience: [enduser|moddev|contributor|sponsor|gamedev]
---

# Seitentitel

[Vollständiger Inhalt auf Deutsch, Code-Kommentare auf Englisch]
```

### Pflichtregeln:
- **Deutsch** (Code-Kommentare Englisch)
- Code-Beispiele **IMMER** in beiden Sprachen: `🦀 Rust` und `🔷 C#`
- Kein Platzhalter, kein "TODO", kein "Details folgen"
- Interne Links als relative Pfade: `../contributor/add-hook.md`
- Hinweis auf Code-Seiten: "Du musst nicht beide Sprachen kennen."

## Schritt 4 — Datei schreiben und Lock aufräumen

```bash
# Datei wurde oben generiert und geschrieben
# Lock entfernen und als "done" markieren
rm ".wiki-state/pages/${PAGE_NAME}.lock"
touch ".wiki-state/pages/${PAGE_NAME}.done"
echo "✓ Fertig: docs/wiki/${OUTPUT_PATH}.md"
echo "→ Bereit für nächste Seite. Führe /wiki-worker erneut aus."
```

## Schritt 5 — Nächste Seite prüfen

```bash
REMAINING=$(ls .wiki-state/pages/*.todo 2>/dev/null | wc -l | tr -d ' ')
echo "Noch $REMAINING Seiten in Queue."
```
