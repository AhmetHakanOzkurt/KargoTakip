import { KeyboardEvent } from 'react';

/**
 * Tiklanabilir div'ler klavyeyle kullanilamiyordu (SonarCloud:
 * "Visible, non-interactive elements with click handlers must have at
 * least one keyboard listener"). Bu yardimci, karti buton gibi
 * davranacak sekilde isaretler: Tab ile odaklanilir, Enter ve Space
 * ile tetiklenir.
 */
export function tiklanabilirKart(onActivate: () => void) {
  return {
    role: 'button',
    tabIndex: 0,
    onClick: onActivate,
    onKeyDown: (e: KeyboardEvent) => {
      if (e.key === 'Enter' || e.key === ' ') {
        e.preventDefault();
        onActivate();
      }
    }
  };
}
