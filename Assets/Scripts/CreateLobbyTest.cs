using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateLobbyTest : MonoBehaviour
{
    string data = @"{""test_key"":""test_value2""}";
    public void CreateLobbyButton()
    {
        Debug.Log("create lobby...");
        StartCoroutine(FirebaseManager.Instance.SaveData("lobbies/" + "0001", data));
    }
}
