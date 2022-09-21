using Photon.Pun;

using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace WR.ScreenShare
{
    public class PlayerRCPCarrier : MonoBehaviour
    {
        public PhotonView photonView;

        public GameObject MyTV;
        private void Awake()
        {
            photonView = GetComponent<PhotonView>();
        }
        void Start()
        {
            RoomManager.Instance.ActivePlayerList.Add(this);
            name = photonView.ViewID + "";
        }

        public void TellOhersToTurnOfTheTV()
        {
            photonView.RPC(nameof(TellOhersToTurnOffTheTVRPC), RpcTarget.All, photonView.ViewID);
        }

        [PunRPC]
        private void TellOhersToTurnOffTheTVRPC(int viewID)
        {
            //Debug.LogError("RPC to TellOhersToTurnOffTheTVRPC " + name, gameObject);
            for (int i = 0; i < RoomManager.Instance.ActivePlayerList.Count; i++)
            {
                //Debug.LogError($"{RoomManager.Instance.ActivePlayerList[i].photonView.ViewID} != {viewID}", gameObject);
                if (RoomManager.Instance.ActivePlayerList[i].photonView.ViewID != viewID)
                {
                    //Debug.LogError($"Inside {RoomManager.Instance.ActivePlayerList[i].photonView.ViewID} != {viewID}", gameObject);

                    VoiceChatManager.Instance.StopScreenCapture();
                    RoomManager.Instance.ActivePlayerList[i].MyTV.SetActive(false);
                }
            }
            VoiceChatManager.Instance.curruntScreensharingPlayer = this;
            MyTV.SetActive(true);
            TheAssigner.Instance.TVParent.SetActive(false);
        }

        public void TellOhersIAMTurningOffTheTV()
        {
            photonView.RPC(nameof(TellOhersIAMTurningOffTheTVRPC), RpcTarget.All);
        }

        [PunRPC]
        private void TellOhersIAMTurningOffTheTVRPC()
        {
            //Debug.LogError("RPC to TellOhersIAMTurningOffTheTVRPC** " + name, gameObject);
            MyTV.SetActive(false);
            TheAssigner.Instance.TVParent.SetActive(true);
            VoiceChatManager.Instance.curruntScreensharingPlayer = null;
        }
    }
}