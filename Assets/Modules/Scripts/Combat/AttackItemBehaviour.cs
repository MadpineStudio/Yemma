using UnityEngine;

public class AttackItemBehaviour : MonoBehaviour
{
    public delegate void AttackItemDelegate(bool value);
    public static AttackItemDelegate OnAtackInitiated;

    private bool _isInAtack = false;
    [SerializeField] private float hitDamage;
    void OnEnable()
    {
        OnAtackInitiated += OnInitiateAtack;
    }
    void OnDisable()
    {
        OnAtackInitiated -= OnInitiateAtack;
    }
    void Start() { }

    void Update() { }

    void OnTriggerEnter(Collider other)
    {
        if (!_isInAtack) return;
        if (other.CompareTag("Hitable"))
        {
            other.GetComponent<HitableBehaviour>().GetHit(hitDamage);
        }
    }

    private void OnInitiateAtack(bool isInAtack)
    {
        _isInAtack = isInAtack;
    }
}
