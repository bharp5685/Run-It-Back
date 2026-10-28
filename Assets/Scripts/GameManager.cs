using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    // The returner in the scene
    public Transform returner;

    // Prefab used to make tacklers
    public GameObject tacklerPrefab;

    // UI text
    public TMP_Text levelText;
    public TMP_Text gameMessage;

    // Message timing
    public float messageDuration = 0.8f;

    // Touchdowns get a little more celebration time
    public float levelStartDelay = 0.4f;

    // Tackles restart quicker
    public float tackleMessageDuration = 0.45f;
    public float tackleStartDelay = 0.15f;

    // Sound effects
    public AudioClip whistleSound;
    public AudioClip tackleSound;
    public AudioClip touchdownSound;
    public AudioClip victorySound;

    // Level setup
    public int currentLevel = 1;
    public int maxLevel = 11;

    // Width used by the defensive lanes
    public float leftLaneX = -5f;
    public float rightLaneX = 5f;

    // Starting spots for the rows
    public float singleRowY = 6f;
    public float frontRowY = 4.5f;
    public float middleRowY = 5.8f;
    public float backRowY = 7f;

    // Returner gets a small speed boost each level
    public float speedIncreasePerLevel = 0.1f;

    private Vector2 returnerStart;

    private PlayerController playerController;
    private AudioSource audioSource;
    private float baseReturnerSpeed;

    // Stops tackles and touchdowns from triggering twice
    private bool isTransitioning = false;

    // Used after Level 11 is completed
    private bool gameWon = false;

    // Keeps track of tacklers currently on the field
    private List<GameObject> activeTacklers =
        new List<GameObject>();

    void Start()
    {
        // Remember where the returner starts
        returnerStart = returner.position;

        // Grab the player's movement script
        playerController =
            returner.GetComponent<PlayerController>();

        // Grab the AudioSource
        audioSource =
            GetComponent<AudioSource>();

        // Remember the player's starting speed
        baseReturnerSpeed =
            playerController.moveSpeed;

        UpdateReturnerSpeed();
        UpdateLevelText();

        // Hide the big message when the game starts
        if (gameMessage != null)
        {
            gameMessage.gameObject.SetActive(false);
        }

        SpawnTacklers();

        // Start the first play with a whistle
        PlaySound(whistleSound);
    }

    void Update()
    {
        // Press R to restart after beating Level 11
        if (gameWon &&
            Keyboard.current != null &&
            Keyboard.current.rKey.wasPressedThisFrame)
        {
            RestartGame();
        }
    }

    void SpawnTacklers()
    {
        // Levels 1-4 use one row
        if (currentLevel <= 4)
        {
            SpawnRow(
                currentLevel,
                singleRowY,
                0f
            );
        }

        // Levels 5-8 use two rows
        else if (currentLevel <= 8)
        {
            int frontCount =
                (currentLevel + 1) / 2;

            int backCount =
                currentLevel - frontCount;

            SpawnRow(
                frontCount,
                frontRowY,
                0f
            );

            SpawnRow(
                backCount,
                middleRowY,
                0.5f
            );
        }

        // Levels 9-11 use three rows
        else
        {
            int baseCount =
                currentLevel / 3;

            int extra =
                currentLevel % 3;

            int frontCount =
                baseCount +
                (extra > 0 ? 1 : 0);

            int middleCount =
                baseCount +
                (extra > 1 ? 1 : 0);

            int backCount =
                baseCount;

            SpawnRow(
                frontCount,
                frontRowY,
                0f
            );

            SpawnRow(
                middleCount,
                middleRowY,
                0.5f
            );

            SpawnRow(
                backCount,
                backRowY,
                -0.5f
            );
        }
    }

    void SpawnRow(
        int tacklerCount,
        float rowY,
        float rowOffset)
    {
        // Divide the field into equal lanes
        float fieldWidth =
            rightLaneX - leftLaneX;

        float laneWidth =
            fieldWidth / tacklerCount;

        // Each level gets its own lane overlap
        float currentLaneOverlap =
            GetLaneOverlap();

        for (int i = 0; i < tacklerCount; i++)
        {
            float baseLaneMin =
                leftLaneX +
                laneWidth * i;

            float baseLaneMax =
                baseLaneMin +
                laneWidth;

            float laneMin =
                baseLaneMin +
                rowOffset -
                currentLaneOverlap;

            float laneMax =
                baseLaneMax +
                rowOffset +
                currentLaneOverlap;

            // Keep lanes inside the field
            laneMin = Mathf.Clamp(
                laneMin,
                leftLaneX,
                rightLaneX
            );

            laneMax = Mathf.Clamp(
                laneMax,
                leftLaneX,
                rightLaneX
            );

            // Start near the middle of the lane
            float spawnX =
                (baseLaneMin + baseLaneMax) / 2f +
                rowOffset;

            spawnX = Mathf.Clamp(
                spawnX,
                laneMin,
                laneMax
            );

            Vector2 spawnPosition =
                new Vector2(
                    spawnX,
                    rowY
                );

            // Create the tackler
            GameObject newTackler =
                Instantiate(
                    tacklerPrefab,
                    spawnPosition,
                    Quaternion.identity
                );

            TacklerAI tacklerAI =
                newTackler.GetComponent<TacklerAI>();

            float currentTacklerSpeed =
                GetTacklerSpeed();

            // Tell the tackler which lane it owns
            tacklerAI.Setup(
                returner,
                this,
                currentTacklerSpeed,
                laneMin,
                laneMax
            );

            activeTacklers.Add(newTackler);
        }
    }

    float GetTacklerSpeed()
    {
        // Smooth difficulty increase through the game
        if (currentLevel == 1)
        {
            return 3.1f;
        }
        else if (currentLevel == 2)
        {
            return 3.2f;
        }
        else if (currentLevel == 3)
        {
            return 3.3f;
        }
        else if (currentLevel == 4)
        {
            return 3.4f;
        }
        else if (currentLevel == 5)
        {
            return 3.45f;
        }
        else if (currentLevel == 6)
        {
            return 3.5f;
        }
        else if (currentLevel == 7)
        {
            return 3.55f;
        }
        else if (currentLevel == 8)
        {
            return 3.6f;
        }
        else if (currentLevel == 9)
        {
            // Three rows already make this level harder
            return 3.4f;
        }
        else if (currentLevel == 10)
        {
            // Slightly easier than the old Level 10
            return 3.4f;
        }
        else
        {
            // Level 11 should be the hardest
            return 3.5f;
        }
    }

    float GetLaneOverlap()
    {
        // Early levels can cover more ground
        // because there are fewer tacklers
        if (currentLevel == 1)
        {
            return 0.35f;
        }
        else if (currentLevel == 2)
        {
            return 0.35f;
        }
        else if (currentLevel == 3)
        {
            return 0.32f;
        }
        else if (currentLevel == 4)
        {
            return 0.30f;
        }
        else if (currentLevel == 5)
        {
            return 0.28f;
        }
        else if (currentLevel == 6)
        {
            return 0.25f;
        }
        else if (currentLevel == 7)
        {
            return 0.22f;
        }
        else if (currentLevel == 8)
        {
            return 0.18f;
        }
        else if (currentLevel == 9)
        {
            return 0.12f;
        }
        else if (currentLevel == 10)
        {
            // Slightly clearer gaps than before
            return 0.05f;
        }
        else
        {
            // Level 11 has no lane overlap
            return 0f;
        }
    }

    void UpdateReturnerSpeed()
    {
        // Slowly increase the returner's speed each level
        playerController.moveSpeed =
            baseReturnerSpeed +
            (currentLevel - 1) *
            speedIncreasePerLevel;

        // Small final-level bonus
        if (currentLevel == 11)
        {
            playerController.moveSpeed += 0.15f;
        }
    }

    void UpdateLevelText()
    {
        if (levelText != null)
        {
            levelText.text =
                "LEVEL " + currentLevel;
        }
    }

    void ShowMessage(string message)
    {
        if (gameMessage != null)
        {
            gameMessage.text = message;
            gameMessage.gameObject.SetActive(true);
        }
    }

    void HideMessage()
    {
        if (gameMessage != null)
        {
            gameMessage.gameObject.SetActive(false);
        }
    }

    void PlaySound(AudioClip sound)
    {
        if (audioSource != null && sound != null)
        {
            audioSource.PlayOneShot(sound);
        }
    }

    float GetSoundWaitTime(AudioClip sound)
    {
        // Wait for either the message or sound,
        // whichever lasts longer
        if (sound == null)
        {
            return messageDuration;
        }

        return Mathf.Max(
            messageDuration,
            sound.length
        );
    }

    void SetGameplayEnabled(bool enabled)
    {
        // Stop or start the returner
        if (playerController != null)
        {
            playerController.enabled = enabled;
        }

        // Stop or start all tacklers
        foreach (GameObject tackler in activeTacklers)
        {
            if (tackler != null)
            {
                TacklerAI tacklerAI =
                    tackler.GetComponent<TacklerAI>();

                if (tacklerAI != null)
                {
                    tacklerAI.enabled = enabled;
                }
            }
        }
    }

    public void PlayerTackled()
    {
        // Ignore tackles while changing plays
        if (isTransitioning || gameWon)
        {
            return;
        }

        StartCoroutine(TackledRoutine());
    }

    IEnumerator TackledRoutine()
    {
        isTransitioning = true;

        Debug.Log(
            "Tackled! Restarting Level " +
            currentLevel
        );

        SetGameplayEnabled(false);

        PlaySound(tackleSound);

        ShowMessage("TACKLED!");

        // Tackles intentionally restart quickly
        yield return new WaitForSeconds(
            tackleMessageDuration
        );

        HideMessage();

        ResetPlay();

        // Keep everybody still for a short pause
        SetGameplayEnabled(false);

        yield return new WaitForSeconds(
            tackleStartDelay
        );

        // Stop any long tackle sound before whistle
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        PlaySound(whistleSound);

        SetGameplayEnabled(true);

        isTransitioning = false;
    }

    public void Touchdown()
    {
        // Ignore touchdowns while changing plays
        if (isTransitioning || gameWon)
        {
            return;
        }

        StartCoroutine(TouchdownRoutine());
    }

    IEnumerator TouchdownRoutine()
    {
        isTransitioning = true;

        Debug.Log("Touchdown!");

        SetGameplayEnabled(false);

        // Beating Level 11 wins the game
        if (currentLevel == maxLevel)
        {
            gameWon = true;

            PlaySound(victorySound);

            ShowMessage(
                "YOU WIN!\nPRESS R TO RESTART"
            );

            Debug.Log(
                "You Win! Level 11 Complete!"
            );

            yield break;
        }

        PlaySound(touchdownSound);

        ShowMessage("TOUCHDOWN!");

        // Let the touchdown celebration finish
        yield return new WaitForSeconds(
            GetSoundWaitTime(touchdownSound)
        );

        // Move to the next level
        currentLevel++;

        UpdateReturnerSpeed();
        UpdateLevelText();

        Debug.Log(
            "Starting Level " +
            currentLevel
        );

        HideMessage();

        ResetPlay();

        SetGameplayEnabled(false);

        // Small pause before the next level
        yield return new WaitForSeconds(
            levelStartDelay
        );

        PlaySound(whistleSound);

        SetGameplayEnabled(true);

        isTransitioning = false;
    }

    void RestartGame()
    {
        // Go all the way back to Level 1
        currentLevel = 1;

        gameWon = false;
        isTransitioning = false;

        UpdateReturnerSpeed();
        UpdateLevelText();

        HideMessage();

        ResetPlay();

        SetGameplayEnabled(false);

        StartCoroutine(RestartDelay());
    }

    IEnumerator RestartDelay()
    {
        yield return new WaitForSeconds(
            levelStartDelay
        );

        PlaySound(whistleSound);

        SetGameplayEnabled(true);

        Debug.Log("Game restarted!");
    }

    void ResetPlay()
    {
        // Put the returner back at the start
        returner.position = returnerStart;

        // Remove all old tacklers
        foreach (GameObject tackler in activeTacklers)
        {
            if (tackler != null)
            {
                tackler.SetActive(false);
                Destroy(tackler);
            }
        }

        activeTacklers.Clear();

        // Make the defense for this level
        SpawnTacklers();
    }
}