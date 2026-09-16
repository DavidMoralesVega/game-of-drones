import { Component, input } from '@angular/core';
import { Game } from '../../core/models';

@Component({
  selector: 'app-score-board',
  templateUrl: './score-board.html',
})
export class ScoreBoard {
  readonly game = input.required<Game>();
}
