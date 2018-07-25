import { Component, Input } from '@angular/core';
import { Player } from '../../../player.model';

@Component({
  selector: 'app-area',
  templateUrl: './area.component.html',
  styleUrls: ['./area.component.css']
})
export class AreaComponent {
  @Input() player: Player = new Player();
  @Input() opponentList: Player[] = new Array<Player>();
  @Input() action: number = 0;
  @Input() cost: number = 0;

  public set() {
    this.player.setSpy(this.action);
  }

  public drop() {
    this.player.dropSpy(this.action);
  }

  public getAction(): string {
    switch (this.action) {
      case 0: return "None";
      case 1: return "Spy";
      case 2: return "Invent";
      case 3: return "Research";
      case 4: return "Work";
    }
  }
}
