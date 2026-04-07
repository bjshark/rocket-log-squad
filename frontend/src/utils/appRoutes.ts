export function getAppRoutes(isAdmin: boolean) {
  const baseRoutes = [
    { path: '/', label: 'Dashboard' },
    { path: '/inventory', label: 'Inventory' },
    { path: '/launches', label: 'Launches' },
    { path: '/catalog', label: 'Catalog' },
  ]

  if (isAdmin) {
    baseRoutes.push({ path: '/admin', label: 'Admin' })
  }

  baseRoutes.push({ path: '/settings', label: 'Settings' })
  return baseRoutes
}