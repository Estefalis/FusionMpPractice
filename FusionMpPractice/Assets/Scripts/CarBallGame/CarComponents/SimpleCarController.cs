using Fusion;
using UnityEngine;

public class SimpleCarController : NetworkBehaviour
{
    [SerializeField]
    private Wheel[] wheels = new Wheel[4]; 
    
    [SerializeField]
    private TractionMode tractionMode = TractionMode.FrontWheelDrive;
    
    [SerializeField]
    private float maxAcceleration = 30f;
    
    [SerializeField]
    private float maxNormalSpeed = 30;
    
    [SerializeField]
    private float minNormalSpeed = -15;
    
    [SerializeField]
    private float maxSpeedWithRocket = 50;
    
    [SerializeField]
    private float breakAcceleration = 20f;
    
    [SerializeField, Range(0f, 45f)]
    private float maxSteerAngle = 30f;
    
    [SerializeField]
    private float steerSensitivity = 1f;
    
    [SerializeField]
    private float jumpForce = 6000f;
    
    [SerializeField]
    private float rocketForcePerSec = 500;
    
    [SerializeField]
    private float flyRotationTorquePerSecond = 2500f;
    
    [SerializeField]
    private Vector3 centerOfMass = new Vector3(0f, 0.2f, 0.0f);

    [SerializeField, Unity.Collections.ReadOnly]
    private float speedPerSecond;
    
    [SerializeField, Unity.Collections.ReadOnly]
    private float speedPerHour;
    
    private float _horizontalInput;
    private float _verticalInput;
    private bool _isHandBrake;
    private bool _isRocketing;
    private float _steerAngle;
    private bool _canMove = true;
    private int _maxJumpRight = 2;
    private int _jumpRight = 2;
    private bool _isJumpPressed;
    private float _canJumpTime;
    private Rigidbody _rigidbody;
    private Transform _carBodyTransform;
    
    [Networked] CarInputData CarInputData { get; set; }
    
    public float SpeedPerHour => speedPerHour;
    
    private void Awake()
    {
        _rigidbody = GetComponentInChildren<Rigidbody>();
        _carBodyTransform = _rigidbody.transform;
    }
    
    private void Start()
    {
        //var massCenter = _rigidbody.centerOfMass;
        _rigidbody.centerOfMass = new Vector3(0, centerOfMass.y, centerOfMass.z);
    }
    
    private void Update()
    {
        AnimateWheelMeshes();
    }
    
    public void DisableMovementAndRocket()
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = 0;
        }
        
        _isRocketing = false;
        _verticalInput = 0;
        _canMove = false;
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }
    
    public void EnableMovementAndRocket()
    {
        Debug.Log("EnableMovementAndRocket");
        _canMove = true;
    }

    public override void FixedUpdateNetwork()
    {
        base.FixedUpdateNetwork();
        
        _horizontalInput = CarInputData.Direction.x;
        _verticalInput = CarInputData.Direction.z;
        _isHandBrake = CarInputData.IsBraking;
        _isRocketing = CarInputData.IsRocketing;
        
        CalculateSpeed();
        Steer();
        HandBrake();

        if (_canMove)
        {
            Move();
            Rocket();
        }
    }
    
    private void CalculateSpeed()
    {
        //speed at forward direction
        speedPerSecond = Vector3.Dot(_rigidbody.velocity, _carBodyTransform.forward);
        speedPerHour = speedPerSecond * 3.6f;
    }
    
    private void Move()
    {
        if (speedPerSecond > maxNormalSpeed || speedPerSecond < minNormalSpeed)
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.motorTorque = 0;
            }
            return;
        }

        foreach (var wheel in wheels)
        {
            switch (tractionMode)
            {
                case TractionMode.FrontWheelDrive:
                    if (wheel.IsFrontWheel)
                    {
                        wheel.wheelCollider.motorTorque = _verticalInput * maxAcceleration * Runner.DeltaTime * Mathf.Lerp(1.25f, 0.75f, speedPerHour / maxNormalSpeed);
                    }
                    break;
                case TractionMode.RearWheelDrive:
                    if (wheel.IsBackWheel)
                    {
                        wheel.wheelCollider.motorTorque = _verticalInput * maxAcceleration * Runner.DeltaTime * Mathf.Lerp(1.25f, 0.75f, speedPerHour / maxNormalSpeed);
                    }
                    break;
                case TractionMode.AllWheelDrive:
                    wheel.wheelCollider.motorTorque = _verticalInput * maxAcceleration * Runner.DeltaTime * Mathf.Lerp(1.25f, 0.75f, speedPerHour / maxNormalSpeed);
                    break;
            }
        }
    }
    
    private void Fly()
    {
        if (CheckIfWheelsAreGrounded())
        {
            if (Time.time > _canJumpTime)
            {
                _jumpRight = _maxJumpRight;
            }
        }//Car can fly
        else 
        {
            //Fly with torque
            _rigidbody.AddRelativeTorque(Vector3.right * flyRotationTorquePerSecond * Runner.DeltaTime * _verticalInput + 
                                         Vector3.up * flyRotationTorquePerSecond * Runner.DeltaTime * _horizontalInput, ForceMode.Impulse);
        }
    }    
    
    private void CheckJump()
    {
        //check if there is a jump input
        if (CarInputData.IsJumping)
        {
            if (_isJumpPressed)
            {
                return;
            }else if (_jumpRight > 0 && Time.time > _canJumpTime)
            {
                JumpAction();
            }
        }else
        {
            _isJumpPressed = false;
        }
        
        //check if there is a jump right
    }
    
    private void JumpAction()
    {
        _canJumpTime = Time.time + 0.15f;
        _isJumpPressed = true;
        _jumpRight--;
        
        var forceMode = ForceMode.VelocityChange;
        //if there is a horizontal input
        if (_horizontalInput is < -0.1f or > 0.1f)
        {
            //if there are horizontal and vertical inputs
            if (_verticalInput is < -0.1f or > 0.1f)
            {
                _rigidbody.AddForce(((_carBodyTransform.right * _horizontalInput + _carBodyTransform.forward * _verticalInput) * 0.75f + _carBodyTransform.up).normalized * jumpForce, forceMode);
                _rigidbody.AddTorque(_carBodyTransform.forward * -_horizontalInput * jumpForce * 0.5f + _carBodyTransform.right * _verticalInput * jumpForce * 0.25f, forceMode);
            }
            else
            {
                //Just horizontal input
                _rigidbody.AddForce((_carBodyTransform.right * _horizontalInput * 0.75f + _carBodyTransform.up).normalized * jumpForce, forceMode);
                _rigidbody.AddTorque(_carBodyTransform.forward * -_horizontalInput * jumpForce * 0.5f, forceMode);
            }
        }
        else
        {
            //Just vertical input
            if (_verticalInput is < -0.1f or > 0.1f)
            {
                _rigidbody.AddForce((_carBodyTransform.forward * _verticalInput  + _carBodyTransform.up* 1.1f).normalized * jumpForce, forceMode);
                _rigidbody.AddTorque(_carBodyTransform.right * jumpForce * _verticalInput * 0.35f, forceMode);
            }
            else
            {
                //Just jump
                _rigidbody.AddForce(_carBodyTransform.up * jumpForce * 0.95f, forceMode);
            }
        }
    }

    private bool CheckIfWheelsAreGrounded()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.IsGrounded)
            {
                return true;
            }
        }

        return false;
    }

    private void Steer()
    {
        _steerAngle = _horizontalInput * maxSteerAngle * steerSensitivity;
        foreach (var wheel in wheels)
        {
            if (wheel.IsFrontWheel)
            {
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, Runner.DeltaTime * 30f);
            }
        }
    }
    
    private void AnimateWheelMeshes()
    {
        foreach (var wheel in wheels)
        {
            wheel.SynchronizeWheels();
        }
    }

    private void HandBrake()
    {
        if (_isHandBrake)
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = breakAcceleration * Runner.DeltaTime;
            }
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0f;
            }
        }
    }
    
    private void Rocket()
    {
        if (!_isRocketing) return;
        if (speedPerSecond > maxSpeedWithRocket) return;
        _rigidbody.AddForce(_carBodyTransform.forward * rocketForcePerSec * Runner.DeltaTime, ForceMode.Acceleration);

    }

    public void SetInputData(CarInputData data)
    {
        CarInputData = data;
    }
}
