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


    [Header("Player List Panel")] [SerializeField]
    private GameObject playerListPanel;

    [SerializeField] private Transform playerListContent;
    [SerializeField] private TextMeshProUGUI playerEntryPrefab;

    private void Start()
    {
        connectToLobbyButton.onClick.AddListener(OnConnectToLobbyClicked);
        createSessionButton.onClick.AddListener(OnCreateSessionClicked);
        confirmCreateButton.onClick.AddListener(OnConfirmCreateClicked);
        cancelCreateButton.onClick.AddListener(OnCancelCreateClicked);

        ShowPanel(lobbyPanel);
    }

    private void OnEnable()
    {
        LobbyManager.OnSessionListUpdatedAction += OnSessionListUpdated;
        LobbyManager.OnPlayerListUpdatedAction += OnPlayerListUpdated;
        LobbyManager.OnLobbyConnectedAction += OnLobbyConnected; // ← subscribe
    }

    private void OnDisable()
    {
        LobbyManager.OnSessionListUpdatedAction -= OnSessionListUpdated;
        LobbyManager.OnPlayerListUpdatedAction -= OnPlayerListUpdated;
        LobbyManager.OnLobbyConnectedAction -= OnLobbyConnected; // ← unsubscribe
    }

    private void OnConnectToLobbyClicked()
    {
        string lobbyName = lobbyNameInput.text.Trim();
        if (string.IsNullOrEmpty(lobbyName)) return;

        lobbyManager.ConnectToLobby(lobbyName);
    }

    private void OnSessionListUpdated(List<SessionInfo> sessionList)
    {
        foreach (Transform child in sessionListContent)
            Destroy(child.gameObject);

        foreach (var session in sessionList)
        {
            SessionEntry entry = Instantiate(sessionEntryPrefab, sessionListContent);
            entry.Setup(session, JoinSession);
        }
    }

    private void JoinSession(string sessionName)
    {
        lobbyManager.JoinSession(sessionName);
        ShowPanel(playerListPanel);
    }

    private void OnCreateSessionClicked() => ShowPanel(createSessionPanel);

    private void OnConfirmCreateClicked()
    {
        string sessionName = newSessionNameInput.text.Trim();
        if (string.IsNullOrEmpty(sessionName)) return;

        int maxPlayers = int.Parse(maxPlayersInput.text) <= 0 ? 4 : int.Parse(maxPlayersInput.text);

        lobbyManager.CreateSession(sessionName, maxPlayers);
        ShowPanel(playerListPanel);
    }

    private void OnCancelCreateClicked() => ShowPanel(sessionListPanel);

    private void OnPlayerListUpdated(List<PlayerRef> players)
    {
        foreach (Transform child in playerListContent)
            Destroy(child.gameObject);

        foreach (var player in players)
        {
            TextMeshProUGUI label = Instantiate(playerEntryPrefab, playerListContent);
            label.text = $"Player {player}";
        }
    }

    private void OnLobbyConnected() 
    {
        ShowPanel(sessionListPanel);
        
    }

    private void ShowPanel(GameObject panelToShow)
    {
        lobbyPanel.SetActive(panelToShow == lobbyPanel);
        sessionListPanel.SetActive(panelToShow == sessionListPanel);
        createSessionPanel.SetActive(panelToShow == createSessionPanel);
        playerListPanel.SetActive(panelToShow == playerListPanel);
    }
}