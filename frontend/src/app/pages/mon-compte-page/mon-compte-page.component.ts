import { Component, OnInit, signal } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
import { Router } from '@angular/router';
import { AuthStateService } from '../../services/auth-state.service';
import { UsersService } from '../../services/api/users.service';
import { UserProfile } from '../../services/api/models/user-profile.model';
import { NavbarComponent } from '../../components/navbar/navbar.component';

/**
 * Validateur personnalisé : vérifie que "confirmerMotDePasse" correspond à "nouveauMotDePasse".
 * Appliqué au niveau du FormGroup pour accéder aux deux contrôles.
 */
const passwordMatchValidator: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
  const nouveau = group.get('nouveauMotDePasse')?.value;
  const confirmer = group.get('confirmerMotDePasse')?.value;
  return nouveau && confirmer && nouveau !== confirmer
    ? { passwordMismatch: true }
    : null;
};

/**
 * Page "Mon compte" — accessible à tous les rôles connectés.
 *
 * Sections :
 *  1. En-tête : avatar initiales + email + date membre
 *  2. Informations personnelles (Nom, Prénom, Email, Téléphone)
 *  3. Changement de mot de passe
 *  4. Se déconnecter
 *  5. Zone dangereuse — Supprimer mon compte (confirmation modale inline)
 *
 * L'état affiché est porté par des signals : en zoneless, ils déclenchent
 * automatiquement le rafraîchissement de la vue après les callbacks async.
 */
@Component({
  selector: 'app-compte-page',
  standalone: true,
  imports: [ReactiveFormsModule, NavbarComponent],
  templateUrl: './mon-compte-page.component.html',
  styleUrls: ['./mon-compte-page.component.css'],
})
export class MonComptePageComponent implements OnInit {
  // ── État général ──────────────────────────────────────────────
  /** Profil chargé depuis l'API */
  readonly profile = signal<UserProfile | null>(null);
  /** Chargement initial */
  readonly isLoading = signal(true);

  // ── Formulaires ───────────────────────────────────────────────
  profileForm!: FormGroup;
  passwordForm!: FormGroup;

  // ── Feedback UI ───────────────────────────────────────────────
  readonly profileSuccess = signal(false);
  readonly profileError = signal('');
  readonly passwordSuccess = signal(false);
  readonly passwordError = signal('');

  /** Affiche la confirmation de suppression */
  readonly showDeleteConfirm = signal(false);
  readonly deleteError = signal('');

  // ── Soumissions en cours ──────────────────────────────────────
  readonly isSubmittingProfile = signal(false);
  readonly isSubmittingPassword = signal(false);
  readonly isDeletingAccount = signal(false);

  constructor(
    private fb: FormBuilder,
    private usersService: UsersService,
    private authStateService: AuthStateService,
    private router: Router
  ) {}

  // ─────────────────────────────────────────────────────────────
  // Cycle de vie
  // ─────────────────────────────────────────────────────────────

  ngOnInit(): void {
    this._initForms();
    this._loadProfile();
  }

  // ─────────────────────────────────────────────────────────────
  // Initialisation des formulaires
  // ─────────────────────────────────────────────────────────────

  private _initForms(): void {
    /** Formulaire informations personnelles */
    this.profileForm = this.fb.group({
      nom:       ['', [Validators.required, Validators.maxLength(100)]],
      prenom:    ['', [Validators.required, Validators.maxLength(100)]],
      email:     ['', [Validators.required, Validators.email, Validators.maxLength(255)]],
      telephone: ['', [Validators.pattern(/^\+?[\d\s\-().]{7,20}$/)]],
    });

    /** Formulaire changement de mot de passe avec validateur croisé */
    this.passwordForm = this.fb.group(
      {
        motDePasseActuel:   ['', [Validators.required]],
        nouveauMotDePasse:  ['', [Validators.required]],
        confirmerMotDePasse:['', [Validators.required]],
      },
      { validators: passwordMatchValidator }
    );
  }

  // ─────────────────────────────────────────────────────────────
  // Chargement du profil
  // ─────────────────────────────────────────────────────────────

  private _loadProfile(): void {
    this.isLoading.set(true);
    this.usersService.getMyProfile().subscribe({
      next: (profile) => {
        this.profile.set(profile);
        // Préremplissage du formulaire avec les données reçues
        this.profileForm.patchValue({
          nom:       profile.nom,
          prenom:    profile.prenom,
          email:     profile.email,
          telephone: profile.telephone ?? '',
        });
        this.isLoading.set(false);
      },
      error: () => {
        this.profileError.set('Impossible de charger le profil. Veuillez réessayer.');
        this.isLoading.set(false);
      },
    });
  }

  // ─────────────────────────────────────────────────────────────
  // Helpers affichage
  // ─────────────────────────────────────────────────────────────

  /**
   * Génère les initiales affichées dans l'avatar.
   * Ex. : "Nicolas Pirson" → "NP"
   */
  get initiales(): string {
    const profile = this.profile();
    if (!profile) return '?';
    const n = profile.nom?.charAt(0).toUpperCase() ?? '';
    const p = profile.prenom?.charAt(0).toUpperCase() ?? '';
    return `${n}${p}`;
  }

  /**
   * Formate la date d'inscription en "Membre depuis <mois> <année>".
   * Ex. : 2025-01-15 → "Membre depuis janvier 2025"
   */
  get membreDepuisLabel(): string {
    const membreDepuis = this.profile()?.membreDepuis;
    if (!membreDepuis) return '';
    const date = new Date(membreDepuis);
    return `Membre depuis ${date.toLocaleDateString('fr-BE', { month: 'long', year: 'numeric' })}`;
  }

  // ─────────────────────────────────────────────────────────────
  // Soumission — Informations personnelles
  // ─────────────────────────────────────────────────────────────

  /** Soumet les modifications du profil (Nom, Prénom, Email, Téléphone). */
  onSubmitProfile(): void {
    this.profileSuccess.set(false);
    this.profileError.set('');

    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.isSubmittingProfile.set(true);

    this.usersService.updateMyProfile(this.profileForm.value).subscribe({
      next: () => {
        this.profileSuccess.set(true);
        this.isSubmittingProfile.set(false);
        // Met à jour le profil local affiché dans l'avatar
        const current = this.profile();
        if (current) {
          this.profile.set({ ...current, ...this.profileForm.value });
        }
      },
      error: (err) => {
        this.profileError.set(err?.error?.message ?? 'Erreur lors de la mise à jour du profil.');
        this.isSubmittingProfile.set(false);
      },
    });
  }

  // ─────────────────────────────────────────────────────────────
  // Soumission — Changement de mot de passe
  // ─────────────────────────────────────────────────────────────

  /** Soumet le changement de mot de passe. */
  onSubmitPassword(): void {
    this.passwordSuccess.set(false);
    this.passwordError.set('');

    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.isSubmittingPassword.set(true);

    this.usersService.changeMyPassword(this.passwordForm.value).subscribe({
      next: () => {
        this.passwordSuccess.set(true);
        this.passwordForm.reset();
        this.isSubmittingPassword.set(false);
      },
      error: (err) => {
        this.passwordError.set(err?.error?.message ?? 'Erreur lors du changement de mot de passe.');
        this.isSubmittingPassword.set(false);
      },
    });
  }

  // ─────────────────────────────────────────────────────────────
  // Déconnexion
  // ─────────────────────────────────────────────────────────────

  /** Termine la session et redirige vers /login via AuthStateService. */
  onLogout(): void {
    this.authStateService.logout();
  }

  // ─────────────────────────────────────────────────────────────
  // Suppression de compte
  // ─────────────────────────────────────────────────────────────

  /** Affiche la confirmation de suppression. */
  onRequestDelete(): void {
    this.showDeleteConfirm.set(true);
    this.deleteError.set('');
  }

  /** Annule la suppression. */
  onCancelDelete(): void {
    this.showDeleteConfirm.set(false);
    this.deleteError.set('');
  }

  /**
   * Confirme et exécute la suppression définitive du compte.
   * Après suppression, déconnecte et redirige vers /login.
   */
  onConfirmDelete(): void {
    this.isDeletingAccount.set(true);
    this.deleteError.set('');

    this.usersService.deleteMyAccount().subscribe({
      next: () => {
        this.isDeletingAccount.set(false);
        this.authStateService.logout(); // Nettoie le token et redirige
      },
      error: (err) => {
        this.deleteError.set(err?.error?.message ?? 'Erreur lors de la suppression du compte.');
        this.isDeletingAccount.set(false);
      },
    });
  }

  // ─────────────────────────────────────────────────────────────
  // Accesseurs rapides pour le template
  // ─────────────────────────────────────────────────────────────

  get nom()       { return this.profileForm.get('nom'); }
  get prenom()    { return this.profileForm.get('prenom'); }
  get email()     { return this.profileForm.get('email'); }
  get telephone() { return this.profileForm.get('telephone'); }

  get motDePasseActuel()    { return this.passwordForm.get('motDePasseActuel'); }
  get nouveauMotDePasse()   { return this.passwordForm.get('nouveauMotDePasse'); }
  get confirmerMotDePasse() { return this.passwordForm.get('confirmerMotDePasse'); }
}
