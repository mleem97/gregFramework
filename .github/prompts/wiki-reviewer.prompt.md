---
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
