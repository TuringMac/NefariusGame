import { Component, Input } from '@angular/core';
import { Player } from '../../player.model';

@Component({
  selector: 'app-opponent',
  templateUrl: './opponent.component.html',
  styleUrls: ['./opponent.component.css']
})
export class OpponentComponent {
  @Input() opponent: Player;
}
