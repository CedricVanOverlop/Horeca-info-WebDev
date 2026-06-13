/**
 * Source unique des noms de rôle de l'application.
 * Les valeurs correspondent exactement au claim `role` du JWT (champ `acces`
 * de la table EMPLOYE côté backend). Utiliser ces constantes partout plutôt
 * que des chaînes littérales pour éviter les fautes de frappe silencieuses.
 */
export const Roles = {
  Client: 'Client',
  Employe: 'Employe',
  Cuisine: 'Cuisine',
  Administrateur: 'Administrateur'
} as const;

/** Type union des rôles valides ('Client' | 'Employe' | 'Cuisine' | 'Administrateur'). */
export type Role = (typeof Roles)[keyof typeof Roles];

/** Tous les rôles — raccourci pour les pages accessibles à tout utilisateur connecté. */
export const ALL_ROLES: Role[] = [
  Roles.Client,
  Roles.Employe,
  Roles.Cuisine,
  Roles.Administrateur
];
