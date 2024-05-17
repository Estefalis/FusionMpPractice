using Fusion;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NetworkSpawnerManager : NetworkBehaviour, IPlayerJoined, IPlayerLeft   //Just a different name for NetworkSpawnerController in Guide.
{
    [SerializeField] private TextMeshProUGUI m_roomCodeText;
    [SerializeField] private NetworkPrefabRef m_playerNetworkPrefab;
    [SerializeField] private Transform[] m_spawnPointsArray;
    private List<Transform> m_spawnPointsList;

    //private Dictionary<PlayerRef, NetworkObject> m_players = new();   //Version 2

    private NetworkManager m_networkManager;

    private void Awake()
    {
        m_spawnPointsList = new List<Transform>();
        m_networkManager = FindObjectOfType<NetworkManager>();

        if (m_roomCodeText != null)
        {
            m_roomCodeText.text = string.Empty;
            string sessionCode = m_networkManager.DevRoomCode;
            m_roomCodeText.text = sessionCode;
        }
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

            foreach (Transform takenTransform in m_spawnPointsList) //SaveList for randomed PlayerSpawns.
            {
                //'+= 1' on 'randomSpawnPosition', if previously randomed SpawnPoints are already in use, to prevent PlayerSpawns on the same Position.
                if (m_spawnPointsArray[randomSpawnPosition].position == takenTransform.position)
                    randomSpawnPosition += 1 % m_spawnPointsArray.Length;
            }

            //_playerRef sets (Has)InputAuthority over the spawned Object.
            NetworkObject playerObject = Runner.Spawn(m_playerNetworkPrefab, m_spawnPointsArray[randomSpawnPosition].position, Quaternion.identity, _playerRef);

            Runner.SetPlayerObject(_playerRef, playerObject);           //sets IsLocalPlayerObject.
            ////Add the used/randomed Position to the runtime SpawnPointList.
            m_spawnPointsList.Add(m_spawnPointsArray[randomSpawnPosition]);
            Debug.Log(m_spawnPointsList.Count - 1); //TODO: Connect this index with Player A/B/C/D index!!!
            //m_players.Add(_playerRef, playerObject);     //Version 2
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
            if (Runner.TryGetPlayerObject(_playerRef, out var playerObject))
            {
                Runner.Despawn(playerObject);
            }

            Runner.SetPlayerObject(_playerRef, null);           //resets IsLocalPlayerObject.

            ////Version 2
            //if (m_players.TryGetValue(_playerRef, out var playerObject))
            //{
            //    Runner.Despawn(playerObject);
            //    m_players.Remove(_playerRef);
            //}
        }
    }
}