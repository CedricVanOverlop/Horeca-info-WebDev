import { Injectable, signal } from '@angular/core';

/**
 * Partage l'état ouvert/fermé du panneau latéral entre la navbar et le side-menu.
 * Un seul signal sert de source de vérité pour les deux composants.
 */
@Injectable({
  providedIn: 'root'
})
export class MenuStateService {
  private readonly _isOpen = signal(false);

  /** Signal en lecture seule de l'état d'ouverture du panneau latéral. */
  public readonly isOpen = this._isOpen.asReadonly();

  /**
   * Inverse l'état d'ouverture du panneau latéral.
   */
  public toggle(): void {
    this._isOpen.update(open => !open);
  }

  /**
   * Ouvre le panneau latéral.
   */
  public open(): void {
    this._isOpen.set(true);
  }

  /**
   * Ferme le panneau latéral.
   */
  public close(): void {
    this._isOpen.set(false);
  }
}
