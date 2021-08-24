using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueZoneController : MonoBehaviour
{
    [SerializeField]
    private GameObject[] speakers;
    [SerializeField]
    private string[] lines;
    [SerializeField]
    private int[] speaks;
    [SerializeField]
    private GameObject dialogueBox;
    private Animator[] anims;
    private Dialogue dialogue;

    public bool onInput = true;

    private void Start()
    {
        anims = new Animator[speakers.Length];
        int i = 0;
        foreach (GameObject speaker in speakers)
        {
            anims[i++] = speaker.GetComponent<Animator>();
        }
        dialogue = dialogueBox.GetComponent<Dialogue>();
    }

    private void StartDialogue()
    {
        dialogue.StartDialogue(lines, speakers, speaks, anims, gameObject);
    }

}
