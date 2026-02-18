using UnityEngine;
using TMPro;
using System.Linq;
using System.Collections.Generic;

public class EndGameUI : MonoBehaviour
{
    [Header("Main Stats TextMeshPro")]
    public TextMeshProUGUI statusText; 
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI errorText;
    public TextMeshProUGUI rankText;

    [Header("Panels")]
    public GameObject clipboardPanel;  
    public GameObject scoreBoardPanel; 

    [Header("Scoreboard List Settings")]
    public Transform scoreListParent; 
    public GameObject scoreRowPrefab; 

    void Start()
    {
        ShowResult();
    }

    public void ShowResult()
    {
        if (GlobalDataManager.instance == null) return;

        var data = GlobalDataManager.instance;
        int finalScore = data.currentScore;
        float finalTime = data.sessionTimer;
        int finalErrors = data.totalMistakes;
        
        // Senin GetRank metodunu kullanarak rütbeyi alıyoruz
        string finalRank = GetRank();
        bool passed = finalScore >= 400; // Certified Tech ve üstü geçti sayılsın dedik

        statusText.text = passed ? "PASSED" : "FAILED";
        statusText.color = passed ? Color.green : Color.red;
        
        scoreText.text = "POINTS: " + finalScore;
        timeText.text = "TIME: " + FormatTime(finalTime);
        errorText.text = "ERRORS: " + finalErrors;
        rankText.text = "RANK: " + finalRank;

        if (ScoreManager.instance != null)
        {
            ScoreManager.instance.AddAttempt(finalScore, finalTime, finalErrors, finalRank, passed);
        }
    }

    // SENİN REPORT SLIDE CONTROLLER'DAKİ ASIL BOMBA MANTIK:
    string GetRank()
    {
        var data = GlobalDataManager.instance;

        if (data.totalInputs > 100 && data.totalMistakes > 5) return "PANICKED ROOKIE";
        if (data.totalMistakes == 0 && data.currentScore > 1000) return "CYBER-SURGEON";
        if (data.sessionTimer < 120 && data.currentScore > 500) return "SONIC TECHNICIAN";
        if (data.totalMistakes > 10) return "RECKLESS REPAIRMAN";
        if (data.currentScore > 800) return "SENIOR MECHANIC";
        if (data.currentScore > 400) return "CERTIFIED TECH";
        if (data.currentScore < 0) return "Omg plz delete the game bro";
        
        return "APPRENTICE";
    }

    public void OpenScoreBoard()
    {
        clipboardPanel.SetActive(false);
        scoreBoardPanel.SetActive(true);

        foreach (Transform child in scoreListParent) Destroy(child.gameObject);

        if (ScoreManager.instance != null)
        {
            var sortedList = ScoreManager.instance.history.attempts
                .OrderByDescending(x => x.totalScore).ToList();

            foreach (var attempt in sortedList)
            {
                GameObject go = Instantiate(scoreRowPrefab, scoreListParent);
                go.GetComponent<ScoreRow>().Setup(attempt);
            }
        }
    }

    string FormatTime(float t) => string.Format("{0:00}:{1:00}", Mathf.FloorToInt(t / 60), Mathf.FloorToInt(t % 60));
}