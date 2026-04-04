---
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
