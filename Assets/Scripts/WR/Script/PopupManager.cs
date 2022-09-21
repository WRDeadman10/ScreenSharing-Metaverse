using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using System.Security.Cryptography;

public enum PopupType
{
    OneButton,
    TwoButton,
    ToastMassage
};
public enum MessageType
{
    Info,
    Warning,
    Error
};
public class ButtonProperties
{
    public Action buttonAction_;
    public string buttonName;
}
public class PopupManager : MonoBehaviour
{

    public static PopupManager Instance { set; get; }

    [SerializeField] GameObject loadingPopup_;
    [SerializeField] GameObject popup_;
    [SerializeField] TextMeshProUGUI text_TextMeshProUGUI_;
    [SerializeField] Button leftButton_;
    [SerializeField] Button rightButton_;
    [SerializeField] Image typeMsgImage_;

    [SerializeField] Sprite infoSprite_;
    [SerializeField] Sprite warningSprite_;
    [SerializeField] Sprite errorSprite_;

    [SerializeField] float padding_;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void ShowLoading()
    {
        ClosePopup();
        loadingPopup_.SetActive(true);
    }

    public void HideLoading()
    {
        loadingPopup_.SetActive(false);
    }

    public void ShowPopup(string msg, MessageType messageType, PopupType popupType, ButtonProperties leftbuttonProperties = null, ButtonProperties rightbuttonProperties = null)
    {
        popup_.SetActive(true);
        HideLoading();
        if (msg.Length > 255)
        {
            msg = msg.Substring(0, 255) + "...";
            Debug.LogError("Message too long truncating");
        }

        text_TextMeshProUGUI_.text = msg;
        switch (popupType)
        {
            case PopupType.OneButton:
                OneButtonLogic();
                break;
            case PopupType.TwoButton:
                TwoButtonLogic(leftbuttonProperties, rightbuttonProperties);
                break;
            case PopupType.ToastMassage:
                ToastMassageLogic(msg);
                break;
        }

        switch (messageType)
        {
            case MessageType.Info:
                typeMsgImage_.sprite = infoSprite_;
                break;
            case MessageType.Warning:
                typeMsgImage_.sprite = warningSprite_;
                break;
            case MessageType.Error:
                typeMsgImage_.sprite = errorSprite_;
                break;
        }


        StartCoroutine(SetSize());
    }

    IEnumerator SetSize()
    {
        popup_.transform.GetChild(0).GetComponent<ContentSizeFitter>().enabled = false;
        yield return new WaitForSecondsRealtime(0.01f);
        popup_.transform.GetChild(0).GetComponent<ContentSizeFitter>().enabled = true;
        if (popup_.GetComponent<RectTransform>().sizeDelta.y + padding_ > Screen.height)
        {
            text_TextMeshProUGUI_.GetComponent<LayoutElement>().preferredHeight = 700;
        }
        else
        {
            Debug.Log("Reset LayoutGroup", text_TextMeshProUGUI_);
        }
    }

    private void OneButtonLogic()
    {
        rightButton_.gameObject.SetActive(false);
        leftButton_.GetComponentInChildren<TextMeshProUGUI>().text = "Okay";
        leftButton_.onClick.RemoveAllListeners();
        leftButton_.onClick.AddListener(ClosePopup);
    }

    private void TwoButtonLogic(ButtonProperties leftButtonCallback, ButtonProperties rightButtonCallback)
    {

        leftButton_.GetComponentInChildren<TextMeshProUGUI>().text = leftButtonCallback.buttonName;
        rightButton_.GetComponentInChildren<TextMeshProUGUI>().text = rightButtonCallback.buttonName;

        leftButton_.onClick.RemoveAllListeners();
        rightButton_.onClick.RemoveAllListeners();

        rightButton_.gameObject.SetActive(true);
        leftButton_.gameObject.SetActive(true);

        if (leftButtonCallback.buttonAction_ != null)
        {
            leftButton_.onClick.AddListener(leftButtonCallback.buttonAction_.Invoke);
        }

        if (rightButtonCallback.buttonAction_ != null)
        {
            rightButton_.onClick.AddListener(rightButtonCallback.buttonAction_.Invoke);
        }

        leftButton_.onClick.AddListener(ClosePopup);
        rightButton_.onClick.AddListener(ClosePopup);
    }

    public void ClosePopup()
    {
        popup_.SetActive(false);
    }

    private void ToastMassageLogic(string msg)
    {
        //			ToastManager.Instance.showToast(msg);
    }
}
