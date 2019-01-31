import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavigationComponent } from './navigation/navigation.component';
import { ElectricityComponent } from './electricity/electricity.component';
import { HomeComponent } from './home/home.component';
import { HttpModule } from '@angular/http';
import { HttpClientModule } from '@angular/common/http';
import { DxChartModule, DxDateBoxModule, DxSelectBoxModule } from 'devextreme-angular';
import { FormsModule } from '@angular/forms';
import { locale, loadMessages } from 'devextreme/localization';

import nlMessages from 'devextreme/localization/messages/nl.json';
import supplemental from 'devextreme-cldr-data/supplemental.json';
import nlCldrData from 'devextreme-cldr-data/nl.json';

import Globalize from 'globalize';

Globalize.load(
  supplemental, nlCldrData
);

// Globalize.loadMessages('nlMessages');

Globalize.locale('nl');

@NgModule({
  declarations: [
    AppComponent,
    NavigationComponent,
    ElectricityComponent,
    HomeComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpModule,
    HttpClientModule,
    FormsModule,
    DxChartModule,
    DxSelectBoxModule,
    DxDateBoxModule
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

    // loadMessages(nlMessages);
    // locale('nl');
  }
}
