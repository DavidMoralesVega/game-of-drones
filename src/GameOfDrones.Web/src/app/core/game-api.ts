import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import { Game, Move, PlayerStats } from './models';

@Injectable({ providedIn: 'root' })
export class GameApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiUrl;

  createGame(player1Name: string, player2Name: string): Promise<Game> {
    return firstValueFrom(
      this.http.post<Game>(`${this.baseUrl}/games`, { player1Name, player2Name }),
    );
  }

  playRound(gameId: number, player1MoveId: number, player2MoveId: number): Promise<Game> {
    return firstValueFrom(
      this.http.post<Game>(`${this.baseUrl}/games/${gameId}/rounds`, {
        player1MoveId,
        player2MoveId,
      }),
    );
  }

  getMoves(): Promise<Move[]> {
    return firstValueFrom(this.http.get<Move[]>(`${this.baseUrl}/moves`));
  }

  createMove(name: string): Promise<Move> {
    return firstValueFrom(this.http.post<Move>(`${this.baseUrl}/moves`, { name }));
  }

  deleteMove(moveId: number): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${this.baseUrl}/moves/${moveId}`));
  }

  updateMoveKills(moveId: number, kills: number[]): Promise<Move> {
    return firstValueFrom(this.http.put<Move>(`${this.baseUrl}/moves/${moveId}/kills`, { kills }));
  }

  getPlayerStats(): Promise<PlayerStats[]> {
    return firstValueFrom(this.http.get<PlayerStats[]>(`${this.baseUrl}/players/stats`));
  }
}
