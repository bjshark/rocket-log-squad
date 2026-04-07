import { useEffect, useMemo, useState } from 'react'
import { Alert, Badge, Button, Card, Col, Form, Row, Spinner } from 'react-bootstrap'

import {
  addEngineToInventory,
  addRocketToInventory,
  fetchCatalogFilterOptions,
  fetchEngines,
  fetchRockets,
} from '../../api/catalog'
import type { EngineCatalogItem, RocketCatalogItem } from '../../types/catalog'

type ItemActionStatus = {
  state: 'idle' | 'loading' | 'success' | 'error'
  message?: string
}

export function CatalogPage() {
  const [query, setQuery] = useState('')
  const [manufacturer, setManufacturer] = useState('')
  const [impulseClass, setImpulseClass] = useState('')
  const [page, setPage] = useState(1)

  const [rocketItems, setRocketItems] = useState<RocketCatalogItem[]>([])
  const [engineItems, setEngineItems] = useState<EngineCatalogItem[]>([])
  const [totalRockets, setTotalRockets] = useState(0)
  const [totalEngines, setTotalEngines] = useState(0)
  const [manufacturerOptions, setManufacturerOptions] = useState<string[]>([])
  const [impulseClassOptions, setImpulseClassOptions] = useState<string[]>([])
  const [rocketActions, setRocketActions] = useState<Record<string, ItemActionStatus>>({})
  const [engineActions, setEngineActions] = useState<Record<string, ItemActionStatus>>({})
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  const pageSize = 8

  const totalPages = useMemo(() => {
    return Math.max(1, Math.ceil(Math.max(totalRockets, totalEngines) / pageSize))
  }, [pageSize, totalEngines, totalRockets])

  useEffect(() => {
    let isCancelled = false

    async function loadFilterOptions() {
      try {
        const options = await fetchCatalogFilterOptions()

        if (isCancelled) {
          return
        }

        setManufacturerOptions(options.manufacturers)
        setImpulseClassOptions(options.impulseClasses)
      } catch {
        if (!isCancelled) {
          // Keep page functional even if filter options are temporarily unavailable.
          setManufacturerOptions([])
          setImpulseClassOptions([])
        }
      }
    }

    void loadFilterOptions()

    return () => {
      isCancelled = true
    }
  }, [])

  useEffect(() => {
    let isCancelled = false

    async function loadCatalog() {
      setIsLoading(true)
      setError(null)

      try {
        const [rockets, engines] = await Promise.all([
          fetchRockets({ query, manufacturer, page, pageSize }),
          fetchEngines({ query, manufacturer, impulseClass, page, pageSize }),
        ])

        if (isCancelled) {
          return
        }

        setRocketItems(rockets.items)
        setEngineItems(engines.items)
        setTotalRockets(rockets.total)
        setTotalEngines(engines.total)
      } catch (caughtError) {
        if (!isCancelled) {
          setError(caughtError instanceof Error ? caughtError.message : 'Unable to load catalog data.')
        }
      } finally {
        if (!isCancelled) {
          setIsLoading(false)
        }
      }
    }

    void loadCatalog()

    return () => {
      isCancelled = true
    }
  }, [impulseClass, manufacturer, page, query])

  function resetFilters() {
    setQuery('')
    setManufacturer('')
    setImpulseClass('')
    setPage(1)
  }

  async function handleAddRocketToInventory(rocketId: string) {
    setRocketActions((previous) => ({
      ...previous,
      [rocketId]: { state: 'loading' },
    }))

    try {
      const response = await addRocketToInventory(rocketId)
      setRocketActions((previous) => ({
        ...previous,
        [rocketId]: {
          state: 'success',
          message: response.added ? 'Added to your rockets.' : 'Already in your rockets.',
        },
      }))
    } catch (caughtError) {
      setRocketActions((previous) => ({
        ...previous,
        [rocketId]: {
          state: 'error',
          message: caughtError instanceof Error ? caughtError.message : 'Unable to add rocket.',
        },
      }))
    }
  }

  async function handleAddEngineToInventory(engineId: string) {
    setEngineActions((previous) => ({
      ...previous,
      [engineId]: { state: 'loading' },
    }))

    try {
      const response = await addEngineToInventory(engineId)
      const message = response.addedNewEntry
        ? 'Added to your engines (qty 1).'
        : `Quantity updated to ${response.quantityOnHand}.`

      setEngineActions((previous) => ({
        ...previous,
        [engineId]: {
          state: 'success',
          message,
        },
      }))
    } catch (caughtError) {
      setEngineActions((previous) => ({
        ...previous,
        [engineId]: {
          state: 'error',
          message: caughtError instanceof Error ? caughtError.message : 'Unable to add engine.',
        },
      }))
    }
  }

  return (
    <section className="d-grid gap-4">
      <header>
        <p className="phase-chip mb-3">Master catalog</p>
        <h1 className="page-heading h2 mb-2">Browse and search rockets and engines</h1>
        <p className="text-body-secondary mb-0">
          Search the master catalog and add rockets or engines to your inventory in one step.
        </p>
      </header>

      <Card className="route-card">
        <Card.Body>
          <Row className="g-3 align-items-end">
            <Col md={4}>
              <Form.Label htmlFor="catalog-query">Search</Form.Label>
              <Form.Control
                id="catalog-query"
                value={query}
                onChange={(event) => {
                  setQuery(event.target.value)
                  setPage(1)
                }}
                placeholder="Rocket name, SKU, engine designation"
              />
            </Col>

            <Col md={3}>
              <Form.Label htmlFor="catalog-manufacturer">Manufacturer</Form.Label>
              <Form.Select
                id="catalog-manufacturer"
                value={manufacturer}
                onChange={(event) => {
                  setManufacturer(event.target.value)
                  setPage(1)
                }}
              >
                <option value="">All manufacturers</option>
                {manufacturerOptions.map((option) => (
                  <option key={option} value={option}>
                    {option}
                  </option>
                ))}
              </Form.Select>
            </Col>

            <Col md={2}>
              <Form.Label htmlFor="catalog-impulse">Impulse</Form.Label>
              <Form.Select
                id="catalog-impulse"
                value={impulseClass}
                onChange={(event) => {
                  setImpulseClass(event.target.value)
                  setPage(1)
                }}
              >
                <option value="">All classes</option>
                {impulseClassOptions.map((option) => (
                  <option key={option} value={option}>
                    {option}
                  </option>
                ))}
              </Form.Select>
            </Col>

            <Col md={3} className="d-flex gap-2">
              <Button variant="outline-secondary" onClick={resetFilters}>
                Reset
              </Button>
            </Col>
          </Row>
        </Card.Body>
      </Card>

      {error && <Alert variant="danger">{error}</Alert>}

      {isLoading && (
        <div className="d-flex align-items-center gap-2 text-body-secondary">
          <Spinner animation="border" size="sm" role="status" />
          Loading catalog...
        </div>
      )}

      <Row className="g-4">
        <Col md={6}>
          <Card className="route-card">
            <Card.Body>
              <Card.Title className="d-flex justify-content-between align-items-center">
                <span>Rocket catalog</span>
                <Badge bg="warning" text="dark">
                  {totalRockets}
                </Badge>
              </Card.Title>

              <div className="d-grid gap-3">
                {rocketItems.map((rocket) => (
                  <div key={rocket.id} className="catalog-item">
                    <h3 className="h6 mb-1">
                      {rocket.name} <small className="text-body-secondary">({rocket.sku})</small>
                    </h3>
                    <p className="mb-1 text-body-secondary">
                      {rocket.manufacturer} · {rocket.skillLevel}
                    </p>
                    <p className="mb-0 small text-body-secondary">{rocket.description}</p>
                    <div className="d-flex align-items-center gap-2 mt-2">
                      <Button
                        size="sm"
                        variant="outline-primary"
                        disabled={rocketActions[rocket.id]?.state === 'loading'}
                        onClick={() => {
                          void handleAddRocketToInventory(rocket.id)
                        }}
                      >
                        {rocketActions[rocket.id]?.state === 'loading' ? 'Adding...' : 'Add to my rockets'}
                      </Button>
                      {rocketActions[rocket.id]?.state === 'success' && (
                        <small className="text-success">{rocketActions[rocket.id]?.message}</small>
                      )}
                      {rocketActions[rocket.id]?.state === 'error' && (
                        <small className="text-danger">{rocketActions[rocket.id]?.message}</small>
                      )}
                    </div>
                  </div>
                ))}

                {!isLoading && rocketItems.length === 0 && (
                  <p className="mb-0 text-body-secondary">No rockets found for the current filters.</p>
                )}
              </div>
            </Card.Body>
          </Card>
        </Col>

        <Col md={6}>
          <Card className="route-card">
            <Card.Body>
              <Card.Title className="d-flex justify-content-between align-items-center">
                <span>Engine catalog</span>
                <Badge bg="warning" text="dark">
                  {totalEngines}
                </Badge>
              </Card.Title>

              <div className="d-grid gap-3">
                {engineItems.map((engine) => (
                  <div key={engine.id} className="catalog-item">
                    <h3 className="h6 mb-1">
                      {engine.designation} <small className="text-body-secondary">({engine.impulseClass})</small>
                    </h3>
                    <p className="mb-1 text-body-secondary">
                      {engine.manufacturer} · {engine.propellantType}
                    </p>
                    <p className="mb-0 small text-body-secondary">
                      Avg thrust {engine.averageThrustN}N · Delay {engine.delayS}s
                    </p>
                    <div className="d-flex align-items-center gap-2 mt-2">
                      <Button
                        size="sm"
                        variant="outline-primary"
                        disabled={engineActions[engine.id]?.state === 'loading'}
                        onClick={() => {
                          void handleAddEngineToInventory(engine.id)
                        }}
                      >
                        {engineActions[engine.id]?.state === 'loading' ? 'Adding...' : 'Add to my engines'}
                      </Button>
                      {engineActions[engine.id]?.state === 'success' && (
                        <small className="text-success">{engineActions[engine.id]?.message}</small>
                      )}
                      {engineActions[engine.id]?.state === 'error' && (
                        <small className="text-danger">{engineActions[engine.id]?.message}</small>
                      )}
                    </div>
                  </div>
                ))}

                {!isLoading && engineItems.length === 0 && (
                  <p className="mb-0 text-body-secondary">No engines found for the current filters.</p>
                )}
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      <div className="d-flex justify-content-between align-items-center">
        <small className="text-body-secondary">
          Page {page} of {totalPages}
        </small>
        <div className="d-flex gap-2">
          <Button variant="outline-secondary" size="sm" disabled={page <= 1} onClick={() => setPage((prev) => prev - 1)}>
            Previous
          </Button>
          <Button
            variant="outline-secondary"
            size="sm"
            disabled={page >= totalPages}
            onClick={() => setPage((prev) => prev + 1)}
          >
            Next
          </Button>
        </div>
      </div>
    </section>
  )
}