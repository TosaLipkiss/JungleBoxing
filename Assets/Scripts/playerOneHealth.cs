using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerOneHealth : MonoBehaviour
{
    public GameManager gameManager;

    void Update()
    {
        gameManager.myPlayerHealth();
    }
}
