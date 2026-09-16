import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { GameApi } from './game-api';
import { GameStore } from './game-store';
import { Game, Move } from './models';

const moves: Move[] = [
  { id: 1, name: 'Rock', kills: [3] },
  { id: 2, name: 'Paper', kills: [1] },
  { id: 3, name: 'Scissors', kills: [2] },
];

function game(overrides: Partial<Game> = {}): Game {
  return {
    id: 7,
    player1: { id: 1, name: 'Alice' },
    player2: { id: 2, name: 'Bob' },
    player1Score: 0,
    player2Score: 0,
    winsToConquer: 3,
    isFinished: false,
    winner: null,
    rounds: [],
    ...overrides,
  };
}

function rejected(detail: string): HttpErrorResponse {
  return new HttpErrorResponse({ status: 400, error: { detail } });
}

describe('GameStore', () => {
  const api = { createGame: vi.fn(), getMoves: vi.fn(), playRound: vi.fn() };
  let store: GameStore;

  beforeEach(() => {
    vi.resetAllMocks();
    api.createGame.mockResolvedValue(game());
    api.getMoves.mockResolvedValue(moves);
    TestBed.configureTestingModule({ providers: [{ provide: GameApi, useValue: api }] });
    store = TestBed.inject(GameStore);
  });

  it('starts a game and asks player 1 first', async () => {
    const started = await store.start('Alice', 'Bob');

    expect(started).toBe(true);
    expect(api.createGame).toHaveBeenCalledWith('Alice', 'Bob');
    expect(store.game()?.id).toBe(7);
    expect(store.moves()).toEqual(moves);
    expect(store.phase()).toBe('player1');
    expect(store.currentPlayer()?.name).toBe('Alice');
    expect(store.roundNumber()).toBe(1);
    expect(store.hasGameInProgress()).toBe(true);
  });

  it('keeps the first move hidden and hands the device over', async () => {
    await store.start('Alice', 'Bob');

    const accepted = await store.choose(2);

    expect(accepted).toBe(true);
    expect(api.playRound).not.toHaveBeenCalled();
    expect(store.phase()).toBe('handoff');
    expect(store.currentPlayer()?.name).toBe('Bob');
  });

  it('sends both moves once player 2 has chosen', async () => {
    const afterRound = game({
      player1Score: 1,
      rounds: [{ number: 1, player1Move: 'Paper', player2Move: 'Rock', winner: 'Alice' }],
    });
    api.playRound.mockResolvedValue(afterRound);
    await store.start('Alice', 'Bob');
    await store.choose(2);
    store.beginPlayer2Turn();

    const accepted = await store.choose(1);

    expect(accepted).toBe(true);
    expect(api.playRound).toHaveBeenCalledWith(7, 2, 1);
    expect(store.game()).toEqual(afterRound);
    expect(store.phase()).toBe('player1');
    expect(store.roundNumber()).toBe(2);
  });

  it('ignores a choice while the device is being handed over', async () => {
    await store.start('Alice', 'Bob');
    await store.choose(2);

    const accepted = await store.choose(1);

    expect(accepted).toBe(false);
    expect(api.playRound).not.toHaveBeenCalled();
    expect(store.phase()).toBe('handoff');
  });

  it('shows the API message and lets player 2 retry when a round is rejected', async () => {
    api.playRound.mockRejectedValue(rejected('Move 9 does not exist.'));
    await store.start('Alice', 'Bob');
    await store.choose(2);
    store.beginPlayer2Turn();

    const accepted = await store.choose(9);

    expect(accepted).toBe(false);
    expect(store.error()).toBe('Move 9 does not exist.');
    expect(store.phase()).toBe('player2');
    expect(store.busy()).toBe(false);
  });

  it('flags the finished game so the winner screen can open', async () => {
    api.playRound.mockResolvedValue(game({ player1Score: 3, isFinished: true, winner: 'Alice' }));
    await store.start('Alice', 'Bob');
    await store.choose(2);
    store.beginPlayer2Turn();

    await store.choose(1);

    expect(store.hasFinishedGame()).toBe(true);
    expect(store.hasGameInProgress()).toBe(false);
  });

  it('reports a failed start with the server detail', async () => {
    api.createGame.mockRejectedValue(rejected('Players must have different names.'));

    const started = await store.start('Alice', 'alice');

    expect(started).toBe(false);
    expect(store.game()).toBeNull();
    expect(store.error()).toBe('Players must have different names.');
  });

  it('forgets everything on reset', async () => {
    await store.start('Alice', 'Bob');
    await store.choose(2);

    store.reset();

    expect(store.game()).toBeNull();
    expect(store.phase()).toBe('player1');
    expect(store.currentPlayer()).toBeNull();
    expect(store.error()).toBeNull();
  });
});
