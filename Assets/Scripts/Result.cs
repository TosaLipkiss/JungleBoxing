using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Result : MonoBehaviour
{
    GameManager gameManager;
    public GameObject leopardPhoto;
    public GameObject zebraPhoto;

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
        SceneManager.LoadScene("MainMenuScene");
    }
}
