using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;
using Firebase.Auth;

public class SaveUser : MonoBehaviour
{
    public InputField userName;

    public void SaveUserNameButton()
    {
        FirebaseDatabase db = FirebaseDatabase.DefaultInstance;
        db.RootReference.Child("users").Child(userName.text).SetRawJsonValueAsync("{}");
        //  string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        // Debug.Log(userId);
        Debug.Log("Save name");
    }

}
