import { useCallback, useEffect, useMemo, useState } from 'react'
import type { FmfAction, FmfMessage, RuntimeMode, UiSyncState } from '../types/bridge'

const detectMode = (): RuntimeMode => (window.fmfBridge ? 'wrapper-connected' : 'standalone-preview')

export const useFmfBridge = () => {
  const [mode, setMode] = useState<RuntimeMode>(() => detectMode())
  const [lastSyncState, setLastSyncState] = useState<UiSyncState['payload'] | null>(null)

  useEffect(() => {
    const updateMode = () => setMode(detectMode())
    updateMode()

    const timer = window.setInterval(updateMode, 2000)
    return () => window.clearInterval(timer)
  }, [])

  useEffect(() => {
    const onMessage = (event: MessageEvent<unknown>) => {
      const data = event.data
      if (!data || typeof data !== 'object') {
        return
      }

      const maybeSync = data as Partial<UiSyncState>
      if (maybeSync.type !== 'STATE_UPDATE' || !maybeSync.payload) {
        return
      }

      setLastSyncState(maybeSync.payload)
    }

    window.addEventListener('message', onMessage)
    return () => window.removeEventListener('message', onMessage)
  }, [])

  const invokeAction = useCallback((action: FmfAction, payload?: Record<string, unknown>): boolean => {
    const message: FmfMessage = {
      action,
      payload,
      timestamp: new Date().toISOString(),
      source: 'fmf-react-ui',
    }

    const bridge = window.fmfBridge
    if (bridge?.invoke) {
      bridge.invoke(action, payload)
      return true
    }

    if (bridge?.emit) {
      bridge.emit(action, payload)
      return true
    }

    if (bridge?.postMessage) {
      bridge.postMessage(message)
      return true
    }

    if (window.parent && window.parent !== window) {
      window.parent.postMessage(message, '*')
      return true
    }

    return false
  }, [])

  const wrapperStateLabel = useMemo(
    () => (mode === 'wrapper-connected' ? 'Connected to FMF wrapper bridge' : 'Standalone preview mode'),
    [mode],
  )

  return {
    mode,
    wrapperStateLabel,
    lastSyncState,
    invokeAction,
  }
}
