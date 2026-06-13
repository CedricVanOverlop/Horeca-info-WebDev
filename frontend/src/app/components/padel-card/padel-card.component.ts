import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

/**
 * Carte de présentation du Padel Center Lobbes.
 * Affichée sur la page d'accueil lorsque l'onglet "Padel Lobbes" est actif.
 * Le bouton "Réserver un terrain" navigue vers la route interne /reservations.
 */
@Component({
  selector: 'app-padel-card',
  imports: [RouterLink],
  templateUrl: './padel-card.component.html',
  styleUrl: './padel-card.component.css'
})
export class PadelCardComponent {}