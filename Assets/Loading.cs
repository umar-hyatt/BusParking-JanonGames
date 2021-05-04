using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Loading : MonoBehaviour
{
    void Start()
    {
        Invoke("LoadScene",3f);
    }
    void LoadScene()
    {
        SceneManager.LoadScene("MainMenu");
    }

}
