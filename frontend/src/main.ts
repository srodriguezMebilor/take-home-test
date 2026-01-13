import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { MatSnackBarModule } from '@angular/material/snack-bar';
import { importProvidersFrom } from '@angular/core';

bootstrapApplication(AppComponent, {
    providers: [
      ...appConfig.providers,
      importProvidersFrom(
        MatSnackBarModule
      )
    ]
  })
  .catch((err) => console.error(err));
