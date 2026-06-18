/** Terrain de padel renvoyé par l'API. Les heures sont au format "HH:mm:ss". */
export interface Terrain {
  id: string;
  nom: string;
  type: string;
  disponible: boolean;
  heureOuverture: string;
  heureFermeture: string;
  idCommerce: number;
}

/** Corps de requête pour créer un terrain (admin uniquement). */
export interface CreerTerrainRequest {
  nom: string;
  heureOuverture: string;
  heureFermeture: string;
  idCommerce: number;
}

/** Corps de requête pour modifier un terrain (nom + horaires, admin uniquement). */
export type ModifierTerrainRequest = Omit<CreerTerrainRequest, 'idCommerce'>;
