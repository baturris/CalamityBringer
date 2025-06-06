using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{

    WaitForSeconds oneSec;
    public Transform[] spawnPositions;

    CharacterManager charM;
    LevelUI levelUI;
    CameraManager camM;

    public int maxTurns = 2;
    int currentTurn = 1;

    public bool countdown;
    public int maxTurnTimer = 30;
    int currentTimer;
    float internalTimer;
    AudioManager audioManager;
   

        void Start()
    {
        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();
        charM = CharacterManager.GetInstance();
        levelUI = LevelUI.GetInstance();
        camM = CameraManager.GetInstance();

        oneSec = new WaitForSeconds(1);

        levelUI.AnnouncerTextLine1.gameObject.SetActive(false);
        levelUI.AnnouncerTextLine2.gameObject.SetActive(false);

        StartCoroutine("StartGame");

    }

    void FixedUpdate()
    {
        GameObject audioObj = GameObject.FindGameObjectWithTag("Audio");
        if (charM.players[0].playerState.transform.position.x < charM.players[1].playerState.transform.position.x)
        {
            charM.players[0].playerState.lookRight = true;
            charM.players[1].playerState.lookRight = false;
        }
        else
        {
            charM.players[0].playerState.lookRight = false;
            charM.players[1].playerState.lookRight = true;
        }

    }

    void Update()
    {
        if (countdown)
        {
            HandleTurnTimer();
        }
    }

    void HandleTurnTimer()
    { 
        levelUI.LevelTimer.text = currentTimer.ToString();

        internalTimer += Time.deltaTime;

        if (internalTimer > 1)
        {
            currentTimer--;
            internalTimer = 0;
        }

        if (currentTimer <= 0)
        {
            EndTurnFunction(true);
            countdown = false;
        }
    }

    IEnumerator StartGame()
    {
        yield return CreatePlayers();

        DisableControl();

        yield return InitTurn();
    }

    IEnumerator InitTurn()
    { 
      
        levelUI.AnnouncerTextLine1.gameObject.SetActive(false);
        levelUI.AnnouncerTextLine2.gameObject.SetActive(false);

        currentTimer = maxTurnTimer;
        countdown = false;

        yield return InitPlayers();

        yield return EnableControl();

    }

    IEnumerator CreatePlayers()
    {
        for (int i = 0; i < charM.players.Count; i++)
        {
            GameObject go = Instantiate(charM.players[i].playerPrefab, spawnPositions[i].position, Quaternion.identity) as GameObject;

            charM.players[i].playerState = go.GetComponent<StateManager>();

            charM.players[i].playerState.healthSlider = levelUI.healthSliders[i];

            camM.targets.Add(go.transform);

            charM.players[i].playerId = "Player " + (i + 1);
            go.name = "Player " + (i + 1);
        }

        yield return null;
    }

    IEnumerator InitPlayers()
    {

        for (int i = 0; i < charM.players.Count; i++)
        {
            charM.players[i].playerState.health = 100;
            charM.players[i].playerState.handleAnim.anim.Play("Locomotion");
            charM.players[i].playerState.transform.position = spawnPositions[i].position;
        }

        yield return null;

    }

    IEnumerator EnableControl()
    { 
        levelUI.AnnouncerTextLine1.gameObject.SetActive(true);
        levelUI.AnnouncerTextLine1.text = "Turn" + currentTurn;
        levelUI.AnnouncerTextLine1.color = Color.white;
        yield return oneSec;
        yield return oneSec;

        levelUI.AnnouncerTextLine1.text = "3";
        levelUI.AnnouncerTextLine1.color = Color.green;
        yield return oneSec;
        levelUI.AnnouncerTextLine1.text = "2";
        levelUI.AnnouncerTextLine1.color = Color.yellow;
        yield return oneSec;
        levelUI.AnnouncerTextLine1.text = "1";
        levelUI.AnnouncerTextLine1.color = Color.red;
        yield return oneSec;
        levelUI.AnnouncerTextLine1.text = "FIGHT!";
        levelUI.AnnouncerTextLine1.color = Color.red;

        for (int i = 0; i < charM.players.Count; i++)
        {
            if (charM.players[i].playerType == PlayerBase.PlayerType.user)
            { 
                InputHandler ih = charM.players[i].playerState.gameObject.GetComponent<InputHandler>();
                ih.playerInput = charM.players[i].inputId;
                ih.enabled = true;
            }
        }

        yield return oneSec;
        levelUI.AnnouncerTextLine1.gameObject.SetActive(false);
        countdown = true;
    }

    void DisableControl()
    {
        for (int i = 0; i < charM.players.Count; i++)
        {
            charM.players[i].playerState.ResetStateInputs();

            if (charM.players[i].playerType == PlayerBase.PlayerType.user)
            {
                charM.players[i].playerState.GetComponent<InputHandler>().enabled = false;
            }
        }
    }

    public void EndTurnFunction(bool timeOut = false)
    { 
      
        countdown = false;

        levelUI.LevelTimer.text = maxTurnTimer.ToString();

        if (timeOut)
        {
            levelUI.AnnouncerTextLine1.gameObject.SetActive(true);
            levelUI.AnnouncerTextLine1.text = "Time Out!";
            levelUI.AnnouncerTextLine1.color = Color.cyan;
        }
        else
        {
            levelUI.AnnouncerTextLine1.gameObject.SetActive(true);
            levelUI.AnnouncerTextLine1.text = "K.O!";
            audioManager.PlaySFXWithVolume(audioManager.death, 0.2f);

            levelUI.AnnouncerTextLine1.color = Color.red;
        }

        DisableControl();

        StartCoroutine("EndTurn");
    }

    IEnumerator EndTurn()
    {
       
        yield return oneSec;
        yield return oneSec;
        yield return oneSec;

        PlayerBase vPlayer = FindWinningPlayer();

        if (vPlayer == null)
        {
            levelUI.AnnouncerTextLine1.text = "Draw";
            levelUI.AnnouncerTextLine1.color = Color.blue;
        }
        else
        {
            levelUI.AnnouncerTextLine1.text = vPlayer.playerId + " Wins!";
            levelUI.AnnouncerTextLine1.color = Color.red;
        }

        yield return oneSec;
        yield return oneSec;
        yield return oneSec;

        if (vPlayer != null)
        {
            if (vPlayer.playerState.health == 100)
            {
                levelUI.AnnouncerTextLine2.gameObject.SetActive(true);
                levelUI.AnnouncerTextLine2.text = "Perfect!";
            }
        }

        yield return oneSec;
        yield return oneSec;
        yield return oneSec;

        currentTurn++;

        bool matchOver = isMatchOver();

        if (!matchOver)
        {
            StartCoroutine("InitTurn");
        }
        else
        {
            for (int i = 0; i < charM.players.Count; i++)
            {
                charM.players[i].score = 0;
                charM.players[i].hasCharacter = false;
            }

            SceneManager.LoadSceneAsync("select");
        }
    }

    bool isMatchOver()
    { 
        bool retVal = false;

        for (int i = 0; i < charM.players.Count; i++)
        {
            if (charM.players[i].score >= maxTurns)
            { 
                retVal = true;
                break;
            }
        }

        return retVal;
    }

    PlayerBase FindWinningPlayer()
    {
        PlayerBase retVal = null;

        StateManager targetPlayer = null;

        if (charM.players[0].playerState.health != charM.players[1].playerState.health)
        {

            if (charM.players[0].playerState.health < charM.players[1].playerState.health)
            {
                charM.players[1].score++;
                targetPlayer = charM.players[1].playerState;
                levelUI.AddWinIndicator(1);
            }
            else
            {
                charM.players[0].score++;
                targetPlayer = charM.players[0].playerState;
                levelUI.AddWinIndicator(0);
            }

            retVal = charM.returnPlayerFromStates(targetPlayer);

        }
        return retVal;
        
    }

    public static LevelManager instance;
    public static LevelManager getInstance()
    { 
        return instance;
    }

    void Awake()
    { 
        instance = this;

        
    }
}
