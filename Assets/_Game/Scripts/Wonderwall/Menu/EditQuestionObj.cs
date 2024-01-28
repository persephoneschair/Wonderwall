using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class EditQuestionObj : MonoBehaviour
{
    public Question containedQuestion;

    public TMP_InputField questionInput;
    public TMP_InputField correctInput;
    public TMP_InputField incorrectInput;
    public Toggle usedToggle;
    public Button deleteQ;

    public void OnChangeQuestionText(string s)
    {
        QuestionDatabase.Questions.FirstOrDefault(x => x.ID == containedQuestion.ID).question = s;
    }

    public void OnChangeCorrectText(string s)
    {
        QuestionDatabase.Questions.FirstOrDefault(x => x.ID == containedQuestion.ID).correctAnswer = s;
    }

    public void OnChangeIncorrectText(string s)
    {
        QuestionDatabase.Questions.FirstOrDefault(x => x.ID == containedQuestion.ID).incorrectAnswer = s;
    }

    public void OnDeleteQuestion()
    {
        PersistenceManager.OnDeleteQuestion(containedQuestion);
        Destroy(this.gameObject);
    }
}
