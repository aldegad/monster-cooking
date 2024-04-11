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
    [SerializeField] private GameScene _gameScene = GameScene.MainMenuScene;

    [Header("Databases")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private BuildingManager buildingManager;

    [Header("Internal State")]
    [SerializeField] private PlayerFollowCamera playerFollowCamera;
    [SerializeField] private GameState _gameState = GameState.Menu;

    public static GameManager Instance { get; private set; }
    public GameScene gameScene { get { return _gameScene; } private set { _gameScene = value; }}
    public GameState gameState { get { return _gameState; } set { _gameState = value; }}
    

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode)
    {
        GameScene gameScene;
        Enum.TryParse(scene.name, out gameScene);
        this.gameScene = gameScene;
    }

    public void StartSingle()
    { 
        ServerManager.Instance.StartSingle();
        StartGame();
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
        NetworkManager.Singleton.SceneManager.LoadScene(GameScene.GameScene.ToString(), LoadSceneMode.Single);
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
        if (gameState == GameState.Exploration || gameState == GameState.Building)
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
        if (gameState == GameState.Exploration || gameState == GameState.Building)
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
        if (gameState == GameState.Exploration || gameState == GameState.Building)
        {
            return Input.GetKeyDown(keyCode);
        }
        else
        {
            return false;
        }
    }
}


[SerializeField]
public enum GameScene
{ 
    MainMenuScene,
    GameScene
}

[Serializable]
public enum GameState
{
    Exploration,
    BuildingUI,
    Building,
    Menu
}