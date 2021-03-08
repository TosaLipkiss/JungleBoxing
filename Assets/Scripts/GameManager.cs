using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
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
    public string status;
    public string displayGameName;
    public string gameID;
    public string turn;
    public PlayerInfo player1;
    public PlayerInfo player2;
}

public enum BlockSideState : int
{
    None = 0,
    Left = 1,
    Right = 2
}

public class GameManager : MonoBehaviour
{
    GameInfo currentGameInfo = null;

    float downloadInterval = 10.0f;
    float timer = 0f;

    public Text status;
    UserInfo user;
    string userID;
    FirebaseManager fbManager;

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
        waitingForMatchStart,
        connectUserData,
        enemyTurn,
        playerSelection,
        performSelection,
        updateDatabase
    }

    private void Start()
    {
        //Ref for our userID
        userID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        //Create ref for our firebase Instance
        fbManager = FirebaseManager.Instance;

        //Tell the user what's happening
        Log("Loading data for: " + userID);

        //Load userInfo
        StartCoroutine(fbManager.LoadData("users/" + userID, LoadedUser));

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
        timer += Time.deltaTime;

        if(timer >= downloadInterval)
        {
            DownloadGameInfo();
            timer = 0f;
        }

        //When game is filled max player
        if (currentGameState == GameState.waitingForMatchStart)
        {
            //Debug.Log("waiting");
            if (currentGameInfo != null)
            {
                if (currentGameInfo.status == "full")
                {
                    Debug.Log("we should not longer wait, the game has been filled");
                    currentGameState = GameState.connectUserData;
                }
            }
        }

        //Decide which "player" is whos
        if (currentGameState == GameState.connectUserData)
        {
            string player1UserId = currentGameInfo.player1.userID;
            string player2UserId = currentGameInfo.player2.userID;

            if (userID == player1UserId)
            {
                myPlayer = currentGameInfo.player1;
                enemyPlayer = currentGameInfo.player2;
            }
            else
            {
                myPlayer = currentGameInfo.player2;
                enemyPlayer = currentGameInfo.player1;
            }

            string whos_turn = currentGameInfo.turn;

            if (whos_turn == "Player1")
            {
                if (player1UserId == userID)
                {
                    currentGameState = GameState.playerSelection;
                }
                else
                {
                    currentGameState = GameState.enemyTurn;
                }
            }

            if (whos_turn == "Player2")
            {
                if (player2UserId == userID)
                {
                    currentGameState = GameState.playerSelection;
                }
                else
                {
                    currentGameState = GameState.enemyTurn;
                }
            }

        }

        if (currentGameState == GameState.playerSelection)
        {
            if (selection != null)
            {
                currentGameState = GameState.performSelection;
            }
        }


        //Fight moves
        if (currentGameState == GameState.performSelection)
        {
            if (selection == "PunchLeft")
            {
                playerOneAnimator.SetTrigger("Punch");
                if (enemyPlayer.blockState == BlockSideState.Right || enemyPlayer.blockState == BlockSideState.None)
                {
                    playerTwoAnimator.SetTrigger("Damaged");

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
                if (enemyPlayer.blockState == BlockSideState.Left || enemyPlayer.blockState == BlockSideState.None)
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
            // upload GameInfo to firebase
            if (currentGameInfo.turn == "Player1")
            {
                currentGameInfo.turn = "Player2";
            }
            else if (currentGameInfo.turn == "Player2")
            {
                currentGameInfo.turn = "Player1";
            }

            StartCoroutine(fbManager.SaveData("games/" + currentGameInfo.gameID, JsonUtility.ToJson(currentGameInfo)));

            currentGameState = GameState.enemyTurn;
        }

        if(currentGameState == GameState.enemyTurn)
        {
            Debug.Log("enemy turn");
            //turn = "Player2";
            //enemy turn, my player cannot do anything

            string whos_turn = currentGameInfo.turn;
            string player1_user_id = currentGameInfo.player1.userID;
            string player2_user_id = currentGameInfo.player2.userID;

            if (whos_turn == "Player1")
            {
                if (player1_user_id == userID)
                {
                    currentGameState = GameState.playerSelection;
                }
            }

            if (whos_turn == "Player2")
            {
                if (player2_user_id == userID)
                {
                    currentGameState = GameState.playerSelection;
                }
            }
        }
    }

    /// //////////FIREBASE////////////
    private void Log(string message)
    {
        status.text = message;
        Debug.Log(message);
    }

    //process the user data
    private void LoadedUser(string jsonData)
    {
        Log("Processing user data: " + userID);

        //If we cant find any user data we need to create it
        if (jsonData == null || jsonData == "")
        {
            Log("No user data found, creating new user data");

            user = new UserInfo();
            user.activeGame = "";
            StartCoroutine(fbManager.SaveData("users/" + userID, JsonUtility.ToJson(user)));
        }
        else
        {
            //We found user data
            user = JsonUtility.FromJson<UserInfo>(jsonData);
        }

        //We now have a user, lets check if our user have an active game.
        CheckedActiveGame();
    }

    private void CheckedActiveGame()
    {
        //Does our user doesn't have an active game?
        if (user.activeGame == "" || user.activeGame == null)
        {
            //Start the new game process
            Log("No active game for the user, look for a game");
            StartCoroutine(fbManager.CheckForGame("games/", NewGameLoaded));
        }
        else
        {
            //We already have a game, load it
            Log("Loading Game: " + user.activeGame);
            DownloadGameInfo();
            //StartCoroutine(fbManager.LoadData("games/" + user.activeGame, GameLoaded));
        }
    }

    private void NewGameLoaded(string jsonData)
    {
        //We couldn't find a new game to join
        if (jsonData == "" || jsonData == null || jsonData == "{}")
        {
            //Create a unique ID for the new game
            string key = FirebaseDatabase.DefaultInstance.RootReference.Child("games/").Push().Key;
            string path = "games/" + key;

            //Create game structure
            var newGame = new GameInfo();
         //   newGame.player1 = userID;
            newGame.status = "new";
            newGame.gameID = key;

            newGame.turn = "Player1";

            newGame.player1 = new PlayerInfo();
            newGame.player1.currentHealth = 100;
            newGame.player1.maxHealth = 100;
            newGame.player1.power = 0;
            newGame.player1.blockState = BlockSideState.None;
            newGame.player1.userID = userID;

            newGame.player2 = new PlayerInfo();
            newGame.player2.currentHealth = 100;
            newGame.player2.maxHealth = 100;
            newGame.player2.power = 0;
            newGame.player2.blockState = BlockSideState.None;
            newGame.player2.userID = "";

            //Save our new game
            StartCoroutine(fbManager.SaveData(path, JsonUtility.ToJson(newGame)));

            Log("Creating new game: " + key);

            //add the key to our active games
            user.activeGame = key;
            StartCoroutine(fbManager.SaveData("users/" + userID, JsonUtility.ToJson(user)));

            GameLoaded(newGame);
        }
        else
        {
            //We found a game, lets join it
            var game = JsonUtility.FromJson<GameInfo>(jsonData);

            //Update the game
            game.player2.userID = userID;

            game.status = "full";
            StartCoroutine(fbManager.SaveData("games/" + game.gameID, JsonUtility.ToJson(game)));

            //Update the user
            user.activeGame = game.gameID;
            StartCoroutine(fbManager.SaveData("users/" + userID, JsonUtility.ToJson(user)));

            GameLoaded(game);
        }
    }

    private void GameLoaded(string jsonData)
    {
        //Debug.Log(jsonData);
        if (jsonData == null || jsonData == "")
        {
           // Log("no game data");
           // Debug.LogError("Error while loading game data");
            Log("No game, creating new game...");
            //lösningen på problemet
            StartCoroutine(fbManager.CheckForGame("games/", NewGameLoaded));
        }
        else
        {
            GameLoaded(JsonUtility.FromJson<GameInfo>(jsonData));
            currentGameInfo = JsonUtility.FromJson<GameInfo>(jsonData);
            Debug.Log("Data download complete");
        }
    }

    private void GameLoaded(GameInfo game)
    {
        Log("Game has been loaded");
        GetComponent<GameStatus>().StartGame(game);
    }

    private void DownloadGameInfo()
    {
        Debug.Log("Starting downloading new data...");
        StartCoroutine(fbManager.LoadData("games/" + user.activeGame, GameLoaded));
    }


    public void PlayerTwo()
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
    public void BlockLeftButton()
    {
        selection = "BlockLeft";
    }

    public void BlockRightButton()
    {
        selection = "BlockRight";
    }

    public void PunchLeftButton()
    {
        selection = "PunchLeft";
    }

    public void PunchRightButton()
    {
        selection = "PunchRight";
    }
/*
    public void EnemyPlayerHealth()
    {
        enemyHealthBar.fillAmount = enemyPlayer.currentHealth / enemyPlayer.maxHealth;
    }

    public void MyPlayerHealth()
    {
        playerHealthBar.fillAmount = myPlayer.currentHealth / myPlayer.maxHealth;
    }
    */
}
