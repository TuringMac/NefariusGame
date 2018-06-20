import { Component } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel } from '@aspnet/signalr';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'app';
  public hubConnection: HubConnection;
  //nick = '';
  //message = '';
  //messages: string[] = [];

  ngOnInit() {
    //this.nick = window.prompt('Your name:', 'John');

    this.hubConnection = new HubConnectionBuilder()
      .withUrl("/game")
      .configureLogging(LogLevel.Information)
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('Connection started!'))
      .catch(err => console.log('Error while establishing connection :('));

  }
}
