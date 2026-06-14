import { Component, OnInit, signal } from '@angular/core';
import { FideliteService } from '../../services/api/fidelite.service';
import { CarteFidelite } from '../../services/api/models/carte-fidelite.model';
import { AuthStateService } from '../../services/auth-state.service';
import { NavbarComponent } from '../../components/navbar/navbar.component';

@Component({
  selector: 'app-fidelite-page',
  imports: [NavbarComponent],
  templateUrl: './fidelite-page.component.html',
  styleUrl: './fidelite-page.component.css'
})
export class FidelitePageComponent implements OnInit {
  readonly carte = signal<CarteFidelite | null>(null);
  readonly error = signal('');
  readonly loading = signal(false);

  constructor(
    private fideliteService: FideliteService,
    private authStateService: AuthStateService
  ) {}

  ngOnInit(): void {
    this.loading.set(true);
    this.fideliteService.getCarte().subscribe({
      next: carte => {
        this.carte.set(carte);
        this.loading.set(false);
      },
      error: err => {
        if (err.status === 401) {
          this.error.set('Session expirée. Veuillez vous reconnecter.');
          this.authStateService.logout();
        } else {
          this.error.set('Erreur lors du chargement de la carte fidélité.');
        }
        this.loading.set(false);
      }
    });
  }
}
