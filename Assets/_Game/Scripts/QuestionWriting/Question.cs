using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Question
{
    public string question;
    public string correctAnswer;
    public string incorrectAnswer;

    public Question()
    {

    }

    public Question(string question, string correctAnswer, string incorrectAnswer)
    {
        this.question = question;
        this.correctAnswer = correctAnswer;
        this.incorrectAnswer = incorrectAnswer;
    }
}
