using UnityEngine;
using System.Collections.Generic;

public class TTSDemoSceneManager : MonoBehaviour
{
    private bool _initializeError = false;
    private string _inputText = "\n\n\n\n";
    private int _speechId = 0;
    private float _pitch = 1f, _speechRate = 1f;
    private int _selectedLocale = 0;
    private string[] _localeStrings;

    // Use this for initialization
    void Start()
    {
        // Screen.sleepTimeout = SleepTimeout.NeverSleep;
        TTSManager.Initialize(transform.name, "OnTTSInit");
    }
    
    void OnGUI()
    {
        float originalScreenWidth = 400f;
        float originalScreenHeight = (originalScreenWidth / Screen.width) * Screen.height;
        float scale = Screen.width / originalScreenWidth;
        Matrix4x4 svMat = GUI.matrix; // save current matrix
        GUI.matrix = Matrix4x4.Scale(Vector3.one * scale);

        GUI.skin.label.fontStyle = FontStyle.Bold;
        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUI.Label(new Rect(0f, 0f, originalScreenWidth, originalScreenHeight * 0.05f), "Android TTS Plugin");

        GUI.skin.label.fontStyle = FontStyle.Normal;
        GUI.skin.label.alignment = TextAnchor.LowerLeft;
        GUI.skin.horizontalSliderThumb.fixedWidth = 30;
        GUI.skin.horizontalSliderThumb.fixedHeight = 30;
        Rect layoutRect = new Rect(originalScreenWidth * 0.05f, originalScreenHeight * 0.1f, originalScreenWidth * 0.9f, originalScreenHeight * 0.85f);
        GUILayout.BeginArea(layoutRect, GUI.skin.box);
        
        GUILayout.BeginVertical();
        
        GUILayout.Space(25f);
        
        GUILayout.Label("Initialized: " + TTSManager.IsInitialized());
        
        GUILayout.Space(35f);

        if (TTSManager.IsInitialized())
        {
            _inputText = GUILayout.TextArea(_inputText);

            if (GUILayout.Button("Speak"))
                TTSManager.Speak(_inputText, false, TTSManager.STREAM.Music, 1f, 0f, transform.name, "OnSpeechCompleted", "speech_" + (++_speechId));

            if (GUILayout.Button("Add to Queue"))
                TTSManager.Speak(_inputText, true, TTSManager.STREAM.Music, 1f, 0f, transform.name, "OnSpeechCompleted", "speech_" + (++_speechId));

            if (GUILayout.Button("Synthesize to SDCard root folder"))
                TTSManager.SynthesizeToFile(_inputText, "/mnt/sdcard/speech_" + (++_speechId) + ".wav", transform.name, "OnSynthesizeCompleted", "speech_" + (++_speechId));

            if (GUILayout.Button("Stop"))
                TTSManager.Stop();

            GUILayout.Space(25f);

            GUILayout.Label("Device Supported Languages:");

            if (_localeStrings != null && _localeStrings.Length > 0)
            {
                _selectedLocale = GUILayout.SelectionGrid(_selectedLocale, _localeStrings, 3);
                TTSManager.SetLanguage(TTSManager.GetAvailableLanguages() [_selectedLocale]);
            } 

            GUILayout.Space(25f);

            GUILayout.BeginHorizontal();

            GUILayout.Label("Pitch: ");

            _pitch = GUILayout.HorizontalSlider(_pitch, 0f, 2f);
            TTSManager.SetPitch(_pitch);

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            
            GUILayout.Label("Speech Rate: ");
            
            _speechRate = GUILayout.HorizontalSlider(_speechRate, 0f, 2f);
            TTSManager.SetSpeechRate(_speechRate);
            
            GUILayout.EndHorizontal();
                        
        } else if (_initializeError)
        {
            GUI.contentColor = Color.red;
            GUILayout.Label("Error during initialization of TTS.\nIs it installed on your device?");
        }

        GUILayout.EndVertical();
        
        GUILayout.EndArea();

        GUI.matrix = svMat;
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
