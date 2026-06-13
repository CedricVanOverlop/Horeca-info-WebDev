import { Route, Routes } from '@angular/router';
import { NAV, NavItem } from './nav.config';
import { AuthGuard } from './guards/auth.guard';
import { RoleGuard } from './guards/role.guard';

/**
 * Transforme une entrée de la config NAV en route Angular.
 * Les pages publiques sont montées sans guard ; les autres reçoivent
 * AuthGuard + RoleGuard, avec les rôles autorisés passés via `data.roles`
 * (lu par le RoleGuard).
 */
function toRoute(item: NavItem): Route {
  if (item.public) {
    return { path: item.path, component: item.component };
  }
  return {
    path: item.path,
    component: item.component,
    canActivate: [AuthGuard, RoleGuard],
    data: { roles: item.roles ?? [] }
  };
}

/** Routes générées depuis la source unique NAV (cf. nav.config.ts). */
export const routes: Routes = [
  ...NAV.map(toRoute),
  { path: '**', redirectTo: '' }
];
