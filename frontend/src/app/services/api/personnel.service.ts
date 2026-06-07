import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Employe } from './models/employe.model';
import { AuthStateService } from '../auth-state.service';

@Injectable({
  providedIn: 'root'
})
export class PersonnelService {
  private apiUrl = `${environment.baseUrl}/api/personnel`;

  constructor(private http: HttpClient, private authStateService: AuthStateService) { }

  getEmployes(): Observable<Employe[]> {
    return this.http.get<Employe[]>(this.apiUrl, { headers: this.authStateService.getAuthHeaders() });
  }

  createEmploye(employe: Omit<Employe, 'id'>): Observable<Employe> {
    return this.http.post<Employe>(this.apiUrl, employe, { headers: this.authStateService.getAuthHeaders() });
  }

  deleteEmploye(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, { headers: this.authStateService.getAuthHeaders() });
  }
}
