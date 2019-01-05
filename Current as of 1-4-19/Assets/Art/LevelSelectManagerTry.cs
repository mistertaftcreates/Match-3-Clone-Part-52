using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelectManagerTry : MonoBehaviour {

    public GameObject[] levelSelectPanels;
    public GameObject currentPage;
    public int page;
    public GameData gameData;
    private int currentLevel = 0;

	// Use this for initialization
	void Start () {
        for (int i = 0; i < levelSelectPanels.Length; i ++)
        {
            levelSelectPanels[i].SetActive(false);
        }
        gameData = FindObjectOfType<GameData>();
        if(gameData != null)
        {
            for (int i = 0; i < gameData.saveData.isActive.Length; i ++)
            {
                if(gameData.saveData.isActive[i])
                {
                    currentLevel = i;
                }
            }
        }
        page = (int)Mathf.Floor(currentLevel / 9);
        currentPage = levelSelectPanels[page];
        levelSelectPanels[page].SetActive(true);
	}
	
	// Update is called once per frame

    public void PageRight()
    {
        if(page < levelSelectPanels.Length - 1)
        {
            currentPage.SetActive(false);
            page++;
            currentPage = levelSelectPanels[page];
            currentPage.SetActive(true);
        }
    }

    public void PageLeft()
    {

        if (page > 0)
        {
            currentPage.SetActive(false);
            page--;
            currentPage = levelSelectPanels[page];
            currentPage.SetActive(true);
        }
    }
}
