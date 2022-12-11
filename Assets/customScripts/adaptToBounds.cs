using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class adaptToBounds : MonoBehaviour
{
    public GameObject AttachTo;
    public GameObject AttachToVfx;
    public string VfxParameter;
    public float radiusOffset;
    public float smoothingTime;
    public float RimWidth; 
    private ComputedExtend compExtent;
    public bool AdaptScale = true;
    private Rigidbody Rigid;
    public bool allowOnlyShrink;
    public float minimumSize;

    private Vector3 transformOffset;
    private Vector3 previousTransform; //stores the transform of the previous frame for smoothing
    private Vector3 previousScale;  //stores the scale of the previous frame for smoothing
    private float previousMaxSize;
    private Vector3 transformVelocity = Vector3.zero;
    private Vector3 scaleVelocity = Vector3.zero;
    private UnityEngine.VFX.VisualEffect VFX;
    void Start()
    {
        Rigid = gameObject.GetComponent<Rigidbody>();
        if (AttachTo != null && AttachTo.GetComponent<ComputedExtend>() != null)
        {
            compExtent = AttachTo.GetComponent<ComputedExtend>(); //Link to the Component in attachTo Object that hosts the parameters for midpoint position and extent
            transformOffset = gameObject.GetComponent<Transform>().position; //save initial position of the Object to calculate the relative movement
            previousTransform = gameObject.transform.position;
            previousScale = gameObject.transform.localScale;
        }
        else
        {
            Debug.Log("Reference GameObject is not Set or is missing a Computed Extent script");
        }

        if (AttachToVfx != null && AttachToVfx.GetComponent<UnityEngine.VFX.VisualEffect>() != null )
        {
            VFX = AttachToVfx.GetComponent<UnityEngine.VFX.VisualEffect>();
        }
    }

    void FixedUpdate()
    {
        updateEqualSides();
    }

    void updateEqualSides()
    {

        var maxsize = compExtent.width;
        if (compExtent.depth > compExtent.width)//check if depth is larger than width
        {
            maxsize = compExtent.depth;
        }
        if (maxsize>0) //check if Volume is not empty
        {
            if (!Rigid) //if there is a rigidbody , move this instead of using transform directly!
            {
                gameObject.transform.position = Vector3.SmoothDamp(previousTransform, new Vector3(compExtent.midpoint.x, gameObject.transform.position.y, compExtent.midpoint.z), ref transformVelocity, smoothingTime);  // + transformOffset;
            }
            else
            {
                Rigid.MovePosition(Vector3.SmoothDamp(previousTransform, new Vector3(compExtent.midpoint.x, gameObject.transform.position.y, compExtent.midpoint.z), ref transformVelocity, smoothingTime));  
            }
        }

        if (AdaptScale && maxsize >= minimumSize ) //only adapt size when it is not smaller than minimumsize
        {
            gameObject.transform.localScale = Vector3.SmoothDamp(previousScale, new Vector3(maxsize + RimWidth, gameObject.transform.localScale.y, maxsize + RimWidth), ref scaleVelocity, smoothingTime);
        }

        if (AttachToVfx != null && VFX != null && VfxParameter != null)
        {
            var radius = gameObject.transform.localScale[0]/2.0f;
            VFX.SetFloat(VfxParameter, radius);
        }


        previousTransform = gameObject.transform.position;
        previousScale = gameObject.transform.localScale;
        previousMaxSize = maxsize;

    }
}

