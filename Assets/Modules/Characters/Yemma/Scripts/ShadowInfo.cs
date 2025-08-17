using UnityEngine;

[ExecuteInEditMode]
public class ShadowInfo : MonoBehaviour
{
    public Light mainLight;

    [SerializeField] private Transform head;
    [SerializeField] private float lightY;
    [SerializeField] private double result;
    private float _headY;

    [SerializeField] private Material material;
    private static readonly int MainLightY = Shader.PropertyToID("_MainLightY");
    private static readonly int HeadY = Shader.PropertyToID("_HeadY");

    void Update()
    {
        UpdateShaderParameters();
    }

    void UpdateShaderParameters()
    {
        lightY = mainLight.transform.localEulerAngles.y * Mathf.Deg2Rad;
        _headY = head.localEulerAngles.y * Mathf.Deg2Rad;
        result = (_headY - lightY) / Mathf.PI;

        material.SetFloat(MainLightY, lightY);
        material.SetFloat(HeadY, _headY);
    }
}