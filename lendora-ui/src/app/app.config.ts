import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withFetch } from '@angular/common/http';

import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    // withFetch() uses the native Fetch API instead of XHR — required for SSR
    // compatibility and aligns with Angular's modern defaults (v17+).
    provideHttpClient(withFetch()),
  ],
};
