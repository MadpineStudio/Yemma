using UnityEngine;

public class Point
{
   public Vector3 Pos;
   public bool Pinned;

   Vector3 _oldPos;
   Vector3 _force;
   float _mass;
   float _damping;

   public Point(Vector3 pos, Vector3 force, float mass, float damping, bool pinned)
   {
      Pos = pos;
      _oldPos = Pos;
      _force = force;
      _mass = mass;
      Pinned = pinned;
      _damping = damping;
   }
   public void Update(float dt)
   {
      // verlet integration
      if(Pinned) return;

      Vector3 acceleration = _force / _mass;
      Vector3 newPos = 2 * Pos - _oldPos + acceleration * (dt * dt);
      _oldPos = Pos * (1.0f - _damping) + _oldPos * _damping; // Adiciona damping
      
      // Atualiza posições antigas antes das novas
      _oldPos = Pos;
      // Pos = new Vector3(newX, newY, newZ);
      Pos = newPos;
   }
}
