import { buildApiUrl } from './http'
import type { AuthUser } from '../types/auth'

function authHeaders(token: string | null): HeadersInit {
  const headers: HeadersInit = {
    Accept: 'application/json',
  }

  if (token) {
    headers.Authorization = `Bearer ${token}`
  }

  return headers
}

export async function fetchCurrentUser(token: string | null): Promise<AuthUser> {
  const response = await fetch(buildApiUrl('/api/v1/users/me'), {
    headers: authHeaders(token),
  })

  if (!response.ok) {
    throw new Error(`Unable to load user (${response.status})`)
  }

  return (await response.json()) as AuthUser
}
