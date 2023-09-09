using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameMenuUi : MonoBehaviour
{
    [SerializeField] GameSettings gameSettings;
    //sound
    //  0          1        2           3
    //word      build    complete     fail
    [SerializeField] private AudioClip[] gameMenuUiAudioClip;
    public AudioSource gameMenuUiAudioSource;
    //[SerializeField] private Player player;
    private CanvasGroupFunc canvasGroupFunc;
    public Button homeBtn, settingBtn;
    //private InGame inGame;
    public GameObject[] alphabetPlayerInfoContainer;
    public List<char> alphabetsWord;
    public GameObject[] charUIInfoObj1, charUIInfoObj2;
    public Image[] charUIInfoImg1, charUIInfoImg2;
    public GameObject[] alphabetStoreContainer, alphabetWordStoreContainer;
    public Button[] alphabetStoreBtn, alphabetStoreBtn2, alphabetTutorialBtn;
    public Image[] alphabetStoreBtnImg, alphabetStoreBtnImg2, alphabetTutorialBtnImg;
    public Button[] alphabetWordBtn, alphabetWordTutorialBtn;
    public Image[] alphabetWordBtnImg, alphabetWordTutorialBtnImg;
    //word pt
    public GameObject wordPtObj, wordCoinObj;
    public TextMeshProUGUI wordPtText, wordCoinText;
    public Image hpImg;
    //public RectTransform hpImgRT;
    public GameObject hpNotice;
    public Animator hpNoticeAnim, wordContainerAnim, wordTutorialContainerAnim;
    public Sprite[] hpSprite;
    public PlayerData playerData;
    public Animator gameMenuUiAnim;
    public CanvasGroup buildCooldownCG, buildLimitCG;
    public TextMeshProUGUI buildCooldownText;
    public GameObject completeLadderImg;
    public CanvasGroup[] builBtnCG;
    public Image[] buildBtnImg;
    public GameObject musicBtnOn, musicBtnOff, SoundBtnOn, SoundBtnOff;
    public Slider musicSlider, soundSlider;
    [SerializeField] private Image deathImg, winImg;
    [SerializeField] private TextMeshProUGUI bookNumText, pointText;
    [SerializeField] private TextMeshProUGUI[] buildText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextAsset wordList;
    [SerializeField] private Image uiFill;
    //status
    [SerializeField] private Image statusInfoImg, statusActionImg;
    //  0
    //doubleEarn, 
    [SerializeField] private Sprite[] statusSprite;
    //shield earn
    [SerializeField] private Button shieldBtn;
    [SerializeField] private TextMeshProUGUI shieldText;
    //death challenge
    [SerializeField] private TextMeshProUGUI timeChText, scoreText;
    //first end
    [SerializeField] private GameObject specFirstAlpContainerInfo, specFirstAlpContainerAction;
    [SerializeField] private Image[] specFirstAlpInfoObj, specFirstAlpActionObj;
    [SerializeField] private GameObject firstEndNoti;
    //[SerializeField] private Text uiText;
    private List<string> words;
    private string letterCombine;
    public int wordPoint;
    private int wordBtnNum;
    public bool isRunGame, isHasPlayCancel;
    [SerializeField] private bool isCharLvl1, isObjBuildBtnClickable = true;

    [SerializeField] private int objBuildCooldownNum = 100, objBuildCooldownDuration;
    //timer    
    [SerializeField] private int deathClockDuration;
    private int remainingDurationClock;
    private bool isStartdeathClock;
    //for tutorial mode
    private bool isTutorialCombineClicked;
    //first end
    [SerializeField] private string end1, end2, end3;
    void Awake()
    {
        //convert word in .txt to string word
        words = new List<string>(wordList.text.Split(new char[]{
            ',', ' ', '\n', '\r'},
            System.StringSplitOptions.RemoveEmptyEntries
        ));
    }

    // Start is called before the first frame update
    void Start()
    {
        canvasGroupFunc = GameManager.instance.canvasGroupFunc;
    }

    // 50 frame per sec
    void FixedUpdate()
    {
        if (isStartdeathClock)
            UpdateTimerDeathClock();
        if (GameManager.instance.isPauseGame)
            return;
        if (GameManager.instance.inGame.isFence || GameManager.instance.inGame.isSlime)
        {
            //stop countdown
            if (isObjBuildBtnClickable)
                return;
            if (objBuildCooldownNum < objBuildCooldownDuration)
                objBuildCooldownNum++;
            //SetBuildCooldown();
        }
    }

    //set game menu ui mode base on game mode
    public void SetGameMenuUIMode(bool isRun)
    {
        // Debug.Log("tutorialMode = " + GameManager.instance.isTutorialMode);
        // Debug.Log("set game menu ui");
        // inGame = GameManager.instance.inGame;
        isRunGame = isRun;
        if (!isRun)
        {
            //for drowned game
            gameMenuUiAnim.SetTrigger("info");
            ShowBuildCooldownObj(false);
        }
        else
        {
            //for run game
            gameMenuUiAnim.SetTrigger("infoHp");
            //ShowBuildCooldownObj(true);
        }
        SetCharUIInfo();
        SetCharUIAction();
        SetCharUiWord();
        SetBuildBtnsActive();
        //SetBuildBtnsInteracteable();
        //Challenge MODE () - FIRST END
        if (GameManager.instance.inGame.isFirstEnd)
            ShowFirstUi(true);
        else
            ShowFirstUi(false);
    }

    //action menu
    public void SetActionMenu()
    {
        SetCoinEvent();
        SetBuildCooldown();
        SetBuildLimit();
        GameManager.instance.player.SetShieldNum();
        WordConAnimStatus(0);
        if (objBuildCooldownNum >= objBuildCooldownDuration)
        {
            SetRunBuildBtn(true);
            SetBuildBtnsInteracteable();
        }
        //TUTORIAL MODE ()
        if (GameManager.instance.isTutorialMode)
        {
            homeBtn.enabled = false;
            settingBtn.enabled = false;
        }
        else
        {
            homeBtn.enabled = true;
            settingBtn.enabled = true;
        }
        //Challenge MODE () - FIRST END
        if (GameManager.instance.inGame.isFirstEnd)
            ChangeFirstUiAction();
    }
    //set build button active event
    public void SetBuildBtnsActive()
    {
        SetBuildBtnActive(0, GameManager.instance.inGame.isLadder, GameManager.instance.inGame.ladderPt);
        SetBuildBtnActive(1, GameManager.instance.inGame.isGround, GameManager.instance.inGame.groundPt);
        SetBuildBtnActive(2, GameManager.instance.inGame.isFence, GameManager.instance.inGame.fencePt);
        SetBuildBtnActive(3, GameManager.instance.inGame.isSlime, GameManager.instance.inGame.slimePt);
    }
    private void SetBuildBtnActive(int buildBtnNo, bool isActive, int point)
    {
        builBtnCG[buildBtnNo].gameObject.SetActive(isActive);
        if (isActive)
        {
            buildText[buildBtnNo].text = point.ToString();
            buildBtnImg[buildBtnNo].sprite = GameManager.instance.inGame.builderSprite[buildBtnNo];
        }
    }

    //set build cooldown obj in game menu ui
    private void SetBuildCooldown()
    {
        if (objBuildCooldownNum < objBuildCooldownDuration)
        {
            ShowBuildCooldownObj(true);
            buildCooldownText.text = (2 - objBuildCooldownNum / 50).ToString();
        }
        else
            ShowBuildCooldownObj(false);
    }

    //show build cooldown obj in game menu ui
    private void ShowBuildCooldownObj(bool isShow)
    {
        if (!GameManager.instance.inGameUi.isRun)
        {
            GameManager.instance.canvasGroupFunc.ModifyCG(buildCooldownCG, 0, isShow, isShow);
            return;
        }
        if (isShow)
        {
            GameManager.instance.canvasGroupFunc.ModifyCG(buildCooldownCG, 1, isShow, isShow);
            //Debug.Log("build cooldown show");
        }
        else
        {
            GameManager.instance.canvasGroupFunc.ModifyCG(buildCooldownCG, 0, isShow, isShow);
            //Debug.Log("build cooldown hide");
        }
    }

    //set build limit
    private void SetBuildLimit()
    {
        //TUTORIAL MODE ()
        if (!GameManager.instance.inGameUi.isRun)
        {
            GameManager.instance.canvasGroupFunc.ModifyCG(buildLimitCG, 0, false, false);
            return;
        }
        if (GameManager.instance.inGame.builderInRun.objBuildInGame >= GameManager.instance.inGame.builderInRun.objBuildInGameLimit)
            GameManager.instance.canvasGroupFunc.ModifyCG(buildLimitCG, 1, true, true);
        else
            GameManager.instance.canvasGroupFunc.ModifyCG(buildLimitCG, 0, false, false);
    }

    //add char in player
    public void SetCharPlayer()
    {
        //show in the player info ui
        //alphabetsPlayerInfo.Add(abc);
        //Debug.Log("abc count = " + alphabetsPlayerInfo.Count);
        // if (alphabetsPlayerInfo.Count > 10)
        //     alphabetsPlayerInfo.RemoveAt(0);
        //set char ui
        SetCharUIInfo();
        SetCharUIAction();
    }
    //remove char in player ui
    public void RemoveCharUi(int charIndex)
    {
        //alphabetsPlayerInfo.RemoveAt(charIndex);
        //alphabetsPlayerInfo.Remove(charIndex);--
        SetCharUIInfo();
        SetCharUIAction();
    }
    public void RemoveCharUi(int charIndex, char alphabet)
    {
        //alphabetsPlayerInfo.RemoveAt(charIndex);---
        //alphabetsPlayerInfo.Remove(alphabet);
        SetCharUIInfo();
        SetCharUIAction();
    }
    //remove all char in player ui
    public void RemoveAllCharUI()
    {
        //alphabetsPlayerInfo.RemoveRange(0, alphabetsPlayerInfo.Count);
        SetCharUIInfo();
        SetCharUIAction();
    }

    //remove char in player - 
    public void RemoveCharPlayer(int charIndex)
    {
        try
        {
            GameManager.instance.player.RemoveChar(charIndex);
            SetCharUIInfo();
            SetCharUIAction();
        }
        catch (System.Exception e)
        {
            Debug.Log("failed remove char Player" + e.Message);
        }
    }

    //remove char in word - 
    public void RemoveCharWord()
    {
        alphabetsWord.RemoveRange(0, alphabetsWord.Count);
        SetCharUiWord();
    }

    //set char UI Info
    public void SetCharUIInfo()
    {
        //info ()
        if (playerData.charMaxNo < 11)
        {
            //make active for contain char
            for (int i = 0; i < GameManager.instance.player.alphabetsStore.Count; i++)
            {
                charUIInfoObj1[i].SetActive(true);
                //set image
                charUIInfoImg1[i].sprite = GameManager.instance.alphabetSprite[(int)GameManager.instance.player.alphabetsStore[i] - 65];
            }
            //make unactive for remaining
            for (int i = charUIInfoObj1.Length - 1; i >= GameManager.instance.player.alphabetsStore.Count; i--)
            {
                charUIInfoObj1[i].SetActive(false);
            }
        }
        else
        {
            //make active for contain char
            for (int i = 0; i < GameManager.instance.player.alphabetsStore.Count; i++)
            {
                charUIInfoObj2[i].SetActive(true);
                //set image
                charUIInfoImg2[i].sprite = GameManager.instance.alphabetSprite[(int)GameManager.instance.player.alphabetsStore[i] - 65];
            }
            //make unactive for remaining
            for (int i = charUIInfoObj2.Length - 1; i >= GameManager.instance.player.alphabetsStore.Count; i--)
            {
                charUIInfoObj2[i].SetActive(false);
            }
        }

    }

    //set char UI Info
    public void SetCharUIAction()
    {
        //TUTORIAL MODE ()
        if (GameManager.instance.isTutorialMode)
        {
            //make active for contain char
            for (int i = 0; i < GameManager.instance.player.alphabetsStore.Count; i++)
            {
                alphabetTutorialBtn[i].gameObject.SetActive(true);
                //set image
                alphabetTutorialBtnImg[i].sprite = GameManager.instance.alphabetSprite[(int)GameManager.instance.player.alphabetsStore[i] - 65];
            }
            //make unactive for remaining
            for (int i = alphabetTutorialBtn.Length - 1; i >= GameManager.instance.player.alphabetsStore.Count; i--)
            {
                alphabetTutorialBtn[i].gameObject.SetActive(false);
            }
        }
        else
        {
            if (!isCharLvl1)
            {
                //make active for contain char
                for (int i = 0; i < GameManager.instance.player.alphabetsStore.Count; i++)
                {
                    alphabetStoreBtn[i].gameObject.SetActive(true);
                    //set image
                    alphabetStoreBtnImg[i].sprite = GameManager.instance.alphabetSprite[(int)GameManager.instance.player.alphabetsStore[i] - 65];
                }
                //make unactive for remaining
                for (int i = alphabetStoreBtn.Length - 1; i >= GameManager.instance.player.alphabetsStore.Count; i--)
                {
                    alphabetStoreBtn[i].gameObject.SetActive(false);
                }
            }
            else
            {
                //make active for contain char
                for (int i = 0; i < GameManager.instance.player.alphabetsStore.Count; i++)
                {
                    alphabetStoreBtn2[i].gameObject.SetActive(true);
                    //set image
                    alphabetStoreBtnImg2[i].sprite = GameManager.instance.alphabetSprite[(int)GameManager.instance.player.alphabetsStore[i] - 65];
                }
                //make unactive for remaining
                for (int i = alphabetStoreBtn2.Length - 1; i >= GameManager.instance.player.alphabetsStore.Count; i--)
                {
                    alphabetStoreBtn2[i].gameObject.SetActive(false);
                }
            }
        }
    }

    //set char ui word
    public void SetCharUiWord()
    {
        try
        {
            //TUTORIAL MODE () 
            if (GameManager.instance.isTutorialMode)
            {
                //make active for contain char
                for (int i = 0; i < alphabetsWord.Count; i++)
                {
                    alphabetWordTutorialBtn[i].gameObject.SetActive(true);
                    //set image
                    alphabetWordTutorialBtnImg[i].sprite = GameManager.instance.alphabetSprite[(int)alphabetsWord[i] - 65];
                }
                //make unactive for remaining
                for (int i = alphabetWordTutorialBtn.Length - 1; i >= alphabetsWord.Count; i--)
                {
                    alphabetWordTutorialBtn[i].gameObject.SetActive(false);
                }
            }
            else
            {
                //make active for contain char
                for (int i = 0; i < alphabetsWord.Count; i++)
                {
                    alphabetWordBtn[i].gameObject.SetActive(true);
                    //set image
                    alphabetWordBtnImg[i].sprite = GameManager.instance.alphabetSprite[(int)alphabetsWord[i] - 65];
                }
                //make unactive for remaining
                for (int i = alphabetWordBtn.Length - 1; i >= alphabetsWord.Count; i--)
                {
                    alphabetWordBtn[i].gameObject.SetActive(false);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("failed set Char Ui Word" + e.Message);
        }
    }

    public void SetPlayerLevelUI(int charLvl)
    {
        //set hp lvl ui and char container ui in menu
        hpImg.sprite = hpSprite[GameManager.instance.playerData.hpTemp];
        //TUTORIAL MODE ()
        if (GameManager.instance.isTutorialMode)
        {
            alphabetStoreContainer[0].SetActive(false);
            alphabetStoreContainer[1].SetActive(false);
            alphabetStoreContainer[2].SetActive(true);

            alphabetWordStoreContainer[0].SetActive(false);
            alphabetWordStoreContainer[1].SetActive(true);
        }
        else
        {
            alphabetWordStoreContainer[0].SetActive(true);
            alphabetWordStoreContainer[1].SetActive(false);

            if (charLvl == 0)
            {
                alphabetStoreContainer[0].SetActive(true);
                alphabetStoreContainer[1].SetActive(false);
                alphabetStoreContainer[2].SetActive(false);
                alphabetPlayerInfoContainer[0].SetActive(true);
                alphabetPlayerInfoContainer[1].SetActive(false);
                isCharLvl1 = false;
                //hpImgRT.position = new Vector3(hpImgRT.position.x, -195, hpImgRT.position.z);
            }
            else
            {
                alphabetStoreContainer[0].SetActive(false);
                alphabetStoreContainer[1].SetActive(true);
                alphabetStoreContainer[2].SetActive(false);
                alphabetPlayerInfoContainer[0].SetActive(false);
                alphabetPlayerInfoContainer[1].SetActive(true);
                isCharLvl1 = true;
                //hpImgRT.position = new Vector3(hpImgRT.position.x, -115, hpImgRT.position.z);
            }
        }
    }
    public void SetHpUI(bool isHeal)
    {
        hpImg.sprite = hpSprite[GameManager.instance.playerData.hpTemp];
        //Debug.Log("change hp Ui");
        if (GameManager.instance.playerData.hpTemp < 2)
            hpNotice.SetActive(true);
        else
        {
            if (isHeal)
                hpNotice.SetActive(false);
            else
                StartCoroutine(HpAlertTemp());
        }
    }

    public IEnumerator HpAlertTemp()
    {
        hpNotice.SetActive(true);
        hpNoticeAnim.SetTrigger("fast");
        yield return new WaitForSeconds(0.31f);
        hpNotice.SetActive(false);
    }

    //close action menu - USED () - in close btn in action menu
    public void CloseActionMenu()
    {
        //TUTORIAL MODE ()
        if (GameManager.instance.isTutorialMode)
            if (GameManager.instance.tutorial.TutorialPhaseNo != 11)
                return;
        ResetAlphabetWordBtnClick();
        GameManager.instance.inGame.ResetLastTimeSpawn();
        WordConAnimStatus(0);
        if (!isRunGame)
            gameMenuUiAnim.SetTrigger("info");
        else
            gameMenuUiAnim.SetTrigger("infoHp");
        canvasGroupFunc.ModifyCG(GameManager.instance.inGameUi.inGameUICG, 1, true, false);
        GameManager.instance.swipeUpDownAction.ChangeIsActionInvalid(false);
        gameSettings.UpdateMenuVolumeSetting();
        GameManager.instance.PauseGame(false);
        //Debug.Log("Game Menu ui info");
        //TUTORIAL MODE ()
        if (!GameManager.instance.isTutorialMode)
            return;
        if (GameManager.instance.tutorial.TutorialPhaseNo == 11)
        {
            GameManager.instance.tutorialUI.TutorialEvent(11);
        }
    }

    //make shield
    //USED () - in shield btn
    public void ShieldBtn()
    {
        GameManager.instance.shieldBought--;
        GameManager.instance.player.SetShieldNum();
        //make shield
        GameManager.instance.player.ChangeImmuneDamage(true);
        PlaySoundBuild();
    }

    //reset alphabet word button
    private void ResetAlphabetWordBtnClick()
    {
        int limitWord = alphabetsWord.Count;
        //Debug.Log("alphabetsWord count = " + limitWord);
        //reset word button
        for (int i = 0; i < limitWord; i++)
        {
            AlphabetWordBtnClick(0);
            //Debug.Log("word reset " + i);
        }
    }

    //USED () - in the alpahbet btn
    public void AlphabetBtnClick(int indexBtn)
    {
        //prevent from word be more than container supported
        if (wordBtnNum > 15)
            return;
        wordBtnNum++;
        try
        {
            // add in word
            alphabetsWord.Add(GameManager.instance.player.alphabetsStore[indexBtn]);
        }
        catch (System.Exception e)
        {
            Debug.Log("failed add alphabetsWord" + e.Message);
        }
        //remove in store
        RemoveCharPlayer(indexBtn);
        SetCharUiWord();
    }

    //TUTORIAL MODE ()
    //USED () - in the alpahbet btn tutorial
    public void AlphabetBtnTutorialClick(int indexBtn)
    {
        //prevent from word be more than container supported
        if (wordBtnNum > 15)
            return;
        wordBtnNum++;
        //Debug.Log("alphabet tutorial " + indexBtn);
        // add in word
        alphabetsWord.Add(GameManager.instance.player.alphabetsStore[indexBtn]);
        GameManager.instance.tutorialUI.Tutorial3AltEnd(indexBtn);
        //remove in store
        RemoveCharPlayer(indexBtn);
        SetCharUiWord();
        if (GameManager.instance.gameMenuUi.alphabetsWord.Count <= 4)
            GameManager.instance.tutorialUI.Tutorial3AltStart(indexBtn);
        else
        {
            //Debug.Log("alphabet tutorial end");
            GameManager.instance.tutorialUI.AllTutorial3AltEnd();
            GameManager.instance.tutorialUI.TutorialEnd();
            GameManager.instance.tutorialUI.TutorialEvent(4);
        }
    }

    //USED () - in the alpahbet word btn
    public void AlphabetWordBtnClick(int indexBtn)
    {
        if (GameManager.instance.isTutorialMode)
            if (GameManager.instance.tutorial.TutorialPhaseNo != 5)
                return;
        wordBtnNum--;
        //add in store
        GameManager.instance.player.AddAlphabetStore(alphabetsWord[indexBtn]);
        //remove in word
        alphabetsWord.RemoveAt(indexBtn);
        SetCharUiWord();
        //TUTORIAL MODE ()
        if (!GameManager.instance.isTutorialMode)
            return;
        if (GameManager.instance.tutorial.TutorialPhaseNo == 5)
        {
            GameManager.instance.tutorialUI.TutorialEvent(5);
            //GameManager.instance.tutorialUI.TutorialEnd();
        }
    }

    //USED () - in word button
    //convert char to word for points
    public void WordCombine()
    {
        //TUTORIAL MODE ()
        if (GameManager.instance.isTutorialMode)
            if (GameManager.instance.tutorial.TutorialPhaseNo != 6)
                return;
        letterCombine = "";
        //Debug.Log(alphabetsWord.Length);
        for (int i = 0; i < alphabetsWord.Count; i++)
        {
            wordBtnNum--;
            //Debug.Log("word combine " + i);
            //combine letter to be word
            letterCombine += alphabetsWord[i].ToString();
        }
        Debug.Log("Word = " + letterCombine);
        Debug.Log(CheckWordExist(letterCombine));
        //reward combine letter to word
        if (CheckWordExist(letterCombine))
        {
            //show success status word container
            WordConAnimStatus(1);
            //add word pt
            int wordToWordPt = letterCombine.Length + playerData.addWordPt;
            //Challenge MODE () - FIRST END
            if (GameManager.instance.inGame.isFirstEnd)
            {
                //safety measure - for first end - is empty
                if (end1 != "")
                {
                    Debug.Log("end first init");
                    //first != end1
                    if (letterCombine[0].ToString() != end1)
                    {
                        //first == end2
                        if (letterCombine[0].ToString() == end2)
                            wordToWordPt--;
                        //first == end3
                        else if (letterCombine[0].ToString() == end3)
                            wordToWordPt -= 2;
                        //first != end1 && end2 && end3
                        else
                        {
                            StartCoroutine(ShowFirstEndNoti());
                            return;
                        }
                    }
                }
                //make it so word must be created with specified first alp
                SetEndToFirst(letterCombine);
            }
            PlaySoundWord();
            //if > 5 letter - get bonus - excess letter = +wordpt
            if (letterCombine.Length > 5)
                wordToWordPt += (letterCombine.Length - 5);
            //set word pt
            wordPoint += wordToWordPt;
            StartCoroutine(ShowWordPt(wordToWordPt));
            //add coin event
            WordToCoin();
            //player.PlaySoundWord();
            //give point based on number of char in word
            Debug.Log("give " + wordToWordPt + " points");
            //remove char in data
            RemoveCharWord();
            //set word pt event
            SetWordPointEvent();
            //TUTORIAL MODE ()
            if (!GameManager.instance.isTutorialMode)
                return;
            if (isTutorialCombineClicked)
                return;
            if (GameManager.instance.tutorial.TutorialPhaseNo == 6)
            {
                isTutorialCombineClicked = true;
                GameManager.instance.tutorialUI.TutorialEvent(6);
            }
        }
        else
        {
            //show fail status word container
            WordConAnimStatus(2);
            PlaySoundFail();
            //player.PlaySoundFailed();
            ResetAlphabetWordBtnClick();
            //TUTORIAL MODE () - use for fail to combine - made tutorial 5F
            // if (!GameManager.instance.isTutorialMode)
            //     return;
            // if (GameManager.instance.tutorial.TutorialPhaseNo == 6)
            // {
            //     //GameManager.instance.tutorialUI.TutorialEvent(6);
            // }
        }
    }

    //show wordPt get ui pop up
    private IEnumerator ShowWordPt(int point)
    {
        wordPtObj.SetActive(true);
        wordPtText.text = "" + point;
        yield return new WaitForSeconds(1f);
        wordPtObj.SetActive(false);
    }

    //check word in word txt
    private bool CheckWordExist(string word)
    {
        return words.Contains(word);
    }

    //add coin event
    public void WordToCoin()
    {
        //if word is 4 - produce coin
        if (letterCombine.Length >= 4)
        {
            //get coin for letter length >= 4 --- 4 letter = 1 coin, 5 ltr = 2 coin, etc
            int addCoin = letterCombine.Length - 3;
            GameManager.instance.AddCoin(addCoin);
            StartCoroutine(ShowWordCoin(addCoin));
            GameManager.instance.player.PlaySoundChar();
            SetCoinEvent();
        }
    }
    //show word coin get ui pop up
    private IEnumerator ShowWordCoin(int addCoin)
    {
        wordCoinObj.SetActive(true);
        wordCoinText.text = "" + addCoin;
        yield return new WaitForSeconds(1f);
        wordCoinObj.SetActive(false);
    }
    //set coin in gamemenu - coin info
    public void SetCoinEvent()
    {
        coinText.text = "$" + GameManager.instance.coin.ToString();
    }

    //set word point event - set word pt text
    public void SetWordPointEvent()
    {
        //set word pt text
        pointText.text = wordPoint.ToString();
        //set build button availability
        SetBuildBtnsInteracteable();
    }

    //reset word point event
    public void ResetWordPointEvent()
    {
        wordPoint = 0;
        SetWordPointEvent();
        //reset double earn
        GameManager.instance.player.SetDoubleEarn(false);
        //reset first end
        end1 = "";
        end2 = "";
    }

    //Challenge MODE () - FIRST END--------------------
    //set end alp to be as first alp in next creation
    private void SetEndToFirst(string abcString)
    {
        int num1 = abcString.Length - 1;
        int num2 = abcString.Length - 2;
        end1 = abcString[num1].ToString();
        end2 = abcString[num2].ToString();
        //Debug.Log("abcString length = " + abcString.Length);
        //for end 3 need word with 3 alp
        if (abcString.Length > 2)
        {
            int num3 = abcString.Length - 3;
            end3 = abcString[num3].ToString();
        }
        //Debug.Log("end char = " + abc);
        ChangeFirstUiInfo(abcString);
        ChangeFirstUiAction();
    }
    //show first ui
    private void ShowFirstUi(bool isTrue)
    {
        if (isTrue)
        {
            specFirstAlpContainerInfo.SetActive(true);
            specFirstAlpContainerAction.SetActive(true);
            ResetFirstEndUi();
        }
        else
        {
            specFirstAlpContainerInfo.SetActive(false);
            specFirstAlpContainerAction.SetActive(false);
        }
    }
    //set first ui - spec first alp to be created in word
    private void ChangeFirstUiInfo(string abcString)
    {
        specFirstAlpInfoObj[0].gameObject.SetActive(true);
        specFirstAlpInfoObj[0].sprite = GameManager.instance.alphabetSprite[(int)end1[0] - 65];
        specFirstAlpInfoObj[1].gameObject.SetActive(true);
        specFirstAlpInfoObj[1].sprite = GameManager.instance.alphabetSprite[(int)end2[0] - 65];
        //for end 3 need word with 3 alp
        if (abcString.Length > 2)
        {
            specFirstAlpInfoObj[2].gameObject.SetActive(true);
            specFirstAlpInfoObj[2].sprite = GameManager.instance.alphabetSprite[(int)end3[0] - 65];
        }
    }
    private void ChangeFirstUiAction()
    {
        if (end1 == "")
            return;
        specFirstAlpActionObj[0].gameObject.SetActive(true);
        specFirstAlpActionObj[0].sprite = GameManager.instance.alphabetSprite[(int)end1[0] - 65];
        specFirstAlpActionObj[1].gameObject.SetActive(true);
        specFirstAlpActionObj[1].sprite = GameManager.instance.alphabetSprite[(int)end2[0] - 65];
        //for end 3 need word with 3 alp
        if (letterCombine.Length > 2)
        {
            specFirstAlpActionObj[2].gameObject.SetActive(true);
            specFirstAlpActionObj[2].sprite = GameManager.instance.alphabetSprite[(int)end3[0] - 65];
        }
    }
    //reset first end ui
    private void ResetFirstEndUi()
    {
        for (int i = 0; i < specFirstAlpInfoObj.Length; i++)
        {
            specFirstAlpInfoObj[i].gameObject.SetActive(false);
            specFirstAlpActionObj[i].gameObject.SetActive(false);
        }
    }
    //show first end noti
    private IEnumerator ShowFirstEndNoti()
    {
        PlaySoundFail();
        firstEndNoti.SetActive(true);
        yield return new WaitForSeconds(1f);
        firstEndNoti.SetActive(false);
    }
    //------------------------------------------

    //check build button availability - set clickable or not
    private void SetBuildBtnsInteracteable()
    {
        //ladder
        if (GameManager.instance.inGame.isLadder)
        {
            //Challenge MODE ()
            //exclude for ladder
            if (GameManager.instance.inGame.isChallengeStage)
            {
                SetBuildBtninteractable(0, false);
                completeLadderImg.SetActive(false);
            }
            //if ladders complete
            else if (!GameManager.instance.inGame.ladders.isCompleted)
            {
                //check if word point more than point needed
                SetBuildBtninteractable(0, wordPoint >= GameManager.instance.inGame.ladderPt);
                completeLadderImg.SetActive(false);
            }
            else
            {
                //set ladder complete notification
                SetBuildBtninteractable(0, false);
                completeLadderImg.SetActive(true);
                PlaySoundComplete();
            }
        }
        //ground
        if (GameManager.instance.inGame.isGround)
        {
            //if ladders complete
            if (!GameManager.instance.inGame.ladders.isCompleted)
                //check if word point more than point needed
                SetBuildBtninteractable(1, wordPoint >= GameManager.instance.inGame.groundPt);
            else
                SetBuildBtninteractable(1, false);
        }
        //fence
        if (GameManager.instance.inGame.isFence)
        {
            //off the complete ladder img
            completeLadderImg.SetActive(false);
            if (!isObjBuildBtnClickable)
                SetBuildBtninteractable(2, false);
            //limit the obj to build based on th total of obj build existed
            else if (GameManager.instance.inGame.builderInRun.objBuildInGame < GameManager.instance.inGame.builderInRun.objBuildInGameLimit)
                SetBuildBtninteractable(2, wordPoint >= GameManager.instance.inGame.fencePt);
            else
                SetBuildBtninteractable(2, false);
        }
        //slime
        if (GameManager.instance.inGame.isSlime)
        {
            if (!isObjBuildBtnClickable)
                SetBuildBtninteractable(3, false);
            else if (GameManager.instance.inGame.builderInRun.objBuildInGame < GameManager.instance.inGame.builderInRun.objBuildInGameLimit)
                SetBuildBtninteractable(3, wordPoint >= GameManager.instance.inGame.slimePt);
            else
                SetBuildBtninteractable(3, false);
        }
    }
    private void SetBuildBtninteractable(int buildBtnNo, bool isActive)
    {
        canvasGroupFunc.ModifyCG(builBtnCG[buildBtnNo], 1, isActive, true);
    }
    private void SetRunBuildBtn(bool isEnable)
    {
        isObjBuildBtnClickable = isEnable;
        if (!isEnable)
            objBuildCooldownNum = 0;
    }

    public void Death(bool isReal)
    {
        if (!isReal)
        {
            bookNumText.text = GameManager.instance.playerData.bookNum.ToString();
            gameMenuUiAnim.SetTrigger("death");
            //clock countdown anim
            TimerClockCountdown(deathClockDuration);
        }
        else
        {
            // switch (GameManager.instance.playerData.deathScenario)
            // {
            //     case "alphabet":
            //         deathImg.sprite = GameManager.instance.inGameUi.deathSprite[0];
            //         break;
            //     default:
            //         deathImg.sprite = GameManager.instance.inGameUi.deathSprite[1];
            //         break;
            // }
            gameMenuUiAnim.SetTrigger("realDeath");
        }
    }

    //Challenge MODE 
    public void DeathChallenge(bool isRun)
    {
        //convert float seconds to timespan
        float timeNum = GameManager.instance.inGameUi.timeLeft;
        timeChText.text = "Time: " + GameManager.instance.ChangeFloatToTime(timeNum);
        // TimeSpan time = TimeSpan.FromSeconds(timeNum);
        // if (timeNum < 3600)
        //     //set time format - min:sec
        //     timeChText.text = "Time : " + time.ToString("mm':'ss");
        // else
        //     //set time format - hour:min:sec
        //     timeChText.text = "Time : " + time.ToString("hh':'mm':'ss");
        if (!isRun)
            scoreText.text = "Build: " + GameManager.instance.inGame.groundManager.groundCount;
        gameMenuUiAnim.SetTrigger("dieChallenge");
        GameManager.instance.UpdateChallengeScore();
    }

    public void Revive()
    {
        if (!isRunGame)
            gameMenuUiAnim.SetTrigger("info");
        else
            gameMenuUiAnim.SetTrigger("infoHp");
    }

    public void Win()
    {
        //confetti win 
        GameManager.instance.inGame.confettiWin.SetActive(true);
        //TUTORIAL MODE ()
        if (GameManager.instance.isTutorialMode)
        {
            gameMenuUiAnim.SetTrigger("winTutorial");
            //GameManager.instance.tutorialUI.TutorialEvent(12);
            GameManager.instance.tutorialUI.TutorialEnd();
            GameManager.instance.isTutorialMode = false;
            GameManager.instance.tutorialUI.isTutorialEnd = false;
            GameManager.instance.isHasTutorial = true;
            GameManager.instance.player.ManagePlayerLevel();
            GameManager.instance.SaveState(false, true);
        }
        //Challenge MODE()
        else if (GameManager.instance.inGame.isChallengeStage)
        {
            gameMenuUiAnim.SetTrigger("challenge");
        }
        ////TODO () - if END - show finish window
        else if (GameManager.instance.inGame.nextStageName == "END")
            gameMenuUiAnim.SetTrigger("end");
        else
            gameMenuUiAnim.SetTrigger("winTemp");
        //Debug.Log("win window");
        GameManager.instance.player.PlaySoundWin();
        //change music background to win theme
        gameSettings.ChangeMusicBackground(true, 1);
        FinishGame(false);
    }

    //timer clock countdown
    private void TimerClockCountdown(int Second)
    {
        remainingDurationClock = Second;
        //StartCoroutine(UpdateTimerClock());
        isStartdeathClock = true;
    }
    private void UpdateTimerDeathClock()
    {
        if (remainingDurationClock >= 0)
        {
            //uiText.text = $"{remainingDuration / 60:00}:{remainingDuration % 60:00}";
            uiFill.fillAmount = Mathf.InverseLerp(0, deathClockDuration, remainingDurationClock);
            remainingDurationClock--;
            //yield return new WaitForSeconds(1f);
        }
        else
            OnEnd();
    }
    private void OnEnd()
    {
        if (isHasPlayCancel)
            return;
        isHasPlayCancel = true;
        //reset death clock
        isStartdeathClock = false;
        //End Time , if want Do something
        //Debug.Log("End");
        //go to main menu
        // FinishGame(true);
        // gameMenuUiAnim.SetTrigger("hide");
        // GameManager.instance.mainMenuUI.PlaySoundNavigate();
        //open real die window
        Death(true);
        GameManager.instance.mainMenuUI.PlaySoundCancel();
    }

    //get coin ads
    //USED () - coinbtn in winTempWindow
    public void CoinAdsButton()
    {
        GameManager.instance.adsMediate.ShowRewarded("coin");
    }

    //get book ads
    //USED () - bookbtn in winTempWindow
    public void BookAdsButton()
    {
        GameManager.instance.adsMediate.ShowRewarded("book");
    }

    //USED () - in ladder btn
    public void BuildLadder()
    {
        PlaySoundBuild();
        GameManager.instance.inGame.BuildLadder();
        //pay with word pt
        wordPoint -= GameManager.instance.inGame.ladderPt;
        //set word point event and builder button interactable
        SetWordPointEvent();
        //TUTORIAL MODE ()
        if (!GameManager.instance.isTutorialMode)
            return;
        if (GameManager.instance.tutorial.TutorialPhaseNo == 10)
        {
            GameManager.instance.tutorialUI.TutorialEvent(10);
        }
    }
    //USED () - in ground btn
    public void BuildGround()
    {
        PlaySoundBuild();
        GameManager.instance.inGame.BuildGround();
        wordPoint -= GameManager.instance.inGame.groundPt;
        SetWordPointEvent();
    }
    //USED () - in fence btn
    public void BuildFence()
    {
        PlaySoundBuild();
        GameManager.instance.inGame.BuildFence();
        wordPoint -= GameManager.instance.inGame.fencePt;
        SetRunBuildBtn(false);
        SetWordPointEvent();
        CloseActionMenu();
        objBuildCooldownNum = 0;
    }
    //USED () - in slime btn
    public void BuildSlime()
    {
        PlaySoundBuild();
        GameManager.instance.inGame.BuildSlime();
        wordPoint -= GameManager.instance.inGame.slimePt;
        SetRunBuildBtn(false);
        SetWordPointEvent();
        CloseActionMenu();
        objBuildCooldownNum = 0;
    }

    //USED () - in setting btn
    public void SettingBtn()
    {
        UpdateSoundSetting(gameSettings.musicVolume, gameSettings.soundVolume, gameSettings.isMusicOn, gameSettings.isSoundOn);
    }

    //USED () - in home btn, home btn in winWindow
    public void BackToHome()
    {
        ResetAlphabetWordBtnClick();
        GameManager.instance.swipeUpDownAction.ChangeIsActionInvalid(false);
        gameSettings.UpdateMenuVolumeSetting();
        //GameManager.instance.PauseGame(false);
        GameManager.instance.BackToHome();
        //Debug.Log("game menu ui hide");
    }

    //USED () - dieWindow, giveUpBtn, homeBtn
    public void FinishGame(bool isBackToHome)
    {
        isHasPlayCancel = false;
        //reset death clock
        isStartdeathClock = false;
        ResetAlphabetWordBtnClick();
        //GameManager.instance.swipeUpDownAction.ChangeIsActionInvalid(false);
        //GameManager.instance.gameSettings.UpdateMenuVolumeSetting();
        //GameManager.instance.PauseGame(false);
        GameManager.instance.FinishGame(isBackToHome);
    }

    //USED () - continuebookbtn, continuesadsbtn
    public void ContinueAfterDeathBtn(bool isAds)
    {
        isStartdeathClock = false;
        GameManager.instance.ContinueAfterDeath(isAds);
    }

    //USED () - continuebtn in winWindow
    public void ContinueNextStage()
    {
        GameManager.instance.ContinueNextStage();
    }

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
        gameSettings.TurnOnSoundVolume(isWantOn);
    }

    //USED IN () - musicSlide
    public void ChangeMusicVolume()
    {
        gameSettings.ChangeMusicVolumeSystem(musicSlider.value);
    }
    //USED IN () - soundSlide
    public void ChangeSoundVolume()
    {
        gameSettings.ChangeSoundVolumeSystem(soundSlider.value);
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

    //USED () - in home btn, giveup btn, continue btn in win and die window
    //show interstitial ads
    public void ShowInterstitial()
    {
        //TODO () - decide want to use or not
        GameManager.instance.ShowInterstitial();
    }

    //DELETE ()
    //set challenge mode
    public void SetChallengeMode()
    {
        GameManager.instance.inGame.ChangeDifficultyInChallengeMode();
    }

    //set status ui
    public void SetStatusUi(bool isActive, int statusNo)
    {
        if (isActive)
        {
            statusInfoImg.gameObject.SetActive(true);
            statusInfoImg.sprite = statusSprite[statusNo];
            statusActionImg.gameObject.SetActive(true);
            statusActionImg.sprite = statusSprite[statusNo];
        }
        else
        {
            statusInfoImg.gameObject.SetActive(false);
            statusActionImg.gameObject.SetActive(false);
        }
    }

    //set shield earn ui
    public void SetShieldBtn(bool isActive, int num)
    {
        if (isActive)
        {
            shieldBtn.gameObject.SetActive(true);
            shieldText.text = num.ToString();
        }
        else
        {
            shieldBtn.gameObject.SetActive(false);
        }
    }

    //word container anim
    private void WordConAnimStatus(int num)
    {
        //TUTORIAL MODE ()
        if (GameManager.instance.isTutorialMode)
        {
            if (num == 1)
                wordTutorialContainerAnim.SetTrigger("success");
            else if (num == 2)
                wordTutorialContainerAnim.SetTrigger("fail");
        }
        else
        {
            if (num == 1)
                wordContainerAnim.SetTrigger("success");
            else if (num == 2)
                wordContainerAnim.SetTrigger("fail");
        }
    }

    //play sound -------------------------------------------
    public void PlaySoundWord()
    {
        if (gameMenuUiAudioSource.isPlaying)
            return;
        gameMenuUiAudioSource.PlayOneShot(gameMenuUiAudioClip[0]);
    }
    public void PlaySoundBuild()
    {
        // if (gameMenuUiAudioSource.isPlaying)
        //     return;
        gameMenuUiAudioSource.PlayOneShot(gameMenuUiAudioClip[1]);
    }
    public void PlaySoundComplete()
    {
        gameMenuUiAudioSource.PlayOneShot(gameMenuUiAudioClip[2]);
    }
    public void PlaySoundFail()
    {
        gameMenuUiAudioSource.PlayOneShot(gameMenuUiAudioClip[3]);
    }
    //----------------------------------------------------

}
