using System.Collections.Generic;

[System.Serializable]
public class GameAttempt
{
    public string date;
    public float totalScore;
    public float timeSpent;
    public int errorCount;
    public string rank;
    public bool isPassed;
}

[System.Serializable]
public class ScoreHistory
{
    public List<GameAttempt> attempts = new List<GameAttempt>();
}