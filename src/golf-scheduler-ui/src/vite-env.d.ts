/// <reference types="vite/client" />

declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<{}, {}, any>
  export default component
}

interface ImportMetaEnv {
  readonly VITE_API_URL: string
  readonly VITE_B2C_CLIENT_ID: string
  readonly VITE_B2C_AUTHORITY: string
  readonly VITE_B2C_KNOWN_AUTHORITIES: string
  readonly VITE_B2C_REDIRECT_URI: string
  readonly VITE_B2C_SCOPES: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
