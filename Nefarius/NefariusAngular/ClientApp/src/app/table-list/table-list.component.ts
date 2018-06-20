import { Component, OnInit } from '@angular/core';
import { GameService } from '../game.service';
import { Table } from '../table.model';

@Component({
  selector: 'app-table-list',
  templateUrl: './table-list.component.html',
  styleUrls: ['./table-list.component.css']
})

export class TableListComponent implements OnInit {
  public name = 'Player';
  public tableList: Table[] = [];

  constructor(private gameService: GameService) { }

  ngOnInit(): void {
    this.refresh();
  }

  public join(tableName) {
    this.gameService.join(tableName, this.name);
  }

  public refresh() {
    this.tableList = this.gameService.getTableList();
  }
}
