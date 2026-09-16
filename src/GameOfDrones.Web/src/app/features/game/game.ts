import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { GameStore } from '../../core/game-store';
import { ScoreBoard } from '../../shared/score-board/score-board';

@Component({
  selector: 'app-game',
  imports: [FormsModule, ScoreBoard],
  templateUrl: './game.html',
})
export class GamePage {
  private readonly router = inject(Router);
  protected readonly store = inject(GameStore);

  protected readonly selectedMoveId = signal<number | null>(null);

  protected async confirm(): Promise<void> {
    const moveId = this.selectedMoveId();
    if (moveId === null) {
      return;
    }

    const accepted = await this.store.choose(moveId);
    this.selectedMoveId.set(null);

    if (accepted && this.store.hasFinishedGame()) {
      await this.router.navigate(['/winner']);
    }
  }
}
