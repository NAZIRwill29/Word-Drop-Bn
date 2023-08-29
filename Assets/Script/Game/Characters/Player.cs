using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerData playerData;
    [SerializeField]
    private Collider2D playerColl, hitBoxColl, lifeLine1Coll, lifeLine2Coll, lifeLine3Coll, lifeLine4Coll;
    public Rigidbody2D playerRB;
    //sound
    //  0                 1              2         3       4       5         6         7
    //levelUp    getChar/coin/book    getDamage   immune   win   revive    death     climb
    [SerializeField] private AudioClip[] playerAudioClip;
    public AudioSource playerAudioSource;
    public SwipeRigthLeftMove swipeRigthLeftMove;
    [SerializeField] private GameMenuUi gameMenuUi;
    public Animator playerAnim, hitEffectAnim;
    public SpriteRenderer playerSr;
    public List<char> alphabetsStore;
    public float objHeight;
    //double earn = receive char double
    public bool isSquare, isDoubleEarn, isStartDoubleEarn;
    private bool isClimb, isRunFinish;
    //for push forward by monster
    //private bool isPush;
    //private int pushNum = 50;
    // Start is called before the first frame update
    void Start()
    {
        //ignore collision of 2 collider
        Physics2D.IgnoreCollision(playerColl, hitBoxColl, true);
        Physics2D.IgnoreCollision(playerColl, lifeLine1Coll, true);
        Physics2D.IgnoreCollision(playerColl, lifeLine2Coll, true);
        Physics2D.IgnoreCollision(playerColl, lifeLine3Coll, true);
        Physics2D.IgnoreCollision(playerColl, lifeLine4Coll, true);
        Physics2D.IgnoreCollision(hitBoxColl, lifeLine1Coll, true);
        Physics2D.IgnoreCollision(hitBoxColl, lifeLine2Coll, true);
        Physics2D.IgnoreCollision(hitBoxColl, lifeLine3Coll, true);
        Physics2D.IgnoreCollision(hitBoxColl, lifeLine4Coll, true);
        playerData.originPos = transform.position;
        objHeight = GetComponent<SpriteRenderer>().bounds.size.y;
    }

    void Update()
    {
        //make climb event
        if (playerData.climbNo > 0)
        {
            ClimbEvent();
        }
        if (playerData.winMoveNo > 0)
        {
            WinMoveEvent();
        }
        if (!isClimb && !playerData.isHasWin)
        {
            if (isSquare)
                playerAnim.SetBool("run", true);
        }
        else
        {
            if (isSquare)
                playerAnim.SetBool("run", false);
        }
    }

    //50 frame per sec
    void FixedUpdate()
    {
        if (!GameManager.instance.isStartGame || GameManager.instance.isPauseGame)
            return;
        //shield
        if (playerData.immuneDamageCount < playerData.immuneDamageDuration)
        {
            playerData.immuneDamageCount++;
            playerAnim.SetBool("shield", true);
        }
        else
        {
            playerData.isImmuneDamage = false;
            playerAnim.SetBool("shield", false);
            //Debug.Log("stop immuneDamage - " + playerData.isImmuneDamage + playerData.immuneDamageCount);
        }
        //double earn
        if (playerData.doubleEarnCount < playerData.doubleEarnDuration)
        {
            playerData.doubleEarnCount++;
        }
        else
        {
            //prevent from repeat many time
            if (!isStartDoubleEarn)
                return;
            SetDoubleEarn(false);
            isStartDoubleEarn = false;
        }
        //if (isPush)
        //PushByMonster();
    }

    //move player
    public void MovePlayer(float posX)
    {
        //make player face the direction they go
        if (posX > transform.position.x)
            playerSr.flipX = false;
        else
            playerSr.flipX = true;
        transform.position = new Vector3(posX, transform.position.y, transform.position.z);
    }
    //player move right
    public void MoveRight()
    {
        playerSr.flipX = false;
        transform.position = new Vector3(transform.position.x + 0.5f, transform.position.y, transform.position.z);
    }
    //player move left
    public void MoveLeft()
    {
        playerSr.flipX = true;
        transform.position = new Vector3(transform.position.x - 0.5f, transform.position.y, transform.position.z);
    }
    public void StartGame(int mode)
    {
        //Debug.Log("player start game");
        GameMode(mode);
        //store player mode in current stage
        playerData.playerMode = mode;
        ChangeImmuneDamage(true);
        ManagePlayerLevel();
        gameMenuUi.SetHpUI(true);
        playerData.isHasWin = false;
    }
    public void FinishGame()
    {
        ChangeDieNum(0);
        ChangeIsHasDie(false);
        ChangeImmune(false);
    }
    public void PauseGame(bool isPause)
    {
        ChangeImmune(isPause);
    }
    //revive
    public void Revive()
    {
        ChangeIsHasDie(false);
        ChangeImmuneDamage(true);
        ReturnGameModeAfterDeath();
        PlaySoundRevive();
        playerData.hpTemp = playerData.hp;
        gameMenuUi.SetHpUI(true);
        switch (playerData.deathScenario)
        {
            case "alphabet":
                //Debug.Log("Revive alphabet");
                ReviveAlphabet();
                //TODO () - Revive animation
                break;
            case "drowning":
                //Debug.Log("Revive drowning");
                ReviveDrowning();
                //TODO () - Revive animation
                break;
            case "monster":
                //Debug.Log("Revive monster");
                ReviveMonster();
                //TODO () - Revive animation
                break;
            default:
                break;
        }
    }
    private void ReviveAlphabet()
    {

    }
    private void ReviveDrowning()
    {
        //move water down
        GameManager.instance.inGame.water.AfterRevive();
    }
    private void ReviveMonster()
    {
        //move monster down
        GameManager.instance.inGame.monster.AfterRevive();
    }

    //when hitted by obj or damaged ground
    public void ReceiveDamage(Damage dmg)
    {
        if (playerData.isImmune)
            return;
        if (playerData.isImmuneDamage)
        {
            PlaySoundImmune();
            return;
        }
        if (dmg.objType == "Spike")
            GameManager.instance.inGame.groundManager.PlaySoundAttack1();
        else
            PlaySoundDamage();
        //make damage - delete char in store
        //Debug.Log("damage " + dmg.damageAmount);
        int numDeleteChar = dmg.damageAmount;
        //prevent from over the alphabetStore count
        if (numDeleteChar > alphabetsStore.Count)
            numDeleteChar = alphabetsStore.Count;

        if (alphabetsStore.Count > 0)
        {
            //delete number of alphabet in store base on damage amount
            for (int i = 0; i < numDeleteChar; i++)
            {
                //Debug.Log("remove char");
                RemoveChar(Random.Range(0, alphabetsStore.Count));
            }
            //shake camera
            GameManager.instance.cameraManager.CamShake();
            //hit effect anim
            hitEffectAnim.SetTrigger("hit");
        }
        else
        {
            // if (hp > 0)
            // {
            //     hp--;
            // }
            // else
            // {
            Death("alphabet");
            //}
        }
    }

    //call when attacked by monster
    public void ReceiveDamageHp(Damage dmg)
    {
        if (playerData.isImmune)
            return;
        if (playerData.isImmuneDamage)
        {
            PlaySoundImmune();
            return;
        }
        if (playerData.hpTemp > 1)
        {
            PlaySoundDamage();
            playerData.hpTemp--;
            //set push forward by monster
            //StartPushByMonster();
            //shake camera
            GameManager.instance.cameraManager.CamShake();
            //hit effect anim
            hitEffectAnim.SetTrigger("hit");
        }
        else
        {
            Death("monster");
        }
        gameMenuUi.SetHpUI(false);
    }

    //char-----------------------------------
    public void ReceiveChar(char abc)
    {
        if (playerData.isImmune)
            return;
        PlaySoundChar();
        AddChar(abc);
        //TUTORIAL MODE ()
        if (!GameManager.instance.isTutorialMode)
            return;
        if (GameManager.instance.tutorial.TutorialPhaseNo == 2)
            GameManager.instance.tutorial.Tutorial2Trigger(alphabetsStore.Count);
    }
    //for reverse char
    public void ReceiveReverseChar(char abc)
    {
        if (playerData.isImmune)
            return;
        RemoveAddChar(abc, 1);
    }
    //for blood char
    public void ReceiveBloodChar(char abc)
    {
        if (playerData.isImmune)
            return;
        isStartDoubleEarn = true;
        RemoveAddChar(abc, 5);
        SetDoubleEarn(true);
    }
    //for shield char
    public void ReceiveShieldChar(char abc)
    {
        if (playerData.isImmune)
            return;
        RemoveAddChar(abc, 5);
        GameManager.instance.shieldBought++;
        SetShieldNum();
    }
    //for skull char
    public void ReceiveFakeChar()
    {
        if (playerData.isImmune)
            return;
        PlaySoundDamage();
        //if have shield remove charx5
        if (playerData.isImmuneDamage)
        {
            //check if player alphabet is low then wanted remove
            if (alphabetsStore.Count < 5)
                Death("alphabet");
            else
            {
                for (int i = 0; i < 5; i++)
                {
                    alphabetsStore.RemoveAt(0);
                }
            }
        }
        //if no shield remove charx10
        else
        {
            //check if player alphabet is low then wanted remove
            if (alphabetsStore.Count < 10)
                Death("alphabet");
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    alphabetsStore.RemoveAt(0);
                }
            }
        }
        SetAlphabetStore();
    }
    //remove then add char
    public void RemoveAddChar(char abc, int removeNum)
    {
        if (playerData.isImmune)
            return;
        PlaySoundChar();
        //remove first char x4
        if (!playerData.isImmuneDamage)
        {
            //check if player alphabet is low then wanter remove
            if (alphabetsStore.Count < removeNum)
                Death("alphabet");
            else
            {
                for (int i = 0; i < removeNum; i++)
                {
                    alphabetsStore.RemoveAt(0);
                }
            }
        }
        AddChar(abc);
    }
    //add char to storage
    private void AddChar(char abc)
    {
        //check if double earn
        if (isDoubleEarn)
        {
            //double earn
            for (int i = 0; i < 2; i++)
            {
                alphabetsStore.Add(abc);
                LimitAlphabetNum();
            }
        }
        else
            //single earn
            alphabetsStore.Add(abc);
        SetAlphabetStore();
    }
    //set alphabet visual
    private void SetAlphabetStore()
    {
        LimitAlphabetNum();
        gameMenuUi.SetCharPlayer();
    }
    //limit alphabet - avoid out of bound
    private void LimitAlphabetNum()
    {
        //if more than char max ->  remove first char
        if (alphabetsStore.Count > playerData.charMaxNo)
            alphabetsStore.RemoveAt(0);
    }
    //set double earn
    public void SetDoubleEarn(bool isTrue)
    {
        isDoubleEarn = isTrue;
        if (isTrue)
            playerData.doubleEarnCount = 0;
        //set ui
        GameManager.instance.gameMenuUi.SetStatusUi(isTrue, 0);
    }
    //set shield status num
    public void SetShieldNum()
    {
        int num = GameManager.instance.shieldBought;
        if (num > 0)
            //set active            
            gameMenuUi.SetShieldBtn(true, num);
        else
            gameMenuUi.SetShieldBtn(false, num);
    }
    //----------------------------------------

    public void ReceiveBook(int num)
    {
        AddBookNum(num);
    }
    public void ReceiveCoin(int coin)
    {
        GameManager.instance.AddCoin(coin);
        gameMenuUi.SetCoinEvent();
        PlaySoundChar();
    }

    public void RemoveChar(int charIndex)
    {
        alphabetsStore.RemoveAt(charIndex);
        gameMenuUi.RemoveCharUi(charIndex);
    }
    // public void RemoveChar(int charIndex, char alphabet)
    // {
    //     alphabetsStore.Remove(alphabet);
    //     gameMenuUi.RemoveCharUi(charIndex, alphabet);
    // }

    public void RemoveAllChar()
    {
        alphabetsStore.RemoveRange(0, alphabetsStore.Count);
        gameMenuUi.RemoveAllCharUI();
    }

    //TODO() - 
    public void Death(string scenario)
    {
        if (playerData.isHasDie)
            return;
        PlaySoundDeath();
        playerData.deathScenario = scenario;
        switch (scenario)
        {
            case "alphabet":
                //Debug.Log("DEATH alphabet");
                if (!GameManager.instance.inGameUi.isRun)
                    LifeLine(0);
                DieEvent();
                //TODO () - die animation
                break;
            case "drowning":
                //Debug.Log("DEATH drowning");
                LifeLine(0);
                DieEvent();
                //TODO () - die animation
                break;
            case "monster":
                //Debug.Log("DEATH monster");
                DieEvent();
                //TODO () - die animation
                break;
            default:
                break;
        }
    }
    private void DieEvent()
    {
        ChangeIsHasDie(true);
        //freeze all - pause game - off rigidbody player
        GameMode(2);
        //Challenge MODE ()
        if (GameManager.instance.inGame.isChallengeStage)
        {
            GameManager.instance.DeathChallenge();
        }
        else
        {
            //make chance after die once only
            playerData.dieNum++;
            if (playerData.dieNum < 2)
                GameManager.instance.Death(false);
            else
                GameManager.instance.Death(true);
        }
    }

    //Lifeline effect - call when water touch lifeline
    public void LifeLine(int num)
    {
        // if (playerData.isImmune)
        //     return;
        switch (num)
        {
            case 0:
                //call when free from water
                //back to normal
                playerData.lifeLineTrigger = 0;
                break;
            case 1:
                if (playerData.lifeLineTrigger < 1)
                    playerData.lifeLineTrigger = 1;
                break;
            case 2:
                if (playerData.lifeLineTrigger < 2)
                    playerData.lifeLineTrigger = 2;
                break;
            case 3:
                if (playerData.lifeLineTrigger < 3)
                    playerData.lifeLineTrigger = 3;
                break;
            case 4:
                if (playerData.lifeLineTrigger < 4)
                    Death("drowning");
                break;
            case -1:
                if (playerData.lifeLineTrigger > 0)
                {
                    playerData.lifeLineTrigger--;
                    playerData.lifeLineBuildTrigger = -2;
                }
                break;
            case -2:
                if (playerData.lifeLineTrigger > 1)
                {
                    playerData.lifeLineTrigger -= 2;
                    playerData.lifeLineBuildTrigger = -1;
                }
                else if (playerData.lifeLineTrigger > 0)
                {
                    playerData.lifeLineTrigger--;
                    playerData.lifeLineBuildTrigger = -1;
                }
                break;
            default:
                break;
        }
        SetSpeed(playerData.lifeLineTrigger);
    }
    //set player speed based on the water effect
    private void SetSpeed(int num)
    {
        switch (num)
        {
            case 0:
                ChangeSpeed(0.01f);
                break;
            case 1:
                ChangeSpeed(0.008f);
                break;
            case 2:
                ChangeSpeed(0.005f);
                break;
            case 3:
                ChangeSpeed(0.002f);
                break;
            case 4:
                //Debug.Log("death drowned");
                ChangeSpeed(0.01f);
                Death("drowning");
                break;
            default:
                break;
        }
    }

    //level up
    public void LevelUp(bool isShowOnly)
    {
        int levelPlayerTemp = playerData.levelPlayer + 1;
        switch (levelPlayerTemp)
        {
            case 2:
                LevelUpEvent(10, 1, isShowOnly);
                break;
            case 3:
                LevelUpEvent(50, 4, isShowOnly);
                break;
            case 4:
                LevelUpEvent(90, 8, isShowOnly);
                break;
            case 5:
                LevelUpEvent(140, 13, isShowOnly);
                break;
            case 6:
                LevelUpEvent(200, 19, isShowOnly);
                break;
            case 7:
                LevelUpEvent(270, 26, isShowOnly);
                break;
            case 8:
                LevelUpEvent(340, 33, isShowOnly);
                break;
            default:
                break;
        }
    }
    private void LevelUpEvent(int coinNeed, int bookNeed, bool isShowOnly)
    {
        //check if have enough coin and book
        if (GameManager.instance.coin >= coinNeed && playerData.bookNum >= bookNeed)
        {
            GameManager.instance.mainMenuUI.SetPlayerUpgradeNotice(true);
        }
        else
        {
            GameManager.instance.mainMenuUI.SetPlayerUpgradeNotice(false);
            return;
        }
        //only exec if not show only
        if (!isShowOnly)
        {
            PlaySoundLevelUp();
            GameManager.instance.AddCoin(-coinNeed);
            playerData.bookNum -= bookNeed;
            playerData.levelPlayer++;
            GameManager.instance.SaveState(false, false);
        }
        ManagePlayerLevel();
    }

    public void SetPlayerLevel(int num)
    {
        //prevent from level 0
        if (num <= 0)
            num = 1;
        playerData.levelPlayer = num;
        //Debug.Log("in SetPlayerLevel : level = " + playerData.levelPlayer);
        ManagePlayerLevel();
    }
    //player level
    public void ManagePlayerLevel()
    {
        switch (playerData.levelPlayer)
        {
            case 1:
                playerData.charMaxNo = 10;
                playerData.hpTemp = 2;
                playerData.hp = 2;
                playerData.immuneDamageDuration = 150;
                playerData.addWordPt = 0;
                gameMenuUi.SetPlayerLevelUI(0);
                break;
            case 2:
                playerData.charMaxNo = 12;
                playerData.hpTemp = 2;
                playerData.hp = 2;
                playerData.immuneDamageDuration = 150;
                playerData.addWordPt = 0;
                gameMenuUi.SetPlayerLevelUI(1);
                break;
            case 3:
                playerData.charMaxNo = 14;
                playerData.hpTemp = 3;
                playerData.hp = 3;
                playerData.immuneDamageDuration = 200;
                playerData.addWordPt = 1;
                gameMenuUi.SetPlayerLevelUI(1);
                break;
            case 4:
                playerData.charMaxNo = 16;
                playerData.hpTemp = 3;
                playerData.hp = 3;
                playerData.immuneDamageDuration = 200;
                playerData.addWordPt = 1;
                gameMenuUi.SetPlayerLevelUI(1);
                break;
            case 5:
                playerData.charMaxNo = 18;
                playerData.hpTemp = 4;
                playerData.hp = 4;
                playerData.immuneDamageDuration = 250;
                playerData.addWordPt = 2;
                gameMenuUi.SetPlayerLevelUI(1);
                break;
            case 6:
                playerData.charMaxNo = 20;
                playerData.hpTemp = 4;
                playerData.hp = 4;
                playerData.immuneDamageDuration = 250;
                playerData.addWordPt = 3;
                gameMenuUi.SetPlayerLevelUI(1);
                break;
            case 7:
                playerData.charMaxNo = 20;
                playerData.hpTemp = 5;
                playerData.hp = 5;
                playerData.immuneDamageDuration = 300;
                playerData.addWordPt = 4;
                gameMenuUi.SetPlayerLevelUI(1);
                break;
            case 8:
                playerData.charMaxNo = 20;
                playerData.hpTemp = 5;
                playerData.hp = 5;
                playerData.immuneDamageDuration = 350;
                playerData.addWordPt = 5;
                gameMenuUi.SetPlayerLevelUI(1);
                break;
            default:
                break;
        }
        //Debug.Log("in ManagePlayerLevel : hp = " + playerData.hp);
    }

    //climb ladder - how many ladder
    public void Climb(int num)
    {
        //Debug.Log("climb");
        //TUTORIAL MODE ()
        if (GameManager.instance.isTutorialMode)
            playerData.climbNo = (7 + 1) * 2;
        else
            playerData.climbNo = (num + 2) * 2;
        LifeLine(0);
        playerAnim.SetBool("reset", false);
        isClimb = true;
        playerAnim.SetTrigger("climb");
        PlaySoundClimb();
        playerData.isImmune = true;
        GameManager.instance.inGame.spawn.StopSpawn(true);
        GameManager.instance.inGame.spawn.DeleteAllObj();
    }
    private void ClimbEvent()
    {
        transform.position = new Vector3(GameManager.instance.inGame.laddersObj.transform.position.x, transform.position.y, transform.position.z);
        transform.position += new Vector3(0, 0.525f, 0);
        playerData.climbNo--;
        if (playerData.climbNo == 0)
            Win(true);
    }

    //win
    public void Win(bool isStaticGameMode)
    {
        //Debug.Log("Win");
        if (playerData.isHasWin)
            return;
        GameManager.instance.inGame.spawn.StopSpawn(true);
        GameManager.instance.UpdateStageProgress();
        if (isStaticGameMode)
        {
            //for static game
            LifeLine(0);
            playerData.isHasWin = true;
            isClimb = false;
            //Debug.Log("win");
            //freeze all - pause game - off rigidbody player
            GameMode(2);
            StartCoroutine(WinStatic());
        }
        else
        {
            //for run game
            playerData.isHasWin = true;
            //add push forward
            //Debug.Log("win");
            //freeze all - pause game - off rigidbody player
            GameMode(2);
            StartCoroutine(WinRun());
        }
    }
    //TODO () - win static
    private IEnumerator WinStatic()
    {
        yield return new WaitForSeconds(0.3f);
        gameMenuUi.Win();
    }
    //TODO () - win run
    private IEnumerator WinRun()
    {
        playerData.winMoveNo = 50;
        GameManager.instance.cameraManager.isStopFollow = true;
        gameMenuUi.Win();
        yield return new WaitUntil(() => isRunFinish);
        isRunFinish = false;
        GameManager.instance.cameraManager.isStopFollow = false;
        GameManager.instance.ShowRateReviewPopUp();
    }

    private void WinMoveEvent()
    {
        transform.position += new Vector3(0, 0.12f, 0);
        playerData.winMoveNo--;
        if (playerData.winMoveNo < 2)
        {
            isRunFinish = true;
            playerData.winMoveNo = 0;
        }
    }

    //player game mode - call in gamemanager
    //      0       1       2
    //    static   run     pause
    public void GameMode(int mode)
    {
        //Debug.Log("game mode = " + mode);
        switch (mode)
        {
            case 0:
                //drowned mode
                playerRB.bodyType = RigidbodyType2D.Dynamic;
                transform.position = playerData.originPos;
                playerData.isImmune = false;
                break;
            case 1:
                //run mode
                //Debug.Log("game mode run");
                playerRB.bodyType = RigidbodyType2D.Static;
                transform.position = new Vector3(playerData.originPos.x, -1.538f, playerData.originPos.z);
                playerData.isImmune = false;
                break;
            default:
                //pause
                playerRB.bodyType = RigidbodyType2D.Static;
                playerData.isImmune = true;
                break;
        }
    }
    private void ReturnGameModeAfterDeath()
    {
        switch (playerData.playerMode)
        {
            case 0:
                //drowned mode
                playerRB.bodyType = RigidbodyType2D.Dynamic;
                playerData.isImmune = false;
                break;
            case 1:
                //run mode
                playerRB.bodyType = RigidbodyType2D.Static;
                playerData.isImmune = false;
                break;
            default:
                break;
        }
    }

    //variable
    public void ChangeSpeed(float num)
    {
        playerData.speed = num;
    }
    public void ChangeImmune(bool isTrue)
    {
        playerData.isImmune = isTrue;
    }
    public void AddAlphabetStore(char abc)
    {
        alphabetsStore.Add(abc);
        //if more than char max ->  remove first char
        if (alphabetsStore.Count > playerData.charMaxNo)
            alphabetsStore.RemoveAt(0);
        gameMenuUi.SetCharPlayer();
    }
    public void ChangeImmuneDamage(bool isTrue)
    {
        playerData.isImmuneDamage = isTrue;
        if (isTrue)
        {
            playerData.immuneDamageCount = 0;
            if (!GameManager.instance.inGameUi.isRun)
                //reset player animation
                playerAnim.SetBool("reset", true);
            //playerAnim.SetBool("run", true);
            playerAnim.SetBool("shield", true);
        }

    }
    public void ChangeIsHasDie(bool isTrue)
    {
        playerData.isHasDie = isTrue;
    }
    public void ChangeDieNum(int num)
    {
        playerData.dieNum = num;
    }
    public void SetBookNum(int num)
    {
        playerData.bookNum = num;
    }
    public void AddBookNum(int num)
    {
        playerData.bookNum += num;
    }
    public void ResetBookNum()
    {
        playerData.bookNum = 0;
    }

    //play sound -------------------------------------------
    public void PlaySoundLevelUp()
    {
        // if (playerAudioSource.isPlaying)
        //     return;
        playerAudioSource.PlayOneShot(playerAudioClip[0]);
    }
    public void PlaySoundChar()
    {
        // if (playerAudioSource.isPlaying)
        //     return;
        playerAudioSource.PlayOneShot(playerAudioClip[1]);
    }
    public void PlaySoundDamage()
    {
        // if (playerAudioSource.isPlaying)
        //     return;
        playerAudioSource.PlayOneShot(playerAudioClip[2]);
    }
    public void PlaySoundImmune()
    {
        // if (playerAudioSource.isPlaying)
        //     return;
        playerAudioSource.PlayOneShot(playerAudioClip[3]);
    }
    public void PlaySoundWin()
    {
        // if (playerAudioSource.isPlaying)
        //     return;
        playerAudioSource.PlayOneShot(playerAudioClip[4]);
        //Debug.Log("play sound win");
    }
    public void PlaySoundRevive()
    {
        // if (playerAudioSource.isPlaying)
        //     return;
        playerAudioSource.PlayOneShot(playerAudioClip[5]);
    }
    public void PlaySoundDeath()
    {
        // if (playerAudioSource.isPlaying)
        //     return;
        playerAudioSource.PlayOneShot(playerAudioClip[6]);
    }
    public void PlaySoundClimb()
    {
        // if (playerAudioSource.isPlaying)
        //     return;
        playerAudioSource.PlayOneShot(playerAudioClip[7]);
    }
    //----------------------------------------------------
}
