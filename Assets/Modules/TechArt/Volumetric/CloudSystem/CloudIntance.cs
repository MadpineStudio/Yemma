using UnityEngine;

[ExecuteInEditMode]
public class CloudInstance : MonoBehaviour
{
   public CloudManager cloudManager;
   private void Update()
   {
      cloudManager.SetCloudArea(transform);
   }

   private void OnDrawGizmos()
   {
      Gizmos.color = new Color(1, 1, 0, .5f);
      var transform1 = transform;
      Gizmos.DrawWireCube(transform1.position, transform1.localScale);
   }
}