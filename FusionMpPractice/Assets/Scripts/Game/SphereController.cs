using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereController : NetworkBehaviour
{
    [SerializeField] private GameObject m_localSideParent;
    [SerializeField] private float m_moveForce = 100f;

    private Rigidbody m_rigidbody;

    private void Awake()
    {
        m_rigidbody = gameObject.GetComponent<Rigidbody>();
    }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            m_localSideParent.SetActive(true);
        }
        else
        {
            Destroy(m_localSideParent);
        }
    }

    public override void FixedUpdateNetwork()
    {
        if(GetInput(out SphereInputData _inputData))
        {
            Movement(_inputData);
        }
    }

    private void Movement(SphereInputData _inputData)
    {
        var movement = new Vector3(_inputData.HorizontalValue, 0, _inputData.VerticalValue);
        m_rigidbody.AddForce(movement * m_moveForce * Runner.DeltaTime);
    }
}