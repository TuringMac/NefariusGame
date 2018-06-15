import { Component } from '@angular/core';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent {
  public gameState = 0;
  public action = 0;

  public turnSpy() {
    this.action = 1;
  }

  public turnInvent() {
    this.action = 2;
  }

  public turnResearch() {
    this.action = 3;
  }

  public turnWork() {
    this.action = 4;
  }
}
