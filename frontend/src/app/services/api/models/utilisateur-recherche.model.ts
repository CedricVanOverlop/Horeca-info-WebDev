/** Utilisateur renvoyé par la recherche pour rattacher une réservation manuelle. */
export interface UtilisateurRecherche {
  id: number;
  nom: string;
  prenom: string;
  email: string;
  telephone: string;
  pointsSolde: number;
  role: string;
}
