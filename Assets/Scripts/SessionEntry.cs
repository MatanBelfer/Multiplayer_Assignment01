using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System;

public class SessionEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI sessionNameText;
    [SerializeField] private TextMeshProUGUI playerCountText;
    [SerializeField] private Button joinButton;

    private Action<string> _onJoinClicked;
    private string _sessionName;

    public void Setup(SessionInfo session, Action<string> onJoin)
    {
        _sessionName = session.Name;
        _onJoinClicked = onJoin;

        sessionNameText.text = session.Name;
        playerCountText.text = $"{session.PlayerCount} / {session.MaxPlayers}";
        joinButton.interactable = session.PlayerCount < session.MaxPlayers;
        
        joinButton.onClick.RemoveAllListeners();
        joinButton.onClick.AddListener(() => _onJoinClicked?.Invoke(_sessionName));
    }
}