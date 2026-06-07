import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
import { AuthStateService } from '../services/auth-state.service';

@Injectable({
  providedIn: 'root'
})
export class RoleGuard implements CanActivate {
  constructor(private authState: AuthStateService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot): boolean {
    const token = this.authState.currentUserTokenValue;
    if (!token) {
      this.router.navigate(['/login']);
      return false;
    }

    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      const userRole: string =
        payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
        payload['role'] ??
        '';
      const requiredRoles: string[] = route.data['roles'] ?? [];

      if (requiredRoles.length === 0 || requiredRoles.includes(userRole)) {
        return true;
      }
    } catch {
      // token invalide
    }

    this.router.navigate(['/']);
    return false;
  }
}
