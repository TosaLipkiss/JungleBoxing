using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameStatus : MonoBehaviour
{
	public float timer;
	public GameObject statusMessage;
    public Text status;
    public Text player1;
    public Text player2;

    private void Update()
    {
		timer += Time.deltaTime;
    }
    internal void StartGame(GameInfo game)
	{
		statusMessage.SetActive(true);
		player1.text = game.player1.userID;
		player2.text = game.player2.userID;

		if (game.player2.userID == "" || game.player2 == null)
		{
			status.text = "Waiting for opponent to join";
			timer = 0f;
		}
		else
		{
			status.text = "Game starting...";
			if(timer > 1.0f)
            {
				statusMessage.SetActive(false);
			}
		}
	}
}
