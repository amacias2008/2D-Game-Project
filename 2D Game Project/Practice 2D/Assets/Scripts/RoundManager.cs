using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class RoundManager : NetworkBehaviour {

    string mainText;
    string clockText;

    // Round
	[SyncVar]
    public bool fightActive = false;
    private bool timeExpired = false;
    private bool healthKO = false;
    private bool offMapKO = false;
    private float roundTime = 60;

    // Score
    private int p1wins = 0, p2wins = 0;
    private bool p1won = false, p2won = false;

    // Countdown timer
    private int secondsOnCountdown = 3;
    private float timeUntilNextSec = 1;

	// Use this for initialization
	void Start ()
    {
        
	}

    // Update is called once per frame
    void Update()
    {
		//Check ESC for reset
		//FullReset(); 

        // check for R input
		//NewRound();

        // Round timer
		RoundTimer();

        //Check if either player has won
        if (p1won || p2won) return;

        // Countdown timer at start of round
		Timer();
        
        
        // Time ran out
		TimeIsUp();

        // Player ran out of HP or fell off the level
		GameEndCheck();

        // Determine which PlayerController is player 1
		DetermineWinner();
        
    }

    // Reset round
	[Server]
    public void NewRound()
    {
		Debug.Log ("hello2");
		CmdNewRound ();
    }

	[Command]
	void CmdNewRound()
	{
		fightActive = false;
		timeExpired = false;
		healthKO = false;
		offMapKO = false;
		roundTime = 60;

		secondsOnCountdown = 3;
		timeUntilNextSec = 1;

		p1won = false;
		p2won = false;

		ClearPickups();
		ResetPlayers();
	}


    // Reset round and score
    public void FullReset()
    {
			CmdServerReset();
    }

	[Command]
	void CmdServerReset()
	{
		NewRound ();
		p1wins = 0;
		p2wins = 0;
	}

	void RoundTimer()
	{
		if (fightActive) 
			roundTime -= Time.deltaTime;
	}

	void Timer()
	{
		if (timeUntilNextSec > -1) timeUntilNextSec -= Time.deltaTime;
		if (timeUntilNextSec <= 0 && secondsOnCountdown >= 0)
		{
			secondsOnCountdown--;
			timeUntilNextSec += 1;
		}
		// Display text for countdown timer
		if (secondsOnCountdown > 0)
		{
			//mainText.text = "" + secondsOnCountdown;
			//clockText.text = "";
			string mainText;
			mainText = "" + secondsOnCountdown;
			Debug.Log (mainText);
			PlayerCanvas.canvas.setMainText (mainText);
			string clockText;
			clockText = "";
			PlayerCanvas.canvas.setRoundText (clockText);

		}
		else if (secondsOnCountdown == 0)
		{
			//mainText.text = "FIGHT!";
			//clockText.text = "" + (int)roundTime;
			mainText = "FIGHT!";
			PlayerCanvas.canvas.setMainText (mainText);
			clockText = "" + (int)roundTime;
			PlayerCanvas.canvas.setRoundText (clockText);

			fightActive = true;
		}
		else
		{
			//mainText.text = "";
			//clockText.text = "" + (int)roundTime;

			PlayerCanvas.canvas.setMainText ("");
			clockText = "" + (int)roundTime;
			PlayerCanvas.canvas.setRoundText (clockText);
		}
	}

	void TimeIsUp()
	{
		var player = GameObject.FindGameObjectWithTag("Player");
		var local = GameObject.Find ("Local");

		if (roundTime < 0)
		{
			fightActive = false;
			timeExpired = true;

			PlayerController pc = player.GetComponent<PlayerController> ();
			PlayerController pc2 = local.GetComponent<PlayerController> ();

			pc.IdleAnim ();
			pc2.IdleAnim ();

			/*foreach (var p in players)
			{
				PlayerController pc = p.GetComponent<PlayerController>();
				pc.IdleAnim();
			}*/
		}
	}

	void GameEndCheck()
	{
		var player = GameObject.FindGameObjectWithTag("Player");
		var local = GameObject.Find ("Local");

		HealthScript hsPlayer = player.GetComponent<HealthScript> ();
		PlayerController pc = player.GetComponent<PlayerController> ();
			
		if (hsPlayer.GetHealth () <= 0 || player.transform.position.y < -20) 
		{
			fightActive = false;
			pc.IdleAnim ();

			if (hsPlayer.GetHealth () <= 0)
				healthKO = true;
			if (pc.transform.position.y < -20)
				offMapKO = true;
		}

		HealthScript localPlayer = local.GetComponent<HealthScript> ();
		PlayerController localPC = local.GetComponent<PlayerController> ();

		if (localPlayer.GetHealth () <= 0 || local.transform.position.y < -20) 
		{
			fightActive = false;
			localPC.IdleAnim ();

			if (localPlayer.GetHealth () <= 0)
				healthKO = true;
			if (localPC.transform.position.y < -20)
				offMapKO = true;
		}
			
	}

	void DetermineWinner()
	{
		//PlayerController playerOne = GameObject.FindGameObjectWithTag("Local").GetComponent<PlayerController>();
		PlayerController playerOne = GameObject.Find("Local").GetComponent<PlayerController>();
		//PlayerController playerTwo = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		PlayerController playerTwo = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
		if(playerOne.GetPlayerNumber() != 1)
		{
			PlayerController temp = playerOne;
			playerOne = playerTwo;
			playerTwo = temp;
		}

		// Win conditions
		if (timeExpired || healthKO) // Time or health ran out
		{
			if(playerOne.GetHealth() > playerTwo.GetHealth())
			{
				p1won = true;
			}
			else if (playerOne.GetHealth() < playerTwo.GetHealth())
			{
				p2won = true;
			}
			else
			{
				p1won = true;
				p2won = true;
				Debug.Log("1");
			}
		}
		else if(offMapKO) // Fell off map
		{
			if (playerOne.GetY() > playerTwo.GetY())
			{
				p1won = true;
			}
			else if (playerOne.GetY() < playerTwo.GetY())
			{
				p2won = true;
			}
			else
			{
				p1won = true;
				p2won = true;
				Debug.Log("2");
			}
		}

		if (p1won) p1wins++;
		if (p2won) p2wins++;

		if (p1won && !p2won) PlayerCanvas.canvas.setMainText("PLAYER 1 WON!\n");
		if (!p1won && p2won) PlayerCanvas.canvas.setMainText("PLAYER 2 WON!\n");
		if (p1won && p2won) PlayerCanvas.canvas.setMainText("IT'S A DRAW!\n");

		if (p1won || p2won) {
			string mainText = "";
			mainText = PlayerCanvas.canvas.bigText + "Score: " + p1wins + " - " + p2wins;
			PlayerCanvas.canvas.setMainText (mainText);
			//mainText.text += "Score:  " + p1wins + " - " + p2wins;
		}
	}

    // Destroy all Pickups
    void ClearPickups()
    {
        var pickups = GameObject.FindGameObjectsWithTag("Pickup");
        foreach (var p in pickups)
        {
            Destroy(p);
        }
    }

    // Equip knife, fill health, clear powerup effects, and move to spawn
    void ResetPlayers()
    {
		var player = GameObject.FindGameObjectWithTag ("Player");
		PlayerController pc = player.GetComponent<PlayerController> ();
		pc.NewRound ();

		var localPlayer = GameObject.Find ("Local");
		PlayerController pc2 = localPlayer.GetComponent<PlayerController> ();
		pc2.NewRound ();
        /*var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
        {
            PlayerController pc = p.GetComponent<PlayerController>();
            pc.NewRound();
        }*/
    }

    // Returns true if the fight is active and clock running
    public bool IsFightActive()
    {
        return fightActive;
    }
}
