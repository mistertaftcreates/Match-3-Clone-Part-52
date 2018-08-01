using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToSplash : MonoBehaviour {

    public string sceneToLoad;
    private GameData gameData;
    private Board board;

    public void WinOK()
    {
        if(gameData != null)
        {
            gameData.saveData.isActive[board.level + 1] = true;
            gameData.Save();
        }
        SceneManager.LoadScene(sceneToLoad);
    }

    public void LoseOK()
    {
        SceneManager.LoadScene(sceneToLoad);
    }

	// Use this for initialization
	void Start () {
        gameData = FindObjectOfType<GameData>();
        board = FindObjectOfType<Board>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
