import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { UtilisateurAdminService } from '../../services/api/utilisateur-admin.service';
import {
  UserAdminDto,
  RoleAdmin,
  ReservationAdmin,
  HoraireAdmin,
} from '../../services/api/models/utilisateur-admin.model';
import { formatHeure, formatDate } from '../../shared/format.util';

/** Filtre de statut appliqué à la liste. */
type FiltreStatut = '' | 'actif' | 'bloque';

/** Type de modal actuellement ouverte. */
type ModalType = 'role' | 'points' | 'block' | 'details' | null;

/** Accès accordés par chaque rôle (affiché lors d'un changement de rôle). */
const ACCES_PAR_ROLE: Record<RoleAdmin, string[]> = {
  Client: ['Espace client', 'Carte de fidélité', 'Ses réservations de padel'],
  Employe: ['Tout du Client', 'Son planning et ses horaires', 'Ses disponibilités'],
  Cuisine: ['Tout de l\'Employé', 'Gestion de la cuisine', 'Gestion des stocks'],
  Administrateur: ['Accès complet', 'Gestion des utilisateurs', 'Gestion des terrains', 'Création des horaires'],
};

/**
 * Console d'administration des utilisateurs (rôle Administrateur uniquement).
 * Liste par rôle, recherche/filtre, et 4 modals dédiées : rôle, points, blocage, détails.
 * État géré par signals (zoneless).
 */
@Component({
  selector: 'app-gestion-utilisateurs-page',
  imports: [NavbarComponent],
  templateUrl: './gestion-utilisateurs-page.component.html',
  styleUrl: './gestion-utilisateurs-page.component.css'
})
export class GestionUtilisateursPageComponent implements OnInit {
  private readonly adminService = inject(UtilisateurAdminService);

  // ── Données ───────────────────────────────────────────────────
  readonly utilisateurs = signal<UserAdminDto[]>([]);
  readonly loading = signal(false);
  readonly serverError = signal('');

  // ── Filtres / navigation ──────────────────────────────────────
  readonly activeTab = signal<RoleAdmin>('Client');
  readonly searchQuery = signal('');
  readonly filterStatut = signal<FiltreStatut>('');

  // ── Modals ────────────────────────────────────────────────────
  readonly modalType = signal<ModalType>(null);
  readonly modalUser = signal<UserAdminDto | null>(null);
  readonly modalError = signal('');
  readonly modalSaving = signal(false);

  // Formulaires de modal
  readonly formRole = signal<RoleAdmin>('Client');
  readonly formPointsAjustement = signal('');
  readonly formMotifAjustement = signal('');

  // Détails (réservations + horaires)
  readonly reservations = signal<ReservationAdmin[]>([]);
  readonly horaires = signal<HoraireAdmin[]>([]);
  readonly detailsLoading = signal(false);

  // ── Statistiques par rôle ─────────────────────────────────────
  readonly stats = computed(() => {
    const users = this.utilisateurs();
    return {
      clients:  users.filter(u => u.role === 'Client').length,
      employes: users.filter(u => u.role === 'Employe').length,
      cuisine:  users.filter(u => u.role === 'Cuisine').length,
      admins:   users.filter(u => u.role === 'Administrateur').length,
    };
  });

  // ── Liste filtrée (onglet + recherche + statut) ───────────────
  readonly utilisateursFiltres = computed(() => {
    const tab = this.activeTab();
    const query = this.searchQuery().trim().toLowerCase();
    const statut = this.filterStatut();

    return this.utilisateurs().filter(u => {
      if (u.role !== tab) return false;
      if (statut === 'actif' && !u.actif) return false;
      if (statut === 'bloque' && u.actif) return false;
      if (query) {
        const haystack = `${u.nom} ${u.prenom} ${u.email}`.toLowerCase();
        if (!haystack.includes(query)) return false;
      }
      return true;
    });
  });

  /** Accès du rôle actuellement sélectionné dans la modal rôle. */
  readonly accesRoleSelectionne = computed(() => ACCES_PAR_ROLE[this.formRole()]);

  /** Indique si le rôle choisi diffère du rôle actuel de l'utilisateur. */
  readonly roleAChange = computed(() => {
    const user = this.modalUser();
    return user ? this.formRole() !== user.role : false;
  });

  ngOnInit(): void {
    this.charger();
  }

  // ─────────────────────────────────────────────────────────────
  // Chargement
  // ─────────────────────────────────────────────────────────────

  /** Charge tous les utilisateurs depuis l'API. */
  charger(): void {
    this.loading.set(true);
    this.serverError.set('');
    this.adminService.getAll().subscribe({
      next: users => {
        this.utilisateurs.set(users);
        this.loading.set(false);
      },
      error: () => {
        this.serverError.set('Impossible de charger les utilisateurs.');
        this.loading.set(false);
      },
    });
  }

  // ─────────────────────────────────────────────────────────────
  // Helpers d'affichage
  // ─────────────────────────────────────────────────────────────

  setTab(role: RoleAdmin): void { this.activeTab.set(role); }
  onSearch(value: string): void { this.searchQuery.set(value); }
  onFilterStatut(value: string): void { this.filterStatut.set(value as FiltreStatut); }

  initiales(user: UserAdminDto): string {
    const n = user.nom?.charAt(0).toUpperCase() ?? '';
    const p = user.prenom?.charAt(0).toUpperCase() ?? '';
    return `${n}${p}`;
  }

  /** Libellé lisible d'un rôle. */
  roleLabel(role: RoleAdmin): string {
    return role === 'Employe' ? 'Employé' : role;
  }

  readonly formatDate = formatDate;
  readonly formatHeure = formatHeure;

  // ─────────────────────────────────────────────────────────────
  // Ouverture / fermeture des modals
  // ─────────────────────────────────────────────────────────────

  /** Modal de changement de rôle. */
  ouvrirModal(user: UserAdminDto): void {
    this.modalUser.set(user);
    this.formRole.set(user.role);
    this.modalError.set('');
    this.modalType.set('role');
  }

  /** Modal d'ajustement de points. */
  ouvrirModalPoints(user: UserAdminDto): void {
    this.modalUser.set(user);
    this.formPointsAjustement.set('');
    this.formMotifAjustement.set('');
    this.modalError.set('');
    this.modalType.set('points');
  }

  /** Modal de confirmation de blocage / suppression. */
  confirmerBlocage(user: UserAdminDto): void {
    this.modalUser.set(user);
    this.modalError.set('');
    this.modalType.set('block');
  }

  /** Modal de détails (réservations + horaires). */
  ouvrirModalDetails(user: UserAdminDto): void {
    this.modalUser.set(user);
    this.reservations.set([]);
    this.horaires.set([]);
    this.detailsLoading.set(true);
    this.modalType.set('details');

    this.adminService.getReservations(user.id).subscribe({
      next: res => this.reservations.set(res),
      error: () => this.reservations.set([]),
    });
    this.adminService.getHoraires(user.id).subscribe({
      next: hor => { this.horaires.set(hor); this.detailsLoading.set(false); },
      error: () => { this.horaires.set([]); this.detailsLoading.set(false); },
    });
  }

  /** Ferme toute modal. */
  fermerModal(): void {
    this.modalType.set(null);
    this.modalUser.set(null);
    this.modalSaving.set(false);
  }

  onFormRole(value: string): void { this.formRole.set(value as RoleAdmin); }
  onFormPoints(value: string): void { this.formPointsAjustement.set(value); }
  onFormMotif(value: string): void { this.formMotifAjustement.set(value); }

  // ─────────────────────────────────────────────────────────────
  // Actions des modals
  // ─────────────────────────────────────────────────────────────

  /** Enregistre le changement de rôle. */
  enregistrerRole(): void {
    const user = this.modalUser();
    if (!user || !this.roleAChange()) { this.fermerModal(); return; }

    this.modalSaving.set(true);
    this.modalError.set('');
    this.adminService.changeRole(user.id, { acces: this.formRole() }).subscribe({
      next: () => this.onModalSuccess(),
      error: err => this.onModalError(err),
    });
  }

  /** Enregistre l'ajustement de points. */
  enregistrerPoints(): void {
    const user = this.modalUser();
    if (!user) return;

    const montant = Number(this.formPointsAjustement());
    const motif = this.formMotifAjustement().trim();

    if (isNaN(montant) || montant === 0) {
      this.modalError.set('Saisir un montant non nul (positif ou négatif).');
      return;
    }
    if (!motif) {
      this.modalError.set('Le motif est requis.');
      return;
    }

    this.modalSaving.set(true);
    this.modalError.set('');
    this.adminService.adjustPoints(user.id, { montant, motif }).subscribe({
      next: () => this.onModalSuccess(),
      error: err => this.onModalError(err),
    });
  }

  /** Bloque le compte (depuis la modal de blocage). */
  bloquer(): void {
    const user = this.modalUser();
    if (!user) return;
    this.modalSaving.set(true);
    this.modalError.set('');
    this.adminService.block(user.id).subscribe({
      next: () => this.onModalSuccess(),
      error: err => this.onModalError(err),
    });
  }

  /** Supprime définitivement le compte (depuis la modal de blocage). */
  supprimer(): void {
    const user = this.modalUser();
    if (!user) return;
    this.modalSaving.set(true);
    this.modalError.set('');
    this.adminService.deleteUser(user.id).subscribe({
      next: () => this.onModalSuccess(),
      error: err => this.onModalError(err),
    });
  }

  /** Débloque un compte directement depuis la liste. */
  debloquer(user: UserAdminDto): void {
    this.adminService.unblock(user.id).subscribe({
      next: () => this.charger(),
      error: err => this.serverError.set(err?.error?.message ?? 'Erreur lors du déblocage.'),
    });
  }

  private onModalSuccess(): void {
    this.modalSaving.set(false);
    this.fermerModal();
    this.charger();
  }

  private onModalError(err: any): void {
    this.modalSaving.set(false);
    this.modalError.set(err?.error?.message ?? 'Erreur lors de l\'opération.');
  }
}
