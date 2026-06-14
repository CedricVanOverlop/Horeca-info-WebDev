import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthStateService } from '../auth-state.service';
import {
  UserAdminDto,
  ChangeRoleRequest,
  AjustementPointsRequest,
  ReservationAdmin,
  HoraireAdmin,
} from './models/utilisateur-admin.model';

/**
 * Appels HTTP de la console d'administration des utilisateurs.
 * Toutes les routes sont protégées par la policy AdminOnly côté serveur.
 */
@Injectable({
  providedIn: 'root'
})
export class UtilisateurAdminService {
  private readonly http = inject(HttpClient);
  private readonly authState = inject(AuthStateService);
  private readonly apiUrl = `${environment.baseUrl}/api/utilisateurs`;

  /** Charge tous les utilisateurs (actifs et bloqués) avec leur rôle. */
  getAll(): Observable<UserAdminDto[]> {
    return this.http.get<UserAdminDto[]>(`${this.apiUrl}/admin`, {
      headers: this.authState.getAuthHeaders()
    });
  }

  /**
   * Change le niveau d'accès d'un utilisateur.
   * @param id Identifiant de l'utilisateur.
   * @param request Niveau d'accès cible.
   */
  changeRole(id: number, request: ChangeRoleRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/role`, request, {
      headers: this.authState.getAuthHeaders()
    });
  }

  /**
   * Ajuste le solde de points d'un utilisateur.
   * @param id Identifiant de l'utilisateur.
   * @param request Montant signé et motif.
   */
  adjustPoints(id: number, request: AjustementPointsRequest): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${id}/points`, request, {
      headers: this.authState.getAuthHeaders()
    });
  }

  /** Bloque un compte (empêche la connexion). */
  block(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/bloquer`, {}, {
      headers: this.authState.getAuthHeaders()
    });
  }

  /** Débloque un compte. */
  unblock(id: number): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${id}/debloquer`, {}, {
      headers: this.authState.getAuthHeaders()
    });
  }

  /** Supprime (soft-delete) un compte en tant qu'administrateur. */
  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`, {
      headers: this.authState.getAuthHeaders()
    });
  }

  /** Récupère les réservations d'un utilisateur. */
  getReservations(id: number): Observable<ReservationAdmin[]> {
    return this.http.get<ReservationAdmin[]>(`${this.apiUrl}/${id}/reservations`, {
      headers: this.authState.getAuthHeaders()
    });
  }

  /** Récupère les horaires de travail d'un utilisateur employé. */
  getHoraires(id: number): Observable<HoraireAdmin[]> {
    return this.http.get<HoraireAdmin[]>(`${this.apiUrl}/${id}/horaires`, {
      headers: this.authState.getAuthHeaders()
    });
  }
}
