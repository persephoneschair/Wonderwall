using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Pack
{
    public List<Question> questions = new List<Question>();

    public Pack()
    {

    }

    public Pack(List<Question> questions)
    {
        this.questions = questions;
    }
}
