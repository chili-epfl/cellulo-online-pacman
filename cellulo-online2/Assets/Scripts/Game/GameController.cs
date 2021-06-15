using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Globals;
using static PacManController;

/// <summary>
/// Initially intended to be a controller for all gamemodes. It is now more
/// specific to all Pac-Man game modes. Though, it could still used to accomodate
/// non Pac-Man game modes.
/// </summary>
public class GameController : MonoBehaviour
{
    [Header("General")]
    public Camera mainCamera;

    public UnityEngine.Object menuScene;

    public Text scoreText;

    public GameObject winPanel;
    public Text winPanelTimeText;

    [Header("Cellulo Entity Stuff")]

    [SerializeField] public float initialMoveSpeed;
    [SerializeField] public float initialAiMoveSpeed;

    public GameObject pacManCelluloEntityPrefab;
    public GameObject pacMan2CelluloEntityPrefab;
    public GameObject ghostCelluloEntityPrefab;

    [Header("Cellulo Visuals")]
    public Color celluloPacManColor;
    public Color celluloPacMan2Color;
    public Color celluloGhostColor;

    public Color cellulosCatchColor;
    public Color cellulosWinColor;
    public int winBlinkPeriod; // Value in milliseconds

    [Header("Game Maps")]
    [SerializeField] private GameMap gameMapSmall;
    [SerializeField] private GameMap gameMapBig;

    private List<GameMap> _gameMaps;
    private PacManController _pacManController;

    private void Awake()
    {
        if (!PhotonNetwork.InRoom)
        {
            SceneManager.LoadScene(menuScene.name);
        }

        // Initalize Cellulo Robot Communication
        if (PhotonNetwork.IsMasterClient)
            CelluloManager.TryInitialize();

        _gameMaps = new List<GameMap> {null, gameMapSmall, gameMapBig};
    }

    private void OnApplicationQuit()
    {
        CelluloManager.TryDeinitialize();
    }

    // Start is called before the first frame update
    private void Start()
    {
        var customProperties = PhotonNetwork.CurrentRoom.CustomProperties;
        var gameMode = (GameMode) (int) customProperties[CustomPropertiesGamemodeKey];

        var gameMapIndex = (int) customProperties[CustomPropertiesMapKey];
        var gameMap = ActivateMap(gameMapIndex);

        Debug.Log("Gamemode: " + gameMode);

        if (gameMode == GameMode.Pacman)
        {
            _pacManController = gameObject.AddComponent<PacManController>();
            _pacManController.Initialize(this, false, PacManMode.Adversarial, gameMap);
        }
        else if (gameMode == GameMode.PacmanVirutal)
        {
            _pacManController = gameObject.AddComponent<PacManController>();
            _pacManController.Initialize(this, true, PacManMode.Adversarial, gameMap);
        }
        else if (gameMode == GameMode.PacmanCoop)
        {
            _pacManController = gameObject.AddComponent<PacManController>();
            _pacManController.Initialize(this, false, PacManMode.CoOp, gameMap);
        }
        else if (gameMode == GameMode.PacmanCoopVirtual)
        {
            _pacManController = gameObject.AddComponent<PacManController>();
            _pacManController.Initialize(this, true, PacManMode.CoOp, gameMap);
        }
        else if (gameMode == GameMode.SpaceInvaders)
        {
            throw new NotImplementedException();
        }
        else if (gameMode == GameMode.DebugPun)
        {
            gameObject.AddComponent<DebugPunController>();
        }
        else if (gameMode == GameMode.DebugCellulo)
        {
        }
    }

    private void OnGUI()
    {
        // Debug Mode Below
        if (Globals.DebugShowPing)
            if (PhotonNetwork.IsConnected)
            {
                GUI.Label(new Rect(20, 10, 80, 20),
                    "Ping : " + PhotonNetwork.GetPing() + "ms");
            }
    }

    private GameMap ActivateMap(int mapIndex)
    {
        for (int i = 1; i < _gameMaps.Count; i++)
        {
            _gameMaps[i].gameObject.SetActive(i == mapIndex);
            // Debug.Log("Map " + _gameMaps[i].name + " active: " + (i == mapIndex));
        }

        // Adjusting Camera for to map size
        mainCamera.transform.position = _gameMaps[mapIndex].cameraCenter.position;
        if (mapIndex == 2)
        {
            mainCamera.orthographicSize = 280;
        }

        return _gameMaps[mapIndex];
    }

    //========================================================================
    // UI

    public void DisplayWinPanel(float timeToWin)
    {
        winPanel.SetActive(true);
        winPanelTimeText.text = "Time: " + Globals.FormatTime(timeToWin);
    }

    public void HideWinPanel()
    {
        winPanel.SetActive(false);
    }

    public void OnClickPlayAgain()
    {
        Debug.Log("Clicked Play Again!");
        _pacManController.RestartGame();
    }

    public void OnClickQuit()
    {
        _pacManController.DefaultCelluloLights();
        Debug.Log("Clicked Quit!");

        Application.Quit();
    }
}
