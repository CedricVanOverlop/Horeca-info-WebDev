import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { PadelService } from '../../services/api/padel.service';
import { Terrain } from '../../services/api/models/terrain.model';
import { ReservationAdmin } from '../../services/api/models/reservation.model';
import { AuthStateService } from '../../services/auth-state.service';
import { formatHeure, formatDate } from '../../shared/format.util';

/**
 * Carte « Réservations » (vue staff) : liste toutes les réservations (terrain + client),
 * filtrable par terrain, et permet d'annuler n'importe laquelle, y compris celles des clients.
 */
@Component({
  selector: 'app-reservations-card',
  templateUrl: './reservations-card.component.html',
  styleUrl: './reservations-card.component.css'
})
export class ReservationsCardComponent implements OnInit {
  private readonly padel = inject(PadelService);
  private readonly authState = inject(AuthStateService);

  readonly terrains = signal<Terrain[]>([]);
  readonly reservationsAdmin = signal<ReservationAdmin[]>([]);
  readonly resaLoading = signal(false);
  readonly resaError = signal('');
  readonly resaFiltreTerrain = signal(''); // '' = tous
  readonly formatHeure = formatHeure;
  readonly formatDate = formatDate;

  readonly reservationsFiltrees = computed(() => {
    const f = this.resaFiltreTerrain();
    const list = this.reservationsAdmin();
    return f ? list.filter(r => r.terrain === f) : list;
  });

  ngOnInit(): void {
    this.padel.getTerrains().subscribe({
      next: terrains => this.terrains.set(terrains),
      error: () => { /* le filtre terrain reste vide, la liste globale reste utilisable */ }
    });
    this.chargerReservationsAdmin();
  }

  /** Charge toutes les réservations (terrain + client) pour la vue staff. */
  chargerReservationsAdmin(): void {
    this.resaLoading.set(true);
    this.resaError.set('');
    this.padel.getReservationsAdmin().subscribe({
      next: list => { this.reservationsAdmin.set(list); this.resaLoading.set(false); },
      error: err => {
        this.resaLoading.set(false);
        if (err.status === 401) this.authState.logout();
        else this.resaError.set('Erreur lors du chargement des réservations.');
      }
    });
  }

  /** Annule (supprime) une réservation, y compris celle d'un client (staff : sans restriction). */
  annulerResa(r: ReservationAdmin): void {
    if (!confirm(`Annuler la réservation de ${r.client} le ${this.formatDate(r.date)} (${this.formatHeure(r.heureDebut)}–${this.formatHeure(r.heureFin)}) ?`)) {
      return;
    }
    this.resaError.set('');
    this.padel.annulerReservation(String(r.id)).subscribe({
      next: () => this.chargerReservationsAdmin(),
      error: err => this.resaError.set(err.error?.message ?? 'Annulation impossible.')
    });
  }

  /** Vrai si le créneau est dans le futur (pour distinguer à venir / passé). */
  estAVenir(r: ReservationAdmin): boolean {
    return new Date(`${r.date.slice(0, 10)}T${r.heureDebut}`) > new Date();
  }
}
