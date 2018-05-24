using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour {

    public enum GameState { MAIN_MENU, START_GAME, GAME_PAUSED, RUNNING, GAME_ENDED };

    // Determine if the game is ended and who won
    static public bool IsGameOver = false;
    static public string EndedResult;

    // List of prefabs
    public GameObject[] AliensPrefabs;

    // UI Elem
    public GameObject GameUI;

    // Cache the alien controller
    AlienController[] AliensComponents;
    const int AliensCanFire = 15;

    // Fire rate
    public float FireRate;
    float FireTimer;

    // Scores
    Vector2Int Scores = new Vector2Int(0, 0);

    // Current game state
    public GameState CurrentGameState = GameState.MAIN_MENU;

    // Callback for when an alien is destroyed
    public void OnAlienDestroyed(AlienController Alien)
    {
        // Find the index of the delayed object
        int DelayedIndex = System.Array.IndexOf(AliensComponents, Alien);

        // If we destroy an alien not in the front row skip the function
        if (DelayedIndex > 14)
            return;

        // Get the alien name
        string AlienName = Alien.name;

        // Get the coordinate of the destroyed alien
        int DelayedRow = int.Parse(AlienName.Substring(6, 1));
        int DelayedCol = int.Parse(AlienName.Substring(8));
        
        // Store the index of the next alien that can fire, if it's -1 there is no alien that can fire
        int NewIndex = -1;

        // Store the name of the next alien that can fire
        string NewName = "";

        // If there is a row behind the alien with an alien that can fire
        while (--DelayedRow >= 0 && NewIndex == -1)
        {
            // Compute the new name
            NewName = string.Format("Alien_{0}_{1}", DelayedRow, DelayedCol);

            // Get the new index
            NewIndex = System.Array.FindIndex(AliensComponents, x => x != null && x.name == NewName);
        }

        // If we found an alien that can fire
        if (NewIndex >= 0)
        {            
            // The new index can fire
            AliensComponents[NewIndex].CanFire = true;

            // Swap the 
            AlienController tempswap = AliensComponents[DelayedIndex];
            AliensComponents[DelayedIndex] = AliensComponents[NewIndex];
            AliensComponents[NewIndex] = tempswap;
        }
        // If we went through the entire row and no alien can fire
        else
        {
            // Decrement the number of alien that can fire
            NewIndex = AliensCanFire - 1;

            AlienController tempswap = AliensComponents[DelayedIndex];
            AliensComponents[DelayedIndex] = AliensComponents[AliensCanFire];
            AliensComponents[AliensCanFire] = tempswap;
        }

        // Update the score
        UpdateScore(Alien.Points);
    }

    public void UpdateScore(int Score)
    {
        // Update the score
        Scores.x += Score;

        GameObject.Find("ScorePoints").GetComponent<Text>().text = System.Convert.ToString(Scores.x);
    }

    public void UpdateHighScore()
    {
        if (Scores.x < Scores.y)
            return;

        // Update the high score
        Scores.y = Scores.x;

        GameObject.Find("HighScorePoints").GetComponent<Text>().text = System.Convert.ToString(Scores.y);
    }

    static public void EndGame(string Result)
    {
        IsGameOver = true;
        EndedResult = Result;
    }

    public IEnumerator OnMainMenu()
    {
        // Reset the game
        ResetGame();

        // Reset the player and walls
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().ResetPlayer();
        GameObject[] Walls = GameObject.FindGameObjectsWithTag("Wall");
        foreach (GameObject Wall in Walls)
            Wall.GetComponent<WallController>().ResetWall();

        // Show the new menu screen
        GameUI.transform.Find("NewGameScreen").gameObject.SetActive(true);

        // Get the text list object
        Transform TextList = GameObject.Find("TextList").transform;

        //
        for (int Index = 0; Index < TextList.childCount; ++Index)
        {
            yield return new WaitForSecondsRealtime(0.5f);
            TextList.GetChild(Index).gameObject.SetActive(true);
        }
    }

    public IEnumerator OnStartGame()
    {
        // Remove all fired missile
        GameObject[] Missiles = GameObject.FindGameObjectsWithTag("Respawn");
        foreach (GameObject Missile in Missiles)
            Destroy(Missile);

        // Hide the new menu screen
        GameUI.transform.Find("NewGameScreen").gameObject.SetActive(false);

        // Hide the UI
        GameUI.SetActive(false);

        // Spawn the aliens
        yield return StartCoroutine(SpawnAliens(5, AliensCanFire));

        // Change the game status
        ChangeGameState(GameState.RUNNING);
    }

    public void OnGameRunning()
    {
        // Resume the entities update
        Time.timeScale = 1;

        // Hide the GUI
        GameUI.SetActive(false);
    }

    public void OnGamePaused()
    {
        // Pause the entities update
        Time.timeScale = 0;

        // Show the GUI
        GameUI.SetActive(true);

        // Show the game paused screen
        GameUI.transform.Find("GameEndedScreen").gameObject.SetActive(false);
        GameUI.transform.Find("GamePausedScreen").gameObject.SetActive(true);
    }

    public void OnGameEnded()
    {
        // Stop all running coroutines
        StopAllCoroutines();

        // Pause the entities update
        Time.timeScale = 0;

        // Show the GUI
        GameUI.SetActive(true);

        // Show the game paused screen
        GameUI.transform.Find("GameEndedScreen").gameObject.SetActive(true);

        GameObject.Find("GameOverText").GetComponent<Text>().text = EndedResult;

        // Update the high score
        UpdateHighScore();
    }

    // Function to change the current state of the game
    public void ChangeGameState(GameState NewState)
    {
        CurrentGameState = NewState;

        switch (CurrentGameState)
        {
            case GameState.MAIN_MENU:
                StartCoroutine(OnMainMenu());
                break;

            case GameState.START_GAME:
                StartCoroutine(OnStartGame());
                break;

            case GameState.GAME_PAUSED:
                OnGamePaused();
                break;

            case GameState.GAME_ENDED:
                OnGameEnded();
                break;

            case GameState.RUNNING:
                OnGameRunning();
                break;
        }
    }

    // Destroy all aliens
    public void DestroyAliens()
    {
        GameObject[] Aliens = GameObject.FindGameObjectsWithTag("Aliens");

        foreach (GameObject Alien in Aliens)
            Destroy(Alien);
    }

    // Spawn the alien
    IEnumerator SpawnAliens(int RowCount, int ColCount)
    {
        // Which alien sprite to select
        int SelectedAlien = 0;

        // Interval between rows moving
        const float RowSpeed = 0.2f;
        const float TotSpeed = RowSpeed * 5;

        // Temp array to keep track of all the created GameObject
        GameObject[] Aliens = new GameObject[RowCount * ColCount];
        int Index = 0;

        GameObject AlienListObject = GameObject.Find("AlienList");

        // Instantiate a lot of aliens
        for (int Row = 0; Row < RowCount; ++Row)
        {
            if (Row > 0)
                SelectedAlien = 1;
            if (Row > 2)
                SelectedAlien = 2;

            for (int Col = 0; Col < ColCount; ++Col)
            {
                // Instantiate a new alien at a location determined by our col and row
                GameObject SpawnedAlien = Aliens[Index] = Instantiate(AliensPrefabs[SelectedAlien], new Vector3(-8f + (1 + Col), 3.25f - (0.75f * Row)), new Quaternion(), AlienListObject.transform);
                
                // Set some properties for our object
                SpawnedAlien.name = "Alien_" + Row + "_" + Col;
                SpawnedAlien.transform.localScale = new Vector3(0.5f, 0.5f, 1);

                // Get the "AlienMovement" component and change some values
                AlienController MovComponent = SpawnedAlien.GetComponent<AlienController>();
                MovComponent.MovementDelay = RowSpeed;
                MovComponent.CurrentGeneralDelay = Row == 4 ? 0 : (RowSpeed * Row);
                MovComponent.GeneralDelay = TotSpeed;
                MovComponent.isMoving = Row == 4 ? true : false;
                MovComponent.CanFire = Row == 4 ? true : false;

                // Increment our counter
                ++Index;

                yield return new WaitForSecondsRealtime(0.025f);
            }
        }

        // Flash the aliens 5 times
        for (int Counter = 0; Counter < 5; ++Counter)
        {
            AlienListObject.SetActive(false);
            yield return new WaitForSecondsRealtime(0.1f);
            AlienListObject.SetActive(true);
            yield return new WaitForSecondsRealtime(0.1f);
        }

        // Cache the alien components
        AliensComponents = new AlienController[Aliens.Length];
        Index = 0;
        foreach (GameObject Alien in Aliens)
            AliensComponents[Index++] = Alien.GetComponent<AlienController>();

        // Put the alien that can fire in the front
        System.Array.Sort(AliensComponents, delegate (AlienController A1, AlienController A2)
        {
            return A1.CanFire && !A2.CanFire ? -1 : 1;
        });

        
    }
        
    void ResetGame()
    {
        // Show the UI
        GameUI.SetActive(true);

        // Destroy all the aliens entities
        DestroyAliens();

        // Pause the game
        Time.timeScale = 0;

        // Reset the fire timer
        FireTimer = 0f;

        // Game is not over!
        IsGameOver = false;
        
        // Reset the player current score
        Scores.x = 0;
    }

    // Use this for initialization
    void Start ()
    {
        // Cache the UI
        GameUI = GameObject.Find("UI");

        // Reset the game
        ResetGame();

        // Start with the main menu
        ChangeGameState(GameState.MAIN_MENU);
    }

	// Update is called once per frame
	void Update ()
    {
        // Do somehing based on the current state
        switch (CurrentGameState)
        {
            case GameState.MAIN_MENU:
                if (Input.GetKey(KeyCode.Space) == true)
                    ChangeGameState(GameState.START_GAME);
                break;

            case GameState.GAME_PAUSED:
                if (Input.GetKeyDown(KeyCode.P) == true)
                    ChangeGameState(GameState.RUNNING);
                break;

            case GameState.GAME_ENDED:
                if (Input.GetKeyDown(KeyCode.Space) == true)
                    ChangeGameState(GameState.MAIN_MENU);
                break;

            case GameState.RUNNING:

                if (Input.GetKeyDown(KeyCode.P) == true)
                    ChangeGameState(GameState.GAME_PAUSED);

                if (IsGameOver)
                    ChangeGameState(GameState.GAME_ENDED);

                if (AlienController.AlienCount == 0)
                        EndGame("You Won!");

                    // Increment the fire timer
                    FireTimer += Time.deltaTime;

                    // Fire a missile if we can
                    if (FireTimer > FireRate)
                    {
                        // Reset the timer
                        FireTimer = 0f;

                        // Have an alien firing
                        int AlienIndex = Random.Range(0, Mathf.Clamp(AliensComponents.Length, 0,  15));

                        try
                        {
                            // Spawn a missile if we can                        
                            AliensComponents[AlienIndex].FireMissile();
                        }
                        catch (MissingReferenceException Ex)
                        {
                            Debug.Log("This Index doesn't exist: " + AlienIndex);

                            throw;
                        }
                    }
                break;
        }

        
    }
}
