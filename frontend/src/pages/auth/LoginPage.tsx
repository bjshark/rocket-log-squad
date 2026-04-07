import { Alert, Button, Card, Col, Form, Row } from 'react-bootstrap'
import { Link, useLocation, useNavigate } from 'react-router-dom'

import { useAuth } from '../../context/AuthContext'

export function LoginPage() {
  const { authEnabled, signInDev, error, isLoading } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()

  const fromPath = (location.state as { from?: { pathname?: string } } | null)?.from?.pathname ?? '/'

  async function handleDevContinue() {
    await signInDev()
    navigate(fromPath, { replace: true })
  }

  return (
    <Row className="justify-content-center py-4 py-md-5">
      <Col md={8} lg={6}>
        <Card className="auth-card border-0 shadow-sm">
          <Card.Body className="p-4 p-md-5">
            <h1 className="h3 mb-3">Welcome Back</h1>
            <p className="text-body-secondary mb-4">
              Phase 1 uses a development bypass identity so you can use the app without full auth infrastructure.
            </p>

            {!authEnabled && (
              <Alert variant="info" className="mb-4">
                Development mode is active. Press continue to use the fixed dev identity.
              </Alert>
            )}

            {authEnabled && (
              <Alert variant="warning" className="mb-4">
                Production auth endpoint wiring is planned next. Toggle development auth bypass to continue now.
              </Alert>
            )}

            {error && <Alert variant="danger">{error}</Alert>}

            <Form className="d-grid gap-3" onSubmit={(event) => event.preventDefault()}>
              <Form.Group controlId="email">
                <Form.Label>Email</Form.Label>
                <Form.Control type="email" placeholder="name@example.com" disabled />
              </Form.Group>

              <Form.Group controlId="password">
                <Form.Label>Password</Form.Label>
                <Form.Control type="password" placeholder="Password" disabled />
              </Form.Group>

              <Button variant="warning" onClick={handleDevContinue} disabled={isLoading}>
                Continue With Dev Session
              </Button>
            </Form>

            <p className="mt-4 mb-0 text-body-secondary">
              Need an account? <Link to="/register">Create one</Link>
            </p>
          </Card.Body>
        </Card>
      </Col>
    </Row>
  )
}
