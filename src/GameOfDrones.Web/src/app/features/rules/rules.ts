import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { errorMessage } from '../../core/error-message';
import { GameApi } from '../../core/game-api';
import { Move } from '../../core/models';

@Component({
  selector: 'app-rules',
  imports: [FormsModule, RouterLink],
  templateUrl: './rules.html',
})
export class RulesPage {
  private readonly api = inject(GameApi);

  protected readonly moves = signal<Move[]>([]);
  protected readonly error = signal<string | null>(null);
  protected readonly busy = signal(false);
  protected newMoveName = '';

  constructor() {
    this.reload();
  }

  protected kills(move: Move, target: Move): boolean {
    return move.kills.includes(target.id);
  }

  protected async addMove(): Promise<void> {
    await this.save(async () => {
      await this.api.createMove(this.newMoveName);
      this.newMoveName = '';
    });
  }

  protected async removeMove(move: Move): Promise<void> {
    await this.save(() => this.api.deleteMove(move.id));
  }

  protected async toggleKill(event: Event, move: Move, target: Move): Promise<void> {
    event.preventDefault();

    const kills = this.kills(move, target)
      ? move.kills.filter((id) => id !== target.id)
      : [...move.kills, target.id];

    await this.save(async () => {
      await this.api.updateMoveKills(move.id, kills);
    });
  }

  private async save(action: () => Promise<void>): Promise<void> {
    this.busy.set(true);
    this.error.set(null);

    try {
      await action();
    } catch (error) {
      this.error.set(errorMessage(error));
    } finally {
      await this.reload();
      this.busy.set(false);
    }
  }

  private async reload(): Promise<void> {
    this.moves.set(await this.api.getMoves());
  }
}
