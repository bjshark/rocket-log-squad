import { createContext, useCallback, useContext, useEffect, useMemo, useState, type PropsWithChildren } from 'react'

import { fetchCurrentUser } from '../api/auth'
import type { AuthContextValue, AuthUser } from '../types/auth'
import { useRuntimeFlags } from '../hooks/useRuntimeFlags'

const TOKEN_STORAGE_KEY = 'rocketlog.auth.token'

const AuthContext = createContext<AuthContextValue | null>(null)

function readStoredToken() {
  try {
    return localStorage.getItem(TOKEN_STORAGE_KEY)
  } catch {
    return null
  }
}

function persistToken(token: string | null) {
  try {
    if (token) {
      localStorage.setItem(TOKEN_STORAGE_KEY, token)
      return
    }

    localStorage.removeItem(TOKEN_STORAGE_KEY)
  } catch {
    // Ignore storage failures in privacy mode.
  }
}

function fallbackDevUser(): AuthUser {
  return {
    subject: 'dev-admin',
    email: 'dev-admin@rocketlog.local',
    displayName: 'Rocket Log Dev Admin',
    roles: ['admin', 'user'],
  }
}

export function AuthProvider({ children }: PropsWithChildren) {
  const { authEnabled } = useRuntimeFlags()
  const [token, setToken] = useState<string | null>(() => readStoredToken())
  const [user, setUser] = useState<AuthUser | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const refreshUser = useCallback(async () => {
    setIsLoading(true)
    setError(null)

    try {
      const loadedUser = await fetchCurrentUser(token)
      setUser(loadedUser)
    } catch (caughtError) {
      if (!authEnabled) {
        setUser(fallbackDevUser())
      } else {
        setUser(null)
        setError(caughtError instanceof Error ? caughtError.message : 'Unable to load user')
      }
    } finally {
      setIsLoading(false)
    }
  }, [authEnabled, token])

  useEffect(() => {
    void refreshUser()
  }, [refreshUser])

  const signInDev = useCallback(async () => {
    if (authEnabled) {
      setError('Production auth flow is not implemented yet. Use development bypass for now.')
      return
    }

    setToken(null)
    persistToken(null)
    await refreshUser()
  }, [authEnabled, refreshUser])

  const signOut = useCallback(() => {
    setToken(null)
    persistToken(null)
    setUser(null)
  }, [])

  const value = useMemo<AuthContextValue>(() => {
    return {
      isLoading,
      isAuthenticated: Boolean(user),
      authEnabled,
      token,
      user,
      error,
      signInDev,
      signOut,
      refreshUser,
    }
  }, [authEnabled, error, isLoading, refreshUser, signInDev, signOut, token, user])

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)

  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider')
  }

  return context
}
