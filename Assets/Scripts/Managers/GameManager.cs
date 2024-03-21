using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Cinemachine;

public class GameManager : NetworkBehaviour
{
    [SerializeField] public LoadingScreen loadingScreen;

    [Header("Scenes")]
    [SerializeField] public string mainMenuScene = "MainMenuScene";
    [SerializeField] public string gameScene = "GameScene";

    [Header("Databases")]
    [SerializeField] private GameObject playerPrefab;

    [Header("Auto Settings")]
    [SerializeField] private PlayerFollowCamera playerFollowCamera;
    [SerializeField] private GameState gameState = GameState.Menu;


    public static GameManager Instance { get; private set; }

    public GameState GameState
    {
        get { return gameState; }
        set { gameState = value; }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public async void StartHost()
    {
        loadingScreen.gameObject.SetActive(true);
        await ServerManager.Instance.StartHost();
        StartGame();
    }

    public Task<bool> StartClient(string joinCode)
    {
        loadingScreen.gameObject.SetActive(true);
        return ServerManager.Instance.StartClient(joinCode);
    }

    public void StartGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(gameScene, LoadSceneMode.Single);
    }

    public void SetCamera(PlayerFollowCamera playerFollowCamera)
    {
        this.playerFollowCamera = playerFollowCamera;
    }
    public void ResumeCamera()
    {
        playerFollowCamera.freeLookCam.m_XAxis.m_MaxSpeed = playerFollowCamera.maxSpeedX;
        playerFollowCamera.freeLookCam.m_YAxis.m_MaxSpeed = playerFollowCamera.maxSpeedY;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void PauseCamera()
    {
        playerFollowCamera.freeLookCam.m_XAxis.m_MaxSpeed = 0;
        playerFollowCamera.freeLookCam.m_YAxis.m_MaxSpeed = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public float Input_GetAxis_Player(string axisName)
    {
        if (GameState == GameState.Exploration || GameState == GameState.Building)
        {
            return Input.GetAxis(axisName);
        }
        else
        {
            return 0f;
        }
    }

    public bool Input_GetButton_Player(string buttonName)
    {
        if (GameState == GameState.Exploration || GameState == GameState.Building)
        {
            return Input.GetButton(buttonName);
        }
        else
        {
            return false;
        }
    }

    public bool Input_GetKeyDown_Player(KeyCode keyCode)
    {
        if (GameState == GameState.Exploration || GameState == GameState.Building)
        {
            return Input.GetKeyDown(keyCode);
        }
        else
        {
            return false;
        }
    }
}


[Serializable]
public enum GameState
{
    Exploration,
    BuildingUI,
    Building,
    Menu
}