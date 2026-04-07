import 'bootstrap/dist/css/bootstrap.min.css'
import './assets/app.css'

import { StrictMode } from 'react'
import ReactDOM from 'react-dom/client'
import { registerSW } from 'virtual:pwa-register'

import { App } from './App'

registerSW({ immediate: true })

ReactDOM.createRoot(document.getElementById('app')!).render(
  <StrictMode>
    <App />
  </StrictMode>,
)