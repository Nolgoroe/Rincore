using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalStatueData : MonoBehaviour
{
    public AnimalsInGame animal;
    public AnimalTypesInGame animalType;
    public Animator statueAnimator;

    public GameObject livePrefab;

    public void OnValidate()
    {
        if(statueAnimator == null)
        {
            TryGetComponent<Animator>(out statueAnimator);
            if(statueAnimator == null)
            {
                Debug.LogError("No animator component");
            }
        }     
    }
}
