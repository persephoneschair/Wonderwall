using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using UnityEngine.PlayerLoop;

public static class QuestionManager
{
    public static Pack currentPack = null;
    private static int currentIndex = 0;

    public static void LoadPack(Pack p)
    {
        currentPack = p;
    }

    public static Question GetNextQuestion()
    {
        if (currentIndex >= currentPack.questions.Count)
            return null;

        currentIndex++;
        return currentPack.questions[currentIndex - 1];
    }
}
