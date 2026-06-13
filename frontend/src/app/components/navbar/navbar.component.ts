import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { Subscription, filter } from 'rxjs';
import { AuthStateService } from '../../services/auth-state.service';
import { MenuStateService } from '../../services/menu-state.service';
import { CommerceTab, TabStateService } from '../../services/tab-state.service';

/**
 * Barre de navigation fixe affichée sur toutes les pages.
 * Affiche les onglets commerce uniquement sur la route '/', et bascule
 * entre l'état déconnecté (boutons connexion/inscription) et connecté
 * (initiales + hamburger ouvrant le side-menu).
 */
@Component({
  selector: 'app-navbar',
  imports: [CommonModule, RouterLink],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css'
})
export class NavbarComponent implements OnInit, OnDestroy {
  /** Onglet commerce actif, synchronisé avec le TabStateService. */
  activeTab: CommerceTab = 'friterie';

  /** Vrai si l'utilisateur se trouve sur la route '/' (onglets visibles). */
  isHome = false;

  private subscriptions = new Subscription();

  constructor(
    private router: Router,
    private authState: AuthStateService,
    private menuState: MenuStateService,
    private tabState: TabStateService
  ) {}

  ngOnInit(): void {
    this.isHome = this.router.url === '/';
    this.subscriptions.add(
      this.router.events
        .pipe(filter(e => e instanceof NavigationEnd))
        .subscribe((e) => {
          this.isHome = (e as NavigationEnd).urlAfterRedirects === '/';
        })
    );
    this.subscriptions.add(
      this.tabState.activeTab$.subscribe(tab => (this.activeTab = tab))
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  /** Vrai si un token valide est présent. */
  get isLoggedIn(): boolean {
    return this.authState.isLoggedIn;
  }

  /** Initiales de l'utilisateur connecté pour le cercle d'avatar. */
  get initials(): string {
    return this.authState.currentUserInitials;
  }

  /**
   * Active l'onglet commerce sélectionné.
   * @param tab Onglet à activer
   */
  selectTab(tab: CommerceTab): void {
    this.tabState.setTab(tab);
  }

  /**
   * Ouvre ou ferme le panneau latéral.
   */
  toggleMenu(): void {
    this.menuState.toggle();
  }
}
