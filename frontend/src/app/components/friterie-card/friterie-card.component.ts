import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

/**
 * Contenu de l'onglet Friterie.net sur la home page.
 * Présentation statique, sans appel HTTP. Boutons non fonctionnels pour l'instant.
 */
@Component({
  selector: 'app-friterie-card',
  imports: [RouterLink],
  templateUrl: './friterie-card.component.html',
  styleUrl: './friterie-card.component.css'
})
export class FriterieCardComponent {}
