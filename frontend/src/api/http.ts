const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:8080'

export function buildApiUrl(path: string) {
  return new URL(path, API_BASE_URL).toString()
}

interface ApiErrorResponse {
  error?: string
  code?: string
}

async function parseResponseError(response: Response): Promise<Error> {
  try {
    const payload = (await response.json()) as ApiErrorResponse
    if (payload?.error) {
      return new Error(payload.error)
    }
  } catch {
    // Ignore parse failures and fallback to status-based message.
  }

  return new Error(`Request failed with status ${response.status}`)
}

export async function getJson<T>(path: string): Promise<T> {
  const response = await fetch(buildApiUrl(path), {
    headers: {
      Accept: 'application/json',
    },
  })

  if (!response.ok) {
    throw await parseResponseError(response)
  }

  return (await response.json()) as T
}

export async function postJson<TResponse>(path: string, body: unknown): Promise<TResponse> {
  const response = await fetch(buildApiUrl(path), {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    throw await parseResponseError(response)
  }

  return (await response.json()) as TResponse
}

export async function putJson<TResponse>(path: string, body: unknown): Promise<TResponse> {
  const response = await fetch(buildApiUrl(path), {
    method: 'PUT',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/json',
    },
    body: JSON.stringify(body),
  })

  if (!response.ok) {
    throw await parseResponseError(response)
  }

  return (await response.json()) as TResponse
}

export async function postForm<TResponse>(path: string, body: FormData): Promise<TResponse> {
  const response = await fetch(buildApiUrl(path), {
    method: 'POST',
    headers: {
      Accept: 'application/json',
    },
    body,
  })

  if (!response.ok) {
    throw await parseResponseError(response)
  }

  return (await response.json()) as TResponse
}

export async function deleteJson(path: string): Promise<void> {
  const response = await fetch(buildApiUrl(path), {
    method: 'DELETE',
    headers: {
      Accept: 'application/json',
    },
  })

  if (!response.ok) {
    throw await parseResponseError(response)
  }
}

export { API_BASE_URL }