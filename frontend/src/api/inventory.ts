import { deleteJson, getJson, postForm, postJson, putJson } from './http'
import type {
  AccessoryItem,
  AccessoryMutationRequest,
  UpdateUserEngineRequest,
  UpdateUserRocketRequest,
  UploadRocketPhotoResponse,
  UserEngineInventoryItem,
  UserRocketInventoryItem,
} from '../types/inventory'

export async function fetchMyRockets() {
  return getJson<UserRocketInventoryItem[]>('/api/v1/my/rockets')
}

export async function updateMyRocket(id: string, request: UpdateUserRocketRequest) {
  return putJson<UserRocketInventoryItem>(`/api/v1/my/rockets/${id}`, request)
}

export async function deleteMyRocket(id: string) {
  await deleteJson(`/api/v1/my/rockets/${id}`)
}

export async function uploadMyRocketPhoto(id: string, file: File) {
  const form = new FormData()
  form.append('file', file)
  return postForm<UploadRocketPhotoResponse>(`/api/v1/my/rockets/${id}/photo`, form)
}

export async function fetchMyEngines() {
  return getJson<UserEngineInventoryItem[]>('/api/v1/my/engines')
}

export async function updateMyEngine(id: string, request: UpdateUserEngineRequest) {
  return putJson<UserEngineInventoryItem>(`/api/v1/my/engines/${id}`, request)
}

export async function deleteMyEngine(id: string) {
  await deleteJson(`/api/v1/my/engines/${id}`)
}

export async function fetchAccessories() {
  return getJson<AccessoryItem[]>('/api/v1/my/accessories')
}

export async function createAccessory(request: AccessoryMutationRequest) {
  return postJson<AccessoryItem>('/api/v1/my/accessories', request)
}

export async function updateAccessory(id: string, request: AccessoryMutationRequest) {
  return putJson<AccessoryItem>(`/api/v1/my/accessories/${id}`, request)
}

export async function deleteAccessory(id: string) {
  await deleteJson(`/api/v1/my/accessories/${id}`)
}
