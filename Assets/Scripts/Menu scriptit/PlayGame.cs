using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayGame : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
public void GoGame()
    {
        SceneManager.LoadSceneAsync("startscene");
    }
}
