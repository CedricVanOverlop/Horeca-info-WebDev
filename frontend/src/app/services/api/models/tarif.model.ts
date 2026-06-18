/** Tarif d'un terrain : prix horaire pour un jour ISO (1=lundi … 7=dimanche) et une plage horaire. */
export interface Tarif {
  id: string;
  type: string;
  prixHeure: number;
  heureDebut: string;
  heureFin: string;
  jourSemaine: number;
  terrainId: string;
}

/** Corps de requête pour créer/modifier un tarif. */
export interface TarifRequest {
  type: string;
  prixHeure: number;
  heureDebut: string;
  heureFin: string;
  jourSemaine: number;
  terrainId: string;
}
