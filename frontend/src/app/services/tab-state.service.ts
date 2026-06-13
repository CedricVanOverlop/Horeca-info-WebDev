import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

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
  private readonly _activeTab = new BehaviorSubject<CommerceTab>('friterie');

  /** Flux observable de l'onglet commerce actif. */
  public readonly activeTab$: Observable<CommerceTab> = this._activeTab.asObservable();

  /**
   * Sélectionne l'onglet commerce actif.
   * @param tab Onglet à activer
   */
  public setTab(tab: CommerceTab): void {
    this._activeTab.next(tab);
  }
}
