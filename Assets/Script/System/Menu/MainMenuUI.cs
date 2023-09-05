using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private GameSettings gameSettings;
    //[SerializeField] Player player;
    //sound
    //  0       1       2       
    //play   cancel  navigate 
    [SerializeField] private AudioClip[] mainMenuUIAudioClip;
    public AudioSource mainMenuUIAudioSource;
    public GameObject musicBtnOn, musicBtnOff, SoundBtnOn, SoundBtnOff;
    public GameObject blackScreen, blackScreen2, firstScreen;
    public Slider musicSlider, soundSlider;
    [SerializeField]
    private Animator mainMenuAnim, backgroundAnim, playerInfoWindowAnim, settingWindowAnim, playerInfoBtnAnim, playerLvlBtnAnim;
    public Animator loadingScreenAnim, tipModules, levelWindowAnim, tipAnim, tipScreenAnim, challengeWindowAnim, updateStatusAnim;
    [SerializeField] private Animator rateReviewWindowAnim, taskWindowAnim;
    //player info window
    public Image playerImg;
    public TextMeshProUGUI lvlText, hpText, abcText, shieldText, strengthText, addWordPtText, coinText, bookText;
    //book btn ads 
    public Button bookBtn, coinBtn;
    //public Image bookAdsImg;
    //public GameObject[] stageBtnObj;
    public Button[] stageBtnBtn;
    public GameObject[] stageBtnFalse;
    [SerializeField] private ScrollRect levelScrollRect;
    [SerializeField] private Image[] challengeCompleteImg;
    [SerializeField] private TextMeshProUGUI[] challengeCompleteText, challengeImcompleteText, chTimeText, chBuildTime;
    [SerializeField] private GameObject challengeWindowBtnObj;
    [SerializeField] private Button[] taskBtn;
    [SerializeField] private GameObject[] taskCompleteObj;
    //private bool isLoadingScreenAnimate;
    // Start is called before the first frame update
    void Start()
    {

    }

    //USED () - in tipAdsBtn
    //get tip by rewarded ads
    public void TipAdsButton()
    {
        GameManager.instance.TipAds();
    }

    //start game - USED IN () = start button
    public void StartButton(string name)
    {
        GameManager.instance.StartGame(name, 0);
    }

    public void StartButtonRun(string name)
    {
        GameManager.instance.StartGame(name, 1);
    }

    //setting - USED IN () = setting button
    public void SettingButton()
    {
        UpdateSoundSetting(gameSettings.musicVolume, gameSettings.soundVolume, gameSettings.isMusicOn, gameSettings.isSoundOn);
    }

    //update status--------------------------------------
    //TODO () - DELETE ()
    public void ShowUpdateStatus()
    {
        if (GameManager.instance.listUpdateNo.Count > 0)
            updateStatusAnim.SetInteger("state", 1);
    }
    public void CloseUpdateStatus()
    {
        updateStatusAnim.SetInteger("state", 0);
        GameManager.instance.RemoveListUpdateNo(0);
    }
    //--------------------------------------------------

    //level window------------------------------
    //USED () - in normalPlayBtn
    public void LevelWindow()
    {
        //hide stage btn
        foreach (var item in stageBtnBtn)
        {
            //item.SetActive(false);
            item.enabled = false;
        }
        foreach (var item in stageBtnFalse)
        {
            //item.SetActive(false);
            item.SetActive(true);
        }
        //unhide stage btn 
        for (int i = 0; i <= GameManager.instance.passStageNo + 1; i++)
        {
            //prevent from enable non exist btn
            if (i >= stageBtnBtn.Length)
                return;
            // stageBtnObj[i].SetActive(true);
            stageBtnBtn[i].enabled = true;
            stageBtnFalse[i].SetActive(false);
        }
        //TUTORIAL MODE () - ended - hide stage btn
        if (GameManager.instance.isHasTutorial)
        {
            stageBtnBtn[0].enabled = false;
            stageBtnFalse[0].SetActive(true);
        }
        //reset scroll
        levelScrollRect.verticalNormalizedPosition = 1;
    }
    //----------------------------------------

    //challenge window--------------------------
    //USED () - in challengePlayBtn
    public void ChallengeWindow()
    {
        //hide all
        for (int i = 0; i < challengeCompleteImg.Length; i++)
        {
            challengeCompleteImg[i].enabled = false;
            challengeCompleteText[i].enabled = false;
            challengeImcompleteText[i].enabled = true;
        }
        //set challenge score status
        for (int i = 0; i < chTimeText.Length; i++)
        {
            if (GameManager.instance.listChScoreTime[i] != 0)
            {
                chTimeText[i].text = "Time : " + GameManager.instance.ChangeFloatToTime(GameManager.instance.listChScoreTime[i]);
                chBuildTime[i].text = "Build : " + GameManager.instance.listChScoreBuild[i];
            }
        }
        //check if not pass ch
        if (GameManager.instance.challengePassNo.Count < 1)
            return;
        //show complete stat for every challenge stage completed
        foreach (var item in GameManager.instance.challengePassNo)
        {
            challengeCompleteImg[item].enabled = true;
            challengeCompleteText[item].enabled = true;
            challengeImcompleteText[item].enabled = false;
        }
    }
    //------------------------------------------

    //player info window----------------------
    //USED () - in player info btn
    public void PlayerInfoWindow()
    {
        //Update player info
        //make show level up option only
        GameManager.instance.player.LevelUp(true);
        if (GameManager.instance.playerData.levelPlayer == 8)
            lvlText.text = "Lv " + GameManager.instance.playerData.levelPlayer + " (MAX)";
        else
            lvlText.text = "Lv " + GameManager.instance.playerData.levelPlayer;
        hpText.text = GameManager.instance.playerData.hp.ToString();
        abcText.text = GameManager.instance.playerData.charMaxNo.ToString();
        shieldText.text = GameManager.instance.playerData.immuneDamageDuration / 50 + "sec";
        strengthText.text = GameManager.instance.playerData.levelPlayer.ToString();
        addWordPtText.text = "+" + GameManager.instance.playerData.addWordPt;
        coinText.text = GameManager.instance.coin.ToString() + " (" + CoinReqLvlUpText() + ")";
        bookText.text = GameManager.instance.playerData.bookNum + " (" + BookReqLvlUpText() + ")";
        GameManager.instance.gameMenuUi.SetCoinEvent();
        //show if book have free gift ads
        if (!GameManager.instance.isBookAdsUsed)
        {
            bookBtn.enabled = true;
            coinBtn.enabled = true;
            //bookAdsImg.enabled = true;
            PlayerInfoWindowAnim(2);
        }
        else
        {
            bookBtn.enabled = false;
            coinBtn.enabled = false;
            //bookAdsImg.enabled = false;
            PlayerInfoWindowAnim(1);
        }
    }
    //show level up requirement text
    private string CoinReqLvlUpText()
    {
        int coinNeed;
        switch (GameManager.instance.playerData.levelPlayer + 1)
        {
            case 2:
                coinNeed = GameManager.instance.coin - 10;
                break;
            case 3:
                coinNeed = GameManager.instance.coin - 50;
                break;
            case 4:
                coinNeed = GameManager.instance.coin - 90;
                break;
            case 5:
                coinNeed = GameManager.instance.coin - 140;
                break;
            case 6:
                coinNeed = GameManager.instance.coin - 200;
                break;
            case 7:
                coinNeed = GameManager.instance.coin - 270;
                break;
            case 8:
                coinNeed = GameManager.instance.coin - 340;
                break;
            default:
                return "";
        }
        //add + if has more coin than needed
        if (coinNeed - 0 > 0)
            return "+" + coinNeed;
        else
            return coinNeed.ToString();
    }
    private string BookReqLvlUpText()
    {
        int bookNeed;
        switch (GameManager.instance.playerData.levelPlayer + 1)
        {
            case 2:
                bookNeed = GameManager.instance.playerData.bookNum - 1;
                break;
            case 3:
                bookNeed = GameManager.instance.playerData.bookNum - 4;
                break;
            case 4:
                bookNeed = GameManager.instance.playerData.bookNum - 8;
                break;
            case 5:
                bookNeed = GameManager.instance.playerData.bookNum - 13;
                break;
            case 6:
                bookNeed = GameManager.instance.playerData.bookNum - 19;
                break;
            case 7:
                bookNeed = GameManager.instance.playerData.bookNum - 26;
                break;
            case 8:
                bookNeed = GameManager.instance.playerData.bookNum - 33;
                break;
            default:
                return "";
        }
        //add + if has more book than needed
        if (bookNeed - 0 > 0)
            return "+" + bookNeed;
        else
            return bookNeed.ToString();
    }
    //USED () - in player lvl btn
    public void LevelUp()
    {
        //levele up
        GameManager.instance.player.LevelUp(false);
        PlayerInfoWindow();
    }
    //USED () - in bookBtn
    //get book by watching ads
    public void BookAdsBtn()
    {
        GameManager.instance.adsMediate.ShowRewarded("book");
        //GameManager.instance.GetBook();
    }
    //USED () - in coinBtn
    //get coin by watching ads
    public void CoinAdsBtn()
    {
        GameManager.instance.adsMediate.ShowRewarded("coin");
        //GameManager.instance.GetBook();
    }
    //----------------------------------------

    //setting window--------------------------
    //music
    public void MusicToggle(bool isWantOn)
    {
        musicBtnOn.SetActive(isWantOn);
        musicBtnOff.SetActive(!isWantOn);
        //on/off music
        GameManager.instance.gameSettings.TurnOnMusicVolume(isWantOn);
    }
    //sound effect
    public void SoundToggle(bool isWantOn)
    {
        SoundBtnOn.SetActive(isWantOn);
        SoundBtnOff.SetActive(!isWantOn);
        //on/off sound
        GameManager.instance.gameSettings.TurnOnSoundVolume(isWantOn);
    }
    //USED IN () - credit button
    public void CreditButton()
    {

    }
    //USED IN () - support button
    public void SupportButton()
    {

    }
    //USED IN () - musicSlide
    public void ChangeMusicVolume()
    {
        GameManager.instance.gameSettings.ChangeMusicVolumeSystem(musicSlider.value);
    }
    //USED IN () - soundSlide
    public void ChangeSoundVolume()
    {
        GameManager.instance.gameSettings.ChangeSoundVolumeSystem(soundSlider.value);
    }

    //update sound setting
    public void UpdateSoundSetting(float musicVolume, float soundVolume, bool isMusicOn, bool isSoundOn)
    {
        musicSlider.value = musicVolume;
        soundSlider.value = soundVolume;
        musicBtnOn.SetActive(isMusicOn);
        musicBtnOff.SetActive(!isMusicOn);
        SoundBtnOn.SetActive(isSoundOn);
        SoundBtnOff.SetActive(!isSoundOn);
    }

    //email support
    //USED () - in email support btn
    public void EmailSupportBtn(string emailName)
    {
        GameManager.instance.connectBrowser.EmailSupportUrl(emailName);
    }

    //email review
    //USED () - in review btn
    public void EmailReviewBtn(string emailName)
    {
        GameManager.instance.connectBrowser.EamilReviewUrl(emailName);
    }

    //delete user account
    //USED () - in deleteBtn
    public void RequestDeleteAccount()
    {
        GameManager.instance.RequestDeleteUserAccDb();
    }

    //rate review window --------------------------------
    public void ShowRateReviewWindow(int num)
    {
        rateReviewWindowAnim.SetInteger("state", num);
        if (num == 0)
            GameManager.instance.SaveState(true, false);
    }
    //rate review btn
    //USED () - in rate btn - in rate review window
    public void RateBtn()
    {
        GameManager.instance.connectBrowser.OpenUrl("https://play.google.com/store/apps/details?id=com.MohdNazir.WordGather&pcampaignid=web_share");
        GameManager.instance.isHasRateReview = true;
        ShowRateReviewWindow(0);
        RewardBookFromTask(5);
    }
    //--------------------------------------------------
    //task window --------------------------------------
    public void ShowTaskWindow(int num)
    {
        taskWindowAnim.SetInteger("state", num);
        if (num == 0)
            GameManager.instance.SaveState(true, false);
        TaskBtnStatus(0, GameManager.instance.isHasRateReview);
        TaskBtnStatus(1, GameManager.instance.gameData.isHasFbShare);
        TaskBtnStatus(2, GameManager.instance.gameData.isHasTwShare);
        TaskBtnStatus(3, GameManager.instance.gameData.isHasWsShare);
    }
    //USED () - in play store btn - in task window
    public void TaskRateBtn()
    {
        GameManager.instance.connectBrowser.OpenUrl("https://play.google.com/store/apps/details?id=com.MohdNazir.WordGather&pcampaignid=web_share");
        GameManager.instance.isHasRateReview = true;
        RewardBookFromTask(5);
        TaskBtnStatus(0, true);
    }
    //USED () - in fb share btn
    public void ShareFacebookBtn()
    {
        GameManager.instance.connectBrowser.OpenUrl("https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fplay.google.com%2Fstore%2Fapps%2Fdetails%3Fid%3Dcom.MohdNazir.WordGather%26pcampaignid%3Dweb_share");
        GameManager.instance.gameData.isHasFbShare = true;
        RewardBookFromTask(3);
        TaskBtnStatus(1, true);
    }
    //USED () - in tw share btn
    public void ShareTwitterBtn()
    {
        GameManager.instance.connectBrowser.OpenUrl("https://twitter.com/intent/tweet?url=https%3A%2F%2Fplay.google.com%2Fstore%2Fapps%2Fdetails%3Fid%3Dcom.MohdNazir.WordGather%26pcampaignid%3Dweb_share&via=GooglePlay&text=Have%20a%20look%20at%20%27Word%20Gather%20-%20Word%20Escape%20Run%27");
        GameManager.instance.gameData.isHasTwShare = true;
        RewardBookFromTask(3);
        TaskBtnStatus(2, true);
    }
    //USED () - in ws share btn
    public void ShareWhatsappBtn()
    {
        GameManager.instance.connectBrowser.OpenUrl("https://api.whatsapp.com/send/?text=https%3A%2F%2Fplay.google.com%2Fstore%2Fapps%2Fdetails%3Fid%3Dcom.MohdNazir.WordGather%26pcampaignid%3Dweb_share&type=custom_url");
        GameManager.instance.gameData.isHasWsShare = true;
        RewardBookFromTask(3);
        TaskBtnStatus(3, true);
    }
    private void RewardBookFromTask(int numBook)
    {
        GameManager.instance.player.ReceiveBook(numBook);
        GameManager.instance.SaveState(true, false);
        StartCoroutine(DelayGetBookPopUp(numBook));
    }
    private IEnumerator DelayGetBookPopUp(int numBook)
    {
        yield return new WaitForSeconds(0.5f);
        GameManager.instance.popUpUi.GetItemPopUp(true, false, numBook, 0);
    }
    //decide to make task btn interactable based on task completion
    private void TaskBtnStatus(int numTaskBtn, bool isComplete)
    {
        if (isComplete)
        {
            taskBtn[numTaskBtn].enabled = false;
            taskCompleteObj[numTaskBtn].SetActive(true);
        }
        else
        {
            taskBtn[numTaskBtn].enabled = true;
            taskCompleteObj[numTaskBtn].SetActive(false);
        }
    }
    //--------------------------------------------------

    //set player lvl upgrade notice
    public void SetPlayerUpgradeNotice(bool isUpgradable)
    {
        playerInfoBtnAnim.SetBool("upgradable", isUpgradable);
        playerLvlBtnAnim.SetBool("upgradable", isUpgradable);
    }

    //loading screen
    public void ShowLoadingScreen(bool isShow, string sceneName)
    {
        if (isShow)
        {
            switch (sceneName)
            {
                case "Stage 1-1":
                    ShowLoadingScreenEvent(9);
                    break;
                case "Stage 1-2":
                    ShowLoadingScreenEvent(3);
                    break;
                case "Stage 1-3":
                    ShowLoadingScreenEvent(7);
                    break;
                case "Stage 2-1":
                    ShowLoadingScreenEvent(8);
                    break;
                case "Stage 2-2":
                    ShowLoadingScreenEvent(2);
                    break;
                case "Stage 2-3":
                    ShowLoadingScreenEvent(1);
                    break;
                // case "Stage 3-3":
                //     ShowLoadingScreenEvent(10);
                //     break;
                case "Stage 4-1":
                    ShowLoadingScreenEvent(13);
                    break;
                case "Stage 4-2":
                    ShowLoadingScreenEvent(14);
                    break;
                case "Challenge Drown FirstEnd":
                    ShowLoadingScreenEvent(16);
                    break;
                default:
                    //change tip module randomly
                    int tipNo = Random.Range(1, 15);
                    ShowLoadingScreenEvent(tipNo);
                    break;
            }
        }
        else
        {
            ShowLoadingScreenEvent(0);
        }
    }
    private void ShowLoadingScreenEvent(int tipNo)
    {
        //change tip module
        tipModules.SetInteger("state", tipNo);
        if (tipNo == 0)
            //hide loading screen
            loadingScreenAnim.SetInteger("state", 0);
        else
            //show loading screen
            loadingScreenAnim.SetInteger("state", 3);
    }

    //show tip screen
    public void ShowTip()
    {
        tipScreenAnim.SetBool("show", true);
        int tipNo = Random.Range(1, 15);
        tipAnim.SetInteger("state", tipNo);
    }
    //USED () - in closeTipBtn
    //close tip screen
    public void CloseTip()
    {
        tipAnim.SetInteger("state", 0);
        tipScreenAnim.SetBool("show", false);
    }

    //unlock full stage
    public void UnlockFullStage()
    {
        GameManager.instance.passStageNo = 24;
    }

    //open url
    //USED () - in facebook btn
    public void OpenUrl(string urlName)
    {
        GameManager.instance.connectBrowser.OpenUrl(urlName);
    }

    //animation-------------------------------------------
    public IEnumerator ShowAnim()
    {
        yield return new WaitForSeconds(0);
        MainMenuAnim(1);
        BackgroundAnim(1);
        Debug.Log("main menu show");
        GameManager.instance.OnMainMenu();
    }
    public IEnumerator HideAnim()
    {
        yield return new WaitForSeconds(0);
        //mainMenuAnim.SetTrigger("hide");
        BackgroundAnim(0);
        Debug.Log("main menu hide");
    }
    public void MainMenuAnim(int num)
    {
        mainMenuAnim.SetInteger("state", num);
        //show challenge button if more than 2 stage
        if (GameManager.instance.passStageNo > 2)
            challengeWindowBtnObj.SetActive(true);
        else
            challengeWindowBtnObj.SetActive(false);
    }
    public void BackgroundAnim(int num)
    {
        backgroundAnim.SetInteger("state", num);
    }
    public void PlayerInfoWindowAnim(int num)
    {
        playerInfoWindowAnim.SetInteger("state", num);
    }
    public void SettingWindowAnim(int num)
    {
        settingWindowAnim.SetInteger("state", num);
    }
    public void LevelWindowAnim(int num)
    {
        levelWindowAnim.SetInteger("state", num);
    }
    public void ChallengeWindowAnim(int num)
    {
        challengeWindowAnim.SetInteger("state", num);
    }
    public void TaskWindowAnim(int num)
    {
        taskWindowAnim.SetInteger("state", num);
    }
    //---------------------------------------------------

    //play sound -------------------------------------------
    public void PlaySoundPlay()
    {
        if (mainMenuUIAudioSource.isPlaying)
            return;
        mainMenuUIAudioSource.PlayOneShot(mainMenuUIAudioClip[0]);
    }
    public void PlaySoundCancel()
    {
        if (mainMenuUIAudioSource.isPlaying)
            return;
        mainMenuUIAudioSource.PlayOneShot(mainMenuUIAudioClip[1]);
    }
    public void PlaySoundNavigate()
    {
        if (mainMenuUIAudioSource.isPlaying)
            return;
        mainMenuUIAudioSource.PlayOneShot(mainMenuUIAudioClip[2]);
    }
    //----------------------------------------------------
}
