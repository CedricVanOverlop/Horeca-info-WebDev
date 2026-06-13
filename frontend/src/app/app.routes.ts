import { Routes } from '@angular/router';
import { LoginPageComponent } from './pages/login-page/login-page.component';
import { RegisterPageComponent } from './pages/register-page/register-page.component';
import { HomePageComponent } from './pages/home-page/home-page.component';
import { MonComptePageComponent } from './pages/mon-compte-page/mon-compte-page.component';
import { FidelitePageComponent } from './pages/fidelite-page/fidelite-page.component';
import { PadelPageComponent } from './pages/padel-page/padel-page.component';
import { DisponibilitesPageComponent } from './pages/disponibilites-page/disponibilites-page.component';
import { MonHorairePageComponent } from './pages/mon-horaire-page/mon-horaire-page.component';
import { GestionCuisinePageComponent } from './pages/gestion-cuisine-page/gestion-cuisine-page.component';
import { GestionTerrainsPageComponent } from './pages/gestion-terrains-page/gestion-terrains-page.component';
import { GestionStocksPageComponent } from './pages/gestion-stocks-page/gestion-stocks-page.component';
import { CreerHorairesPageComponent } from './pages/creer-horaires-page/creer-horaires-page.component';
import { GestionUtilisateursPageComponent } from './pages/gestion-utilisateurs-page/gestion-utilisateurs-page.component';
import { AuthGuard } from './guards/auth.guard';
import { RoleGuard } from './guards/role.guard';

export const routes: Routes = [
  // Pages publiques
  { path: '', component: HomePageComponent },
  { path: 'login', component: LoginPageComponent },
  { path: 'register', component: RegisterPageComponent },

  // Pages protégées — rôles explicites par page
  {
    path: 'mon-compte',
    component: MonComptePageComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Client', 'Employe', 'Cuisine', 'Administrateur'] }
  },
  {
    path: 'fidelite',
    component: FidelitePageComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Client','Employe', 'Administrateur'] }
  },
  {
    path: 'padel',
    component: PadelPageComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Client','Employe', 'Cuisine', 'Administrateur'] }
  },
  {
    path: 'disponibilites',
    component: DisponibilitesPageComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Employe', 'Administrateur'] }
  },
  {
    path: 'mon-horaire',
    component: MonHorairePageComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Employe', 'Administrateur'] }
  },
  {
    path: 'cuisine',
    component: GestionCuisinePageComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Cuisine', 'Administrateur'] }
  },
  {
    path: 'terrains',
    component: GestionTerrainsPageComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Cuisine', 'Administrateur'] }
  },
  {
    path: 'stocks',
    component: GestionStocksPageComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Cuisine', 'Administrateur'] }
  },
  {
    path: 'horaires',
    component: CreerHorairesPageComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Administrateur'] }
  },
  {
    path: 'utilisateurs',
    component: GestionUtilisateursPageComponent,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: ['Administrateur'] }
  },

  { path: '**', redirectTo: '' }
];
