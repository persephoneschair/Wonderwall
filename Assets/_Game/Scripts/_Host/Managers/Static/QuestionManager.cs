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

    private static bool bespokeWall;

    public static void LoadPack(Pack p)
    {
        currentPack = p;
    }

    public static void LoadPack(List<Question> questions, bool bespoke)
    {
        bespokeWall = bespoke;
        currentPack = new Pack(questions);
        HackboxManager.Get.SendOperatorGetName();
    }

    public static void ClearDownPack()
    {
        currentPack = null;
        bespokeWall = false;
        currentIndex = 0;
        WonderwallManager.Get.CurrentQuestion = null;
    }

    public static Question GetNextQuestion()
    {
        if (currentIndex >= currentPack.questions.Count)
            return null;

        currentIndex++;
        if(!bespokeWall)
            PersistenceManager.CurrentDatabaseProfile.UsedQsOnThisProfile.Add(currentPack.questions[currentIndex - 1].ID);
        PersistenceManager.WriteDatabaseProfiles();
        return currentPack.questions[currentIndex - 1];
    }
}
