using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InGameUi : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timer, prevTimer;
    [SerializeField] private GameObject dangerIndicator;
    public Image runMap;
    public CanvasGroup inGameUICG;
    //Run type
    [SerializeField] private float totalRunLength, currentRunLength;
    public float totalTime;
    [SerializeField] private Vector3 prevPlayerLinePos;
    [SerializeField] private GameObject startLine, endLine, playerLine;
    [SerializeField] private TextMeshProUGUI dangerText;
    [SerializeField] private TextMeshProUGUI buildText, prevBuildText;
    [Tooltip("Tick one only")]
    public bool isTimeCountDown, isTimeScore, isRun;
    private bool isCanStart;
    [Tooltip("time for game to finish")]
    public float timeLeft = 300;
    //  0               1       
    //alphabet    monster/water
    //public Sprite[] deathSprite;
    public Sprite winSprite;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!isCanStart)
            return;
        if (GameManager.instance.playerData.isHasWin)
            return;
        if (GameManager.instance.isPauseGame || !GameManager.instance.isStartGame)
            return;
        // if (isHasTimer)
        // {
        if (isTimeCountDown)
            TimeCountdown();
        else if (isTimeScore)
            TimeScore();
        //}
        else if (isRun)
            UpdateMap();
    }

    //setup inGameUi
    public void SetupInGameUi(bool isShow)
    {
        if (isShow)
        {
            if (isTimeCountDown)
                timer.gameObject.SetActive(true);
            else if (isTimeScore)
            {
                timer.gameObject.SetActive(true);
                timeLeft = 0;
            }
            else if (isRun)
            {
                runMap.gameObject.SetActive(true);
                totalRunLength = endLine.transform.position.x - startLine.transform.position.x;
                totalTime = timeLeft;
                prevPlayerLinePos = playerLine.transform.position;
            }
            dangerIndicator.SetActive(true);
            if (GameManager.instance.inGame.isChallengeStage)
            {
                buildText.gameObject.SetActive(true);
                prevTimer.gameObject.SetActive(true);
                prevBuildText.gameObject.SetActive(true);
                Debug.Log("set ch ui");
                //set previous high score
                prevTimer.text = "Prev Time: " + GameManager.instance.ChangeFloatToTime(GameManager.instance.listChScoreTime[GameManager.instance.inGame.challengeStageNo]);
                prevBuildText.text = "Prev Build: " + GameManager.instance.listChScoreBuild[GameManager.instance.inGame.challengeStageNo];
            }
            isCanStart = true;
        }
        else
        {
            if (timer)
                timer.gameObject.SetActive(false);
            if (runMap)
                runMap.gameObject.SetActive(false);
            dangerIndicator.SetActive(false);
            if (buildText)
                buildText.gameObject.SetActive(false);
            if (prevTimer)
                prevTimer.gameObject.SetActive(false);
            if (prevBuildText)
                prevBuildText.gameObject.SetActive(false);
        }
    }

    //danger indicator
    public void UpdateDangerIndicator(float num)
    {
        if (!isCanStart)
            return;
        if (num < 0.1f)
            num = 0.0f;
        dangerText.text = num.ToString("F1") + "m";
    }

    //timer----------------------------------
    private void TimeScore()
    {
        timeLeft += Time.deltaTime;
        UpdateTimer(timeLeft);
    }
    private void TimeCountdown()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            UpdateTimer(timeLeft);
        }
        else
        {
            Debug.Log("player die");
            GameManager.instance.player.Death("drowning");
        }
    }
    private void UpdateTimer(float timeNum)
    {
        timer.text = "Time: " + GameManager.instance.ChangeFloatToTime(timeNum);
        // //convert float seconds to timespan
        // TimeSpan time = TimeSpan.FromSeconds(timeNum);
        // if (timeNum < 3600)
        //     //set time format - min:sec
        //     timer.text = time.ToString("mm':'ss");
        // else
        //     //set time format - hour:min:sec
        //     timer.text = time.ToString("hh':'mm':'ss");
    }
    //--------------------------------------

    //run---------------------------------------
    private void UpdateMap()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            //get run length
            currentRunLength = (totalTime - timeLeft) * totalRunLength / totalTime;
            //move player line
            playerLine.transform.position = prevPlayerLinePos + new Vector3(currentRunLength, 0, 0);
        }
        else
        {
            GameManager.instance.player.Win(false);
        }
    }
    //-----------------------------------------

    //update ui challenge
    public void UpdateUiChallengeScore()
    {
        buildText.text = "Build: " + GameManager.instance.inGame.groundManager.groundCount;
    }
}
