using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InLevelUserControls : MonoBehaviour
{
    // THINK ABOUT MAYBE CRATING INHERITENCE FOR "CONTROLS"

    [Header("Raycast data")]
    [SerializeField] private LayerMask everythingLayer;
    [SerializeField] private LayerMask tileGrabbingLayer;
    [SerializeField] private LayerMask tileInsertingLayer;
    [SerializeField] private LayerMask dealLayer;
    [SerializeField] private float overlapRadius;
    [SerializeField] private float overlapZDepth;

    [Header("Follow settings")]
    [SerializeField] private float pickupSpeed;
    [SerializeField] private float tileFollowSpeed;
    [SerializeField] private Vector3 tileFollowOffset;

    [Header("General")]
    [SerializeField] private TileParentLogic currentTileToMove;
    [SerializeField] private TileHolder tileOriginalHolder;
    [SerializeField] private TileHolder lastTileHolder;

    [Header("Needed Classes")]
    [SerializeField] private Ring gameRing;
    [SerializeField] private ClipManager gameClip;

    private Vector3 touchPos;
    private Vector3 tileOriginalPos;

    private void Update()
    {
        // we use both isusingUI and isduringfade since we have some ui elements
        // as part of a ui object that we wat to not be able to click when were fadind
        // but do what to be able to click when using ui
        bool canUseControls = !UIManager.IS_DURING_POTION_USAGE && 
            !UIManager.IS_USING_UI && 
            !UIManager.IS_DURING_TRANSITION && 
            !PowerupManager.USING_POWER;

        if (canUseControls)
        {
            NormalControls();
            return;
        }

        if(PowerupManager.USING_POWER && !UIManager.IS_DURING_POTION_USAGE)
        {
            PowerUpControls();
        }
    }


    public void InitUserControls(Ring _gameRing, ClipManager _gameClip)
    {
        gameRing = _gameRing;
        gameClip = _gameClip;
    }

    private void NormalControls()
    {
        Touch touch;


        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);

            touchPos = touch.position;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnTouchBegin();
                    break;
                case TouchPhase.Moved:
                    if (currentTileToMove) OnTouchMoveOrStationairy();
                    break;
                case TouchPhase.Stationary:
                    if (currentTileToMove) OnTouchMoveOrStationairy();
                    break;
                case TouchPhase.Ended:
                    if (currentTileToMove) OnTouchEnd();
                    break;
                case TouchPhase.Canceled:
                    Debug.LogError("Cancelled??");
                    break;
                default:
                    break;
            }
        }

    }

    private void PowerUpControls()
    {
        Touch touch;

        if (Input.touchCount > 0)
        {
            touch = Input.GetTouch(0);

            touchPos = touch.position;

            switch (touch.phase)
            {
                case TouchPhase.Began:
                    OnTouchBeginPower();
                    break;
            }
        }

    }

    private void OnTouchBegin()
    {

        RaycastHit intersectionsArea = GetFirstIntersection3D(touchPos, tileGrabbingLayer);

        // we also already have a point on raycast function called "GetIntersectionsAtPoint"

        if (intersectionsArea.transform)
        {
            TileHolder holder = intersectionsArea.transform.GetComponent<TileHolder>();

            if (holder.heldTile && !holder.isLocked && !holder.isStone)
            {
                GrabTile(holder);
            }
            else
            {
                ReleaseData();
            }
        }
        else
        {
            ReleaseData();
        }
    }
    private void OnTouchBeginPower()
    {
        RaycastHit intersectionsArea = GetFirstIntersection3D(touchPos, everythingLayer);

        if(TutorialManager.IS_DURING_TUTORIAL)
        {
            if (intersectionsArea.transform)
            {
                if(TutorialManager.instance.ReturnHitCurrentNeededObject(intersectionsArea.transform))
                {
                    IPowerUsable powerLogic = null;

                    intersectionsArea.transform.TryGetComponent<IPowerUsable>(out powerLogic);

                    PowerupManager.instance.InitPowerUsageData(intersectionsArea.transform, powerLogic);
                }
            }
        }
        else
        {
            if (intersectionsArea.transform)
            {
                IPowerUsable powerLogic = null;

                intersectionsArea.transform.TryGetComponent<IPowerUsable>(out powerLogic);

                PowerupManager.instance.InitPowerUsageData(intersectionsArea.transform, powerLogic);
            }
            else
            {
                ReleasePotionData();
            }
        }
    }

    private void GrabTile(TileHolder holder)
    {
        SoundManager.instance.CallPlaySound(sounds.TilepPickup);

        IGrabTileFrom grabbedObject = holder.GetComponent<IGrabTileFrom>();

        if(grabbedObject != null)
        {
            currentTileToMove = holder.heldTile;
            holder.OnRemoveTileDisplay();
            tileOriginalHolder = holder;
            grabbedObject.GrabTileFrom();
            tileOriginalPos = currentTileToMove.transform.localPosition;

            LeanTween.move(currentTileToMove.gameObject, TargetPosOffset(), pickupSpeed);

            RotateTileTowardsBoard();
        }
        else
        {
            Debug.LogError("Tried to grab object that doesn't have grabbable interface");
        }
    }

    private void OnTouchMoveOrStationairy()
    {
        RotateTileTowardsBoard();

        SmoothPieceMover();

        /// do VFX according to hits here.
    }
    private void SmoothPieceMover()
    {
        currentTileToMove.transform.position = Vector3.Lerp(currentTileToMove.transform.position, TargetPosOffset(), Time.deltaTime * tileFollowSpeed);
    }

    private void OnTouchEnd()
    {
        //RaycastHit intersection = GetFirstIntersection3D(touchPos, tileInsertingLayer);
        RaycastHit[] intersection = GetIntersectionsArea3D(touchPos, tileInsertingLayer);
        // we also already have a point on raycast function called "GetIntersectionsAtPoint"

        if (intersection.Length > 0 && intersection[0].transform)
        {
            CellBase droopedOnObject = null;
            intersection[0].transform.TryGetComponent<CellBase>(out droopedOnObject);

            if (droopedOnObject == null)
            {
                Debug.LogError("no interface of type dropped on.");
                return;
            }

            //don't place the tile if it's the last one and we have problems in ring
            if (!droopedOnObject.heldTile && gameRing.LastPieceRingProblems())
            {
                UIManager.instance.DisplayInLevelRingHasNonMatchingMessage();

                return;
            }

            if (TutorialManager.IS_DURING_TUTORIAL)
            {
                if (TutorialManager.instance.ReturnHitCurrentNeededObject(intersection[0].transform))
                {
                    droopedOnObject.DroppedOn(currentTileToMove, GameManager.gameRing);

                    tileOriginalHolder.RemoveTile();

                    StartCoroutine(TutorialManager.instance.AdvanceTutorialStep());
                }
                else
                {
                    ReturnHome();
                }
            }
            else
            {
                if (!droopedOnObject.DroppedOn(currentTileToMove, GameManager.gameRing))
                {
                    //can't place tile

                    ReturnHome();
                }
                else
                {

                    //If we enter here that means we actually succeeded placing the tile.
                    //this does not mean that the tile is a good match! this is why we check to see if we have problems.
                    //we did the connection checks, so we must "remove" the tile if we have problems (use the "GrabTileFrom" sicne we know it has to be a cell)

                    if (gameRing.LastPieceRingProblems())
                    {
                        UIManager.instance.DisplayInLevelRingHasNonMatchingMessage();
                        lastTileHolder = droopedOnObject;
                        return;
                    }
                    else
                    {
                        tileOriginalHolder.RemoveTile();
                    }
                }
            }
        }
        else
        {
            ReturnHome();
        }

        ReleaseData();
    }

    private RaycastHit2D[] GetIntersectionsArea(Vector3 touchPos, LayerMask layerToHit)
    {
        Vector3 pointToCheck = touchPos;
        pointToCheck.z = gameRing.transform.position.z;

        Vector3 posCheck = Camera.main.ScreenToWorldPoint(pointToCheck);

        RaycastHit2D[] hit2D = Physics2D.CircleCastAll(posCheck, overlapRadius, transform.right, 0, layerToHit);


        return hit2D;

    }

    private RaycastHit2D GetIntersectionsAtPoint(Vector3 touchPos, LayerMask layerToHit)
    {
        Ray touchRay;

        touchRay = Camera.main.ScreenPointToRay(touchPos);

        RaycastHit2D hit2D = Physics2D.GetRayIntersection(touchRay, Mathf.Infinity, layerToHit);

        return hit2D;

    }

    private RaycastHit[] GetIntersectionsArea3D(Vector2 touchpos, LayerMask layerToHit)
    {
        Vector3 pointToCheck = touchPos;
        pointToCheck.z = overlapZDepth;

        Vector3 posCheck = Camera.main.ScreenToWorldPoint(pointToCheck);

        RaycastHit[] hits = Physics.SphereCastAll(posCheck, overlapRadius, transform.right, 0, layerToHit);


        return hits;
    }
    private RaycastHit GetFirstIntersection3D(Vector2 touchpos, LayerMask layerToHit)
    {
        RaycastHit hit;

        Vector3 pointToCheck = touchPos;
        Vector3 posCheck = Camera.main.ScreenToWorldPoint(pointToCheck);

        Ray ray = Camera.main.ScreenPointToRay(pointToCheck);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerToHit))
        {
            //Debug.Log("Did Hit");
        }


        return hit;
    }

    private void RotateTileTowardsBoard()
    {
        float difY = gameRing.transform.position.z - currentTileToMove.transform.position.z;
        float difX = gameRing.transform.position.x + currentTileToMove.transform.position.x ;

        float angle = Mathf.Atan2(difY, difX) * Mathf.Rad2Deg;

        currentTileToMove.transform.rotation = Quaternion.Euler(new Vector3(0, angle + 90, 0));
    }

    private void ReturnHome()
    {
        SoundManager.instance.CallPlaySound(sounds.TilePlace);

        LeanTween.cancel(currentTileToMove.gameObject);

        CellBase cellbasee = tileOriginalHolder as CellBase;

        if(cellbasee)
        {
            cellbasee.DroppedOn(currentTileToMove, gameRing);
        }
        else
        {
            tileOriginalHolder.RecieveTileDisplayer(currentTileToMove);
        }
    }
    public void ReturnHomeBadRingConnections()
    {
        UndoSystem.instance.RemoveSpecificEntryTile(currentTileToMove);
        LeanTween.cancel(currentTileToMove.gameObject);

        IGrabTileFrom grabbedObject = lastTileHolder.GetComponent<IGrabTileFrom>();

        if(grabbedObject != null)
        {
            grabbedObject.GrabTileFrom();
        }

        tileOriginalHolder.RecieveTileDisplayer(currentTileToMove);

        currentTileToMove = null;
    }

    private void ReleaseData()
    {
        tileOriginalPos = Vector3.zero;
        currentTileToMove = null;
        tileOriginalHolder = null;
    }
    private void ReleasePotionData()
    {
        PowerupManager.instance.ResetPowerUpData();
    }

    private Vector3 TargetPosOffset()
    {
        float targetPosClacZ = tileOriginalPos.z + tileFollowOffset.z;

        Vector3 targetPos = Camera.main.ScreenToWorldPoint(new Vector3(touchPos.x + tileFollowOffset.x, touchPos.y, targetPosClacZ));

        return targetPos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 pointToCheck = Input.mousePosition;
        pointToCheck.z = overlapZDepth;

        Vector3 posCheck = Camera.main.ScreenToWorldPoint(pointToCheck);

        Gizmos.DrawWireSphere(posCheck + transform.right * 0, overlapRadius);

    }
}
