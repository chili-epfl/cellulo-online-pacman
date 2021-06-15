using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using RoundRobin;
using UnityEngine;
using UnityEngine.UI;

public sealed class MenuController : MonoBehaviourPunCallbacks
{

    //========================================================================
    // UI Constants/Variables

    [Header("Main Panel")]

    [SerializeField] private GameObject mainPanel;

    [SerializeField] private InputField usernameInput;

    [SerializeField] private GameObject statusConnectedButton;
    [SerializeField] private GameObject statusOfflineButton;
    [SerializeField] private GameObject statusCancelButton;

    [SerializeField] private InputField roomNameInput;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;

    //------------------------------------------------------------------------
    [Header("Room Settings Panel")]

    [SerializeField] private GameObject roomSettingsPanel;
    [SerializeField] private Dropdown gamemodeDropdown;
    [SerializeField] private Dropdown mapDropdown;
    [SerializeField] private Button confirmCreateRoomButton;

    //------------------------------------------------------------------------
    [Header("Inside Room Panel")]

    [SerializeField] private GameObject insideRoomPanel;

    [SerializeField] private Text insideRoomText;
    [SerializeField] private Text playerCountText;
    [SerializeField] private GameObject leaveGameButton;
    [SerializeField] private GameObject startGameButton;

    //========================================================================
    // Other Constants/Variables
    // [Header("Other")]

    private bool gamemodeDropdownValid = false;
    private bool mapDropdownValid = false;

    // [SerializeField] private UnityEngine.Object gameScene;

    //========================================================================
    // Misc

    private RoundRobinList<Selectable> _mainSelectableRobin;

    private bool _connectedToLobby = false;

    //========================================================================
    // Automatically called methods (such as Update)

    private void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    private void Start()
    {
        if(!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

        // Setup Tab selection RoundRobin
        _mainSelectableRobin = new RoundRobinList<Selectable>(
            new List<Selectable>{
                    usernameInput, roomNameInput, createRoomButton, joinRoomButton
            });

        // Select next elem in RoundRobin
        _mainSelectableRobin.Next().Select();

        // Dropdown
        gamemodeDropdown.onValueChanged.AddListener(OnChangeGamemodeDropdownInputs);
        mapDropdown.onValueChanged.AddListener(OnChangeMapDropdownInput);
    }

    private void Update()
    {
        if (mainPanel.activeSelf) {
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _mainSelectableRobin.Next().Select();
            }

            if (Globals.DebugCreateJoinRoomShortcut && Input.GetKeyDown(KeyCode.D) &&
                (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                roomNameInput.text = "debug";
                if (!(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                {
                    usernameInput.text = "host";
                    OnChangeMainPanelInputs(); // Force rechecking if create/join buttons should be interactable
                    if (createRoomButton.interactable) {OnClickCreateRoomButton();}
                }
                else
                {
                    usernameInput.text = "player";
                    OnChangeMainPanelInputs(); // Force rechecking if create/join buttons should be interactable
                    if (joinRoomButton.interactable) {OnClickJoinRoomButton();}
                }
            }
        }
        else if (insideRoomPanel.activeSelf)
        {
            playerCountText.text = "Number of Players : " + PhotonNetwork.PlayerList.Length;
        }

    }

    // Updates status indicator when connection to PUN servers is established.
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to photon master server");
        SetConnectionStatus(statusConnectedButton);
        PhotonNetwork.JoinLobby();
        _connectedToLobby = true;
    }

    // Use to display debug information
    private void OnGUI()
    {
        // Debug Mode Below
        if (!Globals.DebugCreateJoinRoomShortcut) return;
        if (PhotonNetwork.IsConnected)
        {
            GUI.Label(new Rect(20,40,80,20),
                "Ping : " + PhotonNetwork.GetPing()+"ms" );
        }
    }

    //========================================================================
    // Button Click Handlers

    public void OnClickCreateRoomButton()
    {
        SetActiveMenuPanel(roomSettingsPanel);
        if (Globals.DebugFillRoomSettings)
        {
            gamemodeDropdown.value = (int) Globals.DebugGamemodeSelection;
            mapDropdown.value = Globals.DebugMapSelection;

            confirmCreateRoomButton.Select();
        }
    }

    public void OnClickConfirmCreateRoomButton()
    {
        Debug.Log("Creating Room " + roomNameInput.text);

        var roomOptions = new RoomOptions
        {
            IsVisible = true,
            IsOpen = true,
            MaxPlayers = 4,
            CustomRoomPropertiesForLobby =
                new[] {Globals.CustomPropertiesGamemodeKey, Globals.CustomPropertiesMapKey},
            CustomRoomProperties = new Hashtable
            {
                {Globals.CustomPropertiesGamemodeKey, gamemodeDropdown.value},
                {Globals.CustomPropertiesMapKey, mapDropdown.value}
            }
        };

        PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
        PhotonNetwork.LocalPlayer.NickName = usernameInput.text;
    }

    public void OnClickJoinRoomButton()
    {
        Debug.Log("Joining Room " + roomNameInput.text);

        PhotonNetwork.JoinRoom(roomNameInput.text);
        PhotonNetwork.LocalPlayer.NickName = usernameInput.text;
    }

    public void OnClickQuitButton()
    {
        Application.Quit();
    }

    /// Enforces that game can only be started with "valid" inputs
    public void OnChangeMainPanelInputs()
    {
        var isUsernameValid = usernameInput.text.Length >= 3;
        var isRoomNameValid = roomNameInput.text.Length >= 1;
        createRoomButton.interactable = joinRoomButton.interactable =
            PhotonNetwork.IsConnected &&
            _connectedToLobby &&
            isUsernameValid &&
            isRoomNameValid;
    }

    /// Enforces that Room Settings have "valid" inputs
    public void OnChangeGamemodeDropdownInputs(int selectionIndex)
    {
        gamemodeDropdownValid = (selectionIndex != 0);
        confirmCreateRoomButton.interactable = gamemodeDropdownValid && mapDropdownValid;
    }

    public void OnChangeMapDropdownInput(int selectionIndex)
    {
        mapDropdownValid = (selectionIndex != 0);
        confirmCreateRoomButton.interactable = gamemodeDropdownValid && mapDropdownValid;
    }

    public void OnClickLeaveRoomButton()
    {
        Debug.Log("Leaving Room");
        PhotonNetwork.LeaveRoom();

        startGameButton.SetActive(false);

        SetActiveMenuPanel(mainPanel);
    }

    public void OnClickStartGameButton()
    {
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.LoadLevel("GameScene");
    }

    //========================================================================
    // Methods called as a result of room creation (callbacks)

    public override void OnCreatedRoom()
    {
        Debug.Log("Created Room " + roomNameInput.text);
        base.OnCreatedRoom();
    }

    public override void OnCreateRoomFailed(short returnCode, string message )
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log("Room Creation Failed !");
    }

    // Callback called after joining a room
    public override void OnJoinedRoom()
    {
        SetActiveMenuPanel(insideRoomPanel);

        if(PhotonNetwork.IsMasterClient) {
            startGameButton.SetActive(true);
            startGameButton.GetComponent<Button>().Select();
            insideRoomText.text = "";
        }
        else
        {
            insideRoomText.text = "Waiting for the host to start the game";
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log("Room Join Failed !");
    }

    //========================================================================
    // "Helper Functions"

    /// <summary>
    /// Update GUI Connection status
    /// </summary>
    /// <param name="statusButton"></param> Which status button should be shown.
    private void SetConnectionStatus(GameObject statusButton)
    {
        statusOfflineButton.SetActive(statusOfflineButton.Equals(statusButton));
        statusConnectedButton.SetActive(statusConnectedButton.Equals(statusButton));
        statusCancelButton.SetActive(statusCancelButton.Equals(statusButton));
    }


    /// <summary>
    /// Set Active Menu Panel. Used to switch between different menu "pages".
    /// </summary>
    /// <param name="panel"></param> The panel to show to user.
    private void SetActiveMenuPanel(GameObject panel)
    {
        mainPanel.SetActive(mainPanel.Equals(panel));
        roomSettingsPanel.SetActive(roomSettingsPanel.Equals(panel));
        insideRoomPanel.SetActive(insideRoomPanel.Equals(panel));
    }
}
