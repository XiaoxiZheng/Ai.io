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


public class MyApiAiModule : MonoBehaviour
{

    public Text answerTextField;
    public Text inputTextField;
	  public AudioClip listeningSound;

    private ApiAiUnity apiAiUnity;
    private AudioSource aud;
	private bool startedListening = false;

	//Text to Speech Stuff
	private bool _initializeError = false;
	private string _inputText = "\n\n\n\n";
	private int _speechId = 0;
	private float _pitch = 1f, _speechRate = 1f;
	private int _selectedLocale = 0;
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

		const string ACCESS_TOKEN = "3485a96fb27744db83e78b8c4bc9e7b7";//5566b67b2556447cb8ea0a005475c038 //Default: 3485a96fb27744db83e78b8c4bc9e7b7

        var config = new AIConfiguration(ACCESS_TOKEN, SupportedLanguage.English);

        apiAiUnity = new ApiAiUnity();
        apiAiUnity.Initialize(config);

		if (Application.platform == RuntimePlatform.Android) {
			// you can use Android recognition here
			StartNativeRecognition();
		}

		if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
			// Using Unity Default Voice Regonition Powered by Microsoft
		}
        apiAiUnity.OnError += HandleOnError;
        apiAiUnity.OnResult += HandleOnResult;
    }

    void HandleOnResult(object sender, AIResponseEventArgs e)
    {
        RunInMainThread(() => {
            var aiResponse = e.Response;
            if (aiResponse != null)
            {
                Debug.Log(aiResponse.Result.ResolvedQuery);
                var outText = JsonConvert.SerializeObject(aiResponse, jsonSettings);

                Debug.Log(outText);

                answerTextField.text = outText;

            } else
            {
                Debug.LogError("Response is null");
            }
        });

		if (TTSManager.IsInitialized ()) {
			TTSManager.Speak(answerTextField.text, false, TTSManager.STREAM.Music, 1f, 0f, transform.name, "OnSpeechCompleted", "speech_" + (++_speechId));
		}
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
			if(Input.GetKey(KeyCode.Space) && !startedListening){
				StartListening ();
				startedListening = true;
			}
			if (Input.GetKey(KeyCode.Escape)){
				StopListening ();
				startedListening = false;
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
            var outText = JsonConvert.SerializeObject(response, jsonSettings);

            Debug.Log("Result: " + outText);

            answerTextField.text = outText;
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
}
