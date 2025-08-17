using UnityEditor;
using UnityEngine;

public class Madpaint : EditorWindow {

    private Color _brushColor = Color.red;
    private float _brushSize = 0.5f;
    private float _brushStrength = 1f;

    private Vector3 _lastHitPoint;
    private Vector3 _lastHitNormal;
    private bool _hasHit;

    private Mesh _targetMesh;
    private Color[] _vertexColors;
    private Transform _targetTransform;

    public SingleCompute singleCompute;
    public ComputeShader vertexPainterCS; // Compute shader para pintura de vértices
    public VertexPaintData vertexPaintAsset; // Asset para persistir as cores

    private bool _paintingEnabled = true; // Toggle para habilitar/desabilitar pintura

    [MenuItem("Tools/Madpaint")]
    public static void ShowWindow() {
        GetWindow<Madpaint>("Madpaint");
    }

    private void OnEnable() {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable() {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    private void OnGUI() {
        GUILayout.Label("Vertex Painter Settings", EditorStyles.boldLabel);
        if(GUILayout.Button(_paintingEnabled ? "Desabilitar Pintura" : "Habilitar Pintura"))
            _paintingEnabled = !_paintingEnabled;
        _brushColor = EditorGUILayout.ColorField("Brush Color", _brushColor);
        _brushSize = EditorGUILayout.Slider("Brush Size", _brushSize, 0.1f, 2f);
        _brushStrength = EditorGUILayout.Slider("Brush Strength", _brushStrength, 0.1f, 1f);
        vertexPainterCS = (ComputeShader)EditorGUILayout.ObjectField("Vertex Painter Compute", vertexPainterCS, typeof(ComputeShader), false);
        vertexPaintAsset = (VertexPaintData)EditorGUILayout.ObjectField("Vertex Paint Asset", vertexPaintAsset, typeof(VertexPaintData), false);
        singleCompute = (SingleCompute)EditorGUILayout.ObjectField("Single Compute", singleCompute, typeof(SingleCompute), true);

        if (GUILayout.Button("Salvar Vertex Paint"))
            SalvarVertexPaint();
    }

    private void OnSceneGUI(SceneView sceneView) {
        Event e = Event.current;
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            _lastHitPoint = hit.point;
            _lastHitNormal = hit.normal;
            _hasHit = true;
            MeshFilter mf = hit.collider.GetComponent<MeshFilter>();
            if (mf != null) {
                _targetMesh = mf.sharedMesh;
                _targetTransform = mf.transform;
                if (_targetMesh.colors == null || _targetMesh.colors.Length == 0) {
                    _vertexColors = new Color[_targetMesh.vertexCount];
                    for (int i = 0; i < _vertexColors.Length; i++)
                        _vertexColors[i] = Color.black;
                    _targetMesh.colors = _vertexColors;
                } else {
                    _vertexColors = _targetMesh.colors;
                }
                if (vertexPaintAsset != null && vertexPaintAsset.vertexColors != null && vertexPaintAsset.vertexColors.Length == _vertexColors.Length) {
                    _vertexColors = vertexPaintAsset.vertexColors;
                    _targetMesh.colors = _vertexColors;
                }
            } else {
                _targetMesh = null;
                _vertexColors = null;
                _targetTransform = null;
            }
        } else {
            _hasHit = false;
            _targetMesh = null;
            _vertexColors = null;
            _targetTransform = null;
        }

        if (_paintingEnabled && (e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && e.button == 0 && _hasHit) {
            Paint(_lastHitPoint);
            e.Use();
        }

        if (e.type == EventType.Repaint && _hasHit) {
            Handles.color = _brushColor;
            Handles.DrawWireDisc(_lastHitPoint, _lastHitNormal, _brushSize);
        }
        sceneView.Repaint();
    }

    struct VertexData {
        public Vector3 position;
        public Vector4 color;
    }

    private void Paint(Vector3 worldPos) {
        if (_targetMesh == null || _vertexColors == null || _targetTransform == null || vertexPainterCS == null)
            return;

        Vector3 localPos = _targetTransform.InverseTransformPoint(worldPos);
        int vertexCount = _targetMesh.vertexCount;
        VertexData[] vertexArray = new VertexData[vertexCount];
        Vector3[] vertices = _targetMesh.vertices;
        for (int i = 0; i < vertexCount; i++) {
            vertexArray[i] = new VertexData() {
                position = vertices[i],
                color = _vertexColors[i]
            };
        }

        ComputeBuffer buffer = new ComputeBuffer(vertexCount, sizeof(float) * 7);
        buffer.SetData(vertexArray);
        int kernel = vertexPainterCS.FindKernel("CSMain");
        vertexPainterCS.SetBuffer(kernel, "vertexBuffer", buffer);
        vertexPainterCS.SetVector("brushWorldPos", localPos);
        vertexPainterCS.SetFloat("brushSize", _brushSize);
        vertexPainterCS.SetFloat("brushStrength", _brushStrength);
        vertexPainterCS.SetVector("brushColor", _brushColor);

        int threadGroups = Mathf.CeilToInt(vertexCount / 64f);
        vertexPainterCS.Dispatch(kernel, threadGroups, 1, 1);
        buffer.GetData(vertexArray);
        buffer.Release();

        for (int i = 0; i < vertexCount; i++)
            _vertexColors[i] = vertexArray[i].color;

        _targetMesh.colors = _vertexColors;
        if (vertexPaintAsset != null) {
            vertexPaintAsset.vertexColors = _vertexColors;
            EditorUtility.SetDirty(vertexPaintAsset);
        }

        singleCompute.Paint();
    }

    private void SalvarVertexPaint() {
        if (vertexPaintAsset == null) {
            Debug.LogWarning("Nenhum Vertex Paint Asset atribuído.");
            return;
        }
        if (_vertexColors == null) {
            Debug.LogWarning("Não há dados de pintura para salvar.");
            return;
        }
        vertexPaintAsset.vertexColors = _vertexColors;
        EditorUtility.SetDirty(vertexPaintAsset);
        AssetDatabase.SaveAssets();
        Debug.Log("Vertex paint salvo.");
    }
}
