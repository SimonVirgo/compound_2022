using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdaptSizeToBounds : MonoBehaviour
{
    public GameObject AttachTo;
    public float radiusOffset;
    
    private ComputedExtend compExtent;
    private Vector3 transformOffset; 
    void Start()
    {
        if (AttachTo != null && AttachTo.GetComponent<ComputedExtend>() != null)
        {
            compExtent = AttachTo.GetComponent<ComputedExtend>(); //Link to the Component in attachTo Object that hosts the parameters for midpoint position and extent
            transformOffset = gameObject.GetComponent<Transform>().position; //save initial position of the Object to calculate the relative movement
        }
        else
        {
            Debug.Log("Reference GameObject is not Set or is missing a Computed Extent script");
        }
    }

    void Update()
    {
        updateEqualSides();
    }

    void updateEqualSides()
    {
        gameObject.transform.position = compExtent.midpoint;  // + transformOffset;
        var size = compExtent.width + (2 * radiusOffset);
        gameObject.transform.localScale = new Vector3(size, 0, size);
    }
}
