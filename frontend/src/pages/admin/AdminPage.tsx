import { useEffect, useMemo, useState } from 'react'
import { Alert, Badge, Button, Card, Col, Form, Row, Spinner, Table } from 'react-bootstrap'

import {
  createAdminEngine,
  createAdminRocket,
  deleteAdminEngine,
  deleteAdminRocket,
  exportAdminSeeds,
  fetchAdminEngineById,
  fetchAdminEngines,
  fetchAdminRocketById,
  fetchAdminRockets,
  updateAdminEngine,
  updateAdminRocket,
  uploadAdminImage,
} from '../../api/admin'
import { useAuth } from '../../context/AuthContext'
import type {
  AdminEngineItem,
  AdminEngineMutationInput,
  AdminRocketItem,
  AdminRocketMutationInput,
} from '../../types/admin'

const defaultRocketDraft: AdminRocketMutationInput = {
  manufacturer: 'Estes',
  sku: '0000',
  name: 'New Rocket',
  description: 'Describe this catalog rocket.',
  skillLevel: 'Beginner',
  recommendedEngines: ['A8-3'],
  diameterMm: 24,
  lengthMm: 300,
  weightG: 50,
  finMaterial: 'Balsa',
  noseCone: 'Plastic',
  recoverySystem: 'Parachute',
  thumbnailUrl: 'https://example.com/catalog/rockets/new-rocket-thumb.jpg',
  imageUrls: ['https://example.com/catalog/rockets/new-rocket.jpg'],
  productUrl: null,
  isActive: true,
}

const defaultEngineDraft: AdminEngineMutationInput = {
  manufacturer: 'Estes',
  designation: 'A8-3',
  impulseClass: 'A',
  totalImpulseNs: 2.5,
  averageThrustN: 8,
  delayS: 3,
  diameterMm: 18,
  lengthMm: 70,
  propellantWeightG: 1.6,
  totalWeightG: 16.2,
  caseType: 'Single-use',
  propellantType: 'Black powder',
  thumbnailUrl: 'https://example.com/catalog/engines/new-engine-thumb.jpg',
  imageUrls: ['https://example.com/catalog/engines/new-engine.jpg'],
  certificationBody: 'NAR',
  isActive: true,
}

function toEditableRocketPayload(item: AdminRocketItem): AdminRocketMutationInput {
  return {
    manufacturer: item.manufacturer,
    sku: item.sku,
    name: item.name,
    description: item.description,
    skillLevel: item.skillLevel,
    recommendedEngines: [...item.recommendedEngines],
    diameterMm: item.diameterMm,
    lengthMm: item.lengthMm,
    weightG: item.weightG,
    finMaterial: item.finMaterial,
    noseCone: item.noseCone,
    recoverySystem: item.recoverySystem,
    thumbnailUrl: item.thumbnailUrl,
    imageUrls: [...item.imageUrls],
    productUrl: item.productUrl,
    isActive: item.isActive,
  }
}

function toEditableEnginePayload(item: AdminEngineItem): AdminEngineMutationInput {
  return {
    manufacturer: item.manufacturer,
    designation: item.designation,
    impulseClass: item.impulseClass,
    totalImpulseNs: item.totalImpulseNs,
    averageThrustN: item.averageThrustN,
    delayS: item.delayS,
    diameterMm: item.diameterMm,
    lengthMm: item.lengthMm,
    propellantWeightG: item.propellantWeightG,
    totalWeightG: item.totalWeightG,
    caseType: item.caseType,
    propellantType: item.propellantType,
    thumbnailUrl: item.thumbnailUrl,
    imageUrls: [...item.imageUrls],
    certificationBody: item.certificationBody,
    isActive: item.isActive,
  }
}

export function AdminPage() {
  const { user } = useAuth()
  const [rockets, setRockets] = useState<AdminRocketItem[]>([])
  const [engines, setEngines] = useState<AdminEngineItem[]>([])

  const [isLoading, setIsLoading] = useState(false)
  const [isMutating, setIsMutating] = useState(false)

  const [selectedRocketId, setSelectedRocketId] = useState<string | null>(null)
  const [selectedEngineId, setSelectedEngineId] = useState<string | null>(null)

  const [rocketDraftText, setRocketDraftText] = useState(() => JSON.stringify(defaultRocketDraft, null, 2))
  const [engineDraftText, setEngineDraftText] = useState(() => JSON.stringify(defaultEngineDraft, null, 2))

  const [uploadScope, setUploadScope] = useState('catalog-rockets')
  const [uploadFile, setUploadFile] = useState<File | null>(null)

  const [status, setStatus] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  const isAdmin = useMemo(() => {
    return (user?.roles ?? []).some((role) => role.toLowerCase() === 'admin')
  }, [user?.roles])

  async function loadAdminData() {
    setIsLoading(true)
    setError(null)

    try {
      const [rocketPage, enginePage] = await Promise.all([fetchAdminRockets(1, 100), fetchAdminEngines(1, 100)])
      setRockets(rocketPage.items)
      setEngines(enginePage.items)
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to load admin catalog data.')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    if (!isAdmin) {
      return
    }

    void loadAdminData()
  }, [isAdmin])

  function parseRocketDraft(): AdminRocketMutationInput {
    return JSON.parse(rocketDraftText) as AdminRocketMutationInput
  }

  function parseEngineDraft(): AdminEngineMutationInput {
    return JSON.parse(engineDraftText) as AdminEngineMutationInput
  }

  async function withMutation(action: () => Promise<void>) {
    setIsMutating(true)
    setStatus(null)
    setError(null)

    try {
      await action()
      await loadAdminData()
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Admin operation failed.')
    } finally {
      setIsMutating(false)
    }
  }

  async function handleCreateRocket() {
    await withMutation(async () => {
      const payload = parseRocketDraft()
      const created = await createAdminRocket(payload)
      setSelectedRocketId(created.id)
      setStatus(`Created rocket ${created.name} (${created.sku}).`)
    })
  }

  async function handleUpdateRocket() {
    if (!selectedRocketId) {
      setError('Select a rocket before updating.')
      return
    }

    await withMutation(async () => {
      const payload = parseRocketDraft()
      const updated = await updateAdminRocket(selectedRocketId, payload)
      setStatus(`Updated rocket ${updated.name}.`)
    })
  }

  async function handleDeleteRocket(id: string) {
    await withMutation(async () => {
      await deleteAdminRocket(id)
      if (selectedRocketId === id) {
        setSelectedRocketId(null)
      }
      setStatus('Rocket was deactivated from the master catalog.')
    })
  }

  async function handleLoadRocketIntoEditor(id: string) {
    setError(null)
    setStatus(null)

    try {
      const item = await fetchAdminRocketById(id)
      setSelectedRocketId(id)
      setRocketDraftText(JSON.stringify(toEditableRocketPayload(item), null, 2))
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to load rocket payload.')
    }
  }

  async function handleCreateEngine() {
    await withMutation(async () => {
      const payload = parseEngineDraft()
      const created = await createAdminEngine(payload)
      setSelectedEngineId(created.id)
      setStatus(`Created engine ${created.designation}.`)
    })
  }

  async function handleUpdateEngine() {
    if (!selectedEngineId) {
      setError('Select an engine before updating.')
      return
    }

    await withMutation(async () => {
      const payload = parseEngineDraft()
      const updated = await updateAdminEngine(selectedEngineId, payload)
      setStatus(`Updated engine ${updated.designation}.`)
    })
  }

  async function handleDeleteEngine(id: string) {
    await withMutation(async () => {
      await deleteAdminEngine(id)
      if (selectedEngineId === id) {
        setSelectedEngineId(null)
      }
      setStatus('Engine was deactivated from the master catalog.')
    })
  }

  async function handleLoadEngineIntoEditor(id: string) {
    setError(null)
    setStatus(null)

    try {
      const item = await fetchAdminEngineById(id)
      setSelectedEngineId(id)
      setEngineDraftText(JSON.stringify(toEditableEnginePayload(item), null, 2))
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to load engine payload.')
    }
  }

  async function handleUploadImage() {
    if (!uploadFile) {
      setError('Choose an image file first.')
      return
    }

    await withMutation(async () => {
      const response = await uploadAdminImage(uploadFile, uploadScope)
      setStatus(`Image uploaded: ${response.imageUrl}`)
    })
  }

  async function handleExportSeeds() {
    setError(null)
    setStatus(null)

    try {
      const payload = await exportAdminSeeds()
      const content = JSON.stringify(payload, null, 2)
      const blob = new Blob([content], { type: 'application/json' })
      const url = URL.createObjectURL(blob)

      const link = document.createElement('a')
      link.href = url
      link.download = `rocket-log-seed-export-${new Date().toISOString().replace(/[:.]/g, '-')}.json`
      link.click()

      URL.revokeObjectURL(url)
      setStatus('Seed export downloaded as JSON.')
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to export seed JSON.')
    }
  }

  if (!isAdmin) {
    return (
      <section className="d-grid gap-3">
        <p className="phase-chip mb-0">Phase 6</p>
        <h1 className="page-heading h2 mb-0">Admin Panel</h1>
        <Alert variant="warning" className="mb-0">
          Admin access is required to manage the master catalog.
        </Alert>
      </section>
    )
  }

  return (
    <section className="d-grid gap-4">
      <header>
        <p className="phase-chip mb-3">Phase 6</p>
        <h1 className="page-heading h2 mb-2">Admin and data management</h1>
        <p className="text-body-secondary mb-0">
          Manage master rockets and engines, upload catalog images, and export seed JSON from current database state.
        </p>
      </header>

      {error && <Alert variant="danger">{error}</Alert>}
      {status && <Alert variant="success">{status}</Alert>}

      <Row className="g-4">
        <Col lg={6}>
          <Card className="route-card h-100">
            <Card.Body className="d-grid gap-3">
              <div className="d-flex justify-content-between align-items-center">
                <Card.Title className="mb-0">Master rockets</Card.Title>
                <Badge bg="warning" text="dark">
                  {rockets.length}
                </Badge>
              </div>

              <div className="table-responsive">
                <Table size="sm" hover>
                  <thead>
                    <tr>
                      <th>Name</th>
                      <th>SKU</th>
                      <th>State</th>
                      <th className="text-end">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {rockets.map((rocket) => (
                      <tr key={rocket.id} className={selectedRocketId === rocket.id ? 'table-warning' : undefined}>
                        <td>{rocket.name}</td>
                        <td>{rocket.sku}</td>
                        <td>{rocket.isActive ? 'Active' : 'Inactive'}</td>
                        <td className="text-end d-flex justify-content-end gap-2">
                          <Button
                            size="sm"
                            variant="outline-secondary"
                            onClick={() => {
                              void handleLoadRocketIntoEditor(rocket.id)
                            }}
                          >
                            Edit
                          </Button>
                          <Button
                            size="sm"
                            variant="outline-danger"
                            onClick={() => {
                              void handleDeleteRocket(rocket.id)
                            }}
                          >
                            Delete
                          </Button>
                        </td>
                      </tr>
                    ))}
                    {rockets.length === 0 && !isLoading && (
                      <tr>
                        <td colSpan={4} className="text-body-secondary">
                          No rockets found in the master catalog.
                        </td>
                      </tr>
                    )}
                  </tbody>
                </Table>
              </div>

              <Form.Group>
                <Form.Label>Rocket JSON payload</Form.Label>
                <Form.Control
                  as="textarea"
                  rows={14}
                  value={rocketDraftText}
                  onChange={(event) => setRocketDraftText(event.target.value)}
                />
              </Form.Group>

              <div className="d-flex gap-2 flex-wrap">
                <Button disabled={isMutating} onClick={() => void handleCreateRocket()}>
                  Create rocket
                </Button>
                <Button variant="outline-primary" disabled={isMutating} onClick={() => void handleUpdateRocket()}>
                  Update selected rocket
                </Button>
              </div>
            </Card.Body>
          </Card>
        </Col>

        <Col lg={6}>
          <Card className="route-card h-100">
            <Card.Body className="d-grid gap-3">
              <div className="d-flex justify-content-between align-items-center">
                <Card.Title className="mb-0">Master engines</Card.Title>
                <Badge bg="warning" text="dark">
                  {engines.length}
                </Badge>
              </div>

              <div className="table-responsive">
                <Table size="sm" hover>
                  <thead>
                    <tr>
                      <th>Designation</th>
                      <th>Class</th>
                      <th>State</th>
                      <th className="text-end">Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {engines.map((engine) => (
                      <tr key={engine.id} className={selectedEngineId === engine.id ? 'table-warning' : undefined}>
                        <td>{engine.designation}</td>
                        <td>{engine.impulseClass}</td>
                        <td>{engine.isActive ? 'Active' : 'Inactive'}</td>
                        <td className="text-end d-flex justify-content-end gap-2">
                          <Button
                            size="sm"
                            variant="outline-secondary"
                            onClick={() => {
                              void handleLoadEngineIntoEditor(engine.id)
                            }}
                          >
                            Edit
                          </Button>
                          <Button
                            size="sm"
                            variant="outline-danger"
                            onClick={() => {
                              void handleDeleteEngine(engine.id)
                            }}
                          >
                            Delete
                          </Button>
                        </td>
                      </tr>
                    ))}
                    {engines.length === 0 && !isLoading && (
                      <tr>
                        <td colSpan={4} className="text-body-secondary">
                          No engines found in the master catalog.
                        </td>
                      </tr>
                    )}
                  </tbody>
                </Table>
              </div>

              <Form.Group>
                <Form.Label>Engine JSON payload</Form.Label>
                <Form.Control
                  as="textarea"
                  rows={14}
                  value={engineDraftText}
                  onChange={(event) => setEngineDraftText(event.target.value)}
                />
              </Form.Group>

              <div className="d-flex gap-2 flex-wrap">
                <Button disabled={isMutating} onClick={() => void handleCreateEngine()}>
                  Create engine
                </Button>
                <Button variant="outline-primary" disabled={isMutating} onClick={() => void handleUpdateEngine()}>
                  Update selected engine
                </Button>
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      <Card className="route-card">
        <Card.Body>
          <Card.Title>Catalog image upload and seed export</Card.Title>
          <Row className="g-3 align-items-end">
            <Col md={3}>
              <Form.Label htmlFor="upload-scope">Upload scope</Form.Label>
              <Form.Select id="upload-scope" value={uploadScope} onChange={(event) => setUploadScope(event.target.value)}>
                <option value="catalog-rockets">catalog-rockets</option>
                <option value="catalog-engines">catalog-engines</option>
                <option value="catalog">catalog</option>
              </Form.Select>
            </Col>
            <Col md={5}>
              <Form.Label htmlFor="upload-file">Image file</Form.Label>
              <Form.Control
                id="upload-file"
                type="file"
                accept="image/*"
                onChange={(event) => {
                  const input = event.currentTarget as HTMLInputElement
                  setUploadFile(input.files?.[0] ?? null)
                }}
              />
            </Col>
            <Col md={4} className="d-flex gap-2">
              <Button variant="outline-primary" disabled={isMutating} onClick={() => void handleUploadImage()}>
                Upload image
              </Button>
              <Button variant="outline-secondary" onClick={() => void handleExportSeeds()}>
                Export seed JSON
              </Button>
            </Col>
          </Row>
        </Card.Body>
      </Card>

      {isLoading && (
        <div className="d-flex align-items-center gap-2 text-body-secondary">
          <Spinner animation="border" size="sm" role="status" />
          Loading admin data...
        </div>
      )}
    </section>
  )
}
