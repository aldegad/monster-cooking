using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameUIScreen : MonoBehaviour
{
    [SerializeField] GameMenu gameMenu;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.Escape))
        {
            if (gameMenu.gameObject.activeSelf)
            {
                GameManager.Instance.ResumeCamera();
                GameManager.Instance.GameState = GameState.Exploration;
                gameMenu.gameObject.SetActive(false);
            }
            else if(GameManager.Instance.GameState == GameState.Exploration)
            {
                
                GameManager.Instance.PauseCamera();
                GameManager.Instance.GameState = GameState.Menu;
                gameMenu.gameObject.SetActive(true);
            }
        }
    }
}
