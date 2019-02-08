import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AuthenticationGuard } from './authentication';
import { CallbackComponent } from './callback/callback.component';
import { ElectricityComponent } from './electricity/electricity.component';
import { GasComponent } from './gas/gas.component';
import { HomeComponent } from './home/home.component';

const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'callback', component: CallbackComponent },
  { path: 'electricity', component: ElectricityComponent, canActivate: [AuthenticationGuard] },
  { path: 'gas', component: GasComponent, canActivate: [AuthenticationGuard] }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
