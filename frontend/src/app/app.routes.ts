import { Routes } from '@angular/router';
import { LoginPageComponent } from './pages/login-page/login-page.component';
import { RegisterPageComponent } from './pages/register-page/register-page.component';
import { HomePageComponent } from './pages/home-page/home-page.component';
import { PadelPageComponent } from './pages/padel-page/padel-page.component';
import { PlanningPageComponent } from './pages/planning-page/planning-page.component';
import { FidelitePageComponent } from './pages/fidelite-page/fidelite-page.component';
import { PersonnelPageComponent } from './pages/personnel-page/personnel-page.component';
import { AuthGuard } from './guards/auth.guard';
import { RoleGuard } from './guards/role.guard';

export const routes: Routes = [
  { path: '', component: HomePageComponent, canActivate: [AuthGuard] },
  { path: 'login', component: LoginPageComponent },
  { path: 'register', component: RegisterPageComponent },
  { path: 'padel', component: PadelPageComponent, canActivate: [AuthGuard] },
  { path: 'planning', component: PlanningPageComponent, canActivate: [AuthGuard] },
  { path: 'fidelite', component: FidelitePageComponent, canActivate: [AuthGuard] },
  {
    path: 'personnel',
    component: PersonnelPageComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Administrateur'] }
  },
  { path: '**', redirectTo: '' }
];
