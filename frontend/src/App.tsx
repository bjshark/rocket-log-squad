import { BrowserRouter, Navigate, Route, Routes } from 'react-router-dom'

import { ProtectedRoute } from './components/auth/ProtectedRoute'
import { AppShell } from './components/layout/AppShell'
import { AppProviders } from './context/AppProviders'
import { LoginPage } from './pages/auth/LoginPage'
import { RegisterPage } from './pages/auth/RegisterPage'
import { CatalogPage } from './pages/catalog/CatalogPage'
import { DashboardPage } from './pages/dashboard/DashboardPage'
import { InventoryPage } from './pages/inventory/InventoryPage'
import { LaunchesPage } from './pages/launches/LaunchesPage'
import { AdminPage } from './pages/admin/AdminPage'
import { SettingsPage } from './pages/settings/SettingsPage'

export function App() {
  return (
    <AppProviders>
      <BrowserRouter>
        <Routes>
          <Route path="login" element={<LoginPage />} />
          <Route path="register" element={<RegisterPage />} />
          <Route element={<ProtectedRoute />}>
            <Route element={<AppShell />}>
              <Route index element={<DashboardPage />} />
              <Route path="inventory" element={<InventoryPage />} />
              <Route path="launches" element={<LaunchesPage />} />
              <Route path="catalog" element={<CatalogPage />} />
              <Route path="admin" element={<AdminPage />} />
              <Route path="settings" element={<SettingsPage />} />
            </Route>
          </Route>
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </BrowserRouter>
    </AppProviders>
  )
}