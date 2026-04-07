export interface PagedResponse<TItem> {
  items: TItem[]
  total: number
  page: number
  pageSize: number
}

export interface RocketCatalogItem {
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
  thumbnailUrl: string
  productUrl: string | null
}

export interface EngineCatalogItem {
  id: string
  manufacturer: string
  designation: string
  impulseClass: string
  totalImpulseNs: number
  averageThrustN: number
  delayS: number
  caseType: string
  propellantType: string
  thumbnailUrl: string
  certificationBody: string | null
}

export interface RocketCatalogQuery {
  query?: string
  manufacturer?: string
  page?: number
  pageSize?: number
}

export interface EngineCatalogQuery {
  query?: string
  manufacturer?: string
  impulseClass?: string
  page?: number
  pageSize?: number
}

export interface RocketFilterOptions {
  manufacturers: string[]
}

export interface EngineFilterOptions {
  manufacturers: string[]
  impulseClasses: string[]
}

export interface CatalogFilterOptions {
  manufacturers: string[]
  impulseClasses: string[]
}

export interface AddRocketToInventoryResponse {
  userRocketId: string
  rocketId: string
  added: boolean
}

export interface AddEngineToInventoryResponse {
  userEngineId: string
  engineId: string
  quantityOnHand: number
  addedNewEntry: boolean
}
