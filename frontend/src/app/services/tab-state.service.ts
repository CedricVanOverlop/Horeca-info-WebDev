import { Injectable, signal } from '@angular/core';

/** Onglet commerce actuellement sélectionné sur la home page. */
export type CommerceTab = 'friterie' | 'glaces' | 'padel';

/**
 * Partage l'onglet commerce actif entre la navbar (qui affiche les onglets)
 * et la home page (qui affiche la card correspondante).
 */
@Injectable({
  providedIn: 'root'
})
export class TabStateService {
  private readonly _activeTab = signal<CommerceTab>('friterie');

  /** Signal en lecture seule de l'onglet commerce actif. */
  public readonly activeTab = this._activeTab.asReadonly();

  /**
   * Sélectionne l'onglet commerce actif.
   * @param tab Onglet à activer
   */
  public setTab(tab: CommerceTab): void {
    this._activeTab.set(tab);
  }
}
