import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientModule } from '@angular/common/http';
import { RouterModule } from '@angular/router';

import { AppComponent } from './app.component';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { GameComponent } from './game/game.component';
import { PlayerComponent } from './game/player/player.component';
import { OpponentComponent } from './game/opponent/opponent.component';
import { InventionComponent } from './game/invention/invention.component';
import { TableListComponent } from './table-list/table-list.component';

import { GameService } from './game.service';

@NgModule({
  declarations: [
    AppComponent,
    NavMenuComponent,
    TableListComponent,
    GameComponent,
    PlayerComponent,
    OpponentComponent,
    InventionComponent,
  ],
  imports: [
    BrowserModule.withServerTransition({ appId: 'ng-cli-universal' }),
    HttpClientModule,
    FormsModule,
    RouterModule.forRoot([
      { path: '', component: TableListComponent, pathMatch: 'full' },
      { path: 'game', component: GameComponent },
      //{ path: 'winner', component: WinnComponent },
    ])
  ],
  providers: [GameService],
  bootstrap: [AppComponent]
})
export class AppModule { }
