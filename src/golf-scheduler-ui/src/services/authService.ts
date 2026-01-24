import { PublicClientApplication, Configuration, AccountInfo, InteractionRequiredAuthError } from '@azure/msal-browser';

const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_B2C_CLIENT_ID || '',
    authority: import.meta.env.VITE_B2C_AUTHORITY || '',
    knownAuthorities: [import.meta.env.VITE_B2C_KNOWN_AUTHORITIES || ''],
    redirectUri: import.meta.env.VITE_B2C_REDIRECT_URI || 'http://localhost:5173',
    postLogoutRedirectUri: import.meta.env.VITE_B2C_REDIRECT_URI || 'http://localhost:5173',
  },
  cache: {
    cacheLocation: 'localStorage',
    storeAuthStateInCookie: false,
  },
};

const loginRequest = {
  scopes: [import.meta.env.VITE_B2C_SCOPES || ''],
};

const tokenRequest = {
  scopes: [import.meta.env.VITE_B2C_SCOPES || ''],
};

class AuthService {
  private msalInstance: PublicClientApplication;
  private initialized: boolean = false;

  constructor() {
    this.msalInstance = new PublicClientApplication(msalConfig);
  }

  async initialize(): Promise<void> {
    if (this.initialized) return;
    await this.msalInstance.initialize();
    await this.msalInstance.handleRedirectPromise();
    this.initialized = true;
  }

  async login(): Promise<void> {
    await this.initialize();
    try {
      await this.msalInstance.loginPopup(loginRequest);
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  }

  async logout(): Promise<void> {
    await this.initialize();
    const account = this.getAccount();
    if (account) {
      await this.msalInstance.logoutPopup({
        account,
        postLogoutRedirectUri: msalConfig.auth.postLogoutRedirectUri,
      });
    }
  }

  getAccount(): AccountInfo | null {
    const accounts = this.msalInstance.getAllAccounts();
    return accounts.length > 0 ? accounts[0] : null;
  }

  isAuthenticated(): boolean {
    return this.getAccount() !== null;
  }

  async getAccessToken(): Promise<string | null> {
    await this.initialize();
    const account = this.getAccount();
    if (!account) return null;

    try {
      const response = await this.msalInstance.acquireTokenSilent({
        ...tokenRequest,
        account,
      });
      return response.accessToken;
    } catch (error) {
      if (error instanceof InteractionRequiredAuthError) {
        try {
          const response = await this.msalInstance.acquireTokenPopup(tokenRequest);
          return response.accessToken;
        } catch (popupError) {
          console.error('Token acquisition failed:', popupError);
          return null;
        }
      }
      console.error('Token acquisition failed:', error);
      return null;
    }
  }

  getUserInfo(): { name: string; email: string } | null {
    const account = this.getAccount();
    if (!account) return null;

    return {
      name: account.name || account.username || 'Unknown',
      email: (account.idTokenClaims?.emails as string[])?.[0] || account.username || '',
    };
  }
}

export const authService = new AuthService();
