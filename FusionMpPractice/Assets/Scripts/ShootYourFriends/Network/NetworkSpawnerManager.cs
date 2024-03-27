using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSpawnerManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef m_playerNetworkPrefab = NetworkPrefabRef.Empty;
    [SerializeField] private Transform[] m_spawnPoints;

    private Dictionary<PlayerRef, NetworkObject> m_players = new();   //Version 2

    public void PlayerJoined(PlayerRef _playerRef)
    {
        SpawnPlayer(_playerRef);
    }

    public void PlayerLeft(PlayerRef _playerRef)
    {
        DespawnPlayer(_playerRef);
    }

    private void SpawnPlayer(PlayerRef _playerRef)
    {
        if (Runner.IsServer)
        {
            int randomSpawnPosition = Random.Range(0, m_spawnPoints.Length);
            var playerObject = Runner.Spawn(m_playerNetworkPrefab, m_spawnPoints[randomSpawnPosition].position, Quaternion.identity, _playerRef);
            //Runner.SetPlayerObject(_playerRef, playerObject);   //set IsLocalPlayerObject.
            m_players.Add(_playerRef, playerObject);     //Version 2
        }
    }

    private void DespawnPlayer(PlayerRef _playerRef)
    {
        if (Runner.IsServer)
        {
            //if (Runner.TryGetPlayerObject(_playerRef, out var playerNetworkObject))
            //{
            //    Runner.Despawn(playerNetworkObject);
            //}

            //Runner.SetPlayerObject(_playerRef, null);           //resets IsLocalPlayerObject.

            //Version 2
            if (m_players.TryGetValue(_playerRef, out var playerNetworkObject))
            {
                Runner.Despawn(playerNetworkObject);
                m_players.Remove(_playerRef);
            }
        }
    }
}