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

    // Awake is called when the script instance is being loaded.
    void Awake()
    {
        topDownCarController = GetComponent<TopDownCarController>();
        // carBCIReader = GetComponent<CarBCIReader>();
    }
    // Update is called once per frame
    void Update()
    {
        Vector2 inputVector = Vector2.zero;

        if (useBCIInput)
        {
            // inputVector.x = carBCIReader.GetAxis("Horizontal");
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
