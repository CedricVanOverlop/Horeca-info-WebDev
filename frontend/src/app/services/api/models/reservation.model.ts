export interface Reservation {
  id: string;
  userId: string;
  terrainId: string;
  dateDebut: string;
  dateFin: string;
  prix: number;
}

export interface ReservationRequest {
  terrainId: string;
  dateDebut: string;
  dateFin: string;
}
