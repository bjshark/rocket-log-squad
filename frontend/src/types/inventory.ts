export interface UserRocketInventoryItem {
  id: string
  rocketId: string
  manufacturer: string | null
  sku: string | null
  name: string | null
  thumbnailUrl: string | null
  nickname: string | null
  buildDate: string | null
  condition: 'New' | 'Good' | 'Fair' | 'Retired'
  buildNotes: string | null
  photoUrl: string | null
  createdAt: string
  updatedAt: string
}

export interface UpdateUserRocketRequest {
  nickname: string | null
  buildDate: string | null
  condition: 'New' | 'Good' | 'Fair' | 'Retired'
  buildNotes: string | null
  photoUrl: string | null
}

export interface UploadRocketPhotoResponse {
  userRocketId: string
  photoUrl: string | null
}

export interface UserEngineInventoryItem {
  id: string
  engineId: string
  manufacturer: string | null
  designation: string | null
  impulseClass: string | null
  thumbnailUrl: string | null
  quantityOnHand: number
  purchaseDate: string | null
  notes: string | null
  updatedAt: string
}

export interface UpdateUserEngineRequest {
  quantityOnHand: number
  purchaseDate: string | null
  notes: string | null
}

export interface AccessoryItem {
  id: string
  name: string
  category: string
  brand: string | null
  notes: string | null
  photoUrl: string | null
  createdAt: string
  updatedAt: string
}

export interface AccessoryMutationRequest {
  name: string
  category: string
  brand: string | null
  notes: string | null
  photoUrl: string | null
}
