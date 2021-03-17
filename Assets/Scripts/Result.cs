using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Firebase;
using Firebase.Auth;
using Firebase.Database;

public class Result : MonoBehaviour
{
    GameManager gameManager;
    public GameObject leopardPhoto;
    public GameObject zebraPhoto;
    DatabaseReference removeUsers;
    DatabaseReference removeGame;

    private void Start()
    {
        leopardPhoto.SetActive(false);
        zebraPhoto.SetActive(false);

        if (GameManager.leopardResultHealth <= 0)
        {
            zebraPhoto.SetActive(true);
        }
        else if(GameManager.zebraResultHealth <= 0)
        {
            leopardPhoto.SetActive(true);
        }
    }

    public void ExitResultButton()
    {

        FirebaseDatabase.DefaultInstance.RootReference.Child("users/" + GameManager.resultUserID).SetValueAsync(null);
        SceneManager.LoadScene("MainMenuScene");
    }
}
