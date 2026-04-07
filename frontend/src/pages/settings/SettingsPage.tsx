import { Card } from 'react-bootstrap'

export function SettingsPage() {
  return (
    <section className="d-grid gap-4">
      <header>
        <p className="phase-chip mb-3">Settings placeholder</p>
        <h1 className="page-heading h2 mb-2">Settings stays intentionally thin in Phase 0.</h1>
        <p className="text-body-secondary mb-0">
          User preferences, unit selection, and admin-only controls belong to later phases once auth and profile data exist.
        </p>
      </header>

      <Card className="route-card">
        <Card.Body>
          <Card.Title>Deferred items</Card.Title>
          <Card.Text as="div" className="mb-0">
            <ul className="placeholder-list">
              <li>Unit preferences and default launch site</li>
              <li>Admin catalog management controls</li>
              <li>Auth-driven profile management</li>
            </ul>
          </Card.Text>
        </Card.Body>
      </Card>
    </section>
  )
}