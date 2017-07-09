using System.Collections;
using System.Collections.Generic;
using Net.Junian.UniPlugins;
using UnityEngine;

public class SpotifySample : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlayMusic()
    {
        SpotifyPlayer.Instance.Play("spotify:track:5uJgz2iuvfePb9RFJYy4pu");
    }

    public void AuthSpotify()
    {
        UnityEngine.Debug.Log(SpotifyPlayer.Instance.Init("coba command"));
    }
}
