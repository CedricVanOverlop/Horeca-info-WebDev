import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Terrain, CreerTerrainRequest, ModifierTerrainRequest } from './models/terrain.model';
import { Reservation, CreerReservationRequest, CreneauOccupe, ReservationAdmin } from './models/reservation.model';
import { Tarif, TarifRequest } from './models/tarif.model';
import { UtilisateurRecherche } from './models/utilisateur-recherche.model';
import { AuthStateService } from '../auth-state.service';

/**
 * Accès HTTP au module Padel : terrains, tarifs, disponibilités et réservations
 * (client + manuelle). Le header Authorization est ajouté via AuthStateService.
 */
@Injectable({ providedIn: 'root' })
export class PadelService {
  private readonly http = inject(HttpClient);
  private readonly authState = inject(AuthStateService);
  private readonly apiUrl = `${environment.baseUrl}/api/padel`;

  private get options() {
    return { headers: this.authState.getAuthHeaders() };
  }

  // ── Terrains ────────────────────────────────────────────────

  /** Liste tous les terrains (actifs et inactifs). */
  getTerrains(): Observable<Terrain[]> {
    return this.http.get<Terrain[]>(`${this.apiUrl}/terrains`, this.options);
  }

  /** Crée un terrain (admin uniquement). */
  creerTerrain(request: CreerTerrainRequest): Observable<Terrain> {
    return this.http.post<Terrain>(`${this.apiUrl}/terrains`, request, this.options);
  }

  /** Modifie le nom et les horaires d'un terrain (admin uniquement). */
  modifierTerrain(terrainId: string, request: ModifierTerrainRequest): Observable<Terrain> {
    return this.http.put<Terrain>(`${this.apiUrl}/terrains/${terrainId}`, request, this.options);
  }

  /** Active ou désactive un terrain (admin/cuisine). */
  toggleTerrainActif(terrainId: string, actif: boolean): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/terrains/${terrainId}/actif`, { actif }, this.options);
  }

  // ── Tarifs ──────────────────────────────────────────────────

  /** Grille tarifaire d'un terrain. */
  getTarifs(terrainId: string): Observable<Tarif[]> {
    return this.http.get<Tarif[]>(`${this.apiUrl}/terrains/${terrainId}/tarifs`, this.options);
  }

  /** Crée un tarif (admin uniquement). */
  creerTarif(request: TarifRequest): Observable<Tarif> {
    return this.http.post<Tarif>(`${this.apiUrl}/tarifs`, request, this.options);
  }

  /** Modifie un tarif (admin uniquement). */
  modifierTarif(tarifId: string, request: TarifRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/tarifs/${tarifId}`, request, this.options);
  }

  /** Supprime un tarif (admin uniquement). */
  supprimerTarif(tarifId: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/tarifs/${tarifId}`, this.options);
  }

  // ── Disponibilité ───────────────────────────────────────────

  /**
   * Créneaux occupés d'un terrain entre deux dates (sans donnée nominative),
   * pour griser les cases prises dans la grille.
   * @param terrainId Identifiant du terrain
   * @param from Date de début incluse (YYYY-MM-DD)
   * @param to Date de fin incluse (YYYY-MM-DD)
   */
  getCreneauxOccupes(terrainId: string, from: string, to: string): Observable<CreneauOccupe[]> {
    const params = new HttpParams().set('from', from).set('to', to);
    return this.http.get<CreneauOccupe[]>(`${this.apiUrl}/terrains/${terrainId}/creneaux`,
      { ...this.options, params });
  }

  // ── Réservations client ─────────────────────────────────────

  /** Réservations du client connecté. */
  getMesReservations(): Observable<Reservation[]> {
    return this.http.get<Reservation[]>(`${this.apiUrl}/reservations/me`, this.options);
  }

  /** Toutes les réservations enrichies (terrain + client) — staff (admin/cuisine). */
  getReservationsAdmin(): Observable<ReservationAdmin[]> {
    return this.http.get<ReservationAdmin[]>(`${this.apiUrl}/reservations/admin`, this.options);
  }

  /** Crée une réservation pour le client connecté. */
  creerReservation(request: CreerReservationRequest): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.apiUrl}/reservations`, request, this.options);
  }

  /** Annule (supprime) une réservation. */
  annulerReservation(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/reservations/${id}`, this.options);
  }

  // ── Réservation manuelle (admin/cuisine) ────────────────────

  /** Recherche des utilisateurs (nom/prénom/email) pour rattacher une réservation manuelle. */
  rechercherUtilisateurs(q: string): Observable<UtilisateurRecherche[]> {
    const params = new HttpParams().set('q', q);
    return this.http.get<UtilisateurRecherche[]>(`${this.apiUrl}/utilisateurs/recherche`,
      { ...this.options, params });
  }

  /** Crée une réservation manuelle rattachée à un utilisateur existant. */
  creerReservationManuelle(idUtilisateur: number, request: CreerReservationRequest): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.apiUrl}/reservations/manuelle/${idUtilisateur}`, request, this.options);
  }
}
