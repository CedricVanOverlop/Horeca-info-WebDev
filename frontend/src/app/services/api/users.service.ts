import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthStateService } from '../auth-state.service';
import { UserProfile } from './models/user-profile.model';
import { UpdateUserRequest } from './models/update-user-request.model';
import { ChangePasswordRequest } from './models/change-password-request.model';

/**
 * Service HTTP pour la gestion du compte de l'utilisateur courant.
 * Toutes les routes ciblent l'utilisateur identifié par le token JWT (/me).
 */
@Injectable({
  providedIn: 'root'
})
export class UsersService {
  private apiUrl = `${environment.baseUrl}/api/utilisateurs`;

  constructor(private http: HttpClient, private authStateService: AuthStateService) { }

  /**
   * Récupère le profil de l'utilisateur connecté.
   * @returns Le profil (nom, prénom, email, téléphone, ...).
   */
  getMyProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.apiUrl}/me`, {
      headers: this.authStateService.getAuthHeaders()
    });
  }

  /**
   * Met à jour les informations personnelles de l'utilisateur connecté.
   * @param request Nouvelles infos (nom, prénom, email, téléphone).
   */
  updateMyProfile(request: UpdateUserRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/me`, request, {
      headers: this.authStateService.getAuthHeaders()
    });
  }

  /**
   * Change le mot de passe de l'utilisateur connecté.
   * L'ancien mot de passe est vérifié côté serveur (BCrypt) avant l'écrasement.
   * `confirmerMotDePasse` ne sert qu'à la validation côté client.
   * @param request Données du formulaire de changement de mot de passe.
   */
  changeMyPassword(request: ChangePasswordRequest): Observable<void> {
    return this.http.put<void>(
      `${this.apiUrl}/me/password`,
      {
        ancienMotDePasse: request.motDePasseActuel,
        nouveauMotDePasse: request.nouveauMotDePasse
      },
      { headers: this.authStateService.getAuthHeaders() }
    );
  }

  /**
   * Supprime (soft-delete) le compte de l'utilisateur connecté.
   */
  deleteMyAccount(): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/me`, {
      headers: this.authStateService.getAuthHeaders()
    });
  }
}
