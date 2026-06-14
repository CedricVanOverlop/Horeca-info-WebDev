import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
  AbstractControl,
  ValidationErrors,
  ValidatorFn,
} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthStateService } from '../../services/auth-state.service';
import { UsersService } from '../../services/api/users.service';
import { UserProfile } from '../../services/api/models/user-profile.model';

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
 */
@Component({
  selector: 'app-compte-page',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './mon-compte-page.component.html',
  styleUrls: ['./mon-compte-page.component.css'],
})
export class MonComptePageComponent implements OnInit {
  // ── État général ──────────────────────────────────────────────
  /** Profil chargé depuis l'API */
  profile: UserProfile | null = null;
  /** Chargement initial */
  isLoading = true;

  // ── Formulaires ───────────────────────────────────────────────
  profileForm!: FormGroup;
  passwordForm!: FormGroup;

  // ── Feedback UI ───────────────────────────────────────────────
  profileSuccess = false;
  profileError = '';
  passwordSuccess = false;
  passwordError = '';

  /** Affiche la confirmation de suppression */
  showDeleteConfirm = false;
  deleteError = '';

  // ── Soumissions en cours ──────────────────────────────────────
  isSubmittingProfile = false;
  isSubmittingPassword = false;
  isDeletingAccount = false;

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
        nouveauMotDePasse:  ['', [Validators.required, Validators.minLength(8)]],
        confirmerMotDePasse:['', [Validators.required]],
      },
      { validators: passwordMatchValidator }
    );
  }

  // ─────────────────────────────────────────────────────────────
  // Chargement du profil
  // ─────────────────────────────────────────────────────────────

  private _loadProfile(): void {
    this.isLoading = true;
    this.usersService.getMyProfile().subscribe({
      next: (profile) => {
        this.profile = profile;
        // Préremplissage du formulaire avec les données reçues
        this.profileForm.patchValue({
          nom:       profile.nom,
          prenom:    profile.prenom,
          email:     profile.email,
          telephone: profile.telephone ?? '',
        });
        this.isLoading = false;
      },
      error: () => {
        this.profileError = 'Impossible de charger le profil. Veuillez réessayer.';
        this.isLoading = false;
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
    if (!this.profile) return '?';
    const n = this.profile.nom?.charAt(0).toUpperCase() ?? '';
    const p = this.profile.prenom?.charAt(0).toUpperCase() ?? '';
    return `${n}${p}`;
  }

  /**
   * Formate la date d'inscription en "Membre depuis <mois> <année>".
   * Ex. : 2025-01-15 → "Membre depuis janvier 2025"
   */
  get membreDepuisLabel(): string {
    if (!this.profile?.membreDepuis) return '';
    const date = new Date(this.profile.membreDepuis);
    return `Membre depuis ${date.toLocaleDateString('fr-BE', { month: 'long', year: 'numeric' })}`;
  }

  // ─────────────────────────────────────────────────────────────
  // Soumission — Informations personnelles
  // ─────────────────────────────────────────────────────────────

  /** Soumet les modifications du profil (Nom, Prénom, Email, Téléphone). */
  onSubmitProfile(): void {
    this.profileSuccess = false;
    this.profileError   = '';

    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }

    this.isSubmittingProfile = true;

    this.usersService.updateMyProfile(this.profileForm.value).subscribe({
      next: () => {
        this.profileSuccess = true;
        this.isSubmittingProfile = false;
        // Met à jour le profil local affiché dans l'avatar
        if (this.profile) {
          this.profile = { ...this.profile, ...this.profileForm.value };
        }
      },
      error: (err) => {
        this.profileError = err?.error?.message ?? 'Erreur lors de la mise à jour du profil.';
        this.isSubmittingProfile = false;
      },
    });
  }

  // ─────────────────────────────────────────────────────────────
  // Soumission — Changement de mot de passe
  // ─────────────────────────────────────────────────────────────

  /** Soumet le changement de mot de passe. */
  onSubmitPassword(): void {
    this.passwordSuccess = false;
    this.passwordError   = '';

    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }

    this.isSubmittingPassword = true;

    this.usersService.changeMyPassword(this.passwordForm.value).subscribe({
      next: () => {
        this.passwordSuccess = true;
        this.passwordForm.reset();
        this.isSubmittingPassword = false;
      },
      error: (err) => {
        this.passwordError = err?.error?.message ?? 'Erreur lors du changement de mot de passe.';
        this.isSubmittingPassword = false;
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
    this.showDeleteConfirm = true;
    this.deleteError = '';
  }

  /** Annule la suppression. */
  onCancelDelete(): void {
    this.showDeleteConfirm = false;
    this.deleteError = '';
  }

  /**
   * Confirme et exécute la suppression définitive du compte.
   * Après suppression, déconnecte et redirige vers /login.
   */
  onConfirmDelete(): void {
    this.isDeletingAccount = true;
    this.deleteError = '';

    this.usersService.deleteMyAccount().subscribe({
      next: () => {
        this.isDeletingAccount = false;
        this.authStateService.logout(); // Nettoie le token et redirige
      },
      error: (err) => {
        this.deleteError = err?.error?.message ?? 'Erreur lors de la suppression du compte.';
        this.isDeletingAccount = false;
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