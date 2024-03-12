using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameUIScreen : MonoBehaviour
{
    [SerializeField] GameMenu gameMenu;

    private float maxSpeedX = 0f;
    private float maxSpeedY = 0f;
    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (gameMenu.gameObject.activeSelf)
            {
                gameMenu.gameObject.SetActive(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                CinemachineFreeLook freelook = FindFirstObjectByType<CinemachineFreeLook>();
                freelook.m_XAxis.m_MaxSpeed = maxSpeedX;
                freelook.m_YAxis.m_MaxSpeed = maxSpeedY;
            }
            else
            {
                gameMenu.gameObject.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                CinemachineFreeLook freelook = FindFirstObjectByType<CinemachineFreeLook>();
                maxSpeedX = freelook.m_XAxis.m_MaxSpeed;
                maxSpeedY = freelook.m_YAxis.m_MaxSpeed;
                freelook.m_XAxis.m_MaxSpeed = 0f;
                freelook.m_YAxis.m_MaxSpeed = 0f;
            }
        }
    }
}
