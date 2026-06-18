import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { PadelService } from '../../services/api/padel.service';
import { Terrain } from '../../services/api/models/terrain.model';
import { MoyenPaiement } from '../../services/api/models/reservation.model';
import { UtilisateurRecherche } from '../../services/api/models/utilisateur-recherche.model';
import { AuthStateService } from '../../services/auth-state.service';

/**
 * Carte « Réserver » : réservation manuelle par le personnel (admin/cuisine),
 * rattachée à un compte client existant recherché par nom/prénom/email.
 */
@Component({
  selector: 'app-reserver-card',
  templateUrl: './reserver-card.component.html',
  styleUrl: './reserver-card.component.css'
})
export class ReserverCardComponent implements OnInit {
  private readonly padel = inject(PadelService);
  private readonly authState = inject(AuthStateService);

  readonly terrains = signal<Terrain[]>([]);
  readonly mTerrainId = signal('');
  readonly mDate = signal('');
  readonly mHeure = signal('');
  readonly mDuree = signal<60 | 90 | 120>(60);
  readonly mMoyen = signal<MoyenPaiement>('SurPlace');
  readonly mMessage = signal('');
  readonly recherche = signal('');
  readonly resultats = signal<UtilisateurRecherche[]>([]);
  readonly clientChoisi = signal<UtilisateurRecherche | null>(null);
  readonly mSaving = signal(false);
  readonly mError = signal('');
  readonly mSuccess = signal('');

  readonly terrainsActifs = computed(() => this.terrains().filter(t => t.disponible));

  ngOnInit(): void {
    this.padel.getTerrains().subscribe({
      next: terrains => {
        this.terrains.set(terrains);
        const actifs = this.terrainsActifs();
        if (!this.mTerrainId() && actifs.length) this.mTerrainId.set(actifs[0].id);
      },
      error: err => { if (err.status === 401) this.authState.logout(); }
    });
  }

  /** Recherche d'utilisateurs (min. 2 caractères). */
  onRecherche(valeur: string): void {
    this.recherche.set(valeur);
    this.clientChoisi.set(null);
    if (valeur.trim().length < 2) {
      this.resultats.set([]);
      return;
    }
    this.padel.rechercherUtilisateurs(valeur.trim()).subscribe({
      next: users => this.resultats.set(users),
      error: () => this.resultats.set([])
    });
  }

  choisirClient(user: UtilisateurRecherche): void {
    this.clientChoisi.set(user);
    this.resultats.set([]);
    this.recherche.set(`${user.prenom} ${user.nom}`);
  }

  /** Crée la réservation manuelle rattachée au client choisi. */
  reserverManuel(): void {
    const client = this.clientChoisi();
    if (!client) { this.mError.set('Sélectionnez un client.'); return; }
    if (!this.mTerrainId() || !this.mDate() || !this.mHeure()) {
      this.mError.set('Terrain, date et heure sont obligatoires.');
      return;
    }

    this.mSaving.set(true);
    this.mError.set('');
    this.mSuccess.set('');
    this.padel.creerReservationManuelle(client.id, {
      terrainId: this.mTerrainId(),
      date: this.mDate(),
      heureDebut: `${this.mHeure()}:00`,
      heureFin: this.ajouterMinutes(this.mHeure(), this.mDuree()),
      moyenPaiement: this.mMoyen(),
      remarques: this.mMessage().trim() || null
    }).subscribe({
      next: () => {
        this.mSaving.set(false);
        this.mSuccess.set(`Réservation créée pour ${client.prenom} ${client.nom}.`);
        this.mMessage.set('');
        this.clientChoisi.set(null);
        this.recherche.set('');
      },
      error: err => {
        this.mSaving.set(false);
        this.mError.set(err.error?.message ?? 'Réservation impossible.');
      }
    });
  }

  /** "HH:mm" + minutes → "HH:mm:ss". */
  private ajouterMinutes(heure: string, minutes: number): string {
    const [h, m] = heure.split(':').map(Number);
    const total = h * 60 + m + minutes;
    const hh = Math.floor(total / 60) % 24;
    const mm = total % 60;
    return `${String(hh).padStart(2, '0')}:${String(mm).padStart(2, '0')}:00`;
  }
}
