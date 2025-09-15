using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;


[Serializable]
public class CameraObject
{
    public int id;
    public CinemachineCamera camera;
}


public class CameraManager : MonoBehaviour
{
    public List<CameraObject> cameras;

}
