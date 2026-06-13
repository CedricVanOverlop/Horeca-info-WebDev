import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { CommerceTab, TabStateService } from '../../services/tab-state.service';
import { NavbarComponent } from '../../components/navbar/navbar.component';
import { FriterieCardComponent } from '../../components/friterie-card/friterie-card.component';
import { GlacesCardComponent } from '../../components/glaces-card/glaces-card.component';
import { PadelCardComponent } from '../../components/padel-card/padel-card.component';

/**
 * Page d'accueil publique. Inclut elle-même la navbar et y projette les
 * onglets commerce. Affiche la card correspondant à l'onglet actif, dont la
 * sélection est partagée via le TabStateService.
 */
@Component({
  selector: 'app-home-page',
  imports: [
    NavbarComponent,
    FriterieCardComponent,
    GlacesCardComponent,
    PadelCardComponent
  ],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.css'
})
export class HomePageComponent implements OnInit, OnDestroy {
  /** Onglet commerce actif (Friterie.net par défaut). */
  activeTab: CommerceTab = 'friterie';

  private subscription?: Subscription;

  constructor(private tabState: TabStateService) {}

  ngOnInit(): void {
    this.subscription = this.tabState.activeTab$.subscribe(tab => (this.activeTab = tab));
  }

  ngOnDestroy(): void {
    this.subscription?.unsubscribe();
  }

  /**
   * Active l'onglet commerce sélectionné.
   * @param tab Onglet à activer
   */
  selectTab(tab: CommerceTab): void {
    this.tabState.setTab(tab);
  }
}
