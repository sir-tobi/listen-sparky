using FrostweepGames.Plugins.Core;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace FrostweepGames.Plugins.GoogleCloud.SpeechRecognition
{
    public class VoiceInteractionController : MonoBehaviour
    {
        #region singleton
        public static VoiceInteractionController Instance;

        private void Awake()
        {
            Instance = this;
        }
        #endregion

        public String CurrentResult = "";

        private GCSpeechRecognition _speechRecognition;

        public delegate void OnResultUpdateDelegate(String command);
        public static event OnResultUpdateDelegate resultUpdateDelegate;

        public delegate void OnRecordingEndedDelegate();
        public static event OnRecordingEndedDelegate recordingEndedDelegate;

        private void Start()
        {
            _speechRecognition = GCSpeechRecognition.Instance;
            _speechRecognition.RecognizeSuccessEvent += RecognizeSuccessEventHandler;
            _speechRecognition.RecognizeFailedEvent += RecognizeFailedEventHandler;
            _speechRecognition.FinishedRecordEvent += FinishedRecordEventHandler;
            _speechRecognition.StartedRecordEvent += StartedRecordEventHandler;
            _speechRecognition.RecordFailedEvent += RecordFailedEventHandler;
            _speechRecognition.EndTalkigEvent += EndTalkigEventHandler;

            _speechRecognition.RequestMicrophonePermission(null);

            // select first microphone device
            if (_speechRecognition.HasConnectedMicrophoneDevices())
            {
                _speechRecognition.SetMicrophoneDevice(_speechRecognition.GetMicrophoneDevices()[0]);
            }

            Debug.Log("Using microphone: " + _speechRecognition.GetMicrophoneDevices()[0].ToString());

            // Ask for mic permission
            if (!Debug.isDebugBuild)
            {
                StartRecording(false);
                StopRecording();
            }
        }

        public void StartRecording(bool withVoiceDetection)
        {
            CurrentResult = "";
            _speechRecognition.StartRecord(withVoiceDetection);
        }

        public void StopRecording()
        {
            _speechRecognition.StopRecord();
            recordingEndedDelegate();
            Debug.Log("Recording stopped.");
        }

        public void CancelRecording ()
        {
            _speechRecognition.CancelAllRequests();
        }

        // EVENTS

        private void StartedRecordEventHandler()
        {
            Debug.Log("Recording started.");
        }

        private void EndTalkigEventHandler(AudioClip clip, float[] raw)
        {
            Debug.Log("Talking ended.");
        }

        private void RecordFailedEventHandler()
        {
            Debug.Log("Recording failed.");
        }

        private void FinishedRecordEventHandler(AudioClip clip, float[] raw)
        {
            if (clip == null)
            {
                // Global.Instance.dog.GetComponent<SheepdogController>().PreventCommand();
                return;
            }

            SoundController.Instance.PlaySound("dog", 0f, UnityEngine.Random.Range(0.3f, 0.4f), UnityEngine.Random.Range(1.1f, 1.5f));
            RecognitionConfig config = RecognitionConfig.GetDefault();
            Debug.Log("Config loaded.");
            config.languageCode = "EN";
            config.audioChannelCount = clip.channels;

            GeneralRecognitionRequest recognitionRequest = new GeneralRecognitionRequest()
            {
                audio = new RecognitionAudioContent()
                {
                    content = raw.ToBase64()
                },
                config = config
            };

            _speechRecognition.Recognize(recognitionRequest);
            Debug.Log("Started recognition");
        }

        private void RecognizeFailedEventHandler(string error)
        {
            Debug.Log("Recognition failed: " + error);
        }

        private void RecognizeSuccessEventHandler(RecognitionResponse response)
        {
            String result = "";
            try
            {
                foreach (var item in response.results[0].alternatives[0].words)
                {
                    result += item.word + " ";
                }
            }
            catch
            {
                Debug.Log("An error occured.");
            }

            UpdateResult(result);
        }

        public void UpdateResult(String command)
        {
            CurrentResult = command;
            resultUpdateDelegate(command);

        } 
    }
}

