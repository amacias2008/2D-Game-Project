﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCanvas : MonoBehaviour {

	public static PlayerCanvas canvas;

	public Image GreenHealthBar;				//Used as foreground color of health bar
	public Image RedHealthBar;					//Used as background color of health bar
	public Text debugText;
	public Text debugText2;

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

	public void WriteDebugText(string text)
	{
		debugText.text = text;
	}

	public void WriteDebugTextTwo(string text)
	{
		debugText2.text = text;
	}
}