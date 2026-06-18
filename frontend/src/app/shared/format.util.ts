/**
 * Petites fonctions de formatage partagées par les pages/composants Padel,
 * pour éviter de redéfinir les mêmes one-liners dans chaque composant.
 */

/** "HH:mm:ss" → "HH:mm". */
export function formatHeure(time: string): string {
  return time.slice(0, 5);
}

/** Date ISO ("YYYY-MM-DD…") → date courte fr-BE ("JJ/MM/AAAA"), ou "—" si invalide. */
export function formatDate(iso: string): string {
  const d = new Date(iso);
  return isNaN(d.getTime()) ? '—' : d.toLocaleDateString('fr-BE');
}
