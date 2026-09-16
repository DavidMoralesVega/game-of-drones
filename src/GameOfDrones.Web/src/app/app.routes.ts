import { inject } from '@angular/core';
import { CanActivateFn, Router, Routes } from '@angular/router';
import { GameStore } from './core/game-store';

const requireGameInProgress: CanActivateFn = () =>
  inject(GameStore).hasGameInProgress() || inject(Router).createUrlTree(['/']);

const requireFinishedGame: CanActivateFn = () =>
  inject(GameStore).hasFinishedGame() || inject(Router).createUrlTree(['/']);

const requireNoGameInProgress: CanActivateFn = () =>
  !inject(GameStore).hasGameInProgress() || inject(Router).createUrlTree(['/game']);

export const routes: Routes = [
  { path: '', loadComponent: () => import('./features/start/start').then((m) => m.StartPage) },
  {
    path: 'game',
    canActivate: [requireGameInProgress],
    loadComponent: () => import('./features/game/game').then((m) => m.GamePage),
  },
  {
    path: 'winner',
    canActivate: [requireFinishedGame],
    loadComponent: () => import('./features/winner/winner').then((m) => m.WinnerPage),
  },
  {
    path: 'rules',
    canActivate: [requireNoGameInProgress],
    loadComponent: () => import('./features/rules/rules').then((m) => m.RulesPage),
  },
  { path: '**', redirectTo: '' },
];
