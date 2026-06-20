using System;
using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;

public class LobbyManager : MonoBehaviour, INetworkRunnerCallbacks
{
    // Session list update (while in lobby)
    public static Action<List<SessionInfo>> OnSessionListUpdatedAction;

    // Player list update (while in a session)
    public static Action<List<PlayerRef>> OnPlayerListUpdatedAction;

    // Lobby connection results
    public static Action OnLobbyConnectedAction;
    public static Action<string> OnLobbyConnectFailedAction;

    // Session join/create results
    public static Action OnSessionJoinedAction;
    public static Action<string> OnSessionJoinFailedAction;
    public static Action OnSessionCreatedAction;
    public static Action<string> OnSessionCreateFailedAction;

    // Session/Lobby leave/disconnect results
    public static Action OnSessionLeftAction;
    public static Action OnLobbyDisconnectedAction;


    public NetworkRunner Runner => _runner;

    [SerializeField] private NetworkRunner _runner;

    public async void ConnectToLobby(string lobbyName)
    {
        if (!_runner)
        {
            Debug.LogError("[Lobby] No NetworkRunner found!");
            OnLobbyConnectFailedAction?.Invoke("No NetworkRunner found");
            return;
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
            OnLobbyConnectFailedAction?.Invoke(result.ShutdownReason.ToString());
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
        {
            Debug.Log($"[Session] Successfully joined session: '{sessionName}'");
            OnSessionJoinedAction?.Invoke();
        }
        else
        {
            Debug.LogError($"[Session] Failed to join session '{sessionName}': {result.ShutdownReason}");
            OnSessionJoinFailedAction?.Invoke(result.ShutdownReason.ToString());
        }
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
        {
            Debug.Log($"[Session] Successfully created session: '{sessionName}'");
            OnSessionCreatedAction?.Invoke();
        }
        else
        {
            Debug.LogError($"[Session] Failed to create session '{sessionName}': {result.ShutdownReason}");
            OnSessionCreateFailedAction?.Invoke(result.ShutdownReason.ToString());
        }
    }

    public async void LeaveSession()
    {
        if (!_runner) return;  
        Debug.Log("[Session] Leaving session...");
        await _runner.Shutdown();
        OnSessionLeftAction?.Invoke();
    }

    public async void DisconnectFromLobby()
    {
        if (!_runner) return;  
        Debug.Log("[Lobby] Disconnecting from lobby...");
        await _runner.Shutdown();
        OnLobbyDisconnectedAction?.Invoke();
    }

    private void RecreateRunner()
    {
        if (_runner)
        {
            Destroy(_runner.gameObject);
        }

        _runner = new GameObject("NetRunner").AddComponent<NetworkRunner>();
    }


    #region ── INetworkRunnerCallbacks ──────────────────────────────────────────────

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Session] Player joined: {player}");
        OnPlayerListUpdatedAction?.Invoke(new List<PlayerRef>(runner.ActivePlayers));
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"[Session] Player left: {player}");
        OnPlayerListUpdatedAction?.Invoke(new List<PlayerRef>(runner.ActivePlayers));
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log($"[Lobby] OnSessionListUpdated — {sessionList.Count} session(s)");
        foreach (var session in sessionList)
            Debug.Log(
                $"[Lobby]   -> '{session.Name}' | {session.PlayerCount}/{session.MaxPlayers} | Open: {session.IsOpen}");

        OnSessionListUpdatedAction?.Invoke(sessionList);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"[Runner] OnShutdown — Reason: {shutdownReason}. Recreating runner...");


        RecreateRunner();
    }

    #endregion

    #region ── Unused callbacks  ─────────────────────────────

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
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

    #endregion
}