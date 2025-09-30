using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void TestDelegate();
    public static TestDelegate testEvent;

    public delegate void StateChangeDelegate(string stateName);
    public static StateChangeDelegate onStateChange;
}
