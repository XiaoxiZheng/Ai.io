using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Text;
using System.IO;
using System;
using System.Linq;
using ApiAiSDK;

public class mainDriver : MonoBehaviour
{	
	//public Gameobject apiAiObj;
	public ApiAiModule apiAiScript; 

	// Use this for initialization
	void Start ()
	{	
		//apiAiObj = GameObject.FindGameObjectsWithTag ("ai.io");
		//apiAiScript = apiAiObj.GetComponent<ApiAiModule>;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetKeyDown (KeyCode.Space)) {
			Debug.Log ("Space Key Pressed!");
			apiAiScript.StartListening ();
		} else {
			apiAiScript.StopListening ();
			//Space bar isn't pressed
		}
	}
}


