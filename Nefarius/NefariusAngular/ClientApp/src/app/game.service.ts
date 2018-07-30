import { HubConnection, HubConnectionBuilder, LogLevel } from "@aspnet/signalr";

import { Game } from "./game.model";
import { Player } from "./player.model";
import { Table } from "./table.model";

export class GameService {
  getTableList(): Table[] {
    return this.tableList;
  }

  private connection: HubConnection;
  public tableList: Table[] = [];
  //public player: Player;
  public game: Game = new Game();

  constructor() {
    this.connection = new HubConnectionBuilder()
      .withUrl("/game")
      .configureLogging(LogLevel.Information)
      .build();

    this.connection.on("GameData", (data) => {
      this.game = Object.assign(this.game, data);
      console.log('Game data recieved');
    });
    this.connection.on("PlayerData", (data) => {
      //this.game.player = Object.assign(this.game.player, data);
      //this.player = data;
      console.log('Player data recieved');
    });
    this.connection.on("StateChanged", (data) => {
      //this.game = Object.assign(this.game, data);
      //this.game = data;
      console.log('Game data recieved');
    });
    this.connection.on("TableList", (data) => {
      //this.tableList.length = 0;
      this.tableList = Object.assign(this.tableList, data.tableList);
      //this.tableList = data.tableList;
    });
    this.connection.onclose(e => {
      alert(e);
    });

    //// Установим начальный список с именем "Всем". При его выборе
    //// сообщения будут отправлены всем пользователям, кроме текущего
    //this.Users = [{ label: "Всем", value: "" }];
    //this.connectionExists = false;
    //this.isRegistered = false;

    //this.chat = $.connection.helloHub;
    //this.server = this.chat.server;
    //this.client = this.chat.client;

    //// Установим обработчики событий
    //this.registerOnServerEvents();
    //this.allMessages = new Array<ChatMessage>();

    // Подсоединимся к Хабу
    this.startConnection();
  }

  private startConnection(): void {
    this.connection
      .start()
      .then(() => {
        console.log('Connection started!');
        this.connection.send('GetTableList');
      })
      .catch(err => console.log('Error while establishing connection :('));
  }

  public join(tableName: string, playerName: string) {
    this.connection.send('Join', tableName, playerName);
    console.log('Join req sended');
  }

  public leave() {
    this.connection.send('Leave');
  }

  public begin() {
    this.connection.send('Begin');
  }

  public end() {
    this.connection.send('End');
  }

  public turn(action: number) {
    this.connection.send('Turn', action);
  }

  public setSpy(area: number) {
    this.connection.send('Spy', area);
  }

  public dropSpy(area: number) {
    this.connection.send('Spy', 0, area);
  }
}
