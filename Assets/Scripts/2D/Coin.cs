using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float MinX = -40f;
    public float MaxX = 40f;
    public float MinY = -20f;
    public float MaxY = 20f;
    [Header("Components")]
    public GameObject coin;

    // Variables
    private Vector2 pos;


    public void SpawnCoin()
    {
        pos = new Vector2(Random.Range(MinX, MaxX), Random.Range(MinY, MaxY));
        Instantiate(coin, pos, Quaternion.identity);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Player")
        {
            SpawnCoin();
            Destroy(this.gameObject);
        }
    }
}
