export default function App() {
  return (
    <div className="app-shell">
      <div className="aurora" />
      <div className="app-card">
        <h1>Frika Modern Control Center</h1>
        <p>
          Fully modernized React-style experience for Data Center. This skin is exported to FMF DC2WebBridge and
          applied across menu, HR, and shop surfaces.
        </p>
        <div className="actions">
          <button type="button" className="btn-primary" style={{ border: '1px solid transparent' }}>
            Open Operations
          </button>
          <button type="button" className="btn-secondary" style={{ border: '1px solid transparent' }}>
            View Diagnostics
          </button>
        </div>
      </div>
    </div>
  )
}
