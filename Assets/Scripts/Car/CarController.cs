using UnityEngine;
using System;
using System.Collections.Generic;

public class CarController : MonoBehaviour
{
    public enum ControlMode
    {
        Keyboard,
        Buttons
    };

    public enum Axel
    {
        Front,
        Rear
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public GameObject wheelEffectObj;
        public ParticleSystem smokeParticle;
        public Axel axel;
    }
    //joystick referance
    public Joystick joystick;
    public ControlMode control;

    public float maxAcceleration = 30.0f;
    public float brakeAcceleration = 50.0f;

    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;

    public Vector3 _centerOfMass;

    public List<Wheel> wheels;

    float moveInput;
    float steerInput;
    private bool isBraking;
    private Rigidbody carRb;

    //private CarLights carLights;
    
    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;

       // carLights = GetComponent<CarLights>();
    }

    void Update()
    {
        
        
        GetInputs();
        AnimateWheels();
        WheelEffects();
        
    }

    void LateUpdate()
    {
        Move();
        Steer();
        Brake();
    }

    public void MoveInput(float input)
    {
        moveInput = input;
    }

    public void SteerInput(float input)
    {
        steerInput = input;
    }

    void GetInputs()
    {
        if (control == ControlMode.Buttons)
        {
            if(joystick.Horizontal>= 0.2f || joystick.Horizontal <= -0.2f) { steerInput = joystick.Horizontal; }
            else { steerInput = 0; }
            if (joystick.Vertical >= 0.2f || joystick.Vertical <= -0.2f) {
                moveInput = joystick.Vertical;
            }
            else { moveInput = 0; }
            
        }
        if (control == ControlMode.Keyboard)
        {
            moveInput = Input.GetAxis("Vertical");
            steerInput = Input.GetAxis("Horizontal");
            // Breaking Input
        
        }
    }

    void Move()
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * Time.deltaTime;
        }
    }

    void Steer()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }
    }
     bool isMovingForward()
    {
        Vector3 facing = carRb.rotation * Vector3.forward;
        Vector3 velocity = carRb.velocity;

        // returns the absolute minimum angle difference
        float angleDifference = Vector3.Angle(facing, velocity);

        // returns the angle difference relative to a third axis (e.g. straight up)
        float relativeAngleDifference = Vector3.SignedAngle(facing, velocity, Vector3.up);
        if (MathF.Abs(angleDifference) > 95) { return false; }
        else { return true; }
        
    }
    void Brake()
    {


        if ((Input.GetKey(KeyCode.Space) || (isMovingForward() && moveInput < 0) || (!isMovingForward() && moveInput > 0)) && carRb.velocity.magnitude > 0.1)
        {
            foreach (var wheel in wheels)
            {
               
                wheel.wheelCollider.brakeTorque = 300 * brakeAcceleration * Time.deltaTime;
               
            }
            isBraking = true;           
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }
            isBraking = false;

        }
    }

    void AnimateWheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }

    void WheelEffects()
    {
        foreach (var wheel in wheels)
        {
            var dirtParticleMainSettings = wheel.smokeParticle.main;

            if (  wheel.axel == Axel.Rear && wheel.wheelCollider.isGrounded == true ) 
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = true;
                wheel.smokeParticle.Emit(1);
            }
            else
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = false;
            }
        }
    }
}