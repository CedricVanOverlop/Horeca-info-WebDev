/**
 * Corps de la requête PUT /api/utilisateurs/me (mise à jour des infos personnelles).
 * L'identifiant n'est pas envoyé : le backend l'extrait du token JWT.
 */
export interface UpdateUserRequest {
  nom: string;
  prenom: string;
  email: string;
  telephone: string;
}
