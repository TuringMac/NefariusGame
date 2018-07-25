import { Player } from "./player.model";

export class Game {
  public player: Player;
  public players: Player[];
  public state: number;
  public move: number;
  public table: string;
}
