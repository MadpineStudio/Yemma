using UnityEngine;

[ExecuteInEditMode]
public class CloudManager : MonoBehaviour
{
    struct CloudAreaData
    {
        public Vector3 bounds;
        public Vector3 position;
    }

    [Header("Cloud Settings")]
    public Material cloudMaterial;

    private ComputeBuffer _cloudDataBuffer;
    private CloudAreaData[] _cloudDataArray;
    private static readonly int CloudBufferID = Shader.PropertyToID("_CloudBuffer");

    public void SetupBuffers()
    {
        _cloudDataArray = new CloudAreaData[1];
            
       
        int count = _cloudDataArray.Length;
        int size = sizeof(float) * 6;
        _cloudDataBuffer = new ComputeBuffer(count, size, ComputeBufferType.Structured);
        
        Debug.Log("Buffers");
    }
    public void ClearBuffers()
    {
        _cloudDataBuffer?.Release();
        Debug.Log("Clear Buffers");

    }
    private void LateUpdate()
    {
        if(_cloudDataBuffer != null)
            cloudMaterial.SetBuffer(CloudBufferID, _cloudDataBuffer);
    }
    public void SetCloudArea(Transform cloudArea)
    {
        if(_cloudDataArray == null) return;
        
        CloudAreaData cloud = new CloudAreaData();
        cloud.bounds = cloudArea.localScale;
        cloud.position = cloudArea.position;
        _cloudDataArray[0] = cloud;
        
        _cloudDataBuffer.SetData(_cloudDataArray);

    }
}