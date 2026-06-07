import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Terrain } from './models/terrain.model';
import { Reservation, ReservationRequest } from './models/reservation.model';
import { AuthStateService } from '../auth-state.service';

@Injectable({
  providedIn: 'root'
})
export class PadelService {
  private apiUrl = `${environment.baseUrl}/api/padel`;

  constructor(private http: HttpClient, private authStateService: AuthStateService) { }

  getTerrains(): Observable<Terrain[]> {
    return this.http.get<Terrain[]>(`${this.apiUrl}/terrains`, { headers: this.authStateService.getAuthHeaders() });
  }

  getReservations(): Observable<Reservation[]> {
    return this.http.get<Reservation[]>(`${this.apiUrl}/reservations`, { headers: this.authStateService.getAuthHeaders() });
  }

  createReservation(request: ReservationRequest): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.apiUrl}/reservations`, request, { headers: this.authStateService.getAuthHeaders() });
  }

  deleteReservation(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/reservations/${id}`, { headers: this.authStateService.getAuthHeaders() });
  }
}
