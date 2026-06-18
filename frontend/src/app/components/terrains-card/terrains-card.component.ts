import { Component, OnInit, inject, signal } from '@angular/core';
import { PadelService } from '../../services/api/padel.service';
import { Terrain } from '../../services/api/models/terrain.model';
import { AuthStateService } from '../../services/auth-state.service';
import { Roles } from '../../roles';
import { formatHeure } from '../../shared/format.util';

/** Identifiant du commerce « Padel Center Lobbes » (seul commerce porteur de terrains). */
const PADEL_COMMERCE_ID = 3;

/**
 * Carte « Terrains » : liste des terrains avec activation/désactivation, édition
 * (nom + horaires) et création — réservées à l'administrateur pour la modification.
 */
@Component({
  selector: 'app-terrains-card',
  templateUrl: './terrains-card.component.html',
  styleUrl: './terrains-card.component.css'
})
export class TerrainsCardComponent implements OnInit {
  private readonly padel = inject(PadelService);
  private readonly authState = inject(AuthStateService);

  readonly estAdmin = this.authState.currentUserRole === Roles.Administrateur;
  readonly formatHeure = formatHeure;

  readonly terrains = signal<Terrain[]>([]);
  readonly terrainError = signal('');
  readonly nouveauNom = signal('');
  readonly nouvelleOuverture = signal('08:00');
  readonly nouvelleFermeture = signal('23:00');
  readonly terrainSaving = signal(false);
  // Édition d'un terrain existant (nom + horaires).
  readonly editTerrainId = signal<string | null>(null);
  readonly etNom = signal('');
  readonly etOuverture = signal('');
  readonly etFermeture = signal('');
  readonly etSaving = signal(false);

  ngOnInit(): void {
    this.chargerTerrains();
  }

  private chargerTerrains(): void {
    this.padel.getTerrains().subscribe({
      next: terrains => this.terrains.set(terrains),
      error: err => {
        if (err.status === 401) this.authState.logout();
        else this.terrainError.set('Erreur lors du chargement des terrains.');
      }
    });
  }

  /** Active/désactive un terrain (la désactivation est refusée s'il reste des réservations). */
  basculerActif(terrain: Terrain): void {
    this.terrainError.set('');
    this.padel.toggleTerrainActif(terrain.id, !terrain.disponible).subscribe({
      next: () => this.chargerTerrains(),
      error: err => this.terrainError.set(err.error?.message ?? 'Modification impossible.')
    });
  }

  /** Ouvre l'éditeur d'un terrain (nom + horaires). */
  editerTerrain(t: Terrain): void {
    this.editTerrainId.set(t.id);
    this.etNom.set(t.nom);
    this.etOuverture.set(this.formatHeure(t.heureOuverture));
    this.etFermeture.set(this.formatHeure(t.heureFermeture));
    this.terrainError.set('');
  }

  annulerEditionTerrain(): void {
    this.editTerrainId.set(null);
    this.etNom.set('');
    this.etOuverture.set('');
    this.etFermeture.set('');
  }

  /** Enregistre les modifications d'un terrain (admin). */
  enregistrerTerrain(): void {
    const id = this.editTerrainId();
    if (!id) return;
    if (!this.etNom().trim()) { this.terrainError.set('Le nom est obligatoire.'); return; }
    if (this.etFermeture() <= this.etOuverture()) {
      this.terrainError.set('La fermeture doit être après l\'ouverture.');
      return;
    }

    this.etSaving.set(true);
    this.terrainError.set('');
    this.padel.modifierTerrain(id, {
      nom: this.etNom().trim(),
      heureOuverture: `${this.etOuverture()}:00`,
      heureFermeture: `${this.etFermeture()}:00`
    }).subscribe({
      next: () => {
        this.etSaving.set(false);
        this.annulerEditionTerrain();
        this.chargerTerrains();
      },
      error: err => {
        this.etSaving.set(false);
        this.terrainError.set(err.error?.message ?? 'Modification impossible.');
      }
    });
  }

  /** Crée un terrain (admin). */
  creerTerrain(): void {
    if (!this.nouveauNom().trim()) { this.terrainError.set('Le nom est obligatoire.'); return; }
    if (this.nouvelleFermeture() <= this.nouvelleOuverture()) {
      this.terrainError.set('La fermeture doit être après l\'ouverture.');
      return;
    }

    this.terrainSaving.set(true);
    this.terrainError.set('');
    this.padel.creerTerrain({
      nom: this.nouveauNom().trim(),
      heureOuverture: `${this.nouvelleOuverture()}:00`,
      heureFermeture: `${this.nouvelleFermeture()}:00`,
      idCommerce: PADEL_COMMERCE_ID
    }).subscribe({
      next: () => {
        this.terrainSaving.set(false);
        this.nouveauNom.set('');
        this.chargerTerrains();
      },
      error: err => {
        this.terrainSaving.set(false);
        this.terrainError.set(err.error?.message ?? 'Création impossible.');
      }
    });
  }
}
