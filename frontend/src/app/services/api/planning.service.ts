import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Planning } from './models/planning.model';
import { AuthStateService } from '../auth-state.service';

@Injectable({
  providedIn: 'root'
})
export class PlanningService {
  private apiUrl = `${environment.baseUrl}/api/planning`;

  constructor(private http: HttpClient, private authStateService: AuthStateService) { }

  getPlannings(): Observable<Planning[]> {
    return this.http.get<Planning[]>(this.apiUrl, { headers: this.authStateService.getAuthHeaders() });
  }

  createPlanning(planning: Omit<Planning, 'id'>): Observable<Planning> {
    return this.http.post<Planning>(this.apiUrl, planning, { headers: this.authStateService.getAuthHeaders() });
  }

  deletePlanning(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, { headers: this.authStateService.getAuthHeaders() });
  }
}
