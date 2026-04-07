export interface AuthUser {
  subject: string
  email: string
  displayName: string
  roles: string[]
}

export interface AuthContextValue {
  isLoading: boolean
  isAuthenticated: boolean
  authEnabled: boolean
  token: string | null
  user: AuthUser | null
  error: string | null
  signInDev: () => Promise<void>
  signOut: () => void
  refreshUser: () => Promise<void>
}
