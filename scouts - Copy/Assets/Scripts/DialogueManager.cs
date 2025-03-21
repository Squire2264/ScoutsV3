﻿using TMPro;
using UnityEditor;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
	[HideInInspector] [System.NonSerialized]
	public Dialogue currentDialogue;

	#region Singleton
	public static DialogueManager instance;
	private void Awake()
	{
		if (instance != null)
		{
			throw new System.Exception("Dialogue manager non è un singleton");
		}
		instance = this;
	}
	#endregion

	public Joystick joy;
	public GameObject dialoguePanel;
	public GameObject blackOverlay;
	GameObject nextButton;
	TextMeshProUGUI title, sentenceText;
	public GameObject[] answerButtons;
	public TextMeshProUGUI[] answerTexts;


	[HideInInspector] [System.NonSerialized]
	public CapieCambu currentObject;


	int deltaPoints, currentSentenceIndex;
	bool isOpen, canAnswer;

	protected void Start()
	{
		nextButton = dialoguePanel.transform.Find("Next").gameObject;
		title = dialoguePanel.transform.Find("Name").GetComponent<TextMeshProUGUI>();
		sentenceText = dialoguePanel.transform.Find("Sentence").GetComponent<TextMeshProUGUI>();
		isOpen = false;
	}

	public void TogglePanel(Dialogue dialogue)
	{
		joy.canUseJoystick = isOpen;
		isOpen = !isOpen;
		dialoguePanel.SetActive(isOpen);
		blackOverlay.SetActive(isOpen);
		PanZoom.instance.canDo = !isOpen;
		currentSentenceIndex = 0;
		if (dialogue != null)
		{
			currentDialogue = dialogue;
			deltaPoints = currentDialogue.deltaPoints;
			ShowSentence(currentDialogue.sentences[currentSentenceIndex]);
		}
	}

	
	public void ReEnableJoy()
	{
		joy.canUseJoystick = true;
	}

	public void ChiusuraPanel()
	{
		joy.canUseJoystick = isOpen;
		isOpen = !isOpen;
		dialoguePanel.SetActive(isOpen);
		blackOverlay.SetActive(isOpen);
		PanZoom.instance.canDo = !isOpen;
		if (currentObject != null) currentObject.ResetWait(0);
	}
	void ShowSentence(Sentence s)
	{
		title.text = currentObject.objectName;
		sentenceText.text = s.sentence;

		canAnswer = s.answers != null && s.answers.Length > 0;
		ShowPossibleAnswers(s);
	}
	void ShowPossibleAnswers(Sentence s)
	{
		foreach (var b in answerButtons)
			b.SetActive(false);

		for (int a = 0; a < s.answers.Length; a++)
		{
			answerButtons[a].SetActive(true);
			answerTexts[a].text = s.answers[a].answer;
		}
		nextButton.SetActive(!canAnswer);
	}

	public void NextSentence(int answerNum)//0 or null if no answer
	{
		var s = currentDialogue.sentences[currentSentenceIndex];
		int answerIndex = answerNum - 1;
		if (canAnswer)
		{
			deltaPoints += s.answers[answerIndex].deltaPoints;
			currentSentenceIndex = s.answers[answerIndex].nextSentenceNum - 1;
		}
		else
		{
			currentSentenceIndex = s.nextSentenceNum - 1;
		}

		if (currentSentenceIndex < currentDialogue.sentences.Length)
		{
			ShowSentence(currentDialogue.sentences[currentSentenceIndex]);
		}
		else
		{
			TogglePanel(null);
			currentObject.nextDialogueIndex++;
			currentObject.UnlockAndCreateExPlayerPath();
			GameManager.instance.ChangeCounter(Counter.Punti, CampManager.instance.MultiplyByDurationFactor(deltaPoints, DurationFactor.prizesFactor));
		}
	}
}
