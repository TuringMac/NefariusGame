import { Component } from '@angular/core';

@Component({
  selector: 'app-table-list',
  templateUrl: './table-list.component.html',
  styleUrls: ['./table-list.component.css']
})
export class TableListComponent {
  public name = 'Player';
  public tableList = ["RU table", "Friends game", "SPb table", "Fluxbit emloyee are wellcome"];

  public join(tableName) {
    console.debug('table:' + tableName + ' player:' + this.name);
    hubConnection.Send();
  }
}
