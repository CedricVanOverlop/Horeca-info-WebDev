import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

/**
 * Contenu de l'onglet Padel Lobbes sur la home page.
 * Présentation statique, sans appel HTTP. Bouton non fonctionnel pour l'instant.
 */
@Component({
  selector: 'app-padel-card',
  imports: [RouterLink],
  templateUrl: './padel-card.component.html',
  styleUrl: './padel-card.component.css'
})
export class PadelCardComponent {}
