using System.Collections;
using UnityEngine;

public class YemmaFacial : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private float _random;
    private void Start()
    {
        _random = Random.Range(2, 5);
        InvokeRepeating("Blink",3, _random);
    }
    private void Blink()
    {
        animator.Play("Blink");
        _random = Random.Range(2, 5);
    }
}
