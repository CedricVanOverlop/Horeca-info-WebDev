import { Routes } from '@angular/router';
import { LoginPageComponent } from './pages/login-page/login-page.component';
import { RegisterPageComponent } from './pages/register-page/register-page.component';
import { HomePageComponent } from './pages/home-page/home-page.component';
import { PadelPageComponent } from './pages/padel-page/padel-page.component';
import { PlanningPageComponent } from './pages/planning-page/planning-page.component';
import { FidelitePageComponent } from './pages/fidelite-page/fidelite-page.component';
import { PersonnelPageComponent } from './pages/personnel-page/personnel-page.component';

export const routes: Routes = [
  { path: '', component: HomePageComponent },
  { path: 'login', component: LoginPageComponent },
  { path: 'register', component: RegisterPageComponent },
  { path: 'padel', component: PadelPageComponent },
  { path: 'planning', component: PlanningPageComponent },
  { path: 'fidelite', component: FidelitePageComponent },
  { path: 'personnel', component: PersonnelPageComponent },
  { path: '**', redirectTo: '' }
];
