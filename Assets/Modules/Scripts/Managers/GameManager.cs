using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public void Awake()
    {
        if(Instance != null){
            Destroy(Instance);
        }
        Instance = this;
        DontDestroyOnLoad(Instance);
    }
    void OnEnable()
    {
        // EventManager.testEvent += () => {Debug.Log("AAAAAA");};
    }
    void OnDisable()
    {
        // EventManager.testEvent -= () => {Debug.Log("AAAAAA");};
    }
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        // EventManager.testEvent.Invoke();
    }
}
