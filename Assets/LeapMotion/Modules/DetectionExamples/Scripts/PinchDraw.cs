/******************************************************************************
 * Copyright (C) Leap Motion, Inc. 2011-2017.                                 *
 * Leap Motion proprietary and  confidential.                                 *
 *                                                                            *
 * Use subject to the terms of the Leap Motion SDK Agreement available at     *
 * https://developer.leapmotion.com/sdk_agreement, or another agreement       *
 * between Leap Motion and you, your company or other organization.           *
 ******************************************************************************/

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


namespace Leap.Unity.DetectionExamples
{
    
    public class PinchDraw : MonoBehaviour
    {
        public Text textWhichStyle;

        public const int STYLE_TOTAL_NUMBER = 8;

        float[,] styleValues = new float[STYLE_TOTAL_NUMBER, 3]{
        {0.008f, 0.008f, -1.5f},    // 01_afraid
        {0.005f, 0.005f, 0.5f },    // 02_amorous
        {0.002f, 0.005f, 1.3f },    // 03_angry
        {0.01f, 0.005f, -1.0f },    // 04_disgusted
        
        {0.004f, 0.005f, 0.5f },    // 05_happy
        {0.005f, 0.005f, -0.8f},    // 06_sad
        {0.005f, 0.01f,  0.0f },    // 07_serious
        {0.008f, 0.03f,  0.4f }     // 08_surprised
    };

        string[] nameStyles = { "01_afraid", "02_amorous", "03_angry", "04_disgusted", "05_happy", "06_sad", "07_serious", "08_surprised"};

        Material[] materialStyles = new Material[8];


        [Tooltip("Each pinch detector can draw one line at a time.")]
        [SerializeField]
        private PinchDetector[] _pinchDetectors;

        [SerializeField]
        private Material _material;

        [SerializeField]
        private Color _drawColor = Color.white;

        [SerializeField]
        private float _smoothingDelay = 0.01f;

        [SerializeField]
        private float _drawRadius = 0.002f;

        [SerializeField]
        private int _drawResolution = 8;

        [SerializeField]
        private float _minSegmentLength = 0.005f;

        [Tooltip("Simulate the property of ink. If > 0, the faster your finger moves, the thicker the strikes get. If < 0, the thinner they get.")]
        [SerializeField]
        private float _strength = 0.0f;
        

        [SerializeField]
        private int _emotionIndex = 0;

        private DrawState[] _drawStates;

        public Color DrawColor
        {
            get
            {
                return _drawColor;
            }
            set
            {
                _drawColor = value;
            }
        }

        public float DrawRadius
        {
            get
            {
                return _drawRadius;
            }
            set
            {
                _drawRadius = value;
            }
        }


        void OnValidate()
        {

            // If one emotion style is chosen. 如果输入的数字对应一个style
            if (_emotionIndex >= 1 && _emotionIndex <= STYLE_TOTAL_NUMBER)
            {
                _drawRadius = styleValues[_emotionIndex - 1, 0];
                _minSegmentLength = styleValues[_emotionIndex - 1, 1];
                _strength = styleValues[_emotionIndex - 1, 2];
                _material = materialStyles[_emotionIndex - 1];
            }
            else // Any other value would enable users to customise the parameters. 其他值情况下用户可以自主调节参数
            {
                _drawRadius = Mathf.Max(0, _drawRadius);
                _drawResolution = Mathf.Clamp(_drawResolution, 3, 24);
                _minSegmentLength = Mathf.Max(0, _minSegmentLength);
            }


        }

        void Awake()
        {
            // Load the materials by name. 读取assets里的材料
            for (int i = 0; i < STYLE_TOTAL_NUMBER; i++)
            {
                materialStyles[i] = Resources.Load("Materials/" + nameStyles[i], typeof(Material)) as Material;
            }
            // Assign __non to be the default material. 默认的材料是__non
            _material = Resources.Load("Materials/__non", typeof(Material)) as Material;

            if (_pinchDetectors.Length == 0)
            {
                Debug.LogWarning("No pinch detectors were specified!  PinchDraw can not draw any lines without PinchDetectors.");
            }
        }

        void Start()
        {
            /* USED FOR PRINT TESTS */

            //print(STYLE_TOTAL_NUMBER);
            textWhichStyle.text = "Current style: default.";

            _drawStates = new DrawState[_pinchDetectors.Length];
            for (int i = 0; i < _pinchDetectors.Length; i++)
            {
                _drawStates[i] = new DrawState(this);
            }
        }

        void Update()
        {
            for (int i = 0; i < _pinchDetectors.Length; i++)
            {        // left hand, right hand
                var detector = _pinchDetectors[i];
                var drawState = _drawStates[i];

                if (detector.DidStartHold)
                {
                    drawState.BeginNewLine();
                }

                if (detector.DidRelease)
                {
                    drawState.FinishLine();
                }

                if (detector.IsHolding)
                {
                    drawState.UpdateLine(detector.Position);
                }
            }
        }

        // when user touches a button to choose a style.
        public void chooseStyle(int indexOfStyle)
        {
            _emotionIndex = indexOfStyle;
            print(nameStyles[indexOfStyle-1]);
            textWhichStyle.text = "Current style: " + nameStyles[indexOfStyle-1] + ".";
            OnValidate();   // to update the parameters.
        }



        /* CLASS DRAWSTATE */


        private class DrawState
        {
            private List<Vector3> _vertices = new List<Vector3>();
            private List<int> _tris = new List<int>();
            private List<Vector2> _uvs = new List<Vector2>();
            private List<Color> _colors = new List<Color>();

            private PinchDraw _parent;

            private int _rings = 0;

            private Vector3 _prevRing0 = Vector3.zero;
            private Vector3 _prevRing1 = Vector3.zero;
            private Vector3 _prevNormal0 = Vector3.zero;

            private Mesh _mesh;
            private SmoothedVector3 _smoothedPosition;

            public DrawState(PinchDraw parent)
            {
                _parent = parent;

                _smoothedPosition = new SmoothedVector3();
                _smoothedPosition.delay = parent._smoothingDelay;
                _smoothedPosition.reset = true;
            }

            public GameObject BeginNewLine()
            {
                _rings = 0;
                _vertices.Clear();
                _tris.Clear();
                _uvs.Clear();
                _colors.Clear();

                _smoothedPosition.reset = true;

                _mesh = new Mesh();
                _mesh.name = "Line Mesh";
                _mesh.MarkDynamic();
                
                GameObject lineObj = new GameObject("Line Object");
                lineObj.transform.parent = GameObject.Find("Virtual Canvas").transform;
                lineObj.transform.position = Vector3.zero;
                lineObj.transform.rotation = Quaternion.identity;
                lineObj.transform.localScale = Vector3.one;
                lineObj.AddComponent<MeshFilter>().mesh = _mesh;
                lineObj.AddComponent<MeshRenderer>().sharedMaterial = _parent._material;
                lineObj.AddComponent<Rigidbody>().useGravity = false;
                lineObj.AddComponent<MeshCollider>();


                return lineObj;
            }

            public void UpdateLine(Vector3 position)
            {
                _smoothedPosition.Update(position, Time.deltaTime);

                bool shouldAdd = false;

                shouldAdd |= _vertices.Count == 0;
                shouldAdd |= Vector3.Distance(_prevRing0, _smoothedPosition.value) >= _parent._minSegmentLength;        // 平滑程度

                if (shouldAdd)
                {
                    addRing(_smoothedPosition.value);
                    updateMesh();
                }
            }

            public void FinishLine()
            {
                _mesh.UploadMeshData(true);
            }

            private void updateMesh()
            {
                _mesh.SetVertices(_vertices);
                _mesh.SetColors(_colors);
                _mesh.SetUVs(0, _uvs);
                _mesh.SetIndices(_tris.ToArray(), MeshTopology.Triangles, 0);
                _mesh.RecalculateBounds();
                _mesh.RecalculateNormals();
            }

            private void addRing(Vector3 ringPosition)
            {

                float dis = Vector3.Distance(_prevRing0, _smoothedPosition.value);  // dis为每一次新的描点到上一次的距离
                float ratio = dis / _parent._minSegmentLength;

                _rings++;

                if (_rings == 1)
                {
                    addVertexRing();
                    addVertexRing();      // 加了两个0 0 0的顶点
                    addTriSegment();
                }

                addVertexRing();
                addTriSegment();

                Vector3 ringNormal = Vector3.zero;
                if (_rings == 2)
                {
                    Vector3 direction = ringPosition - _prevRing0;
                    float angleToUp = Vector3.Angle(direction, Vector3.up);

                    if (angleToUp < 10 || angleToUp > 170)
                    {
                        ringNormal = Vector3.Cross(direction, Vector3.right);
                    }
                    else
                    {
                        ringNormal = Vector3.Cross(direction, Vector3.up);
                    }

                    ringNormal = ringNormal.normalized;

                    _prevNormal0 = ringNormal;
                }
                else if (_rings > 2)
                {
                    Vector3 prevPerp = Vector3.Cross(_prevRing0 - _prevRing1, _prevNormal0);
                    ringNormal = Vector3.Cross(prevPerp, ringPosition - _prevRing0).normalized;
                }




                if (_rings == 2)
                {
                    updateRingVerts(0,
                                    _prevRing0,
                                    ringPosition - _prevRing1,
                                    _prevNormal0,
                                    0);
                }

                if (_rings >= 2)
                {
                    updateRingVerts(_vertices.Count - _parent._drawResolution,
                                    ringPosition,
                                    ringPosition - _prevRing0,
                                    ringNormal,
                                    0);
                    updateRingVerts(_vertices.Count - _parent._drawResolution * 2,
                                    ringPosition,
                                    ringPosition - _prevRing0,
                                    ringNormal,
                                    1);   // 控制收笔时的半径
                    updateRingVerts(_vertices.Count - _parent._drawResolution * 3,
                                    _prevRing0,
                                    ringPosition - _prevRing1,
                                    _prevNormal0,
                                    Mathf.Clamp((Mathf.Pow(ratio, _parent._strength)), 0.1f, 10.0f));
                }

                _prevRing1 = _prevRing0;
                _prevRing0 = ringPosition;

                _prevNormal0 = ringNormal;
            }

            private void addVertexRing()
            {
                for (int i = 0; i < _parent._drawResolution; i++)
                {
                    _vertices.Add(Vector3.zero);  //Dummy vertex, is updated later
                    _uvs.Add(new Vector2(i / (_parent._drawResolution - 1.0f), 0));
                    _colors.Add(_parent._drawColor);
                }
            }

            //Connects the most recently added vertex ring to the one before it
            private void addTriSegment()
            {
                for (int i = 0; i < _parent._drawResolution; i++)
                {
                    int i0 = _vertices.Count - 1 - i;
                    int i1 = _vertices.Count - 1 - ((i + 1) % _parent._drawResolution);

                    _tris.Add(i0);
                    _tris.Add(i1 - _parent._drawResolution);
                    _tris.Add(i0 - _parent._drawResolution);

                    _tris.Add(i0);
                    _tris.Add(i1);
                    _tris.Add(i1 - _parent._drawResolution);
                }
            }

            private void updateRingVerts(int offset, Vector3 ringPosition, Vector3 direction, Vector3 normal, float radiusScale)
            {
                direction = direction.normalized;
                normal = normal.normalized;

                for (int i = 0; i < _parent._drawResolution; i++)
                {
                    float angle = 360.0f * (i / (float)(_parent._drawResolution));
                    Quaternion rotator = Quaternion.AngleAxis(angle, direction);
                    Vector3 ringSpoke = rotator * normal * _parent._drawRadius * radiusScale;
                    _vertices[offset + i] = ringPosition + ringSpoke;
                }
            }
        }
    }
}
