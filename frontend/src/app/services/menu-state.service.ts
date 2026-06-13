import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

/**
 * Partage l'état ouvert/fermé du panneau latéral entre la navbar et le side-menu.
 * Un seul BehaviorSubject sert de source de vérité pour les deux composants.
 */
@Injectable({
  providedIn: 'root'
})
export class MenuStateService {
  private readonly _isOpen = new BehaviorSubject<boolean>(false);

  /** Flux observable de l'état d'ouverture du panneau latéral. */
  public readonly isOpen$: Observable<boolean> = this._isOpen.asObservable();

  /**
   * Inverse l'état d'ouverture du panneau latéral.
   */
  public toggle(): void {
    this._isOpen.next(!this._isOpen.value);
  }

  /**
   * Ouvre le panneau latéral.
   */
  public open(): void {
    this._isOpen.next(true);
  }

  /**
   * Ferme le panneau latéral.
   */
  public close(): void {
    this._isOpen.next(false);
  }
}
