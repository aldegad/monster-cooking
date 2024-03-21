using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameUIScreen : MonoBehaviour
{
    [SerializeField] GameMenu gameMenu;

    private GameState prevGameState;

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (gameMenu.gameObject.activeSelf)
            {
                gameMenu.gameObject.SetActive(false);
                GameManager.Instance.ResumeCamera();
                GameManager.Instance.GameState = GameState.Exploration;
            }
            else if(GameManager.Instance.GameState == GameState.Exploration)
            {
                gameMenu.gameObject.SetActive(true);
                GameManager.Instance.PauseCamera();
                GameManager.Instance.GameState = GameState.Menu;
            }
        }
    }
}
