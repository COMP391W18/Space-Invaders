using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileController : MonoBehaviour {
    
    // When our missile hit's something
    void OnTriggerEnter2D(Collider2D Other)
    {
        // Destroy the missile gameobject
        Destroy(gameObject);
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
}
