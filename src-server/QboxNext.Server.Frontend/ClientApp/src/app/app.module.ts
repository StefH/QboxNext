import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavigationComponent } from './navigation/navigation.component';
import { ElectricityComponent } from './electricity/electricity.component';
import { HomeComponent } from './home/home.component';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { DxChartModule } from 'devextreme-angular/ui/chart';
import { DxDateBoxModule } from 'devextreme-angular/ui/date-box';
import { DxSelectBoxModule } from 'devextreme-angular/ui/select-box';
import { DxLoadIndicatorModule } from 'devextreme-angular/ui/load-indicator';
import { DxCheckBoxModule } from 'devextreme-angular/ui/check-box';

// import nlMessages from 'devextreme/localization/messages/nl.json';
// import supplemental from 'devextreme-cldr-data/supplemental.json';
// import nlCldrData from 'devextreme-cldr-data/nl.json';

// import Globalize from 'globalize/message.js';
import { GasComponent } from './gas/gas.component';
import { AuthenticationService, AuthenticationGuard, AuthorizationInterceptor } from './authentication';
import { CallbackComponent } from './callback/callback.component';
import { WindowProvider } from './common/utils';

// Application wide providers/services
const APP_PROVIDERS = [
  AuthenticationGuard,
  AuthenticationService,
  WindowProvider
];

@NgModule({
  declarations: [
    AppComponent,
    NavigationComponent,
    CallbackComponent,

    HomeComponent,
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
    DxChartModule,
    DxSelectBoxModule,
    DxDateBoxModule,
    DxLoadIndicatorModule,
    DxCheckBoxModule
  ],
  providers: [
    // expose our Services and Providers/Services into Angular's dependency injection
    APP_PROVIDERS,
    {
      provide: HTTP_INTERCEPTORS,
      useClass: AuthorizationInterceptor,
      multi: true
    }
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
  constructor() {
    // Globalize.load(
    //   supplemental, nlCldrData
    // );

    // Globalize.loadMessages(nlMessages);

    // Globalize.locale('nl');
  }
}
