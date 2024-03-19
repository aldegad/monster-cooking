using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerFollowCamera : MonoBehaviour
{
    [Header("Camera Default Setting")]
    [SerializeField] public CinemachineFreeLook freeLookCam;
    [SerializeField] public float maxSpeedX = 500f;
    [SerializeField] public float maxSpeedY = 4f;
    [SerializeField] private float topRigHeight = 3.17f, topRigRadius = 1.67f;
    [SerializeField] private float middleRigHeight = 0.9f, middleRigRadius = 2.09f;
    [SerializeField] private float bottomRigHeight = 0.04f, bottomRigRadius = 0.88f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        freeLookCam = gameObject.GetComponent<CinemachineFreeLook>();
    }

    

    void Start()
    {
        // set speed
        freeLookCam.m_XAxis.m_MaxSpeed = maxSpeedX;
        freeLookCam.m_YAxis.m_MaxSpeed = maxSpeedY;

        // Top Rig 설정
        freeLookCam.m_Orbits[0].m_Radius = topRigRadius;
        freeLookCam.m_Orbits[0].m_Height = topRigHeight;
        freeLookCam.GetRig(0). GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XDamping = 0;
        freeLookCam.GetRig(0).GetCinemachineComponent<CinemachineOrbitalTransposer>().m_YDamping = 0;
        freeLookCam.GetRig(0).GetCinemachineComponent<CinemachineOrbitalTransposer>().m_ZDamping = 0;
        freeLookCam.GetRig(0).GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.35f;
        freeLookCam.GetRig(0).GetCinemachineComponent<CinemachineComposer>().m_ScreenY = 0.6f;

        // Middle Rig 설정
        freeLookCam.m_Orbits[1].m_Radius = middleRigRadius;
        freeLookCam.m_Orbits[1].m_Height = middleRigHeight;
        freeLookCam.GetRig(1).GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XDamping = 0;
        freeLookCam.GetRig(1).GetCinemachineComponent<CinemachineOrbitalTransposer>().m_YDamping = 0;
        freeLookCam.GetRig(1).GetCinemachineComponent<CinemachineOrbitalTransposer>().m_ZDamping = 0;
        freeLookCam.GetRig(1).GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.35f;
        freeLookCam.GetRig(1).GetCinemachineComponent<CinemachineComposer>().m_ScreenY = 0.6f;

        // Bottom Rig 설정
        freeLookCam.m_Orbits[2].m_Radius = bottomRigRadius;
        freeLookCam.m_Orbits[2].m_Height = bottomRigHeight;
        freeLookCam.GetRig(2).GetCinemachineComponent<CinemachineOrbitalTransposer>().m_XDamping = 0;
        freeLookCam.GetRig(2).GetCinemachineComponent<CinemachineOrbitalTransposer>().m_YDamping = 0;
        freeLookCam.GetRig(2).GetCinemachineComponent<CinemachineOrbitalTransposer>().m_ZDamping = 0;
        freeLookCam.GetRig(2).GetCinemachineComponent<CinemachineComposer>().m_ScreenX = 0.35f;
        freeLookCam.GetRig(2).GetCinemachineComponent<CinemachineComposer>().m_ScreenY = 0.6f;
    }
}