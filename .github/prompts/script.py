
import os

base = '/home/user/output2'
os.makedirs(f'{base}/.github/prompts', exist_ok=True)
os.makedirs(f'{base}/.wiki-state/pages', exist_ok=True)

# ──────────────────────────────────────────────────────────────────────────────
# 1. wiki-init.prompt.md  (Entry Point — /initwiki)
# ──────────────────────────────────────────────────────────────────────────────
wiki_init = '''---
mode: 'agent'
tools: ['terminal', 'codebase']
description: 'Legt das FrikaMF Wiki vollständig neu an und startet den Orchestrierungs-Loop'
---

# /initwiki — FrikaMF Wiki initialisieren

Du bist der **Init-Agent** für das FrikaMF Wiki. Deine Aufgabe: Initialisiere
den geteilten State, dann starte den Orchestrierer.

## Schritt 1 — State-Verzeichnis aufräumen und neu anlegen

```bash
rm -rf .wiki-state
mkdir -p .wiki-state/pages .wiki-state/review
touch .wiki-state/progress.md
echo "0" > .wiki-state/iteration.txt
```

## Schritt 2 — Alle Seiten als `.todo` registrieren

Lege für **jede** der folgenden 25 Seiten eine leere `.todo`-Datei an.
Das Datei-Basename entspricht dem Wiki-Pfad (Slashes → Doppel-Unterstrich):

```bash
# Shared
touch .wiki-state/pages/index.todo
touch .wiki-state/pages/glossar.todo
touch .wiki-state/pages/changelog.todo
touch .wiki-state/pages/known-issues.todo
touch .wiki-state/pages/legal.todo

# End-User
touch .wiki-state/pages/enduser__index.todo
touch .wiki-state/pages/enduser__installation.todo
touch .wiki-state/pages/enduser__update.todo
touch .wiki-state/pages/enduser__deinstallation.todo
touch .wiki-state/pages/enduser__faq.todo

# Mod-Developer
touch .wiki-state/pages/moddev__index.todo
touch .wiki-state/pages/moddev__setup-csharp.todo
touch .wiki-state/pages/moddev__setup-rust.todo
touch .wiki-state/pages/moddev__api-reference.todo
touch .wiki-state/pages/moddev__reverse-engineering.todo
touch .wiki-state/pages/moddev__architecture.todo

# Contributor
touch .wiki-state/pages/contributor__index.todo
touch .wiki-state/pages/contributor__dev-setup.todo
touch .wiki-state/pages/contributor__project-structure.todo
touch .wiki-state/pages/contributor__add-hook.todo
touch .wiki-state/pages/contributor__conventions.todo
touch .wiki-state/pages/contributor__pitfalls.todo
touch .wiki-state/pages/contributor__ci.todo

# Sponsor + GameDev
touch .wiki-state/pages/sponsor__index.todo
touch .wiki-state/pages/gamedev__index.todo
```

## Schritt 3 — Dokumentationsstruktur anlegen

```bash
mkdir -p docs/wiki/enduser docs/wiki/moddev docs/wiki/contributor docs/wiki/sponsor docs/wiki/gamedev
```

## Schritt 4 — Progress initialisieren

```bash
cat > .wiki-state/progress.md << 'EOF'
# Wiki Progress

| Status | Anzahl |
|--------|--------|
| todo   | 25     |
| locked | 0      |
| done   | 0      |
| approved | 0    |
| feedback | 0    |

Gestartet: $(date)
EOF
```

## Schritt 5 — Anweisung ausgeben

Gib folgende Anweisung aus:

---
**Wiki-State initialisiert. Starte jetzt parallele Worker-Instanzen:**

1. Öffne **3–5 neue Copilot-Chat-Panels** (Symbol oben rechts im Chat-Panel)
2. Führe in **jedem Panel** aus:
   `@workspace /wiki-worker`
3. Starte in einem **separaten Panel** den Reviewer:
   `@workspace /wiki-reviewer`
4. Überwache Fortschritt:
   `@workspace /wiki-orchestrator`

Jeder Worker nimmt sich selbstständig die nächste freie Seite.
---
'''

# ──────────────────────────────────────────────────────────────────────────────
# 2. wiki-orchestrator.prompt.md  (Monitoring + Ralph-Loop-Koordination)
# ──────────────────────────────────────────────────────────────────────────────
wiki_orchestrator = '''---
mode: 'agent'
tools: ['terminal', 'codebase']
description: 'Orchestriert den parallelen Ralph Loop — überwacht Fortschritt, entsperrt blockierte Seiten, startet neue Iterationen'
---

# Wiki-Orchestrator — Ralph Loop Koordination

Du bist der **Orchestrierer**. Du GENERIERST KEINEN Inhalt selbst.
Du koordinierst, überwachst und reparierst den parallelen Loop.

## Schritt 1 — Aktuellen State analysieren

```bash
echo "=== TODO ==="
ls .wiki-state/pages/*.todo 2>/dev/null | wc -l

echo "=== LOCKED ==="
ls .wiki-state/pages/*.lock 2>/dev/null
for f in .wiki-state/pages/*.lock; do
  [ -f "$f" ] && echo "  $f (gelockt seit: $(stat -c %y "$f" 2>/dev/null || stat -f %Sm "$f"))"
done

echo "=== DONE (warten auf Review) ==="
ls .wiki-state/pages/*.done 2>/dev/null | wc -l

echo "=== APPROVED ==="
ls .wiki-state/pages/*.approved 2>/dev/null | wc -l

echo "=== FEEDBACK (braucht Revision) ==="
ls .wiki-state/pages/*.feedback 2>/dev/null

echo "=== GESAMT ==="
echo "Todo:     $(ls .wiki-state/pages/*.todo     2>/dev/null | wc -l)"
echo "Locked:   $(ls .wiki-state/pages/*.lock     2>/dev/null | wc -l)"
echo "Done:     $(ls .wiki-state/pages/*.done     2>/dev/null | wc -l)"
echo "Approved: $(ls .wiki-state/pages/*.approved 2>/dev/null | wc -l)"
echo "Feedback: $(ls .wiki-state/pages/*.feedback 2>/dev/null | wc -l)"
```

## Schritt 2 — Blockierte Locks aufräumen (älter als 10 Minuten)

Locks die zu alt sind bedeuten, dass ein Worker abgestürzt ist.
Setze sie auf `.todo` zurück damit ein neuer Worker sie übernehmen kann:

```bash
find .wiki-state/pages -name "*.lock" -mmin +10 | while read f; do
  base="${f%.lock}"
  echo "  ⚠ Entsperre blockierten Lock: $f"
  mv "$f" "${base}.todo"
done
```

## Schritt 3 — Feedback-Seiten zurück in Queue stellen

Seiten mit `.feedback` müssen von einem Worker überarbeitet werden:

```bash
for f in .wiki-state/pages/*.feedback; do
  [ -f "$f" ] || continue
  base="${f%.feedback}"
  # Kopiere Feedback in review-Ordner bevor wir zurückstellen
  cp "$f" ".wiki-state/review/$(basename "$base").md.feedback" 2>/dev/null || true
  mv "$f" "${base}.todo"
  echo "  ↩ Zurück in Queue: $(basename "$base")"
done
```

## Schritt 4 — Iteration hochzählen

```bash
ITER=$(cat .wiki-state/iteration.txt 2>/dev/null || echo "0")
ITER=$((ITER + 1))
echo $ITER > .wiki-state/iteration.txt
echo "Iteration: $ITER"
```

## Schritt 5 — Progress-Datei aktualisieren

```bash
TODO=$(ls .wiki-state/pages/*.todo     2>/dev/null | wc -l | tr -d ' ')
LOCK=$(ls .wiki-state/pages/*.lock     2>/dev/null | wc -l | tr -d ' ')
DONE=$(ls .wiki-state/pages/*.done     2>/dev/null | wc -l | tr -d ' ')
APPR=$(ls .wiki-state/pages/*.approved 2>/dev/null | wc -l | tr -d ' ')
FEED=$(ls .wiki-state/pages/*.feedback 2>/dev/null | wc -l | tr -d ' ')

cat > .wiki-state/progress.md << EOF
# Wiki Progress — Iteration $ITER

| Status        | Anzahl |
|---------------|--------|
| ⬜ todo       | $TODO  |
| 🔒 locked     | $LOCK  |
| ✅ done       | $DONE  |
| 🚀 approved   | $APPR  |
| 🔄 feedback   | $FEED  |

Fortschritt: $APPR / 25 Seiten approved
Zuletzt aktualisiert: $(date)
EOF
cat .wiki-state/progress.md
```

## Schritt 6 — Abschluss prüfen

```bash
APPR=$(ls .wiki-state/pages/*.approved 2>/dev/null | wc -l | tr -d ' ')
if [ "$APPR" -eq 25 ]; then
  echo "🎉 WIKI VOLLSTÄNDIG! Alle 25 Seiten approved."
  echo "COMPLETE: $(date)" > .wiki-state/.complete
else
  REMAINING=$((25 - APPR))
  echo "→ Noch $REMAINING Seiten ausstehend."
  echo "→ Führe /wiki-worker in offenen Chat-Panels aus um fortzufahren."
  echo "→ Führe /wiki-reviewer aus um fertige Seiten zu prüfen."
fi
```
'''

# ──────────────────────────────────────────────────────────────────────────────
# 3. wiki-worker.prompt.md  (Worker-Instanz — parallel ausführbar)
# ──────────────────────────────────────────────────────────────────────────────
wiki_worker = r'''---
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
'''

# ──────────────────────────────────────────────────────────────────────────────
# 4. wiki-reviewer.prompt.md  (Review-Instanz — Ralph Loop Review Phase)
# ──────────────────────────────────────────────────────────────────────────────
wiki_reviewer = r'''---
mode: 'agent'
tools: ['terminal', 'codebase']
description: 'Reviewer: Prüft fertige Wiki-Seiten auf Qualität und entscheidet APPROVE oder FEEDBACK. Läuft als separate Copilot-Instanz parallel zu den Workern.'
---

# Wiki-Reviewer — Ralph Loop Review Phase

Du bist der **Reviewer**. Du SCHREIBST KEINEN neuen Inhalt.
Du prüfst ob generierte Seiten die Qualitätskriterien erfüllen.

## Schritt 1 — Fertige Seiten finden

```bash
echo "=== Warte auf Review ==="
ls .wiki-state/pages/*.done 2>/dev/null || echo "(keine)"
```

Falls keine `.done`-Dateien: Warte und führe diesen Agenten später erneut aus.

## Schritt 2 — Erste fertige Seite reviewen

```bash
REVIEW_TARGET=$(ls .wiki-state/pages/*.done 2>/dev/null | head -1)
if [ -z "$REVIEW_TARGET" ]; then
  echo "ℹ Keine Seiten zum Reviewen. Später erneut ausführen."
  exit 0
fi

PAGE_NAME=$(basename "${REVIEW_TARGET%.done}")
OUTPUT_PATH=$(echo "$PAGE_NAME" | sed 's/__/\//g')
WIKI_FILE="docs/wiki/${OUTPUT_PATH}.md"

echo "Reviewe: $WIKI_FILE"
echo "=== Inhalt ==="
cat "$WIKI_FILE"
```

## Schritt 3 — Qualitätsprüfung (alle Kriterien)

Prüfe die Seite gegen diese Checkliste:

```bash
echo "=== Qualitätsprüfung: $PAGE_NAME ==="

# Frontmatter vorhanden?
head -5 "$WIKI_FILE" | grep -q "^---" && echo "✓ Frontmatter" || echo "✗ KEIN Frontmatter"

# Pflichtfelder im Frontmatter
grep -q "^title:" "$WIKI_FILE"       && echo "✓ title"       || echo "✗ FEHLT: title"
grep -q "^description:" "$WIKI_FILE" && echo "✓ description" || echo "✗ FEHLT: description"
grep -q "sidebar_position:" "$WIKI_FILE" && echo "✓ sidebar_position" || echo "✗ FEHLT: sidebar_position"
grep -q "audience:" "$WIKI_FILE"     && echo "✓ audience tag" || echo "✗ FEHLT: audience tag"

# Platzhalter? (SOFORT REVISE wenn gefunden)
grep -qi "TODO\|Platzhalter\|Details folgen\|Coming soon\|TBD" "$WIKI_FILE" \
  && echo "✗ PLATZHALTER GEFUNDEN" || echo "✓ Kein Platzhalter"

# Beide Sprachen vorhanden? (nur für Seiten die Code zeigen)
grep -q "🦀" "$WIKI_FILE" && grep -q "🔷" "$WIKI_FILE" \
  && echo "✓ Beide Sprachen (🦀 + 🔷)" \
  || echo "⚠ Nur eine Sprache (prüfe ob Code-Seite)"

# Deutsch? (Heuristik: mindestens 3 deutsche Wörter)
grep -qiE "\b(und|der|die|das|ist|nicht|für|werden|können|wenn)\b" "$WIKI_FILE" \
  && echo "✓ Deutsch erkannt" || echo "✗ SPRACHE PRÜFEN"

# Länge (min. 500 Zeichen)
CHARS=$(wc -c < "$WIKI_FILE")
[ "$CHARS" -gt 500 ] \
  && echo "✓ Länge OK ($CHARS Zeichen)" \
  || echo "✗ ZU KURZ ($CHARS Zeichen — min. 500)"
```

## Schritt 4 — Entscheidung: APPROVE oder FEEDBACK

**APPROVE** wenn alle Pflichtkriterien erfüllt:
- Frontmatter vollständig (title, description, sidebar_position, audience)
- Kein Platzhalter / TODO
- Auf Deutsch
- Min. 500 Zeichen
- Code-Seiten haben beide Sprachen (🦀 + 🔷)

**FEEDBACK** wenn ein Pflichtkriterium fehlt. Feedback muss konkret sein:
- ✗ "Verbesserungsbedarf" → NICHT AKZEPTABEL
- ✓ "Frontmatter fehlt title-Feld. Code-Beispiel für Rust fehlt ab Zeile 42." → AKZEPTABEL

```bash
# APPROVE:
rm ".wiki-state/pages/${PAGE_NAME}.done"
touch ".wiki-state/pages/${PAGE_NAME}.approved"
echo "✅ APPROVED: $PAGE_NAME"

# ODER FEEDBACK (kommentiere APPROVE aus, nutze das hier):
# rm ".wiki-state/pages/${PAGE_NAME}.done"
# cat > ".wiki-state/review/${PAGE_NAME}.md.feedback" << 'EOF'
# [Konkretes Feedback hier]
# EOF
# touch ".wiki-state/pages/${PAGE_NAME}.feedback"
# echo "🔄 FEEDBACK: $PAGE_NAME"
```

## Schritt 5 — Fortschritt ausgeben

```bash
echo "=== Aktueller Stand ==="
echo "Approved: $(ls .wiki-state/pages/*.approved 2>/dev/null | wc -l) / 25"
echo "Noch offen: $(( $(ls .wiki-state/pages/*.todo .wiki-state/pages/*.lock .wiki-state/pages/*.done .wiki-state/pages/*.feedback 2>/dev/null | wc -l) ))"
echo "→ Führe /wiki-reviewer erneut aus für nächste Seite."
```
'''

# ──────────────────────────────────────────────────────────────────────────────
# 5. wiki-update.prompt.md  (/updatewiki Sektion)
# ──────────────────────────────────────────────────────────────────────────────
wiki_update = '''---
mode: 'agent'
tools: ['terminal', 'codebase']
description: 'Stellt eine bestimmte Wiki-Sektion zurück in die Worker-Queue für ein Update'
---

# /updatewiki — Wiki-Sektion aktualisieren

Du requeust die Aktualisierung einer bestimmten Sektion.

## Welche Sektion soll aktualisiert werden?

Gib die Sektion an (oder frage den User falls nicht angegeben):
`enduser` | `moddev` | `contributor` | `sponsor` | `gamedev` | `glossar` | `all`

## Sektion zurück in Queue stellen

```bash
SECTION="${1:-all}"

reset_section() {
  local prefix="$1"
  for approved in .wiki-state/pages/${prefix}*.approved; do
    [ -f "$approved" ] || continue
    base="${approved%.approved}"
    echo "↩ Reset: $(basename "$base")"
    rm "$approved"
    touch "${base}.todo"
  done
}

case "$SECTION" in
  enduser)     reset_section "enduser__" ;;
  moddev)      reset_section "moddev__" ;;
  contributor) reset_section "contributor__" ;;
  sponsor)     reset_section "sponsor__" ;;
  gamedev)     reset_section "gamedev__" ;;
  glossar)     reset_section "glossar" ;;
  all)
    reset_section ""
    echo "Alle Seiten zurückgestellt."
    ;;
  *)
    echo "Unbekannte Sektion: $SECTION"
    echo "Gültig: enduser | moddev | contributor | sponsor | gamedev | glossar | all"
    exit 1
    ;;
esac

echo ""
echo "✓ Sektion '$SECTION' zurück in Queue."
echo "→ Führe /wiki-worker in offenen Panels aus um fortzufahren."
echo "→ Führe /wiki-orchestrator aus um Übersicht zu sehen."
```
'''

# ──────────────────────────────────────────────────────────────────────────────
# 6. copilot-instructions.md  (globale Instruktionen)
# ──────────────────────────────────────────────────────────────────────────────
copilot_instructions = '''# GitHub Copilot — FrikaModdingFramework

## Projekt

FrikaModdingFramework (FrikaMF) ist ein inoffizielles Modding-Framework für
"Data Center" (WASEKU, Unity IL2CPP). Unterstützt Mods in **C#** (MelonLoader/HarmonyX)
und **Rust** (C-ABI FFI via P/Invoke).

**Inoffiziell · Community-driven · Keine Zugehörigkeit zu WASEKU**

## Verfügbare Wiki-Agenten

| Command | Datei | Zweck |
|---|---|---|
| `/wiki-init` | `.github/prompts/wiki-init.prompt.md` | Wiki neu anlegen (State + Queue) |
| `/wiki-orchestrator` | `.github/prompts/wiki-orchestrator.prompt.md` | Fortschritt überwachen, Locks aufräumen |
| `/wiki-worker` | `.github/prompts/wiki-worker.prompt.md` | Seite generieren (parallel starten!) |
| `/wiki-reviewer` | `.github/prompts/wiki-reviewer.prompt.md` | Seiten reviewen |
| `/wiki-update` | `.github/prompts/wiki-update.prompt.md` | Sektion re-queuen |

## Schnellstart Wiki

```
1. /wiki-init                          → Einmalig: State & Queue anlegen
2. /wiki-worker  (Panel 1)             ┐
3. /wiki-worker  (Panel 2)             │ Parallel starten für Geschwindigkeit
4. /wiki-worker  (Panel 3)             ┘
5. /wiki-reviewer (eigenes Panel)      → Prüft fertige Seiten
6. /wiki-orchestrator                  → Fortschritt & Cleanup
```

## Technischer Kontext

- Spielklassen: `Il2Cpp.*` (z.B. `Il2Cpp.Server`, `Il2Cpp.NetworkSwitch`)
- Build: `dotnet build /p:GameDir="..."` oder env `DATA_CENTER_GAME_DIR`
- CI: `$(CI)=true` → keine Spielinstallation nötig
- Commits: Conventional Commits (`feat:`, `fix:`, `docs:`, `chore:`)
- Wiki-Output: `docs/wiki/`
- Wiki-State: `.wiki-state/`

## Allgemeine Regeln

- Wiki-Seiten: Deutsch (Code-Kommentare Englisch)
- Code-Beispiele IMMER in beiden Sprachen: 🦀 Rust + 🔷 C#
- Kein "TODO", kein "Details folgen", keine Platzhalter
- YAML-Frontmatter auf jeder Seite (title, description, sidebar_position, tags)
'''

# ──────────────────────────────────────────────────────────────────────────────
# Schreiben
# ──────────────────────────────────────────────────────────────────────────────
files = {
    '.github/prompts/wiki-init.prompt.md':         wiki_init,
    '.github/prompts/wiki-orchestrator.prompt.md': wiki_orchestrator,
    '.github/prompts/wiki-worker.prompt.md':        wiki_worker,
    '.github/prompts/wiki-reviewer.prompt.md':      wiki_reviewer,
    '.github/prompts/wiki-update.prompt.md':        wiki_update,
    '.github/copilot-instructions.md':              copilot_instructions,
}

for path, content in files.items():
    full = f'{base}/{path}'
    os.makedirs(os.path.dirname(full), exist_ok=True)
    with open(full, 'w') as f:
        f.write(content)
    print(f"  {len(content):>6} chars  →  {path}")

print(f"\n  {len(files)} Dateien | {sum(len(v) for v in files.values())} Zeichen gesamt")
