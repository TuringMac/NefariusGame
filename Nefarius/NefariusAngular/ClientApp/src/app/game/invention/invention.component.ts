import { Component, Input } from '@angular/core';
import { Invention } from '../../invention.model';

@Component({
  selector: 'app-invention',
  templateUrl: './invention.component.html',
  styleUrls: ['./invention.component.css']
})
export class InventionComponent {
  @Input() invention: Invention = new Invention();

  public invPlay() {

  }
}
