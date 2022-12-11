using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComputedExtend : MonoBehaviour
{
    public Vector3 midpoint; //midpoint of renderer bounds in world space

    public float height; 
    public float width;
    public float depth;
    public float radius;//radius around the midpoint that fully encloses the object

    //private MeshRenderer rend;
    private Renderer rend;
    // Start is called before the first frame update
    void Start()
    {
        rend = gameObject.GetComponent<Renderer>();
        if (rend == null) {
            Debug.Log("No Renderer found");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (rend != null) {
            midpoint = rend.bounds.center;
            height = rend.bounds.size.y;
            width = rend.bounds.size.x;
            depth = rend.bounds.size.z;
            radius = rend.bounds.extents.magnitude;
        }
    }
}
