using System;
using System.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Extensions;
using UnityEngine;

public class MyFirebaseAnalytics : MonoBehaviour
{
    const int kMaxLogSize = 16382;
    DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
    protected bool firebaseInitialized;
    public virtual void Start()
    {
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
        });
    }
    void InitializeFirebase()
    {
        Debug.Log("Enabling data collection.");
        FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);

        Debug.Log("Set user properties.");
        // Set the user's sign up method.
        FirebaseAnalytics.SetUserProperty(
            FirebaseAnalytics.UserPropertySignUpMethod,
            "Google");
        // Set the user ID.
        FirebaseAnalytics.SetUserId("uber_user_510");
        // Set default session duration values.
        FirebaseAnalytics.SetSessionTimeoutDuration(new TimeSpan(2, 30, 0));
        firebaseInitialized = true;
    }


    void OnDestroy() { }

    public void AnalyticsLogin()
    {
        // Log an event with no parameters.
        Debug.Log("Logging a login event.");
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventLogin);
    }

    public void AnalyticsProgress()
    {
        // Log an event with a float.
        Debug.Log("Logging a progress event.");
        FirebaseAnalytics.LogEvent("progress", "percent", 0.4f);
    }

    public void AnalyticsScore()
    {
        // Log an event with an int parameter.
        Debug.Log("Logging a post-score event.");
        FirebaseAnalytics.LogEvent(
          FirebaseAnalytics.EventPostScore,
          FirebaseAnalytics.ParameterScore,
          42);
    }

    public void AnalyticsGroupJoin()
    {
        // Log an event with a string parameter.
        Debug.Log("Logging a group join event.");
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventJoinGroup, FirebaseAnalytics.ParameterGroupId,
          "spoon_welders");
    }

    public void AnalyticsLevelUp()
    {
        // Log an event with multiple parameters.
        Debug.Log("Logging a level up event.");
        FirebaseAnalytics.LogEvent(
          FirebaseAnalytics.EventLevelUp,
          new Parameter(FirebaseAnalytics.ParameterLevel, 5),
          new Parameter(FirebaseAnalytics.ParameterCharacter, "mrspoon"),
          new Parameter("hit_accuracy", 3.14f));
    }

    // Reset analytics data for this app instance.
    public void ResetAnalyticsData()
    {
        Debug.Log("Reset analytics data.");
        FirebaseAnalytics.ResetAnalyticsData();
    }

    // Get the current app instance ID.
    public Task<string> DisplayAnalyticsInstanceId()
    {
        return FirebaseAnalytics.GetAnalyticsInstanceIdAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.Log("App instance ID fetch was canceled.");
            }
            else if (task.IsFaulted)
            {
                Debug.Log(String.Format("Encounted an error fetching app instance ID {0}",
                                        task.Exception));
            }
            else if (task.IsCompleted)
            {
                Debug.Log(String.Format("App instance ID: {0}", task.Result));
            }
            return task;
        }).Unwrap();
    }
}
