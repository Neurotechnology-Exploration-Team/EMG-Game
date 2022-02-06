using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManganement : MonoBehaviour
{
    public Player playerScript;
    public GameObject[] guardArray;
    public AudioClip goatClip;
    private AudioSource audioSource;
    private float audioTimer = 0;
    // Start is called before the first frame update
    void Start()
    {
        guardArray = GameObject.FindGameObjectsWithTag("Guard");
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (playerScript.hasGoat)
        {
            foreach(GameObject guard in guardArray)
            {
                guard.GetComponent<GuardScript>().visionRadius += 1;
            }

            if(audioTimer <= 0)
            {
                audioSource.Play();
                audioTimer = 5;
            }

            audioTimer -= Time.deltaTime;
            //playAudio();
            
        }

        if(playerScript.flashLight.enabled == true)
        {
            foreach(GameObject guard in guardArray)
            {
                float distance = Mathf.Pow((playerScript.gameObject.transform.position.x - guard.transform.position.x), 2) + Mathf.Pow((playerScript.gameObject.transform.position.y - guard.transform.position.y), 2);
                distance = Mathf.Sqrt(distance);
                if (distance < 50)
                {
                    guard.GetComponent<GuardScript>().canMove = false;
                }
            }
        }
    }

    private IEnumerator playAudio()
    {
        audioSource.Play();
        yield return new WaitForSeconds(audioSource.clip.length);
}
}
