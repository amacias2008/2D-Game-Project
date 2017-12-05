using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class RoundManager : NetworkBehaviour {

    string mainText;
    string clockText;

    // Round
    private bool fightActive = false;
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
        // check for ESC input
        if (Input.GetKeyDown(KeyCode.Escape)) FullReset();

        // check for R input
        if (Input.GetKeyDown(KeyCode.R)) NewRound();

        // Round timer
        if (fightActive) roundTime -= Time.deltaTime;

        //
        if (p1won || p2won) return;

        // Countdown timer at start of round
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
			PlayerCanvas.canvas.WriteBigText (mainText);
			string clockText;
			clockText = "";
			PlayerCanvas.canvas.WriteRoundText (clockText);

        }
        else if (secondsOnCountdown == 0)
        {
            //mainText.text = "FIGHT!";
            //clockText.text = "" + (int)roundTime;
			mainText = "FIGHT!";
			PlayerCanvas.canvas.WriteBigText (mainText);
			clockText = "" + (int)roundTime;
			PlayerCanvas.canvas.WriteRoundText (clockText);

            fightActive = true;
        }
        else
        {
            //mainText.text = "";
            //clockText.text = "" + (int)roundTime;

			PlayerCanvas.canvas.WriteBigText ("");
			clockText = "" + (int)roundTime;
			PlayerCanvas.canvas.WriteRoundText (clockText);
        }


        var players = GameObject.FindGameObjectsWithTag("Player");
        
        // Time ran out
        if (roundTime < 0)
        {
            fightActive = false;
            timeExpired = true;

            foreach (var p in players)
            {
                PlayerController pc = p.GetComponent<PlayerController>();
                pc.IdleAnim();
            }
        }

        // Player ran out of HP or fell off the level
        foreach (var p in players)
        {
            HealthScript hs = p.GetComponent<HealthScript>();
            PlayerController pc = p.GetComponent<PlayerController>();
            if (hs.GetHealth() <= 0 || p.transform.position.y < -20)
            {
                fightActive = false;
                pc.IdleAnim();

                if (hs.GetHealth() <= 0) healthKO = true;
                if (p.transform.position.y < -20) offMapKO = true;
            }
        }

        // Determine which PlayerController is player 1
        PlayerController playerOne = GameObject.FindGameObjectsWithTag("Player")[0].GetComponent<PlayerController>();
        PlayerController playerTwo = GameObject.FindGameObjectsWithTag("Player")[1].GetComponent<PlayerController>();
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

		if (p1won && !p2won) PlayerCanvas.canvas.WriteBigText("PLAYER 1 WON!\n");
		if (!p1won && p2won) PlayerCanvas.canvas.WriteBigText("PLAYER 2 WON!\n");
		if (p1won && p2won) PlayerCanvas.canvas.WriteBigText("IT'S A DRAW!\n");

		if (p1won || p2won) {
			mainText = "";
			mainText += PlayerCanvas.canvas.bigText + "Score: " + p1wins + " - " + p2wins;
			PlayerCanvas.canvas.WriteBigText (mainText);
			//mainText.text += "Score:  " + p1wins + " - " + p2wins;
		}
    }

    // Reset round
    void NewRound()
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
    void FullReset()
    {
        NewRound();

        p1wins = 0;
        p2wins = 0;
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
        var players = GameObject.FindGameObjectsWithTag("Player");
        foreach (var p in players)
        {
            PlayerController pc = p.GetComponent<PlayerController>();
            pc.NewRound();
        }
    }

    // Returns true if the fight is active and clock running
    public bool IsFightActive()
    {
        return fightActive;
    }
}
