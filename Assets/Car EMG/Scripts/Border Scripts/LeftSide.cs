using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftSide : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.tag == "Player")
        {
            other.gameObject.transform.position = new Vector3(54.918f, other.gameObject.transform.position.y, 0f);
        }
    }
}
