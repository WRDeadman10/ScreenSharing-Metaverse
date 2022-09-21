using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using WR.ScreenShare;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager Instance { get; private set; }

    public static Hashtable RoomProperties = new Hashtable
    {
        ["isPresenting"] = false.ToString(),
        ["UIDOfPresenter"] = "",
        ["RoomNumber"] = byte.MinValue
    };

    public PlayerRCPCarrier MyPlayer;
    public List<PlayerRCPCarrier> ActivePlayerList;

    void Awake()
    {
        if (Instance != null)
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

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);

    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        if (PhotonNetwork.CurrentRoom.CustomProperties["isPresenting"].ToString().ToLower() == "true")
        {
            int viewID = int.Parse(PhotonNetwork.CurrentRoom.CustomProperties["UIDOfPresenter"].ToString());
            StartCoroutine(enumerator());
            IEnumerator enumerator()
            {
                yield return new WaitUntil(() => ActivePlayerList.Count == PhotonNetwork.CurrentRoom.PlayerCount);
                for (int i = 0; i < ActivePlayerList.Count; i++)
                {
                    if (ActivePlayerList[i].photonView.ViewID == viewID)
                    {
                        ActivePlayerList[i].SetupScreenSharingParameters();
                        yield break;
                    }
                }
            }
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex > 0) // We're in the game scene
        {
            MyPlayer = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity).GetComponent<PlayerRCPCarrier>();
            //VoiceChatManager.Instance.JoinCall();
            //VoiceChatManager.Instance.RefreshMicrophone();

        }
    }
}