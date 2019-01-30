import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { HomeComponent } from "./home/home.component";
import { ElectricityComponent } from "./electricity/electricity.component";

const routes: Routes = [
  { path: "", component: HomeComponent },
  { path: "electricity", component: ElectricityComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
