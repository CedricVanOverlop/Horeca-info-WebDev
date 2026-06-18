import { Component, inject, signal } from '@angular/core';
import { AuthStateService } from '../../services/auth-state.service';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { ReserverCardComponent } from '../../components/reserver-card/reserver-card.component';
import { TerrainsCardComponent } from '../../components/terrains-card/terrains-card.component';
import { ReservationsCardComponent } from '../../components/reservations-card/reservations-card.component';
import { TarifsCardComponent } from '../../components/tarifs-card/tarifs-card.component';
import { Roles } from '../../roles';

/** Onglets de la page de gestion. */
type Onglet = 'reserver' | 'terrains' | 'reservations' | 'tarifs';

/**
 * Page de gestion des terrains (Cuisine + Administrateur). Coquille fine : barre
 * d'onglets dans la navbar et affichage de la carte correspondante. Chaque onglet
 * est un composant autonome (réservation manuelle, terrains, réservations, tarifs).
 */
@Component({
  selector: 'app-gestion-terrains-page',
  imports: [
    NavbarComponent,
    ReserverCardComponent,
    TerrainsCardComponent,
    ReservationsCardComponent,
    TarifsCardComponent
  ],
  templateUrl: './gestion-terrains-page.component.html',
  styleUrl: './gestion-terrains-page.component.css'
})
export class GestionTerrainsPageComponent {
  private readonly authState = inject(AuthStateService);

  readonly estAdmin = this.authState.currentUserRole === Roles.Administrateur;
  readonly onglet = signal<Onglet>('reserver');

  changerOnglet(o: Onglet): void {
    this.onglet.set(o);
  }
}
