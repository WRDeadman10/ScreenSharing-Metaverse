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

        //public Transform SpeakerButtonPrefeb;

        //public TMPro.TextMeshProUGUI SelectedAudioDeviceText;
        public TMPro.TextMeshPro CurrentScreenSharingPlayerNameText;

        public bool isInAreaToPresentScreen;
        //public Button refreshAudioDevices;

        private void Awake()
        {
            Instance = this;
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log(other.name,other.gameObject);
            Photon.Pun.PhotonView photonView = other.GetComponent<Photon.Pun.PhotonView>();
            if (photonView == null)
                return;
            if (photonView.IsMine)
            {
                isInAreaToPresentScreen = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            Debug.Log(other.name, other.gameObject);
            Photon.Pun.PhotonView photonView = other.GetComponent<Photon.Pun.PhotonView>();
            if (photonView == null)
                return;
            if (photonView.IsMine)
            {
                isInAreaToPresentScreen = false;
            }
        }
        //private void OnCollisionEnter(Collision collision)
        //{
        //    Photon.Pun.PhotonView photonView = collision.gameObject.GetComponent<Photon.Pun.PhotonView>();
        //    if (photonView.IsMine)
        //    {
        //        isInAreaToPresentScreen = true;
        //    }
        //}

        //private void OnCollisionExit(Collision collision)
        //{
        //    Photon.Pun.PhotonView photonView = collision.gameObject.GetComponent<Photon.Pun.PhotonView>();
        //    if (photonView.IsMine)
        //    {
        //        isInAreaToPresentScreen = false;
        //    }
        //}
    }
}