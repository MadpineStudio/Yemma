using UnityEngine;

public class VerletBase : MonoBehaviour
{
    [SerializeField] private float radius = 0.1f;
    [SerializeField] private float gravity = -9.8f;
    [SerializeField] private float damping = 0.99f;
    [SerializeField] private float stiffness = 0.5f;
    [SerializeField] private int numPoints = 3; // Novo campo para controlar o número de pontos

    // Variáveis para rastrear mudanças
    private float _lastGravity;
    private float _lastDamping;
    private float _lastStiffness;
    private int _lastNumPoints;

    private Point[] _points;
    private Stick[] _sticks;
    
    private void Start()
    {
        InitializeSimulation();
        _lastGravity = gravity;
        _lastDamping = damping;
        _lastStiffness = stiffness;
        _lastNumPoints = numPoints;
    }

    private void Update()
    {
        // Verifica se algum parâmetro foi alterado no Inspector
        if (gravity != _lastGravity || damping != _lastDamping || 
            stiffness != _lastStiffness || numPoints != _lastNumPoints)
        {
            ResetSimulation();
            _lastGravity = gravity;
            _lastDamping = damping;
            _lastStiffness = stiffness;
            _lastNumPoints = numPoints;
        }

        _points[0].Pos = transform.position; // Mantém o primeiro ponto preso ao objeto
        
        // Atualiza pontos e sticks
        foreach (var point in _points) point.Update(Time.deltaTime);
        foreach (var stick in _sticks) stick.Update(Time.deltaTime);
    }

    // Inicializa ou reinicia a simulação
    private void InitializeSimulation()
    {
        _points = new Point[numPoints];
        _sticks = new Stick[numPoints - 1];

        Vector3 startPos = transform.position;
        for (int i = 0; i < numPoints; i++)
        {
            bool isPinned = (i == 0); // Apenas o primeiro ponto é fixo
            _points[i] = new Point(
                startPos - Vector3.up * i,
                new Vector3(0, gravity, 0),
                1.0f,
                damping,
                isPinned
            );

            if (i > 0)
            {
                _sticks[i - 1] = new Stick(_points[i - 1], _points[i], stiffness);
            }
        }
    }

    public void ResetSimulation()
    {
        InitializeSimulation();
    }
    private void OnDrawGizmos()
    {
        if (_points != null)
        {
            for (int i = 0; i < _points.Length; i++)
            {
                if(_points[i].Pinned) Gizmos.color = Color.red;
                else Gizmos.color = Color.green;
                
                Gizmos.DrawSphere(_points[i].Pos, radius);
            }
        }
        if (_sticks != null)
        {
            for (int i = 0; i < _sticks.Length; i++)
            {
                Gizmos.DrawLine(_sticks[i].P0.Pos, _sticks[i].P1.Pos);
            }
        }
    }
}
