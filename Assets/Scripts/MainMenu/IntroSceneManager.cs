using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class IntroSceneManager : MonoBehaviour
{

    public GameObject startText;
    float timer;
    bool loadingLevel;
    bool init;
    public GameObject titlePanel;
    private Animator animator;
    public int activeElement;
    public GameObject menuObj;
    public ButtonRef[] menuOptions;

    private string selectedSceneName = "select";

    void Start()
    {
        menuObj.SetActive(false);
        animator = titlePanel.GetComponent<Animator>();

    }

    void Update()
    {
        if (!init)
        {
            timer += Time.deltaTime;
            if (timer > 0.6f)
            {
                timer = 0;
                startText.SetActive(!startText.activeInHierarchy);
            }

            if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Jump"))
            {
                init = true;
                startText.SetActive(false);
                menuObj.SetActive(true);
                menuOptions[activeElement].selected = true;
                animator.SetBool("Back2", true);
            }
        }
        else
        {
            if (!loadingLevel)
            {
                menuOptions[activeElement].gameObject.SetActive(true);

                if (Input.GetKeyUp(KeyCode.UpArrow))
                {
                    menuOptions[activeElement].selected = false;

                    if (activeElement > 0)

                    {
                        activeElement--;
                    }
                    else
                    {
                        activeElement = menuOptions.Length - 1;
                    }
                    menuOptions[activeElement].selected = true;
                }

                if (Input.GetKeyUp(KeyCode.DownArrow))
                {
                    menuOptions[activeElement].selected = false ;

                    if (activeElement < menuOptions.Length - 1)
                    {
                        activeElement++;
                    }
                    else
                    { 
                        activeElement = 0 ;
                    }
                    menuOptions[activeElement].selected = true;
                }

                if (Input.GetKeyUp(KeyCode.Space) || Input.GetButtonUp("Jump"))

                {
                    Debug.Log("load");
                    loadingLevel = true;
                    StartCoroutine("LoadLevel");
                    menuOptions[activeElement].transform.localScale *= 1.2f;
                }
            }
        }
    }

    void HandleSelectedOption()
    {
        switch (activeElement)
        {
            case 0:
                selectedSceneName = "Controls"; // Replace with actual scene name
                break;

            case 1:
                CharacterManager.GetInstance().numberOfUsers = 2;

                var charMgr = CharacterManager.GetInstance();

                while (charMgr.players.Count < 2)
                {
                    charMgr.players.Add(new PlayerBase());
                }

                charMgr.players[1].playerType = PlayerBase.PlayerType.user;

                selectedSceneName = "Select"; // Replace with actual scene name
                break;
            case 2:
                selectedSceneName = "Settings"; // Replace with actual scene name
                break;

        }
    }

    IEnumerator LoadLevel()
    {
        HandleSelectedOption();
        yield return new WaitForSeconds(0.6f);
        startText.SetActive(false);
        yield return new WaitForSeconds(1.5F);
        SceneManager.LoadSceneAsync(selectedSceneName, LoadSceneMode.Single);
    }
}
