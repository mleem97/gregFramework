export type FmfAction = 'continue' | 'new-game' | 'multiplayer' | 'settings' | 'exit' | 'sync-state'

export type FmfMessage = {
  action: FmfAction
  payload?: Record<string, unknown>
  timestamp: string
  source: 'fmf-react-ui'
}

export type BridgeTransport = {
  invoke?: (action: string, payload?: Record<string, unknown>) => void
  emit?: (action: string, payload?: Record<string, unknown>) => void
  postMessage?: (message: FmfMessage) => void
}

export type RuntimeMode = 'wrapper-connected' | 'standalone-preview'

export type UiSyncState = {
  type: 'STATE_UPDATE'
  payload: {
    sceneName: string
    maxPlayers: number
    uiReplacementEnabled: boolean
    autoRefreshEnabled: boolean
    autoRefreshIntervalSeconds: number
    liveReloadEnabled: boolean
    bridgeReady: boolean
    timestamp: string
  }
}

declare global {
  interface Window {
    fmfBridge?: BridgeTransport
  }
}
