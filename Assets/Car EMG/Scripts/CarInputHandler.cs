using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarInputHandler : MonoBehaviour
{
    [Header("Input Settings")] 
    public bool useBCIInput = true;
    
    // Components
    TopDownCarController topDownCarController;
    // CarBCIReader carBCIReader;
    public MonoBehaviour bciReaderIObject;
    private OpenBCIReaderI bciReaderI;

    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        topDownCarController = GetComponent<TopDownCarController>();
        // carBCIReader = GetComponent<CarBCIReader>();

        bciReaderI = bciReaderIObject.GetComponent<OpenBCIReaderI>();
        bciReaderI.Reconnect();
        bciReaderI.SetThreshold(0, 1);
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 inputVector = Vector2.zero;

        if (useBCIInput)
        {
            inputVector.x = bciReaderI.GetInput(0) ? 1 : 0;
            // inputVector.y = carBCIReader.GetAxis("Vertical");
        }
        else
        {
            inputVector.x = Input.GetAxis("Horizontal");
            inputVector.y = Input.GetAxis("Vertical");
        }

        topDownCarController.SetVector(inputVector);
    }
}
