export interface CarteFidelite {
  id: string;
  userId: string;
  points: number;
}

export interface Transaction {
  id: string;
  carteFideliteId: string;
  points: number;
  description: string;
  createdAt: string;
}
