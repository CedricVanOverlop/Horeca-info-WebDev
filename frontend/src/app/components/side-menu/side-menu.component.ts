import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { AuthStateService } from '../../services/auth-state.service';
import { MenuStateService } from '../../services/menu-state.service';

/** Niveau d'accès requis pour voir un lien, avec son badge de couleur. */
interface MenuLink {
  label: string;
  badge: string;
  color: string;
  /** Rang minimal requis (Client=0, Employe=1, Cuisine=2, Administrateur=3). */
  minRank: number;
}

/** Classement hiérarchique des rôles pour le filtrage des liens. */
const ROLE_RANK: Record<string, number> = {
  Client: 0,
  Employe: 1,
  Cuisine: 2,
  Administrateur: 3
};

/**
 * Panneau latéral piloté par le MenuStateService.
 * Affiche un message d'invitation si déconnecté, sinon le profil de
 * l'utilisateur et la liste des liens filtrés selon son rôle.
 */
@Component({
  selector: 'app-side-menu',
  imports: [CommonModule],
  templateUrl: './side-menu.component.html',
  styleUrl: './side-menu.component.css'
})
export class SideMenuComponent implements OnInit, OnDestroy {
  /** Vrai si le panneau est ouvert. */
  isOpen = false;

  /** Liste complète des liens, badges et niveaux d'accès. */
  private readonly allLinks: MenuLink[] = [
    { label: 'Mon compte', badge: 'Tout le monde', color: '#0F6E56', minRank: 0 },
    { label: 'Mes points de fidélité', badge: 'Tout le monde', color: '#0F6E56', minRank: 0 },
    { label: 'Réserver un terrain', badge: 'Tout le monde', color: '#0F6E56', minRank: 0 },
    { label: 'Voir mon historique', badge: 'Tout le monde', color: '#0F6E56', minRank: 0 },
    { label: 'Mes disponibilités', badge: 'Employé +', color: '#185FA5', minRank: 1 },
    { label: 'Mon horaire', badge: 'Employé +', color: '#185FA5', minRank: 1 },
    { label: 'Gestion Cuisine', badge: 'Cuisine +', color: '#854F0B', minRank: 2 },
    { label: 'Gestion Terrains', badge: 'Cuisine +', color: '#854F0B', minRank: 2 },
    { label: 'Gestion Stocks', badge: 'Cuisine +', color: '#854F0B', minRank: 2 },
    { label: 'Créer des horaires', badge: 'Patron', color: '#A32D2D', minRank: 3 },
    { label: 'Gestion des utilisateurs', badge: 'Patron', color: '#A32D2D', minRank: 3 }
  ];

  private subscription?: Subscription;

  constructor(
    private menuState: MenuStateService,
    private authState: AuthStateService
  ) {}

  ngOnInit(): void {
    this.subscription = this.menuState.isOpen$.subscribe(open => (this.isOpen = open));
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  /** Vrai si un token valide est présent. */
  get isLoggedIn(): boolean {
    return this.authState.isLoggedIn;
  }

  /** Initiales de l'utilisateur connecté. */
  get initials(): string {
    return this.authState.currentUserInitials;
  }

  /** Nom complet "Prénom Nom" de l'utilisateur connecté. */
  get fullName(): string {
    return `${this.authState.currentUserNom ?? ''} ${this.authState.currentUserPrenom ?? ''}`.trim();
  }

  /** Liens visibles selon le rang du rôle courant. */
  get visibleLinks(): MenuLink[] {
    const rank = ROLE_RANK[this.authState.currentUserRole ?? 'Client'] ?? 0;
    return this.allLinks.filter(link => rank >= link.minRank);
  }

  /**
   * Ferme le panneau latéral.
   */
  close(): void {
    this.menuState.close();
  }
}
