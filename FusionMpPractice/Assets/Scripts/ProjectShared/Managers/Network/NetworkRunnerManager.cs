using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Extensions;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
//using Fusion.Photon.Realtime;   //For Regions.

public class NetworkRunnerManager : MonoBehaviour, INetworkRunnerCallbacks
{
    internal string DevRoomCode { get => roomCode; }
    [SerializeField] private string roomCode;

    [SerializeField] private NetworkRunner m_networkRunnerPrefab;
    private NetworkRunner m_networkRunner;

    public async Task StartGame(GameMode mode, string _sessionCode, CancellationToken _cancellationToken)
    {
        try
        {
            roomCode = _sessionCode;

            if (m_networkRunner == null)
            {
                m_networkRunner = Instantiate(m_networkRunnerPrefab, transform);    //On Transform attached to the GameObject in Unity.
                m_networkRunner.AddCallbacks(this);                                 //'AddCallbacks' & 'ProvideInput' enable access to Callbacks.
                m_networkRunner.ProvideInput = true;
            }

            var tryingHost = mode == GameMode.Host;
            //Debug.Log(tryingHost ? $"Starting as host with {roomCode}." : $"Starting as client with {roomCode}.");

            var result = await m_networkRunner.StartGame(new StartGameArgs()
            {
                GameMode = mode,
                SessionName = _sessionCode,
                PlayerCount = 4,    //2 x 2 Tag Team Max for a personal Game.
                Scene = (int)EGameScene.MainGame,
                SceneManager = m_networkRunner.GetComponent<NetworkSceneManagerDefault>()
            }).WithCancellation(_cancellationToken);

            if (result.Ok)
            {
                Debug.Log($"Starting with {_sessionCode}.");
                Debug.Log(tryingHost ? "Game started successfully by the NetworkManager." : "Game joined successfully by the NetworkManager.");
            }
            else
            {
                Debug.LogError($"Game failed to " + (tryingHost ? "start" : "join") + "with result" + result.ShutdownReason);
            }
        }
        catch (OperationCanceledException)
        {
            Debug.Log("StartGame operation was canceled.");
            OnStartCanceled();
        }
        catch (Exception _exception)
        {
            Debug.Log($"An error occurred during StartGame {_exception.Message}");
        }
    }

    private void OnStartCanceled()
    {
        if (m_networkRunner)
        {
            m_networkRunner.Shutdown();
            m_networkRunner = null;
        }
    }

    #region Currently unused INetworkRunnerCallbacks
    public void OnConnectedToServer(NetworkRunner runner)
    {

    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {

    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {

    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {

    }

    public void OnDisconnectedFromServer(NetworkRunner runner)
    {

    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {

    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {

    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {

    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {

    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {

    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data)
    {

    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {

    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {

    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {

    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
//#if UNITY_EDITOR
//        Debug.Log("OnShutdown called.");
//#endif
//        SceneManager.LoadScene((int)EGameScene.MainMenu);
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {

    }
    #endregion
}