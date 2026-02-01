using UnityEngine;
using UnityEngine.SceneManagement;
public class nextscene : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   public void Tonextscene()
    {
        SceneManager.LoadSceneAsync("TheRealScene");
    }
}

