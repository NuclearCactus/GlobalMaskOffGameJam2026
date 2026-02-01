using UnityEngine;
using UnityEngine.SceneManagement;
public class options : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
   public void goOptions()
    {
        SceneManager.LoadSceneAsync("OptionScene");
    }

}
