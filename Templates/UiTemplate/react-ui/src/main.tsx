import React, { useEffect, useMemo, useState } from 'react';
import { createRoot } from 'react-dom/client';

import './styles.css';

type FmfAction = 'continue' | 'new-game' | 'multiplayer' | 'settings' | 'exit' | 'sync-state';

type FmfMessage = {
  action: FmfAction;
  payload?: Record<string, unknown>;
  timestamp: string;
  source: 'fmf-react-ui';
};

type BridgeTransport = {
  invoke?: (action: string, payload?: Record<string, unknown>) => void;
  emit?: (action: string, payload?: Record<string, unknown>) => void;
  postMessage?: (message: FmfMessage) => void;
};

type RuntimeMode = 'wrapper-connected' | 'standalone-preview';

type ActionCard = {
  action: FmfAction;
  label: string;
  description: string;
};

type EventEntry = {
  id: number;
  title: string;
  detail: string;
  tone: 'info' | 'success' | 'warning';
};

declare global {
  interface Window {
    fmfBridge?: BridgeTransport;
  }
}

const actionCards: ActionCard[] = [
  {
    action: 'continue',
    label: 'Continue',
    description: 'Resume the current session through the FMF bridge.',
  },
  {
    action: 'new-game',
    label: 'New Game',
    description: 'Start a fresh campaign from the wrapper host.',
  },
  {
    action: 'multiplayer',
    label: 'Multiplayer',
    description: 'Open the network panel or relay lobby flow.',
  },
  {
    action: 'settings',
    label: 'Settings',
    description: 'Send the wrapper to the configuration screen.',
  },
  {
    action: 'exit',
    label: 'Exit',
    description: 'Request a clean shutdown from the C# wrapper.',
  },
  {
    action: 'sync-state',
    label: 'Sync State',
    description: 'Ask FMF to refresh the bridge snapshot.',
  },
];

const supportedLabels = ['Continue', 'New Game', 'Multiplayer', 'Settings', 'Exit'];

function detectBridge(): RuntimeMode {
  return window.fmfBridge ? 'wrapper-connected' : 'standalone-preview';
}

function sendToBridge(action: FmfAction, payload?: Record<string, unknown>): boolean {
  const message: FmfMessage = {
    action,
    payload,
    timestamp: new Date().toISOString(),
    source: 'fmf-react-ui',
  };

  const bridge = window.fmfBridge;
  if (bridge?.invoke) {
    bridge.invoke(action, payload);
    return true;
  }

  if (bridge?.emit) {
    bridge.emit(action, payload);
    return true;
  }

  if (bridge?.postMessage) {
    bridge.postMessage(message);
    return true;
  }

  if (window.parent && window.parent !== window) {
    window.parent.postMessage(message, '*');
    return true;
  }

  return false;
}

function App() {
  const [mode, setMode] = useState<RuntimeMode>(() => detectBridge());
  const [lastAction, setLastAction] = useState<FmfAction>('sync-state');
  const [eventLog, setEventLog] = useState<EventEntry[]>([
    {
      id: 1,
      title: 'Bridge ready',
      detail: 'The UI is ready to talk to the FMF C# wrapper.',
      tone: 'success',
    },
  ]);

  const wrapperState = useMemo(() => {
    return mode === 'wrapper-connected'
      ? 'Connected to `FMF.UIReplacementMod` bridge'
      : 'Standalone preview mode';
  }, [mode]);

  useEffect(() => {
    const listener = () => setMode(detectBridge());
    listener();

    const timer = window.setInterval(listener, 2000);
    return () => window.clearInterval(timer);
  }, []);

  function pushEvent(title: string, detail: string, tone: EventEntry['tone']) {
    setEventLog((current) => [
      {
        id: Date.now(),
        title,
        detail,
        tone,
      },
      ...current.slice(0, 4),
    ]);
  }

  function handleAction(action: FmfAction) {
    const delivered = sendToBridge(action, { origin: 'react-ui', action });
    setLastAction(action);
    setMode(detectBridge());

    pushEvent(
      delivered ? `Sent ${action}` : `Queued ${action}`,
      delivered
        ? 'FMF wrapper received the command channel request.'
        : 'No host bridge was found, so the UI stayed in preview mode.',
      delivered ? 'success' : 'warning',
    );
  }

  return (
    <main className="app-shell">
      <div className="bg-orb bg-orb-one" />
      <div className="bg-orb bg-orb-two" />

      <section className="hero-card">
        <header className="hero-header">
          <div>
            <p className="eyebrow">Frika Mod Framework</p>
            <h1>Modern React UI with C# bridge support</h1>
            <p className="subtitle">
              This template is wired for FMF-controlled UI replacement and keeps the action labels that the C# wrapper
              can resolve directly.
            </p>
          </div>

          <div className={`status-chip ${mode === 'wrapper-connected' ? 'status-chip--connected' : ''}`}>
            {wrapperState}
          </div>
        </header>

        <div className="stats-grid">
          <article className="stat-card">
            <span>Bridge mode</span>
            <strong>{mode === 'wrapper-connected' ? 'FMF host attached' : 'Browser preview'}</strong>
          </article>
          <article className="stat-card">
            <span>Last action</span>
            <strong>{lastAction}</strong>
          </article>
          <article className="stat-card">
            <span>Known wrapper labels</span>
            <strong>{supportedLabels.length}</strong>
          </article>
        </div>

        <div className="content-grid">
          <section className="panel-card panel-card--primary">
            <div className="panel-card__heading">
              <p className="panel-label">FMF bridge</p>
              <h2>Game actions</h2>
            </div>

            <div className="action-grid">
              {actionCards.map((card) => (
                <button
                  key={card.action}
                  type="button"
                  className={`action-button action-button--${card.action === 'exit' ? 'danger' : card.action === 'sync-state' ? 'ghost' : 'primary'}`}
                  data-action={card.action}
                  onClick={() => handleAction(card.action)}
                >
                  <span className="action-button__label">{card.label}</span>
                  <span className="action-button__copy">{card.description}</span>
                </button>
              ))}
            </div>
          </section>

          <aside className="panel-card side-panel">
            <div className="panel-card__heading">
              <p className="panel-label">Wrapper contract</p>
              <h2>Communication path</h2>
            </div>

            <ul className="bridge-list">
              <li>
                <strong>Primary channel:</strong> `window.fmfBridge.invoke(action, payload)`
              </li>
              <li>
                <strong>Fallbacks:</strong> `emit`, `postMessage`, then `window.parent.postMessage(...)`
              </li>
              <li>
                <strong>C# label match:</strong> button captions stay aligned with FMF action lookup
              </li>
              <li>
                <strong>Payload shape:</strong> `origin`, `action`, and `timestamp` for simple wrapper routing
              </li>
            </ul>

            <div className="mini-status">
              <span>Delivered to wrapper</span>
              <strong>{mode === 'wrapper-connected' ? 'Yes' : 'Not yet'}</strong>
            </div>
          </aside>
        </div>

        <section className="log-card">
          <div className="panel-card__heading">
            <p className="panel-label">Telemetry</p>
            <h2>Recent bridge activity</h2>
          </div>

          <div className="log-list">
            {eventLog.map((entry) => (
              <article key={entry.id} className={`log-entry log-entry--${entry.tone}`}>
                <div>
                  <h3>{entry.title}</h3>
                  <p>{entry.detail}</p>
                </div>
              </article>
            ))}
          </div>
        </section>
      </section>
    </main>
  );
}

const root = document.getElementById('root');

if (root) {
  createRoot(root).render(
    <React.StrictMode>
      <App />
    </React.StrictMode>,
  );
}
