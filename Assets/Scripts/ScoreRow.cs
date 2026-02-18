using UnityEngine;
using TMPro;

public class ScoreRow : MonoBehaviour
{
    [Header("UI Text References")]
    public TextMeshProUGUI dateTxt;
    public TextMeshProUGUI scoreTxt;
    public TextMeshProUGUI timeTxt;
    public TextMeshProUGUI errorTxt;
    public TextMeshProUGUI statusTxt;
    public TextMeshProUGUI rankTxt; // <-- BURAYI EKLEDİK

    public void Setup(GameAttempt data)
    {
        dateTxt.text = data.date;
        scoreTxt.text = data.totalScore.ToString("F0");
        timeTxt.text = FormatTime(data.timeSpent);
        errorTxt.text = data.errorCount.ToString();
        rankTxt.text = data.rank; // <-- BURAYI EKLEDİK
        
        statusTxt.text = data.isPassed ? "PASSED" : "FAILED";
        statusTxt.color = data.isPassed ? Color.green : Color.red;
    }

    private string FormatTime(float time)
    {
        int minutes = (int)time / 60;
        int seconds = (int)time % 60;
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}