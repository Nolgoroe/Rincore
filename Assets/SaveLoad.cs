using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Firebase;
using Firebase.Extensions;
using Firebase.Database;
using System.Threading.Tasks;
using System;

public class SaveLoad : MonoBehaviour
{
    public int TempInt;
    public string TempString;
    public string TempString2;

    public UnityEvent onFirebaseInitialized = new UnityEvent();


    private DatabaseReference database;
    const string url = "https://rincore-a2735-default-rtdb.firebaseio.com/";
    private void Start()
    {
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

    [ContextMenu("Save")]
    private void SaveData()
    {
        database.Child("TEMP_SAVE_DATA").SetRawJsonValueAsync(JsonUtility.ToJson(this));



        database.Child("TEMP_SAVE_DATA").Child("Temp_Int 2").SetValueAsync(TempInt + 1);
        database.Child("TEMP_SAVE_DATA").Child("TempString2").SetValueAsync("Hello!");
    }

    [ContextMenu("Load")]
    public async void LoadSaveData()
    {
        //This part of the code is translating a JSON back to the class itself.
        DataSnapshot snapshot = await database.Child("TEMP_SAVE_DATA").GetValueAsync();

        if(!snapshot.Exists)
        {
            return;
        }

        JsonUtility.FromJsonOverwrite(snapshot.GetRawJsonValue(), this);

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
        database.Child("TEMP_SAVE_DATA").RemoveValueAsync();
    }
    public void CheckSuccessConnection()
    {
        Debug.Log("Success!");
    }
}
