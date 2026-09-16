export interface Player {
  id: number;
  name: string;
}

export interface Round {
  number: number;
  player1Move: string;
  player2Move: string;
  winner: string | null;
}

export interface Game {
  id: number;
  player1: Player;
  player2: Player;
  player1Score: number;
  player2Score: number;
  winsToConquer: number;
  isFinished: boolean;
  winner: string | null;
  rounds: Round[];
}

export interface Move {
  id: number;
  name: string;
  kills: number[];
}

export interface PlayerStats {
  name: string;
  gamesWon: number;
  gamesPlayed: number;
}
