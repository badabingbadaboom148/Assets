using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UISceneLoader : MonoBehaviour
{
    [SerializeField] string FirstSceneToLoad;
    void Start()
    {
        SceneManager.LoadScene(FirstSceneToLoad, LoadSceneMode.Additive);
    }
}
