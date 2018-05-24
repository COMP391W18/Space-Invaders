using System;
using System.Collections.Generic;
using UnityEngine;

public class SpriteAnimation : MonoBehaviour {

    //public enum State { IDLE, DESTROYED }

    [Serializable]
    public struct StateStruct
    {
        public static String Default = "IDLE";

        public String Name;
        public Sprite[] Sprites;

    }
    public StateStruct[] SpriteStates;

    // List of sprites used by this animator
    public Dictionary<String, Sprite[]> Sprites;

    // Current state of the animation
    private String CurrentState /*= StateStruct.Default*/;

    // Which sprite we are rendering
    int CurrentSprite = 0;

    // How long a sprite last
    public float SpriteDuration;
    float CurrentTime;

	// Use this for initialization
	void Start()
    {
        //MaxSprite = SpriteList.Length;

        Sprites = new Dictionary<String, Sprite[]>();
        foreach (StateStruct SpriteState in SpriteStates)
            Sprites.Add(SpriteState.Name, SpriteState.Sprites);

        CurrentState = SpriteStates[0].Name;
    }

    public void ChangeState(String NewState)
    {
        // Set the new state
        CurrentState = NewState;

        // Change the sprite immediatly
        CurrentTime = SpriteDuration + 1f;
    }
    
    // Update is called once per frame
    void Update()
    {
        // Change the time
        CurrentTime += Time.deltaTime;

        // Change the sprite if we reach our target
        if (CurrentTime > SpriteDuration)
        {
            // Reset the time
            CurrentTime = 0f;

            // Increment the sprite
            ++CurrentSprite;

            GetComponent<SpriteRenderer>().sprite = Sprites[CurrentState][CurrentSprite % Sprites[CurrentState].Length];
        }
	}
}
