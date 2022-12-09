using UnityEngine;

public class Character : MonoBehaviour
{
    public int characterSequenceNumber;
    public int price;
    public bool isFree = false;

    private string carID;
    public bool IsUnlocked
    {
        get
        {
            return (isFree || PlayerPrefs.GetInt(carID, 0) == 1);
        }
    }

    void Awake()
    {
        carID = name;
        carID = carID.ToUpper();
    }

    public bool Unlock()
    {
        if (IsUnlocked)
            return true;

        if (SgLib.CoinManager.Instance.Coins >= price)
        {
            PlayerPrefs.SetInt(carID, 1);
            PlayerPrefs.Save();
            SgLib.CoinManager.Instance.RemoveCoins(price);

            return true;
        }

        return false;
    }
}
