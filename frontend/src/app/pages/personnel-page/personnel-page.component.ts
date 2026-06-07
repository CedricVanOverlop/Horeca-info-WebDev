import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PersonnelService } from '../../services/api/personnel.service';
import { Employe } from '../../services/api/models/employe.model';
import { AuthStateService } from '../../services/auth-state.service';

@Component({
  selector: 'app-personnel-page',
  imports: [CommonModule],
  templateUrl: './personnel-page.component.html',
  styleUrl: './personnel-page.component.css'
})
export class PersonnelPageComponent implements OnInit {
  employes: Employe[] = [];
  error: string = '';
  loading: boolean = false;

  constructor(
    private personnelService: PersonnelService,
    private authStateService: AuthStateService
  ) {}

  ngOnInit(): void {
    this.loading = true;
    this.personnelService.getEmployes().subscribe({
      next: employes => {
        this.employes = employes;
        this.loading = false;
      },
      error: err => {
        if (err.status === 401) {
          this.error = 'Session expirée. Veuillez vous reconnecter.';
          this.authStateService.logout();
        } else {
          this.error = 'Erreur lors du chargement du personnel.';
        }
        this.loading = false;
      }
    });
  }
}
