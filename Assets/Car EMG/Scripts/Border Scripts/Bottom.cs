using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bottom : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            other.gameObject.transform.position = new Vector3(other.gameObject.transform.position.x, 30.25f, 0f);
        }
    }
}
