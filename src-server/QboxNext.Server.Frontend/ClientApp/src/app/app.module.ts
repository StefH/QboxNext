import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";

import { AppRoutingModule } from "./app-routing.module";
import { AppComponent } from "./app.component";
import { NavigationComponent } from "./navigation/navigation.component";
import { ElectricityComponent } from "./electricity/electricity.component";
import { HomeComponent } from "./home/home.component";
import { HttpModule } from "@angular/http";
import { HttpClientModule } from "@angular/common/http";
import { DxChartModule } from 'devextreme-angular';

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
    DxChartModule
  ],
  bootstrap: [AppComponent]
})
export class AppModule {}
