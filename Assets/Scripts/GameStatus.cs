using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStatus : MonoBehaviour
{
    public Text status;
    public Text player1;
    public Text player2;
	internal void StartGame(GameInfo game)
	{
		player1.text = game.player1.userID;
		player2.text = game.player2.userID;

		if (game.player2.userID == "" || game.player2 == null)
		{
			status.text = "Waiting for opponent to join";
		}
		else
		{
			status.text = "Game started!";
		}

		//TODO: listen to changes
		//TODO: add more game logic
	}
}
