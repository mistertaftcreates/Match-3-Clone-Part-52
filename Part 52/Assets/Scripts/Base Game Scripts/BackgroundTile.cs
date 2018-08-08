using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundTile : MonoBehaviour {
	public int hitPoints;
	private SpriteRenderer sprite;
    private GoalManager goalManager;

	private void Start()
	{
        goalManager = FindObjectOfType<GoalManager>();
		sprite = GetComponent<SpriteRenderer>();
	}

	private void Update()
	{
		if(hitPoints<= 0)
		{
            if(goalManager != null)
            {
                goalManager.CompareGoal(this.gameObject.tag);
                goalManager.UpdateGoals();
            }
			Destroy(this.gameObject);
		}
	}

	public void TakeDamage(int damage)
	{
		hitPoints -= damage;
		MakeLighter();
	}

    void MakeLighter()
	{
		//take the current color
		Color color = sprite.color;
		//Get the current color's alpha value and cut it in half.  
		float newAlpha = color.a * .5f;
		sprite.color = new Color(color.r, color.g, color.b, newAlpha);
	}
}
