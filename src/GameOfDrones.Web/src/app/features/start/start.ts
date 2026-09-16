import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { GameApi } from '../../core/game-api';
import { GameStore } from '../../core/game-store';
import { PlayerStats } from '../../core/models';

@Component({
  selector: 'app-start',
  imports: [FormsModule, RouterLink],
  templateUrl: './start.html',
})
export class StartPage {
  private readonly router = inject(Router);
  private readonly api = inject(GameApi);
  protected readonly store = inject(GameStore);

  protected player1Name = '';
  protected player2Name = '';
  protected readonly stats = signal<PlayerStats[]>([]);

  constructor() {
    this.store.reset();
    this.api
      .getPlayerStats()
      .then((stats) => this.stats.set(stats))
      .catch(() => this.stats.set([]));
  }

  protected async start(): Promise<void> {
    if (await this.store.start(this.player1Name, this.player2Name)) {
      await this.router.navigate(['/game']);
    }
  }
}
