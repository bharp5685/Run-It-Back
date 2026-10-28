using UnityEngine;
using System.Collections.Generic;

public class TacklerAI : MonoBehaviour
{
    // The player this tackler is chasing
    public Transform returner;

    // How fast this tackler moves
    public float moveSpeed = 3.5f;

    // Lets us tell the GameManager when a tackle happens
    public GameManager gameManager;

    // Keeps tacklers from stacking on each other
    public float separationRadius = 1f;
    public float separationStrength = 1.5f;

    // The section of the field this tackler is responsible for
    private float laneMinX;
    private float laneMaxX;

    private Rigidbody2D rb;

    // Keeps track of all tacklers on the field
    private static List<TacklerAI> allTacklers =
        new List<TacklerAI>();

    void Awake()
    {
        // Grab the Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        if (!allTacklers.Contains(this))
        {
            allTacklers.Add(this);
        }
    }

    void OnDisable()
    {
        allTacklers.Remove(this);
    }

    public void Setup(
        Transform newReturner,
        GameManager newGameManager,
        float newMoveSpeed,
        float newLaneMinX,
        float newLaneMaxX)
    {
        returner = newReturner;
        gameManager = newGameManager;
        moveSpeed = newMoveSpeed;

        laneMinX = newLaneMinX;
        laneMaxX = newLaneMaxX;
    }

    void FixedUpdate()
    {
        if (returner == null)
        {
            return;
        }

        Vector2 returnerPosition = returner.position;

        // Chase the returner, but only inside this tackler's lane
        float targetX = Mathf.Clamp(
            returnerPosition.x,
            laneMinX,
            laneMaxX
        );

        Vector2 targetPosition =
            new Vector2(
                targetX,
                returnerPosition.y
            );

        Vector2 chaseDirection =
            (targetPosition - rb.position).normalized;

        Vector2 separation = Vector2.zero;
        int nearbyTacklers = 0;

        // Give nearby tacklers some space
        foreach (TacklerAI otherTackler in allTacklers)
        {
            if (otherTackler == null ||
                otherTackler == this)
            {
                continue;
            }

            Vector2 awayFromTackler =
                rb.position - otherTackler.rb.position;

            float distance =
                awayFromTackler.magnitude;

            if (distance > 0f &&
                distance < separationRadius)
            {
                float pushAmount =
                    1f - (distance / separationRadius);

                separation +=
                    awayFromTackler.normalized *
                    pushAmount;

                nearbyTacklers++;
            }
        }

        if (nearbyTacklers > 0)
        {
            separation /= nearbyTacklers;
        }

        Vector2 moveDirection =
            (chaseDirection +
             separation * separationStrength).normalized;

        Vector2 newPosition =
            rb.position +
            moveDirection *
            moveSpeed *
            Time.fixedDeltaTime;

        // Don't let this tackler leave its assigned lane
        newPosition.x = Mathf.Clamp(
            newPosition.x,
            laneMinX,
            laneMaxX
        );

        rb.MovePosition(newPosition);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Reset the play if we tackle the returner
        if (other.CompareTag("Returner") &&
            gameManager != null)
        {
            gameManager.PlayerTackled();
        }
    }
}