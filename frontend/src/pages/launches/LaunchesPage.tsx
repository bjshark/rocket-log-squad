import { useEffect, useMemo, useState } from 'react'
import { Alert, Badge, Button, Card, Col, Form, Row, Spinner } from 'react-bootstrap'
import { useForm } from 'react-hook-form'

import {
  createLaunch,
  deleteLaunch,
  fetchLaunchById,
  fetchMyLaunches,
  fetchWeatherSnapshot,
  updateLaunch,
  uploadLaunchPhoto,
} from '../../api/launches'
import { fetchMyEngines, fetchMyRockets } from '../../api/inventory'
import type { UserEngineInventoryItem, UserRocketInventoryItem } from '../../types/inventory'
import type { LaunchDetail, LaunchListItem, LaunchMutationRequest, LaunchOutcome } from '../../types/launches'

type CreateLaunchFormValues = {
  userRocketId: string
  engineId: string
  launchDate: string
  locationName: string
  lat: string
  lng: string
  weatherSource: 'api' | 'manual'
  temperatureF: string
  windSpeedMph: string
  windDirection: string
  humidity: string
  conditions: string
  visibilityMi: string
  outcome: LaunchOutcome
  altitudeFt: string
  notes: string
  photoUrl: string
}

type LaunchEditDraft = {
  userRocketId: string
  engineId: string
  launchDate: string
  locationName: string
  lat: string
  lng: string
  weatherSource: 'api' | 'manual'
  temperatureF: string
  windSpeedMph: string
  windDirection: string
  humidity: string
  conditions: string
  visibilityMi: string
  outcome: LaunchOutcome
  altitudeFt: string
  notes: string
  photoUrl: string
}

const outcomes: LaunchOutcome[] = ['Success', 'Partial', 'Failure', 'No Launch']

function formatDateInput(isoDate: string) {
  const asDate = new Date(isoDate)
  if (Number.isNaN(asDate.getTime())) {
    return ''
  }

  const tzOffsetMs = asDate.getTimezoneOffset() * 60000
  return new Date(asDate.getTime() - tzOffsetMs).toISOString().slice(0, 16)
}

function toNullableString(value: string) {
  const trimmed = value.trim()
  return trimmed.length > 0 ? trimmed : null
}

function toRequiredNumber(value: string, fallback: number) {
  const parsed = Number.parseFloat(value)
  return Number.isFinite(parsed) ? parsed : fallback
}

function toNullableNumber(value: string) {
  const trimmed = value.trim()
  if (!trimmed) {
    return null
  }

  const parsed = Number.parseFloat(trimmed)
  return Number.isFinite(parsed) ? parsed : null
}

function toMutationRequest(values: LaunchEditDraft | CreateLaunchFormValues): LaunchMutationRequest {
  return {
    userRocketId: values.userRocketId,
    engineId: values.engineId,
    launchDate: new Date(values.launchDate).toISOString(),
    location: {
      name: toNullableString(values.locationName),
      lat: toRequiredNumber(values.lat, 0),
      lng: toRequiredNumber(values.lng, 0),
    },
    weather: {
      source: values.weatherSource,
      temperatureF: toRequiredNumber(values.temperatureF, 0),
      windSpeedMph: toRequiredNumber(values.windSpeedMph, 0),
      windDirection: values.windDirection.trim(),
      humidity: toRequiredNumber(values.humidity, 0),
      conditions: values.conditions.trim(),
      visibilityMi: toNullableNumber(values.visibilityMi),
    },
    outcome: values.outcome,
    altitudeFt: toNullableNumber(values.altitudeFt),
    notes: toNullableString(values.notes),
    photoUrl: toNullableString(values.photoUrl),
  }
}

function toDraft(launch: LaunchDetail): LaunchEditDraft {
  return {
    userRocketId: launch.userRocketId,
    engineId: launch.engineId,
    launchDate: formatDateInput(launch.launchDate),
    locationName: launch.location.name ?? '',
    lat: String(launch.location.lat),
    lng: String(launch.location.lng),
    weatherSource: launch.weather.source,
    temperatureF: String(launch.weather.temperatureF),
    windSpeedMph: String(launch.weather.windSpeedMph),
    windDirection: launch.weather.windDirection,
    humidity: String(launch.weather.humidity),
    conditions: launch.weather.conditions,
    visibilityMi: launch.weather.visibilityMi === null ? '' : String(launch.weather.visibilityMi),
    outcome: launch.outcome,
    altitudeFt: launch.altitudeFt === null ? '' : String(launch.altitudeFt),
    notes: launch.notes ?? '',
    photoUrl: launch.photoUrl ?? '',
  }
}

export function LaunchesPage() {
  const [rockets, setRockets] = useState<UserRocketInventoryItem[]>([])
  const [engines, setEngines] = useState<UserEngineInventoryItem[]>([])
  const [launches, setLaunches] = useState<LaunchListItem[]>([])
  const [selectedLaunchId, setSelectedLaunchId] = useState<string | null>(null)
  const [editDraft, setEditDraft] = useState<LaunchEditDraft | null>(null)

  const [isLoading, setIsLoading] = useState(true)
  const [isLoadingDetail, setIsLoadingDetail] = useState(false)
  const [busyKey, setBusyKey] = useState<string | null>(null)
  const [isOnline, setIsOnline] = useState(() => navigator.onLine)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  const {
    register,
    setValue,
    getValues,
    reset,
    handleSubmit,
    formState: { isSubmitting },
  } = useForm<CreateLaunchFormValues>({
    defaultValues: {
      userRocketId: '',
      engineId: '',
      launchDate: formatDateInput(new Date().toISOString()),
      locationName: '',
      lat: '',
      lng: '',
      weatherSource: 'api',
      temperatureF: '70',
      windSpeedMph: '5',
      windDirection: 'N',
      humidity: '50',
      conditions: 'Clear',
      visibilityMi: '10',
      outcome: 'Success',
      altitudeFt: '',
      notes: '',
      photoUrl: '',
    },
  })

  const totalLaunches = useMemo(() => launches.length, [launches.length])

  async function loadLaunchesAndOptions() {
    setIsLoading(true)
    setError(null)

    try {
      const [rocketItems, engineItems, launchPage] = await Promise.all([
        fetchMyRockets(),
        fetchMyEngines(),
        fetchMyLaunches(1, 50),
      ])

      setRockets(rocketItems)
      setEngines(engineItems)
      setLaunches(launchPage.items)

      if (rocketItems.length > 0) {
        setValue('userRocketId', rocketItems[0].id)
      }

      if (engineItems.length > 0) {
        setValue('engineId', engineItems[0].engineId)
      }

      if (launchPage.items.length > 0) {
        setSelectedLaunchId(launchPage.items[0].id)
      } else {
        setSelectedLaunchId(null)
        setEditDraft(null)
      }
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to load launch data.')
    } finally {
      setIsLoading(false)
    }
  }

  useEffect(() => {
    void loadLaunchesAndOptions()
  }, [])

  useEffect(() => {
    const handleOnline = () => setIsOnline(true)
    const handleOffline = () => setIsOnline(false)

    window.addEventListener('online', handleOnline)
    window.addEventListener('offline', handleOffline)

    return () => {
      window.removeEventListener('online', handleOnline)
      window.removeEventListener('offline', handleOffline)
    }
  }, [])

  useEffect(() => {
    if (!selectedLaunchId) {
      setEditDraft(null)
      return
    }

    const launchId = selectedLaunchId

    let cancelled = false

    async function loadDetail() {
      setIsLoadingDetail(true)

      try {
        const detail = await fetchLaunchById(launchId)
        if (cancelled) {
          return
        }

        setEditDraft(toDraft(detail))
      } catch (caughtError) {
        if (!cancelled) {
          setError(caughtError instanceof Error ? caughtError.message : 'Unable to load launch detail.')
        }
      } finally {
        if (!cancelled) {
          setIsLoadingDetail(false)
        }
      }
    }

    void loadDetail()

    return () => {
      cancelled = true
    }
  }, [selectedLaunchId])

  async function handleCaptureLocation() {
    setError(null)
    setSuccess(null)

    if (!navigator.geolocation) {
      setError('Geolocation is not supported in this browser.')
      return
    }

    setBusyKey('capture-location')

    navigator.geolocation.getCurrentPosition(
      (position) => {
        setValue('lat', position.coords.latitude.toFixed(6))
        setValue('lng', position.coords.longitude.toFixed(6))
        setSuccess('Location captured from your device.')
        setBusyKey(null)
      },
      () => {
        setError('Unable to capture location. Please allow location permission and try again.')
        setBusyKey(null)
      },
      {
        enableHighAccuracy: true,
        maximumAge: 0,
      },
    )
  }

  async function handleAutoFetchWeather() {
    setError(null)
    setSuccess(null)

    const lat = Number.parseFloat(getValues('lat'))
    const lng = Number.parseFloat(getValues('lng'))

    if (!Number.isFinite(lat) || !Number.isFinite(lng)) {
      setError('Valid latitude and longitude are required before weather fetch.')
      return
    }

    setBusyKey('fetch-weather')

    try {
      const snapshot = await fetchWeatherSnapshot(lat, lng)

      setValue('weatherSource', snapshot.source)
      setValue('temperatureF', String(snapshot.temperatureF))
      setValue('windSpeedMph', String(snapshot.windSpeedMph))
      setValue('windDirection', snapshot.windDirection)
      setValue('humidity', String(snapshot.humidity))
      setValue('conditions', snapshot.conditions)
      setValue('visibilityMi', snapshot.visibilityMi === null ? '' : String(snapshot.visibilityMi))

      if (snapshot.locationName) {
        setValue('locationName', snapshot.locationName)
      }

      setSuccess('Weather snapshot loaded.')
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to fetch weather snapshot.')
    } finally {
      setBusyKey(null)
    }
  }

  async function handleCreateLaunch(values: CreateLaunchFormValues) {
    setError(null)
    setSuccess(null)

    if (!isOnline) {
      setError('You are offline. New launch submissions are disabled until connectivity returns.')
      return
    }

    try {
      await createLaunch(toMutationRequest(values))
      reset({
        ...values,
        notes: '',
        altitudeFt: '',
      })
      await loadLaunchesAndOptions()
      setSuccess('Launch logged successfully.')
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to create launch.')
    }
  }

  async function handleSaveSelectedLaunch() {
    if (!selectedLaunchId || !editDraft) {
      return
    }

    setBusyKey('save-launch')
    setError(null)
    setSuccess(null)

    try {
      const updated = await updateLaunch(selectedLaunchId, toMutationRequest(editDraft))
      setEditDraft(toDraft(updated))

      const refreshed = await fetchMyLaunches(1, 50)
      setLaunches(refreshed.items)

      setSuccess('Launch updated.')
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to update launch.')
    } finally {
      setBusyKey(null)
    }
  }

  async function handleDeleteSelectedLaunch() {
    if (!selectedLaunchId) {
      return
    }

    setBusyKey('delete-launch')
    setError(null)
    setSuccess(null)

    try {
      await deleteLaunch(selectedLaunchId)

      const refreshed = await fetchMyLaunches(1, 50)
      setLaunches(refreshed.items)

      if (refreshed.items.length > 0) {
        setSelectedLaunchId(refreshed.items[0].id)
      } else {
        setSelectedLaunchId(null)
        setEditDraft(null)
      }

      setSuccess('Launch deleted.')
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to delete launch.')
    } finally {
      setBusyKey(null)
    }
  }

  async function handleUploadPhoto(file: File) {
    if (!selectedLaunchId) {
      return
    }

    setBusyKey('upload-photo')
    setError(null)
    setSuccess(null)

    try {
      const payload = await uploadLaunchPhoto(selectedLaunchId, file)
      setEditDraft((previous) => {
        if (!previous) {
          return previous
        }

        return {
          ...previous,
          photoUrl: payload.photoUrl ?? '',
        }
      })

      setSuccess('Launch photo uploaded.')
    } catch (caughtError) {
      setError(caughtError instanceof Error ? caughtError.message : 'Unable to upload launch photo.')
    } finally {
      setBusyKey(null)
    }
  }

  return (
    <section className="d-grid gap-4">
      <header>
        <p className="phase-chip mb-3">Phase 5 · PWA polish and offline UX</p>
        <h1 className="page-heading h2 mb-2">Log launches with weather and location snapshots</h1>
        <p className="text-body-secondary mb-0">
          Launch logging stays readable offline, and new launch writes are guarded until connectivity returns.
        </p>
      </header>

      <Card className="route-card">
        <Card.Body className="d-flex align-items-center justify-content-between flex-wrap gap-3">
          <div>
            <strong>Logged launches:</strong> {totalLaunches}
          </div>
          <Button variant="outline-secondary" size="sm" onClick={() => void loadLaunchesAndOptions()} disabled={isLoading}>
            Refresh
          </Button>
        </Card.Body>
      </Card>

      {error && <Alert variant="danger">{error}</Alert>}
      {success && <Alert variant="success">{success}</Alert>}

      {isLoading && (
        <div className="d-flex align-items-center gap-2 text-body-secondary">
          <Spinner animation="border" size="sm" role="status" />
          Loading launch data...
        </div>
      )}

      <Row className="g-4">
        <Col lg={7}>
          <Card className="route-card">
            <Card.Body className="d-grid gap-3">
              <Card.Title>New launch</Card.Title>

              {!isOnline && (
                <Alert variant="warning" className="mb-0">
                  You are currently offline. New launch submissions are disabled in v1 while offline. You can still view and edit
                  previously loaded launch data.
                </Alert>
              )}

              <Form className="d-grid gap-3" onSubmit={handleSubmit((values) => void handleCreateLaunch(values))}>
                <Row className="g-3">
                  <Col md={6}>
                    <Form.Label htmlFor="launch-rocket">Rocket</Form.Label>
                    <Form.Select id="launch-rocket" {...register('userRocketId')}>
                      {rockets.map((rocket) => (
                        <option key={rocket.id} value={rocket.id}>
                          {rocket.nickname || rocket.name || 'Unnamed rocket'}
                        </option>
                      ))}
                    </Form.Select>
                  </Col>

                  <Col md={6}>
                    <Form.Label htmlFor="launch-engine">Engine</Form.Label>
                    <Form.Select id="launch-engine" {...register('engineId')}>
                      {engines.map((engine) => (
                        <option key={engine.id} value={engine.engineId}>
                          {engine.designation || engine.engineId}
                        </option>
                      ))}
                    </Form.Select>
                  </Col>

                  <Col md={6}>
                    <Form.Label htmlFor="launch-date">Launch date and time</Form.Label>
                    <Form.Control id="launch-date" type="datetime-local" {...register('launchDate')} />
                  </Col>

                  <Col md={6}>
                    <Form.Label htmlFor="launch-outcome">Outcome</Form.Label>
                    <Form.Select id="launch-outcome" {...register('outcome')}>
                      {outcomes.map((outcome) => (
                        <option key={outcome} value={outcome}>
                          {outcome}
                        </option>
                      ))}
                    </Form.Select>
                  </Col>
                </Row>

                <Row className="g-3">
                  <Col md={12}>
                    <Form.Label htmlFor="launch-location-name">Location name</Form.Label>
                    <Form.Control id="launch-location-name" {...register('locationName')} placeholder="Club field, city park, etc." />
                  </Col>

                  <Col md={4}>
                    <Form.Label htmlFor="launch-lat">Latitude</Form.Label>
                    <Form.Control id="launch-lat" {...register('lat')} placeholder="39.7392" />
                  </Col>

                  <Col md={4}>
                    <Form.Label htmlFor="launch-lng">Longitude</Form.Label>
                    <Form.Control id="launch-lng" {...register('lng')} placeholder="-104.9903" />
                  </Col>

                  <Col md={4} className="d-flex align-items-end gap-2">
                    <Button
                      type="button"
                      variant="outline-secondary"
                      onClick={() => void handleCaptureLocation()}
                      disabled={busyKey === 'capture-location'}
                    >
                      {busyKey === 'capture-location' ? 'Capturing...' : 'Use GPS'}
                    </Button>
                    <Button
                      type="button"
                      variant="outline-secondary"
                      onClick={() => void handleAutoFetchWeather()}
                      disabled={busyKey === 'fetch-weather'}
                    >
                      {busyKey === 'fetch-weather' ? 'Loading weather...' : 'Fetch weather'}
                    </Button>
                  </Col>
                </Row>

                <Row className="g-3">
                  <Col md={3}>
                    <Form.Label htmlFor="launch-weather-source">Weather source</Form.Label>
                    <Form.Select id="launch-weather-source" {...register('weatherSource')}>
                      <option value="api">API</option>
                      <option value="manual">Manual</option>
                    </Form.Select>
                  </Col>

                  <Col md={3}>
                    <Form.Label htmlFor="launch-temp">Temp (F)</Form.Label>
                    <Form.Control id="launch-temp" {...register('temperatureF')} />
                  </Col>

                  <Col md={3}>
                    <Form.Label htmlFor="launch-wind-speed">Wind mph</Form.Label>
                    <Form.Control id="launch-wind-speed" {...register('windSpeedMph')} />
                  </Col>

                  <Col md={3}>
                    <Form.Label htmlFor="launch-wind-direction">Wind direction</Form.Label>
                    <Form.Control id="launch-wind-direction" {...register('windDirection')} />
                  </Col>

                  <Col md={4}>
                    <Form.Label htmlFor="launch-humidity">Humidity</Form.Label>
                    <Form.Control id="launch-humidity" {...register('humidity')} />
                  </Col>

                  <Col md={4}>
                    <Form.Label htmlFor="launch-conditions">Conditions</Form.Label>
                    <Form.Control id="launch-conditions" {...register('conditions')} />
                  </Col>

                  <Col md={4}>
                    <Form.Label htmlFor="launch-visibility">Visibility (mi)</Form.Label>
                    <Form.Control id="launch-visibility" {...register('visibilityMi')} />
                  </Col>
                </Row>

                <Row className="g-3">
                  <Col md={4}>
                    <Form.Label htmlFor="launch-altitude">Altitude (ft)</Form.Label>
                    <Form.Control id="launch-altitude" {...register('altitudeFt')} />
                  </Col>

                  <Col md={8}>
                    <Form.Label htmlFor="launch-photo-url">Photo URL (optional)</Form.Label>
                    <Form.Control id="launch-photo-url" {...register('photoUrl')} />
                  </Col>

                  <Col md={12}>
                    <Form.Label htmlFor="launch-notes">Notes</Form.Label>
                    <Form.Control id="launch-notes" as="textarea" rows={3} {...register('notes')} />
                  </Col>
                </Row>

                <div>
                  <Button type="submit" disabled={isSubmitting || !isOnline || rockets.length === 0 || engines.length === 0}>
                    {isSubmitting ? 'Saving launch...' : 'Save launch'}
                  </Button>
                </div>
              </Form>
            </Card.Body>
          </Card>
        </Col>

        <Col lg={5}>
          <Card className="route-card h-100">
            <Card.Body className="d-grid gap-3">
              <Card.Title className="d-flex justify-content-between align-items-center">
                <span>Launch log</span>
                <Badge bg="warning" text="dark">
                  {launches.length}
                </Badge>
              </Card.Title>

              <div className="d-grid gap-2">
                {launches.map((launch) => {
                  const title = launch.rocketNickname || launch.rocketName || 'Unknown rocket'
                  const engine = launch.engineDesignation || 'Unknown engine'

                  return (
                    <Button
                      key={launch.id}
                      variant={selectedLaunchId === launch.id ? 'dark' : 'outline-secondary'}
                      className="text-start"
                      onClick={() => setSelectedLaunchId(launch.id)}
                    >
                      <div className="fw-semibold">{title}</div>
                      <div className="small opacity-75">
                        {new Date(launch.launchDate).toLocaleString()} · {engine} · {launch.outcome}
                      </div>
                    </Button>
                  )
                })}

                {!isLoading && launches.length === 0 && (
                  <p className="mb-0 text-body-secondary">No launches logged yet. Save your first launch from the form.</p>
                )}
              </div>
            </Card.Body>
          </Card>
        </Col>
      </Row>

      <Card className="route-card">
        <Card.Body className="d-grid gap-3">
          <Card.Title>Launch detail</Card.Title>

          {!selectedLaunchId && <p className="mb-0 text-body-secondary">Select a launch from the log to view and edit details.</p>}

          {isLoadingDetail && (
            <div className="d-flex align-items-center gap-2 text-body-secondary">
              <Spinner animation="border" size="sm" role="status" />
              Loading launch detail...
            </div>
          )}

          {editDraft && (
            <Form className="d-grid gap-3">
              <Row className="g-3">
                <Col md={4}>
                  <Form.Label>Rocket</Form.Label>
                  <Form.Select
                    value={editDraft.userRocketId}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              userRocketId: event.target.value,
                            }
                          : previous,
                      )
                    }
                  >
                    {rockets.map((rocket) => (
                      <option key={rocket.id} value={rocket.id}>
                        {rocket.nickname || rocket.name || 'Unnamed rocket'}
                      </option>
                    ))}
                  </Form.Select>
                </Col>

                <Col md={4}>
                  <Form.Label>Engine</Form.Label>
                  <Form.Select
                    value={editDraft.engineId}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              engineId: event.target.value,
                            }
                          : previous,
                      )
                    }
                  >
                    {engines.map((engine) => (
                      <option key={engine.id} value={engine.engineId}>
                        {engine.designation || engine.engineId}
                      </option>
                    ))}
                  </Form.Select>
                </Col>

                <Col md={4}>
                  <Form.Label>Outcome</Form.Label>
                  <Form.Select
                    value={editDraft.outcome}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              outcome: event.target.value as LaunchOutcome,
                            }
                          : previous,
                      )
                    }
                  >
                    {outcomes.map((outcome) => (
                      <option key={outcome} value={outcome}>
                        {outcome}
                      </option>
                    ))}
                  </Form.Select>
                </Col>
              </Row>

              <Row className="g-3">
                <Col md={4}>
                  <Form.Label>Launch date/time</Form.Label>
                  <Form.Control
                    type="datetime-local"
                    value={editDraft.launchDate}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              launchDate: event.target.value,
                            }
                          : previous,
                      )
                    }
                  />
                </Col>

                <Col md={4}>
                  <Form.Label>Latitude</Form.Label>
                  <Form.Control
                    value={editDraft.lat}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              lat: event.target.value,
                            }
                          : previous,
                      )
                    }
                  />
                </Col>

                <Col md={4}>
                  <Form.Label>Longitude</Form.Label>
                  <Form.Control
                    value={editDraft.lng}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              lng: event.target.value,
                            }
                          : previous,
                      )
                    }
                  />
                </Col>
              </Row>

              <Row className="g-3">
                <Col md={6}>
                  <Form.Label>Location name</Form.Label>
                  <Form.Control
                    value={editDraft.locationName}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              locationName: event.target.value,
                            }
                          : previous,
                      )
                    }
                  />
                </Col>

                <Col md={6}>
                  <Form.Label>Photo URL</Form.Label>
                  <Form.Control
                    value={editDraft.photoUrl}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              photoUrl: event.target.value,
                            }
                          : previous,
                      )
                    }
                  />
                </Col>
              </Row>

              <Row className="g-3">
                <Col md={3}>
                  <Form.Label>Weather source</Form.Label>
                  <Form.Select
                    value={editDraft.weatherSource}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              weatherSource: event.target.value as 'api' | 'manual',
                            }
                          : previous,
                      )
                    }
                  >
                    <option value="api">API</option>
                    <option value="manual">Manual</option>
                  </Form.Select>
                </Col>

                <Col md={3}>
                  <Form.Label>Temp (F)</Form.Label>
                  <Form.Control
                    value={editDraft.temperatureF}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              temperatureF: event.target.value,
                            }
                          : previous,
                      )
                    }
                  />
                </Col>

                <Col md={3}>
                  <Form.Label>Wind mph</Form.Label>
                  <Form.Control
                    value={editDraft.windSpeedMph}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              windSpeedMph: event.target.value,
                            }
                          : previous,
                      )
                    }
                  />
                </Col>

                <Col md={3}>
                  <Form.Label>Wind direction</Form.Label>
                  <Form.Control
                    value={editDraft.windDirection}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              windDirection: event.target.value,
                            }
                          : previous,
                      )
                    }
                  />
                </Col>
              </Row>

              <Row className="g-3">
                <Col md={3}>
                  <Form.Label>Humidity</Form.Label>
                  <Form.Control
                    value={editDraft.humidity}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              humidity: event.target.value,
                            }
                          : previous,
                      )
                    }
                  />
                </Col>

                <Col md={3}>
                  <Form.Label>Conditions</Form.Label>
                  <Form.Control
                    value={editDraft.conditions}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              conditions: event.target.value,
                            }
                          : previous,
                      )
                    }
                  />
                </Col>

                <Col md={3}>
                  <Form.Label>Visibility (mi)</Form.Label>
                  <Form.Control
                    value={editDraft.visibilityMi}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              visibilityMi: event.target.value,
                            }
                          : previous,
                      )
                    }
                  />
                </Col>

                <Col md={3}>
                  <Form.Label>Altitude (ft)</Form.Label>
                  <Form.Control
                    value={editDraft.altitudeFt}
                    onChange={(event) =>
                      setEditDraft((previous) =>
                        previous
                          ? {
                              ...previous,
                              altitudeFt: event.target.value,
                            }
                          : previous,
                      )
                    }
                  />
                </Col>
              </Row>

              <Form.Group>
                <Form.Label>Notes</Form.Label>
                <Form.Control
                  as="textarea"
                  rows={3}
                  value={editDraft.notes}
                  onChange={(event) =>
                    setEditDraft((previous) =>
                      previous
                        ? {
                            ...previous,
                            notes: event.target.value,
                          }
                        : previous,
                    )
                  }
                />
              </Form.Group>

              <div className="d-flex gap-2 flex-wrap">
                <Button type="button" onClick={() => void handleSaveSelectedLaunch()} disabled={busyKey === 'save-launch'}>
                  {busyKey === 'save-launch' ? 'Saving...' : 'Save changes'}
                </Button>

                <Button
                  type="button"
                  variant="outline-danger"
                  onClick={() => void handleDeleteSelectedLaunch()}
                  disabled={busyKey === 'delete-launch'}
                >
                  {busyKey === 'delete-launch' ? 'Deleting...' : 'Delete launch'}
                </Button>

                <Form.Label className="btn btn-outline-secondary mb-0">
                  {busyKey === 'upload-photo' ? 'Uploading...' : 'Upload photo'}
                  <Form.Control
                    type="file"
                    className="d-none"
                    accept="image/*"
                    onChange={(event) => {
                      const input = event.currentTarget as HTMLInputElement
                      const file = input.files?.[0]
                      if (file) {
                        void handleUploadPhoto(file)
                        input.value = ''
                      }
                    }}
                    disabled={busyKey === 'upload-photo'}
                  />
                </Form.Label>
              </div>
            </Form>
          )}
        </Card.Body>
      </Card>
    </section>
  )
}