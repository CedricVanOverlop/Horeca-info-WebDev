import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthenticationRequest } from './models/authentication-request.model';
import { LoginResponse } from './models/login-response.model';
import { RegisterRequest } from './models/register-request.model';
import { RegisterResponse } from './models/register-response.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.baseUrl}/api/users`;

  constructor(private http: HttpClient) { }

  login(credentials: AuthenticationRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/auth`, credentials);
  }

  register(userInfo: RegisterRequest): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.apiUrl}/register`, userInfo);
  }
}
