import { Card, Col, Row } from 'react-bootstrap'

import { API_BASE_URL } from '../../api/http'
import { useRuntimeFlags } from '../../hooks/useRuntimeFlags'

const statusCards = [
  { label: 'Routes', value: '5', note: 'Dashboard, inventory, launches, catalog, settings' },
  { label: 'PWA', value: 'On', note: 'Manifest and generated service worker configured' },
  { label: 'Forms', value: 'Ready', note: 'React Hook Form installed for upcoming field workflows' },
]

export function DashboardPage() {
  const flags = useRuntimeFlags()

  return (
    <section className="d-grid gap-4 gap-lg-5">
      <Card className="hero-panel border-0">
        <Card.Body className="p-4 p-lg-5">
          <p className="phase-chip mb-3">Phase 0</p>
          <Row className="g-4 align-items-center">
            <Col lg={8}>
              <h1 className="display-5 fw-semibold mb-3">Frontend foundation is in place.</h1>
              <p className="lead text-body-secondary mb-4">
                This shell keeps the app buildable and PWA-ready without leaking into Phase 1 auth flows or the later launch logging experience.
              </p>
              <div className="d-flex flex-wrap gap-3 text-body-secondary">
                <span>API base: {API_BASE_URL}</span>
                <span>Auth enabled: {flags.authEnabled ? 'yes' : 'no'}</span>
              </div>
            </Col>
            <Col lg={4}>
              <Card className="status-card">
                <Card.Body>
                  <Card.Title className="mb-3">Current focus</Card.Title>
                  <ul className="placeholder-list">
                    <li>Monorepo frontend scaffold under `frontend/`</li>
                    <li>Router, Bootstrap UI baseline, and PWA registration</li>
                    <li>Documented source folders ready for feature work</li>
                  </ul>
                </Card.Body>
              </Card>
            </Col>
          </Row>
        </Card.Body>
      </Card>

      <Row className="g-4">
        {statusCards.map((card) => (
          <Col key={card.label} md={4}>
            <Card className="status-card">
              <Card.Body>
                <Card.Subtitle className="text-uppercase text-body-secondary small mb-3">
                  {card.label}
                </Card.Subtitle>
                <div className="display-6 fw-semibold mb-2">{card.value}</div>
                <Card.Text className="mb-0 text-body-secondary">{card.note}</Card.Text>
              </Card.Body>
            </Card>
          </Col>
        ))}
      </Row>
    </section>
  )
}