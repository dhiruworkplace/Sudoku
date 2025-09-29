using System;
using System.Collections;
using System.Collections.Generic;
using PlayerIO.GameLibrary;

namespace Sudokuking
{
	public class Player : BasePlayer {
		
	}

	[RoomType("sudoku")]
	public class GameCode : Game<Player>
	{
		private int _players;
		private int JoinedPlayers {
            get { return _players; }
            set {
                _players = value;
                string maxplayer = RoomData["maxPlayers"];
                if (_players >= int.Parse(maxplayer)) {
                    Visible = false;
                    isGameStarted = true;
                    if (RoomData["playType"] == "False") {
                        StartGame();
                        timer.Stop();
                    }
                }
                else if (!isGameStarted) {
                    Visible = true;
                }
            }
        }

		private Dictionary<string, string> JoinPlayer = new Dictionary<string, string>();
        private ArrayList botPlayerList = new ArrayList();

        List<string> winners = new List<string>();
		private bool isGameStarted = false;
        private Timer timer;
        private Timer waitTimer;

        // This method is called when an instance of your the game is created
        public override void GameStarted()
		{
			// anything you write to the Console will show up in the 
			// output window of the development server
			Console.WriteLine("Game is started: " + RoomId);
            if (RoomData["playType"] == "False") {
                StartTimer();
            }
		}

		// This method is called when the last player leaves the room, and it's closed down.
		public override void GameClosed()
		{
			Console.WriteLine("RoomId: " + RoomId);
		}

		// This method is called whenever a player joins the game
		public override void UserJoined(Player player)
		{
			foreach (Player pl in Players)
			{
				if (pl.ConnectUserId != player.ConnectUserId)
				{
					//pl.Send("PlayerJoined", player.ConnectUserId, 0, 0);
					//player.Send("PlayerJoined", pl.ConnectUserId, pl.posx, pl.posz);
				}
			}
		}

		// This method is called when a player leaves the game
		public override void UserLeft(Player player)
		{
			Broadcast("PlayerLeft", player.ConnectUserId);
			if (JoinPlayer.ContainsKey(player.ConnectUserId))
			{
				JoinedPlayers--;
				JoinPlayer.Remove(player.ConnectUserId);
			}
		}

        //This method is called before a user joins a room.
        //If you return false, the user is not allowed to join.
        public override bool AllowUserJoin(Player player) {
            int maxplayer = int.Parse(RoomData["maxPlayers"]);
            if (PlayerCount >= maxplayer) {
                Visible = false;
                player.Send("NotAllowToJoin", player.ConnectUserId);
                return false;
            }
            else {
                return true;
            }
        }

        // This method is called when a player sends a message into the server code
        public override void GotMessage(Player player, Message message)
		{
			switch (message.Type)
			{
				// called when a player clicks on the ground
				case "JoinPlayer":
					string playerId = message.GetString(0);
					string playerName = message.GetString(1);

					if (!JoinPlayer.ContainsKey(playerId))
					{
						JoinedPlayers++;
						JoinPlayer.Add(playerId, playerName);
						player.Send("JoinPlayer", playerId, playerName);
						foreach (KeyValuePair<string, string> pair in JoinPlayer)
						{
							Broadcast("JoinPlayer", pair.Key, pair.Value);
						}
					}
                    Broadcast("levelType", RoomData["levelType"]);
					break;

				case "GameStart":
					Broadcast("GameStart", message.GetString(0));
					Visible = false;
					isGameStarted = true;
					break;

				case "DisconectPlayer":
					Broadcast("DisconectPlayer", player.ConnectUserId);
					if (JoinPlayer.ContainsKey(player.ConnectUserId))
					{
						JoinedPlayers--;
						JoinPlayer.Remove(player.ConnectUserId);
					}
					break;

				case "Winner":
					string playerId1 = message.GetString(0);
					if (!winners.Contains(playerId1))
					{
						Broadcast("Winner", playerId1);
						winners.Add(playerId1);
					}
					break;

				case "Mistakes":
					string playerId2 = message.GetString(0);
					int mistakes = message.GetInt(1);
					Broadcast("Mistakes", playerId2, mistakes);
					break;

				case "Progress":
					Broadcast("Progress", player.ConnectUserId, message.GetInt(0));
					break;
			}
		}

        private void StartTimer() {
            int time = 10;
            timer = AddTimer(delegate {
                time--;
                Console.WriteLine("timer:" + time);
                int playerLimit = int.Parse(RoomData["maxPlayers"]);
                int players = JoinPlayer.Count;
                Broadcast("Timer", time);
                                
                if (time == 2) {
                    Visible = false;
                }
                if (time == 0) {
                    if (players < playerLimit) {
                        GenerateBotPlayer(players, playerLimit);
                    }
                    timer.Stop();
                }
            },
            1000);
        }

        private void GenerateBotPlayer(int joinedPlayer,int maxPlayer) {
            Random random = new Random();
            for (int i = joinedPlayer + 1; i <= maxPlayer; i++) {
                string name = random.Next(0, 50).ToString();
                while (botPlayerList.Contains(name)) {
                    name = random.Next(0, 50).ToString();
                }
                botPlayerList.Add(name);
            }
            Broadcast("BotPlayers", ConvertStringArrayToStringJoin());
        }

        //it will add "," under array
        string ConvertStringArrayToStringJoin() {
            // Use string Join to concatenate the string elements.
            string pre_number_array = string.Join(",",  botPlayerList.ToArray());

            return pre_number_array;
        }

        private void StartGame(int wait = 2000) {
            waitTimer = AddTimer(delegate {
                Broadcast("GameStart", RoomData["levelType"]);
                waitTimer.Stop();
            },
            wait);
        }
    }
}