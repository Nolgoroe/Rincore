
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;
using System;
using System.IO;
using System.Threading.Tasks;

public class SaveLoad : MonoBehaviour
{
    public static SaveLoad instance;

    [SerializeField] private string UID_TEXT;
    [SerializeField] private int delayLoading;

    const string TEST_SAVE = "TEMP_SAVE_DATA";

    //public static SaveLoad instance;
    public UnityEvent onFirebaseInitialized = new UnityEvent();
    private DatabaseReference database;


    string path;

    const string url = "https://rincore-a2735-default-rtdb.firebaseio.com/";


    [Header("General refrences")]
    [SerializeField] private Player playerRef;
    [SerializeField] private PowerupManager powerManager;
    [SerializeField] private MapLogic mapLogic;
    [SerializeField] private SavedData saveData;
    [SerializeField] private bool allowSaveGame;

    private void Awake()
    {
        instance = this;
        path = Application.persistentDataPath + "/UniqueIDUser.txt"; //check if can shorten
    }



    private void Start()
    {
        FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
        StartCoroutine(InitSaveLoad());
    }

    private IEnumerator InitSaveLoad()
    {
        yield return new WaitForSeconds(2);
        if (File.Exists(path))
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
                Debug.LogError($"Failed to initialize Firebase with {task.Exception}");
                return;
            }

            database = FirebaseDatabase.DefaultInstance.RootReference; //This is a DatabaseReference type object

            onFirebaseInitialized?.Invoke();
        });
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
        if (!allowSaveGame) return;

        database.Child(UID_TEXT).Child(TEST_SAVE).SetRawJsonValueAsync(JsonUtility.ToJson(saveData));


        //This is how we access specific variables - DO NOT DELETE!
        //database.Child("TEMP_SAVE_DATA").Child("Temp_Int 2").SetValueAsync(TempInt + 1);
        //database.Child("TEMP_SAVE_DATA").Child("TempString2").SetValueAsync("Hello!");
    }

    [ContextMenu("Save")]
    public void SaveAction()
    {
        saveData.currentClusterIDReached = GameManager.instance.currentCluster.clusterID - 1;
        saveData.savedCoins = playerRef.GetOwnedCoins;
        saveData.savedBombCount = powerManager.ReturnAmountOfPower(PowerupType.Bomb);
        saveData.savedJokerCount = powerManager.ReturnAmountOfPower(PowerupType.Joker);
        saveData.savedRefreshCount = powerManager.ReturnAmountOfPower(PowerupType.RefreshTiles);
        saveData.savedSwitchCount = powerManager.ReturnAmountOfPower(PowerupType.Switch);
        saveData.savedUndoCount = powerManager.ReturnAmountOfPower(PowerupType.Undo);

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

            //This part of the code is translating a JSON back to the class itself - DO NOT DELETE!
            //DataSnapshot snapshot = await database.Child(UID_TEXT).Child(TEST_SAVE).GetValueAsync();
            //JsonUtility.FromJsonOverwrite(snapshot.GetRawJsonValue(),CLASS INSTANCE HERE);

            // load data only if has data! - for now, not good!
            await Task.Delay(delayLoading);

            GameManager.instance.OnLoadData();
            mapLogic.OnLoadData();
            playerRef.OnLoadData();
            powerManager.InitPowerUpManager();

            await Task.Delay(1000);

            UIManager.instance.OnLoadData(); // has to be lase to close the loading screen
        }












        //FROM HERE on the code is aimed at getting specific values from the database instead of a complete JSON - DO NOT DELETE!

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

    [ContextMenu("Temp Reload Scene")]
    public void ReloadScene()
    {
        SceneManager.LoadScene(0);
    }
}
