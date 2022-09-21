using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using WR.ScreenShare;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance;

    public PlayerRCPCarrier MyPlayer;
    public List<PlayerRCPCarrier> ActivePlayerList;

    void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    public override void OnEnable()
    {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex > 0) // We're in the game scene
        {
            MyPlayer = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity).GetComponent<PlayerRCPCarrier>();
            //VoiceChatManager.Instance.JoinCall();
            VoiceChatManager.Instance.RefreshMicrophone();

        }
    }
}