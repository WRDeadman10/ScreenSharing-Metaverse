using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using agora_gaming_rtc;
using System;
using Photon.Pun;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using WR.ScreenShare;
using UnityEngine.UI;

public class VoiceChatManager : MonoBehaviourPunCallbacks
{
    string appID = "38df37ed5ecc4e988fe0208008c718af";

    public static VoiceChatManager Instance;
    public PlayerRCPCarrier curruntScreensharingPlayer;
    internal IRtcEngine rtcEngine;

    private AudioRawDataManager AudioRawDataManager;
    private AudioPlaybackDeviceManager audioPlaybackDeviceManager;
    private AudioRecordingDeviceManager audioRecordingDeviceManager;
    private VideoDeviceManager videoDeviceManager;

    internal bool shareScreen { get; private set; }
    int GetAudioRecordingDeviceCount;

    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void Start()
    {
        if (string.IsNullOrEmpty(appID))
        {
            Debug.LogError("App ID not set in VoiceChatManager script");
            return;
        }

        rtcEngine = IRtcEngine.GetEngine(appID);

        rtcEngine.OnJoinChannelSuccess += OnJoinChannelSuccess;
        rtcEngine.OnLeaveChannel += OnLeaveChannel;
        rtcEngine.OnError += OnError;
        rtcEngine.OnUserJoined += onUserJoined;

        rtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
        rtcEngine.SetMultiChannelWant(true);
        rtcEngine.EnableSoundPositionIndication(true);

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

        rtcEngine.SetCameraCapturerConfiguration(cameraCapturerConfiguration);
        rtcEngine.SetVideoEncoderConfiguration(videoEncoderConfiguration);

        rtcEngine.SetAudioProfile(AUDIO_PROFILE_TYPE.AUDIO_PROFILE_DEFAULT,
            AUDIO_SCENARIO_TYPE.AUDIO_SCENARIO_GAME_STREAMING);

        rtcEngine.EnableInEarMonitoring(false);

        rtcEngine.SetInEarMonitoringVolume(80);

        audioPlaybackDeviceManager = (AudioPlaybackDeviceManager)rtcEngine.GetAudioPlaybackDeviceManager();
        audioRecordingDeviceManager = (AudioRecordingDeviceManager)rtcEngine.GetAudioRecordingDeviceManager();
        videoDeviceManager = (VideoDeviceManager)rtcEngine.GetVideoDeviceManager();

    }

    private void onUserJoined(uint uid, int elapsed)
    {
        string userJoinedMessage = string.Format("onUserJoined callback uid {0} {1}", uid, elapsed);
        Debug.Log(userJoinedMessage);
        OnVideoUserJoined(uid, elapsed);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (rtcEngine != null)
            {
                StartSharingScreen();
            }
        }
    }

    void OnError(int error, string msg)
    {
        Debug.LogError("Error with Agora: " + msg);
    }

    void OnLeaveChannel(RtcStats stats)
    {
        inCall = false;
        Debug.Log("Left channel with duration " + stats.duration);
    }

    void OnJoinChannelSuccess(string channelName, uint uid, int elapsed)
    {
        Debug.Log("Joined channel " + channelName);

        int status = rtcEngine.EnableVideoObserver();
        //Debug.Log("EnableVideoObserver : " + status);

        status = rtcEngine.EnableVideo();
        //Debug.Log("EnableVideo : " + status);

        rtcEngine.EnableLocalVideo(false);
        //Debug.Log("EnableLocalVideo : " + status);

        OnVideoUserJoined(uid, elapsed);
        Hashtable hash = new Hashtable();
        hash.Add("agoraID", uid.ToString());
        PhotonNetwork.SetPlayerCustomProperties(hash);
        ListenTODeskotop();

        inCall = true;
    }

    public IRtcEngine GetRtcEngine()
    {
        return rtcEngine;
    }

    public void JoinCall()
    {
        GetAudioRecordingDeviceCount = -1;
        if (GetAudioRecordingDeviceCount > 1)
        {
            audioCallChannel = rtcEngine.CreateChannel(PhotonNetwork.CurrentRoom.Name + "AudioCall");
            screenSharingCallChannel = rtcEngine.CreateChannel(PhotonNetwork.CurrentRoom.Name + "AudioCall");

            ChannelMediaOptions channelMediaOptions = new ChannelMediaOptions();
            channelMediaOptions.autoSubscribeAudio = true;
            channelMediaOptions.autoSubscribeVideo = false;
            channelMediaOptions.publishLocalAudio = true;
            channelMediaOptions.publishLocalVideo = false;

            int status = audioCallChannel.JoinChannel("", "", (uint)RoomManager.Instance.MyPlayer.photonView.ViewID, channelMediaOptions);

            Debug.Log("audioCallChannel.JoinChannel : " + status);


            channelMediaOptions.autoSubscribeAudio = false;
            channelMediaOptions.autoSubscribeVideo = false;
            channelMediaOptions.publishLocalAudio = true;
            channelMediaOptions.publishLocalVideo = true;

            status = screenSharingCallChannel.JoinChannel("", "", (uint)RoomManager.Instance.MyPlayer.photonView.ViewID, channelMediaOptions);
            screenSharingCallChannel.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            Debug.Log("screenSharingCallChannel.JoinChannel : " + status);

        }
        else
        {
            int status = rtcEngine.JoinChannel(PhotonNetwork.CurrentRoom.Name, "", (uint)RoomManager.Instance.MyPlayer.photonView.ViewID);
        }

        TheAssigner.Instance.refreshAudioDevices.onClick.AddListener(RefreshMicrophone);
        //Debug.Log("JoinChannel : " + status);
    }

    public override void OnLeftRoom()
    {
        rtcEngine.LeaveChannel();

    }

    void OnDestroy()
    {
        audioPlaybackDeviceManager.ReleaseAAudioPlaybackDeviceManager();
        videoDeviceManager.ReleaseAVideoDeviceManager();
        audioRecordingDeviceManager.ReleaseAAudioRecordingDeviceManager();
        IRtcEngine.Destroy();
    }

    public void StartSharingScreen()
    {
        if (curruntScreensharingPlayer != null)
        {
            Debug.LogError("Someone else is playing, Aks them to turn of their sceen.");
        }

        RoomManager.Instance.MyPlayer.TellOhersToTurnOfTheTV();
        shareScreen = !shareScreen;
        //StopScreenCapture();
        if (shareScreen)
        {
            rtcEngine.SetChannelProfile(CHANNEL_PROFILE.CHANNEL_PROFILE_LIVE_BROADCASTING);
            rtcEngine.SetClientRole(CLIENT_ROLE_TYPE.CLIENT_ROLE_BROADCASTER);
            int status = rtcEngine.MuteLocalVideoStream(!true);
            Debug.Log("MuteLocalVideoStream : " + status);

            status = rtcEngine.EnableLocalVideo(true);
            Debug.Log("EnableLocalVideo : " + status);
            Rectangle rectangle = new Rectangle
            {
                height = Screen.currentResolution.height,
                width = Screen.currentResolution.width
            };
            VideoDimensions videoDimensions = new VideoDimensions
            {
                height = 720,
                width = 1280
            };
            ScreenCaptureParameters screenCaptureParameters = new ScreenCaptureParameters
            {
                //bitrate = 10,

                captureMouseCursor = true,
                //dimensions = videoDimensions,
                frameRate = 22
            };
            int code = 889;
            //windowID = foregroundWindowsHandle.ToInt32();
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            code = rtcEngine.StartScreenCaptureByScreenRect(rectangle, rectangle, screenCaptureParameters);
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                code = ShareDisplayScreen(screenCaptureParameters, default);
#endif

            Debug.Log("Scree sharing => " + code);
            if (code == 0)
            {

            }
            else
            {
                shareScreen = !shareScreen;
                Debug.LogError("Screen Sharing error => " + code);
            }
        }
        else
        {
            StopSharingScreen();
        }
    }

    public void StopSharingScreen()
    {
        RoomManager.Instance.MyPlayer.TellOhersIAMTurningOffTheTV();

        shareScreen = false;
        StopScreenCapture();
    }

    public void StopScreenCapture()
    {
        rtcEngine.EnableLocalVideo(false);
        int i = rtcEngine.StopScreenCapture();

        if (i == 0)
        {

        }
    }

    internal void OnVideoUserJoined(uint uid, int elapsed)
    {
        PlayerRCPCarrier Parent = null;

        for (int i = 0; i < RoomManager.Instance.ActivePlayerList.Count; i++)
        {
            if (RoomManager.Instance.ActivePlayerList[i].photonView.ViewID == uid)
            {
                Parent = RoomManager.Instance.ActivePlayerList[i];
                break;
            }
        }

        Debug.Log("onUserJoined: uid = " + uid + " elapsed = " + elapsed);
        // this is called in main thread

        Parent.MyTV = Instantiate(TheAssigner.Instance.TVParent, TheAssigner.Instance.TVParent.transform.parent);
        VideoSurface videoSurface = Parent.MyTV.AddComponent<VideoSurface>();

        Parent.MyTV.name = uid.ToString();
        if (!ReferenceEquals(videoSurface, null))
        {
            // configure videoSurface
            if (uid == RoomManager.Instance.MyPlayer.photonView.ViewID)
            {
                Debug.Log("Mah Player");
                videoSurface.SetForUser(0);
            }
            else
            {
                if (GetAudioRecordingDeviceCount > 1)
                {
                    videoSurface.SetForMultiChannelUser(screenSharingCallChannel.ChannelId(), uid);
                }
                else
                {
                    videoSurface.SetForUser(uid);
                }
            }
            videoSurface.EnableFilpTextureApply(true, false);
            videoSurface.SetEnable(true);
            videoSurface.SetVideoSurfaceType(AgoraVideoSurfaceType.Renderer);
        }
        Parent.MyTV.SetActive(false);
    }
    //--------------------------------------------------------------------------------------------
    bool GrabSceneAudio = true;
    //// OnAudioListenerRender
    virtual public void OnAudioFilterRead(float[] data, int channels)
    {
        if (GrabSceneAudio)
        {
            short[] intData = new short[data.Length];
            //converting in 2 steps : float[] to Int16[], //then Int16[] to Byte[]

            byte[] bytesData = new byte[data.Length * 2];
            //bytesData array is twice the size of
            //dataSource array because a float converted in Int16 is 2 bytes.

            var rescaleFactor = 32767; //to convert float to Int16

            for (int i = 0; i < data.Length; i++)
            {
                float sample = data[i];
                if (sample > 1)
                    sample = 1;
                else if (sample < -1)
                    sample = -1;

                intData[i] = (short)(sample * rescaleFactor);
                byte[] byteArr = new byte[2];
                byteArr = BitConverter.GetBytes(intData[i]);
                byteArr.CopyTo(bytesData, i * 2);
            }

            PushExternalAudioFrame(bytesData, channels);
        }
    }
    void ListenTODeskotop()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.clip = Microphone.Start("Microphone Array (Realtek High Definition Audio)", true, 10, 44100);
        audioSource.loop = true;
        while (!(Microphone.GetPosition(null) > 0))
        {

        }

        audioSource.Play();
        //StartCoroutine(CoAudioRender());
    }
    public AudioSource audioSource;
    public bool inCall;
    const int SAMPLE_RATE = 44100;
    /// FINALLY PUSH FRAME INTO STREAM
    // _externalAudioFrameBuffer.Length = samples * channels * bytesPerSample
    public IEnumerator CoAudioRender()
    {
        int channels = audioSource.clip.channels;
        float[] samples = new float[audioSource.clip.samples * channels];
        audioSource.clip.GetData(samples, 0);
        GrabSceneAudio = true;
        int SourceDataIndex = channels * audioSource.timeSamples;
        //int SourceDataIndex = channels * (audioSource.clip.samples - 120000);
        Debug.LogWarning("CoAudioRender started. Found audio samples = " +
            samples.Length + " channels = " + audioSource.clip.channels);

        while (audioSource != null && audioSource.isActiveAndEnabled && audioSource.isPlaying)
        {
            int readSamples = (int)(SAMPLE_RATE * Time.deltaTime); // SamplesRate * elapsedTime => number of samples to read
            int delta = channels * readSamples;
            float[] copySample = new float[delta];
            if (readSamples + SourceDataIndex / channels <= audioSource.clip.samples)
            {
                Array.Copy(samples, SourceDataIndex, copySample, 0, delta);
            }
            else // wrap
            {
                int cur2EndCnt = samples.Length - SourceDataIndex;
                int wrap2HeadCnt = delta - cur2EndCnt;
                Array.Copy(samples, SourceDataIndex, copySample, 0, cur2EndCnt);
                Array.Copy(samples, 0, copySample, cur2EndCnt, wrap2HeadCnt);
            }
            SourceDataIndex = (SourceDataIndex + delta) % samples.Length;

            OnAudioFilterRead(copySample, channels);
            yield return new WaitForEndOfFrame();
        }
        GrabSceneAudio = false;
        Debug.LogWarning("Done Audio Render coroutine...");
    }

    public void PushExternalAudioFrame(byte[] _externalAudioFrameBuffer, int channels)
    {
        AudioFrame _externalAudioFrame = new AudioFrame();

        int bytesPerSample = 2;

        _externalAudioFrame.type = AUDIO_FRAME_TYPE.FRAME_TYPE_PCM16;
        _externalAudioFrame.samples = _externalAudioFrameBuffer.Length / (channels * bytesPerSample);
        _externalAudioFrame.bytesPerSample = bytesPerSample;
        _externalAudioFrame.samplesPerSec = SAMPLE_RATE;
        _externalAudioFrame.channels = channels;
        _externalAudioFrame.buffer = _externalAudioFrameBuffer;

        if (rtcEngine != null)
        {
            int status = rtcEngine.PushAudioFrame(_externalAudioFrame);
            //  Debug.Log($"[{nameof(VoiceChatManager)}] PushAudioFrame :" + status);
        }
    }
    //--------------------------------------------------------------------------------------------
    private Dictionary<string, string> AudioDevices = new Dictionary<string, string>();
    private AgoraChannel screenSharingCallChannel;
    private AgoraChannel audioCallChannel;

    public void RefreshAudio()
    {
        audioPlaybackDeviceManager.ReleaseAAudioPlaybackDeviceManager();
        audioPlaybackDeviceManager.CreateAAudioPlaybackDeviceManager();

        Transform SpeakerButtonPrefebParent = TheAssigner.Instance.SpeakerButtonPrefeb.parent;

        for (int k = 1; k < SpeakerButtonPrefebParent.childCount; k++)
        {
            Destroy(SpeakerButtonPrefebParent.GetChild(k).gameObject);
        }

        AudioDevices.Clear();
        int GetAudioPlaybackDeviceCount = audioPlaybackDeviceManager.GetAudioPlaybackDeviceCount();
        if (GetAudioPlaybackDeviceCount >= 1)
        {
            SpeakerButtonPrefebParent.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            SpeakerButtonPrefebParent.GetChild(0).gameObject.SetActive(true);
        }

        for (int i = 0; i < GetAudioPlaybackDeviceCount; i++)
        {
            string devicename = string.Empty;
            string devicenid = string.Empty;
            audioPlaybackDeviceManager.GetAudioPlaybackDevice(i, ref devicename, ref devicenid);
            GameObject go = Instantiate(TheAssigner.Instance.SpeakerButtonPrefeb, SpeakerButtonPrefebParent).gameObject;
            go.GetComponentInChildren<Text>().text = devicename;
            go.GetComponent<Button>().onClick.AddListener(delegate
            {
                string deviceName = devicename;
                string devicenID = devicenid;
                Debug.Log(deviceName);
                int setA = audioPlaybackDeviceManager.SetAudioPlaybackDevice(devicenID);
                //SelectedAudioDeviceText.text = $"Selected : {deviceName}";
            });
            go.SetActive(true);
            AudioDevices.Add(devicenid, devicename);
            Debug.Log($"<color=green>Audio devicename  - {devicename} devicenid - {devicenid}</color>");
        }

        string currentSelectedDevice = string.Empty;
        audioPlaybackDeviceManager.GetCurrentPlaybackDevice(ref currentSelectedDevice);
        foreach (KeyValuePair<string, string> item in AudioDevices)
        {
            if (item.Key == currentSelectedDevice)
            {
                TheAssigner.Instance.SelectedAudioDeviceText.text = $"Selected : {item.Value}";
                break;
            }
        }
    }

    public void RefreshMicrophone()
    {
        audioRecordingDeviceManager.ReleaseAAudioRecordingDeviceManager();
        audioRecordingDeviceManager.CreateAAudioRecordingDeviceManager();

        Transform SpeakerButtonPrefebParent = TheAssigner.Instance.SpeakerButtonPrefeb.parent;

        for (int k = 1; k < SpeakerButtonPrefebParent.childCount; k++)
        {
            Destroy(SpeakerButtonPrefebParent.GetChild(k).gameObject);
        }

        AudioDevices.Clear();
        GetAudioRecordingDeviceCount = audioRecordingDeviceManager.GetAudioRecordingDeviceCount();
        if (GetAudioRecordingDeviceCount >= 1)
        {
            SpeakerButtonPrefebParent.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            SpeakerButtonPrefebParent.GetChild(0).gameObject.SetActive(true);
        }

        for (int i = 0; i < GetAudioRecordingDeviceCount; i++)
        {
            string devicename = string.Empty;
            string devicenid = string.Empty;
            audioRecordingDeviceManager.GetAudioRecordingDevice(i, ref devicename, ref devicenid);
            GameObject go = Instantiate(TheAssigner.Instance.SpeakerButtonPrefeb, SpeakerButtonPrefebParent).gameObject;
            go.GetComponentInChildren<Text>().text = devicename;
            go.GetComponent<Button>().onClick.AddListener(delegate
            {
                string deviceName = devicename;
                string devicenID = devicenid;
                Debug.Log(deviceName);
                int setA = audioRecordingDeviceManager.SetAudioRecordingDevice(devicenID);
                //SelectedAudioDeviceText.text = $"Selected : {deviceName}";
            });
            go.SetActive(true);
            AudioDevices.Add(devicenid, devicename);
            Debug.Log($"<color=green>Audio devicename  - {devicename} devicenid - {devicenid}</color>");
        }

        string currentSelectedDevice = string.Empty;
        audioRecordingDeviceManager.GetCurrentRecordingDevice(ref currentSelectedDevice);
        foreach (KeyValuePair<string, string> item in AudioDevices)
        {
            if (item.Key == currentSelectedDevice)
            {
                TheAssigner.Instance.SelectedAudioDeviceText.text = $"Selected : {item.Value}";
                break;
            }
        }
        JoinCall();
    }
}
