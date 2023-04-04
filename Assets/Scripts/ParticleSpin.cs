using UnityEngine;

//-----------------------------------------------------------------------------
// name: ParticleSpin.cs
// desc: rotate entire particle system
//-----------------------------------------------------------------------------

public class ParticleSpin : MonoBehaviour
{
    public Vector3 rate;

    void Update()
    {
        transform.Rotate(rate * Time.deltaTime);
    }
}
