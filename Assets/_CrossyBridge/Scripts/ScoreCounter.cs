using UnityEngine;
using SgLib;

public class ScoreCounter : MonoBehaviour {

	void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            ScoreManager.Instance.AddScore(1);
        }
    }
}
