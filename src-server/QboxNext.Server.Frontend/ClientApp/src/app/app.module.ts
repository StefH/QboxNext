import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';

import { AppInsightsService, ApplicationInsightsModule } from '@markpieszak/ng-application-insights';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { DxButtonModule } from 'devextreme-angular/ui/button';
import { DxChartModule } from 'devextreme-angular/ui/chart';
import { DxCheckBoxModule } from 'devextreme-angular/ui/check-box';
import { DxDateBoxModule } from 'devextreme-angular/ui/date-box';
import { DxLoadIndicatorModule } from 'devextreme-angular/ui/load-indicator';
import { DxSelectBoxModule } from 'devextreme-angular/ui/select-box';

// import nlMessages from 'devextreme/localization/messages/nl.json';
// import supplemental from 'devextreme-cldr-data/supplemental.json';
// import nlCldrData from 'devextreme-cldr-data/nl.json';

// import Globalize from 'globalize/message.js';
import { CurrencyPipe } from '@angular/common';
import { registerLocaleData } from '@angular/common';
import localeNl from '@angular/common/locales/nl';
import { LOCALE_ID } from '@angular/core';
import { AboutComponent } from './about/about.component';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { AuthenticationGuard, AuthenticationService, AuthorizationInterceptor } from './authentication';
import { CallbackComponent } from './callback/callback.component';
import { WindowProvider } from './common/utils';
import { ElectricityComponent } from './electricity/electricity.component';
import { GasComponent } from './gas/gas.component';
import { HomeComponent } from './home/home.component';
import { NavigationComponent } from './navigation/navigation.component';

// Application wide providers/services
const APP_PROVIDERS = [
  AuthenticationGuard,
  AuthenticationService,
  WindowProvider,
  AppInsightsService
];

@NgModule({
  declarations: [
    AppComponent,
    NavigationComponent,
    CallbackComponent,

    HomeComponent,
    AboutComponent,
    ElectricityComponent,
    GasComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    FormsModule,

    // 3rd party
    NgbModule,

    ApplicationInsightsModule.forRoot({
      instrumentationKeySetLater: true
    }),

    DxButtonModule,
    DxChartModule,
    DxSelectBoxModule,
    DxDateBoxModule,
    DxLoadIndicatorModule,
    DxCheckBoxModule
  ],
  providers: [
    { provide: LOCALE_ID, useValue: 'nl-NL' },
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthorizationInterceptor,
      multi: true
    },
    CurrencyPipe,

    // expose our Services and Providers/Services into Angular's dependency injection
    APP_PROVIDERS
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
  constructor() {
    registerLocaleData(localeNl);

    // Globalize.load(
    //   supplemental, nlCldrData
    // );

    // Globalize.loadMessages(nlMessages);

    // Globalize.locale('nl');
  }
}
