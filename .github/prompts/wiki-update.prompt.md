---
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
