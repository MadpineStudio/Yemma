using UnityEngine;

[System.Serializable]
public struct CloudBoxData
{
    public Vector3 position;
    public Vector3 bounds;
    public float density;
    
    public CloudBoxData(Vector3 pos, Vector3 size, float dens)
    {
        position = pos;
        bounds = size;
        density = dens;
    }
}

public class CloudArea
{
    public Vector3 bounds;
    public Vector3 position;
    public float density;
    
    public CloudArea(Vector3 bounds, Vector3 position, float density = 1f)
    {
        this.bounds = bounds;
        this.position = position;
        this.density = density;
    }
    
    public bool Contains(Vector3 point)
    {
        Vector3 localPoint = point - position;
        return Mathf.Abs(localPoint.x) <= bounds.x * 0.5f && 
               Mathf.Abs(localPoint.y) <= bounds.y * 0.5f && 
               Mathf.Abs(localPoint.z) <= bounds.z * 0.5f;
    }
    
    public CloudBoxData GetShaderData()
    {
        return new CloudBoxData(position, bounds, density);
    }
}