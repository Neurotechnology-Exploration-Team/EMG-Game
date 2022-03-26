using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class WifiTesting : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UnityWebRequest.Get("http://192.168.4.1/all");
        // UnityWebRequest.Post("192.168.4.1", "Hello");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
