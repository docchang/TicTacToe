using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;


public class Title : MonoBehaviour
{
	public void Play()
	{
		SceneManager.LoadScene ("Game");
	}

	public void Quit()
	{
		SceneManager.LoadScene ("Title");
	}
}
