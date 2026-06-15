using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class SessionListUI : MonoBehaviour
{
    [SerializeField] private LobbyManager lobbyManager;
    [SerializeField] private SessionEntry sessionEntryPrefab;
    [SerializeField] private Transform contentParent;

    private void OnEnable() => LobbyManager.OnSessionListUpdatedAction += UpdateUI;
    private void OnDisable() => LobbyManager.OnSessionListUpdatedAction -= UpdateUI;

    private void UpdateUI(List<SessionInfo> sessionList)
    {
        foreach (Transform child in contentParent) Destroy(child.gameObject);

        foreach (var session in sessionList)
        {
            SessionEntry entry = Instantiate(sessionEntryPrefab, contentParent);
            entry.Setup(session, (name) => lobbyManager.JoinSession(name));
        }
    }
}