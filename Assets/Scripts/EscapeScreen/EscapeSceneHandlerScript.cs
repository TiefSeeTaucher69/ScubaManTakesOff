using UnityEngine;
using Michsky.LSS;

public class EscapeSceneHandlerScript : MonoBehaviour
{
    public void QuitGame()
    {
        // Logic to quit the game
        Debug.Log("Game Quit");
        Application.Quit();
    }
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton1))
        {
            LSS_LoadingScreen.LoadScene("MainMenu", "Standard");
        }
    }

    
}
