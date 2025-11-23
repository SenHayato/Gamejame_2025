using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
[System.Serializable]
public struct DialogueLine
{
    public string speakerName;
    [TextArea(3, 5)]
    public string dialogueText;
    public Sprite speakerImageA;
    public Sprite speakerImageB;
    public bool isSpeakerAActive;
}

public class NPCInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private GameObject interactIndicator;
    public UnityEngine.Events.UnityEvent onDialogueFinished;
    [Header("Dialogue Settings")]
    [SerializeField] private List<DialogueLine> dialogueLines;
    [SerializeField] private GameObject dialogueUI;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image speakerImageA;
    [SerializeField] private Image speakerImageB;
    [SerializeField] private GameObject skipButton;

    [Header("Skip Dialogue")]
    [SerializeField] private GameObject skipPanel;
    [SerializeField] private TextMeshProUGUI skipSummaryText;
    [SerializeField][TextArea(2, 5)] private string skipSummary;

    [Header("Typing Settings")]
    [SerializeField] private float typingSpeed = 0.03f;

    [Header("Patrol Settings")]
    [SerializeField] private Transform patrolPoint;
    [SerializeField] private float patrolSpeed = 2f;

    [Header("Auto Dialogue Trigger")]
    [SerializeField] private bool autoStartDialogue = false;
    [SerializeField] private bool oneTimeOnly = false;
    [SerializeField] private float autoStartDelay = 3.0f;

    [SerializeField] private GameObject blurPanel;


    private bool isPlayerInRange = false;
    private bool isPatrolling = false;
    private bool isTyping = false;
    private bool isSkipPanelOpen = false;
    private bool isDialogueActive = false;
    private bool hasTriggered = false;

    private string currentFullLine = "";
    private Coroutine typingCoroutine;
    private Coroutine autoStartCoroutine;
    private Queue<DialogueLine> dialogueQueue;

    private float previousTimeScale = 1f;

    private void Start()
    {
        dialogueQueue = new Queue<DialogueLine>();
        dialogueUI.SetActive(false);

        if (interactIndicator != null)
            interactIndicator.SetActive(false);

        speakerImageA.enabled = false;
        speakerImageB.enabled = false;

        // --- NEW LOGIC: Trigger automatically when Scene Starts ---
        if (autoStartDialogue && !hasTriggered)
        {
            autoStartCoroutine = StartCoroutine(WaitAndStartDialogue());
        }
    }

    // --- The Timer Logic ---
    private IEnumerator WaitAndStartDialogue()
    {
        // Wait for 3 seconds (or whatever is set in Inspector)
        yield return new WaitForSeconds(autoStartDelay);

        // Double check dialogue hasn't been triggered manually yet
        if (!isDialogueActive && (!oneTimeOnly || !hasTriggered))
        {
            StartDialogue();
        }

        autoStartCoroutine = null;
    }

    // ... (Rest of the code remains the same) ...

    public void OnInteractButtonClicked()
    {
        if (!isDialogueActive && isPlayerInRange)
        {
            StartDialogue();
        }
    }

    private void Update()
    {
        if (isSkipPanelOpen) return;

        // --- PART 1: ADVANCE DIALOGUE (Click / Space / F) ---
        // We REMOVED isPlayerInRange here. 
        // As long as the dialogue is running, you can click from anywhere.
        if (isDialogueActive)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.F))
            {
                if (isTyping)
                {
                    // Instant finish typing
                    if (typingCoroutine != null) StopCoroutine(typingCoroutine);
                    dialogueText.text = currentFullLine;
                    isTyping = false;
                    typingCoroutine = null;
                }
                else
                {
                    // Next sentence
                    DisplayNextSentence();
                }
            }
            // We return here so we don't accidentally trigger the "Start" logic below in the same frame
            return;
        }

        // --- PART 2: MANUAL START (F) ---
        // This still requires you to be close to the NPC to START the conversation manually.
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.F))
        {
            StartDialogue();
        }

        // --- PART 3: PATROL LOGIC ---
        if (isPatrolling)
        {
            float step = patrolSpeed * Time.unscaledDeltaTime;
            transform.position = Vector3.MoveTowards(transform.position, patrolPoint.position, step);

            if (Vector3.Distance(transform.position, patrolPoint.position) < 0.01f)
            {
                isPatrolling = false;
            }
        }
    }

    public void StartDialogue()
    {
        if (autoStartCoroutine != null)
        {
            StopCoroutine(autoStartCoroutine);
            autoStartCoroutine = null;
        }

        if (hasTriggered && oneTimeOnly)
        {
            Debug.Log("Dialogue already triggered and is set to one-time only.");
            return;
        }

        if (dialogueLines.Count == 0)
        {
            Debug.LogWarning("No dialogue lines assigned for this NPC!");
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        isDialogueActive = true;
        dialogueQueue.Clear();

        foreach (var line in dialogueLines)
            dialogueQueue.Enqueue(line);

        dialogueUI.SetActive(true);
        if (interactIndicator != null) interactIndicator.SetActive(false);
        if (blurPanel != null) blurPanel.SetActive(true);
        if (skipButton != null) skipButton.SetActive(true);

        DisplayNextSentence();
    }

    private void DisplayNextSentence()
    {
        if (dialogueQueue.Count == 0)
        {
            EndDialogue();
            return;
        }

        var currentLine = dialogueQueue.Dequeue();

        speakerText.text = currentLine.speakerName;
        currentFullLine = currentLine.dialogueText;

        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeSentence(currentFullLine));

        Vector3 fullSize = Vector3.one;
        Vector3 dimmedSize = new Vector3(0.9f, 0.9f, 1f);

        Color bright = Color.white;
        Color dim = new Color(0.8f, 0.8f, 0.8f, 1f);

        if (currentLine.speakerImageA != null)
        {
            speakerImageA.enabled = true;
            speakerImageA.sprite = currentLine.speakerImageA;
            speakerImageA.transform.localScale = currentLine.isSpeakerAActive ? fullSize : dimmedSize;
            speakerImageA.color = currentLine.isSpeakerAActive ? bright : dim;
        }
        else
        {
            speakerImageA.enabled = false;
        }

        if (currentLine.speakerImageB != null)
        {
            speakerImageB.enabled = true;
            speakerImageB.sprite = currentLine.speakerImageB;
            speakerImageB.transform.localScale = currentLine.isSpeakerAActive ? dimmedSize : fullSize;
            speakerImageB.color = currentLine.isSpeakerAActive ? dim : bright;
        }
        else
        {
            speakerImageB.enabled = false;
        }
    }

    private void EndDialogue()
    {
        Debug.Log("Dialogue ended.");

        Time.timeScale = previousTimeScale;

        isDialogueActive = false;
        dialogueUI.SetActive(false);
        if (skipButton != null) skipButton.SetActive(false);
        if (blurPanel != null) blurPanel.SetActive(false);

        speakerText.text = "";
        dialogueText.text = "";

        speakerImageA.enabled = false;
        speakerImageB.enabled = false;

        hasTriggered = oneTimeOnly || hasTriggered;

        // Only show indicator if player is actually near (logic for manual re-trigger)
        if (interactIndicator != null && isPlayerInRange)
        {
            if (oneTimeOnly && hasTriggered && patrolPoint != null)
            {
                interactIndicator.SetActive(false);
            }
            else if (!(oneTimeOnly && hasTriggered))
            {
                interactIndicator.SetActive(true);
            }
        }
        onDialogueFinished?.Invoke();
        if (oneTimeOnly && patrolPoint != null)
        {
            isPatrolling = true;
        }
    }

    public void OnSkipButtonPressed()
    {
        if (!isDialogueActive) return;

        if (skipPanel != null)
        {
            skipPanel.SetActive(true);
            skipSummaryText.text = skipSummary;
            isSkipPanelOpen = true;

            if (skipButton != null) skipButton.SetActive(false);
        }
    }

    public void ConfirmSkip()
    {
        isSkipPanelOpen = false;

        while (dialogueQueue.Count > 0)
            dialogueQueue.Dequeue();

        EndDialogue();

        if (skipPanel != null) skipPanel.SetActive(false);
    }

    public void CancelSkip()
    {
        isSkipPanelOpen = false;

        if (skipPanel != null) skipPanel.SetActive(false);
        if (skipButton != null) skipButton.SetActive(true);
    }

    // --- Trigger logic for Manual Interaction (pressing F) ---
    // We keep this so if the auto-dialogue finishes, the player can come back 
    // and talk again manually (if oneTimeOnly is false).
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInRange = true;

        if (interactIndicator != null)
            interactIndicator.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (interactIndicator != null)
                interactIndicator.SetActive(false);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }

    private IEnumerator TypeSentence(string sentence)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(typingSpeed);
        }

        isTyping = false;
        typingCoroutine = null;
    }
}