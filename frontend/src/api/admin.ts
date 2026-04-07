import { deleteJson, getJson, postForm, postJson, putJson } from './http'
import type {
  AdminEngineItem,
  AdminEngineMutationInput,
  AdminEnginesPage,
  AdminImageUploadResponse,
  AdminRocketItem,
  AdminRocketMutationInput,
  AdminRocketsPage,
  AdminSeedExportPayload,
} from '../types/admin'

function toQueryString(query: Record<string, string | number | undefined>) {
  const params = new URLSearchParams()

  for (const [key, value] of Object.entries(query)) {
    if (value === undefined || value === '') {
      continue
    }

    params.set(key, String(value))
  }

  return params.toString()
}

export async function fetchAdminRockets(page = 1, pageSize = 50) {
  const queryString = toQueryString({ page, pageSize })
  return getJson<AdminRocketsPage>(`/api/v1/admin/rockets?${queryString}`)
}

export async function fetchAdminRocketById(id: string) {
  return getJson<AdminRocketItem>(`/api/v1/admin/rockets/${id}`)
}

export async function createAdminRocket(payload: AdminRocketMutationInput) {
  return postJson<AdminRocketItem>('/api/v1/admin/rockets', payload)
}

export async function updateAdminRocket(id: string, payload: AdminRocketMutationInput) {
  return putJson<AdminRocketItem>(`/api/v1/admin/rockets/${id}`, payload)
}

export async function deleteAdminRocket(id: string) {
  return deleteJson(`/api/v1/admin/rockets/${id}`)
}

export async function fetchAdminEngines(page = 1, pageSize = 50) {
  const queryString = toQueryString({ page, pageSize })
  return getJson<AdminEnginesPage>(`/api/v1/admin/engines?${queryString}`)
}

export async function fetchAdminEngineById(id: string) {
  return getJson<AdminEngineItem>(`/api/v1/admin/engines/${id}`)
}

export async function createAdminEngine(payload: AdminEngineMutationInput) {
  return postJson<AdminEngineItem>('/api/v1/admin/engines', payload)
}

export async function updateAdminEngine(id: string, payload: AdminEngineMutationInput) {
  return putJson<AdminEngineItem>(`/api/v1/admin/engines/${id}`, payload)
}

export async function deleteAdminEngine(id: string) {
  return deleteJson(`/api/v1/admin/engines/${id}`)
}

export async function uploadAdminImage(file: File, scope: string) {
  const body = new FormData()
  body.set('file', file)
  body.set('scope', scope)
  return postForm<AdminImageUploadResponse>('/api/v1/admin/images/upload', body)
}

export async function exportAdminSeeds() {
  return getJson<AdminSeedExportPayload>('/api/v1/admin/seeds/export')
}
