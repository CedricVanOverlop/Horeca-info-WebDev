import { Injectable } from '@angular/core';
import { AuthService } from './api/auth.service';
import { AuthenticationRequest } from './api/models/authentication-request.model';
import { RegisterRequest } from './api/models/register-request.model';
import { Router } from '@angular/router';
import { HttpHeaders } from '@angular/common/http';

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

  public get isLoggedIn(): boolean {
    return !!this._token;
  }

  login(credentials: AuthenticationRequest) {
    this.authService.login(credentials).subscribe({
      next: response => {
        if (response && response.token) {
          localStorage.setItem(this.TOKEN_KEY, response.token);
          this._token = response.token;
          this.router.navigate(['/']);
        }
      },
      error: err => {
        console.error(err);
      }
    });
  }

  register(credentials: RegisterRequest) {
    this.authService.register(credentials).subscribe({
      next: _ => {
        this.router.navigate(['/login']);
      },
      error: err => {
        console.error(err);
      }
    });
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    this._token = null;
    this.router.navigate(['/login']);
  }

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
