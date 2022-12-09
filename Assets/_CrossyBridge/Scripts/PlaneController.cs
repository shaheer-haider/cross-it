using UnityEngine;
using System.Collections;

public class PlaneController : MonoBehaviour
{


    public bool isTheLastPlane;
    public bool isMove = false;
    public bool isTheTopXAxis;
    public bool isTheTopZAxis;
    public bool movingByXAxis;
    public bool isVisible = false;
    public bool stopMoving = false;
    public float planeMovingSpeed;
    public int movingAmplitude;

    PlayerController playerController;

    bool isDestroying = false;

    // Use this for initialization
    void Start()
    {
        playerController = GameObject.FindObjectOfType<PlayerController>();

        if (isMove)
        {
            StartCoroutine(MovePlane());
        }
    }

    void Update()
    {
        if (GameManager.Instance.turn == 1)
        {
            if (playerController.gameObject.transform.position.x + 10f < (gameObject.transform.position.x) && !isDestroying)
            {
                isDestroying = true;
                Destroy(gameObject);
            }
        }
        else
        {
            // forward direction
            if (playerController.gameObject.transform.position.z > (gameObject.transform.position.z + 10f) && !isDestroying)
            {
                isDestroying = true;
                Destroy(gameObject);
            }
        }
    }

    void OnBecameVisible()
    {
        isVisible = true;
    }

    IEnumerator MovePlane()
    {
        if (movingByXAxis)
        {
            while (!stopMoving)
            {
                Vector3 startPos = transform.position;
                Vector3 endPos;
                if (isTheTopXAxis)
                {
                    endPos = transform.position + new Vector3(-(movingAmplitude * 2), 0, 0);
                }
                else
                {
                    endPos = transform.position + new Vector3((movingAmplitude * 2), 0, 0);
                }
                float t = 0;
                while (t < planeMovingSpeed && !stopMoving)
                {
                    t += Time.deltaTime;
                    float fraction = t / planeMovingSpeed;
                    transform.position = Vector3.Lerp(startPos, endPos, fraction);
                    yield return null;
                }

                isTheTopXAxis = !isTheTopXAxis;
            }
        }
        else
        {
            while (!stopMoving)
            {
                Vector3 startPos = transform.position;
                Vector3 endPos;
                if (isTheTopZAxis)
                {
                    endPos = transform.position + new Vector3(0, 0, -(movingAmplitude * 2));
                }
                else
                {
                    endPos = transform.position + new Vector3(0, 0, (movingAmplitude * 2));
                }
                float t = 0;
                while (t < planeMovingSpeed && !stopMoving)
                {
                    t += Time.deltaTime;
                    float fraction = t / planeMovingSpeed;
                    transform.position = Vector3.Lerp(startPos, endPos, fraction);
                    yield return null;
                }

                isTheTopZAxis = !isTheTopZAxis;
            }
        }
    }

}
