using UnityEngine;

public class EventManager : MonoBehaviour
{
    public delegate void TestDelegate();
    public static TestDelegate testEvent;
}
