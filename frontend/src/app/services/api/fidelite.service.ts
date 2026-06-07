import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CarteFidelite, Transaction } from './models/carte-fidelite.model';
import { AuthStateService } from '../auth-state.service';

@Injectable({
  providedIn: 'root'
})
export class FideliteService {
  private apiUrl = `${environment.baseUrl}/api/fidelite`;

  constructor(private http: HttpClient, private authStateService: AuthStateService) { }

  getCarte(): Observable<CarteFidelite> {
    return this.http.get<CarteFidelite>(this.apiUrl, { headers: this.authStateService.getAuthHeaders() });
  }

  getTransactions(): Observable<Transaction[]> {
    return this.http.get<Transaction[]>(`${this.apiUrl}/transactions`, { headers: this.authStateService.getAuthHeaders() });
  }
}
