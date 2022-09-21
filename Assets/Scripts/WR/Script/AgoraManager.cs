using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using UnityEngine.UI;
namespace WR.ScreenShare
{
    public class AgoraManager : MonoBehaviour
    {
        internal IRtcEngine mRtcEngine = null;

        //public Button joinChannel;
        //public Button leaveChannel;
        //public Button muteButton;
        //public Button ShareScreen;
        //public Button StopSharing;

        public GameObject BigScreenprefab;
        private AudioRawDataManager AudioRawDataManager;
        private AudioPlaybackDeviceManager audioPlaybackDeviceManager;

        private VideoDeviceManager videoDeviceManager;

        private void OnEnable()
        {
            //joinChannel.onClick.AddListener(delegate
            //{
            //    JoinChannel(true);
            //});
            //leaveChannel.onClick.AddListener(LeaveChannel);
            //muteButton.onClick.AddListener(MuteButtonTapped);
            //if (mRtcEngine != null)
            //{
            //    IRtcEngine.Destroy();
            //}
            mRtcEngine = VoiceChatManager.Instance.rtcEngine; //IRtcEngine.GetEngine("f010103072c845719b4a440626985113");

            //versionText.GetComponent<Text>().text = "Version : " + getSdkVersion();
            VideoEncoderConfiguration videoEncoderConfiguration = new VideoEncoderConfiguration
            {
                orientationMode = ORIENTATION_MODE.ORIENTATION_MODE_ADAPTIVE,
                degradationPreference = DEGRADATION_PREFERENCE.MAINTAIN_FRAMERATE
            };
            videoEncoderConfiguration.dimensions.width = 512;
            videoEncoderConfiguration.dimensions.height = 512;
            videoEncoderConfiguration.minFrameRate = 24;
            videoEncoderConfiguration.frameRate = FRAME_RATE.FRAME_RATE_FPS_30;

            CameraCapturerConfiguration cameraCapturerConfiguration = new CameraCapturerConfiguration
            {
                cameraDirection = CAMERA_DIRECTION.CAMERA_FRONT,
                preference = CAPTURER_OUTPUT_PREFERENCE.CAPTURER_OUTPUT_PREFERENCE_AUTO
            };

            mRtcEngine.SetCameraCapturerConfiguration(cameraCapturerConfiguration);
            mRtcEngine.SetVideoEncoderConfiguration(videoEncoderConfiguration);

            mRtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_MUSIC_STANDARD,
                AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_MEETING);

            mRtcEngine.EnableInEarMonitoring(false);

            mRtcEngine.SetInEarMonitoringVolume(80);

            audioPlaybackDeviceManager = (AudioPlaybackDeviceManager)mRtcEngine.GetAudioPlaybackDeviceManager();
            videoDeviceManager = (VideoDeviceManager)mRtcEngine.GetVideoDeviceManager();
        }

        private void OnDestroy()
        {
            //if (mRtcEngine != null)
            //{
            //    IRtcEngine.Destroy();
            //}
        }

        private void OnDisable()
        {
            audioPlaybackDeviceManager.ReleaseAAudioPlaybackDeviceManager();
            videoDeviceManager.ReleaseAVideoDeviceManager();
        }
    }
}