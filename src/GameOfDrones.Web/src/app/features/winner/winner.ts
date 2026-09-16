import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { GameStore } from '../../core/game-store';
import { ScoreBoard } from '../../shared/score-board/score-board';

@Component({
  selector: 'app-winner',
  imports: [RouterLink, ScoreBoard],
  templateUrl: './winner.html',
})
export class WinnerPage {
  protected readonly store = inject(GameStore);
}
