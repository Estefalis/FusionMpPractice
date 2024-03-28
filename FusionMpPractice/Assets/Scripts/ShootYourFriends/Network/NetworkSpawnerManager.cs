using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSpawnerManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft   //Just a different name for NetworkSpawnerController in Guide.
{
    [SerializeField] private NetworkPrefabRef m_playerNetworkPrefab;
    [SerializeField] private Transform[] m_spawnPointsArray;
    private List<Transform> m_spawnPointsList;

    private Dictionary<PlayerRef, NetworkObject> m_players = new();   //Version 2

    private void Awake()
    {
        m_spawnPointsList = new List<Transform>();
    }

    public void PlayerJoined(PlayerRef _playerRef)
    {
        SpawnPlayer(_playerRef);
    }

    private void SpawnPlayer(PlayerRef _playerRef)
    {
        if (Runner.IsServer)
        {
            int randomSpawnPosition = Random.Range(0, m_spawnPointsArray.Length);

            foreach (Transform transform in m_spawnPointsList)
            {
                if (m_spawnPointsArray[randomSpawnPosition].position == transform.position)
                    randomSpawnPosition += 1 % m_spawnPointsArray.Length;
            }

            var playerObject = Runner.Spawn(m_playerNetworkPrefab, m_spawnPointsArray[randomSpawnPosition].position, Quaternion.identity, _playerRef);
            m_spawnPointsList.Add(m_spawnPointsArray[randomSpawnPosition]);
            m_players.Add(_playerRef, playerObject);     //Version 2
        }
    }

    public void PlayerLeft(PlayerRef _playerRef)
    {
        DespawnPlayer(_playerRef);
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