using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadePanelController : MonoBehaviour {

	public Animator panelAnim;
	public Animator gameInfoAnim;

	public void OK(){
		if (panelAnim != null && gameInfoAnim != null)
		{
			panelAnim.SetBool("Out", true);
			gameInfoAnim.SetBool("Out", true);
            StartCoroutine(GameStartCo());
		}
	}

    public void GameOver()
    {
        panelAnim.SetBool("Out", false);
        panelAnim.SetBool("Game Over", true);
    }

    IEnumerator GameStartCo()
    {
        yield return new WaitForSeconds(1f);
        Board board = FindObjectOfType<Board>();
        board.currentState = GameState.move;
    }
}
