import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { PadelService } from '../../services/api/padel.service';
import { Terrain } from '../../services/api/models/terrain.model';
import { Reservation, CreneauOccupe, MoyenPaiement } from '../../services/api/models/reservation.model';
import { AuthStateService } from '../../services/auth-state.service';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { formatHeure } from '../../shared/format.util';

/** Durées de réservation proposées, en minutes. */
type Duree = 60 | 90 | 120;

/** Créneau cliqué dans la grille (jour + minute de début). */
interface SlotChoisi {
  jour: Date;
  heure: number;
}

const JOURS_FR = ['Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam', 'Dim'];
const JOURS_LONG = ['lundi', 'mardi', 'mercredi', 'jeudi', 'vendredi', 'samedi', 'dimanche'];
const MOIS_FR = ['janvier', 'février', 'mars', 'avril', 'mai', 'juin',
  'juillet', 'août', 'septembre', 'octobre', 'novembre', 'décembre'];

/**
 * Page de réservation client : grille hebdomadaire par terrain, sélection d'un créneau
 * libre, confirmation (durée / paiement / message), liste des réservations et annulation
 * (avec récapitulatif téléchargeable).
 */
@Component({
  selector: 'app-padel-page',
  imports: [NavbarComponent],
  templateUrl: './padel-page.component.html',
  styleUrl: './padel-page.component.css'
})
export class PadelPageComponent implements OnInit {
  private readonly padel = inject(PadelService);
  private readonly authState = inject(AuthStateService);

  // ── État général ──────────────────────────────────────────
  readonly terrains = signal<Terrain[]>([]);
  readonly terrainSelectionne = signal<Terrain | null>(null);
  readonly semaineOffset = signal(0);
  readonly creneaux = signal<CreneauOccupe[]>([]);
  readonly mesReservations = signal<Reservation[]>([]);
  readonly loading = signal(false);
  readonly error = signal('');

  // ── Modal de réservation ──────────────────────────────────
  readonly slot = signal<SlotChoisi | null>(null);
  readonly duree = signal<Duree>(60);
  readonly moyenPaiement = signal<MoyenPaiement>('EnLigne');
  readonly message = signal('');
  readonly saving = signal(false);
  readonly modalError = signal('');

  // ── Annulation ────────────────────────────────────────────
  readonly resaAAnnuler = signal<Reservation | null>(null);
  readonly annulSaving = signal(false);
  readonly annulError = signal('');
  readonly recapAnnulation = signal<string | null>(null);

  /** Terrains réservables (actifs uniquement) pour les onglets client. */
  readonly terrainsActifs = computed(() => this.terrains().filter(t => t.disponible));

  /** Les 7 dates (lundi → dimanche) de la semaine affichée. */
  readonly joursSemaine = computed<Date[]>(() => {
    const lundi = this.lundiDeLaSemaine(this.semaineOffset());
    return Array.from({ length: 7 }, (_, i) => {
      const d = new Date(lundi);
      d.setDate(lundi.getDate() + i);
      return d;
    });
  });

  /**
   * Minutes de début de chaque ligne horaire (pas de 30 min : créneaux à l'heure pile
   * et à la demi-heure). On garde une durée minimale d'1h avant la fermeture.
   */
  readonly heures = computed<number[]>(() => {
    const t = this.terrainSelectionne();
    if (!t) return [];
    const debut = this.parseTime(t.heureOuverture);
    const fin = this.parseTime(t.heureFermeture);
    const out: number[] = [];
    for (let m = debut; m + 60 <= fin; m += 30) out.push(m);
    return out;
  });

  /** Libellé de la semaine affichée (ex. "14 – 20 avril 2025"). */
  readonly libelleSemaine = computed(() => {
    const jours = this.joursSemaine();
    const debut = jours[0];
    const fin = jours[6];
    return `${debut.getDate()} – ${fin.getDate()} ${MOIS_FR[fin.getMonth()]} ${fin.getFullYear()}`;
  });

  ngOnInit(): void {
    this.loading.set(true);
    this.padel.getTerrains().subscribe({
      next: terrains => {
        this.terrains.set(terrains);
        const premier = terrains.find(t => t.disponible) ?? null;
        this.terrainSelectionne.set(premier);
        this.loading.set(false);
        if (premier) this.chargerCreneaux();
      },
      error: err => this.gererErreurChargement(err)
    });
    this.chargerMesReservations();
  }

  // ── Navigation ────────────────────────────────────────────

  /** Sélectionne un terrain et recharge sa disponibilité. */
  selectionnerTerrain(terrain: Terrain): void {
    this.terrainSelectionne.set(terrain);
    this.chargerCreneaux();
  }

  /** Semaine précédente. */
  semainePrecedente(): void {
    this.semaineOffset.update(o => o - 1);
    this.chargerCreneaux();
  }

  /** Semaine suivante. */
  semaineSuivante(): void {
    this.semaineOffset.update(o => o + 1);
    this.chargerCreneaux();
  }

  // ── Grille ────────────────────────────────────────────────

  /** Vrai si le créneau (jour + heure) chevauche une réservation existante. */
  estOccupe(jour: Date, heure: number): boolean {
    const ds = this.toDateStr(jour);
    return this.creneaux().some(c =>
      c.date.slice(0, 10) === ds
      && this.parseTime(c.heureDebut) < heure + 60
      && heure < this.parseTime(c.heureFin));
  }

  /** Vrai si le créneau est déjà passé. */
  estPasse(jour: Date, heure: number): boolean {
    const d = new Date(jour);
    d.setHours(Math.floor(heure / 60), heure % 60, 0, 0);
    return d.getTime() < Date.now();
  }

  /** Vrai si la date dépasse l'horizon de réservation client (15 jours à l'avance). */
  estTropLoin(jour: Date): boolean {
    const limite = new Date();
    limite.setHours(0, 0, 0, 0);
    limite.setDate(limite.getDate() + 15);
    const j = new Date(jour);
    j.setHours(0, 0, 0, 0);
    return j.getTime() > limite.getTime();
  }

  /** Ouvre la modal de confirmation sur un créneau libre. */
  ouvrirReservation(jour: Date, heure: number): void {
    if (this.estOccupe(jour, heure) || this.estPasse(jour, heure) || this.estTropLoin(jour)) return;
    this.slot.set({ jour, heure });
    this.duree.set(60);
    this.moyenPaiement.set('EnLigne');
    this.message.set('');
    this.modalError.set('');
  }

  fermerModal(): void {
    this.slot.set(null);
  }

  // ── Confirmation de réservation ───────────────────────────

  /** Récapitulatif affiché en tête de modal ("Mercredi 16 avril · 10h00 → 11h00 · Terrain 2"). */
  readonly recapModal = computed(() => {
    const s = this.slot();
    const t = this.terrainSelectionne();
    if (!s || !t) return '';
    const fin = s.heure + this.duree();
    return `${this.formatDateLongue(s.jour)} · ${this.formatHeureLisible(s.heure)} → ${this.formatHeureLisible(fin)} · ${t.nom}`;
  });

  /** Envoie la réservation au serveur (prix calculé côté serveur). */
  confirmerReservation(): void {
    const s = this.slot();
    const t = this.terrainSelectionne();
    if (!s || !t) return;

    this.saving.set(true);
    this.modalError.set('');
    this.padel.creerReservation({
      terrainId: t.id,
      date: this.toDateStr(s.jour),
      heureDebut: this.formatTime(s.heure),
      heureFin: this.formatTime(s.heure + this.duree()),
      moyenPaiement: this.moyenPaiement(),
      remarques: this.message().trim() || null
    }).subscribe({
      next: () => {
        this.saving.set(false);
        this.fermerModal();
        this.chargerCreneaux();
        this.chargerMesReservations();
      },
      error: err => {
        this.saving.set(false);
        this.modalError.set(err.error?.message ?? 'Réservation impossible.');
      }
    });
  }

  // ── Annulation ────────────────────────────────────────────

  demanderAnnulation(resa: Reservation): void {
    this.resaAAnnuler.set(resa);
    this.recapAnnulation.set(null);
    this.annulError.set('');
  }

  fermerAnnulation(): void {
    this.resaAAnnuler.set(null);
    this.recapAnnulation.set(null);
  }

  /** Confirme l'annulation puis prépare le récapitulatif téléchargeable. */
  confirmerAnnulation(): void {
    const r = this.resaAAnnuler();
    if (!r) return;

    this.annulSaving.set(true);
    this.annulError.set('');
    this.padel.annulerReservation(r.id).subscribe({
      next: () => {
        this.annulSaving.set(false);
        this.recapAnnulation.set(this.construireRecap(r));
        this.chargerCreneaux();
        this.chargerMesReservations();
      },
      error: err => {
        this.annulSaving.set(false);
        this.annulError.set(err.error?.message ?? 'Annulation impossible.');
      }
    });
  }

  /** Déclenche le téléchargement du récapitulatif d'annulation en .txt. */
  telechargerRecap(): void {
    const contenu = this.recapAnnulation();
    if (!contenu) return;
    const blob = new Blob([contenu], { type: 'text/plain;charset=utf-8' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'reservation-annulee.txt';
    a.click();
    URL.revokeObjectURL(url);
  }

  // ── Affichage des réservations ────────────────────────────

  /** Nom d'un terrain à partir de son identifiant (pour la liste des réservations). */
  nomTerrain(terrainId: string): string {
    return this.terrains().find(t => t.id === terrainId)?.nom ?? 'Terrain';
  }

  formatDateCourte(dateIso: string): string {
    const d = new Date(dateIso);
    return `${JOURS_LONG[(d.getDay() + 6) % 7]} ${d.getDate()} ${MOIS_FR[d.getMonth()]}`;
  }

  readonly formatHeure = formatHeure;

  // ── Chargements ───────────────────────────────────────────

  private chargerCreneaux(): void {
    const t = this.terrainSelectionne();
    if (!t) return;
    const jours = this.joursSemaine();
    this.padel.getCreneauxOccupes(t.id, this.toDateStr(jours[0]), this.toDateStr(jours[6])).subscribe({
      next: creneaux => this.creneaux.set(creneaux),
      error: err => this.gererErreurChargement(err)
    });
  }

  private chargerMesReservations(): void {
    this.padel.getMesReservations().subscribe({
      next: resas => this.mesReservations.set(resas),
      error: err => this.gererErreurChargement(err)
    });
  }

  private gererErreurChargement(err: { status?: number }): void {
    this.loading.set(false);
    if (err.status === 401) {
      this.error.set('Session expirée. Veuillez vous reconnecter.');
      this.authState.logout();
    } else {
      this.error.set('Erreur lors du chargement.');
    }
  }

  // ── Helpers ───────────────────────────────────────────────

  /** Construit le texte du récapitulatif d'annulation. */
  private construireRecap(r: Reservation): string {
    const moyen = r.moyenPaiement === 'EnLigne' ? 'En ligne' : 'Sur place';
    return [
      'RÉSERVATION ANNULÉE',
      '====================',
      `Terrain        : ${this.nomTerrain(r.terrainId)}`,
      `Date           : ${this.formatDateCourte(r.date)}`,
      `Horaire        : ${this.formatHeure(r.heureDebut)} → ${this.formatHeure(r.heureFin)}`,
      `Prix payé      : ${r.prixPaye} €`,
      `Moyen paiement : ${moyen}`,
      r.remarques ? `Remarque       : ${r.remarques}` : '',
      '',
      `Annulée le ${new Date().toLocaleString('fr-BE')}`
    ].filter(Boolean).join('\n');
  }

  /** Minute de début de la semaine : lundi 00h00 décalé de `offset` semaines. */
  private lundiDeLaSemaine(offset: number): Date {
    const d = new Date();
    d.setHours(0, 0, 0, 0);
    const jour = (d.getDay() + 6) % 7; // 0 = lundi
    d.setDate(d.getDate() - jour + offset * 7);
    return d;
  }

  /** "HH:mm:ss" → minutes depuis minuit. */
  private parseTime(time: string): number {
    const [h, m] = time.split(':').map(Number);
    return h * 60 + m;
  }

  /** minutes → "HH:mm:ss". */
  private formatTime(min: number): string {
    const h = Math.floor(min / 60);
    const m = min % 60;
    return `${String(h).padStart(2, '0')}:${String(m).padStart(2, '0')}:00`;
  }

  /** minutes → "10h00" (affichage). */
  private formatHeureLisible(min: number): string {
    const h = Math.floor(min / 60);
    const m = min % 60;
    return `${h}h${String(m).padStart(2, '0')}`;
  }

  /** Date → "Mercredi 16 avril". */
  private formatDateLongue(d: Date): string {
    const nom = JOURS_LONG[(d.getDay() + 6) % 7];
    return `${nom.charAt(0).toUpperCase()}${nom.slice(1)} ${d.getDate()} ${MOIS_FR[d.getMonth()]}`;
  }

  /** Date → "YYYY-MM-DD" (local, sans décalage de fuseau). */
  private toDateStr(d: Date): string {
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
  }

  /** Affiche le libellé court d'un jour (Lun, Mar…). */
  jourCourt(index: number): string {
    return JOURS_FR[index];
  }

  /** Libellé d'une ligne horaire de la grille (ex. "10h00"). */
  heureLabel(min: number): string {
    return this.formatHeureLisible(min);
  }
}
