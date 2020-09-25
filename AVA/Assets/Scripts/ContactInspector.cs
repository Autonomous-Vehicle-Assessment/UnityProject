using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContactInspector : MonoBehaviour
{
    public bool debugCollisions;

    public void OnCollisionStay(Collision collision)
    {
        if (debugCollisions)
        {
            foreach (ContactPoint contact in collision.contacts)
            {
                float impactVelocity = collision.relativeVelocity.magnitude;
                Debug.DrawRay(contact.point, contact.normal * impactVelocity, Color.red, 0, false);
                Debug.Log(collision.gameObject.name);
            }
        }
    }
}
