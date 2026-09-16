import { Injectable, computed, inject, signal } from '@angular/core';
import { errorMessage } from './error-message';
import { GameApi } from './game-api';
import { Game, Move, Player } from './models';

export type Phase = 'player1' | 'handoff' | 'player2';

@Injectable({ providedIn: 'root' })
export class GameStore {
  private readonly api = inject(GameApi);

  private readonly gameState = signal<Game | null>(null);
  private readonly movesState = signal<Move[]>([]);
  private readonly phaseState = signal<Phase>('player1');
  private readonly player1MoveId = signal<number | null>(null);
  private readonly busyState = signal(false);
  private readonly errorState = signal<string | null>(null);

  readonly game = this.gameState.asReadonly();
  readonly moves = this.movesState.asReadonly();
  readonly phase = this.phaseState.asReadonly();
  readonly busy = this.busyState.asReadonly();
  readonly error = this.errorState.asReadonly();

  readonly hasGameInProgress = computed(() => this.gameState()?.isFinished === false);
  readonly hasFinishedGame = computed(() => this.gameState()?.isFinished === true);
  readonly roundNumber = computed(() => (this.gameState()?.rounds.length ?? 0) + 1);
  readonly currentPlayer = computed<Player | null>(() => {
    const game = this.gameState();
    if (!game) {
      return null;
    }

    return this.phaseState() === 'player1' ? game.player1 : game.player2;
  });

  async start(player1Name: string, player2Name: string): Promise<boolean> {
    return this.run(async () => {
      const [game, moves] = await Promise.all([
        this.api.createGame(player1Name, player2Name),
        this.api.getMoves(),
      ]);

      this.gameState.set(game);
      this.movesState.set(moves);
      this.phaseState.set('player1');
      this.player1MoveId.set(null);
    });
  }

  async choose(moveId: number): Promise<boolean> {
    const game = this.gameState();
    if (!game) {
      return false;
    }

    if (this.phaseState() === 'player1') {
      this.player1MoveId.set(moveId);
      this.phaseState.set('handoff');
      return true;
    }

    const player1MoveId = this.player1MoveId();
    if (this.phaseState() !== 'player2' || player1MoveId === null) {
      return false;
    }

    return this.run(async () => {
      this.gameState.set(await this.api.playRound(game.id, player1MoveId, moveId));
      this.phaseState.set('player1');
      this.player1MoveId.set(null);
    });
  }

  beginPlayer2Turn(): void {
    if (this.phaseState() === 'handoff') {
      this.phaseState.set('player2');
    }
  }

  reset(): void {
    this.gameState.set(null);
    this.phaseState.set('player1');
    this.player1MoveId.set(null);
    this.errorState.set(null);
  }

  private async run(action: () => Promise<void>): Promise<boolean> {
    this.busyState.set(true);
    this.errorState.set(null);

    try {
      await action();
      return true;
    } catch (error) {
      this.errorState.set(errorMessage(error));
      return false;
    } finally {
      this.busyState.set(false);
    }
  }
}
