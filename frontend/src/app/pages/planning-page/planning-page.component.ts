import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PlanningService } from '../../services/api/planning.service';
import { Planning } from '../../services/api/models/planning.model';
import { AuthStateService } from '../../services/auth-state.service';

@Component({
  selector: 'app-planning-page',
  imports: [CommonModule],
  templateUrl: './planning-page.component.html',
  styleUrl: './planning-page.component.css'
})
export class PlanningPageComponent implements OnInit {
  plannings: Planning[] = [];
  error: string = '';
  loading: boolean = false;

  constructor(
    private planningService: PlanningService,
    private authStateService: AuthStateService
  ) {}

  ngOnInit(): void {
    this.loading = true;
    this.planningService.getPlannings().subscribe({
      next: plannings => {
        this.plannings = plannings;
        this.loading = false;
      },
      error: err => {
        if (err.status === 401) {
          this.error = 'Session expirée. Veuillez vous reconnecter.';
          this.authStateService.logout();
        } else {
          this.error = 'Erreur lors du chargement du planning.';
        }
        this.loading = false;
      }
    });
  }
}
