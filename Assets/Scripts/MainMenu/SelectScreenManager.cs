using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SelectScreenManager : MonoBehaviour
{

    public int numberOfPlayers = 1;
    public List<PlayerInterfaces> plInterfaces = new List<PlayerInterfaces> ();
    public PortraitInfo[] portraitPrefabs;
    public int maxX = 2;
    public int maxY = 3;
    PortraitInfo[,] charGrid;

    public GameObject portraitCanvas;

    bool loadLevel;
    public bool bothPlayersSelected;

    CharacterManager charManager;

    #region Singleton
    public static SelectScreenManager instance;
    public static SelectScreenManager GetInstance()
    { 
         return instance;
    }

    void Awake()
    { 
        instance = this;
    }
    #endregion

    void Start()
    {
        charManager = CharacterManager.GetInstance();
        numberOfPlayers = charManager.numberOfUsers;
        Debug.Log("Portrait Canvas active: " + portraitCanvas.activeInHierarchy);
        charGrid = new PortraitInfo[maxX, maxY];

        int x = 0;
        int y = 0;

        portraitPrefabs = portraitCanvas.GetComponentsInChildren<PortraitInfo>();
        Debug.Log($"Length prefab {portraitPrefabs.Length} ");
        for (int i = 0; i < portraitPrefabs.Length; i++)
        { 
            portraitPrefabs[i].posX = x;
            portraitPrefabs[i].posY = y;
            Debug.Log($"Assigning {portraitPrefabs[i].name} to grid [{x},{y}]");
            charGrid[x, y] = portraitPrefabs[i];

            if (x < maxX - 1)
            {
                x++;
            }
            else
            {
                x = 0;
                y++;
            }
        }
    }

    void Update()
    {
        if (!loadLevel)
        {
            for (int i = 0; i < plInterfaces.Count; i++)
            {
                if (i < numberOfPlayers)
                {
                    if (Input.GetButtonUp("Fire2" + charManager.players[i].inputId))
                    {
                        plInterfaces[i].playerBase.hasCharacter = false;
                    }

                    if (!charManager.players[i].hasCharacter)
                    {
                        plInterfaces[i].playerBase = charManager.players[i];

                        HandleSelectorPosition(plInterfaces[i]);
                        HandleSelectScreenInput(plInterfaces[i], charManager.players[i].inputId);
                        HandleCharacterPreview(plInterfaces[i]);
                    }
                }
                else
                {
                    charManager.players[i].hasCharacter = true;
                }
            }
        }

        if (bothPlayersSelected)
        {
            Debug.Log("loading");
            StartCoroutine("LoadLevel");
            loadLevel = true;
        }
        else 
        {
            if (charManager.players[0].hasCharacter && charManager.players[1].hasCharacter)
            { 
                bothPlayersSelected = true;
            }
        }
    }

    IEnumerator LoadLevel() 
    {
        for (int i = 0; i < charManager.players.Count; i++)
        {
            if (charManager.players[i].playerType == PlayerBase.PlayerType.ai) 
            {
                if (charManager.players[i].playerPrefab == null) 
                {
                    int ranValue = Random.Range(0, portraitPrefabs.Length);

                    charManager.players[i].playerPrefab = charManager.returnCharacterWithID(portraitPrefabs[ranValue].characterId).prefab;

                    Debug.Log(portraitPrefabs[ranValue].characterId);
                }
            }
        }

        yield return new WaitForSeconds(2);
        SceneManager.LoadSceneAsync("level", LoadSceneMode.Single);

    }

    void HandleSelectScreenInput(PlayerInterfaces pl, string playerId) 
    {
        #region Grid Navigation

        float vertical = Input.GetAxis("Vertical" + playerId);

        if (vertical != 0)
        {
            if (!pl.hitInputOnce) 
            {
                if (vertical > 0)
                {
                    pl.activeY = (pl.activeY > 0) ? pl.activeY - 1 : maxY - 1;
                }
                else
                {
                    pl.activeY = (pl.activeY < maxY - 1) ? pl.activeY + 1 : 0;
                    
                }

                pl.hitInputOnce = true;
            }
        }

        float horizontal = Input.GetAxis("Horizontal" + playerId);

        if (horizontal != 0) 
        {
            if (!pl.hitInputOnce) 
            {
                if (horizontal > 0)
                {
                    pl.activeX = (pl.activeX < maxX - 1) ? pl.activeX + 1 : 0;
                }
                else
                {
                    pl.activeX = (pl.activeX > 0) ? pl.activeX - 1 : maxX - 1;
                }

                pl.timerToReset = 0;
                pl.hitInputOnce = true;
            }
        }

        if (vertical == 0 && horizontal == 0)
        { 
            pl.hitInputOnce = false;
        }

        if (pl.hitInputOnce)
        {
            pl.timerToReset = Time.deltaTime;

            if (pl.timerToReset > 0.8f)
            { 
                pl.hitInputOnce = false ;
                pl.timerToReset = 0;
            }
        }

        #endregion

        if (Input.GetButtonUp("Fire1" + playerId))
        { 
            pl.createdCharacter.GetComponentInChildren<Animator>().Play("kick");

            pl.playerBase.playerPrefab = charManager.returnCharacterWithID(pl.activePortrait.characterId).prefab;

            pl.playerBase.hasCharacter = true;
        }

    }

    void HandleSelectorPosition(PlayerInterfaces pl) 
    {
        pl.selector.SetActive(true);
        Debug.Log($"Trying to access charGrid[{pl.activeX}, {pl.activeY}] � Grid Size: [{maxX}, {maxY}]");
        Debug.Log($"[SelectorPosition] activeX: {pl.activeX}, activeY: {pl.activeY}");
        Debug.Log($"[SelectorPosition] charGrid[{pl.activeX},{pl.activeY}] is " + (charGrid[pl.activeX, pl.activeY] == null ? "NULL" : "OK"));
        pl.activePortrait = charGrid[pl.activeX, pl.activeY];

        Vector2 selectorPosition = pl.activePortrait.transform.localPosition;
        selectorPosition = selectorPosition + new Vector2(portraitCanvas.transform.localPosition.x, portraitCanvas.transform.localPosition.y);

        pl.selector.transform.localPosition = selectorPosition;

    }

    void HandleCharacterPreview(PlayerInterfaces pl)
    {
        if (pl.previewPortrait != pl.activePortrait)
        {
            if (pl.createdCharacter != null)
            {
                Destroy(pl.createdCharacter);
            }

            GameObject go = Instantiate(CharacterManager.GetInstance().returnCharacterWithID(pl.activePortrait.characterId).prefab, pl.charVisPos.position, Quaternion.identity) as GameObject;

            pl.createdCharacter = go;
           
            pl.previewPortrait = pl.activePortrait;

            if (!string.Equals(pl.playerBase.playerId, charManager.players[0].playerId))
            { 
                pl.createdCharacter.GetComponent<StateManager>().lookRight = false;
            }
        }
    }
    [System.Serializable]
    public class PlayerInterfaces
    {
        public PortraitInfo activePortrait;
        public PortraitInfo previewPortrait;
        public GameObject selector;
        public Transform charVisPos;
        public GameObject createdCharacter;

        public int activeX;
        public int activeY;

        public bool hitInputOnce;
        public float timerToReset;

        public PlayerBase playerBase;
    }

}
