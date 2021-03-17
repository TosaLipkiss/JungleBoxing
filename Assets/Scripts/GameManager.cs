using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[Serializable]
public class PlayerInfo
{
    public string displayName;
    public string userID;
    public int currentPower;
    public int currentHealth;
    public int maxHealth;
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
    float playerIdleAnimationInterval;
    float enemyIdleAnimationInterval;
    float playerIdleTimer = 0f;
    float enemyIdleTimer = 0f;
    int playerIdleAnimation;
    int enemyIdleAnimation;
    float punchSoundTimer = 0f;
    bool punchTimerActive = false;

    public static string resultUserID;

    public static int leopardResultHealth;
    public static int zebraResultHealth;

    public Text state;
    public Text status;
    UserInfo user;
    public string userID;
    FirebaseManager fbManager;

    public Image playerLeopardHealthBar;
    public Image playerZebraHealthBar;

    public Animator leopard;
    public Animator zebra;

    public GameObject playerLeopardPower1;
    public GameObject playerLeopardPower2;
    public GameObject playerLeopardPower3;

    public GameObject playerZebraPower1;
    public GameObject playerZebraPower2;
    public GameObject playerZebraPower3;

    public GameObject playerLeopardTurn;
    public GameObject playerZebraTurn;

    // string turn;
    // string me;
    string selection = null;

    [NonSerialized]
    public PlayerInfo enemyPlayer = null;
    [NonSerialized]
    public PlayerInfo myPlayer = null;

    Animator myCharacter;
    Animator enemyCharacter;

    Image myPlayerHealthbar;
    Image enemyHealthbar;

    GameObject myPlayerPower1;
    GameObject myPlayerPower2;
    GameObject myPlayerPower3;

    GameObject enemyPlayerPower1;
    GameObject enemyPlayerPower2;
    GameObject enemyPlayerPower3;

    GameObject myPlayerTurn;
    GameObject enemyPlayerTurn;

    public AudioSource audioSource;
    public AudioClip punch;
    public AudioClip block;

    public AudioSource mjauSource;
    public AudioClip mjau;

    GameState currentGameState;

    enum GameState
    {
        waitingForMatchStart,
        connectUserData,
        enemyTurn,
        playerSelection,
        performSelection,
        updateDatabase,
        gameResult
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

        leopard.SetTrigger("Idle");
        zebra.SetTrigger("Idle");

        currentGameState = GameState.waitingForMatchStart;
    }

    private void Update()
    {
        MyPlayerPower();
        MyPlayerHealth();
        EnemyPlayerPower();
        EnemyPlayerHealth();

        enemyIdleTimer += Time.deltaTime;
        playerIdleTimer += Time.deltaTime;

        state.text = currentGameState.ToString();
        timer += Time.deltaTime;

        if(punchTimerActive == true)
        {
            punchSoundTimer += Time.deltaTime;
            if(punchSoundTimer >= 0.2f)
            {
                audioSource.PlayOneShot(punch);
                punchTimerActive = false;
                punchSoundTimer = 0f;
            }
        }

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
                myCharacter = leopard;
                myPlayerHealthbar = playerLeopardHealthBar;
                myPlayerPower1 = playerLeopardPower1;
                myPlayerPower2 = playerLeopardPower2;
                myPlayerPower3 = playerLeopardPower3;
                myPlayerTurn = playerLeopardTurn;

                enemyPlayer = currentGameInfo.player2;
                enemyCharacter = zebra;
                enemyHealthbar = playerZebraHealthBar;
                enemyPlayerPower1 = playerZebraPower1;
                enemyPlayerPower2 = playerZebraPower2;
                enemyPlayerPower3 = playerZebraPower3;
                enemyPlayerTurn = playerZebraTurn;
            }
            else
            {
                myPlayer = currentGameInfo.player2;
                myCharacter = zebra;
                myPlayerHealthbar = playerZebraHealthBar;
                myPlayerPower1 = playerZebraPower1;
                myPlayerPower2 = playerZebraPower2;
                myPlayerPower3 = playerZebraPower3;
                myPlayerTurn = playerZebraTurn;

                enemyPlayer = currentGameInfo.player1;
                enemyCharacter = leopard;
                enemyHealthbar = playerLeopardHealthBar;
                enemyPlayerPower1 = playerLeopardPower1;
                enemyPlayerPower2 = playerLeopardPower2;
                enemyPlayerPower3 = playerLeopardPower3;
                enemyPlayerTurn = playerLeopardTurn;
            }

            string whosTurn = currentGameInfo.turn;

            if (whosTurn == "Player1")
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

            if (whosTurn == "Player2")
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
            IdleAnimationSwitch();

            enemyPlayerTurn.SetActive(false);
            myPlayerTurn.SetActive(true);

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
                mjauSource.PlayOneShot(mjau);
                myCharacter.SetTrigger("Punch");
                if (enemyPlayer.blockState == BlockSideState.Right || enemyPlayer.blockState == BlockSideState.None)
                {
                    punchTimerActive = true;
                //    audioSource.PlayOneShot(punch);
                    enemyCharacter.SetTrigger("Damaged");
                    Punch();
                }
                else if (enemyPlayer.blockState == BlockSideState.Left)
                {
                    audioSource.PlayOneShot(block);
                    enemyCharacter.SetTrigger("Block");
                    enemyPlayer.currentPower++;
                }
                else
                {
                    Debug.Log("Selection failed");
                }

                myPlayer.currentPower = 0;
                myPlayer.blockState = BlockSideState.None;
                enemyPlayer.blockState = BlockSideState.None;
            }

            if (selection == "PunchRight")
            {
                mjauSource.PlayOneShot(mjau);
                myCharacter.SetTrigger("Punch");
                if (enemyPlayer.blockState == BlockSideState.Left || enemyPlayer.blockState == BlockSideState.None)
                {
                    punchTimerActive = true;
                    //    audioSource.PlayOneShot(punch);
                    enemyCharacter.SetTrigger("Damaged");
                    Punch();
                }
                else if (enemyPlayer.blockState == BlockSideState.Right)
                {
                    audioSource.PlayOneShot(block);
                    enemyCharacter.SetTrigger("Block");
                    enemyPlayer.currentPower++;
                }
                else
                {
                    Debug.Log("Selection failed");
                }

                myPlayer.currentPower = 0;
                myPlayer.blockState = BlockSideState.None;
                enemyPlayer.blockState = BlockSideState.None;
            }

            if (selection == "BlockLeft")
            {
                audioSource.PlayOneShot(block);
                myCharacter.SetTrigger("Block");
                myPlayer.blockState = BlockSideState.Left;
            }

            if (selection == "BlockRight")
            {
                audioSource.PlayOneShot(block);
                myCharacter.SetTrigger("Block");
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

            if (myPlayer.currentHealth <= 0f || enemyPlayer.currentHealth <= 0f)
            {
                currentGameState = GameState.gameResult;
            }
        }

        if(currentGameState == GameState.enemyTurn)
        {
            IdleAnimationSwitch();
            enemyPlayerTurn.SetActive(true);
            myPlayerTurn.SetActive(false);
            string whosTurn = currentGameInfo.turn;
            string player1UserId = currentGameInfo.player1.userID;
            string player2UserId = currentGameInfo.player2.userID;

            if (whosTurn == "Player1")
            {
                if (player1UserId == userID)
                {
                    currentGameState = GameState.playerSelection;
                }
            }

            if (whosTurn == "Player2")
            {
                if (player2UserId == userID)
                {
                    currentGameState = GameState.playerSelection;
                }
            }
        }

        if(currentGameState == GameState.gameResult)
        {
            if (myCharacter == leopard)
            {
                leopardResultHealth = myPlayer.currentHealth;
                zebraResultHealth = enemyPlayer.currentHealth;
            }
            else
            {
                leopardResultHealth = enemyPlayer.currentHealth;
                zebraResultHealth = myPlayer.currentHealth;
            }

            resultUserID = myPlayer.userID;
            SceneManager.LoadScene("ResultScene");
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
            newGame.player1.currentPower = 0;
            newGame.player1.blockState = BlockSideState.None;
            newGame.player1.userID = userID;

            newGame.player2 = new PlayerInfo();
            newGame.player2.currentHealth = 100;
            newGame.player2.maxHealth = 100;
            newGame.player2.currentPower = 0;
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

    public void EnemyPlayerHealth()
    {
        if(enemyPlayer != null)
        {
            enemyHealthbar.fillAmount = (float)enemyPlayer.currentHealth / (float)enemyPlayer.maxHealth;
        }
    }

    public void MyPlayerHealth()
    {
        if(myPlayer != null)
        {
            myPlayerHealthbar.fillAmount = (float)myPlayer.currentHealth / (float)myPlayer.maxHealth;
        }
    }
    public void MyPlayerPower()
    {
        if(myPlayer != null)
        {
            if (myPlayer.currentPower == 1)
            {
                myPlayerPower1.SetActive(true);
            }
            else if (myPlayer.currentPower == 2)
            {
                myPlayerPower1.SetActive(true);
                myPlayerPower2.SetActive(true);
            }
            else if (myPlayer.currentPower == 3)
            {
                myPlayerPower1.SetActive(true);
                myPlayerPower2.SetActive(true);
                myPlayerPower3.SetActive(true);
            }
            else if (myPlayer.currentPower == 0)
            {
                myPlayerPower1.SetActive(false);
                myPlayerPower2.SetActive(false);
                myPlayerPower3.SetActive(false);
            }
            else
            {
                Debug.Log(myPlayer.currentPower);
            }
        }
    }
    
    public void EnemyPlayerPower()
    {
        if(enemyPlayer != null)
        {
            if (enemyPlayer.currentPower == 1)
            {
                enemyPlayerPower1.SetActive(true);
            }
            else if (enemyPlayer.currentPower == 2)
            {
                enemyPlayerPower1.SetActive(true);
                enemyPlayerPower2.SetActive(true);
            }
            else if (enemyPlayer.currentPower >= 3)
            {
                enemyPlayerPower1.SetActive(true);
                enemyPlayerPower2.SetActive(true);
                enemyPlayerPower3.SetActive(true);
            }
            else if (enemyPlayer.currentPower == 0)
            {
                enemyPlayerPower1.SetActive(false);
                enemyPlayerPower2.SetActive(false);
                enemyPlayerPower3.SetActive(false);
            }
            else
            {
                Debug.Log(myPlayer.currentPower);
            }
        }
    }

    public void Punch()
    {
        if (myPlayer != null)
        {
            if (myPlayer.currentPower == 1)
            {
                enemyPlayer.currentHealth -= 15;
            }
            else if (myPlayer.currentPower == 2)
            {
                enemyPlayer.currentHealth -= 25;
            }
            else if (myPlayer.currentPower == 3)
            {
                enemyPlayer.currentHealth -= 50;
            }
            else if (myPlayer.currentPower == 0)
            {
                enemyPlayer.currentHealth -= 10;
            }
            else
            {
                Debug.Log("Punch failed or blocked");
            }
        }
    }

    public void IdleAnimationSwitch()
    {
        playerIdleAnimationInterval = UnityEngine.Random.Range(3f, 5f);
        enemyIdleAnimationInterval = UnityEngine.Random.Range(3f, 5f);

        playerIdleAnimation = UnityEngine.Random.Range(0, 3);
        enemyIdleAnimation = UnityEngine.Random.Range(0, 3);

        if (playerIdleTimer >= playerIdleAnimationInterval)
        {
            if (playerIdleAnimation == 0)
            {
                myCharacter.SetTrigger("SideStep");
            }
            else if (playerIdleAnimation == 1)
            {
                myCharacter.SetTrigger("ShortStep");
            }
            else
            {
                Debug.Log("Normal IDLE");
            }
            playerIdleTimer = 0f;
        }

        if (enemyIdleTimer >= enemyIdleAnimationInterval)
        {
            if (enemyIdleAnimation == 0)
            {
                enemyCharacter.SetTrigger("SideStep");
            }
            else if (enemyIdleAnimation == 1)
            {
                enemyCharacter.SetTrigger("ShortStep");
            }
            else
            {
                Debug.Log("Normal IDLE");
            }
            enemyIdleTimer = 0f;
        }
    }
}
