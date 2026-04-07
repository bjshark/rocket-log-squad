import type { PagedResponse } from './catalog'

export interface AdminRocketItem {
  id: string
  manufacturer: string
  sku: string
  name: string
  description: string
  skillLevel: string
  recommendedEngines: string[]
  diameterMm: number
  lengthMm: number
  weightG: number
  finMaterial: string
  noseCone: string
  recoverySystem: string
  thumbnailUrl: string
  imageUrls: string[]
  productUrl: string | null
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface AdminEngineItem {
  id: string
  manufacturer: string
  designation: string
  impulseClass: string
  totalImpulseNs: number
  averageThrustN: number
  delayS: number
  diameterMm: number
  lengthMm: number
  propellantWeightG: number
  totalWeightG: number
  caseType: string
  propellantType: string
  thumbnailUrl: string
  imageUrls: string[]
  certificationBody: string | null
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface AdminRocketMutationInput {
  manufacturer: string
  sku: string
  name: string
  description: string
  skillLevel: string
  recommendedEngines: string[]
  diameterMm: number
  lengthMm: number
  weightG: number
  finMaterial: string
  noseCone: string
  recoverySystem: string
  thumbnailUrl: string
  imageUrls: string[]
  productUrl: string | null
  isActive: boolean
}

export interface AdminEngineMutationInput {
  manufacturer: string
  designation: string
  impulseClass: string
  totalImpulseNs: number
  averageThrustN: number
  delayS: number
  diameterMm: number
  lengthMm: number
  propellantWeightG: number
  totalWeightG: number
  caseType: string
  propellantType: string
  thumbnailUrl: string
  imageUrls: string[]
  certificationBody: string | null
  isActive: boolean
}

export interface AdminImageUploadResponse {
  imageUrl: string
}

export interface AdminSeedRocket {
  manufacturer: string
  sku: string
  name: string
  description: string
  skillLevel: string
  recommendedEngines: string[]
  diameterMm: number
  lengthMm: number
  weightG: number
  finMaterial: string
  noseCone: string
  recoverySystem: string
  thumbnailUrl: string
  imageUrls: string[]
  productUrl: string | null
}

export interface AdminSeedEngine {
  manufacturer: string
  designation: string
  impulseClass: string
  totalImpulseNs: number
  averageThrustN: number
  delayS: number
  diameterMm: number
  lengthMm: number
  propellantWeightG: number
  totalWeightG: number
  caseType: string
  propellantType: string
  thumbnailUrl: string
  imageUrls: string[]
  certificationBody: string | null
}

export interface AdminSeedExportPayload {
  rockets: AdminSeedRocket[]
  engines: AdminSeedEngine[]
  exportedAtUtc: string
}

export type AdminRocketsPage = PagedResponse<AdminRocketItem>
export type AdminEnginesPage = PagedResponse<AdminEngineItem>
