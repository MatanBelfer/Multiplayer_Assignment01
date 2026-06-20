using System;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using TMPro;
using UnityEngine.UI;

public class SessionListUI : MonoBehaviour
{
    [Header("References")] [SerializeField]
    private LobbyManager lobbyManager;

    [SerializeField] private TextMeshProUGUI lobbyStatusText;

    [Header("Lobby Panel")] [SerializeField]
    private GameObject lobbyPanel;

    [SerializeField] private TMP_InputField lobbyNameInput;
    [SerializeField] private Button connectToLobbyButton;

    [Header("Session List Panel")] [SerializeField]
    private GameObject sessionListPanel;

    [SerializeField] private GameObject createSessionPanel;
    [SerializeField] private Transform sessionListContent;
    [SerializeField] private SessionEntry sessionEntryPrefab;
    [SerializeField] private Button createSessionButton;
    [SerializeField] private Button confirmCreateButton;
    [SerializeField] private Button cancelCreateButton;
    [SerializeField] private TMP_InputField newSessionNameInput;
    [SerializeField] private TMP_InputField maxPlayersInput;
    [SerializeField] private Button leaveLobbyButton;


    private List<SessionEntry> _sessionEntries = new List<SessionEntry>();

    [Header("Player List Panel")] [SerializeField]
    private GameObject playerListPanel;

    [SerializeField] private Transform playerListContent;
    [SerializeField] private TextMeshProUGUI playerEntryPrefab;
    [SerializeField] private Button leaveSessionButton;

    private void Start()
    {
        connectToLobbyButton.onClick.AddListener(OnConnectToLobbyClicked);
        createSessionButton.onClick.AddListener(OnCreateSessionClicked);
        confirmCreateButton.onClick.AddListener(OnConfirmCreateClicked);
        cancelCreateButton.onClick.AddListener(OnCancelCreateClicked);
        leaveLobbyButton.onClick.AddListener(OnLeaveLobbyClicked);
        leaveSessionButton.onClick.AddListener(OnLeaveSessionClicked);

        ShowPanel(lobbyPanel);
    }

    private void OnEnable()
    {
        LobbyManager.OnLobbyConnectedAction += OnLobbyConnected;
        LobbyManager.OnLobbyConnectFailedAction += OnLobbyConnectFailed;
        LobbyManager.OnSessionListUpdatedAction += OnSessionListUpdated;
        LobbyManager.OnPlayerListUpdatedAction += OnPlayerListUpdated;
        LobbyManager.OnSessionJoinedAction += OnSessionJoined;
        LobbyManager.OnSessionJoinFailedAction += OnSessionJoinFailed;
        LobbyManager.OnSessionCreatedAction += OnSessionCreated;
        LobbyManager.OnSessionCreateFailedAction += OnSessionCreateFailed;
        LobbyManager.OnSessionLeftAction += OnSessionLeft;
        LobbyManager.OnLobbyDisconnectedAction += OnLobbyDisconnected;
    }

    private void OnDisable()
    {
        LobbyManager.OnLobbyConnectedAction -= OnLobbyConnected;
        LobbyManager.OnLobbyConnectFailedAction -= OnLobbyConnectFailed;
        LobbyManager.OnSessionListUpdatedAction -= OnSessionListUpdated;
        LobbyManager.OnPlayerListUpdatedAction -= OnPlayerListUpdated;
        LobbyManager.OnSessionJoinedAction -= OnSessionJoined;
        LobbyManager.OnSessionJoinFailedAction -= OnSessionJoinFailed;
        LobbyManager.OnSessionCreatedAction -= OnSessionCreated;
        LobbyManager.OnSessionCreateFailedAction -= OnSessionCreateFailed;
        LobbyManager.OnSessionLeftAction -= OnSessionLeft;
        LobbyManager.OnLobbyDisconnectedAction -= OnLobbyDisconnected;
    }


    private void OnConnectToLobbyClicked()
    {
        string lobbyName = lobbyNameInput.text.Trim();
        if (string.IsNullOrWhiteSpace(lobbyName))
        {
            lobbyStatusText.text = "Please enter a lobby name.";
            return;
        }

        connectToLobbyButton.interactable = false;
        lobbyManager.ConnectToLobby(lobbyName);
    }

    private void OnLobbyConnected()
    {
        connectToLobbyButton.interactable = true;
        ShowPanel(sessionListPanel);
    }

    private void OnLobbyConnectFailed(string reason)
    {
        connectToLobbyButton.interactable = true;
        lobbyStatusText.text = $"Failed to connect: {reason}";
        Debug.LogError($"Lobby connection failed: {reason}");
    }


    private void OnSessionListUpdated(List<SessionInfo> sessionList)
    {
        foreach (Transform child in sessionListContent)
            Destroy(child.gameObject);

        _sessionEntries.Clear();

        foreach (var session in sessionList)
        {
            SessionEntry entry = Instantiate(sessionEntryPrefab, sessionListContent);
            entry.Setup(session, JoinSession);
            _sessionEntries.Add(entry);
        }

        int totalPlayers = 0;
        foreach (var session in sessionList)
            totalPlayers += session.PlayerCount;

        lobbyStatusText.text = $"Sessions: {sessionList.Count} | Total Players: {totalPlayers}";
    }


    private void JoinSession(string sessionName)
    {
        SetSessionListButtonsInteractable(false);
        lobbyManager.JoinSession(sessionName);
    }

    private void OnSessionJoined()
    {
        Debug.Log("Session joined successfully!");
        ShowPanel(playerListPanel);

        if (lobbyManager.Runner)  
        {
            var players = new List<PlayerRef>(lobbyManager.Runner.ActivePlayers);
            players.Sort((a, b) => a.PlayerId.CompareTo(b.PlayerId));
            OnPlayerListUpdated(players);
        }
    }


    private void OnSessionJoinFailed(string reason)
    {
        SetSessionListButtonsInteractable(true);
        lobbyStatusText.text = $"Join failed: {reason}";
        Debug.LogError($"Session join failed: {reason}");
    }


    private void OnCreateSessionClicked()
    {
        ShowPanel(createSessionPanel);
    }

    private void OnConfirmCreateClicked()
    {
        string sessionName = newSessionNameInput.text.Trim();
        if (string.IsNullOrWhiteSpace(sessionName))
        {
            lobbyStatusText.text = "Please enter a session name.";
            return;
        }

        string maxPlayersText = maxPlayersInput.text.Trim();
        if (string.IsNullOrWhiteSpace(maxPlayersText))
        {
            lobbyStatusText.text = "Please enter max players.";
            return;
        }

        int maxPlayers;
        if (!int.TryParse(maxPlayersText, out maxPlayers) || maxPlayers <= 0 || maxPlayers > 20)
        {
            lobbyStatusText.text = "Max players must be a positive number and under 20.";
            return;
        }

        confirmCreateButton.interactable = false;
        cancelCreateButton.interactable = false;
        lobbyManager.CreateSession(sessionName, maxPlayers);
    }

    private void OnCancelCreateClicked()
    {
        ShowPanel(sessionListPanel);
    }

    private void OnSessionCreated()
    {
        Debug.Log("Session created successfully!");
        confirmCreateButton.interactable = true;
        cancelCreateButton.interactable = true;
        ShowPanel(playerListPanel);

        if (lobbyManager.Runner)  
        {
            var players = new List<PlayerRef>(lobbyManager.Runner.ActivePlayers);
            players.Sort((a, b) => a.PlayerId.CompareTo(b.PlayerId));
            OnPlayerListUpdated(players);
        }
    }

    private void OnSessionCreateFailed(string reason)
    {
        confirmCreateButton.interactable = true;
        cancelCreateButton.interactable = true;
        lobbyStatusText.text = $"Create failed: {reason}";
        Debug.LogError($"Session creation failed: {reason}");
    }


    private void OnPlayerListUpdated(List<PlayerRef> players)
    {
        foreach (Transform child in playerListContent)
            Destroy(child.gameObject);

        players.Sort((a, b) => a.PlayerId.CompareTo(b.PlayerId));

        foreach (var player in players)
        {
            TextMeshProUGUI label = Instantiate(playerEntryPrefab, playerListContent);
            label.text = $"Player {player.PlayerId}";
        }
    }

    private void OnLeaveSessionClicked()
    {
        leaveSessionButton.interactable = false; 
        lobbyManager.LeaveSession();
    }

    private void OnSessionLeft()
    {
        Debug.Log("Left session, reconnecting to lobby...");
        leaveSessionButton.interactable = true;

        foreach (Transform child in playerListContent)
            Destroy(child.gameObject);

        string lobbyName = lobbyNameInput.text.Trim();
        if (string.IsNullOrWhiteSpace(lobbyName))
        {
            ShowPanel(lobbyPanel);
            return;
        }

        ShowPanel(lobbyPanel);
        connectToLobbyButton.interactable = false;
        lobbyManager.ConnectToLobby(lobbyName);
    }
    
    private void OnLeaveLobbyClicked()
    {
        Debug.Log("Leave lobby button clicked!");
        leaveLobbyButton.interactable = false; 
        lobbyManager.DisconnectFromLobby();
    }

    private void OnLobbyDisconnected()
    {
        Debug.Log("Disconnected from lobby.");
        leaveLobbyButton.interactable = true;

      
        foreach (Transform child in sessionListContent)
            Destroy(child.gameObject);
        _sessionEntries.Clear();

        ShowPanel(lobbyPanel);
    }
    
    #region ─── Helpers ─────────────────────────────────────────

    private void ShowPanel(GameObject panelToShow)
    {
        if (lobbyPanel) lobbyPanel.SetActive(panelToShow == lobbyPanel);
        if (sessionListPanel) sessionListPanel.SetActive(panelToShow == sessionListPanel);
        if (createSessionPanel) createSessionPanel.SetActive(panelToShow == createSessionPanel);
        if (playerListPanel) playerListPanel.SetActive(panelToShow == playerListPanel);
        
        if (lobbyStatusText) lobbyStatusText.text = "";
    }

    private void SetSessionListButtonsInteractable(bool interactable)
    {
        createSessionButton.interactable = interactable;
        foreach (var entry in _sessionEntries)
        {
            entry.SetJoinButtonInteractable(interactable);
        }
    }

    #endregion
}