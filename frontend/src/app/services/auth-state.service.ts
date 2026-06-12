import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { AuthService } from './api/auth.service';
import { AuthenticationRequest } from './api/models/authentication-request.model';
import { RegisterRequest } from './api/models/register-request.model';
import { LoginResponse } from './api/models/login-response.model';
import { RegisterResponse } from './api/models/register-response.model';

/**
 * Gère l'état d'authentification en mémoire : stockage du token JWT,
 * décodage des claims, détection d'expiration et navigation post-auth.
 */
@Injectable({
  providedIn: 'root'
})
export class AuthStateService {
  private readonly TOKEN_KEY = 'authToken';
  private _token: string | null = null;

  constructor(
    private authService: AuthService,
    private router: Router
  ) {
    this._token = localStorage.getItem(this.TOKEN_KEY);
  }

  public get currentUserTokenValue(): string | null {
    return this._token;
  }

  /**
   * Vrai si un token est présent ET non expiré.
   */
  public get isLoggedIn(): boolean {
    const payload = this.decodeToken();
    if (!payload) return false;
    const exp = payload['exp'];
    if (typeof exp === 'number' && exp * 1000 <= Date.now()) {
      this.clearToken();
      return false;
    }
    return true;
  }

  /**
   * Rôle de l'utilisateur courant extrait du token, ou null.
   */
  public get currentUserRole(): string | null {
    const payload = this.decodeToken();
    if (!payload) return null;
    return (
      payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
      payload['role'] ??
      null
    );
  }

  /**
   * Authentifie l'utilisateur, stocke le token et redirige vers l'accueil.
   * Retourne l'Observable pour que l'appelant gère les erreurs serveur.
   * @param credentials Email et mot de passe
   */
  login(credentials: AuthenticationRequest): Observable<LoginResponse> {
    return this.authService.login(credentials).pipe(
      tap(response => {
        if (response && response.token) {
          localStorage.setItem(this.TOKEN_KEY, response.token);
          this._token = response.token;
          this.router.navigate(['/']);
        }
      })
    );
  }

  /**
   * Inscrit un nouvel utilisateur puis redirige vers la page de connexion.
   * Retourne l'Observable pour que l'appelant gère les erreurs serveur.
   * @param credentials Données d'inscription
   */
  register(credentials: RegisterRequest): Observable<RegisterResponse> {
    return this.authService.register(credentials).pipe(
      tap(() => this.router.navigate(['/login']))
    );
  }

  logout(): void {
    this.clearToken();
    this.router.navigate(['/login']);
  }

  /**
   * Décode le payload du JWT. Retourne null si absent ou malformé.
   */
  private decodeToken(): Record<string, any> | null {
    if (!this._token) return null;
    try {
      return JSON.parse(atob(this._token.split('.')[1]));
    } catch {
      return null;
    }
  }

  private clearToken(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    this._token = null;
  }

  /**
   * Construit les headers HTTP avec le token Bearer pour les appels protégés.
   */
  public getAuthHeaders(): HttpHeaders {
    const token = this.currentUserTokenValue;
    if (!token) {
      console.error('No token found for authorizing request');
      return new HttpHeaders();
    }
    return new HttpHeaders({
      'Authorization': `Bearer ${token}`
    });
  }
}
