using UnityEngine;
using System.IO;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    private string savePath;
    public ScoreHistory history = new ScoreHistory();

    void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        savePath = Application.persistentDataPath + "/scores.json";
        LoadScores();
    }

    public void AddAttempt(float score, float time, int errors, string rank, bool passed)
    {
        GameAttempt newAttempt = new GameAttempt {
            date = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            totalScore = score,
            timeSpent = time,
            errorCount = errors,
            rank = rank,
            isPassed = passed
        };

        history.attempts.Add(newAttempt);
        SaveScores();
    }

    private void SaveScores()
    {
        string json = JsonUtility.ToJson(history, true); 
        File.WriteAllText(savePath, json);
    }

    private void LoadScores()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            history = JsonUtility.FromJson<ScoreHistory>(json);
        }
    }
}