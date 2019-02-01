import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavigationComponent } from './navigation/navigation.component';
import { ElectricityComponent } from './electricity/electricity.component';
import { HomeComponent } from './home/home.component';
import { HttpModule } from '@angular/http';
import { HttpClientModule } from '@angular/common/http';
import { FormsModule } from '@angular/forms';

import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { DxChartModule, DxDateBoxModule, DxSelectBoxModule, DxLoadIndicatorModule, DxCheckBoxModule } from 'devextreme-angular';

import nlMessages from 'devextreme/localization/messages/nl.json';
import supplemental from 'devextreme-cldr-data/supplemental.json';
import nlCldrData from 'devextreme-cldr-data/nl.json';

import Globalize from 'globalize/message.js';
import { GasComponent } from './gas/gas.component';

@NgModule({
  declarations: [
    AppComponent,
    NavigationComponent,

    HomeComponent,
    ElectricityComponent,
    GasComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpModule,
    HttpClientModule,
    FormsModule,
    NgbModule.forRoot(),
    DxChartModule,
    DxSelectBoxModule,
    DxDateBoxModule,
    DxLoadIndicatorModule,
    DxCheckBoxModule
  ],
  bootstrap: [AppComponent]
})
export class AppModule {
  constructor() {
    Globalize.load(
      supplemental, nlCldrData
    );

    Globalize.loadMessages(nlMessages);

    Globalize.locale('nl');
  }
}
