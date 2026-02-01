using UnityEngine;
using UnityEngine.SceneManagement;
public class tomainmenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   public void ToMainMenu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
