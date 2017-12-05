using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerCanvas : NetworkBehaviour {

	public static PlayerCanvas canvas;

	public Image GreenHealthBar;				//Used as foreground color of health bar
	public Image RedHealthBar;					//Used as background color of health bar
	public Image EnemyHealthBar;
	public Image EnemyRedHealthBar;
	public Text PlayerName;
	public Text debugText;
	public Text debugText2;
	public Text roundTimeText;
	public Text bigText;

	[SyncVar(hook = "OnRoundTextChange") ]
	string roundString;
	[SyncVar(hook = "OnMainTextChange") ]
	string mainString;

	void Awake()
	{
		if (canvas == null)
			canvas = this;
		else
			Destroy (gameObject);
	}
	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	public void SetHealth(float amount)
	{
		GreenHealthBar.fillAmount = amount / 100;
	}

	public void SetEnemyHealth(float amount)
	{
		EnemyHealthBar.fillAmount = amount / 100;
	}

	public void WritePlayerText(string text)
	{
		PlayerName.text = text;
	}
	public void WriteDebugText(string text)
	{
		debugText.text = text;
	}

	public void WriteDebugTextTwo(string text)
	{
		debugText2.text = text;
	}

	public void setRoundText(string text)
	{
		roundString = text;
	}

	public void setMainText(string text)
	{
		mainString = text;
	}

	public void OnRoundTextChange(string text)
	{
		roundTimeText.text = "";
		roundTimeText.text = text;
	}

	public void OnMainTextChange(string text)
	{
		bigText.text = "";
		bigText.text = text;
	}
}