using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTwoHealth : MonoBehaviour
{
    public GameManager gameManager;

    void Update()
    {
        gameManager.EnemyPlayerHealth();
    }
}
