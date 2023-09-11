using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Firebase;
using Firebase.Extensions;
using Firebase.RemoteConfig;
using UnityEngine;
using UnityEngine.Scripting;

[Preserve]
public class MyFireBaseRemoteConfig : MonoBehaviour
{
    [Preserve]
    [XmlAttribute("remote_config_defaults")]
    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    protected bool isFirebaseInitialized;
    [Preserve]
    protected virtual void Start()
    {
        FetchDataAsync();
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError(
                    "Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        }
        );
    }
    [Preserve]
    public void FetchFireBase()
    {

        FetchDataAsync();
    }
    [Preserve]
    public void ShowData()
    {
        Debug.Log("Currency: " +
                 FirebaseRemoteConfig.DefaultInstance
                     .GetValue("Currency").LongValue);
    }
    [Preserve]
    void InitializeFirebase()
    {
        //EVERYTHING SHOULD GO THROUGH THIS FUNCTION AT THE START OF THE GAME.
        //load data from database to my variables - from here I should send to a function in my save load system and load all the game's data

        // [START set_defaults]
        Dictionary<string, object> defaults =
            new Dictionary<string, object>
            {
                // These are the values that are used if we haven't fetched data from the
                // server
                // yet, or if we ask for values that the server doesn't have:
                { "config_test_string", "default local string" },
                { "config_test_int", 1 },
                { "config_test_float", 1.0 },
                { "config_test_bool", false }
            };

        FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults)
            .ContinueWithOnMainThread(task =>
            {
                // [END set_defaults]
                Debug.Log("RemoteConfig configured and ready!");
                isFirebaseInitialized = true;
                FetchFireBase();
            });

    }
    // Start a fetch request.
    // FetchAsync only fetches new data if the current data is older than the provided
    // timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
    // By default the timespan is 12 hours, and for production apps, this is a good
    // number. For this example though, it's set to a timespan of zero, so that
    // changes in the console will always show up immediately.



    [Preserve]
    [XmlRoot(ElementName = "entry")]
    public class Entry
    {
        [Preserve]
        [XmlElement(ElementName = "key")]
        public string Key { get; set; }
        [Preserve]
        [XmlElement(ElementName = "value")]
        public string Value { get; set; }
    }
    [Preserve]
    [XmlRoot(ElementName = "defaults")]
    public class Defaults
    {
        [Preserve]
        [XmlElement(ElementName = "entry")]
        public List<Entry> Entry { get; set; }
    }

    [Preserve]
    private void DefultConfig()
    {
        Defaults defaults;

        XmlSerializer serializer = new XmlSerializer(typeof(Defaults));
        using (StringReader reader = new StringReader(Resources.Load<TextAsset>("remote_config_defaults").text))
        {
            defaults = (Defaults)serializer.Deserialize(reader);
        }

        var dict = new Dictionary<string, object>();

        foreach (var entry in defaults.Entry)
        {
            if (bool.TryParse(entry.Value, out bool boolValue))
            {
                dict.Add(entry.Key, boolValue);
            }
            else if (int.TryParse(entry.Value, out int intValue))
            {
                dict.Add(entry.Key, intValue);
            }
            else if (float.TryParse(entry.Value, out float floatValue))
            {
                dict.Add(entry.Key, floatValue);
            }
            else
            {
                dict.Add(entry.Key, entry.Value);
            }
        }

        FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(dict);

    }
    [Preserve]
    public Task FetchDataAsync()
    {

        //If internet not found
        DefultConfig();

        Debug.Log("Fetching data...");
        Task fetchTask =
            FirebaseRemoteConfig.DefaultInstance.FetchAsync(
                TimeSpan.Zero);
        return fetchTask.ContinueWithOnMainThread(FetchComplete);
    }
    //[END fetch_async]
    [Preserve]
    void FetchComplete(Task fetchTask)
    {
        if (fetchTask.IsCanceled)
        {
            Debug.Log("Fetch canceled.");
        }
        else if (fetchTask.IsFaulted)
        {
            Debug.Log("Fetch encountered an error.");

        }
        else if (fetchTask.IsCompleted)
        {
            Debug.Log("Fetch completed successfully!");
        }

        var info = FirebaseRemoteConfig.DefaultInstance.Info;
        switch (info.LastFetchStatus)
        {
            case LastFetchStatus.Success:
                FirebaseRemoteConfig.DefaultInstance.ActivateAsync()
                .ContinueWithOnMainThread(task =>
                {
                    Debug.Log(String.Format("Remote data loaded and ready (last fetch time {0}).",
                                   info.FetchTime));
                });

                break;
            case LastFetchStatus.Failure:
                switch (info.LastFetchFailureReason)
                {
                    case FetchFailureReason.Error:
                        Debug.Log("Fetch failed for unknown reason");

                        break;
                    case FetchFailureReason.Throttled:
                        Debug.Log("Fetch throttled until " + info.ThrottledEndTime);

                        break;
                }
                break;
            case LastFetchStatus.Pending:
                Debug.Log("Latest Fetch call still pending.");
                break;
        }
    }

}