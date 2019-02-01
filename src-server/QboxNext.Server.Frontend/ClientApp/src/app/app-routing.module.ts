import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { ElectricityComponent } from './electricity/electricity.component';
import { GasComponent } from './gas/gas.component';

const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'electricity', component: ElectricityComponent },
  { path: 'gas', component: GasComponent }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule {}
