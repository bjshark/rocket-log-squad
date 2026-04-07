import { getJson, postJson } from './http'
import type {
  AddEngineToInventoryResponse,
  AddRocketToInventoryResponse,
  CatalogFilterOptions,
  EngineFilterOptions,
  EngineCatalogItem,
  EngineCatalogQuery,
  PagedResponse,
  RocketCatalogItem,
  RocketFilterOptions,
  RocketCatalogQuery,
} from '../types/catalog'

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

export async function fetchRockets(query: RocketCatalogQuery) {
  const queryString = toQueryString({
    query: query.query?.trim(),
    manufacturer: query.manufacturer?.trim(),
    page: query.page ?? 1,
    pageSize: query.pageSize ?? 10,
  })

  return getJson<PagedResponse<RocketCatalogItem>>(`/api/v1/rockets?${queryString}`)
}

export async function fetchEngines(query: EngineCatalogQuery) {
  const queryString = toQueryString({
    query: query.query?.trim(),
    manufacturer: query.manufacturer?.trim(),
    impulseClass: query.impulseClass?.trim().toUpperCase(),
    page: query.page ?? 1,
    pageSize: query.pageSize ?? 10,
  })

  return getJson<PagedResponse<EngineCatalogItem>>(`/api/v1/engines?${queryString}`)
}

export async function fetchRocketFilterOptions() {
  return getJson<RocketFilterOptions>('/api/v1/rockets/filters')
}

export async function fetchEngineFilterOptions() {
  return getJson<EngineFilterOptions>('/api/v1/engines/filters')
}

export async function fetchCatalogFilterOptions(): Promise<CatalogFilterOptions> {
  const [rocketOptions, engineOptions] = await Promise.all([
    fetchRocketFilterOptions(),
    fetchEngineFilterOptions(),
  ])

  const manufacturers = Array.from(
    new Set([...rocketOptions.manufacturers, ...engineOptions.manufacturers]),
  ).sort((left, right) => left.localeCompare(right))

  return {
    manufacturers,
    impulseClasses: [...engineOptions.impulseClasses],
  }
}

export async function addRocketToInventory(rocketId: string) {
  return postJson<AddRocketToInventoryResponse>('/api/v1/my/rockets', { rocketId })
}

export async function addEngineToInventory(engineId: string) {
  return postJson<AddEngineToInventoryResponse>('/api/v1/my/engines', { engineId })
}
