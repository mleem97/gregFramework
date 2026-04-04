---
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
