using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ActiveUser : MonoBehaviour
{
    //Singleton
    private static ActiveUser instance;
    public static ActiveUser Instance { get { return instance; } }

    //This scripts gives easy access to our userInfoData and our ID
    public UserInfo userInfo;
    public string userID;

    private void Awake()
    {
        //Singleton stuff
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }

    public void LoadUserData()
    {
        userID = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        StartCoroutine(FirebaseManager.Instance.LoadData("users/" + userID, LoadUser));
    }

    //Makes sure that we have a userInfo object that we can access.
    public void LoadUser(string jsonData)
    {
        /*
        if (jsonData == null || jsonData == "")
        {
            //No user, create a new empty one.
            userInfo = new UserInfo();
            SaveUser();
        }
        else
        {
            userInfo = JsonUtility.FromJson<UserInfo>(jsonData);
        }
        */
        //Go to the next scene, lobby
        SceneManager.LoadScene("GameScene");
    }

    public void SaveUser()
    {
        string jsonData = JsonUtility.ToJson(userInfo);
        StartCoroutine(FirebaseManager.Instance.SaveData("users/" + userID, jsonData));
    }
}
