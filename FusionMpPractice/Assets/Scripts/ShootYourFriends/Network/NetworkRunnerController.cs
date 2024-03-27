using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkRunnerController : MonoBehaviour, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner m_networkRunnerPrefab;
    private NetworkRunner m_networkRunnerInstance;

    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        Debug.Log($"OnConnectFailed");
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
        Debug.Log($"OnConnectRequest");
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
        Debug.Log($"OnCustomAuthenticationResponse");
    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {
        Debug.Log($"OnDisconnectedFromServer");
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
        Debug.Log($"OnHostMigration");
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        Debug.Log($"OnInput");
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
        Debug.Log($"OnInputMissing");
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"OnPlayerJoined");
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"OnPlayerLeft");
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {
        Debug.Log($"OnReliableDataReceived");
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
        Debug.Log($"OnSceneLoadDone");
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
        Debug.Log($"OnSceneLoadStart");
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        Debug.Log($"OnSessionListUpdated");
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"OnShutdown");
        const string LOBBY_SCENE = "Lobby";
        SceneManager.LoadScene(LOBBY_SCENE);
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
        Debug.Log($"OnUserSimulationMessage");
    }

    public async void StartGame(GameMode _gameMode, string _roomName)
    {
        if (m_networkRunnerInstance == null)
        {
            m_networkRunnerInstance = Instantiate(m_networkRunnerPrefab);    //On Transform attached to the GameObject in Unity.
            m_networkRunnerInstance.AddCallbacks(this);                      //'AddCallbacks' & 'ProvideInput' enable access to Callbacks.
            m_networkRunnerInstance.ProvideInput = true;
        }

        var tryingHost = _gameMode == GameMode.Host;
        Debug.Log(tryingHost ? $"Starting as host." : $"Starting as client.");

        var startGameArg = new StartGameArgs()
        {
            GameMode = _gameMode,
            SessionName = _roomName,
            PlayerCount = 4,
            SceneManager = m_networkRunnerPrefab.GetComponent<INetworkSceneManager>()
        };

        var result = await m_networkRunnerInstance.StartGame(startGameArg);

        if (result.Ok)
        {
            Debug.Log(tryingHost ? "Game started successfully" : "Game join successfully");
            const string SCENE_NAME = "MainGame";
            m_networkRunnerInstance.SetActiveScene(SCENE_NAME);
        }
        else
        {
            Debug.LogError($"Game failed to " + (tryingHost ? "start" : "join") + "with result" + result.ShutdownReason);
        }
    }
}
