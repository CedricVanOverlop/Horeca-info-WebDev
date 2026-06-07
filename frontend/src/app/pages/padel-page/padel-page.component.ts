import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PadelService } from '../../services/api/padel.service';
import { Terrain } from '../../services/api/models/terrain.model';
import { AuthStateService } from '../../services/auth-state.service';

@Component({
  selector: 'app-padel-page',
  imports: [CommonModule],
  templateUrl: './padel-page.component.html',
  styleUrl: './padel-page.component.css'
})
export class PadelPageComponent implements OnInit {
  terrains: Terrain[] = [];
  error: string = '';
  loading: boolean = false;

  constructor(
    private padelService: PadelService,
    private authStateService: AuthStateService
  ) {}

  ngOnInit(): void {
    this.loading = true;
    this.padelService.getTerrains().subscribe({
      next: terrains => {
        this.terrains = terrains;
        this.loading = false;
      },
      error: err => {
        if (err.status === 401) {
          this.error = 'Session expirée. Veuillez vous reconnecter.';
          this.authStateService.logout();
        } else {
          this.error = 'Erreur lors du chargement des terrains.';
        }
        this.loading = false;
      }
    });
  }
}
