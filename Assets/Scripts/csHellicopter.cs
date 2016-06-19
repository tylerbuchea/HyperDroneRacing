﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class csHellicopter : MonoBehaviour
{

    public KeyCode UpKey;
    public KeyCode DownKey;
    public KeyCode FrontSpin;
    public KeyCode BackWardSpin;
    public KeyCode LeftTurn;
    public KeyCode RightTurn;
    public KeyCode LeftSpin;
    public KeyCode RightSpin;
    public KeyCode EngineOnOffKey;

    public GameObject[] MainMotor; // Helicoptor Big Propeller. Use Y Rotate Value.
                                   // public GameObject[] SubMotor; //Helicoptor Small Propeller. User X RotateValue.
                                   // public AudioSource _AudioSource; //Helicoptor Engine Sound.

    public float MainMotorRotateVelocity;
    // public float SubMotorRotateVelocity;

    float verticalForceMultiplier = 0.0f;
    float MainMotorRotation = 0.0f;
    // float SubMotorRotation = 0.0f;

    public float UpDownValue;
    float UpDown;
    float yUpDown;

    //Rotate Value
    float Pitch;
    float UpDownTurn;
    float yUpDownTrun;

    float Yaw;
    float LeftRightTurn;
    float yLeftRightTurn;

    float Roll;
    float LeftRightSpin;
    float yLeftRightSpin;

    bool engineIsOn = false;
    float EngineValue = 0.0f;


    float hoverValue;
    bool isHovering;

    // debug
    public GameObject debugText1;
    public GameObject debugText2;
    public GameObject debugText3;

    public enum ControlState
    {
        Mouse,
        KeyBoard
    };

    public ControlState ControlType = new ControlState();

    void Awake()
    {
        Time.timeScale = 0.5f;


        // hoverValue = 10.0f;
        hoverValue = Mathf.Abs(GetComponent<Rigidbody>().mass * Physics.gravity.y); // equivale ao peso do drone, mas não contraria a inércia
        // ^ vai ser o valor base, porque contraria o peso
        

        // vai ter de ser uma força decrescente para baixo se ele sobe e decrescente para cima se ele desce
        // com base em manter a altitude, talvez

        print("hovering set to " + hoverValue);
    }


    void FixedUpdate()
    {
        if (engineIsOn)
        {
            // get vertical speed
            float verticalSpeed = this.GetComponent<Rigidbody>().velocity.y;
            debugText3.GetComponent<Text>().text = "velY = " + verticalSpeed.ToString();

            if (isHovering) { 
                GetComponent<Rigidbody>().AddRelativeForce(Vector3.up * hoverValue);

                float hoverForce = 1.0f;

                GetComponent<Rigidbody>().AddRelativeForce(-Vector3.up * hoverForce * verticalSpeed);
            }
            else
                GetComponent<Rigidbody>().AddRelativeForce(Vector3.up * UpDownValue * verticalForceMultiplier);
            
            /*
            if (transform.position.y > 100)
                GetComponent<Rigidbody>().AddRelativeForce(-Vector3.up * UpDownValue * UpDownVelocity);
            */

            // UpDown = KeyValue(DownKey, UpKey, UpDown, yUpDown, 1.5f, 0.01f); // original
            UpDown = KeyValue(DownKey, UpKey, UpDown, yUpDown, 1.5f, 0.005f);

            InvokeRepeating("updateDebugText", 0.0f, 0.5f);

            if (ControlType == ControlState.KeyBoard)
            {
                UpDownTurn = KeyValue(BackWardSpin, FrontSpin, UpDownTurn, yUpDownTrun, 1.5f, 0.1f);
                LeftRightTurn = KeyValue(LeftTurn, RightTurn, LeftRightTurn, yLeftRightTurn, 1.5f, 0.1f);
                LeftRightSpin = KeyValue(LeftSpin, RightSpin, LeftRightSpin, yLeftRightSpin, 1, 0.1f);
            }

            // else if (ControlType == ControlState.Mouse) // ...

            //Pitch Value
            Pitch += UpDownTurn * Time.fixedDeltaTime;
            Pitch = Mathf.Clamp(Pitch, -1.2f, 1.2f);

            //Yaw Value
            Yaw += LeftRightTurn * Time.fixedDeltaTime;

            //Roll Value
            Roll += -LeftRightSpin * Time.fixedDeltaTime;
            Roll = Mathf.Clamp(Roll, -1.2f, 1.2f);

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.EulerRotation(Pitch, Yaw, Roll), Time.fixedDeltaTime * 1.5f);
            // transform.rotation = Quaternion.Slerp(droneBody.gameObject.transform.rotation, Quaternion.EulerRotation(Pitch, Yaw, Roll), Time.fixedDeltaTime * 1.5f);
        }
    }

    void Update()
    {
        // SoundControl(); // Check Helicoptor Engine Sound
        MotorRotateControl(); // Check Motor Rotate State
        MotorVelocityContorl(); // Check Helicoptor UpDown State
        EngineControl(); // Check Engine Turn On/Off
    }

    float KeyValue(KeyCode A, KeyCode B, float Value, float yValue, float _float, float SmoothTime)
    {
        if (Input.GetKey(A))
        { 
            Value -= Time.deltaTime * _float;
        }
        else if (Input.GetKey(B))
        { 
            Value += Time.deltaTime * _float;
        }
        else
        {
            Value = Mathf.SmoothDamp(Value, 0, ref yValue, SmoothTime);
        }

        Value = Mathf.Clamp(Value, -1, 1);
        return Value;
    }
    /*
    float StabilizeUp(float Value, float yValue, float _float, float SmoothTime)
    {
        if (Input.GetKey(A))
            Value -= Time.deltaTime * _float;
        else if (Input.GetKey(B))
            Value += Time.deltaTime * _float;
        else
            Value = Mathf.SmoothDamp(Value, 0, ref yValue, SmoothTime);

        Value = Mathf.Clamp(Value, -1, 1);
        return Value;
    }
    */

    void MotorVelocityContorl()
    {
        if (engineIsOn)
        {
            // hovering = (Mathf.Abs(GetComponent<Rigidbody>().mass * Physics.gravity.y) / UpDownValue); //for maintain altitude.

            if (UpDown != 0.0f)
            {
                if (isHovering == true)
                {
                    verticalForceMultiplier = 0f;
                    // upDownValue = 0f;
                }

                verticalForceMultiplier += UpDown * 0.1f; //if Input Up/Down Axes, Increace UpDownVelocity for Increace altitude.
                debugText2.GetComponent<Text>().text = "(Nhovering) vertForceMult: " + verticalForceMultiplier;
                isHovering = false;
                // debugText2.GetComponent<Text>().text = "not hovering";
            }
            else { // hover if up/down keys are not pressed
                isHovering = true;
                debugText2.GetComponent<Text>().text = "(hovering) vertForceMult: " + verticalForceMultiplier;
                
                verticalForceMultiplier = Mathf.Lerp(verticalForceMultiplier, hoverValue, Time.deltaTime * 2.5f);
                //if not input Up/Down Axes, Hovering.
                
                // public static function Lerp(a: float, b: float, t: float): float; 
                // The parameter t is clamped to the range [0, 1].
                // When t = 0 returns a.
                // When t = 1 return b.
                // When t = 0.5 returns the midpoint of a and b.
                // aplicar um smooth = 2.0f ou parecido para a interpolação ser mais rápida, e.g.: Time.deltaTime * smooth

                // debugText2.GetComponent<Text>().text = Hovering.ToString();
            }
        }
        limitVerticalForceMultiplier();
    }

    void limitVerticalForceMultiplier()
    {
        if (verticalForceMultiplier > 1.0f)
            verticalForceMultiplier = 1.0f;
        else if (verticalForceMultiplier < 0.1f) // nunca fica negativo porque o updown já fica
            verticalForceMultiplier = 0.1f;
    }

    void MotorRotateControl()
    {
        if (MainMotor.Length > 0)
        {
            /*
            print("wing 0 position: ");
            print(MainMotor[0].transform.position.ToString());
            print("wing 0 local position: ");
            print(MainMotor[0].transform.localPosition.ToString());
            print("wing 0 box collider center position: ");
            print(MainMotor[0].GetComponent<BoxCollider>().center.ToString());
            */

            for (int i = 0; i < MainMotor.Length; i++)
                MainMotor[i].transform.Rotate(0, MainMotorRotation, 0);
        }

        /*
        if (SubMotor.Length > 0)
        {
            for (int i = 0; i < SubMotor.Length; i++)
                SubMotor[i].transform.Rotate(0, 0, SubMotorRotation);
        }
        */

        MainMotorRotation = MainMotorRotateVelocity * EngineValue * Time.deltaTime;
        // SubMotorRotation = SubMotorRotateVelocity * EngineValue * Time.deltaTime;
    }

    void EngineControl()
    {
        if (Input.GetKeyDown(EngineOnOffKey))
        {
            if (engineIsOn)
                engineIsOn = false;
            else
                engineIsOn = true;
        }

        if (engineIsOn == true)
        {
            if (EngineValue < 1)
                EngineValue += Time.deltaTime;
            if (EngineValue >= 1)
                EngineValue = 1;
        }

        else if (engineIsOn == false)
        {
            if (EngineValue > 0)
                EngineValue -= Time.deltaTime / 2;
            if (EngineValue <= 0)
                EngineValue = 0;
        }
    }


    void updateDebugText()
    {
        debugText1.GetComponent<Text>().text = "upDown: " + UpDown.ToString();
    }


    /*
    void SoundControl()
    {
        if (EngineValue > 0)
        {
            _AudioSource.mute = false;
            _AudioSource.pitch = EngineValue;
        }
        else
            _AudioSource.mute = true;
    }
    */
}