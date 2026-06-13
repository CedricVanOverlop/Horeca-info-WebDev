import { Component, OnDestroy, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthStateService } from '../../services/auth-state.service';
import { MenuStateService } from '../../services/menu-state.service';
import { NAV, NavItem } from '../../nav.config';
import { Role } from '../../roles';

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

  /** Liens de la sidebar visibles selon le rôle courant (dérivés de NAV). */
  get visibleLinks(): NavItem[] {
    const role = (this.authState.currentUserRole ?? 'Client') as Role;
    return NAV.filter(item => item.sidebar && (item.roles?.includes(role) ?? false));
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
