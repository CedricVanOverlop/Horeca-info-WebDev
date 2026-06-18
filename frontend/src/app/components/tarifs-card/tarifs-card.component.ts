import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { forkJoin, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { PadelService } from '../../services/api/padel.service';
import { Terrain } from '../../services/api/models/terrain.model';
import { Tarif } from '../../services/api/models/tarif.model';
import { AuthStateService } from '../../services/auth-state.service';
import { formatHeure } from '../../shared/format.util';

const JOURS = [
  { valeur: 1, label: 'Lun' }, { valeur: 2, label: 'Mar' }, { valeur: 3, label: 'Mer' },
  { valeur: 4, label: 'Jeu' }, { valeur: 5, label: 'Ven' }, { valeur: 6, label: 'Sam' },
  { valeur: 7, label: 'Dim' }
];

/** Couleurs attribuées cycliquement aux tarifs de la palette. */
const COULEURS = ['#5b8def', '#e2725b', '#41b3a3', '#c38d9e', '#e8a948', '#8d6cd1', '#669d6a', '#c0566f'];

/** Modèle de tarif réutilisable dans la palette (libellé + prix → couleur). */
interface TarifModele {
  cle: string;     // identifiant logique = `${libelle}|${prix}`
  libelle: string;
  prix: number;
  couleur: string;
}

/** État d'une case du planning (1 heure d'un jour). */
interface Cellule {
  tarifId: string | null; // id du tarif persisté couvrant la case, sinon null (= en attente)
  cle: string | null;     // clé du modèle appliqué, null = vide
}

/**
 * Carte « Tarifs » (admin) : planning hebdomadaire peignable. On définit une palette
 * de tarifs colorés puis on peint les créneaux (clic/glissé). Clic sans pinceau =
 * édition du tarif d'un jour précis (UPDATE en place, compatible avec les réservations).
 */
@Component({
  selector: 'app-tarifs-card',
  templateUrl: './tarifs-card.component.html',
  styleUrl: './tarifs-card.component.css',
  host: { '(document:mouseup)': 'finPeinture()' }
})
export class TarifsCardComponent implements OnInit {
  private readonly padel = inject(PadelService);
  private readonly authState = inject(AuthStateService);

  readonly jours = JOURS;
  readonly formatHeure = formatHeure;

  readonly terrains = signal<Terrain[]>([]);
  readonly planningTerrainId = signal('');           // terrain affiché/édité
  readonly palette = signal<TarifModele[]>([]);
  readonly paletteManuelle = signal<TarifModele[]>([]); // tarifs ajoutés mais pas encore peints/enregistrés
  readonly tarifsCharges = signal<Tarif[]>([]);      // tarifs persistés du terrain affiché
  readonly outil = signal<string | null>(null);      // clé modèle actif, 'gomme', ou null
  readonly grille = signal<Record<string, Cellule>>({});
  readonly aSupprimer = signal<Set<string>>(new Set());
  readonly peinture = signal(false);                 // glissé en cours
  readonly nmLibelle = signal('');
  readonly nmPrix = signal<number | null>(null);
  readonly editionModele = signal<string | null>(null); // clé du tarif en cours d'édition
  readonly emLibelle = signal('');
  readonly emPrix = signal<number | null>(null);
  readonly modeleSaving = signal(false);
  readonly cellEdit = signal<{ id: string; jour: number } | null>(null); // tarif d'un créneau en édition
  readonly ceLibelle = signal('');
  readonly cePrix = signal<number | null>(null);
  readonly ceSaving = signal(false);
  readonly tarifSaving = signal(false);
  readonly tarifError = signal('');
  readonly tarifSuccess = signal('');

  /** Index des modèles par clé pour une résolution O(1) dans la grille (vs find linéaire). */
  readonly paletteParCle = computed(() => new Map(this.palette().map(m => [m.cle, m])));

  /** Heures (cases) du planning : de l'ouverture à la fermeture du terrain affiché. */
  readonly heures = computed(() => {
    const t = this.terrains().find(x => x.id === this.planningTerrainId());
    if (!t) return [];
    const debut = this.parseHeure(t.heureOuverture);
    const fin = this.parseHeure(t.heureFermeture);
    const out: number[] = [];
    for (let h = debut; h < fin; h++) out.push(h);
    return out;
  });

  ngOnInit(): void {
    this.padel.getTerrains().subscribe({
      next: terrains => {
        this.terrains.set(terrains);
        if (terrains.length) this.chargerPlanning(terrains[0].id);
      },
      error: err => {
        if (err.status === 401) this.authState.logout();
        else this.tarifError.set('Erreur lors du chargement des terrains.');
      }
    });
  }

  /** Charge les tarifs d'un terrain et reconstruit palette + grille (réinitialise tout l'état d'édition). */
  chargerPlanning(terrainId: string): void {
    this.planningTerrainId.set(terrainId);
    this.aSupprimer.set(new Set());
    this.outil.set(null);             // pas de pinceau traînant d'un terrain à l'autre
    this.paletteManuelle.set([]);     // les modèles manuels non peints ne suivent pas le changement de terrain
    this.annulerEditionModele();
    this.annulerEditionCellule();
    this.tarifError.set('');
    this.tarifSuccess.set('');
    this.rechargerTarifs(terrainId);
  }

  /** Recharge les tarifs du terrain sans toucher aux messages (après enregistrement). */
  private rechargerTarifs(terrainId: string): void {
    this.padel.getTarifs(terrainId).subscribe({
      next: tarifs => { this.tarifsCharges.set(tarifs); this.construireDepuisTarifs(tarifs); },
      error: () => { this.tarifsCharges.set([]); this.palette.set([]); this.grille.set({}); }
    });
  }

  /**
   * Construit la grille (1 case = 1h) et reconstruit la palette À NEUF depuis les tarifs
   * du serveur, puis ré-ajoute les modèles manuels pas encore peints. Évite les doublons
   * (ex: ancien prix qui resterait après une modification).
   */
  private construireDepuisTarifs(tarifs: Tarif[]): void {
    const modeles = new Map<string, TarifModele>();
    const grille: Record<string, Cellule> = {};

    for (const t of tarifs) {
      const cle = `${t.type}|${t.prixHeure}`;
      if (!modeles.has(cle)) {
        modeles.set(cle, { cle, libelle: t.type, prix: t.prixHeure, couleur: this.couleurPourCle(cle) });
      }
      const debut = this.parseHeure(t.heureDebut);
      const fin = this.parseHeure(t.heureFin);
      for (let h = debut; h < fin; h++) {
        grille[`${t.jourSemaine}-${h}`] = { tarifId: t.id, cle };
      }
    }

    // Modèles ajoutés manuellement mais pas encore présents côté serveur.
    for (const m of this.paletteManuelle()) {
      if (!modeles.has(m.cle)) modeles.set(m.cle, m);
    }

    this.palette.set([...modeles.values()]);
    this.grille.set(grille);
  }

  /** Couleur stable déterminée par la clé du tarif (ne bouge pas d'un rechargement à l'autre). */
  private couleurPourCle(cle: string): string {
    let h = 0;
    for (let i = 0; i < cle.length; i++) h = (h * 31 + cle.charCodeAt(i)) >>> 0;
    return COULEURS[h % COULEURS.length];
  }

  /** Ajoute un modèle de tarif à la palette (libellé + prix). */
  ajouterModele(): void {
    const libelle = this.nmLibelle().trim();
    const prix = this.nmPrix();
    if (!libelle || prix === null || prix <= 0) {
      this.tarifError.set('Libellé et prix (> 0) sont obligatoires.');
      return;
    }
    const cle = `${libelle}|${prix}`;
    if (!this.paletteManuelle().some(m => m.cle === cle) && !this.palette().some(m => m.cle === cle)) {
      this.paletteManuelle.set([...this.paletteManuelle(), { cle, libelle, prix, couleur: this.couleurPourCle(cle) }]);
    }
    this.construireDepuisTarifs(this.tarifsCharges());
    this.outil.set(cle);
    this.nmLibelle.set('');
    this.nmPrix.set(null);
    this.tarifError.set('');
  }

  /** Sélectionne l'outil actif : un modèle de tarif (pinceau) ou la gomme. */
  choisirOutil(cle: string): void {
    this.outil.set(this.outil() === cle ? null : cle);
  }

  modeleParCle(cle: string | null): TarifModele | undefined {
    return cle ? this.paletteParCle().get(cle) : undefined;
  }

  cellule(jour: number, heure: number): Cellule | undefined {
    return this.grille()[`${jour}-${heure}`];
  }

  /** Couleur de fond d'une case (selon le modèle appliqué). */
  couleurCase(jour: number, heure: number): string {
    return this.modeleParCle(this.cellule(jour, heure)?.cle ?? null)?.couleur ?? '';
  }

  // Peinture par clic + glissé. Sans pinceau actif, un clic sur un créneau peint
  // ouvre l'édition du tarif de CE jour (modification du prix/libellé pour ce jour seul).
  debutPeinture(jour: number, heure: number): void {
    if (!this.outil()) {
      this.ouvrirEditionCellule(jour, heure);
      return;
    }
    this.peinture.set(true);
    this.appliquerOutil(jour, heure);
  }

  surviolPeinture(jour: number, heure: number): void {
    if (this.peinture()) this.appliquerOutil(jour, heure);
  }

  finPeinture(): void {
    this.peinture.set(false);
  }

  /** Applique l'outil actif à une case : peint si vide, gomme un tarif si occupé. */
  private appliquerOutil(jour: number, heure: number): void {
    const tool = this.outil();
    if (!tool) return;

    const key = `${jour}-${heure}`;
    const grille = { ...this.grille() };
    const courante = grille[key];

    if (tool === 'gomme') {
      if (!courante) return;
      if (courante.tarifId) {
        // Case persistée : on retire tout le tarif (toutes ses cases) et on le marque à supprimer.
        const tarifId = courante.tarifId;
        for (const k of Object.keys(grille)) {
          if (grille[k]?.tarifId === tarifId) delete grille[k];
        }
        const suppr = new Set(this.aSupprimer());
        suppr.add(tarifId);
        this.aSupprimer.set(suppr);
      } else {
        delete grille[key]; // case en attente : simple effacement
      }
      this.grille.set(grille);
      return;
    }

    // Pinceau : on ne peint que les cases libres (pas de chevauchement possible).
    if (courante) return;
    grille[key] = { tarifId: null, cle: tool };
    this.grille.set(grille);
  }

  /** Recharge le planning en annulant les modifications non enregistrées. */
  annulerModifsPlanning(): void {
    this.chargerPlanning(this.planningTerrainId());
  }

  /**
   * Enregistre le planning du terrain affiché : supprime les tarifs gommés, puis crée
   * les plages (fusion des cases contiguës de même modèle).
   */
  enregistrerPlanning(): void {
    const terrainId = this.planningTerrainId();
    if (!terrainId) return;

    this.tarifSaving.set(true);
    this.tarifError.set('');
    this.tarifSuccess.set('');

    const suppressions = [...this.aSupprimer()].map(id =>
      this.padel.supprimerTarif(id).pipe(
        map(() => ({ ok: true, type: 'del' as const })),
        catchError(() => of({ ok: false, type: 'del' as const }))));

    const creations = this.calculerPlagesAEnregistrer().map(plage =>
      this.padel.creerTarif({
        type: plage.libelle,
        prixHeure: plage.prix,
        heureDebut: `${this.deuxChiffres(plage.debut)}:00:00`,
        heureFin: `${this.deuxChiffres(plage.fin)}:00:00`,
        jourSemaine: plage.jour,
        terrainId
      }).pipe(
        map(() => ({ ok: true, type: 'create' as const })),
        catchError(() => of({ ok: false, type: 'create' as const }))));

    const appels = [...suppressions, ...creations];
    if (!appels.length) {
      this.tarifSaving.set(false);
      this.tarifSuccess.set('Aucune modification à enregistrer.');
      return;
    }

    forkJoin(appels).subscribe(res => {
      this.tarifSaving.set(false);
      const creesOk = res.filter(r => r.type === 'create' && r.ok).length;
      const creesKo = res.filter(r => r.type === 'create' && !r.ok).length;
      const supprKo = res.filter(r => r.type === 'del' && !r.ok).length;

      const msgs: string[] = [];
      if (creesOk) msgs.push(`${creesOk} plage(s) enregistrée(s)`);
      if (creesKo) this.tarifError.set(`${creesKo} plage(s) ignorée(s) (créneau déjà occupé).`);
      if (supprKo) this.tarifError.set('Certains tarifs n\'ont pas pu être supprimés : une réservation les utilise encore.');
      if (msgs.length) this.tarifSuccess.set(msgs.join(', ') + '.');

      // Recharge SANS effacer les messages (sinon l'erreur disparaîtrait aussitôt).
      this.aSupprimer.set(new Set());
      this.rechargerTarifs(this.planningTerrainId());
    });
  }

  // ── Édition d'un tarif de la palette (prix + libellé, sans toucher aux jours) ──

  /** Ouvre l'éditeur inline d'un tarif de la palette. */
  editerModele(m: TarifModele): void {
    this.editionModele.set(m.cle);
    this.emLibelle.set(m.libelle);
    this.emPrix.set(m.prix);
    this.tarifError.set('');
    this.tarifSuccess.set('');
  }

  annulerEditionModele(): void {
    this.editionModele.set(null);
    this.emLibelle.set('');
    this.emPrix.set(null);
  }

  /**
   * Enregistre le nouveau libellé/prix d'un tarif : met à jour (UPDATE, compatible avec
   * les réservations existantes) toutes les lignes TARIF de ce tarif sur le terrain affiché,
   * sans modifier les jours ni les heures.
   */
  enregistrerModele(): void {
    const cle = this.editionModele();
    if (!cle) return;
    const libelle = this.emLibelle().trim();
    const prix = this.emPrix();
    if (!libelle || prix === null || prix <= 0) {
      this.tarifError.set('Libellé et prix (> 0) sont obligatoires.');
      return;
    }

    // Lignes persistées correspondant à ce tarif (même type + prix).
    const lignes = this.tarifsCharges().filter(t => `${t.type}|${t.prixHeure}` === cle);

    if (!lignes.length) {
      // Tarif seulement présent dans la palette (pas encore peint/enregistré) : renommage local.
      const nouvelleCle = `${libelle}|${prix}`;
      this.paletteManuelle.set(this.paletteManuelle().map(m =>
        m.cle === cle ? { cle: nouvelleCle, libelle, prix, couleur: this.couleurPourCle(nouvelleCle) } : m));
      if (this.outil() === cle) this.outil.set(nouvelleCle);
      this.construireDepuisTarifs(this.tarifsCharges());
      this.annulerEditionModele();
      return;
    }

    this.modeleSaving.set(true);
    this.tarifError.set('');
    this.tarifSuccess.set('');

    const appels = lignes.map(t =>
      this.padel.modifierTarif(t.id, {
        type: libelle,
        prixHeure: prix,
        heureDebut: t.heureDebut,
        heureFin: t.heureFin,
        jourSemaine: t.jourSemaine,
        terrainId: t.terrainId
      }).pipe(
        map(() => ({ ok: true })),
        catchError(() => of({ ok: false }))));

    forkJoin(appels).subscribe(res => {
      this.modeleSaving.set(false);
      const ko = res.filter(r => !r.ok).length;
      if (ko > 0) this.tarifError.set(`${ko} créneau(x) n'ont pas pu être mis à jour.`);
      else this.tarifSuccess.set('Tarif mis à jour.');
      this.annulerEditionModele();
      this.rechargerTarifs(this.planningTerrainId());
    });
  }

  /**
   * Convertit les cases « en attente » (peintes, non persistées) en plages horaires :
   * fusionne les heures contiguës de même jour et même modèle.
   */
  private calculerPlagesAEnregistrer(): { jour: number; debut: number; fin: number; libelle: string; prix: number }[] {
    const grille = this.grille();
    const plages: { jour: number; debut: number; fin: number; libelle: string; prix: number }[] = [];

    for (const jour of JOURS.map(j => j.valeur)) {
      let courante: { debut: number; fin: number; cle: string } | null = null;

      for (const h of this.heures()) {
        const c = grille[`${jour}-${h}`];
        const enAttente = c && c.tarifId === null ? c.cle : null;

        if (enAttente && courante && courante.cle === enAttente && courante.fin === h) {
          courante.fin = h + 1; // prolonge la plage contiguë
        } else {
          if (courante) plages.push(this.finaliserPlage(jour, courante));
          courante = enAttente ? { debut: h, fin: h + 1, cle: enAttente } : null;
        }
      }
      if (courante) plages.push(this.finaliserPlage(jour, courante));
    }
    return plages;
  }

  private finaliserPlage(jour: number, p: { debut: number; fin: number; cle: string }) {
    const m = this.modeleParCle(p.cle)!;
    return { jour, debut: p.debut, fin: p.fin, libelle: m.libelle, prix: m.prix };
  }

  // ── Édition d'un seul créneau (un jour) — UPDATE en place ──

  tarifParId(id: string): Tarif | undefined {
    return this.tarifsCharges().find(t => t.id === id);
  }

  /** Ouvre l'éditeur du tarif persisté couvrant ce créneau (ce jour précis). */
  private ouvrirEditionCellule(jour: number, heure: number): void {
    const c = this.cellule(jour, heure);
    if (!c?.tarifId) return; // case vide ou plage non encore enregistrée → rien à éditer ici
    const t = this.tarifParId(c.tarifId);
    if (!t) return;
    this.cellEdit.set({ id: t.id, jour });
    this.ceLibelle.set(t.type);
    this.cePrix.set(t.prixHeure);
    this.tarifError.set('');
    this.tarifSuccess.set('');
  }

  annulerEditionCellule(): void {
    this.cellEdit.set(null);
    this.ceLibelle.set('');
    this.cePrix.set(null);
  }

  /**
   * Met à jour le prix/libellé du tarif de CE créneau uniquement (UPDATE de la ligne :
   * compatible avec les réservations existantes, jours/heures inchangés).
   */
  enregistrerCellule(): void {
    const ce = this.cellEdit();
    if (!ce) return;
    const libelle = this.ceLibelle().trim();
    const prix = this.cePrix();
    if (!libelle || prix === null || prix <= 0) {
      this.tarifError.set('Libellé et prix (> 0) sont obligatoires.');
      return;
    }
    const t = this.tarifParId(ce.id);
    if (!t) return;

    this.ceSaving.set(true);
    this.tarifError.set('');
    this.tarifSuccess.set('');
    this.padel.modifierTarif(t.id, {
      type: libelle,
      prixHeure: prix,
      heureDebut: t.heureDebut,
      heureFin: t.heureFin,
      jourSemaine: t.jourSemaine,
      terrainId: t.terrainId
    }).subscribe({
      next: () => {
        this.ceSaving.set(false);
        this.tarifSuccess.set('Tarif du créneau mis à jour.');
        this.annulerEditionCellule();
        this.rechargerTarifs(this.planningTerrainId());
      },
      error: err => {
        this.ceSaving.set(false);
        this.tarifError.set(err.error?.message ?? 'Modification impossible.');
      }
    });
  }

  // ── Affichage ─────────────────────────────────────────────

  labelJour(valeur: number): string {
    return JOURS.find(j => j.valeur === valeur)?.label ?? '';
  }

  // ── Helpers ───────────────────────────────────────────────

  /** "HH:mm[:ss]" → heure entière (8). */
  private parseHeure(time: string): number {
    return parseInt(time.slice(0, 2), 10);
  }

  private deuxChiffres(h: number): string {
    return String(h).padStart(2, '0');
  }
}
