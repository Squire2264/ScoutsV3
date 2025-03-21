﻿using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue")]
public class Dialogue : ScriptableObject
{
	public int deltaPoints;
	public Sentence[] sentences;
}
[System.Serializable]
public class Sentence
{
	public string sentence;
	public Answer[] answers;
	public int nextSentenceNum; //more than 1 (it's not like an array)
}
[System.Serializable]
public class Answer
{
	public string answer;
	public int nextSentenceNum;

	public int deltaPoints;
}