using UnityEngine;
using System.Collections;
using System.Collections.Generic;


[System.Serializable]
public enum WaterType
{
    Normal,
    Ice,
    Dark,
    Islands
}

public class Ocean : MonoBehaviour
{
    public int width = 32;
    public int height = 32;
    public int renderTexWidth = 128;
    public int renderTexHeight = 128;
    public float scale = 0.1f;
    public float speed = 0.7f;
    public float wakeDistance = 5f;
    public Vector3 size = new Vector3(150.0f, 1.0f, 150.0f);
    public int tiles = 2;
    public GameObject parentTile;
    public GameObject[] tileList;
    private float pWindx = 10.0f;

    public float windx
    {
        get { return pWindx; }
        set
        {
            if (value != pWindx)
            {
                this.pWindx = value;
                this.InitWaveGenerator();
            }
        }
    }

    private int pNormal_scale = 8;

    public int normal_scale
    {
        get { return pNormal_scale; }
        set
        {
            if (value != pNormal_scale)
            {
                pNormal_scale = value;
                this.InitWaveGenerator();
            }
        }
    }

    private float pNormalStrength = 2f;

    public float normalStrength
    {
        get { return pNormalStrength; }
        set
        {
            if (value != pNormalStrength)
            {
                pNormalStrength = value;
            }
        }
    }

    public float choppy_scale = 2.0f;

    public Material material;
    public bool followMainCamera = true;
    private int max_LOD = 4;
    private ComplexF[] h0;
    private ComplexF[] t_x;
    private ComplexF[] n0;
    private ComplexF[] n_x;
    private ComplexF[] n_y;
    private ComplexF[] data;
    private Color[] pixelData;
    private Texture2D textureA;
    private Texture2D textureB;
    private Vector3[] baseHeight;

    private Mesh baseMesh;
    private GameObject child;
    private List<List<Mesh>> tiles_LOD;
    private int g_height;
    private int g_width;
    private int n_width;
    private int n_height;
    private Vector2 sizeInv;

    private bool normalDone = false;
    private bool reflectionRefractionEnabled = false;
    private Camera offscreenCam = null;
    private RenderTexture reflectionTexture = null;
    private RenderTexture refractionTexture = null;

    private Vector3[] vertices;
    private Vector3[] normals;
    private Vector4[] tangents;
    public Transform player;
    public Transform sun;
    public Vector4 SunDir;

    public WaterType waterType = WaterType.Normal;
    public Color surfaceColor = new Color(0.3f, 0.5f, 0.3f, 1.0f);
    public Color iceSurfaceColor = new Color(0.3f, 0.5f, 0.3f, 1.0f);
    public Color darkSurfaceColor = new Color(0.3f, 0.5f, 0.3f, 1.0f);
    public Color islandsSurfaceColor = new Color(0.3f, 0.5f, 0.3f, 1.0f);
    public Color waterColor = new Color(0.3f, 0.4f, 0.3f);
    public Color iceWaterColor = new Color(0.3f, 0.4f, 0.3f);
    public Color darkWaterColor = new Color(0.3f, 0.4f, 0.3f);
    public Color islandsWaterColor = new Color(0.3f, 0.4f, 0.3f);
    public Shader oceanShader;
    public bool renderReflection = true;

    private float prevValue = 0;
    private float nextValue = 0;
    private float prevTime = -100000;
    private const float timeFreq = 1f / 120f;
    
    // 调用该接口改变海水的高度
    public void SetWaves(float x)
    {
        scale = x;
    }

    // 获取海水网格在某个点的海水高度
    public float GetWaterHeightAtLocation(float x, float y)
    {
        x = x / size.x;
        x = (x - Mathf.FloorToInt(x)) * width;
        y = y / size.z;
        y = (y - Mathf.FloorToInt(y)) * height;

        int index = (int) width * Mathf.FloorToInt(y) + Mathf.FloorToInt(x);
        return data[index].Re * scale / (width * height);
    }

    // 高斯公式计算
    float GaussianRnd()
    {
        float x1 = Random.value;
        float x2 = Random.value;

        if (x1 == 0.0f)
            x1 = 0.01f;

        return (float) (System.Math.Sqrt(-2.0 * System.Math.Log(x1)) * System.Math.Cos(2.0 * Mathf.PI * x2));
    }

    // Phillips spectrum （Phillips频谱公式）
    float P_spectrum(Vector2 vec_k, Vector2 wind)
    {
        // Set wind to blow only in one direction - otherwise we get turmoiling water
        float A = vec_k.x > 0.0f ? 1.0f : 0.05f;    // 设置风向只在一个方向 

        float L = wind.sqrMagnitude / 9.81f;
        float k2 = vec_k.sqrMagnitude;
        // Avoid division by zero
        if (vec_k.sqrMagnitude == 0.0f)
        {
            return 0.0f;
        }

        float vcsq = vec_k.magnitude;
        return (float) (A * System.Math.Exp(-1.0f / (k2 * L * L) - System.Math.Pow(vcsq * 0.1, 2.0)) / (k2 * k2) *
                        System.Math.Pow(Vector2.Dot(vec_k / vcsq, wind / wind.magnitude), 2.0)); // * wind_x * wind_y;
    }

    void Start()
    {
        // 标准化海水的大小
        n_width = 128;
        n_height = 128;
        
        // 避免每帧都去除，所以只做了一次启动
        sizeInv = new Vector2(1f / size.x, 1f / size.z);

        SetupOffscreenRendering();


        pixelData = new Color[n_width * n_height];
        
        // 初始化海水的高度矩阵
        data = new ComplexF[width * height];
        
        // 正切
        t_x = new ComplexF[width * height];

        n_x = new ComplexF[n_width * n_height];
        n_y = new ComplexF[n_width * n_height];

        // 几何大小
        g_height = height + 1;
        g_width = width + 1;

        tiles_LOD = new List<List<Mesh>>();

        for (int L0D = 0; L0D < max_LOD; L0D++)
        {
            tiles_LOD.Add(new List<Mesh>());
        }

        GameObject parentTile = new GameObject("ParentTile");
        GameObject tile;
        // 海水网格块数量
        //int chDist; // Chebychev distance	
        for (int y = 0; y < tiles; y++)
        {
            for (int x = 0; x < tiles; x++)
            {
                //chDist = System.Math.Max (System.Math.Abs (tiles_y / 2 - y), System.Math.Abs (tiles_x / 2 - x));
                //chDist = chDist > 0 ? chDist - 1 : 0;
                float cy = y - Mathf.Floor(tiles * 0.5f);
                float cx = x - Mathf.Floor(tiles * 0.5f);
                tile = new GameObject("WaterTile");
                Vector3 pos = tile.transform.position;
                pos.x = cx * size.x;
                pos.y = 0f;
                pos.z = cy * size.z;
                tile.transform.position = pos;
                tile.AddComponent(typeof(MeshFilter));
                tile.AddComponent<MeshRenderer>();
                tile.GetComponent<Renderer>().material = material;

                //Make child of this object, so we don't clutter up the
                //scene hierarchy more than necessary.
                tile.transform.parent = parentTile.transform;

                //我们也不想这些被吸收做折射或反射，所以我们将添加的水层过滤
                //Also we don't want these to be drawn while doing refraction/reflection passes,
                //so we'll add the to the water layer for easy filtering.
                tile.layer = LayerMask.NameToLayer("Water");

                //决定属于哪个海水块的LOD
                // Determine which L0D the tile belongs
                tiles_LOD[0].Add((tile.GetComponent<MeshFilter>()).mesh);
            }
        }
        
        // 初始化海浪的波普一个是顶点的偏移值，另一个是标准地图值
        // Init wave spectra. One for vertex offset and another for normal map
        h0 = new ComplexF[width * height];
        n0 = new ComplexF[n_width * n_height];

        InitWaveGenerator();
        UpdateWaterColor();
        GenerateHeightmap();
        windx = 20f;

        //如果关闭海水的反射和折射，那么把海水的LOD设置成1
        //Set ocean shader to lod 1 if reflection and refraction disabled
        if (!renderReflection)
            EnableReflection(false);
        else
            EnableReflection(true);

        parentTile.transform.parent = this.transform;
        parentTile.transform.localPosition = Vector3.zero;
        foreach (Transform g in parentTile.GetComponentsInChildren<Transform>())
            g.parent = this.transform;
        DestroyImmediate(parentTile);
    }

    //使用高斯公式和菲利普斯普算法实现海浪
    void InitWaveGenerator()
    {
        // 将风力局限于一个方向，减少计算
        // Wind restricted to one direction, reduces calculations
        Vector2 wind = new Vector2(windx, 0.0f);

        // 初始化海浪生成
        // Initialize wave generator	
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float yc = y < height / 2f ? y : -height + y;
                float xc = x < width / 2f ? x : -width + x;
                Vector2 vec_k = new Vector2(2.0f * Mathf.PI * xc / size.x, 2.0f * Mathf.PI * yc / size.z);
                h0[width * y + x] = new ComplexF(GaussianRnd(), GaussianRnd()) * 0.707f *
                                    (float) System.Math.Sqrt(P_spectrum(vec_k, wind));
            }
        }

        for (int y = 0; y < n_height; y++)
        {
            for (int x = 0; x < n_width; x++)
            {
                float yc = y < n_height / 2f ? y : -n_height + y;
                float xc = x < n_width / 2f ? x : -n_width + x;
                Vector2 vec_k = new Vector2(2.0f * Mathf.PI * xc / (size.x / normal_scale),
                    2.0f * Mathf.PI * yc / (size.z / normal_scale));
                n0[n_width * y + x] = new ComplexF(GaussianRnd(), GaussianRnd()) * 0.707f *
                                      (float) System.Math.Sqrt(P_spectrum(vec_k, wind));
            }
        }
    }

    void GenerateHeightmap()
    {
        Mesh mesh = new Mesh();

        int y = 0;
        int x = 0;

        // 建立顶点和UV坐标
        // Build vertices and UVs
        Vector3[] vertices = new Vector3[g_height * g_width];
        Vector4[] tangents = new Vector4[g_height * g_width];
        Vector2[] uv = new Vector2[g_height * g_width];

        Vector2 uvScale = new Vector2(1.0f / (g_width - 1f), 1.0f / (g_height - 1f));
        Vector3 sizeScale = new Vector3(size.x / (g_width - 1f), size.y, size.z / (g_height - 1f));

        for (y = 0; y < g_height; y++)
        {
            for (x = 0; x < g_width; x++)
            {
                Vector3 vertex = new Vector3(x, 0.0f, y);
                vertices[y * g_width + x] = Vector3.Scale(sizeScale, vertex);
                uv[y * g_width + x] = Vector2.Scale(new Vector2(x, y), uvScale);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;

        for (y = 0; y < g_height; y++)
        {
            for (x = 0; x < g_width; x++)
            {
                tangents[y * g_width + x] = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);
            }
        }

        mesh.tangents = tangents;

        for (int L0D = 0; L0D < max_LOD; L0D++)
        {
            Vector3[] verticesLOD = new Vector3[(int) (height / System.Math.Pow(2, L0D) + 1) *
                                                (int) (width / System.Math.Pow(2, L0D) + 1)];
            Vector2[] uvLOD = new Vector2[(int) (height / System.Math.Pow(2, L0D) + 1) *
                                          (int) (width / System.Math.Pow(2, L0D) + 1)];
            int idx = 0;

            for (y = 0; y < g_height; y += (int) System.Math.Pow(2, L0D))
            {
                for (x = 0; x < g_width; x += (int) System.Math.Pow(2, L0D))
                {
                    verticesLOD[idx] = vertices[g_width * y + x];
                    uvLOD[idx++] = uv[g_width * y + x];
                }
            }

            for (int k = 0; k < tiles_LOD[L0D].Count; k++)
            {
                Mesh meshLOD = tiles_LOD[L0D][k];
                meshLOD.vertices = verticesLOD;
                meshLOD.uv = uvLOD;
            }
        }

        // 构建三角形索引以及设置LOD
        // Build triangle indices: 3 indices into vertex array for each triangle
        for (int L0D = 0; L0D < max_LOD; L0D++)
        {
            int index = 0;
            int width_LOD = (int) (width / System.Math.Pow(2, L0D) + 1);
            int[] triangles = new int[(int) (height / System.Math.Pow(2, L0D) * width / System.Math.Pow(2, L0D)) * 6];
            for (y = 0; y < (int) (height / System.Math.Pow(2, L0D)); y++)
            {
                for (x = 0; x < (int) (width / System.Math.Pow(2, L0D)); x++)
                {
                    // 每个格子由两个三角形组成
                    // For each grid cell output two triangles
                    triangles[index++] = (y * width_LOD) + x;
                    triangles[index++] = ((y + 1) * width_LOD) + x;
                    triangles[index++] = (y * width_LOD) + x + 1;

                    triangles[index++] = ((y + 1) * width_LOD) + x;
                    triangles[index++] = ((y + 1) * width_LOD) + x + 1;
                    triangles[index++] = (y * width_LOD) + x + 1;
                }
            }

            for (int k = 0; k < tiles_LOD[L0D].Count; k++)
            {
                Mesh meshLOD = tiles_LOD[L0D][k];
                meshLOD.triangles = triangles;
            }
        }

        baseMesh = mesh;
    }
    
    //对于反射和折射的渲染会单独使用一个摄像机进行处理
    void SetupOffscreenRendering()
    {
        if (this.renderReflection)
        {
            reflectionTexture = new RenderTexture(renderTexWidth, renderTexHeight, 0);
            refractionTexture = new RenderTexture(renderTexWidth, renderTexHeight, 0);

            reflectionTexture.wrapMode = TextureWrapMode.Clamp;
            refractionTexture.wrapMode = TextureWrapMode.Clamp;

            reflectionTexture.isPowerOfTwo = true;
            refractionTexture.isPowerOfTwo = true;

            material.SetTexture("_Reflection", reflectionTexture);
            material.SetTexture("_Refraction", refractionTexture);
            material.SetVector("_Size", new Vector4(size.x, size.y, size.z, 0.0f));
        }

        //生成一个摄像机用于反射和折射的渲染
        //Spawn the camera we'll use for offscreen rendering (refraction/reflection)
        GameObject cam = new GameObject();
        cam.name = "DeepWaterOffscreenCam";
        cam.transform.parent = transform;

        offscreenCam = cam.AddComponent(typeof(Camera)) as Camera;
        //offscreenCam.gameObject.AddComponent<FogLayer>();
        offscreenCam.clearFlags = CameraClearFlags.Color;
        offscreenCam.depth = -1;
        offscreenCam.enabled = false;

        //生成海水网格包围盒
        //Hack to make this object considered by the renderer - first make a plane
        //covering the watertiles so we get a decent bounding box, then
        //scale all the vertices to 0 to make it invisible.
        gameObject.AddComponent(typeof(MeshRenderer));

        GetComponent<Renderer>().material.renderQueue = 1001;
        GetComponent<Renderer>().receiveShadows = false;
        GetComponent<Renderer>().castShadows = false;

        Mesh m = new Mesh();

        Vector3[] verts = new Vector3[4];
        Vector2[] uv = new Vector2[4];
        Vector3[] n = new Vector3[4];
        int[] tris = new int[6];

        float minSizeX = -1024;
        float maxSizeX = 1024;

        float minSizeY = -1024;
        float maxSizeY = 1024;

        verts[0] = new Vector3(minSizeX, 0.0f, maxSizeY);
        verts[1] = new Vector3(maxSizeX, 0.0f, maxSizeY);
        verts[2] = new Vector3(maxSizeX, 0.0f, minSizeY);
        verts[3] = new Vector3(minSizeX, 0.0f, minSizeY);

        tris[0] = 0;
        tris[1] = 1;
        tris[2] = 2;

        tris[3] = 2;
        tris[4] = 3;
        tris[5] = 0;

        m.vertices = verts;
        m.uv = uv;
        m.normals = n;
        m.triangles = tris;


        MeshFilter mfilter = gameObject.GetComponent<MeshFilter>();

        if (mfilter == null)
            mfilter = gameObject.AddComponent<MeshFilter>();

        mfilter.mesh = m;

        m.RecalculateBounds();

        //Hopefully the bounds will not be recalculated automatically
        verts[0] = Vector3.zero;
        verts[1] = Vector3.zero;
        verts[2] = Vector3.zero;
        verts[3] = Vector3.zero;

        m.vertices = verts;

        reflectionRefractionEnabled = true;
    }
    
    //当你要改变渲染纹理质量时，必须重现计算
    //Recalculate need to be called when you want to change render textures quality
    void RecalculateRenderTextures()
    {
        if (this.renderReflection)
        {
            reflectionTexture = new RenderTexture(renderTexWidth, renderTexHeight, 0);
            refractionTexture = new RenderTexture(renderTexWidth, renderTexHeight, 0);

            reflectionTexture.wrapMode = TextureWrapMode.Clamp;
            refractionTexture.wrapMode = TextureWrapMode.Clamp;

            reflectionTexture.isPowerOfTwo = true;
            refractionTexture.isPowerOfTwo = true;

            material.SetTexture("_Reflection", reflectionTexture);
            material.SetTexture("_Refraction", refractionTexture);
        }
    }

    //删除渲染纹理
    //Delete the offscreen rendertextures on script shutdown.
    void OnDisable()
    {
        if (reflectionTexture != null)
            DestroyImmediate(reflectionTexture);

        if (refractionTexture != null)
            DestroyImmediate(refractionTexture);

        reflectionTexture = null;
        refractionTexture = null;
    }

    void Update()
    {
        //如果玩家是null的，则根据Tag查找
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
        
        //得到太阳的反射方向
        if (sun != null)
        {
            SunDir = sun.transform.forward;
            material.SetVector("_SunDir", SunDir);
        }

        if (this.renderReflection)
            RenderObject();

        if (followMainCamera)
        {
            Vector3 centerOffset;

            //centerOffset.x = Mathf.Floor (player.position.x / size.x) * size.x;
            //centerOffset.z = Mathf.Floor (player.position.z / size.z) * size.z;
            centerOffset.y = transform.position.y;
            //centerOffset.x =  (((int)player.position.x + 64) & ~127);
            //centerOffset.z =  (((int)player.position.z + 64) & ~127);
            
            //优化海洋块的移动
            centerOffset.x = Mathf.Floor((player.position.x + size.x * 0.5f) * sizeInv.x) * size.x;
            centerOffset.z = Mathf.Floor((player.position.z + size.z * 0.5f) * sizeInv.y) * size.z;
            if (transform.position != centerOffset)
                transform.position = centerOffset;
        }


        float hhalf = height / 2f;
        float whalf = width / 2f;
        float time = Time.time;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int idx = width * y + x;
                float yc = y < hhalf ? y : -height + y;
                float xc = x < whalf ? x : -width + x;
                Vector2 vec_k = new Vector2(2.0f * Mathf.PI * xc / size.x, 2.0f * Mathf.PI * yc / size.z);

                float sqrtMagnitude = (float) System.Math.Sqrt((vec_k.x * vec_k.x) + (vec_k.y * vec_k.y));
                float iwkt = (float) System.Math.Sqrt(9.81f * sqrtMagnitude) * time * speed;
                ComplexF coeffA = new ComplexF((float) System.Math.Cos(iwkt), (float) System.Math.Sin(iwkt));
                ComplexF coeffB;
                coeffB.Re = coeffA.Re;
                coeffB.Im = -coeffA.Im;

                int ny = y > 0 ? height - y : 0;
                int nx = x > 0 ? width - x : 0;

                data[idx] = h0[idx] * coeffA + h0[width * ny + nx].GetConjugate() * coeffB;
                t_x[idx] = data[idx] * new ComplexF(0.0f, vec_k.x) - data[idx] * vec_k.y;
                
                //计算海浪滚动的波纹
                if (x + y > 0)
                    data[idx] += data[idx] * vec_k.x / sqrtMagnitude;
            }
        }

        Fourier.FFT2(data, width, height, FourierDirection.Backward);
        Fourier.FFT2(t_x, width, height, FourierDirection.Backward);
        
        //得到基础的顶点和UV坐标
        if (baseHeight == null)
        {
            baseHeight = baseMesh.vertices;
            vertices = new Vector3[baseHeight.Length];
            normals = new Vector3[baseHeight.Length];
            tangents = new Vector4[baseHeight.Length];
        }

        int wh = width * height;
        float scaleA = choppy_scale / wh;
        float scaleB = scale / wh;
        float scaleBinv = 1.0f / scaleB;

        for (int i = 0; i < wh; i++)
        {
            int iw = i + i / width;
            vertices[iw] = baseHeight[iw];
            vertices[iw].x += data[i].Im * scaleA;
            //vertices[iw].x = data[i].Im * scaleA;
            vertices[iw].y = data[i].Re * scaleB;

            normals[iw] = Vector3.Normalize(new Vector3(t_x[i].Re, scaleBinv, t_x[i].Im));

            if (((i + 1) % width) == 0)
            {
                int iwi = iw + 1;
                int iwidth = i + 1 - width;
                vertices[iwi] = baseHeight[iwi];
                vertices[iwi].x += data[iwidth].Im * scaleA;
                vertices[iwi].y = data[iwidth].Re * scaleB;

                normals[iwi] = Vector3.Normalize(new Vector3(t_x[iwidth].Re, scaleBinv, t_x[iwidth].Im));
            }
        }

        int offset = g_width * (g_height - 1);

        for (int i = 0; i < g_width; i++)
        {
            int io = i + offset;
            int mod = i % width;
            vertices[io] = baseHeight[io];
            vertices[io].x += data[mod].Im * scaleA;
            vertices[io].y = data[mod].Re * scaleB;

            normals[io] = Vector3.Normalize(new Vector3(t_x[mod].Re, scaleBinv, t_x[mod].Im));
        }

        int gwgh = g_width * g_height - 1;
        for (int i = 0; i < gwgh; i++)
        {
            //需要保留反射和折射
            if (!reflectionRefractionEnabled)
            {
                if (((i + 1) % g_width) == 0)
                {
                    tangents[i] =
                        Vector3.Normalize((vertices[i - width + 1] + new Vector3(size.x, 0.0f, 0.0f) - vertices[i]));
                }
                else
                {
                    tangents[i] = Vector3.Normalize((vertices[i + 1] - vertices[i]));
                }

                tangents[i].w = 1.0f;
            }
            else
            {
                Vector3 tmp; // = Vector3.zero;

                if (((i + 1) % g_width) == 0)
                {
                    tmp = Vector3.Normalize(vertices[i - width + 1] + new Vector3(size.x, 0.0f, 0.0f) - vertices[i]);
                }
                else
                {
                    tmp = Vector3.Normalize(vertices[i + 1] - vertices[i]);
                }

                tangents[i] = new Vector4(tmp.x, tmp.y, tmp.z, tangents[i].w);
            }
        }


        //Vector3 playerRelPos =  player.position - transform.position;
        
        //在反射模式中，使用正切系数w控制海浪泡沫的强度
        if (reflectionRefractionEnabled)
        {
            for (int y = 0; y < g_height; y++)
            {
                for (int x = 0; x < g_width; x++)
                {
                    int item = x + g_width * y;
                    if (x + 1 >= g_width)
                    {
                        tangents[item].w = tangents[g_width * y].w;

                        continue;
                    }

                    if (y + 1 >= g_height)
                    {
                        tangents[item].w = tangents[x].w;

                        continue;
                    }

                    float right = vertices[(x + 1) + g_width * y].x - vertices[item].x;

                    float foam = right / (size.x / g_width);


                    if (foam < 0.0f)
                        tangents[item].w = 1f;
                    else if (foam < 0.5f)
                        tangents[item].w += 3.0f * Time.deltaTime;
                    else
                        tangents[item].w -= 0.4f * Time.deltaTime;

                    if (player != null)
                    {
                        Vector3 player2Vertex = (player.position - vertices[item] - transform.position);

                        //围绕在船周围的泡沫
                        if (player2Vertex.x >= size.x)
                            player2Vertex.x -= size.x;

                        if (player2Vertex.x <= -size.x)
                            player2Vertex.x += size.x;

                        if (player2Vertex.z >= size.z)
                            player2Vertex.z -= size.z;

                        if (player2Vertex.z <= -size.z)
                            player2Vertex.z += size.z;
                        player2Vertex.y = 0;

                        if (player2Vertex.sqrMagnitude < wakeDistance * wakeDistance)
                            tangents[item].w += 3.0f * Time.deltaTime;
                    }


                    tangents[item].w = Mathf.Clamp(tangents[item].w, 0.0f, 2.0f);
                }
            }
        }

        tangents[gwgh] = Vector4.Normalize(vertices[gwgh] + new Vector3(size.x, 0.0f, 0.0f) - vertices[1]);

        for (int L0D = 0; L0D < max_LOD; L0D++)
        {
            int den = (int) System.Math.Pow(2f, L0D);
            int itemcount = (int) ((height / den + 1) * (width / den + 1));

            Vector4[] tangentsLOD = new Vector4[itemcount];
            Vector3[] verticesLOD = new Vector3[itemcount];
            Vector3[] normalsLOD = new Vector3[itemcount];

            int idx = 0;

            for (int y = 0; y < g_height; y += den)
            {
                for (int x = 0; x < g_width; x += den)
                {
                    int idx2 = g_width * y + x;
                    verticesLOD[idx] = vertices[idx2];
                    tangentsLOD[idx] = tangents[idx2];
                    normalsLOD[idx++] = normals[idx2];
                }
            }

            for (int k = 0; k < tiles_LOD[L0D].Count; k++)
            {
                Mesh meshLOD = tiles_LOD[L0D][k];
                meshLOD.vertices = verticesLOD;
                meshLOD.normals = normalsLOD;
                meshLOD.tangents = tangentsLOD;
            }
        }
    }
    
    //对象的渲染需要在每帧中做一次
    void RenderObject()
    {
        if (Camera.current == offscreenCam)
            return;

        if (reflectionTexture == null || refractionTexture == null)
            return;

        if (this.renderReflection)
            RenderReflectionAndRefraction();
    }

/*
 反射和折射的渲染buffer是复制当前摄像机的设置到另一台摄像技上
Renders the reflection and refraction buffers using a second camera copying the current
camera settings.
*/
    public LayerMask renderLayers = -1;

    void RenderReflectionAndRefraction()
    {
        Camera renderCamera = Camera.main;

        Matrix4x4 originalWorldToCam = renderCamera.worldToCameraMatrix;

        int cullingMask = ~(1 << 4) & renderLayers.value;
        ;

        //Reflection pass 反射通道
        Matrix4x4 reflection = Matrix4x4.zero;

        //TODO: Use local plane here, not global!

        float d = -transform.position.y;
        offscreenCam.backgroundColor = RenderSettings.fogColor;

        CameraHelper.CalculateReflectionMatrix(ref reflection, new Vector4(0f, 1f, 0f, d));

        offscreenCam.transform.position = reflection.MultiplyPoint(renderCamera.transform.position);
        offscreenCam.transform.rotation = renderCamera.transform.rotation;
        offscreenCam.worldToCameraMatrix = originalWorldToCam * reflection;

        //折射通道
        offscreenCam.cullingMask = cullingMask;
        offscreenCam.targetTexture = reflectionTexture;

        //Need to reverse face culling for reflection pass, since the camera
        //is now flipped upside/down.
        GL.SetRevertBackfacing(true);

        Vector4 cameraSpaceClipPlane = CameraHelper.CameraSpacePlane(offscreenCam,
            new Vector3(0.0f, transform.position.y, 0.0f), Vector3.up, 1.0f);

        Matrix4x4 projection = renderCamera.projectionMatrix;
        Matrix4x4 obliqueProjection = projection;

        offscreenCam.fieldOfView = renderCamera.fieldOfView;
        offscreenCam.aspect = renderCamera.aspect;

        CameraHelper.CalculateObliqueMatrix(ref obliqueProjection, cameraSpaceClipPlane);

        //Do the actual render, with the near plane set as the clipping plane. See the
        //pro water source for details.
        offscreenCam.projectionMatrix = obliqueProjection;

        if (!renderReflection)
            offscreenCam.cullingMask = 0;

        offscreenCam.Render();


        GL.SetRevertBackfacing(false);

        //Refractionpass
        //TODO: If we want to use this as a refraction seen from under the seaplane,
        //      the cameraclear should be skybox.

        offscreenCam.cullingMask = cullingMask;
        offscreenCam.targetTexture = refractionTexture;
        obliqueProjection = projection;

        offscreenCam.transform.position = renderCamera.transform.position;
        offscreenCam.transform.rotation = renderCamera.transform.rotation;
        offscreenCam.worldToCameraMatrix = originalWorldToCam;


        cameraSpaceClipPlane = CameraHelper.CameraSpacePlane(offscreenCam, Vector3.zero, Vector3.up, -1.0f);
        CameraHelper.CalculateObliqueMatrix(ref obliqueProjection, cameraSpaceClipPlane);
        offscreenCam.projectionMatrix = obliqueProjection;

        //if (!renderRefraction)
        //offscreenCam.cullingMask = 0;

        offscreenCam.Render();

        offscreenCam.projectionMatrix = projection;


        offscreenCam.targetTexture = null;
    }

    //设置改变反射纹理品质
    //Settings for changing reflection textures quality (can be used with NGUI)
    void ReflectionQuality(string quality)
    {
        OnDisable();
        if (quality == "Low")
        {
            renderTexWidth = 128;
            renderTexHeight = 128;
        }
        else
        {
            renderTexWidth = 512;
            renderTexHeight = 512;
        }

        RecalculateRenderTextures();
    }

    //使用反射
    //Toggle reflections (can be used with NGUI)
    void EnableReflection(bool isActive)
    {
        renderReflection = isActive;
        if (!isActive)
        {
            material.SetTexture("_Reflection", null);
            material.SetTexture("_Refraction", null);
            oceanShader.maximumLOD = 1;
        }
        else
        {
            OnDisable();
            oceanShader.maximumLOD = 2;
            RecalculateRenderTextures();
        }
    }

    //不需要每帧都改变海水的颜色，只设置一次就可以了
    //No need to update water color every frame, call this only when need to change color
    public void UpdateWaterColor()
    {
        if (waterType == WaterType.Normal)
        {
            material.SetColor("_WaterColor", waterColor);
            material.SetColor("_SurfaceColor", surfaceColor);
        }
        else if (waterType == WaterType.Ice)
        {
            material.SetColor("_WaterColor", iceWaterColor);
            material.SetColor("_SurfaceColor", iceSurfaceColor);
        }
        else if (waterType == WaterType.Dark)
        {
            material.SetColor("_WaterColor", darkWaterColor);
            material.SetColor("_SurfaceColor", darkSurfaceColor);
        }
        else if (waterType == WaterType.Islands)
        {
            material.SetColor("_WaterColor", islandsWaterColor);
            material.SetColor("_SurfaceColor", islandsSurfaceColor);
        }
    }
}