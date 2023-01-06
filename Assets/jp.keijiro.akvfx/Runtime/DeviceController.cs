using Microsoft.Azure.Kinect.Sensor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Akvfx {

public sealed class DeviceController : MonoBehaviour
{
    //array of planes to check vectors against
    public GameObject[] planes;
    //array of planes in local space
    private Plane[] localPlanes;
    //stuff for the mesh generation
    public bool createMesh;
    private Short3[] PointCloud;
    //Width and Height of Depth image.
    int depthWidth;
    int depthHeight;
    //Number of all points of PointCloud 
    int num;
    //Used to draw a set of points
    Mesh mesh;
    //Array of coordinates for each point in PointCloud
    Vector3[] vertices;
    //Array of colors corresponding to each point in PointCloud
    Color32[] colors;
    //List of indexes of points to be rendered
    int[] indeces;
    //Color image to be attatched to mesh
    Texture2D texture;
    //Class for coordinate transformation(e.g.Color-to-depth, depth-to-xyz, etc.)
    Transformation transformation;

    int nearClip = 300;
    
    #region Editable attribute

    [SerializeField] DeviceSettings _deviceSettings = null;

    [SerializeField] static int _deviceIndex;
    
    public DeviceSettings DeviceSettings
      { get => _deviceSettings; set => SetDeviceSettings(value); }
    
    [SerializeField] public int deviceIndex;
    

        #endregion

    #region Asset reference

    [SerializeField] ComputeShader _compute = null;

    #endregion

    #region Public accessor properties

    public RenderTexture ColorMap => _colorMap;
    public RenderTexture PositionMap => _positionMap;

    public Vector3[] Positions;
    

    #endregion

    #region Private members

    ThreadedDriver _driver;
    ComputeBuffer _xyTable;
    ComputeBuffer _colorBuffer;
    ComputeBuffer _depthBuffer;
    RenderTexture _colorMap;
    RenderTexture _positionMap;
    ComputeBuffer _pointCloudBuffer;

    void SetDeviceSettings(DeviceSettings settings)
    {
        _deviceSettings = settings;
        if (_driver != null) _driver.Settings = settings;
    }

    #endregion

    #region Shader property IDs

    static class ID
    {
        public static int ColorBuffer = Shader.PropertyToID("ColorBuffer");
        public static int DepthBuffer = Shader.PropertyToID("DepthBuffer");
        public static int XYTable     = Shader.PropertyToID("XYTable");
        public static int MaxDepth    = Shader.PropertyToID("MaxDepth");
        public static int ColorMap    = Shader.PropertyToID("ColorMap");
        public static int PositionMap = Shader.PropertyToID("PositionMap");
       // public static int PointCloud = Shader.PropertyToID("PointCloud");
    }

    #endregion

    #region MonoBehaviour implementation

    void Start()
    {
        //SetupMesh
        ConvertPlanesToLocalSpace();
        InitMesh();
        // Start capturing via the threaded driver.
        _driver = new ThreadedDriver(_deviceSettings, deviceIndex);

        // Temporary objects for conversion
        var width = ThreadedDriver.ImageWidth;
        var height = ThreadedDriver.ImageHeight;
        
        

        _colorBuffer = new ComputeBuffer(width * height, 4);
        _depthBuffer = new ComputeBuffer(width * height / 2, 4);
        // Create the point cloud buffer
        //_pointCloudBuffer = new ComputeBuffer(width*height, sizeof(float) * 3, ComputeBufferType.Default);
        //Positions = new Vector3[width * height];
        

        _colorMap = new RenderTexture
          (width, height, 0, RenderTextureFormat.Default);
        _colorMap.enableRandomWrite = true;
        _colorMap.Create();

        _positionMap = new RenderTexture
          (width, height, 0, RenderTextureFormat.ARGBFloat);
        _positionMap.enableRandomWrite = true;
        _positionMap.Create();
    }

    void OnDestroy()
    {
        if (_colorMap    != null) Destroy(_colorMap);
        if (_positionMap != null) Destroy(_positionMap);

        _colorBuffer?.Dispose();
        _depthBuffer?.Dispose();

        _xyTable?.Dispose();
        _driver?.Dispose();
    }

    unsafe void Update()
    {
        // Try initializing XY table if it's not ready.
        if (_xyTable == null)
        {
            var data = _driver.XYTable;
            if (data.IsEmpty) return; // Table is not ready.

            // Allocate and initialize the XY table.
            _xyTable = new ComputeBuffer(data.Length, sizeof(float));
            _xyTable.SetData(data);
        }

        // Try retrieving the last frame.
        var (color, depth) = _driver.LockLastFrame();
        if (color.IsEmpty || depth.IsEmpty)
        {
            return;
        }
        
        // This part is only triggered when a new frame is available! call an event here!

        // Load the frame data into the compute buffers.
        _colorBuffer.SetData(color.Span);
        _depthBuffer.SetData(depth.Span);

        // We don't need the last frame any more.
        _driver.ReleaseLastFrame();

        // Invoke the unprojection compute shader.
        _compute.SetFloat(ID.MaxDepth, _deviceSettings.maxDepth);
        _compute.SetBuffer(0, ID.ColorBuffer, _colorBuffer);
        _compute.SetBuffer(0, ID.DepthBuffer, _depthBuffer);
        _compute.SetBuffer(0, ID.XYTable, _xyTable);
        _compute.SetTexture(0, ID.ColorMap, _colorMap);
        _compute.SetTexture(0, ID.PositionMap, _positionMap);
        //_compute.SetBuffer(0,ID.PointCloud,_pointCloudBuffer);
        _compute.Dispatch(0, _colorMap.width / 8, _colorMap.height / 8, 1);
        
        // Read the point cloud buffer and store it in a Vector3 array
        //_pointCloudBuffer.GetData(Positions);
        
        PointCloud = _driver.cloudImage.GetPixels<Short3>().ToArray();
        
        if (createMesh)
        {
            CreateMesh();
        }
    }

    void CreateMesh()
    {
                        int triangleIndex = 0;
                int pointIndex = 0;
                int topLeft, topRight, bottomLeft, bottomRight;
                int tl, tr, bl, br;
                for (int y = 0; y < depthHeight; y++)
                {
                    
                    for (int x = 0; x < depthWidth; x++)
                    {

                        vertices[pointIndex].x = PointCloud[pointIndex].X * 0.001f;
                        vertices[pointIndex].y = -PointCloud[pointIndex].Y * 0.001f;
                        vertices[pointIndex].z = PointCloud[pointIndex].Z * 0.001f;
                        
                        
                        //filter out all vertices that have their world space location outside of the filter box
                        //check for all planes if the point is in front of it
                        
                        if (IsInsidePlanes(vertices[pointIndex]))
                        {
                            //colors[pointIndex].a = 255;
                            //colors[pointIndex].b = colorArray[pointIndex].B;
                            //colors[pointIndex].g = colorArray[pointIndex].G;
                            //colors[pointIndex].r = colorArray[pointIndex].R;

                            if (x != (depthWidth - 1) && y != (depthHeight - 1))
                            {
                                topLeft = pointIndex;
                                topRight = topLeft + 1;
                                bottomLeft = topLeft + depthWidth;
                                bottomRight = bottomLeft + 1;
                                tl = PointCloud[topLeft].Z;
                                tr = PointCloud[topRight].Z;
                                bl = PointCloud[bottomLeft].Z;
                                br = PointCloud[bottomRight].Z;

                                if (tl > nearClip && tr > nearClip && bl > nearClip)
                                {
                                    indeces[triangleIndex++] = topLeft;
                                    indeces[triangleIndex++] = topRight;
                                    indeces[triangleIndex++] = bottomLeft;
                                }
                                else
                                {
                                    indeces[triangleIndex++] = 0;
                                    indeces[triangleIndex++] = 0;
                                    indeces[triangleIndex++] = 0;
                                }

                                if (bl > nearClip && tr > nearClip && br > nearClip)
                                {
                                    indeces[triangleIndex++] = bottomLeft;
                                    indeces[triangleIndex++] = topRight;
                                    indeces[triangleIndex++] = bottomRight;
                                }
                                else
                                {
                                    indeces[triangleIndex++] = 0;
                                    indeces[triangleIndex++] = 0;
                                    indeces[triangleIndex++] = 0;
                                }
                            }
                        }
                        
                        else
                        {
                            if (pointIndex > 0)
                            {
                                vertices[pointIndex].x = vertices[pointIndex-1].x;
                                vertices[pointIndex].y = vertices[pointIndex-1].y;
                                vertices[pointIndex].z = vertices[pointIndex-1].z;
                            }

                            indeces[triangleIndex++] = 0;
                            indeces[triangleIndex++] = 0;
                            indeces[triangleIndex++] = 0;
                        }

                        pointIndex++;
                    }
                }

                //texture.SetPixels32(colors);
                //texture.Apply();

                mesh.vertices = vertices;

                mesh.triangles = indeces;
    }
    void InitMesh()
    {
        //Get the width and height of the Depth image and calculate the number of all points
        depthWidth = 640/2; //this is for 2x2 binned!
        depthHeight = 576/2;
        num = depthWidth * depthHeight;
        Debug.Log(num);

        //Instantiate mesh
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        

        //Allocation of vertex and color storage space for the total number of pixels in the depth image
        vertices = new Vector3[num];
        colors = new Color32[num];
        texture = new Texture2D(depthWidth, depthHeight);
        Vector2[] uv = new Vector2[num];
        Vector3[] normals = new Vector3[num];
        indeces = new int[6 * (depthWidth - 1) * (depthHeight - 1)];

        //Initialization of uv and normal 
        int index = 0;
        for (int y = 0; y < depthHeight; y++)
        {
            for (int x = 0; x < depthWidth; x++)
            {
                uv[index] = new Vector2(((float)(x + 0.5f) / (float)(depthWidth)), ((float)(y + 0.5f) / ((float)(depthHeight))));
                normals[index] = new Vector3(0, -1, 0);
                index++;
            }
        }

        //Allocate a list of point coordinates, colors, and points to be drawn to mesh
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.normals = normals;

        gameObject.GetComponent<MeshRenderer>().materials[0].mainTexture = texture;
        gameObject.GetComponent<MeshFilter>().mesh = mesh;
    }
    private void ConvertPlanesToLocalSpace()
    {
        if (planes==null) return;
        
        localPlanes = new Plane[planes.Length];
        for (int i = 0; i < planes.Length; i++)
        {
            localPlanes[i] = new Plane(transform.InverseTransformDirection(planes[i].transform.up), transform.InverseTransformPoint(planes[i].transform.position));
        }
    }
    
    //Method that accepts a Vector3 and returns true if it is inside the planes
    private bool IsInsidePlanes(Vector3 point)
    {
        if (planes==null) return true;
        for (int i = 0; i < localPlanes.Length; i++)
        {
            if (localPlanes[i].GetSide(point))
            {
                return false;
            }
        }
        return true;
    }
    #endregion
}

}
