using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    [SerializeField]
    private GameObject coinPrefab;

    [SerializeField]
    private Text coinText;

    private int collectedCoins;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<GameManager>();
            }
            return instance;
        }
    }

    public GameObject CoinPrefab
    {
        get
        {
            return coinPrefab;
        }
    }

    public int CollectedCoins
    {
        get
        {
            return collectedCoins;
        }

        set
        {
            coinText.text = value.ToString();
            this.collectedCoins = value;
        }
    }
}
