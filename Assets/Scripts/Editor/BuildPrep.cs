using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class BuildPrep : MonoBehaviour, IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        CustomWindowParent[] allGameWindows = FindObjectsOfType<CustomWindowParent>();
        foreach (BasicUIElement window in allGameWindows)
        {
            window.gameObject.SetActive(false);
        }

        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
    }
}
