using System.Collections;
using System.Collections.Generic;

public class Data
{
    //public bool isSoundOn;
    //public bool isMusicOn;
    public float soundVolume;
    public float musicVolume;
    public string dateNow;
    public string firstSavedDate;
    public string savedDate;
    public float playTime;
    public int passStageNo;
    public List<int> challengePassNo;
    public int bookNumCollect;
    public int playerLevel;
    public int coin;
    public bool isHasTutorial;
    public bool isPremiumPlan;
    public int diamond;
    public List<int> skinIndexBought;
    public int shieldBought;
    public int charBought;
    public List<int> listUpdateNo;
    //dont save at cloud----------------
    public bool isBookAdsUsed;
    public int bookAdsUsedNo;
    public bool isHasRateReview;
    public bool isHasShowRateReview;
    public float[] listChScoreTime = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
    public int[] listChScoreBuild = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
}
