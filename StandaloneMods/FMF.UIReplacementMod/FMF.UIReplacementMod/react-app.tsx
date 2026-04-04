// @ts-nocheck
import { useMemo, useState } from 'react'
import { useFmfBridge } from './hooks/useFmfBridge'
import type { FmfAction } from './types/bridge'

type UiActionCard = {
  action: FmfAction
  label: string
  description: string
}

type LogEntry = {
  id: number
  title: string
  detail: string
  tone: 'info' | 'success' | 'warning'
}

const actionCards: UiActionCard[] = [
  { action: 'continue', label: 'Continue', description: 'Resume the active save session.' },
  { action: 'new-game', label: 'New Game', description: 'Start a new campaign from scratch.' },
  { action: 'multiplayer', label: 'Multiplayer', description: 'Open multiplayer panel and room flows.' },
  { action: 'settings', label: 'Settings', description: 'Navigate to settings menu.' },
  { action: 'exit', label: 'Exit', description: 'Quit game from wrapper action pipeline.' },
  { action: 'sync-state', label: 'Sync State', description: 'Request runtime state snapshot from C#.' },
]

export default function App() {
  const { mode, wrapperStateLabel, lastSyncState, invokeAction } = useFmfBridge()
  const [lastAction, setLastAction] = useState<FmfAction>('sync-state')
  const [logEntries, setLogEntries] = useState<LogEntry[]>([
    {
      id: 1,
      title: 'Bridge ready',
      detail: 'UI initialized and waiting for wrapper commands.',
      tone: 'success',
    },
  ])

  const maxPlayersText = useMemo(() => {
    if (!lastSyncState) {
      return 'n/a'
    }

    return String(lastSyncState.maxPlayers)
  }, [lastSyncState])

  const pushLog = (title: string, detail: string, tone: LogEntry['tone']) => {
    setLogEntries((current) => [{ id: Date.now(), title, detail, tone }, ...current.slice(0, 5)])
  }

  const handleAction = (action: FmfAction) => {
    const delivered = invokeAction(action, { source: 'react-ui', action })
    setLastAction(action)

    pushLog(
      delivered ? `Dispatched: ${action}` : `Queued: ${action}`,
      delivered ? 'Command was sent through wrapper bridge.' : 'No active bridge found, fallback channel used.',
      delivered ? 'success' : 'warning',
    )
  }

  return (
    <div className="app-shell">
      <div className="aurora" />
      <div className="app-card">
        <div className="header-row">
          <div>
            <p className="eyebrow">Frika Mod Framework</p>
            <h1>Data Center — Start Menu</h1>
            <p className="subtitle">Modernized main menu with integrated framework actions and multiplayer entry point.</p>
          </div>
          <div className="status-chip">{wrapperStateLabel}</div>
        </div>

        <div className="quick-cards">
          <div className="quick-card">
            <h3>Runtime</h3>
            <p>{mode === 'wrapper-connected' ? 'Bridge attached' : 'Standalone preview'} · Last action: {lastAction}</p>
          </div>
          <div className="quick-card">
            <h3>Sync Snapshot</h3>
            <p>Max players: {maxPlayersText}</p>
          </div>
          <div className="quick-card">
            <h3>Scene</h3>
            <p>{lastSyncState?.sceneName ?? 'unknown'}</p>
          </div>
        </div>

        <div className="menu-grid">
          {actionCards.map((card) => (
            <button
              key={card.action}
              type="button"
              className={`menu-action-button ${card.action === 'exit' ? 'menu-action-button--danger' : card.action === 'sync-state' ? 'menu-action-button--secondary' : 'menu-action-button--primary'}`}
              style={{ borderWidth: '1px', borderStyle: 'solid' }}
              onClick={() => handleAction(card.action)}
            >
              {card.label}
              <span className="button-subtitle">{card.description}</span>
            </button>
          ))}
        </div>

        <div className="log-list">
          {logEntries.map((entry) => (
            <div key={entry.id} className={`log-item log-item--${entry.tone}`}>
              <strong>{entry.title}</strong>
              <p>{entry.detail}</p>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}
