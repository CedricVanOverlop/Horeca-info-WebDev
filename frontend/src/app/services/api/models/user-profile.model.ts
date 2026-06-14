/**
 * Profil de l'utilisateur courant, renvoyé par GET /api/utilisateurs/me.
 */
export interface UserProfile {
  id: number;
  nom: string;
  prenom: string;
  email: string;
  telephone?: string;
  pointsSolde?: number;
  role?: string;
  /** Date d'inscription (ISO). Optionnelle : dépend d'une colonne de date côté DB. */
  membreDepuis?: string;
}
