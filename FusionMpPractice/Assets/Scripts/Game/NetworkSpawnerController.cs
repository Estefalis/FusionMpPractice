using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSpawnerController : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef m_networkGameBallPrefab;
    [SerializeField] private NetworkPrefabRef m_networkPlayerPrefab;
    [SerializeField, Range(0.0f, 10.0f)] private float m_randomSpawnPositionRange = 5.0f;

    private List<NetworkObject> m_spawnedNetworkObjects = new();
    private Dictionary<PlayerRef, NetworkObject> m_spherePlayers = new();

    private NetworkObject m_gameBallNetworkObject;
    private Transform m_ballVisual;

    public override void Spawned()
    {
        if (Runner.IsServer)
        {
            SpawnGameBall();
        }
    }

    private void SpawnGameBall()
    {
        var gameBallSpawnPos = new Vector3(0, 1.6825f, 0);
        var m_gameBallNetworkObject = Runner.Spawn(m_networkGameBallPrefab, gameBallSpawnPos, Quaternion.identity);
        m_ballVisual = m_gameBallNetworkObject.transform.GetChild(0).transform;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
            SpawnBall();

        if (Input.GetKeyDown(KeyCode.X))
            DeSpawnBall();
    }

    private void SpawnBall()
    {
        if (Runner.IsServer)
        {
            var randomSpawnPos = new Vector3(Random.Range(-m_randomSpawnPositionRange, m_randomSpawnPositionRange),
                0.5f, Random.Range(-m_randomSpawnPositionRange, m_randomSpawnPositionRange));

            var ballNetworkObject = Runner.Spawn(m_networkGameBallPrefab, randomSpawnPos, Quaternion.identity);
            m_spawnedNetworkObjects.Add(ballNetworkObject);
        }
    }

    private void DeSpawnBall()
    {
        if (Runner.IsServer)
        {
            foreach (var ball in m_spawnedNetworkObjects)
            {
                Runner.Despawn(ball);
            }
        }
    }

    public void PlayerJoined(PlayerRef player)
    {
        if (Runner.IsServer)
            SpawnPlayerObject(player);
    }

    private void SpawnPlayerObject(PlayerRef player)
    {
        if (Runner.IsServer)
        {
            var randomSpawnPos = new Vector3(Random.Range(-m_randomSpawnPositionRange, m_randomSpawnPositionRange),
                1.0f, Random.Range(-m_randomSpawnPositionRange, m_randomSpawnPositionRange));

            var playerNetworkObject = Runner.Spawn(m_networkPlayerPrefab, new Vector3(0, 1, 0), Quaternion.identity, player);
            m_spherePlayers.Add(player, playerNetworkObject);
            var playerScript = playerNetworkObject.GetComponent<Player>();
            playerScript.SetGameBall(m_ballVisual);
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (Runner.IsServer)
        {
            DeSpawnPlayerObject(player);
        }
    }

    private void DeSpawnPlayerObject(PlayerRef player)
    {
        if(m_spherePlayers.TryGetValue(player, out var demoObject))
        {
            Runner.Despawn(demoObject);
            m_spherePlayers.Remove(player);
        }
    }
}