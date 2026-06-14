/**
 * Données du formulaire de changement de mot de passe.
 * Seul `nouveauMotDePasse` est transmis au backend ; `motDePasseActuel` et
 * `confirmerMotDePasse` servent à la validation côté client.
 */
export interface ChangePasswordRequest {
  motDePasseActuel: string;
  nouveauMotDePasse: string;
  confirmerMotDePasse: string;
}
