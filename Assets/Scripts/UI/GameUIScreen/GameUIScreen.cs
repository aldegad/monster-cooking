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
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                PlayerFollowCamera playerFollowCamera = GameManager.Instance.playerFollowCamera;
                playerFollowCamera.freeLookCam.m_XAxis.m_MaxSpeed = playerFollowCamera.maxSpeedX;
                playerFollowCamera.freeLookCam.m_YAxis.m_MaxSpeed = playerFollowCamera.maxSpeedY;

                GameManager.Instance.gameState = prevGameState;
            }
            else
            {
                gameMenu.gameObject.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                PlayerFollowCamera playerFollowCamera = GameManager.Instance.playerFollowCamera;
                playerFollowCamera.freeLookCam.m_XAxis.m_MaxSpeed = 0f;
                playerFollowCamera.freeLookCam.m_YAxis.m_MaxSpeed = 0f;

                prevGameState = GameManager.Instance.gameState;
                GameManager.Instance.gameState = GameState.Menu;
            }
        }
    }
}
