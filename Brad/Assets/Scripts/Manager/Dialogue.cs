using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;

public class Dialogue : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI textComponent;

    private string[] lines;

    private GameObject[] speakers;
    private Animator[] anims;
    private int[] speaks;
    private int currentSpeaker;
    private Transform[] transforms;

    public float camZoomGain;
    private float currentZoom;

    [SerializeField]
    private CinemachineVirtualCamera cam;

    private GameObject dialogueZoneController;

    private GameObject player;

    private bool startCamZoom;

    public float textSpeed;

    private int index;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        currentZoom = cam.m_Lens.OrthographicSize;
    }

    private void OnEnable()
    {
        index = 0;
        cam.Follow = transforms[currentSpeaker];
        startCamZoom = true;
        textComponent.text = string.Empty;
    }

    // Update is called once per frame
    void Update()
    {
        CheckCamZoom();

        if (Input.GetKeyDown(KeyCode.E))
        {

            if (textComponent.text == lines[index])
            {
                NextLine();
            }
            else
            {
                StopAllCoroutines();
                textComponent.text = lines[index];
            }
        }
    }

    private void CheckCamZoom()
    {
        if (startCamZoom)
        {
            currentZoom -= camZoomGain * Time.deltaTime;
            if(currentZoom <= 5.0f)
            {
                currentZoom = 5.0f;
                startCamZoom = false;
            }
            cam.m_Lens.OrthographicSize = currentZoom;
        }
    }

    public void StartDialogue(string[] newLines, GameObject[] newSpeakers, int[] newSpeaks, Animator[] newAnims, GameObject newDialogueZoneController)
    {
        dialogueZoneController = newDialogueZoneController;
        lines = newLines;
        speakers = newSpeakers;
        transforms = new Transform[speakers.Length];
        speaks = newSpeaks;
        currentSpeaker = speaks[0];
        anims = newAnims;

        int i = 0;
        foreach (GameObject speaker in speakers)
        {
            transforms[i++] = speaker.transform;
        }
         
        gameObject.SetActive(true);
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray())
        {
            anims[currentSpeaker].SetTrigger("speak");
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    private void NextLine()
    {
        if (index < lines.Length - 1)
        {
            index++;
            textComponent.text = string.Empty;
            if(currentSpeaker != speaks[index])
            {
                currentSpeaker = speaks[index];
            }
            cam.Follow = transforms[currentSpeaker];
            StartCoroutine(TypeLine());
        }
        else
        {
            cam.Follow = player.transform;
            player.SendMessage("EnablePlayerControl");
            Destroy(dialogueZoneController);
            gameObject.SetActive(false);
        }
    }

}
