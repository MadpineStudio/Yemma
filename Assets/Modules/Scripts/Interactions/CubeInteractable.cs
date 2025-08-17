using _Modules.Player.Script;
using UnityEngine;

public class CubeInteractable : InteractableBehaviour
{
    void Start()
    {
        canToggle = true;
    }
    public override void Action()
    {
        base.Action();
        transform.Translate(Vector3.up * Time.deltaTime * .1f);
    }
}
