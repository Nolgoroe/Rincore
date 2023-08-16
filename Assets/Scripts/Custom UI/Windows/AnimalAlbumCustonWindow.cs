using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

[Serializable]
public class AnimalPagesByType
{
    public AnimalTypesInGame animalType;
    public List<SpecificDisplayerAnimalAlbum> animalsInPage;
}

public class AnimalAlbumCustonWindow : BasicCustomUIWindow
{
    private AnimalsManager localAnimalManager; //we could use macros here to get all getters - problem is the actions we shoot
    private Player localPlayer;//we could use macros here to get all getters - problem is the actions we shoot
    private AnimalTypesInGame currentOpenType;

    [Header("Shaders and materials")]
    [SerializeField] private Shader animalFadeShader;

    [Header("Auto filled Lists")]
    [SerializeField] private List<Transform> allPages;
    [SerializeField] private List<AnimalPagesByType> animalDisplayers;
    [SerializeField] private List<Image> backgroundImages;
    [SerializeField] private List<Image> hiddenAnimalImages;
    [SerializeField] private List<Image> revealedAnimalImages;

    [Header("Current page data")]
    [SerializeField] private int currentlyOpenPageIndex = -1;
    [SerializeField] private int filledAnimalsCount;

    [Header("Required refs")]
    [SerializeField] private Transform pagesParent;
    [SerializeField] private GameObject presentImage;
    [SerializeField] private CanvasGroup getRewardsCanvasGroup;
    [SerializeField] private ImageSwapHelper[] animalTypeSwapHelpers;

    [Header("Reveal data")]
    [SerializeField] private float animalRevealTime;
    [SerializeField] private float rewardButtonRevealTime;


    private void Awake()
    {
        SetInitialData();
    }

    public void InitAnimalAlbum(AnimalsManager animalManager, Player player)
    {
        localAnimalManager = animalManager;
        localPlayer = player;

        SwitchAnimalCategory(0);
    }

    public void SwitchAnimalCategory(int index)
    {
        SetCategoriesDisplay(index);
    }
    private void SetCategoriesDisplay(int index)
    {
        if (currentlyOpenPageIndex == index) return;

        ResetAlbumData(index);

        currentOpenType = animalDisplayers[currentlyOpenPageIndex].animalType;
        for (int i = 0; i < animalTypeSwapHelpers.Length; i++)
        {
            if (i == index)
            {
                animalTypeSwapHelpers[i].SetActivatedChild();

                if (i > allPages.Count - 1) continue;
                allPages[i].gameObject.SetActive(true);
            }
            else
            {
                animalTypeSwapHelpers[i].SetDeActivatedChild();

                if (i > allPages.Count - 1) continue;
                allPages[i].gameObject.SetActive(false);
            }
        }

        CheckAndRevealAnimals();
    }
    private void ResetAlbumData(int index)
    {
        currentlyOpenPageIndex = index;
        filledAnimalsCount = 0;
        getRewardsCanvasGroup.alpha = 0;
        getRewardsCanvasGroup.gameObject.SetActive(false);
        presentImage.gameObject.SetActive(true);
    }

    private void CheckAndRevealAnimals()
    {
        AnimalPagesByType page = animalDisplayers[currentlyOpenPageIndex];
        if (page == null)
        {
            Debug.LogError("No page!");
            return;
        }


        List<Image> imagesToReveal = new List<Image>();
        List<Image> deactivateOnEnd = new List<Image>();

        bool isRevealing = false;
        List<OwnedAnimalDataSet> ownedAnimals = localAnimalManager.GetUnlockedAnimals().Where(p => p.animalTypeEnum == currentOpenType).ToList();


        foreach (OwnedAnimalDataSet ownedAnimal in ownedAnimals)
        {
            // counts filled animals in page
            filledAnimalsCount++;

            bool animalAlreadyRevealed = localAnimalManager.CheckAnimalAlreadyInAlbum(ownedAnimal.animalEnum);
            if (animalAlreadyRevealed) continue;

            isRevealing = true;



            localAnimalManager.AddAnimalToAlbum(ownedAnimal.animalEnum);

            foreach (SpecificDisplayerAnimalAlbum specificAnimalDisplayer in page.animalsInPage)
            {
                if (specificAnimalDisplayer.animal == ownedAnimal.animalEnum)
                {
                    imagesToReveal.Add(specificAnimalDisplayer.revealedBGImage);
                    imagesToReveal.Add(specificAnimalDisplayer.revealedAnimalImage);

                    deactivateOnEnd.Add(specificAnimalDisplayer.hiddenAnimalImage);

                    break;
                }
            }
            Debug.Log("Got here with animal: " + ownedAnimal.animalEnum + " of type: " + ownedAnimal.animalTypeEnum);
        }

        if (isRevealing)
        {
            StartCoroutine(RevealAnimalsAction(imagesToReveal, deactivateOnEnd, filledAnimalsCount));
        }

        //make this somehow not hardcoded!
        if (!isRevealing && filledAnimalsCount == 4)
        {
            if(localAnimalManager.CheckPageAlreadyClaimedInAlbum(currentOpenType))
            {
                presentImage.SetActive(false);
            }
            else
            {
                getRewardsCanvasGroup.gameObject.SetActive(true);
                getRewardsCanvasGroup.alpha = 1;
            }
        }
    }

    private IEnumerator RevealAnimalsAction(List<Image> imagesToReveal, List<Image> deactivateOnEnd, int filledAnimals)
    {
        foreach (Image image in imagesToReveal)
        {
            Material mat = image.material;
            string keyname = "_DissolveSprite";
            GeneralFloatValueTo(mat, 0, 1.7f, animalRevealTime, LeanTweenType.linear, keyname);
        }

        yield return new WaitForSeconds(animalRevealTime + 0.1f);

        foreach (Image image in deactivateOnEnd)
        {
            image.gameObject.SetActive(false);
        }

        //make this somehow not hardcoded!
        if (filledAnimals == 4)
        {
            getRewardsCanvasGroup.gameObject.SetActive(true);
            getRewardsCanvasGroup.blocksRaycasts = false; //can't click on
            GeneralFloatValueTo(getRewardsCanvasGroup, 0, 1, rewardButtonRevealTime, LeanTweenType.linear);
            yield return new WaitForSeconds(rewardButtonRevealTime + 0.1f);
            getRewardsCanvasGroup.blocksRaycasts = true;//can click on
        }
    }

    public void GivePlayerRewardsFromAnimalAlbum()
    {
        getRewardsCanvasGroup.alpha = 0;
        getRewardsCanvasGroup.gameObject.SetActive(false);
        presentImage.SetActive(false);

        localAnimalManager.AddToAlbumPagesCompleted(currentOpenType);
        Debug.Log("Gave player rewards");

        int amountOfReward = localAnimalManager.RollAmountOfReward();
        localPlayer.AddCoins(amountOfReward);
        UIManager.instance.DisplayAnimalAlbumReward(amountOfReward);
    }

    [ContextMenu("Populate All Pages from inspector")]
    public void PopulateAllPages()
    {
        allPages = new List<Transform>();

        for (int i = 0; i < pagesParent.childCount; i++)
        {
            allPages.Add(pagesParent.GetChild(i));
        }
    }


    [ContextMenu("Set animal album data from inspector")]
    public void SetInitialData()
    {
        // this can be called from both inspector and in game!
        // used in game to reset everything to before revealed on awake

        animalDisplayers.Clear();
        backgroundImages.Clear();
        hiddenAnimalImages.Clear();
        revealedAnimalImages.Clear();

        for (int i = 0; i < pagesParent.childCount; i++)
        {
            AnimalPagesByType newPage = new AnimalPagesByType();
            newPage.animalType = (AnimalTypesInGame)i;
            newPage.animalsInPage = new List<SpecificDisplayerAnimalAlbum>();

            for (int k = 0; k < pagesParent.GetChild(i).childCount; k++)
            {
                SpecificDisplayerAnimalAlbum displayer;
                pagesParent.GetChild(i).GetChild(k).gameObject.TryGetComponent<SpecificDisplayerAnimalAlbum>(out displayer);

                if (displayer == null)
                {
                    Debug.LogError("No displayer");
                    return;
                }


                newPage.animalsInPage.Add(displayer);
            }

            animalDisplayers.Add(newPage);

            if (i> 0)
            {
                // make sure that only the first page is active
                pagesParent.GetChild(i).gameObject.SetActive(false);
            }
        }


        foreach (AnimalPagesByType page in animalDisplayers)
        {
            foreach (SpecificDisplayerAnimalAlbum displayer in page.animalsInPage)
            {
                backgroundImages.Add(displayer.revealedBGImage);
                hiddenAnimalImages.Add(displayer.hiddenAnimalImage);
                revealedAnimalImages.Add(displayer.revealedAnimalImage);
            }
        }

        foreach (Image image in hiddenAnimalImages)
        {
            Material mat = new Material(animalFadeShader);
            mat.renderQueue = 3001;
            image.material = mat;

            mat.SetFloat("_DissolveSprite", 1.7f);
            //string keyname = "_DissolveSprite";
            //GeneralFloatValueTo(image.gameObject, 0, 1.7f, 0, LeanTweenType.linear, mat, keyname);
        }

        foreach (Image image in revealedAnimalImages)
        {
            Material mat = new Material(animalFadeShader);
            mat.renderQueue = 3002;
            image.material = mat;
            mat.SetFloat("_DissolveSprite", 0f);
        }

        foreach (Image image in backgroundImages)
        {
            Material mat = new Material(animalFadeShader);
            mat.renderQueue = 3000;
            image.material = mat;
            mat.SetFloat("_DissolveSprite", 0f);
        }
    }
}
