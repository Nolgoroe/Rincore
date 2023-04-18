using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public enum AnimalsInGame
{
    RedFox,
    YellowFox,
    WhiteFox,
    OrangeFox,
    BrownStag,
    PinkStag,
    OrangeStag,
    YellowStag,
    BrownOwl,
    YellowOwl,
    GreyOwl,
    WhiteOwl,
    OrangeBoar,
    DarkBoar,
    BrownBoar,
    WhiteBoar,
    BrownCharmander,
    YellowCharmander,
    BlueCharmander,
    PinkCharmander,
    None
}

[Serializable]
public enum AnimalTypesInGame
{
    Fox,
    Stag,
    Owl,
    Boar,
    Charmander,
    None
}

[Serializable]
public class OwnedAnimalDataSet
{
    public AnimalTypesInGame animalTypeEnum;
    public AnimalsInGame animalEnum;

    public OwnedAnimalDataSet(AnimalTypesInGame _animalType, AnimalsInGame _animal)
    {
        animalTypeEnum = _animalType;
        animalEnum = _animal;
    }
}

public class AnimalsManager : MonoBehaviour
{
    [Header("General data")]
    [SerializeField] private List<OwnedAnimalDataSet> unlockedAnimals;

    [Header("Album data")]
    [SerializeField] private List<AnimalsInGame> animalsRevealedInAlbum;
    [SerializeField] private List<AnimalTypesInGame> albumPagesCompleted;

    [Header("Album rewards")]
    [SerializeField] private int minRubyRewardClearPage;
    [SerializeField] private int maxRubyRewardClearPage;


    public void ReleaseAnimal(AnimalStatueData statueData, Transform parent)
    {
        OwnedAnimalDataSet owned = unlockedAnimals.Where(p => p.animalEnum == statueData.animal).SingleOrDefault();

        if(owned == null)
        {
            OwnedAnimalDataSet newOwned = new OwnedAnimalDataSet(statueData.animalType, statueData.animal);
            unlockedAnimals.Add(newOwned); 
        }

        GameObject animalSpawned = Instantiate(statueData.livePrefab, parent);
        Destroy(statueData.gameObject);
    }

    public void AddAnimalToAlbum(AnimalsInGame animal)
    {
        animalsRevealedInAlbum.Add(animal);
    }
    public void AddToAlbumPagesCompleted(AnimalTypesInGame animalType)
    {
        albumPagesCompleted.Add(animalType);
    }

    public int RollAmountOfReward()
    {
        int amount = UnityEngine.Random.Range(minRubyRewardClearPage, maxRubyRewardClearPage + 1);
        return amount;
    }

    public List<OwnedAnimalDataSet> GetUnlockedAnimals()
    {
        return unlockedAnimals;
    }
    public bool CheckAnimalAlreadyInAlbum(AnimalsInGame animal)
    {
        return animalsRevealedInAlbum.Contains(animal);
    }
    public bool CheckPageAlreadyClaimedInAlbum(AnimalTypesInGame animalType)
    {
        return albumPagesCompleted.Contains(animalType);
    }
}
