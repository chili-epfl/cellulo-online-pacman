using System;
using System.Collections.Generic;
using System.Linq;
using Navigation;
using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;
using static CelluloEnums.VisualEffect;

/// <summary>
/// Note: Is meant to be attached to same GameObject as GameController
/// </summary>
public class PacManController : MonoBehaviourPun
{
    //------------------------------------------------------------------------
    // Debug

    private Logger _logger;

    //------------------------------------------------------------------------
    // Game Logic Fields

    private CelluloEntity _localPlayer;
    private CelluloEntity _remotePlayer;
    private CelluloEntity _aiPlayer;

    private List<CelluloEntity> _celluloEntities;

    private CelluloEntity _caughtPacMan;

    private Vector2 _localInputAxes = Vector2.zero;
    private Vector2 _remoteInputAxes = Vector2.zero;

    private int _score;
    private int _timesCaught = 0;

    private readonly List<Collectible> _collectedCollectibles = new List<Collectible>();

    private GameState _gameState = GameState.Ready;
    private enum GameState
    {
        Ready,
        Active,
        Caught,
        Win,
    }

    private float _gameStartTime;

    //------------------------------------------------------------------------
    // AI Chasing
    private AiChasingMode _aiChasingMode;

    private enum AiChasingMode
    {
        None,
        ShortestPath,
    }

    private List<GameNode> _navNodes;

    private GameNode _previousClosestNodeToLocal;
    private GameNode _previousClosestNodeToRemote;

    //------------------------------------------------------------------------
    // Game Map

    private List<Transform> _spawns;

    //------------------------------------------------------------------------
    // Initializable fields

    // Used to make sure Initialize() is called before using this controller
    private bool _initialized;

    private GameController _gameController;

    // Does the game use real cellulos or Unity Rigidbody2D for movement
    private bool _isGameVirtual;
    private PacManMode _pacManMode;

    public enum PacManMode
    {
        Adversarial,
        CoOp
    }

    //------------------------------------------------------------------------

    public void Initialize(GameController gameController, bool isGameVirtual, PacManMode pacManMode, GameMap gameMap)
    {
        if (!_initialized)
            _initialized = true;
        else
            throw new InvalidOperationException("Can only call initialize() once!");

        _isGameVirtual = isGameVirtual;
        _gameController = gameController;
        _pacManMode = pacManMode;

        _navNodes = gameMap.navNodesParent.GetComponentsInChildren<GameNode>().ToList();
        _spawns = gameMap.spawns.GetComponentsInChildren<Transform>().ToList();
    }

    //========================================================================
    // Callbacks for Cellulo Entities (all "assigned" in Start())

    /// This is a callback is for collecting collectibles (Apples) and updating score
    /// This is meant to be called by OnTriggerEnter2D on the Pac-Man CelluloEntity(ies).
    private void CollectApple(Collider2D other)
    {
        // Get the Collectible script attached to GameObject.
        // if the GameObject doesn't have a Collectible script attached to it
        // its not a collectible. So we just return.
        var collectible = other.GetComponent<Collectible>();
        if (collectible == null) return;

        if (_gameState == GameState.Ready)
        {
            // Start Game (when 1st apple gets collected)
            ChangeGameState(GameState.Active);
            _gameStartTime = Time.realtimeSinceStartup;
            Debug.Log("Game started at time : " + _gameStartTime);
        }

        if (_gameState != GameState.Active)
            return;

        // Start Ghost chasing
        if (_pacManMode == PacManMode.CoOp)
            _aiChasingMode = AiChasingMode.ShortestPath;

        collectible.Collect();
        _collectedCollectibles.Add(collectible);
        UpdateScore();

        // If score is 6 it means all Apples were collected and game is won!
        if (_score == 6)
        {
            if (_aiChasingMode != AiChasingMode.None)
            {
                AiStopChasing();
            }

            ChangeGameState(GameState.Win);
        }
    }

    /// Callback for catching the Pac-Man
    /// Is meant to be called by OnCollisionEnter2D of the Ghost CelluloEntity.
    private void CatchPacMan(Collision2D collision)
    {
        if (_gameState != GameState.Active)
            return;

        var caughtCelluloEntity = collision.gameObject.GetComponent<CelluloEntity>();
        if (caughtCelluloEntity != null && _caughtPacMan == null)
        {
            _caughtPacMan = caughtCelluloEntity;
            ChangeGameState(GameState.Caught);

            Debug.Log("YOU GOT CAUGHT!");
        }
    }

    /// Callback for when Pac-Man is no longer "caught"
    /// Is meant to be called by OnCollisionExit2D of the Ghost CelluloEntity.
    private void UncatchPacMan(Collision2D collision)
    {
        // The other cellulo Entity
        var uncaughtCelluloEntity = collision.gameObject.GetComponent<CelluloEntity>();
        if (uncaughtCelluloEntity != null && uncaughtCelluloEntity == _caughtPacMan)
        {
            _caughtPacMan = null;

            ChangeGameState(GameState.Active);

            Debug.Log("Not Caught Anymore!");
        }
    }

    private void AiStopChasing()
    {
        _aiPlayer.SetDirectionalInput(Vector2.zero);
        _aiChasingMode = AiChasingMode.None;
    }

    //========================================================================
    // Event Functions

    private void Start()
    {
        if (!_initialized)
            throw new InvalidOperationException("Must call initialize() upon object creation");

        SetScore(0);

        // Set Photon update rates (to higher values than default to make game smoother)
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;

        if (!PhotonNetwork.IsMasterClient) return;

        // Setup all the Cellulo Entities according to how the chosen Pac-Man game mode.
        if (_pacManMode == PacManMode.Adversarial) {
            //----------------------------------------------------------------
            // Setup Local Player (The Pac-Man)

            var localPlayerGameObject =
                PhotonNetwork.Instantiate(
                    _gameController.pacManCelluloEntityPrefab.name,
                    new Vector2(200, -100),
                    Quaternion.identity);

            _localPlayer = localPlayerGameObject.AddComponent<CelluloEntity>();
            _localPlayer.Initialize(
                _isGameVirtual,
                _gameController.initialMoveSpeed,
                _gameController.celluloPacManColor,
                CollectApple // Pass the CollectApple method to the Pac-Man CelluloEntity.
                             // Will be called when OnTriggerEnter2D is called.
            );
            _localPlayer.gameObject.name += "(local)";

            //----------------------------------------------------------------
            // Setup Remote Player (The Ghost)

            var remotePlayerGameObject =
                PhotonNetwork.Instantiate(
                    _gameController.ghostCelluloEntityPrefab.name,
                    new Vector2(300, -360),
                    Quaternion.identity);
            _remotePlayer = remotePlayerGameObject.AddComponent<CelluloEntity>();
            _remotePlayer.Initialize(
                _isGameVirtual,
                _gameController.initialMoveSpeed,
                _gameController.celluloGhostColor,
                null,
                null,
                CatchPacMan,
                UncatchPacMan
            );
            _remotePlayer.LightsDefault();

            remotePlayerGameObject.name += "(remote)";

            //----------------------------------------------------------------

            _celluloEntities = new List<CelluloEntity> {_localPlayer, _remotePlayer};
        }
        else if (_pacManMode == PacManMode.CoOp)
        {
            //----------------------------------------------------------------
            // Setup Local Player (The Pac-Man)

            var localPlayerGameObject =
                PhotonNetwork.Instantiate(
                    _gameController.pacManCelluloEntityPrefab.name,
                    new Vector2(200, -100),
                    Quaternion.identity);

            _localPlayer = localPlayerGameObject.AddComponent<CelluloEntity>();
            _localPlayer.Initialize(
                _isGameVirtual,
                _gameController.initialMoveSpeed,
                _gameController.celluloPacManColor,
                CollectApple
            );
            _localPlayer.gameObject.name += "(local)";

            //----------------------------------------------------------------
            // Setup Remote Player (The Pac-Man 2)

            var remotePlayerGameObject =
                PhotonNetwork.Instantiate(
                    _gameController.pacMan2CelluloEntityPrefab.name,
                    new Vector2(200, -360),
                    Quaternion.identity);

            _remotePlayer = remotePlayerGameObject.AddComponent<CelluloEntity>();
            _remotePlayer.Initialize(
                _isGameVirtual,
                _gameController.initialMoveSpeed,
                _gameController.celluloPacMan2Color,
                CollectApple
            );
            _remotePlayer.gameObject.name += "(remote)";

            //----------------------------------------------------------------
            // Setup AI (The Ghost)

            var aiPlayerGameObject =
                PhotonNetwork.Instantiate(
                    _gameController.ghostCelluloEntityPrefab.name,
                    new Vector2(500, -300),
                    Quaternion.identity);
            _aiPlayer = aiPlayerGameObject.AddComponent<CelluloEntity>();
            _aiPlayer.Initialize(
                _isGameVirtual,
                _gameController.initialAiMoveSpeed,
                _gameController.celluloGhostColor,
                null,
                null,
                x => {
                    // Catch Pac-Man and stop AI chasing
                    CatchPacMan(x);
                    AiStopChasing();
                },
                UncatchPacMan
            );
            _aiPlayer.LightsDefault();

            aiPlayerGameObject.name += "(ai)";
            //----------------------------------------------------------------

            _celluloEntities = new List<CelluloEntity> {_localPlayer, _remotePlayer, _aiPlayer};
        }

        ChangeGameState(GameState.Ready);

        _logger = gameObject.AddComponent<Logger>();
    }

    private int _frame = 0; // Default Unity frame rate is 60fps

    /// <summary>
    /// Controls how often keyboard inputs are sent over Photon. This could be 1 with no issue.
    /// </summary>
    private const int PunSendFrequency = 3;

    private void Update()
    {
        //--------------------------------------------------------------------
        // Communication
        var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (PhotonNetwork.IsMasterClient)
        {
            _localInputAxes = input;

            if (Globals.DebugShiftToControlPlayer2 &&
                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                _remoteInputAxes = input;
            }
        }
        else
        {
            if (_frame % PunSendFrequency == 0)
               UpdateInputAxes(input);
        }
        //--------------------------------------------------------------------
        // Cellulo/Virtual cellulo interfacing

        if (PhotonNetwork.IsMasterClient)
        {
            // Allows controlling Cellulo Entity of local player with keyboard.
            // Disable this for games with real Cellulo to not mess with
            // the resistance the robot has to movement by hand (by default).
            if (Globals.DebugAllowLocalControlOfPlayer)
                _localPlayer.SetDirectionalInputRestricted(_localInputAxes);

            _remotePlayer.SetDirectionalInputRestricted(_remoteInputAxes);
        }

        //--------------------------------------------------------------------
        // Misc
        if (_frame++ >= 60)
        {
            _frame = 0;
        }
        //--------------------------------------------------------------------
    }

    private void FixedUpdate()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (_pacManMode == PacManMode.CoOp)
        {
            // All the AI chasing logic in this if condition below.
            // It consists of calculating the shortest paths to each robot.
            // Then telling the Ghost to chase the closest Pac-Man.
            if (_aiChasingMode == AiChasingMode.ShortestPath)
            {
                if (_navNodes == null)
                    throw new InvalidOperationException("Nav Nodes are null!");

                var (closestNodeToLocalPlayer, _) =
                    Navigator.FindClosestNode(_navNodes, _localPlayer.Position);

                var (closestNodeToRemotePlayer, _) =
                    Navigator.FindClosestNode(_navNodes, _remotePlayer.Position);

                if (closestNodeToLocalPlayer != _previousClosestNodeToLocal || closestNodeToRemotePlayer != _previousClosestNodeToRemote)
                {
                    var (closestNodeToAI, _) = Navigator.FindClosestNode(_navNodes, _aiPlayer.Position);

                    var shortestPathToLocal = Navigator.FindShortestPath(_navNodes, closestNodeToAI, closestNodeToLocalPlayer);
                    var shortestPathToRemote = Navigator.FindShortestPath(_navNodes, closestNodeToAI, closestNodeToRemotePlayer);

                    var distToLocalPlayer = Navigator.PathLength(shortestPathToLocal);
                    var distToRemotePlayer = Navigator.PathLength(shortestPathToRemote);

                    // Debug.Log("closestNode Local : " + closestNodeToLocalPlayer.Position + "\t" + distToLocalPlayer);
                    // Debug.Log("closestNode Remote : " + closestNodeToRemotePlayer.Position + "\t" + distToRemotePlayer);

                    _aiPlayer.SetGoalPath(distToLocalPlayer < distToRemotePlayer ? shortestPathToLocal : shortestPathToRemote);
                }

                _previousClosestNodeToLocal = closestNodeToLocalPlayer;
                _previousClosestNodeToRemote = closestNodeToRemotePlayer;
            }
        }
    }

    //========================================================================
    // Game state transitions

    /// <summary>
    /// Changes the game state and performs the according changes for transition
    /// </summary>
    /// <param name="newGameState"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void ChangeGameState(GameState newGameState)
    {
        switch (newGameState)
        {
            case GameState.Ready:
                // Set all robot lights ot their default color. To allow players to find which Cellulo is theirs.
                _celluloEntities.ForEach(celluloEntity => celluloEntity.LightsDefault());
                break;
            case GameState.Active:
                UpdateScore();
                break;
            case GameState.Caught:
                _timesCaught++;
                // Drop all apples & reset score
                ResetApples();

                // Set Cellulos lighting to Catch color (Red) to signify they got caught.
                _localPlayer.SetVisualEffect(VisualEffectConstAll, _gameController.cellulosCatchColor);
                if (_pacManMode == PacManMode.CoOp)
                    _remotePlayer.SetVisualEffect(VisualEffectConstAll, _gameController.cellulosCatchColor);

                break;
            case GameState.Win:

                var timeToWin = Time.realtimeSinceStartup - _gameStartTime;
                _gameController.DisplayWinPanel(timeToWin);
                Debug.Log("You win!! - Time : " + timeToWin);

                // Write game summary to log.
                _logger.writeGameSummaryToLog(_pacManMode, timeToWin, _timesCaught);

                // Set Win blinking lights!
                var _players = new List<CelluloEntity> {_localPlayer};
                if (_pacManMode == PacManMode.CoOp)
                {
                    _players.Add(_remotePlayer);
                }

                _players.ForEach(celluloEntity =>
                celluloEntity.SetVisualEffect(
                    VisualEffectBlink,
                    _gameController.cellulosWinColor,
                    _gameController.winBlinkPeriod / 20));

                // Show GG + Time and restart button

                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newGameState), newGameState, null);
        }

        _gameState = newGameState;
    }

    //========================================================================
    // Helper methods

    public void RestartGame()
    {
        _timesCaught = 0;
        _gameController.HideWinPanel();
        ChangeGameState(GameState.Ready);
        ResetApples();
    }

    private void ResetApples()
    {
        _collectedCollectibles.ForEach(collectible => collectible.Drop());
        _collectedCollectibles.Clear();
        UpdateScore();
    }

    private void SetScore(int score)
    {
        _score = score;
        _gameController.scoreText.text = "Score : " + _score;
    }

    //========================================================================
    // Communication methods

    private void UpdateInputAxes(Vector2 inputAxes)
    {
        photonView.RPC("ReceiveInputAxesRPC", RpcTarget.All, inputAxes);
    }

    // ReSharper disable once UnusedMember.Local
    [PunRPC] private void ReceiveInputAxesRPC(Vector2 inputAxes)
    {
        _remoteInputAxes = inputAxes;
        // Debug.Log("Recived: inputAxes = " + inputAxes);
    }

    private void UpdateScore()
    {
        _localPlayer.LightsOff();
        if (_pacManMode == PacManMode.CoOp)
        {
            _remotePlayer.LightsOff();
        }

        foreach (var collectible in _collectedCollectibles)
        {
            var lightIndex = collectible.id - 1;
            _localPlayer.LightsSingle(lightIndex);
            if (_pacManMode == PacManMode.CoOp)
            {
                _remotePlayer.LightsSingle(lightIndex);
            }
        }

        var score = _collectedCollectibles.Count;
        photonView.RPC("ReceiveScoreRPC", RpcTarget.All, score);
    }

    // ReSharper disable once UnusedMember.Local
    [PunRPC] private void ReceiveScoreRPC(int score)
    {
        SetScore(score);
    }

    //========================================================================
    public void DefaultCelluloLights()
    {
        _celluloEntities.ForEach(celluloEntity => celluloEntity.LightsDefault());
    }
}
