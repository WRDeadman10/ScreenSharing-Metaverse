using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WR.ScreenShare
{
    public class TheAssigner : MonoBehaviour
    {
        public static TheAssigner Instance;
        public GameObject TVParent;
        public Transform SpeakerButtonPrefeb;
        public TMPro.TextMeshProUGUI SelectedAudioDeviceText;
        public Button refreshAudioDevices;

        private void Awake()
        {
            Instance = this;
        }
    }
}