using UnityEngine;
using System.Collections;
using SgLib;

public class PlayerController : MonoBehaviour
{
    public static event System.Action PlayerDie = delegate {};

    [Header("Gameplay Config")]
    public float movingSpeed = 13f;
    //Player moving speed
    public float rotatingSpeed = 250f;
    //Player rotating speed

    [Header("Object Preferences")]
    public GameManager gameManager;
    public GameObject playerChild;
    public ParticleSystem particle;
    [HideInInspector]
    public Vector3 dir;
    [HideInInspector]
    public bool isRunning;

    private Vector3 raycastPoint;
    private bool isRotateLeft;
    private bool isRotateForward;
    private float fixDistance;
    private float zPlayerScale;
    private float zPlaneScale;
    private float xPlaneScale;


    void Start()
    {
        // Change the character to the selected one
        GameObject currentCharacter = CharacterManager.Instance.characters[CharacterManager.Instance.CurrentCharacterIndex];
        Mesh charMesh = currentCharacter.GetComponent<MeshFilter>().sharedMesh;
        Material charMaterial = currentCharacter.GetComponent<Renderer>().sharedMaterial;
        playerChild.GetComponent<MeshFilter>().mesh = charMesh;
        playerChild.GetComponent<MeshRenderer>().material = charMaterial;


        dir = Vector3.forward; //first moving direction
        zPlaneScale = gameManager.normalSummerPlanePrefab.GetComponent<Renderer>().bounds.size.z;
        xPlaneScale = gameManager.normalSummerPlanePrefab.GetComponent<Renderer>().bounds.size.x;

        fixDistance = ((zPlaneScale / 2) - xPlaneScale) + (xPlaneScale / 2);
        zPlayerScale = playerChild.GetComponent<Renderer>().bounds.size.z;

        StartCoroutine(MovePlayer());
    }
	
    // Update is called once per frame
    void Update()
    {
        if (!gameManager.gameOver)
        {
            // Check game over
            if (transform.rotation == Quaternion.Euler(0, 0, 0))
            {
                raycastPoint = new Vector3(0, 1, -zPlayerScale / 2f - 0.3f);
            }
            else
            {
                raycastPoint = new Vector3(zPlayerScale / 2f + 0.3f, 1, 0);
            }

            Debug.DrawLine(transform.position + raycastPoint, transform.position + raycastPoint + Vector3.down * 5f, Color.green);


            Ray raydown = new Ray(transform.position + raycastPoint, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(raydown, out hit, 5f)) //Still alive
            {
                if (!gameManager.gameOver)
                {
                    if (hit.collider.GetComponent<PlaneController>().isTheLastPlane) //This is the last plane, turn right here
                    {
                        if (dir == Vector3.forward) //Player moving forward -> turn and rotate left
                        {
                            isRotateForward = false; //Reset

                            if (transform.position.z >= hit.transform.position.z - zPlaneScale && !isRotateLeft) //Rotate left
                            {
                                isRotateLeft = true;
                                StartCoroutine(RotatePlayer(Vector3.down, -90));
                            }

                            if (transform.position.z >= hit.transform.position.z + fixDistance) //change direction
                            {
                                float zAxis = hit.transform.position.z + fixDistance;
                                transform.position = new Vector3(transform.position.x, transform.position.y, zAxis);
                                dir = Vector3.left;
                                hit.collider.GetComponent<PlaneController>().isTheLastPlane = false;
                            }
                        }
                        else //Player moving left -> turn and rotate forward
                        {
                            isRotateLeft = false; //Reset

                            if (transform.position.x <= hit.transform.position.x + zPlaneScale && !isRotateForward)
                            {
                                isRotateForward = true;
                                StartCoroutine(RotatePlayer(Vector3.up, 90)); //Rotate
                            }
                            if (transform.position.x <= hit.transform.position.x - fixDistance) //Change direction
                            {
                                float xAxis = hit.transform.position.x - fixDistance;
                                transform.position = new Vector3(xAxis, transform.position.y, transform.position.z);
                                dir = Vector3.forward;
                                hit.collider.GetComponent<PlaneController>().isTheLastPlane = false;
                            }
                        }
                    }
                }
            }
            else //Die -> game over
            {
                playerChild.GetComponent<Animator>().enabled = false;
                if (gameManager.listIndex < gameManager.listMovingPlane.Count)
                {
                    gameManager.listMovingPlane[gameManager.listIndex].GetComponent<PlaneController>().stopMoving = true;
                }

                isRunning = false;

                if (!gameManager.gameOver)
                {
                    gameManager.GameOver();
                }

                // Fall down
                StartCoroutine(CRPlayerFall(0.5f));
            }
        }
    }

    IEnumerator MovePlayer()
    {
        while (true)
        {
            if (gameManager.GameState == GameState.Playing)
            {
                isRunning = true;
                while (!gameManager.gameOver)
                {
                    transform.position += dir * movingSpeed * Time.deltaTime;                   
                    yield return null;
                }
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator CRPlayerFall(float delay)
    {
        // Fire event
        PlayerDie();

        yield return new WaitForSeconds(delay);

        SoundManager.Instance.PlaySound(SoundManager.Instance.gameOver);

        // Fall down
        Rigidbody rb = playerChild.GetComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.velocity = Vector3.down * 13f;
    }

    IEnumerator RotatePlayer(Vector3 dir, float rotateAngle)
    {
       
        float currentAngle = 0;
        while (currentAngle < Mathf.Abs(rotateAngle))
        {
            float rotateAmount = rotatingSpeed * Time.deltaTime;
            currentAngle += rotateAmount;
            transform.Rotate(dir, rotateAmount);
            yield return null;
        }

        if (dir == Vector3.down)
        {
            transform.eulerAngles = new Vector3(0, 270, 0);
        }
        else
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
        }

    }
}
