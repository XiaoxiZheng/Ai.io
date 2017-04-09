//
// API.AI Unity SDK Sample
// =================================================
//
// Copyright (C) 2015 by Speaktoit, Inc. (https://www.speaktoit.com)
// https://www.api.ai
//
// ***********************************************************************************************************************
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with
// the License. You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on
// an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the
// specific language governing permissions and limitations under the License.
//
// ***********************************************************************************************************************

using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Reflection;
using ApiAiSDK;
using ApiAiSDK.Model;
using ApiAiSDK.Unity;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Generic;
using SimpleJSON; 


public class ApiAiModule : MonoBehaviour
{

	public Text answerTextField;
	public Text inputTextField;
	public AudioClip listeningSound;

	private ApiAiUnity apiAiUnity;
	private AudioSource aud;
	private bool startedListening = false;

	//Text to Speech Stuff
	private bool _initializeError = false;
	private int _speechId = 0;
	private float _pitch = 1f, _speechRate = 1f;
	private int _selectedLocale = 1;
	private string[] _localeStrings;

	private readonly JsonSerializerSettings jsonSettings = new JsonSerializerSettings
	{
		NullValueHandling = NullValueHandling.Ignore,
	};

	private readonly Queue<Action> ExecuteOnMainThread = new Queue<Action>();

	// Use this for initialization
	IEnumerator Start()
	{	

		// Screen.sleepTimeout = SleepTimeout.NeverSleep;
		TTSManager.Initialize(transform.name, "OnTTSInit");


		// check access to the Microphone
		yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
		if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
		{
			throw new NotSupportedException("Microphone using not authorized");
		}

		ServicePointManager.ServerCertificateValidationCallback = (a, b, c, d) =>
		{
			return true;
		};

		const string ACCESS_TOKEN = "9df41c2b8ebb4c1c83d3e5ca685e76fa";// - TrappedBot//aVeryPoliticalBot: 5566b67b2556447cb8ea0a005475c038 //Default: 3485a96fb27744db83e78b8c4bc9e7b7

		var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);

		apiAiUnity = new ApiAiUnity();
		apiAiUnity.Initialize(config);
	
		apiAiUnity.OnError += HandleOnError;
		apiAiUnity.OnResult += HandleOnResult;
	}

	void HandleOnResult(object sender, AIResponseEventArgs e)
	{
		RunInMainThread (() => {
			var aiResponse = e.Response;
			if (aiResponse != null) {
				Debug.Log (aiResponse.Result.ResolvedQuery);
				string outText = JsonConvert.SerializeObject (aiResponse, jsonSettings);
				Debug.Log ("outText: " + outText);

				/*
				var JSON_outText = JSON.Parse(outText);
				Debug.Log ("JSON_outText: " + JSON_outText);

				string endOutputText = JSON_outText["fulfillment"]["speech"].Value;
				Debug.Log ("endOutputText: " + endOutputText); 
				*/

				String [] endOutputText = outText.Split(new string[] {"speech"},StringSplitOptions.None);
				String[] endSubStrings = endOutputText [1].Split (':', '}');

				Debug.Log ("Size of endSubStrings: " + endSubStrings.Length);
				for (int i = 0; i < endSubStrings.Length; i++) {
					Debug.Log ("endSubStrings[i]: " + endSubStrings[i]); 
				}

				if(endSubStrings[1].Equals("")){
					endSubStrings [1] = "I didn't catch that, will you say it again?"; 
				}
				answerTextField.text = endSubStrings[1];

			} else {
				Debug.LogError ("Response is null");
			}
			if (TTSManager.IsInitialized ()) {

				TTSManager.SetLanguage (TTSManager.GetAvailableLanguages () [_selectedLocale]);

				TTSManager.SetPitch (_pitch);
				TTSManager.SetSpeechRate (_speechRate);

				TTSManager.Speak (answerTextField.text, false, TTSManager.STREAM.Music, 1f, 0f, transform.name, "OnSpeechCompleted", "speech_" + (++_speechId));
			} else if (_initializeError) {
				Debug.LogError ("TTSManager _initializeError");
			}
		});
	}
	void HandleOnError(object sender, AIErrorEventArgs e)
	{
		RunInMainThread(() => {
			Debug.LogException(e.Exception);
			Debug.Log(e.ToString());
			answerTextField.text = e.Exception.Message;
		});
	}

	// Update is called once per frame
	void Update()
	{
		if (apiAiUnity != null)
		{
			apiAiUnity.Update();
		}

		// dispatch stuff on main thread
		while (ExecuteOnMainThread.Count > 0)
		{
			ExecuteOnMainThread.Dequeue().Invoke();

		}
			
	}

	void LateUpdate()
	{
		if(apiAiUnity != null) {
			//Debug.Log ("Space Key Pressed!");
			//Input.GetButtonDown("Submit");
			if (Application.platform == RuntimePlatform.Android) {
				// you can use Android recognition here
				if(Input.GetAxis ("JoyA")==1){
					StartNativeRecognition();
					Debug.Log("Vertical : " + Input.GetAxis("Vertical"));
					Debug.Log("JoyA==1 : " + Input.GetAxis("JoyA"));

				}
				/*
				//Input.GetButtonDown("Cancel");
				if (Input.GetAxis ("JoyB")== 1){
					StartNativeRecognition();
					Debug.Log("Horizontal : " + Input.GetAxis("Horizontal"));
					Debug.Log("JoyB==1 : " + Input.GetAxis("JoyB"));
				}
				*/
			}
			else{
			//if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
				// Using Unity Default Voice Regonition Powered by Microsoft
				if(Input.GetKey(KeyCode.Space) && !startedListening || Input.GetAxis ("JoyA")==1 && !startedListening){
					StartListening ();
					startedListening = true;
					Debug.Log("Vertical : " + Input.GetAxis("Vertical"));
					Debug.Log("JoyA==1 : " + Input.GetAxis("JoyA"));

				}
				//Input.GetButtonDown("Cancel");
				if (Input.GetKey(KeyCode.Escape) || Input.GetAxis ("JoyB")== 1){
					StopListening ();
					startedListening = false;
					Debug.Log("Horizontal : " + Input.GetAxis("Horizontal"));
					Debug.Log("JoyB==1 : " + Input.GetAxis("JoyB"));
				}
			}
		}
	}
	private void RunInMainThread(Action action)
	{
		ExecuteOnMainThread.Enqueue(action);
	}

	public void PluginInit()
	{

	}

	public void StartListening()
	{
		Debug.Log("StartListening");

		if (answerTextField != null)
		{
			answerTextField.text = "Listening...";
		}

		aud = GetComponent<AudioSource>();
		apiAiUnity.StartListening(aud);

	}

	public void StopListening()
	{
		try
		{
			Debug.Log("StopListening");

			if (answerTextField != null)
			{
				answerTextField.text = "";
			}

			apiAiUnity.StopListening();
		} catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}

	public void SendText()
	{
		var text = inputTextField.text;

		Debug.Log(text);

		AIResponse response = apiAiUnity.TextRequest(text);

		if (response != null)
		{
			Debug.Log("Resolved query: " + response.Result.ResolvedQuery);
			string outText = JsonConvert.SerializeObject(response, jsonSettings);
			Debug.Log ("outText: " + outText);

			/*
			var JSON_outText = JSON.Parse(outText);
			Debug.Log ("JSON_outText: " + JSON_outText);

			string endOutputText = JSON_outText["fulfillment"]["speech"].Value;
			Debug.Log ("endOutputText: " + endOutputText); 
			*/

			String [] endOutputText = outText.Split(new string[] {"speech"},StringSplitOptions.None);
			String[] endSubStrings = endOutputText [1].Split (':', '}');

			Debug.Log ("Size of endSubStrings: " + endSubStrings.Length);
			for (int i = 0; i < endSubStrings.Length; i++) {
				Debug.Log ("endSubStrings[i]: " + endSubStrings[i]); 
			}

			if(endSubStrings[1].Equals("")){
				endSubStrings [1] = "I didn't catch that, will you say it again?"; 
			}
			answerTextField.text = endSubStrings[1];
		} else
		{
			Debug.LogError("Response is null");
		}

	}

	public void StartNativeRecognition()
	{
		try
		{
			apiAiUnity.StartNativeRecognition();
		} catch (Exception ex)
		{
			Debug.LogException(ex);
		}
	}


	void OnDestroy()
	{
		TTSManager.Shutdown();
	}

	void OnTTSInit(string message)
	{
		int response = int.Parse(message);

		switch (response)
		{
		case TTSManager.SUCCESS:
			List<TTSManager.Locale> l = TTSManager.GetAvailableLanguages();
			_localeStrings = new string[l.Count];
			for (int i = 0; i < _localeStrings.Length; ++i)
				_localeStrings [i] = l [i].Name;

			break;
		case TTSManager.ERROR:
			_initializeError = true;
			break;
		}
	}

	void OnSpeechCompleted(string id)
	{
		Debug.Log("Speech '" + id + "' is complete.");
	}

	void OnSynthesizeCompleted(string id)
	{
		Debug.Log("Synthesize of speech '" + id + "' is complete.");
	}
}