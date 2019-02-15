import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';

import { AboutComponent } from './about/about.component';
import { AuthenticationGuard } from './authentication';
import { CallbackComponent } from './callback/callback.component';
import { ElectricityComponent } from './electricity/electricity.component';
import { GasComponent } from './gas/gas.component';
import { HomeComponent } from './home/home.component';

const routes: Routes = [
  { path: '', component: HomeComponent, canActivate: [AuthenticationGuard] },
  { path: 'callback', component: CallbackComponent },
  { path: 'about', component: AboutComponent, canActivate: [AuthenticationGuard] },
  { path: 'electricity', component: ElectricityComponent, canActivate: [AuthenticationGuard] },
  { path: 'gas', component: GasComponent, canActivate: [AuthenticationGuard] }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
