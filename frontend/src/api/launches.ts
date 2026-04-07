import { deleteJson, getJson, postForm, postJson, putJson } from './http'
import type {
  LaunchDetail,
  LaunchListResponse,
  LaunchMutationRequest,
  UploadLaunchPhotoResponse,
  WeatherSnapshot,
} from '../types/launches'

export async function fetchMyLaunches(page = 1, pageSize = 20) {
  return getJson<LaunchListResponse>(`/api/v1/my/launches?page=${page}&pageSize=${pageSize}`)
}

export async function fetchLaunchById(id: string) {
  return getJson<LaunchDetail>(`/api/v1/my/launches/${id}`)
}

export async function createLaunch(request: LaunchMutationRequest) {
  return postJson<LaunchDetail>('/api/v1/my/launches', request)
}

export async function updateLaunch(id: string, request: LaunchMutationRequest) {
  return putJson<LaunchDetail>(`/api/v1/my/launches/${id}`, request)
}

export async function deleteLaunch(id: string) {
  await deleteJson(`/api/v1/my/launches/${id}`)
}

export async function uploadLaunchPhoto(id: string, file: File) {
  const form = new FormData()
  form.append('file', file)
  return postForm<UploadLaunchPhotoResponse>(`/api/v1/my/launches/${id}/photo`, form)
}

export async function fetchWeatherSnapshot(lat: number, lng: number) {
  return getJson<WeatherSnapshot>(`/api/v1/weather?lat=${lat}&lng=${lng}`)
}
