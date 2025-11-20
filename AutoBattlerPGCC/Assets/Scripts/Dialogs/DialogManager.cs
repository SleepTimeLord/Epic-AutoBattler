using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [Header("Canvas Elements")]
    [SerializeField]
    private GameObject personOne; // character 1 image
    [SerializeField]
    private GameObject personTwo; // character 2 image
    [SerializeField]
    private GameObject dialogPanel; // entire dialog panel
    [SerializeField]
    private GameObject continueButton;
    [SerializeField]
    private TextMeshProUGUI currentDialogText; // dialog text
    [SerializeField]
    private TextMeshProUGUI personOneName; // person one nameplate text
    [SerializeField]
    private GameObject personOneNameplate; // person two nameplate
    [SerializeField]
    private TextMeshProUGUI personTwoName; // person two nameplate text
    [SerializeField]
    private GameObject personTwoNameplate; // person two nameplate
    [SerializeField]
    private GameObject replyChoicePanel; // entire user replies panel
    [SerializeField]
    private TextMeshProUGUI replyChoiceOneText; // choice one text
    [SerializeField]
    private TextMeshProUGUI replyChoiceTwoText; // choice two text

    private static UnityEngine.Input input;
    private static DialogCharacter personTalking;
    private static DialogCharacter.Scene currentScene;
    private static int sceneIndex = 0;
    
    private static DialogCharacter dialogCharacterOne;
    private static DialogCharacter dialogCharacterTwo;
    
    [SerializeField]
    private StoryScripts storyScripts;
    [SerializeField]
    private DialogView dialogView;

    private int scenesListIndex = -1;

    [SerializeField]
    private float typingSpeed = 25f;
    private string dialogText;
    public static DialogManager Instance { get; private set; }
    public bool isTyping { get; private set; }
    public bool isOpen
    {
        get => dialogPanel.activeInHierarchy;
    }

    protected static string __confirmation = "Are you sure?";
    protected static string[] __affirmationsArr = new string[]
    {
        "That's a great selection!", 
        "Oh? Those are quite handy!", 
        "Great Choice!"
    };

    public void StartDialog()
    {
        scenesListIndex++;
        open();
        nextLine(0);
    }

    public void nextLineViaReply(bool firstOption)
    {
        Debug.Log($"{GetLine(personTalking).replies.Count}........REPLIES LIST COUNT...........");
        int line = firstOption ? GetLine(personTalking).replies[0].goToLineIndex : GetLine(personTalking).replies[1].goToLineIndex;
        Debug.Log($"{GetLine(personTalking).sceneIndex}......SCENE INDEX.............");
        Debug.Log($"{personTalking.dialogIndex}----------PERSON DIALOGINDEX BEFORE--------");
        personTalking.dialogIndex++;
        Debug.Log($"{personTalking.dialogIndex}----------PERSON DIALOGINDEX AFTER--------");
        nextLine(line);
    }
    
    public void nextLine(int nextLines)
    {
        if (!isOpen) return;
        
        if (isTyping)
        {
            stopTyping();
            isTyping = false;
            return;
        }
        
        Debug.Log($"{sceneIndex} scene index-----before------");
        sceneIndex += nextLines;
        Debug.Log($"{sceneIndex} scene index----after-----");
        
        personTalking = dialogView.scenes[scenesListIndex].sceneCharacterIndex[sceneIndex];
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        // Character Image only works for User and One Character for now
        if (personTalking.characterImage == null)
        {
            // if (personTalking.alignment.Equals(1))
            // {
            //     personTwoNameplate.SetActive(false);
            // }
            // else
            // {
                Debug.Log($"{personTalking.characterName} doesn't have a Character Image!");
            //}
        }
        if (personTalking.alignment.Equals(DialogCharacter.eAlignment.LEFT))
        {
            personOne.GetComponent<Image>().sprite = personTalking.characterImage;
            personOneNameplate.SetActive(true);
            personOneName.text = personTalking.characterName;
            personOne.SetActive(true);
        }
        else if (personTalking.alignment.Equals(DialogCharacter.eAlignment.RIGHT))
        {
            personTwo.GetComponent<Image>().sprite = personTalking.characterImage;
            personTwoName.text = personTalking.characterName;
            personTwoNameplate.SetActive(true);
            personTwo.SetActive(true);
        }
        
        
        var line = GetLine(personTalking);
        if (personTalking.dialogIndex < personTalking.dialogLines.Count) typeDialog(line.dialogLine);
        else close();
        
        if (line.replies.Count == 2) {
            replyChoiceOneText.text = line.replies[0].replyText;
            replyChoiceTwoText.text = line.replies[1].replyText;
            replyChoicePanel.SetActive(true);
            continueButton.SetActive(false);
        }

        if (personTalking.dialogLines[personTalking.dialogIndex].utilizesScripts)
        {
            Debug.Log($"entered utilizes scripts if-------{personTalking.dialogLines[personTalking.dialogIndex].utilizesScripts}---------------------- {sceneIndex}");
            switch (currentScene)
            {
                case DialogCharacter.Scene.INTRODUCTION:
                {
                    if (sceneIndex == 10)
                    {
                        Debug.Log($"stoppping input");
                        stopInput();
                        Debug.Log($"trying story script");
                        storyScripts.Invoke(nameof(StoryScripts.exitGame), 1);
                        Debug.Log($"exit script failed.");
                    }
                    if (sceneIndex == 11)
                    {
                        SceneManager.LoadScene(2);
                    }
                    break;
                }
                case DialogCharacter.Scene.DECKBUILDING:
                {
                    switch (sceneIndex)
                    {
                        case 2:
                        {
                            
                            break;
                        }
                    }
                    break;
                }
            }
        }
        
        if (line.replies.Count <= 1)
        {
            if (line.replies is not { Count: > 1 })
            {
                continueButton.SetActive(true);
                replyChoicePanel.SetActive(false);
                // Only auto-advance the per-character index if there are no choices
                personTalking.dialogIndex++;
            }
        }
    }

    public DialogCharacter.DialogLine GetLine(DialogCharacter character)
    {
        Debug.Log($"{character.characterName} has the dialog index of {character.dialogIndex}. " +
                  $"The length of their lines are {character.dialogLines.Count}.");
        return character.dialogLines[character.dialogIndex];
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        Instance = this;
        //close();
    }

    void stopInput()
    {
        continueButton.SetActive(false);
        replyChoicePanel.SetActive(false);
    }

    void open()
    {
        dialogPanel.SetActive(true);
        personOne.SetActive(false);
        personTwo.SetActive(false);
        personOneNameplate.SetActive(false);
        personTwoNameplate.SetActive(false);
    }

    void close()
    {
        dialogPanel.SetActive(false);
    }

    void stopTyping()
    {
        StopAllCoroutines();
        currentDialogText.text = dialogText;
        isTyping = false;
    }

    void typeDialog(string dialogText)
    {
        this.dialogText = dialogText;
        isTyping = true;
        StartCoroutine(typewriterEffect());
    }

    private IEnumerator typewriterEffect()
    {
        currentDialogText.text = string.Empty;
        float delay = 1f / typingSpeed;

        foreach (char c in dialogText)
        {
            currentDialogText.text += c;
            yield return new WaitForSeconds(delay);
        }
        isTyping = false;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartDialog();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
