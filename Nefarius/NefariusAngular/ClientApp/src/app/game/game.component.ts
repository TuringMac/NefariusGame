import { Component, OnInit } from '@angular/core';

import { PlayerComponent } from './player/player.component';
import { OpponentComponent } from './opponent/opponent.component';
import { InventionComponent } from './invention/invention.component';
import { GameService } from '../game.service';
import { Game } from '../game.model';
import { Player } from '../player.model';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent implements OnInit {
  public game: Game;
  public player: Player;

  constructor(private gameService: GameService) { }

  public gameBegin() {
    this.gameService.begin();
  }

  public gameEnd(action: number) {
    this.gameService.end();
  }

  public gameTurn(action: number) {
    this.player.action = action;
  }

  public gameLeave() {
    this.gameService.leave();
  }

  ngOnInit(): void {
    this.game = this.gameService.game;
    this.player = this.gameService.player;
  }
}
