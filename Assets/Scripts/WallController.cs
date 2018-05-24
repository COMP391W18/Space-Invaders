using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallController : MonoBehaviour {

    // Current stage
    int WallStage = 0;

    // Sprite anim controller
    SpriteAnimation Anim;

	// Use this for initialization
	void Start ()
    {
        Anim = GetComponent<SpriteAnimation>();
    }

    public void OnWallHit()
    { 
        // If our wall can take another hit change the wall sprite
        if (++WallStage < 4)
            Anim.ChangeState("STAGE_" + WallStage);
        // Otherwise disable the object
        else
            gameObject.SetActive(false);

    }

    // Reset the wall
    public void ResetWall()
    {
        // Reset the stage
        WallStage = 0;
        if (Anim)
            Anim.ChangeState("STAGE_0");

        // Re enable the gameobject
        gameObject.SetActive(true);
    }

    // When our wall gets hit
    void OnTriggerEnter2D(Collider2D Other)
    {
        // When we get hit 
        if (Other.tag != "Wall")
            OnWallHit();
    }
}
