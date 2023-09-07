using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;
using System;
using System.IO;
using System.Threading.Tasks;

public class SaveLoad : MonoBehaviour
{
    public string UID_TEXT;
    public int delayLoading;

    const string TEST_SAVE = "TEMP_SAVE_DATA";
    //const string PLAYER_SAVE = "Player_Data";

    //public static SaveLoad instance;
    public UnityEvent onFirebaseInitialized = new UnityEvent();
    private DatabaseReference database;


    string path;

    const string url = "https://rincore-a2735-default-rtdb.firebaseio.com/";


    [Header("General refrences")]
    public Player playerRef;
    public MapLogic mapLogic;
    public SavedData saveData;

    private void Awake()
    {
        path = Application.persistentDataPath + "/UniqueIDUser.txt"; //TEMP
        //instance = this;
    }

    private void Start()
    {
        if(File.Exists(path))
        {
            UID_TEXT = File.ReadAllText(path);
        }
        else
        {
            UID_TEXT = uniqueID();

            File.WriteAllText(Application.persistentDataPath + "/UniqueIDUser.txt", UID_TEXT);
        }

        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failes to initialize Firebase with {task.Exception}");
                return;
            }
            onFirebaseInitialized?.Invoke();

        });

        database = FirebaseDatabase.DefaultInstance.RootReference; //This is a DatabaseReference type object
    }

    [ContextMenu("Delete local user")]
    private void DeleteUserLocally()
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
    private void SaveData()
    {
        database.Child(UID_TEXT).Child(TEST_SAVE).SetRawJsonValueAsync(JsonUtility.ToJson(saveData));
        //database.Child(UID_TEXT).Child(PLAYER_SAVE).SetRawJsonValueAsync(JsonUtility.ToJson(playerRef));



        //database.Child("TEMP_SAVE_DATA").Child("Temp_Int 2").SetValueAsync(TempInt + 1);
        //database.Child("TEMP_SAVE_DATA").Child("TempString2").SetValueAsync("Hello!");
    }

    [ContextMenu("Save")]
    private void SaveAction()
    {
        //indexReachedInCluster = GameManager.instance.ReturnCurrentIndexInCluster();
        saveData.currentClusterIDReached = GameManager.instance.currentCluster.clusterID;
        saveData.savedCoins = playerRef.GetOwnedCoins;

        SaveData();
    }

    [ContextMenu("Load")]
    public async void LoadSaveData()
    {
        if (UID_TEXT != "")
        {
            //load this class
            await FirebaseDatabase.DefaultInstance
                .GetReference(UID_TEXT).Child(TEST_SAVE)
                .GetValueAsync().ContinueWithOnMainThread(task => {
                    if (task.IsFaulted)
                    {
                        // Handle the error...
                    }
                    else if (task.IsCompleted)
                    {
                        DataSnapshot snapshot = task.Result;

                        if (snapshot.Exists)
                        {
                            JsonUtility.FromJsonOverwrite(snapshot.GetRawJsonValue(), SavedData.instance);
                        }

                        if (saveData.currentClusterIDReached == 2)
                        {

                        }

                        // Do something with snapshot...
                    }
                });


            ////load the player data
            //await FirebaseDatabase.DefaultInstance
            //    .GetReference(UID_TEXT).Child(TEST_SAVE)
            //    .GetValueAsync().ContinueWithOnMainThread(task => {
            //        if (task.IsFaulted)
            //        {
            //            // Handle the error...
            //        }
            //        else if (task.IsCompleted)
            //        {
            //            DataSnapshot snapshot = task.Result;

            //            if (snapshot.Exists)
            //            {
            //                JsonUtility.FromJsonOverwrite(snapshot.GetRawJsonValue(),  playerRef);
            //            }
            //            // Do something with snapshot...
            //        }
            //    });

            //This part of the code is translating a JSON back to the class itself.
            //DataSnapshot snapshot = await database.Child(UID_TEXT).Child(TEST_SAVE).GetValueAsync();
            //DataSnapshot snapshot2 = null;

            //await Task.Delay(1000);

            // load data only if has data! - for now, not good!
            await Task.Delay(delayLoading);

            GameManager.instance.OnLoadData();
            mapLogic.OnLoadData();
            playerRef.OnLoadData();


            await Task.Delay(1000);

            UIManager.instance.OnLoadData(); // has to be lase to close the loading screen
        }












        //FROM HERE on the code is aimed at getting specific values from the database instead of a complete JSON.

        //FirebaseDatabase.DefaultInstance.GetReference("TEMP_SAVE_DATA").GetValueAsync().ContinueWithOnMainThread(task => {
        //    if (task.IsFaulted)
        //    {
        //        Debug.LogError($"Failes to initialize Firebase with {task.Exception}");
        //        return;
        //    }
        //    else if (task.IsCompleted)
        //    {
        //        DataSnapshot snapshot = task.Result;

        //        TempInt = Convert.ToInt32(snapshot.Child("Temp_Int").GetValue(true).ToString());
        //        TempString = snapshot.Child("TempString").GetValue(true).ToString();

        //        // Do something with snapshot...
        //    }
        //});
    }

    [ContextMenu("Erase")]
    public void EraseSave()
    {
        database.Child(UID_TEXT).Child(TEST_SAVE).RemoveValueAsync();
        //database.Child(UID_TEXT).Child(PLAYER_SAVE).RemoveValueAsync();
    }
    public void CheckSuccessConnection()
    {
        LoadSaveData();

        Debug.Log("Success!");
    }

    private string uniqueID()
    {
        DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        int currentEpochTime = (int)(DateTime.UtcNow - epochStart).TotalSeconds;
        int z1 = UnityEngine.Random.Range(0, 1000000);
        int z2 = UnityEngine.Random.Range(0, 1000000);
        string uid = currentEpochTime + ":" + z1 + ":" + z2;
        return uid;
    }
}
