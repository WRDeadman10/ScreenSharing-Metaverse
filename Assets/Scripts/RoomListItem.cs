using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
public class RoomListItem : MonoBehaviour
{
	[SerializeField] TMP_Text text;

	public RoomInfo info;
	public Image worldImg;

	public void SetUp(RoomInfo _info)
	{
		info = _info;
		byte index = (byte)_info.CustomProperties["RoomNumber"];
		string sceneName = System.IO.Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(index));
		//text.text = _info.Name + " - " + sceneName;
		text.text = _info.Name;

		worldImg.sprite = Launcher.Instance.worlThumbnails[(byte)_info.CustomProperties["RoomNumber"] - 0];
	}

	public void OnClick()
	{
		Launcher.Instance.JoinRoom(info);
	}
}