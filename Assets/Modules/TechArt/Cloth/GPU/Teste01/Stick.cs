using UnityEngine;

public class Stick
{
    public float Lenght;
    public Point P0, P1;
    private float _stiffnes;
    
    public Stick(Point p0, Point p1, float stiffness)
    {
        P0 = p0;
        P1 = p1;
        _stiffnes = stiffness;
        Lenght = Vector3.Distance(p0.Pos, p1.Pos);
    }
    public void Update(float dt)
    {
        Vector3 direction = P1.Pos - P0.Pos;
       
        float dist = direction.magnitude;
        float diff = Lenght - dist;
        // float percent = diff / dist * .5f;
        float percent = diff / dist * _stiffnes; // rigidez

        Vector3 offset = direction * percent;
       
        if (!P0.Pinned)
            P0.Pos -= offset;
        if (!P1.Pinned)
            P1.Pos += offset;

    }
}
