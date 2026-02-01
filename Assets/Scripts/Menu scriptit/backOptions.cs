using UnityEngine;
using UnityEngine.SceneManagement;
public class backOptions : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   
   public void Backoption()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
