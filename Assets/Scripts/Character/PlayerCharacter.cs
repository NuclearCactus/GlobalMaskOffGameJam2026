using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;

public class PlayerCharacter : Character
{
    public enum AttackType { Left, Right, Uppercut }

    [System.Serializable]
    public class Combo
    {
        public string name;
        public List<AttackType> inputChain;
        [Tooltip("Assign the specific CinemachineCamera from your scene here")]
        public CinemachineCamera cam; 
    }

    [Header("Camera Setup")]
    [Tooltip("Assign your main 3rd person gameplay camera here")]
    [SerializeField] private CinemachineCamera defaultCamera; 
    
    [Header("Combo Settings")]
    [SerializeField] private List<Combo> combos; 
    [SerializeField] private float resetTime = 1.0f; 

    [Header("Cinematic Settings")]
    [SerializeField] private float sloMoSpeed = 0.2f;
    [SerializeField] private float effectDuration = 1.5f;

    [Header("Movement")]
    private Vector3 movement;

    private List<AttackType> currentInputs = new List<AttackType>();
    private float lastInputTime;

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null) rb.freezeRotation = true;

        // SETUP: Ensure Default is active (10) and all Combo cameras are inactive (0)
        if (defaultCamera != null) defaultCamera.Priority = 10;
        
        foreach(var c in combos)
        {
            if(c.cam != null) c.cam.Priority = 0;
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();
        Move(movement);
    }

    private void Update()
    {
        HandleMovement();
        HandleAttacks();
    }

    private void HandleAttacks()
    {
        if (Mouse.current == null) return;

        if (Time.time - lastInputTime > resetTime && currentInputs.Count > 0)
            currentInputs.Clear();

        bool pressed = false;
        AttackType type = AttackType.Left;

        if (Mouse.current.leftButton.wasPressedThisFrame && LeftAttack())
        {
            type = AttackType.Left;
            pressed = true;
        }
        else if (Mouse.current.rightButton.wasPressedThisFrame && RightAttack())
        {
            type = AttackType.Right;
            pressed = true;
        }
        else if(Keyboard.current.spaceKey.wasPressedThisFrame && UpperCut())
        {
            type = AttackType.Uppercut;
            pressed = true;
        }
        
        if(Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartDash();
        }
        
        // If an attack happened, record it and check for combos
        if (pressed)
        {
            lastInputTime = Time.time;
            currentInputs.Add(type);
            CheckCombos();
        }
    }

    private void CheckCombos()
    {
        foreach (var combo in combos)
        {
            // Optimization: Don't check combos that are a different length than our current input
            if (currentInputs.Count != combo.inputChain.Count) continue;

            bool match = true;
            for (int i = 0; i < currentInputs.Count; i++)
            {
                if (currentInputs[i] != combo.inputChain[i])
                {
                    match = false;
                    break;
                }
            }

            if (match)
            {
                Debug.Log($"Combo {combo.name} Executed!");
                StartCoroutine(PlayCinematicEffect(combo.cam));
                currentInputs.Clear(); // Reset inputs after a successful combo
                return;
            }
        }
    }

    private IEnumerator PlayCinematicEffect(CinemachineCamera targetCam)
    {
        if (targetCam == null) yield break;

        // 1. Switch TO Finisher Camera
        // We set Priority to 20 to override the Default Camera (which is 10)
        targetCam.Priority = 20; 

        // 2. Apply Slow Motion
        Time.timeScale = sloMoSpeed;
        Time.fixedDeltaTime = 0.02f * Time.timeScale; // Keep physics consistent

        // 3. Wait for the duration (in Realtime, so we aren't affected by our own SlowMo)
        yield return new WaitForSecondsRealtime(effectDuration);

        // 4. Switch BACK to Default
        targetCam.Priority = 0; 

        // 5. Reset Time
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }


    private void HandleMovement()
    {
        if (Keyboard.current == null || rb == null) return;
        var kb = Keyboard.current;
        float x = 0, z = 0;
        if (kb.wKey.isPressed) z = 1;
        if (kb.sKey.isPressed) z = -1;
        if (kb.aKey.isPressed) x = -1;
        if (kb.dKey.isPressed) x = 1;

        movement = (transform.forward * z) + (transform.right * x).normalized;
    }
}