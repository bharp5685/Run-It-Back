using UnityEngine;

public class EndZone : MonoBehaviour
{
    // Lets the end zone tell the GameManager we scored
    public GameManager gameManager;

    void OnTriggerEnter2D(Collider2D other)
    {
        // Score when the returner reaches the end zone
        if (other.CompareTag("Returner"))
        {
            gameManager.Touchdown();
        }
    }
}
