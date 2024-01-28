using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Question
{
    public Guid ID;
    public string question;
    public string correctAnswer;
    public string incorrectAnswer;

    public Question()
    {
        ID = Guid.NewGuid();
    }

    public Question(string question, string correctAnswer, string incorrectAnswer)
    {
        ID = Guid.NewGuid();
        this.question = question;
        this.correctAnswer = correctAnswer;
        this.incorrectAnswer = incorrectAnswer;
    }
}
