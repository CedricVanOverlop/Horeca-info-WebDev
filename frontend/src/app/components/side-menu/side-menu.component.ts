import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthStateService } from '../../services/auth-state.service';
import { MenuStateService } from '../../services/menu-state.service';

/** Lien du menu : libellé, route cible et rôles autorisés à le voir. */
interface MenuLink {
  label: string;
  route: string;
  roles: string[];
}

/**
 * Panneau latéral piloté par le MenuStateService.
 * Affiche un message d'invitation si déconnecté, sinon le profil de
 * l'utilisateur et la liste des liens filtrés selon son rôle.
 */
@Component({
  selector: 'app-side-menu',
  imports: [RouterLink],
  templateUrl: './side-menu.component.html',
  styleUrl: './side-menu.component.css'
})
export class SideMenuComponent implements OnInit, OnDestroy {
  /** Vrai si le panneau est ouvert. */
  isOpen = false;

  /** Liste complète des liens, badges et niveaux d'accès. */
  private readonly allLinks: MenuLink[] = [
    { label: 'Mon compte', route: '/mon-compte', roles: ['Client', 'Employe', 'Cuisine', 'Administrateur'] },
    { label: 'Mes points de fidélité', route: '/fidelite', roles: ['Client', 'Employe', 'Administrateur'] },
    { label: 'Réserver un terrain', route: '/padel', roles: ['Client', 'Employe', 'Cuisine', 'Administrateur'] },
    { label: 'Mes disponibilités', route: '/disponibilites', roles: ['Employe', 'Administrateur'] },
    { label: 'Mon horaire', route: '/mon-horaire', roles: ['Employe', 'Administrateur'] },
    { label: 'Gestion Cuisine', route: '/cuisine', roles: ['Cuisine', 'Administrateur'] },
    { label: 'Gestion Terrains', route: '/terrains', roles: ['Cuisine', 'Administrateur'] },
    { label: 'Gestion Stocks', route: '/stocks', roles: ['Cuisine', 'Administrateur'] },
    { label: 'Créer des horaires', route: '/horaires', roles: ['Administrateur'] },
    { label: 'Gestion des utilisateurs', route: '/utilisateurs', roles: ['Administrateur'] }
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

  /** Liens visibles selon le rôle courant. */
  get visibleLinks(): MenuLink[] {
    const role = this.authState.currentUserRole ?? 'Client';
    return this.allLinks.filter(link => link.roles.includes(role));
  }

  /**
   * Ferme le panneau latéral.
   */
  close(): void {
    this.menuState.close();
  }

  /**
   * Déconnecte l'utilisateur (purge le token, redirige vers /login) et ferme le panneau.
   */
  logout(): void {
    this.menuState.close();
    this.authState.logout();
  }
}
