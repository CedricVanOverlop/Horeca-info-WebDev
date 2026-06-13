import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

/**
 * Contenu de l'onglet Baraque à Glaces sur la home page.
 * Présentation statique, sans appel HTTP. Boutons non fonctionnels pour l'instant.
 */
@Component({
  selector: 'app-glaces-card',
  imports: [RouterLink],
  templateUrl: './glaces-card.component.html',
  styleUrl: './glaces-card.component.css'
})
export class GlacesCardComponent {}
