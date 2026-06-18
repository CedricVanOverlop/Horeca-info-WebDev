/** Réservation renvoyée par l'API. date = "YYYY-MM-DDT00:00:00", heures = "HH:mm:ss". */
export interface Reservation {
  id: string;
  userId: number;
  terrainId: string;
  tarifId: string;
  date: string;
  heureDebut: string;
  heureFin: string;
  prixPaye: number;
  moyenPaiement: string;
  remarques: string | null;
  dateReservation: string;
}

/** Moyen de paiement (informatif, pas de paiement réel). */
export type MoyenPaiement = 'EnLigne' | 'SurPlace';

/**
 * Corps de requête pour créer une réservation (client ou manuelle).
 * Le prix et le tarif sont calculés côté serveur — jamais envoyés par le client.
 */
export interface CreerReservationRequest {
  terrainId: string;
  date: string;
  heureDebut: string;
  heureFin: string;
  moyenPaiement: MoyenPaiement;
  remarques?: string | null;
}

/** Créneau occupé d'un terrain (sans donnée nominative), pour la grille de disponibilité. */
export interface CreneauOccupe {
  date: string;
  heureDebut: string;
  heureFin: string;
}

/**
 * Réservation enrichie pour la vue staff (admin/cuisine) : nom du terrain et identité
 * du client, afin de consulter et annuler n'importe quelle réservation.
 */
export interface ReservationAdmin {
  id: number;
  date: string;
  heureDebut: string;
  heureFin: string;
  prixPaye: number;
  terrain: string;
  client: string;
  clientEmail: string;
  moyenPaiement: string;
  remarques: string | null;
}
