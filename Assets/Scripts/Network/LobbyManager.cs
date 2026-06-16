using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static Action<List<SessionInfo>> OnSessionListUpdatedAction;
    public static Action<List<PlayerRef>> OnPlayerListUpdatedAction;
    public static Action OnLobbyConnectedAction; 

    [SerializeField] NetworkRunner _runner;

    private void Start()
    {
    }

    public async void ConnectToLobby(string lobbyName)
    {
        if (!_runner)
        {
            Debug.Log("No runner found");
        }

        _runner.AddCallbacks(this);

        Debug.Log($"[Lobby] Attempting to connect to lobby: '{lobbyName}'");

        StartGameResult result = await _runner.JoinSessionLobby(SessionLobby.Custom, lobbyName);

        if (result.Ok)
        {
            Debug.Log($"[Lobby] Successfully connected to lobby: '{lobbyName}'");
            OnLobbyConnectedAction?.Invoke(); 
        }
        else
        {
            Debug.LogError($"[Lobby] Failed to connect to lobby '{lobbyName}': {result.ShutdownReason}");
        }
    }

    public async void JoinSession(string sessionName)
    {
        Debug.Log($"[Session] Attempting to join session: '{sessionName}'");

        var result = await _runner.StartGame(new StartGameArgs
        {
            SessionName = sessionName,
            GameMode = GameMode.Shared,
            IsVisible = true,
            IsOpen = true
        });

        if (result.Ok)
            Debug.Log($"[Session] Successfully joined session: '{sessionName}'");
        else
            Debug.LogError($"[Session] Failed to join session '{sessionName}': {result.ShutdownReason}");
    }

    public async void CreateSession(string sessionName, int maxPlayers)
    {
        Debug.Log($"[Session] Attempting to create session: '{sessionName}' with {maxPlayers} max players");

        var result = await _runner.StartGame(new StartGameArgs
        {
            SessionName = sessionName,
            GameMode = GameMode.Shared,
            PlayerCount = maxPlayers,
            IsVisible = true,
            IsOpen = true
        });

        if (result.Ok)
            Debug.Log($"[Session] Successfully created session: '{sessionName}'");
        else
            Debug.LogError($"[Session] Failed to create session '{sessionName}': {result.ShutdownReason}");
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        OnPlayerListUpdatedAction?.Invoke(new List<PlayerRef>(_runner.ActivePlayers));
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        OnPlayerListUpdatedAction?.Invoke(new List<PlayerRef>(_runner.ActivePlayers));
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log($"[Lobby] OnSessionListUpdated fired — {sessionList.Count} session(s) received");

        foreach (var session in sessionList)
        {
            Debug.Log(
                $"[Lobby]   -> Session: '{session.Name}' | Players: {session.PlayerCount}/{session.MaxPlayers} | Open: {session.IsOpen} | Visible: {session.IsVisible}");
        }

        OnSessionListUpdatedAction?.Invoke(sessionList);
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}