import { Component, Input } from '@angular/core';
import { Player } from '../../player.model';

@Component({
  selector: 'app-field',
  templateUrl: './field.component.html',
  styleUrls: ['./field.component.css']
})
export class FieldComponent {
  @Input() player: Player = new Player();
  @Input() opponentList: Player[] = new Array<Player>();

  public set() {
  }

  public drop() {
  }
}
