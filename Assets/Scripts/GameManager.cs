using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerInfo
{
    public string displayName;
    public string userID;
    public int currentHealth;
    public int maxHealth;
    public int power;
    public BlockSideState blockState;
}

[Serializable]
public class UserInfo
{
    public string name;
    public string activeGame;
}

[Serializable]
public class GameInfo
{
    public string displayGameName;
    public string gameID;
    public List<PlayerInfo> players;
}

public enum BlockSideState : int
{
    None = 0,
    Left = 1,
    Right = 2
}

public class GameManager : MonoBehaviour
{
    public Animator playerOneAnimator;
    public Animator playerTwoAnimator;

    string turn;
    string me;
    string selection = null;

    PlayerInfo enemyPlayer;
    PlayerInfo myPlayer;

    PlayerInfo player1;
    PlayerInfo player2;

    public Image playerHealthBar;
    public Image enemyHealthBar;

    GameState currentGameState;

    enum GameState
    {
        enemyTurn,
        playerSelection,
        performSelection,
        updateDatabase
    }

    private void Start()
    {
        playerOneAnimator.SetTrigger("Idle");
        playerTwoAnimator.SetTrigger("Idle");

        turn = "Player1";

        player1 = new PlayerInfo();
        player1.currentHealth = 100;
        player1.maxHealth = 100;
        player1.power = 0;
        player1.blockState = BlockSideState.None;

        player2 = new PlayerInfo();
        player2.currentHealth = 100;
        player2.maxHealth = 100;
        player2.power = 0;
        player2.blockState = BlockSideState.None;

        currentGameState = GameState.playerSelection;

        me = "Player1";
        if(me == "Player1")
        {
            myPlayer = player1;
            enemyPlayer = player2;
        }
        else
        {
            myPlayer = player2;
            enemyPlayer = player1;
        }

        enemyPlayer.blockState = BlockSideState.Right;
    }

    private void Update()
    {
        if(currentGameState == GameState.playerSelection)
        {
            if(selection != null)
            {
                currentGameState = GameState.performSelection;
            }
        }

        if (currentGameState == GameState.performSelection)
        {
            if (selection == "PunchLeft")
            {
                playerOneAnimator.SetTrigger("Punch");
                if (enemyPlayer.blockState == BlockSideState.Right)
                {
                    playerTwoAnimator.SetTrigger("Damaged");
                    Debug.Log("Punch Left");
                    enemyPlayer.currentHealth -= 10;
                }
                else if (enemyPlayer.blockState == BlockSideState.Left)
                {
                    Debug.Log("Punch failed");
                    enemyPlayer.power += 10;
                }
                else
                {
                    Debug.Log("Selection failed");
                }
            }

            if (selection == "PunchRight")
            {
                playerOneAnimator.SetTrigger("Punch");
                if (enemyPlayer.blockState == BlockSideState.Left)
                {
                    playerTwoAnimator.SetTrigger("Damaged");
                    Debug.Log("Punch Right");
                    enemyPlayer.currentHealth -= 10;
                }
                else if (enemyPlayer.blockState == BlockSideState.Right)
                {
                    Debug.Log("Punch failed");
                    enemyPlayer.power += 10;
                }
                else
                {
                    Debug.Log("Selection failed");
                }
            }

            if (selection == "BlockLeft")
            {
                playerOneAnimator.SetTrigger("Block");
                myPlayer.blockState = BlockSideState.Left;
            }

            if (selection == "BlockRight")
            {
                playerOneAnimator.SetTrigger("Block");
                myPlayer.blockState = BlockSideState.Right;
            }

            selection = null;
            currentGameState = GameState.updateDatabase;
        }

        if (currentGameState == GameState.updateDatabase)
        {
            currentGameState = GameState.enemyTurn;
            //updatera json/databasen
        }

        if(currentGameState == GameState.enemyTurn)
        {
            Debug.Log("enemy turn");
            turn = "Player2";
            //enemy turn, my player cannot do anything
        }
    }





    public void playerTwo()
    {
        if (currentGameState == GameState.performSelection)
        {
            if (selection == "PunchLeft")
            {
                if (myPlayer.blockState == BlockSideState.Right)
                {
                    playerOneAnimator.SetTrigger("Damaged");
                    Debug.Log("Punch Left");
                    myPlayer.currentHealth -= 10;
                }
                else if (myPlayer.blockState == BlockSideState.Left)
                {
                    Debug.Log("Punch failed");
                    myPlayer.power += 10;
                }
                else
                {
                    Debug.Log("Selection failed");
                }
            }

            if (selection == "PunchRight")
            {
                if (myPlayer.blockState == BlockSideState.Left)
                {
                    playerOneAnimator.SetTrigger("Damaged");
                    Debug.Log("Punch Right");
                    myPlayer.currentHealth -= 10;
                }
                else if (myPlayer.blockState == BlockSideState.Right)
                {
                    Debug.Log("Punch failed");
                    myPlayer.power += 10;
                }
                else
                {
                    Debug.Log("Selection failed");
                }
            }

            if (selection == "BlockLeft")
            {
                playerTwoAnimator.SetTrigger("Block");
                enemyPlayer.blockState = BlockSideState.Left;
            }

            if (selection == "BlockRight")
            {
                playerTwoAnimator.SetTrigger("Block");
                enemyPlayer.blockState = BlockSideState.Right;
            }

            currentGameState = GameState.updateDatabase;
            turn = "Player1";
        }

        selection = null;
        currentGameState = GameState.playerSelection;
    }

    //GUI Buttons and bars
    public void blockLeftButton()
    {
        selection = "BlockLeft";
    }

    public void blockRightButton()
    {
        selection = "BlockRight";
    }

    public void punchLeftButton()
    {
        selection = "PunchLeft";
    }

    public void punchRightButton()
    {
        selection = "PunchRight";
    }

    public void enemyPlayerHealth()
    {
        enemyHealthBar.fillAmount = enemyPlayer.currentHealth / enemyPlayer.maxHealth;
    }

    public void myPlayerHealth()
    {
        playerHealthBar.fillAmount = myPlayer.currentHealth / myPlayer.maxHealth;
    }
}
