import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FideliteService } from '../../services/api/fidelite.service';
import { CarteFidelite, Transaction } from '../../services/api/models/carte-fidelite.model';
import { AuthStateService } from '../../services/auth-state.service';

@Component({
  selector: 'app-fidelite-page',
  imports: [CommonModule],
  templateUrl: './fidelite-page.component.html',
  styleUrl: './fidelite-page.component.css'
})
export class FidelitePageComponent implements OnInit {
  carte: CarteFidelite | null = null;
  transactions: Transaction[] = [];
  error: string = '';
  loading: boolean = false;

  constructor(
    private fideliteService: FideliteService,
    private authStateService: AuthStateService
  ) {}

  ngOnInit(): void {
    this.loading = true;
    this.fideliteService.getCarte().subscribe({
      next: carte => {
        this.carte = carte;
        this.loading = false;
      },
      error: err => {
        if (err.status === 401) {
          this.error = 'Session expirée. Veuillez vous reconnecter.';
          this.authStateService.logout();
        } else {
          this.error = 'Erreur lors du chargement de la carte fidélité.';
        }
        this.loading = false;
      }
    });
  }
}
