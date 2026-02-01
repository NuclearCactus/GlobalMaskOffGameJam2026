using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

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

    public Image CooldownBar;

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

    private AttackType? activeBarType = null;

    // ─── COMBO COUNTER UI ────────────────────────────────────────────────────
    [Header("Combo UI")]
    [Tooltip("The TMP text that displays the combo count (e.g. 'x3')")]
    [SerializeField] private TextMeshProUGUI comboText;
    [Tooltip("How long after the last hit before the combo resets")]
    [SerializeField] private float comboTimeout = 1.5f;
    [Tooltip("How large the text scales up to at the peak of the bounce")]
    [SerializeField] private float bouncePeakScale = 1.6f;
    [Tooltip("Duration of the scale-up phase of the bounce")]
    [SerializeField] private float bounceUpDuration = 0.12f;
    [Tooltip("Duration of the scale-back-down phase of the bounce")]
    [SerializeField] private float bounceDownDuration = 0.25f;

    private int comboCount = 0;
    private Vector3 comboBaseScale;
    private Coroutine comboBounceCoroutine;
    private Coroutine comboTimeoutCoroutine;

    // ─── MASK POP-UP UI ──────────────────────────────────────────────────────
    [Header("Mask Pop-Up UI")]
    [Tooltip("The TMP text that displays the phrase from the knocked-off or stolen mask")]
    [SerializeField] private TextMeshProUGUI maskPopUpText;
    [Tooltip("How long the mask phrase stays visible before disappearing")]
    [SerializeField] private float maskPopUpDuration = 2.0f;

    private Vector3 maskPopUpBaseScale;
    private Coroutine maskPopUpBounceCoroutine;
    private Coroutine maskPopUpHideCoroutine;
    // ─────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null) rb.freezeRotation = true;

        if (defaultCamera != null) defaultCamera.Priority = 10;
        
        foreach(var c in combos)
        {
            if(c.cam != null) c.cam.Priority = 0;
        }

        if (CooldownBar != null)
            CooldownBar.fillAmount = 1f;

        // Combo text: cache base scale, start hidden
        if (comboText != null)
        {
            comboBaseScale = comboText.transform.localScale;
            comboText.gameObject.SetActive(false);
        }

        // Mask pop-up text: cache base scale, start hidden
        if (maskPopUpText != null)
        {
            maskPopUpBaseScale = maskPopUpText.transform.localScale;
            maskPopUpText.gameObject.SetActive(false);
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
        UpdateCooldownBar();
    }

    // ─── HIT CALLBACK ────────────────────────────────────────────────────────

    protected override void OnHitLanded(string phrase)
    {
        // ── Combo counter ──
        comboCount++;

        if (comboText != null)
        {
            comboText.gameObject.SetActive(true);
            comboText.text = $"x{comboCount}";

            if (comboBounceCoroutine != null)
                StopCoroutine(comboBounceCoroutine);
            comboBounceCoroutine = StartCoroutine(BounceAnimation(comboText.transform, comboBaseScale));

            if (comboTimeoutCoroutine != null)
                StopCoroutine(comboTimeoutCoroutine);
            comboTimeoutCoroutine = StartCoroutine(ComboTimeout());
        }

        // ── Mask phrase pop-up ──
        // Only show if the mask actually had a phrase defined
        if (maskPopUpText != null && !string.IsNullOrEmpty(phrase))
        {
            maskPopUpText.text = phrase;
            maskPopUpText.gameObject.SetActive(true);

            if (maskPopUpBounceCoroutine != null)
                StopCoroutine(maskPopUpBounceCoroutine);
            maskPopUpBounceCoroutine = StartCoroutine(BounceAnimation(maskPopUpText.transform, maskPopUpBaseScale));

            if (maskPopUpHideCoroutine != null)
                StopCoroutine(maskPopUpHideCoroutine);
            maskPopUpHideCoroutine = StartCoroutine(MaskPopUpHide());
        }
    }

    // ─── SHARED BOUNCE ANIMATION ─────────────────────────────────────────────
    // Reusable by both the combo text and the mask pop-up text.

    private IEnumerator BounceAnimation(Transform target, Vector3 baseScale)
    {
        // --- Scale UP (snappy pop) ---
        float elapsed = 0f;
        while (elapsed < bounceUpDuration)
        {
            float t = elapsed / bounceUpDuration;
            float ease = 1f - (1f - t) * (1f - t); // Ease-out quad
            target.localScale = baseScale * Mathf.Lerp(1f, bouncePeakScale, ease);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.localScale = baseScale * bouncePeakScale;

        // --- Scale DOWN (soft settle) ---
        elapsed = 0f;
        while (elapsed < bounceDownDuration)
        {
            float t = elapsed / bounceDownDuration;
            float ease = 1f - Mathf.Pow(1f - t, 3f); // Ease-out cubic
            target.localScale = baseScale * Mathf.Lerp(bouncePeakScale, 1f, ease);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.localScale = baseScale;
    }

    // ─── COMBO TIMEOUT ───────────────────────────────────────────────────────

    private IEnumerator ComboTimeout()
    {
        yield return new WaitForSeconds(comboTimeout);

        comboCount = 0;
        if (comboText != null)
        {
            comboText.gameObject.SetActive(false);
            comboText.transform.localScale = comboBaseScale;
        }
    }

    // ─── MASK POP-UP HIDE ────────────────────────────────────────────────────

    private IEnumerator MaskPopUpHide()
    {
        yield return new WaitForSeconds(maskPopUpDuration);

        if (maskPopUpText != null)
        {
            maskPopUpText.gameObject.SetActive(false);
            maskPopUpText.transform.localScale = maskPopUpBaseScale;
        }
    }

    // ─── COOLDOWN BAR ────────────────────────────────────────────────────────

    private void ActivateCooldownBar(AttackType type)
    {
        activeBarType = type;
        if (CooldownBar != null)
            CooldownBar.fillAmount = 0f;
    }

    private void UpdateCooldownBar()
    {
        if (CooldownBar == null || activeBarType == null) return;

        float timer, cooldown;

        switch (activeBarType.Value)
        {
            case AttackType.Left:
                timer    = leftTimer;
                cooldown = attackCd;
                break;
            case AttackType.Right:
                timer    = rightTimer;
                cooldown = attackCd;
                break;
            case AttackType.Uppercut:
                timer    = uppercutTimer;
                cooldown = upperCutCd;
                break;
            default:
                return;
        }

        CooldownBar.fillAmount = Mathf.Clamp01(timer / cooldown);

        if (CooldownBar.fillAmount >= 1f)
            activeBarType = null;
    }

    // ─────────────────────────────────────────────────────────────────────────

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
            ActivateCooldownBar(AttackType.Left);
        }
        else if (Mouse.current.rightButton.wasPressedThisFrame && RightAttack())
        {
            type = AttackType.Right;
            pressed = true;
            ActivateCooldownBar(AttackType.Right);
        }
        else if(Keyboard.current.spaceKey.wasPressedThisFrame && UpperCut())
        {
            type = AttackType.Uppercut;
            pressed = true;
            ActivateCooldownBar(AttackType.Uppercut);
        }
        
        if(Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartDash();
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
                Debug.Log($"Combo {combo.name} Executed!");
                StartCoroutine(PlayCinematicEffect(combo.cam));
                currentInputs.Clear();
                return;
            }
        }
    }

    private IEnumerator PlayCinematicEffect(CinemachineCamera targetCam)
    {
        if (targetCam == null) yield break;

        targetCam.Priority = 20; 

        Time.timeScale = sloMoSpeed;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(effectDuration);

        targetCam.Priority = 0; 

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