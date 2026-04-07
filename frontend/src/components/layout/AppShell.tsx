import { Container, Nav, Navbar } from 'react-bootstrap'
import { NavLink, Outlet } from 'react-router-dom'

import { useAuth } from '../../context/AuthContext'
import { getAppRoutes } from '../../utils/appRoutes'

export function AppShell() {
  const { user, signOut } = useAuth()
  const isAdmin = (user?.roles ?? []).some((role) => role.toLowerCase() === 'admin')
  const appRoutes = getAppRoutes(isAdmin)

  return (
    <div className="app-shell d-flex flex-column">
      <header className="app-header">
        <Navbar expand="md">
          <Container className="py-2">
            <Navbar.Brand as={NavLink} to="/" className="d-flex align-items-center gap-3 fw-semibold">
              <span className="brand-badge">RL</span>
              <span>
                <span className="d-block">Rocket Log</span>
                <small className="text-body-secondary fw-normal">Inventory and launch tracking</small>
              </span>
            </Navbar.Brand>
            <Navbar.Toggle aria-controls="rocket-log-nav" className="d-md-none" />
            <Navbar.Collapse id="rocket-log-nav" className="justify-content-end d-md-none">
              <Nav className="gap-md-2 pt-2">
                {appRoutes.map((route) => (
                  <Nav.Link
                    key={route.path}
                    as={NavLink}
                    to={route.path}
                    end={route.path === '/'}
                    className="rounded-pill px-3"
                  >
                    {route.label}
                  </Nav.Link>
                ))}
                <Nav.Link onClick={signOut} className="rounded-pill px-3">
                  Sign out
                </Nav.Link>
              </Nav>
            </Navbar.Collapse>

            <div className="d-none d-md-flex align-items-center gap-3">
              <Nav className="desktop-nav gap-2">
                {appRoutes.map((route) => (
                  <Nav.Link
                    key={route.path}
                    as={NavLink}
                    to={route.path}
                    end={route.path === '/'}
                    className="rounded-pill px-3"
                  >
                    {route.label}
                  </Nav.Link>
                ))}
              </Nav>
              <div className="user-pill">
                <span>{user?.displayName ?? 'Dev User'}</span>
                <button type="button" className="link-button" onClick={signOut}>
                  Sign out
                </button>
              </div>
            </div>
          </Container>
        </Navbar>
      </header>

      <main className="flex-grow-1 py-4 py-md-5">
        <Container>
          <Outlet />
        </Container>
      </main>

      <footer className="pb-4">
        <Container className="footer-note">
          Dev auth bypass is active in development mode.
        </Container>
      </footer>

      <nav className="mobile-bottom-nav d-md-none" aria-label="Primary navigation">
        {appRoutes.map((route) => (
          <NavLink key={route.path} to={route.path} end={route.path === '/'} className="mobile-bottom-link">
            {route.label}
          </NavLink>
        ))}
      </nav>
    </div>
  )
}