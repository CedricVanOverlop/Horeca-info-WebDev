import { Component, OnInit, signal } from '@angular/core';
import { PadelService } from '../../services/api/padel.service';
import { Terrain } from '../../services/api/models/terrain.model';
import { AuthStateService } from '../../services/auth-state.service';
import { NavbarComponent } from '../../components/navbar/navbar.component';

@Component({
  selector: 'app-padel-page',
  imports: [NavbarComponent],
  templateUrl: './padel-page.component.html',
  styleUrl: './padel-page.component.css'
})
export class PadelPageComponent implements OnInit {
  readonly terrains = signal<Terrain[]>([]);
  readonly error = signal('');
  readonly loading = signal(false);

  constructor(
    private padelService: PadelService,
    private authStateService: AuthStateService
  ) {}

  ngOnInit(): void {
    this.loading.set(true);
    this.padelService.getTerrains().subscribe({
      next: terrains => {
        this.terrains.set(terrains);
        this.loading.set(false);
      },
      error: err => {
        if (err.status === 401) {
          this.error.set('Session expirée. Veuillez vous reconnecter.');
          this.authStateService.logout();
        } else {
          this.error.set('Erreur lors du chargement des terrains.');
        }
        this.loading.set(false);
      }
    });
  }
}
