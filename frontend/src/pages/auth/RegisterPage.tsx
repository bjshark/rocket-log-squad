import { Alert, Button, Card, Col, Form, Row } from 'react-bootstrap'
import { Link, useNavigate } from 'react-router-dom'

import { useAuth } from '../../context/AuthContext'

export function RegisterPage() {
  const { authEnabled, signInDev, isLoading } = useAuth()
  const navigate = useNavigate()

  async function handleDevRegister() {
    await signInDev()
    navigate('/', { replace: true })
  }

  return (
    <Row className="justify-content-center py-4 py-md-5">
      <Col md={8} lg={6}>
        <Card className="auth-card border-0 shadow-sm">
          <Card.Body className="p-4 p-md-5">
            <h1 className="h3 mb-3">Create Account</h1>
            <p className="text-body-secondary mb-4">
              Registration is stubbed in Phase 1 while development bypass is active.
            </p>

            {!authEnabled && (
              <Alert variant="info" className="mb-4">
                Continue to provision the fixed dev identity and enter the app.
              </Alert>
            )}

            <Form className="d-grid gap-3" onSubmit={(event) => event.preventDefault()}>
              <Form.Group controlId="displayName">
                <Form.Label>Display Name</Form.Label>
                <Form.Control type="text" placeholder="Rocket flyer" disabled />
              </Form.Group>

              <Form.Group controlId="email">
                <Form.Label>Email</Form.Label>
                <Form.Control type="email" placeholder="name@example.com" disabled />
              </Form.Group>

              <Form.Group controlId="password">
                <Form.Label>Password</Form.Label>
                <Form.Control type="password" placeholder="Create password" disabled />
              </Form.Group>

              <Button variant="warning" onClick={handleDevRegister} disabled={isLoading}>
                Continue With Dev Session
              </Button>
            </Form>

            <p className="mt-4 mb-0 text-body-secondary">
              Already have an account? <Link to="/login">Sign in</Link>
            </p>
          </Card.Body>
        </Card>
      </Col>
    </Row>
  )
}
