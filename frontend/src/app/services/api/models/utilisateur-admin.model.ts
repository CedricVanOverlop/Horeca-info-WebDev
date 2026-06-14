/** Niveau d'accès d'un utilisateur. */
export type RoleAdmin = 'Client' | 'Employe' | 'Cuisine' | 'Administrateur';

/**
 * Vue administrateur d'un utilisateur (GET /api/utilisateurs/admin).
 * `actif = false` signifie compte bloqué.
 */
export interface UserAdminDto {
  id: number;
  nom: string;
  prenom: string;
  email: string;
  pointsSolde: number;
  role: RoleAdmin;
  actif: boolean;
}

/** Corps de PUT /api/utilisateurs/{id}/role. */
export interface ChangeRoleRequest {
  acces: RoleAdmin;
}

/** Corps de POST /api/utilisateurs/{id}/points (montant signé, motif obligatoire). */
export interface AjustementPointsRequest {
  montant: number;
  motif: string;
}

/** Réservation d'un utilisateur (GET /api/utilisateurs/{id}/reservations). */
export interface ReservationAdmin {
  id: number;
  date: string;
  heureDebut: string;
  heureFin: string;
  prixPaye: number;
  terrain: string;
}

/** Horaire de travail d'un employé (GET /api/utilisateurs/{id}/horaires). */
export interface HoraireAdmin {
  id: number;
  date: string;
  heureDebut: string;
  heureFin: string;
  heurePayee: number;
  statut: string;
  commerce: string;
}
