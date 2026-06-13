import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthStateService } from '../../services/auth-state.service';
import { MenuStateService } from '../../services/menu-state.service';

/**
 * Barre de navigation fixe affichée par chaque page. Le contenu central est
 * projeté par la page hôte via ng-content : la navbar ne connaît aucune route.
 * Elle gère uniquement les actions à droite (connexion/inscription ou
 * avatar + hamburger selon l'état d'authentification).
 */
@Component({
  selector: 'app-navbar',
  imports: [RouterLink],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent {
  constructor(
    private authState: AuthStateService,
    private menuState: MenuStateService
  ) {}

  /** Vrai si un token valide est présent. */
  get isLoggedIn(): boolean {
    return this.authState.isLoggedIn;
  }

  /** Initiales de l'utilisateur connecté pour le cercle d'avatar. */
  get initials(): string {
    return this.authState.currentUserInitials;
  }

  /**
   * Ouvre ou ferme le panneau latéral.
   */
  toggleMenu(): void {
    this.menuState.toggle();
  }
}
