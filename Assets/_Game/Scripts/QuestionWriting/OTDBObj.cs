using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OTDBObj
{
    public int response_code;
    public List<Result> results = new List<Result>();

    public OTDBObj()
    {

    }
}

public class Result
{
    public string difficult;
    public string category;
    public string question;
    public string correct_answer;
    public List<string> incorrect_answers = new List<string>();
}

