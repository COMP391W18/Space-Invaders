using System;
using UnityEngine;

public class AlienController : MonoBehaviour {

    // Possible direction
    enum Direction { RIGHT, LEFT, DOWN }

    // Movement detail
    float CurrentDelay = 0f;
    public float MovementDelay;

    // Points for alien
    public int Points;

    // Movement speed
    public Vector2 MovementSpeed = new Vector2(0.05f, 0.05f);

    // General delay
    public float CurrentGeneralDelay = 0f;
    public float GeneralDelay;

    // Where we are moving
    Direction OldDirection;
    Direction CurrentDirection;

    // Number of horizontal and vertical steps
    public Vector2Int Steps;
    Vector2Int CurrentStep;

    // Missile gameobject
    public GameObject Missile;

    // If we can fire or not
    public bool CanFire { get; set; }

    // Number oF alien in the scene
    public static int AlienCount = 0;

    // If the sprite is moving
    public bool isMoving = true;

    // Move alien
    private void MoveAlien()
    {
        // Get the current position
        Vector3 CurrPos = transform.position;

        // If we are moving horizontally
        switch (CurrentDirection)
        {
            case Direction.RIGHT:
                // Increment the number of horizontal steps
                ++CurrentStep.x;

                // Move the sprite horizontally
                CurrPos.x += MovementSpeed.x;

                // If we exceed the number of horizontal steps stop moving horizontally
                if (CurrentStep.x > Steps.x - 1)
                {
                    // Reset the number of steps
                    CurrentStep.x = 0;

                    // Now we move vertically
                    CurrentDirection = Direction.DOWN;

                    // Reminder of old location
                    OldDirection = Direction.RIGHT;
                }
                break;
            case Direction.LEFT:
                // Increment the number of horizontal steps
                ++CurrentStep.x;

                // Move the sprite horizontally
                CurrPos.x -= MovementSpeed.x;

                // If we exceed the number of horizontal steps stop moving horizontally
                if (CurrentStep.x > Steps.x - 1)
                {
                    // Reset the number of steps
                    CurrentStep.x = 0;

                    // Now we move vertically
                    CurrentDirection = Direction.DOWN;

                    // Reminder of old location
                    OldDirection = Direction.LEFT;
                }
                break;
            case Direction.DOWN:
                // Increment the number of horizontal steps
                ++CurrentStep.y;

                // Move the sprite vertically
                CurrPos.y -= MovementSpeed.y;

                // If we exceed the number of horizontal steps stop moving horizontally
                if (CurrentStep.y > Steps.y - 1)
                {
                    // Reset the number of steps
                    CurrentStep.y = 0;

                    // Now we move vertically
                    CurrentDirection = OldDirection == Direction.LEFT ? Direction.RIGHT : Direction.LEFT;
                }
                break;
        }

        // Set the new position
        transform.position = CurrPos;
    }

    // Fire a missile
    public void FireMissile()
    {
        GameObject NewMissile = Instantiate(Missile, transform.position - (new Vector3(0, 0.51f, 0)), transform.rotation);
    }

    // Called when the object is destroyed
    private void OnDestroy()
    {
        // Decrement the number of alien in the scene
        --AlienCount;
    }

    public void DestroyAlien()
    {
        // Change the sprite
        GetComponent<SpriteAnimation>().ChangeState("DESTROYED");

        // Stop the movement
        isMoving = false;

        // Notify the gamecontroller
        GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>().OnAlienDestroyed(this);
    }
        
    // Use this for initialization
    void Start ()
    {
        CurrentDirection = Direction.RIGHT;
        OldDirection = CurrentDirection;

        // Increment the number of alien in the scene
        ++AlienCount;
    }

	// Update is called once per frame
	void Update ()
    {
        // If the row is moving
        if (isMoving)
        {
            // Increment our timers
            CurrentDelay += Time.deltaTime;
        
            if (CurrentDelay > MovementDelay)
            {
                CurrentDelay = 0f;

                // Move the alien
                MoveAlien();

                // Disable moving
                isMoving = false;
            }
        }
        // If the row is not moving
        else
        {
            // Increment our timers
            CurrentGeneralDelay += Time.deltaTime;

            if (CurrentGeneralDelay > GeneralDelay)
            {
                CurrentGeneralDelay = 0f;

                // enable the movement again
                isMoving = true;
            }
        }
            
    }

    // When we git hit by a missile
    void OnTriggerEnter2D(Collider2D Other)
    {
        GetComponent<AlienController>().DestroyAlien();

        Destroy(gameObject, 0.5f);
    }
}
