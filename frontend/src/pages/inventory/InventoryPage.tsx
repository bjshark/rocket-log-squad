import { useEffect, useMemo, useState } from 'react'
import { useForm } from 'react-hook-form'
import { Alert, Badge, Button, Card, Col, Form, Row, Spinner } from 'react-bootstrap'

import {
  createAccessory,
  deleteAccessory,
  deleteMyEngine,
  deleteMyRocket,
  fetchAccessories,
  fetchMyEngines,
  fetchMyRockets,
  updateAccessory,
  updateMyEngine,
  updateMyRocket,
  uploadMyRocketPhoto,
} from '../../api/inventory'
import type {
  AccessoryItem,
  AccessoryMutationRequest,
  UpdateUserEngineRequest,
  UpdateUserRocketRequest,
  UserEngineInventoryItem,
  UserRocketInventoryItem,
} from '../../types/inventory'

type AccessoryFormValues = {
  name: string
  category: string
  brand: string
  notes: string
  photoUrl: string
}

type RocketDraft = {
  nickname: string
  buildDate: string
  condition: 'New' | 'Good' | 'Fair' | 'Retired'
  buildNotes: string
  photoUrl: string
}

type EngineDraft = {
  quantityOnHand: number
  purchaseDate: string
  notes: string
}

type AccessoryDraft = {
  name: string
  category: string
  brand: string
  notes: string
  photoUrl: string
}

const rocketConditions: Array<RocketDraft['condition']> = ['New', 'Good', 'Fair', 'Retired']

function toDateInputValue(value: string | null) {
  if (!value) {
    return ''
  }

  return value.split('T')[0] ?? ''
}

function toNullable(value: string) {
  const trimmed = value.trim()
  return trimmed.length > 0 ? trimmed : null
}

function toRocketDraft(item: UserRocketInventoryItem): RocketDraft {
  return {
    nickname: item.nickname ?? '',
    buildDate: toDateInputValue(item.buildDate),
    condition: item.condition,
    buildNotes: item.buildNotes ?? '',
    photoUrl: item.photoUrl ?? '',
  }
}

function toEngineDraft(item: UserEngineInventoryItem): EngineDraft {
  return {
    quantityOnHand: item.quantityOnHand,
    purchaseDate: toDateInputValue(item.purchaseDate),
    notes: item.notes ?? '',
  }
}

function toAccessoryDraft(item: AccessoryItem): AccessoryDraft {
  return {
    name: item.name,
    category: item.category,
    brand: item.brand ?? '',
    notes: item.notes ?? '',
    photoUrl: item.photoUrl ?? '',
  }
}

export function InventoryPage() {
  const [rocketItems, setRocketItems] = useState<UserRocketInventoryItem[]>([])
  const [engineItems, setEngineItems] = useState<UserEngineInventoryItem[]>([])
  const [accessoryItems, setAccessoryItems] = useState<AccessoryItem[]>([])

  const [rocketDrafts, setRocketDrafts] = useState<Record<string, RocketDraft>>({})
  const [engineDrafts, setEngineDrafts] = useState<Record<string, EngineDraft>>({})
  const [accessoryDrafts, setAccessoryDrafts] = useState<Record<string, AccessoryDraft>>({})

  const [busyKey, setBusyKey] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<AccessoryFormValues>({
    defaultValues: {
      name: '',
      category: '',
      brand: '',
      notes: '',
      photoUrl: '',
    },
  })

  const totalInventoryItems = useMemo(
    () => rocketItems.length + engineItems.length + accessoryItems.length,
    [accessoryItems.length, engineItems.length, rocketItems.length],
  )

  async function loadInventory() {
    setIsLoading(true)
    setError(null)

    try {
      const [rockets, engines, accessories] = await Promise.all([
        fetchMyRockets(),
        fetchMyEngines(),
        fetchAccessories(),
      ])

      setRocketItems(rockets)
      setEngineItems(engines)
      setAccessoryItems(accessories)

      setRocketDrafts(
        Object.fromEntries(rockets.map((item) => [item.id, toRocketDraft(item)])),
      )
      setEngineDrafts(
        Object.fromEntries(engines.map((item) => [item.id, toEngineDraft(item)])),
      )
      setAccessoryDrafts(
        Object.fromEntries(accessories.map((item) => [item.id, toAccessoryDraft(item)])),
      )
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Failed to load inventory.')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void loadInventory()
  }, [])

  async function handleSaveRocket(id: string) {
    const draft = rocketDrafts[id]
    if (!draft) {
      return
    }

    const request: UpdateUserRocketRequest = {
      nickname: toNullable(draft.nickname),
      buildDate: draft.buildDate || null,
      condition: draft.condition,
      buildNotes: toNullable(draft.buildNotes),
      photoUrl: toNullable(draft.photoUrl),
    }

    setBusyKey(`rocket-save-${id}`)
    setError(null)
    setSuccess(null)

    try {
      const updated = await updateMyRocket(id, request)
      setRocketItems((previous) => previous.map((item) => (item.id === id ? updated : item)))
      setRocketDrafts((previous) => ({
        ...previous,
        [id]: toRocketDraft(updated),
      }))
      setSuccess('Rocket inventory updated.')
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to update rocket.')
    } finally {
      setBusyKey(null)
    }
  }

  async function handleRocketPhotoUpload(id: string, file: File) {
    setBusyKey(`rocket-photo-${id}`)
    setError(null)
    setSuccess(null)

    try {
      const response = await uploadMyRocketPhoto(id, file)
      setRocketDrafts((previous) => {
        const current = previous[id]
        if (!current) {
          return previous
        }

        return {
          ...previous,
          [id]: {
            ...current,
            photoUrl: response.photoUrl ?? '',
          },
        }
      })

      setRocketItems((previous) =>
        previous.map((item) =>
          item.id === id
            ? {
                ...item,
                photoUrl: response.photoUrl,
              }
            : item,
        ),
      )

      setSuccess('Rocket photo uploaded.')
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to upload photo.')
    } finally {
      setBusyKey(null)
    }
  }

  async function handleDeleteRocket(id: string) {
    setBusyKey(`rocket-delete-${id}`)
    setError(null)
    setSuccess(null)

    try {
      await deleteMyRocket(id)
      setRocketItems((previous) => previous.filter((item) => item.id !== id))
      setSuccess('Rocket removed from inventory.')
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to delete rocket.')
    } finally {
      setBusyKey(null)
    }
  }

  async function handleSaveEngine(id: string) {
    const draft = engineDrafts[id]
    if (!draft) {
      return
    }

    const request: UpdateUserEngineRequest = {
      quantityOnHand: Math.max(0, Number.isNaN(draft.quantityOnHand) ? 0 : draft.quantityOnHand),
      purchaseDate: draft.purchaseDate || null,
      notes: toNullable(draft.notes),
    }

    setBusyKey(`engine-save-${id}`)
    setError(null)
    setSuccess(null)

    try {
      const updated = await updateMyEngine(id, request)
      setEngineItems((previous) => previous.map((item) => (item.id === id ? updated : item)))
      setEngineDrafts((previous) => ({
        ...previous,
        [id]: toEngineDraft(updated),
      }))
      setSuccess('Engine inventory updated.')
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to update engine.')
    } finally {
      setBusyKey(null)
    }
  }

  async function handleDeleteEngine(id: string) {
    setBusyKey(`engine-delete-${id}`)
    setError(null)
    setSuccess(null)

    try {
      await deleteMyEngine(id)
      setEngineItems((previous) => previous.filter((item) => item.id !== id))
      setSuccess('Engine removed from inventory.')
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to delete engine.')
    } finally {
      setBusyKey(null)
    }
  }

  async function handleCreateAccessory(values: AccessoryFormValues) {
    const request: AccessoryMutationRequest = {
      name: values.name.trim(),
      category: values.category.trim(),
      brand: toNullable(values.brand),
      notes: toNullable(values.notes),
      photoUrl: toNullable(values.photoUrl),
    }

    setError(null)
    setSuccess(null)

    try {
      const created = await createAccessory(request)
      setAccessoryItems((previous) => [created, ...previous])
      setAccessoryDrafts((previous) => ({
        ...previous,
        [created.id]: toAccessoryDraft(created),
      }))
      reset()
      setSuccess('Accessory added.')
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to create accessory.')
    }
  }

  async function handleSaveAccessory(id: string) {
    const draft = accessoryDrafts[id]
    if (!draft) {
      return
    }

    const request: AccessoryMutationRequest = {
      name: draft.name.trim(),
      category: draft.category.trim(),
      brand: toNullable(draft.brand),
      notes: toNullable(draft.notes),
      photoUrl: toNullable(draft.photoUrl),
    }

    setBusyKey(`accessory-save-${id}`)
    setError(null)
    setSuccess(null)

    try {
      const updated = await updateAccessory(id, request)
      setAccessoryItems((previous) => previous.map((item) => (item.id === id ? updated : item)))
      setAccessoryDrafts((previous) => ({
        ...previous,
        [id]: toAccessoryDraft(updated),
      }))
      setSuccess('Accessory updated.')
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to update accessory.')
    } finally {
      setBusyKey(null)
    }
  }

  async function handleDeleteAccessory(id: string) {
    setBusyKey(`accessory-delete-${id}`)
    setError(null)
    setSuccess(null)

    try {
      await deleteAccessory(id)
      setAccessoryItems((previous) => previous.filter((item) => item.id !== id))
      setSuccess('Accessory removed.')
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to delete accessory.')
    } finally {
      setBusyKey(null)
    }
  }

  return (
    <section className="d-grid gap-4">
      <header>
        <p className="phase-chip mb-3">Phase 3 · User inventory</p>
        <h1 className="page-heading h2 mb-2">Manage rockets, engines, and accessories</h1>
        <p className="text-body-secondary mb-0">
          Add items from the catalog, then update inventory details, stock counts, and accessory notes here.
        </p>
      </header>

      <Card className="route-card">
        <Card.Body className="d-flex align-items-center justify-content-between flex-wrap gap-3">
          <div>
            <strong>Total tracked items:</strong> {totalInventoryItems}
          </div>
          <Button variant="outline-secondary" size="sm" onClick={() => void loadInventory()} disabled={isLoading}>
            Refresh
          </Button>
        </Card.Body>
      </Card>

      {error && <Alert variant="danger">{error}</Alert>}
      {success && <Alert variant="success">{success}</Alert>}

      {isLoading && (
        <div className="d-flex align-items-center gap-2 text-body-secondary">
          <Spinner animation="border" size="sm" role="status" />
          Loading inventory data...
        </div>
      )}

      <Row className="g-4">
        <Col lg={6}>
          <Card className="route-card h-100">
            <Card.Body className="d-grid gap-3">
              <Card.Title className="d-flex justify-content-between align-items-center">
                <span>My rockets</span>
                <Badge bg="warning" text="dark">
                  {rocketItems.length}
                </Badge>
              </Card.Title>

              {rocketItems.length === 0 && !isLoading && (
                <p className="mb-0 text-body-secondary">No rockets in your inventory yet. Add one from the catalog page.</p>
              )}

              {rocketItems.map((item) => {
                const draft = rocketDrafts[item.id]
                if (!draft) {
                  return null
                }

                return (
                  <Card key={item.id} className="border-0 bg-light-subtle">
                    <Card.Body className="d-grid gap-2">
                      <h3 className="h6 mb-0">
                        {item.name ?? 'Unknown rocket'}
                        <small className="text-body-secondary ms-2">{item.manufacturer ?? 'Catalog missing'}</small>
                      </h3>

                      <Row className="g-2">
                        <Col sm={6}>
                          <Form.Label className="small mb-1">Nickname</Form.Label>
                          <Form.Control
                            size="sm"
                            value={draft.nickname}
                            onChange={(event) =>
                              setRocketDrafts((previous) => ({
                                ...previous,
                                [item.id]: { ...draft, nickname: event.target.value },
                              }))
                            }
                          />
                        </Col>

                        <Col sm={6}>
                          <Form.Label className="small mb-1">Build date</Form.Label>
                          <Form.Control
                            size="sm"
                            type="date"
                            value={draft.buildDate}
                            onChange={(event) =>
                              setRocketDrafts((previous) => ({
                                ...previous,
                                [item.id]: { ...draft, buildDate: event.target.value },
                              }))
                            }
                          />
                        </Col>
                      </Row>

                      <Form.Label className="small mb-1">Condition</Form.Label>
                      <Form.Select
                        size="sm"
                        value={draft.condition}
                        onChange={(event) =>
                          setRocketDrafts((previous) => ({
                            ...previous,
                            [item.id]: {
                              ...draft,
                              condition: event.target.value as RocketDraft['condition'],
                            },
                          }))
                        }
                      >
                        {rocketConditions.map((condition) => (
                          <option key={condition} value={condition}>
                            {condition}
                          </option>
                        ))}
                      </Form.Select>

                      <Form.Label className="small mb-1">Build notes</Form.Label>
                      <Form.Control
                        size="sm"
                        as="textarea"
                        rows={2}
                        value={draft.buildNotes}
                        onChange={(event) =>
                          setRocketDrafts((previous) => ({
                            ...previous,
                            [item.id]: { ...draft, buildNotes: event.target.value },
                          }))
                        }
                      />

                      <Form.Label className="small mb-1">Photo URL</Form.Label>
                      <Form.Control
                        size="sm"
                        value={draft.photoUrl}
                        onChange={(event) =>
                          setRocketDrafts((previous) => ({
                            ...previous,
                            [item.id]: { ...draft, photoUrl: event.target.value },
                          }))
                        }
                        placeholder="https://..."
                      />

                      <Form.Group>
                        <Form.Label className="small mb-1">Upload build photo</Form.Label>
                        <Form.Control
                          size="sm"
                          type="file"
                          accept="image/*"
                          disabled={busyKey === `rocket-photo-${item.id}`}
                          onChange={(event) => {
                            const input = event.currentTarget as HTMLInputElement
                            const file = input.files?.[0]
                            if (file) {
                              void handleRocketPhotoUpload(item.id, file)
                              input.value = ''
                            }
                          }}
                        />
                      </Form.Group>

                      <div className="d-flex gap-2 mt-1">
                        <Button
                          size="sm"
                          variant="outline-primary"
                          disabled={busyKey === `rocket-save-${item.id}`}
                          onClick={() => void handleSaveRocket(item.id)}
                        >
                          Save
                        </Button>
                        <Button
                          size="sm"
                          variant="outline-danger"
                          disabled={busyKey === `rocket-delete-${item.id}`}
                          onClick={() => void handleDeleteRocket(item.id)}
                        >
                          Remove
                        </Button>
                      </div>
                    </Card.Body>
                  </Card>
                )
              })}
            </Card.Body>
          </Card>
        </Col>

        <Col lg={6}>
          <Card className="route-card h-100">
            <Card.Body className="d-grid gap-3">
              <Card.Title className="d-flex justify-content-between align-items-center">
                <span>My engines</span>
                <Badge bg="warning" text="dark">
                  {engineItems.length}
                </Badge>
              </Card.Title>

              {engineItems.length === 0 && !isLoading && (
                <p className="mb-0 text-body-secondary">No engines in your inventory yet. Add one from the catalog page.</p>
              )}

              {engineItems.map((item) => {
                const draft = engineDrafts[item.id]
                if (!draft) {
                  return null
                }

                return (
                  <Card key={item.id} className="border-0 bg-light-subtle">
                    <Card.Body className="d-grid gap-2">
                      <h3 className="h6 mb-0">
                        {item.designation ?? 'Unknown engine'}
                        <small className="text-body-secondary ms-2">{item.manufacturer ?? 'Catalog missing'}</small>
                      </h3>

                      <Form.Label className="small mb-1">Quantity on hand</Form.Label>
                      <div className="d-flex gap-2 align-items-center">
                        <Button
                          size="sm"
                          variant="outline-secondary"
                          onClick={() =>
                            setEngineDrafts((previous) => ({
                              ...previous,
                              [item.id]: {
                                ...draft,
                                quantityOnHand: Math.max(0, draft.quantityOnHand - 1),
                              },
                            }))
                          }
                        >
                          -
                        </Button>
                        <Form.Control
                          size="sm"
                          type="number"
                          min={0}
                          value={draft.quantityOnHand}
                          onChange={(event) =>
                            setEngineDrafts((previous) => ({
                              ...previous,
                              [item.id]: {
                                ...draft,
                                quantityOnHand: Math.max(0, Number(event.target.value)),
                              },
                            }))
                          }
                        />
                        <Button
                          size="sm"
                          variant="outline-secondary"
                          onClick={() =>
                            setEngineDrafts((previous) => ({
                              ...previous,
                              [item.id]: {
                                ...draft,
                                quantityOnHand: draft.quantityOnHand + 1,
                              },
                            }))
                          }
                        >
                          +
                        </Button>
                      </div>

                      <Row className="g-2">
                        <Col sm={6}>
                          <Form.Label className="small mb-1">Purchase date</Form.Label>
                          <Form.Control
                            size="sm"
                            type="date"
                            value={draft.purchaseDate}
                            onChange={(event) =>
                              setEngineDrafts((previous) => ({
                                ...previous,
                                [item.id]: { ...draft, purchaseDate: event.target.value },
                              }))
                            }
                          />
                        </Col>

                        <Col sm={6}>
                          <Form.Label className="small mb-1">Impulse class</Form.Label>
                          <Form.Control size="sm" value={item.impulseClass ?? ''} disabled />
                        </Col>
                      </Row>

                      <Form.Label className="small mb-1">Notes</Form.Label>
                      <Form.Control
                        size="sm"
                        as="textarea"
                        rows={2}
                        value={draft.notes}
                        onChange={(event) =>
                          setEngineDrafts((previous) => ({
                            ...previous,
                            [item.id]: { ...draft, notes: event.target.value },
                          }))
                        }
                      />

                      <div className="d-flex gap-2 mt-1">
                        <Button
                          size="sm"
                          variant="outline-primary"
                          disabled={busyKey === `engine-save-${item.id}`}
                          onClick={() => void handleSaveEngine(item.id)}
                        >
                          Save
                        </Button>
                        <Button
                          size="sm"
                          variant="outline-danger"
                          disabled={busyKey === `engine-delete-${item.id}`}
                          onClick={() => void handleDeleteEngine(item.id)}
                        >
                          Remove
                        </Button>
                      </div>
                    </Card.Body>
                  </Card>
                )
              })}
            </Card.Body>
          </Card>
        </Col>
      </Row>

      <Card className="route-card">
        <Card.Body className="d-grid gap-3">
          <Card.Title className="d-flex justify-content-between align-items-center">
            <span>Accessories</span>
            <Badge bg="warning" text="dark">
              {accessoryItems.length}
            </Badge>
          </Card.Title>

          <Form className="d-grid gap-2" onSubmit={void handleSubmit(handleCreateAccessory)}>
            <Row className="g-2">
              <Col md={4}>
                <Form.Label className="small mb-1">Name</Form.Label>
                <Form.Control
                  size="sm"
                  {...register('name', { required: 'Name is required.' })}
                  isInvalid={Boolean(errors.name)}
                />
                <Form.Control.Feedback type="invalid">{errors.name?.message}</Form.Control.Feedback>
              </Col>

              <Col md={3}>
                <Form.Label className="small mb-1">Category</Form.Label>
                <Form.Control
                  size="sm"
                  {...register('category', { required: 'Category is required.' })}
                  isInvalid={Boolean(errors.category)}
                />
                <Form.Control.Feedback type="invalid">{errors.category?.message}</Form.Control.Feedback>
              </Col>

              <Col md={3}>
                <Form.Label className="small mb-1">Brand</Form.Label>
                <Form.Control size="sm" {...register('brand')} />
              </Col>

              <Col md={2} className="d-flex align-items-end">
                <Button size="sm" type="submit" disabled={isSubmitting}>
                  Add accessory
                </Button>
              </Col>
            </Row>

            <Row className="g-2">
              <Col md={6}>
                <Form.Label className="small mb-1">Photo URL</Form.Label>
                <Form.Control size="sm" {...register('photoUrl')} />
              </Col>
              <Col md={6}>
                <Form.Label className="small mb-1">Notes</Form.Label>
                <Form.Control size="sm" as="textarea" rows={2} {...register('notes')} />
              </Col>
            </Row>
          </Form>

          {accessoryItems.length === 0 && !isLoading && (
            <p className="mb-0 text-body-secondary">No accessories in your inventory yet. Add your first item above.</p>
          )}

          {accessoryItems.map((item) => {
            const draft = accessoryDrafts[item.id]
            if (!draft) {
              return null
            }

            return (
              <Card key={item.id} className="border-0 bg-light-subtle">
                <Card.Body className="d-grid gap-2">
                  <Row className="g-2">
                    <Col md={4}>
                      <Form.Label className="small mb-1">Name</Form.Label>
                      <Form.Control
                        size="sm"
                        value={draft.name}
                        onChange={(event) =>
                          setAccessoryDrafts((previous) => ({
                            ...previous,
                            [item.id]: { ...draft, name: event.target.value },
                          }))
                        }
                      />
                    </Col>

                    <Col md={3}>
                      <Form.Label className="small mb-1">Category</Form.Label>
                      <Form.Control
                        size="sm"
                        value={draft.category}
                        onChange={(event) =>
                          setAccessoryDrafts((previous) => ({
                            ...previous,
                            [item.id]: { ...draft, category: event.target.value },
                          }))
                        }
                      />
                    </Col>

                    <Col md={3}>
                      <Form.Label className="small mb-1">Brand</Form.Label>
                      <Form.Control
                        size="sm"
                        value={draft.brand}
                        onChange={(event) =>
                          setAccessoryDrafts((previous) => ({
                            ...previous,
                            [item.id]: { ...draft, brand: event.target.value },
                          }))
                        }
                      />
                    </Col>

                    <Col md={2}>
                      <Form.Label className="small mb-1">Photo URL</Form.Label>
                      <Form.Control
                        size="sm"
                        value={draft.photoUrl}
                        onChange={(event) =>
                          setAccessoryDrafts((previous) => ({
                            ...previous,
                            [item.id]: { ...draft, photoUrl: event.target.value },
                          }))
                        }
                      />
                    </Col>
                  </Row>

                  <Form.Label className="small mb-1">Notes</Form.Label>
                  <Form.Control
                    size="sm"
                    as="textarea"
                    rows={2}
                    value={draft.notes}
                    onChange={(event) =>
                      setAccessoryDrafts((previous) => ({
                        ...previous,
                        [item.id]: { ...draft, notes: event.target.value },
                      }))
                    }
                  />

                  <div className="d-flex gap-2 mt-1">
                    <Button
                      size="sm"
                      variant="outline-primary"
                      disabled={busyKey === `accessory-save-${item.id}`}
                      onClick={() => void handleSaveAccessory(item.id)}
                    >
                      Save
                    </Button>
                    <Button
                      size="sm"
                      variant="outline-danger"
                      disabled={busyKey === `accessory-delete-${item.id}`}
                      onClick={() => void handleDeleteAccessory(item.id)}
                    >
                      Remove
                    </Button>
                  </div>
                </Card.Body>
              </Card>
            )
          })}
        </Card.Body>
      </Card>
    </section>
  )
}