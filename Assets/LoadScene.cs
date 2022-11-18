using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{

    public void LoadGolf()
    {
        SceneManager.LoadScene("GolfSolitaire");
    }

    public void LoadProspector()
    {
        SceneManager.LoadScene("__Prospector_Scene_0 1");
    }
}
