using System.Collections;
using UnityEngine;

public class DoorInteractable : InteractableBehaviour
{
    private Vector3 _originalPos;
    private Coroutine _doorRoutine;
    void Start()
    {
        canToggle = true;
        _originalPos = transform.position;
    }

    public override void Update() { }
    public override void ToggleActivation()
    {
        base.ToggleActivation();
        if(_doorRoutine != null){
            StopCoroutine(_doorRoutine);
            _doorRoutine = null;
        }
        _doorRoutine = StartCoroutine(DoorAction());

    }
    private IEnumerator DoorAction()
    {
        float limit = 1.2f - (activated? (transform.position.y - _originalPos.y) : (1.2f - (transform.position.y - _originalPos.y)));
        float current = 0;
        Vector3 newPosition =  transform.position;
        while (current <= limit)
        {
            current += Time.deltaTime;
            newPosition.y += Time.deltaTime * (activated? 1 : -1);
            transform.position = newPosition;
            yield return new WaitForEndOfFrame();
            
        }
        transform.position = _originalPos + new Vector3(0, activated? 1.2f: 0, 0);
    }
}
