using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class ServerNameDisplay : MonoBehaviour
{
	[SerializeField] PhotonView playerPV;
	[SerializeField] TMP_Text text;

	void Start()
	{
		if (!playerPV.IsMine)
		{
			gameObject.SetActive(false);
		}

		text.text ="Server: " + PhotonNetwork.CurrentRoom.Name;
	}
}
