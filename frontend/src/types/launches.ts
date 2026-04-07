import type { PagedResponse } from './catalog'

export type LaunchOutcome = 'Success' | 'Partial' | 'Failure' | 'No Launch'

export interface LaunchLocation {
  name: string | null
  lat: number
  lng: number
}

export interface LaunchWeather {
  source: 'api' | 'manual'
  temperatureF: number
  windSpeedMph: number
  windDirection: string
  humidity: number
  conditions: string
  visibilityMi: number | null
}

export interface LaunchListItem {
  id: string
  launchDate: string
  outcome: LaunchOutcome
  locationName: string | null
  lat: number
  lng: number
  userRocketId: string
  rocketNickname: string | null
  rocketName: string | null
  engineId: string
  engineDesignation: string | null
  photoUrl: string | null
  updatedAt: string
}

export interface LaunchDetail {
  id: string
  userRocketId: string
  rocketNickname: string | null
  rocketName: string | null
  engineId: string
  engineDesignation: string | null
  launchDate: string
  location: LaunchLocation
  weather: LaunchWeather
  outcome: LaunchOutcome
  altitudeFt: number | null
  notes: string | null
  photoUrl: string | null
  createdAt: string
  updatedAt: string
}

export interface LaunchMutationRequest {
  userRocketId: string
  engineId: string
  launchDate: string
  location: LaunchLocationRequest
  weather: LaunchWeatherRequest
  outcome: LaunchOutcome
  altitudeFt: number | null
  notes: string | null
  photoUrl: string | null
}

export interface LaunchLocationRequest {
  name: string | null
  lat: number
  lng: number
}

export interface LaunchWeatherRequest {
  source: 'api' | 'manual'
  temperatureF: number
  windSpeedMph: number
  windDirection: string
  humidity: number
  conditions: string
  visibilityMi: number | null
}

export interface UploadLaunchPhotoResponse {
  launchId: string
  photoUrl: string | null
}

export interface WeatherSnapshot {
  source: 'api' | 'manual'
  temperatureF: number
  windSpeedMph: number
  windDirection: string
  humidity: number
  conditions: string
  visibilityMi: number | null
  locationName: string | null
}

export type LaunchListResponse = PagedResponse<LaunchListItem>
