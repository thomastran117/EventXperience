import {
  ApplicationConfig,
  provideBrowserGlobalErrorListeners,
  provideZoneChangeDetection,
  importProvidersFrom,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { CoreModule } from './core/core.module';
import { provideStore } from '@ngrx/store';
import { routes } from './app.routes';

import { userReducer } from './core/stores/user.reducer';

export const appConfig: ApplicationConfig = {
  providers: [
    provideStore({ user: userReducer }),
    provideBrowserGlobalErrorListeners(),
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    importProvidersFrom(CoreModule),
  ],
};
