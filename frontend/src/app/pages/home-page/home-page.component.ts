import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { CommerceTab, TabStateService } from '../../services/tab-state.service';
import { FriterieCardComponent } from '../../components/friterie-card/friterie-card.component';
import { GlacesCardComponent } from '../../components/glaces-card/glaces-card.component';
import { PadelCardComponent } from '../../components/padel-card/padel-card.component';

/**
 * Page d'accueil publique. Affiche la card commerce correspondant à l'onglet
 * actif, dont la sélection est partagée avec la navbar via le TabStateService.
 */
@Component({
  selector: 'app-home-page',
  imports: [CommonModule, FriterieCardComponent, GlacesCardComponent, PadelCardComponent],
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
}
