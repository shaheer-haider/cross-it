using UnityEngine;
using System.Collections;
using SgLib;

public class GoldController : MonoBehaviour {

    // Use this for initialization

    private bool stopBounce;
	void Start () {

        StartCoroutine(Rotate());
        StartCoroutine(Bounce());
    }


    IEnumerator Rotate()
    {
        while (true)
        {
            transform.Rotate(Vector3.down * 5f);
            yield return null;
        }
    }


    IEnumerator Bounce()
    {
        while (true)
        {
            Vector3 startPos = transform.position;
            Vector3 endPos = startPos + new Vector3(0, 1f, 0);
            float t = 0;
            while (t < 0.5f)
            {
                if (stopBounce)
                    yield break;
                t += Time.deltaTime;
                float fraction = t / 0.5f;
                transform.position = Vector3.Lerp(startPos, endPos, fraction);
                yield return null;
            }

            float r = 0;
            while (r < 0.5f)
            {
                if (stopBounce)
                    yield break;
                r += Time.deltaTime;
                float fraction = r / 0.5f;
                transform.position = Vector3.Lerp(endPos, startPos, fraction);
                yield return null;
            }
        }

    }


    IEnumerator GoUp()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + new Vector3(0, 15f, 0);
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            float fraction = t / 1f;
            transform.position = Vector3.Lerp(startPos, endPos, fraction);
            yield return null;
        }
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.coin);
            CoinManager.Instance.AddCoins(1);
            stopBounce = true;
            StartCoroutine(GoUp());
        }
    }
}
