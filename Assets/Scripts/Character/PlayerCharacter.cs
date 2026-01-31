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
        public CinemachineCamera cam;
    }

    [Header("Camera Setup (IMPORTANT)")]
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

        // FORCE SETUP ON START
        // Ensure Default is High (10) and all Combos are Low (0)
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
                StartCoroutine(PlayEffect(combo.cam));
                currentInputs.Clear();
                return;
            }
        }
    }

    private IEnumerator PlayEffect(CinemachineCamera targetCam)
    {
        if (targetCam == null) yield break;

        // 1. Switch TO Finisher
        // We make it higher than the default (which is 10)
        targetCam.Priority = 20; 

        // Slow Motion
        Time.timeScale = sloMoSpeed;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(effectDuration);

        // 2. Switch BACK to Default
        targetCam.Priority = 0; // Drop Finisher
        if (defaultCamera != null) defaultCamera.Priority = 10; // Ensure Default is dominant

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