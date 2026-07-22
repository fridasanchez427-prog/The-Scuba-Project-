using UnityEngine;

public class FinishLineDetector : MonoBehaviour
{
    private bool hasWon = false;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the finish line is tagged as "Player"
        if (other.CompareTag("Player") && !hasWon)
        {
            hasWon = true;
            Debug.Log("Victory! You crossed the finish line.");
        }
    }

    // Draws a simple "You Win!" text box on your screen
    private void OnGUI()
    {
        if (hasWon)
        {
            // Center the text on the screen
            Rect position = new Rect(Screen.width / 2 - 150, Screen.height / 2 - 25, 300, 50);
            
            GUIStyle style = new GUIStyle();
            style.fontSize = 40;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.green;

            GUI.Label(position, "YOU WIN!", style);
        }
    }
}
