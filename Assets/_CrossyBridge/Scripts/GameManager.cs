using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using SgLib;

#if EASY_MOBILE
using EasyMobile;
#endif

public enum GameState
{
    Prepare,
    Playing,
    Paused,
    PreGameOver,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static event System.Action<GameState, GameState> GameStateChanged = delegate { };

    public GameState GameState
    {
        get
        {
            return _gameState;
        }
        private set
        {
            if (value != _gameState)
            {
                GameState oldState = _gameState;
                _gameState = value;

                GameStateChanged(_gameState, oldState);
            }
        }
    }

    private GameState _gameState = GameState.Prepare;

    [Header("Check to enable premium features (require EasyMobile plugin)")]
    public bool enablePremiumFeatures = true;

    [Header("Gameplay Config")]
    public int initialPlanes = 5;

    //How many plane you have on scene
    public int minPlaneNumber = 8;
    //Min plane's number of path
    public int maxPlaneNumber;
    //Max plane's number of path
    public int maxFluctuationRange = 6;
    //Max fluctuation range of plane
    public int minFluctuationRange = 3;
    //Min fluctuation range of plane
    public float minPlaneSpeed;
    //Min plane speed
    public float maxPlaneSpeed;
    //Max plane speed
    public float minDeviation = 0.3f;
    //Min deviation when moving plane stop at the same position with plane ahead
    public int bridgeNumber = 5;
    /* 5 brigdes first, you will have 1 moving plane, 5 brigdes next, you will have 2 moving plane........
    , moving plane will be plus 1 everytime you cross 5 brigdes*/
    public float firstMovingPlaneFrequency = 0.9f;
    [Range(0f, 1f)]
    float movingPlaneFrequency = 0.6f;
    //Probability to create moving plane
    public float amplitudeDecreases = 0.1f;
    public float limitMovingPlaneFrequency = 0.5f;
    [Range(0f, 1f)]
    public float goldFrequency;

    [Header("Object Preferences")]
    public PlayerController playerController;
    public UIManager uIManager;

    [Header("Plane Prefabs")]
    public List<GameObject> PlanPrefabs;
    public List<GameObject> MovingPlanePrefabs;
    public List<GameObject> LastForwardPlanePrefab;
    public List<GameObject> LastLeftPlanePrefab;


    public GameObject normalSummerPlanePrefab;


    public GameObject firstPlane;
    public GameObject snowParticle;
    public GameObject goldPrefab;
    [HideInInspector]
    public List<GameObject> listMovingPlane = new List<GameObject>();
    [HideInInspector]
    public int listIndex = 0;
    [HideInInspector]
    public bool gameOver = false;

    private GameObject normalPlane;
    private GameObject lastForwardPlane;
    private GameObject lastLeftPlane;
    private GameObject movingPlane;

    private GameObject currentPlane;
    private Vector3 planePosition;
    private Vector3 forwardDirection = Vector3.forward;
    private Vector3 leftDirection = Vector3.left;
    private float checkPosition;
    private float xPlaneScale;
    private float yPlaneScale;
    private float zPlaneScale;
    private int planeNumber;
    private int countPlane = 0;
    public int turn = 1;
    private int countMovingPlane = 0;

    public static GameManager Instance;


    int _planPrefabIndex;


    void Awake()
    {
        _planPrefabIndex = Random.Range(0, PlanPrefabs.Count);
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // Use this for initialization
    void Start()
    {
        GameState = GameState.Prepare;

        //PlayerPrefs.DeleteAll();
        xPlaneScale = Mathf.Round(normalSummerPlanePrefab.GetComponent<Renderer>().bounds.size.x);
        yPlaneScale = Mathf.Round(normalSummerPlanePrefab.GetComponent<Renderer>().bounds.size.y);
        zPlaneScale = Mathf.Round(normalSummerPlanePrefab.GetComponent<Renderer>().bounds.size.z);

        //Random plane's type
        RandomPlaneType();

        //Create position for next plane
        planePosition = firstPlane.transform.position + forwardDirection * zPlaneScale;

        //Change first plane 
        firstPlane.GetComponent<MeshFilter>().sharedMesh = normalPlane.GetComponent<MeshFilter>().sharedMesh;
        firstPlane.GetComponent<Renderer>().sharedMaterial = normalPlane.GetComponent<Renderer>().sharedMaterial;
        //Set parent
        firstPlane.transform.SetParent(transform);
        //reset score and create plane
        ScoreManager.Instance.Reset();

        for (int i = 0; i < initialPlanes; i++)
        {
            currentPlane = (GameObject)Instantiate(normalPlane, planePosition, Quaternion.Euler(0, 0, 0));
            planePosition = currentPlane.transform.position + forwardDirection * zPlaneScale;
            currentPlane.transform.SetParent(transform);
        }

        Vector3 planeBehindPosition = firstPlane.transform.position + Vector3.back * zPlaneScale;

        for (int i = 0; i < 3; i++)
        {
            GameObject planeBehind = Instantiate(normalPlane, planeBehindPosition, Quaternion.Euler(0, 0, 0)) as GameObject;
            planeBehind.transform.SetParent(transform);
            planeBehindPosition = planeBehind.transform.position + Vector3.back * zPlaneScale;
        }

        planeNumber = Random.Range(minPlaneNumber, maxPlaneNumber); //Create plane number for path




        SoundManager.Instance.PlayMusic(SoundManager.Instance.background);
        StartCoroutine(CreatePlane());

    }

    // Update is called once per frame
    void Update()
    {
        // Exit on Android Back button
#if UNITY_ANDROID && EASY_MOBILE
        if (Input.GetKeyUp(KeyCode.Escape))
        {   

            NativeUI.AlertPopup alert = NativeUI.ShowTwoButtonAlert(
                                      "Exit Game",
                                      "Are you sure you want to exit?",
                                      "Yes", 
                                      "No");

            if (alert != null)
            {
                alert.OnComplete += (int button) =>
                {
                    switch (button)
                    {
                        case 0: // Yes
                            Application.Quit();
                            break;
                        case 1: // No
                            break;
                    }
                };
            }     
        }
#endif

        if (playerController.isRunning && !gameOver) //Not game over
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (listIndex < listMovingPlane.Count) //Make sure the the listIndex not run out of the list
                {
                    if (listMovingPlane[listIndex].GetComponent<PlaneController>().isVisible) //This moving plane is visible
                    {
                        listMovingPlane[listIndex].GetComponent<PlaneController>().stopMoving = true; //Stop moving plane

                        GameObject currentPlane = listMovingPlane[listIndex];

                        Vector3 point = new Vector3(0, yPlaneScale / 2, 0); //Draw raycast from this point

                        if (currentPlane.transform.rotation == Quaternion.Euler(0, -90, 0))
                        {
                            Ray rayRight = new Ray(currentPlane.transform.position + point, Vector3.right);
                            RaycastHit hit;
                            if (Physics.Raycast(rayRight, out hit, zPlaneScale)) //Draw raycast with length is zPlaneScale
                            {
                                PlaneController planeController = hit.collider.GetComponent<PlaneController>();

                                if (planeController != null)
                                {
                                    if (planeController.isMove) //This plane is normal plane
                                    {
                                        checkPosition = hit.transform.position.z; //Remember z position of this plane 
                                    }
                                }


                                float distance = Mathf.Abs(currentPlane.transform.position.z - checkPosition);

                                if (distance <= minDeviation)//distance is less than minDeviation -> bonus coin
                                {
                                    currentPlane.transform.position = new Vector3(currentPlane.transform.position.x,
                                        currentPlane.transform.position.y,
                                        checkPosition);

                                    CreateGold(currentPlane, 1); //Bonus coin

                                    ScoreManager.Instance.AddScore(2); // Bonus score

                                    SoundManager.Instance.PlaySound(SoundManager.Instance.placeUp);
                                }
                                else
                                {
                                    SoundManager.Instance.PlaySound(SoundManager.Instance.place);
                                }
                            }
                        }
                        else
                        {
                            Ray rayBack = new Ray(currentPlane.transform.position + point, Vector3.back);
                            RaycastHit hit;
                            if (Physics.Raycast(rayBack, out hit, zPlaneScale))
                            {
                                PlaneController planeController = hit.collider.GetComponent<PlaneController>();
                                if (planeController != null)
                                {
                                    if (!planeController.isMove) //This is normal plane
                                    {
                                        checkPosition = hit.transform.position.x; //Remember x position of this plane
                                    }
                                }


                                float distance = Mathf.Abs(currentPlane.transform.position.x - checkPosition);
                                if (distance <= minDeviation)//distance is less than minDeviation -> bonus coin
                                {
                                    currentPlane.transform.position = new Vector3(checkPosition,
                                        currentPlane.transform.position.y,
                                        currentPlane.transform.position.z);

                                    CreateGold(currentPlane, 1); //Bonus coin

                                    ScoreManager.Instance.AddScore(2); // Bonus score

                                    SoundManager.Instance.PlaySound(SoundManager.Instance.placeUp);
                                }
                                else
                                {
                                    SoundManager.Instance.PlaySound(SoundManager.Instance.place);
                                }
                            }
                        }

                        listIndex++; //Next moving plane
                    }
                }
            }
        }
    }





    void RandomPlaneType()
    {
        normalPlane = PlanPrefabs[_planPrefabIndex];
        lastForwardPlane = LastForwardPlanePrefab[_planPrefabIndex];
        lastLeftPlane = LastLeftPlanePrefab[_planPrefabIndex];
        movingPlane = MovingPlanePrefabs[_planPrefabIndex];
        if (_planPrefabIndex == 1) snowParticle.SetActive(true);
        else snowParticle.SetActive(false);
    }


    public void StartGame()
    {
        GameState = GameState.Playing;
    }

    public void GameOver()
    {
        InsertialAds.instance.ShowAd();
        gameOver = true;
        GameState = GameState.GameOver;
        SoundManager.Instance.StopMusic();
    }

    public void RestartGame(float delay)
    {
        StartCoroutine(CRRestart(delay));
    }

    IEnumerator CRRestart(float delay = 0)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


    void GeneratePlane(bool isForwardSide, bool islastPlane = false, bool cannotBeMoving = false)
    {
        float movingPlaneProbability = Random.Range(0f, 1f);
        if (islastPlane) movingPlaneProbability = 2;
        if (cannotBeMoving) movingPlaneProbability = 2;

        if (movingPlaneProbability <= movingPlaneFrequency && countPlane != 0 && countPlane % 2 == 0) //Create moving plane
        {
            //How many moving plane is created 
            int movingPlaneNumber = (countMovingPlane / bridgeNumber) + 1;
            countMovingPlane++;
            for (int i = 0; i < movingPlaneNumber; i++)
            {
                int movingLength = Random.Range(minFluctuationRange, maxFluctuationRange); //Create fluctuation range of plane
                float indexPisitionMovingPlane = Random.Range(0f, 1f);


                if (isForwardSide)
                {
                    currentPlane = (GameObject)Instantiate(movingPlane, planePosition, Quaternion.Euler(0, 0, 0));
                    planePosition = currentPlane.transform.position + forwardDirection * zPlaneScale;
                    PlaneController currentPlaneController = currentPlane.GetComponent<PlaneController>();


                    if (indexPisitionMovingPlane < 0.5f)
                    {
                        currentPlane.transform.position += new Vector3(movingLength, 0, 0);
                        currentPlaneController.isTheTopXAxis = true;
                    }
                    else
                    {
                        currentPlane.transform.position += new Vector3(-movingLength, 0, 0);
                        currentPlaneController.isTheTopXAxis = false;
                    }

                    currentPlaneController.movingByXAxis = true;
                    currentPlaneController.planeMovingSpeed = Random.Range(minPlaneSpeed, maxPlaneSpeed);
                    currentPlaneController.movingAmplitude = movingLength;
                    currentPlaneController.isMove = true;

                    listMovingPlane.Add(currentPlane);
                }
                else
                {
                    currentPlane = (GameObject)Instantiate(movingPlane, planePosition, Quaternion.Euler(0, -90, 0)); //Create plane                           
                    planePosition = currentPlane.transform.position + leftDirection * zPlaneScale;//Create position for next plane
                    PlaneController currentPlaneController = currentPlane.GetComponent<PlaneController>();

                    if (indexPisitionMovingPlane < 0.5f)
                    {
                        currentPlane.transform.position += new Vector3(0, 0, movingLength);
                        currentPlaneController.isTheTopZAxis = true;
                    }
                    else
                    {
                        currentPlane.transform.position += new Vector3(0, 0, -movingLength);
                        currentPlaneController.isTheTopZAxis = false;
                    }

                    currentPlaneController.movingByXAxis = false;
                    currentPlaneController.planeMovingSpeed = Random.Range(minPlaneSpeed, maxPlaneSpeed);
                    currentPlaneController.movingAmplitude = movingLength;
                    currentPlaneController.isMove = true;

                    listMovingPlane.Add(currentPlane);
                }
            }
        }
        else //Create normal plane
        {
            if (!islastPlane)
            {
                if (isForwardSide)
                {
                    currentPlane = (GameObject)Instantiate(normalPlane, planePosition, Quaternion.Euler(0, 0, 0));
                    planePosition = currentPlane.transform.position + forwardDirection * zPlaneScale;
                }
                else
                {
                    currentPlane = (GameObject)Instantiate(normalPlane, planePosition, Quaternion.Euler(0, 90, 0));
                    planePosition = currentPlane.transform.position + leftDirection * zPlaneScale;
                }
            }
            else
            {
                // Create last plane
                if (isForwardSide)
                {
                    currentPlane = (GameObject)Instantiate(lastForwardPlane, planePosition, Quaternion.Euler(0, 0, 0));
                    planePosition = currentPlane.transform.position + leftDirection * zPlaneScale;
                    planePosition += new Vector3(1, 0, 1);
                }
                else
                {
                    currentPlane = (GameObject)Instantiate(lastLeftPlane, planePosition, Quaternion.Euler(0, 90, 0));
                    planePosition = currentPlane.transform.position + forwardDirection * zPlaneScale;
                    planePosition -= new Vector3(1, 0, 1);
                }
                currentPlane.GetComponent<PlaneController>().isTheLastPlane = true;
                for (int i = 0; i < 2; i++)
                {
                    GeneratePlane(!isForwardSide, cannotBeMoving: true);
                }
            }

            currentPlane.transform.SetParent(transform);
            CreateGold(currentPlane, goldFrequency);
        }
    }




    void CreateGold(GameObject plane, float frequency)
    {
        if (Random.value <= frequency)
        {
            Vector3 goldPos = new Vector3(plane.transform.position.x, -0.5f, plane.transform.position.z);
            Instantiate(goldPrefab, goldPos, Quaternion.identity);
        }
    }







    IEnumerator CreatePlane()
    {
        while (!gameOver)
        {

            if (transform.childCount < maxPlaneNumber)
            {
                if (countPlane % 5 == 0)
                {
                    if (turn == 1)
                    {
                        GeneratePlane(true, islastPlane: true);
                    }
                    else
                    {
                        GeneratePlane(false, islastPlane: true);
                    }
                    turn = turn * (-1);
                }
                if (turn == 1)
                {
                    GeneratePlane(true);
                }
                else
                {
                    GeneratePlane(false);
                }

                countPlane++;


            }
            yield return null;
        }
    }




    public void rewardPlayerForAds(int coins)
    {
        CoinManager.Instance.AddCoins(coins);
        UIManager.Instance.ShowRewardUI(coins);
    }
}

