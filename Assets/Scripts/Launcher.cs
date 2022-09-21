using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Linq;
using ExitGames.Client.Photon;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class Launcher : MonoBehaviourPunCallbacks
{
	public static Launcher Instance;
	public TMP_InputField roomNameInputField;
	[SerializeField] TMP_Text errorText;
	[SerializeField] TMP_Text roomNameText;
	[SerializeField] Transform roomListContent;
	[SerializeField] GameObject roomListItemPrefab;
	[SerializeField] Transform playerListContent;
	[SerializeField] GameObject PlayerListItemPrefab;
	[SerializeField] GameObject startGameButton;

	public Sprite[] worlThumbnails;

	void Awake()
	{
		Instance = this;
	}

	void Start()
	{
		Debug.Log("Connecting to Master");
		PhotonNetwork.ConnectUsingSettings();
	}

	public override void OnConnectedToMaster()
	{
		Debug.Log("Connected to Master");
		PhotonNetwork.JoinLobby();
		PhotonNetwork.AutomaticallySyncScene = true;
	}

	public override void OnJoinedLobby()
	{
		MenuManager.Instance.OpenMenu("title");
		Debug.Log("Joined Lobby");
	}

	public void CreateRoom(int RoomNumber)
	{
		if (string.IsNullOrEmpty(roomNameInputField.text) || RoomNumber <= 0)
		{
			return;
		}

		RoomOptions roomOption = new RoomOptions();
		Hashtable CustomRoomProperties = new Hashtable();
		CustomRoomProperties["RoomNumber"] = (byte)RoomNumber;
		roomOption.CustomRoomProperties = CustomRoomProperties;
		roomOption.CustomRoomPropertiesForLobby = new string[] {
			"RoomNumber"
		};

		PhotonNetwork.CreateRoom(roomNameInputField.text, roomOption, TypedLobby.Default);
		MenuManager.Instance.OpenMenu("loading");
	}

	public void JoinRandom()
	{
		PhotonNetwork.JoinRandomRoom();
	}

	public override void OnJoinedRoom()
	{
		if (PhotonNetwork.IsMasterClient == false)
			MenuManager.Instance.OpenMenu("room");

		roomNameText.text = PhotonNetwork.CurrentRoom.Name + "  , Room_" + (byte)PhotonNetwork.CurrentRoom.CustomProperties["RoomNumber"];

		Player[] players = PhotonNetwork.PlayerList;

		foreach (Transform child in playerListContent)
		{
			Destroy(child.gameObject);
		}

		for (int i = 0; i < players.Count(); i++)
		{
			Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
		}

		startGameButton.SetActive(PhotonNetwork.IsMasterClient);

		if (PhotonNetwork.IsMasterClient)
		{
			StartGame();
		}
	}

	public override void OnMasterClientSwitched(Player newMasterClient)
	{
		startGameButton.SetActive(PhotonNetwork.IsMasterClient);
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		errorText.text = "The name is already taken, try another name. " + message;
		Debug.LogError("The name is already taken, try another name. " + message);
		MenuManager.Instance.OpenMenu("error");
	}

	public void StartGame()
	{
		//PhotonNetwork.LoadLevel(1);
		PhotonNetwork.LoadLevel((byte)PhotonNetwork.CurrentRoom.CustomProperties["RoomNumber"]);
	}

	public void LeaveRoom()
	{
		MenuManager.Instance.OpenMenu("title");
		//PhotonNetwork.LeaveRoom();
		//MenuManager.Instance.OpenMenu("loading");
	}

	public void JoinRoom(RoomInfo info)
	{
		PhotonNetwork.JoinRoom(info.Name);
		MenuManager.Instance.OpenMenu("loading");
	}

	public override void OnLeftRoom()
	{
		MenuManager.Instance.OpenMenu("title");
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		foreach(Transform trans in roomListContent)
		{
			Destroy(trans.gameObject);
		}

		for(int i = 0; i < roomList.Count; i++)
		{
			if(roomList[i].RemovedFromList)
				continue;
			Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
	}
}