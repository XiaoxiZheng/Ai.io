using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Unity TTS manager.
/// </summary>
public static class TTSManager
{
    public enum STREAM
    {
        Alarm = 4,
        DTMF = 8,
        Music = 3,
        Notification = 5,
        Ring = 2 }
    ;

    public class Locale
    {
        public string Name { get; private set; }

        public string Language { get; private set; }

        public Locale(string name, string language)
        {
            Name = name;
            Language = language;
        }
    }

    public static readonly Locale CHINESE = new Locale("Chinese", "zh");
    public static readonly Locale ENGLISH = new Locale("English", "en");
    public static readonly Locale FRENCH = new Locale("French", "fr");
    public static readonly Locale GERMAN = new Locale("German", "de");
    public static readonly Locale ITALIAN = new Locale("Italian", "it");
    public static readonly Locale JAPANESE = new Locale("Japanese", "ja");
    public static readonly Locale KOREAN = new Locale("Korean", "ko");
    public static readonly Locale[] Locales;
    public const int SUCCESS = 0;
    public const int ERROR = -1;
    private static AndroidJavaObject ttsManager = null;

    static TTSManager()
    {
        Locales = new Locale[]
        {
            CHINESE,
            ENGLISH,
            FRENCH,
            GERMAN,
            ITALIAN,
            JAPANESE,
            KOREAN
        };
    }

    /// <summary>
    /// Initialize the TextToSpeech Manager.
    /// </summary>
    /// <param name="gameObjectName">Game Object name holding the callback method.</param>
    /// <param name="callbackMethodName">Callback method name.</param>
    public static void Initialize(string gameObjectName, string callbackMethodName)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        ttsManager = new AndroidJavaObject("com.takohi.unity.plugins.tts.TTSManager", new object[] {
                                gameObjectName,
                                callbackMethodName
                        });
    }

    /// <summary>
    /// Speaks the string using the specified queuing strategy and speech parameters. This method is asynchronous, i.e. the method just adds the request to the queue of TTS requests and then returns. The synthesis might not have finished (or even started!) at the time when this method returns.
    /// </summary>
    /// <param name="text">The string of text to be spoken.</param>
    /// <param name="addToQueue">The queuing strategy to use. If true, the new entry is added at the end of the playback queue. Otherwise, all entries in the playback queue (text to be synthesized) are dropped and replaced by the new entry.</param>
    /// <param name="stream">Specify the audio stream type to be used when speaking text.</param>
    /// <param name="volume">Specify the speech volume relative to the current stream type volume used when speaking text. Volume is specified as a float ranging from 0 to 1 where 0 is silence, and 1 is the maximum volume (the default behavior).</param>
    /// <param name="pan">Specify how the speech is panned from left to right when speaking text. Pan is specified as a float ranging from -1 to +1 where -1 maps to a hard-left pan, 0 to center (the default behavior), and +1 to hard-right.</param>
    /// <param name="gameObjectName">Game Object name holding the callback name. (optional)</param>
    /// <param name="callbackMethodName">Method to call when the entry is played and completed. (optional)</param>
    /// <param name="id">Identifier of this entry. (optional)</param>
    public static void Speak(string text, bool addToQueue, STREAM stream = STREAM.Music,
                      float volume = 1f, float pan = 0f, string gameObjectName = null,
                      string callbackMethodName = null, string id = null)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        if (!IsInitialized())
            throw new System.InvalidOperationException("TTS not initialized.");

        ttsManager.Call("speak", text, addToQueue, (int)stream, volume, pan, gameObjectName, callbackMethodName, id);
    }

    /// <summary>
    /// Plays silence for the specified amount of time using the specified queue mode.
    /// </summary>
    /// <param name="durationInMs">Duration in ms of the silence.</param>
    /// <param name="addToQueue">The queuing strategy to use. If true, the new entry is added at the end of the playback queue. Otherwise, all entries in the playback queue (text to be synthesized) are dropped and replaced by the new entry.</param>
    /// <param name="gameObjectName">Game Object name holding the callback name. (optional)</param>
    /// <param name="callbackMethodName">Method to call when the entry is played and completed. (optional)</param>
    /// <param name="id">Identifier of this entry. (optional)</param>
    public static void PlaySilence(int durationInMs, bool addToQueue, string gameObjectName = null,
                             string callbackMethodName = null, string id = null)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;
        
        if (!IsInitialized())
            throw new System.InvalidOperationException("TTS not initialized.");
        
        ttsManager.Call("playSilence", durationInMs, addToQueue, gameObjectName, callbackMethodName, id);
    }

    /// <summary>
    /// Synthesizes to file. The application must have the WRITE_EXTERNAL_STORAGE permission (Write Access set to External in the Unity player settings).
    /// </summary>
    /// <param name="text">The text that should be synthesized.</param>
    /// <param name="filename">Absolute file filename to write the generated audio data to.It should be something like "/sdcard/myappsounds/mysound.wav".</param>
    /// <param name="gameObjectName">Game Object name holding the callback name. (optional)</param>
    /// <param name="callbackMethodName">Method to call when the entry is played and completed. (optional)</param>
    /// <param name="id">Identifier of this entry. (optional)</param>
    public static void SynthesizeToFile(string text, string filename, string gameObjectName = null,
                                   string callbackMethodName = null, string id = null)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;
        
        if (!IsInitialized())
            throw new System.InvalidOperationException("TTS not initialized.");
        
        ttsManager.Call("synthesizeToFile", text, filename, gameObjectName, callbackMethodName, id);
    }

    /// <summary>
    /// Interrupts the current speech and discards others in the queue.
    /// </summary>
    public static void Stop()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;
            
        if (!IsInitialized())
            throw new System.InvalidOperationException("TTS not initialized.");
            
        ttsManager.Call("stop");
    }

    /// <summary>
    /// Releases the resources used by the TextToSpeech engine.
    /// </summary>
    public static void Shutdown()
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        if (!IsInitialized())
            throw new System.InvalidOperationException("TTS not initialized.");

        ttsManager.Call("shutdown");
        ttsManager = null;
    }

    /// <summary>
    /// Determines if is initialized.
    /// </summary>
    /// <returns><c>true</c> if is initialized; otherwise, <c>false</c>.</returns>
    public static bool IsInitialized()
    {
        return Application.platform == RuntimePlatform.Android && ttsManager != null && ttsManager.Call<bool>("isInitialized");
    }

    /// <summary>
    /// Checks whether the TTS engine is busy speaking.
    /// </summary>
    /// <returns><c>true</c> if is speaking; otherwise, <c>false</c>.</returns>
    public static bool IsSpeaking()
    {
        if (Application.platform != RuntimePlatform.Android)
            return false;

        if (!IsInitialized())
            throw new System.InvalidOperationException("TTS not initialized.");

        return ttsManager.Call<bool>("isSpeaking");
    }

    /// <summary>
    /// Checks if the specified language as represented by the Locale is available and supported.
    /// </summary>
    /// <returns><c>true</c> if is language available the specified locale; otherwise, <c>false</c>.</returns>
    /// <param name="locale">The Locale describing the language to be used.</param>
    public static bool IsLanguageAvailable(Locale locale)
    {
        if (Application.platform != RuntimePlatform.Android)
            return false;

        if (!IsInitialized())
            throw new System.InvalidOperationException("TTS not initialized.");

        return ttsManager.Call<bool>("isLanguageAvailable", locale.Language);
    }

    /// <summary>
    /// Returns all languages supported by the current device.
    /// </summary>
    /// <returns>The available languages.</returns>
    public static List<Locale> GetAvailableLanguages()
    {
        List<Locale> availableLanguages = new List<Locale>();

        if (Application.platform != RuntimePlatform.Android)
            return availableLanguages;

        if (!IsInitialized())
            throw new System.InvalidOperationException("TTS not initialized.");

        foreach (Locale locale in Locales)
        {
            if (IsLanguageAvailable(locale))
                availableLanguages.Add(locale);
        }
        return availableLanguages;
    }

    /// <summary>
    /// Sets the text-to-speech language. The TTS engine will try to use the closest match to the specified language as represented by the Locale, but there is no guarantee that the exact same Locale will be used. Use isLanguageAvailable(Locale) to check the level of support before choosing the language to use.
    /// </summary>
    /// <returns><c>true</c>, if language was set, <c>false</c> otherwise.</returns>
    /// <param name="locale">The Locale describing the language to be used.</param>
    public static bool SetLanguage(Locale locale)
    {
        if (Application.platform != RuntimePlatform.Android)
            return false;

        if (!IsInitialized())
            throw new System.InvalidOperationException("TTS not initialized.");

        return ttsManager.Call<bool>("setLanguage", locale.Language);
    }

    /// <summary>
    /// Sets the speech pitch for the TextToSpeech engine.
    /// </summary>
    /// <param name="pitch">Speech pitch. 1.0 is the normal pitch, lower values lower the tone of the synthesized voice, greater values increase it.</param>
    public static void SetPitch(float pitch)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        if (!IsInitialized())
            throw new System.InvalidOperationException("TTS not initialized.");

        ttsManager.Call("setPitch", pitch);
    }

    /// <summary>
    /// Sets the speech rate. This has no effect on any pre-recorded speech.
    /// </summary>
    /// <param name="rate">Speech rate. 1.0 is the normal speech rate, lower values slow down the speech (0.5 is half the normal speech rate), greater values accelerate it (2.0 is twice the normal speech rate).</param>
    public static void SetSpeechRate(float rate)
    {
        if (Application.platform != RuntimePlatform.Android)
            return;

        if (!IsInitialized())
            throw new System.InvalidOperationException("TTS not initialized.");

        ttsManager.Call("setSpeechRate", rate);
    }
}
