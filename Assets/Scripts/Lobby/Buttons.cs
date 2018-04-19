﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Buttons : MonoBehaviour 
{

	public void RestartGame()
	{
		SceneManager.LoadScene ("Main");
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
