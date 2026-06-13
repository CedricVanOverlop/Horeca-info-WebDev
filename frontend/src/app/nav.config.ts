import { Type } from '@angular/core';
import { Role, Roles, ALL_ROLES } from './roles';

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

/**
 * Description d'une page de l'application : son chemin, son composant, ses
 * contraintes d'accès et sa présence dans la sidebar.
 *
 * SOURCE UNIQUE de vérité pour la navigation : `app.routes.ts` (génération des
 * routes + guards) et `side-menu` (liens filtrés par rôle) sont tous deux
 * dérivés de ce tableau. Ajouter / modifier une page = une seule ligne ici.
 */
export interface NavItem {
  /** Segment d'URL sans slash initial (ex. 'mon-compte'). */
  path: string;
  /** Composant page chargé par le router. */
  component: Type<unknown>;
  /** Libellé affiché dans la sidebar (requis si `sidebar` est vrai). */
  label?: string;
  /** Rôles autorisés à accéder à la page. Ignoré si `public` est vrai. */
  roles?: Role[];
  /** Vrai si un lien vers cette page doit apparaître dans la sidebar. */
  sidebar?: boolean;
  /** Vrai si la page est accessible sans authentification (aucun guard). */
  public?: boolean;
}

/** Toutes les pages de l'application, déclarées une seule fois. */
export const NAV: NavItem[] = [
  // Pages publiques (aucun guard, absentes de la sidebar)
  { path: '', component: HomePageComponent, public: true },
  { path: 'login', component: LoginPageComponent, public: true },
  { path: 'register', component: RegisterPageComponent, public: true },

  // Pages protégées (AuthGuard + RoleGuard, présentes dans la sidebar)
  { path: 'mon-compte', component: MonComptePageComponent, label: 'Mon compte', roles: ALL_ROLES, sidebar: true },
  { path: 'fidelite', component: FidelitePageComponent, label: 'Mes points de fidélité', roles: [Roles.Client, Roles.Employe, Roles.Administrateur], sidebar: true },
  { path: 'padel', component: PadelPageComponent, label: 'Réserver un terrain', roles: [Roles.Client, Roles.Employe, Roles.Cuisine, Roles.Administrateur], sidebar: true },
  { path: 'disponibilites', component: DisponibilitesPageComponent, label: 'Mes disponibilités', roles: [Roles.Employe, Roles.Administrateur], sidebar: true },
  { path: 'mon-horaire', component: MonHorairePageComponent, label: 'Mon horaire', roles: [Roles.Employe, Roles.Administrateur], sidebar: true },
  { path: 'cuisine', component: GestionCuisinePageComponent, label: 'Gestion Cuisine', roles: [Roles.Cuisine, Roles.Administrateur], sidebar: true },
  { path: 'terrains', component: GestionTerrainsPageComponent, label: 'Gestion Terrains', roles: [Roles.Cuisine, Roles.Administrateur], sidebar: true },
  { path: 'stocks', component: GestionStocksPageComponent, label: 'Gestion Stocks', roles: [Roles.Cuisine, Roles.Administrateur], sidebar: true },
  { path: 'horaires', component: CreerHorairesPageComponent, label: 'Créer des horaires', roles: [Roles.Administrateur], sidebar: true },
  { path: 'utilisateurs', component: GestionUtilisateursPageComponent, label: 'Gestion des utilisateurs', roles: [Roles.Administrateur], sidebar: true }
];
