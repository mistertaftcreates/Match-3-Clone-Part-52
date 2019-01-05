using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

	public AudioSource[] destroyNoise;
    public AudioSource backgroundMusic;

    private void Start()
    {
        if(PlayerPrefs.HasKey("Sound"))
        {
            if(PlayerPrefs.GetInt("Sound") == 0)
            {
                backgroundMusic.Play();
                backgroundMusic.volume = 0;
            }
            else{
                backgroundMusic.Play();
                backgroundMusic.volume = 1;
            }
        }
        else{
            backgroundMusic.Play();
            backgroundMusic.volume = 1;
        }
    }

    public void adjustVolume()
    {
        if (PlayerPrefs.HasKey("Sound"))
        {
            if (PlayerPrefs.GetInt("Sound") == 0)
            {
                backgroundMusic.volume = 0;
            }
            else
            {
                backgroundMusic.volume = 1;
            }
        }
    }

    public void PlayRandomDestroyNoise()
	{
		//Choose a random number
		int clipToPlay = Random.Range(0, destroyNoise.Length);
		//play that clip
		destroyNoise[clipToPlay].Play();
	}
}
