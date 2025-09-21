using UnityEngine;

public class HitableBehaviour : MonoBehaviour
{
    [SerializeField] private float hitPoints;
    private float invencibilityTimer = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (invencibilityTimer > 0) invencibilityTimer -= Time.deltaTime;
    }
    public void GetHit(float damage)
    {
        if (invencibilityTimer > 0) return;
        invencibilityTimer = 0.7f;
        hitPoints -= damage;
        if (hitPoints <= 0) Die();
    }
    private void Die() {
        Destroy(gameObject);
    }


}
