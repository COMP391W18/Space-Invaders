using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    Rigidbody2D Body;
    public float Speed = 2f;

    // Fire rate
    public float FireRate = 0.25f;
    float FireTimer;

    // If the player is alie
    bool IsAlive { get; set; }

    // Missile gameobject
    public GameObject Missile;

    // Fire a missile
    public void FireMissile()
    {
        GameObject NewMissile = Instantiate(Missile, transform.position + (new Vector3(0, 0.30f, 0)), transform.rotation);
        NewMissile.GetComponent<Rigidbody2D>().gravityScale = -1;
    }

    // When the player gets destroyed
    IEnumerator DestroyPlayer()
    {
        // Change the sprite
        GetComponent<SpriteAnimation>().ChangeState("DESTROYED");

        // Player is not alive anymore
        IsAlive = false;

        yield return new WaitForSecondsRealtime(2f);

        // Notify the game manager
        GameController.EndGame("You Lost");
    }

    // Reset
    public void ResetPlayer()
    {
        GetComponent<SpriteAnimation>().ChangeState("IDLE");

        transform.position = new Vector3(0, -4.5f, 0);

        IsAlive = true;
    }

    // Use this for initialization
    void Start()
    {
        // Cache the body component
        Body = GetComponent<Rigidbody2D>();

        IsAlive = true;
    }
	
	// Update is called once per frame
	void Update()
    {
        if (!IsAlive)
            return;

        // Increment the fire timer
        FireTimer += Time.deltaTime;

        // Fire a missile if we can
        if (FireTimer > FireRate && Input.GetKey(KeyCode.Space))
        {
            FireTimer = 0;
            FireMissile();
        }

        // If we are moving horizontally
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow))
        {
            // Get how much we are moving left or right
            float Movement = Input.GetAxis("Horizontal");

            // Get the current position and update the pos
            Vector3 CurrPos = Body.position;
            CurrPos.x += Movement * Speed * Time.deltaTime;

            // Clamp the left and right
            CurrPos.x = Mathf.Clamp(CurrPos.x, -8.7f, 8.7f);

            Body.position = CurrPos;
        }
    }

    // When the missile touch the player
    void OnTriggerEnter2D(Collider2D Other)
    {
        // Destroy the object
        StartCoroutine(DestroyPlayer());
    }
}
