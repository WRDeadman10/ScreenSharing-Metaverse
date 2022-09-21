using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
public class WorldItem : MonoBehaviour
{
    // Start is called before the first frame update
    public TMP_Text worldName;
    public Image worldImg;
    public int worldIndex;
    void Start()
    {
        string sceneName = System.IO.Path.GetFileNameWithoutExtension(UnityEngine.SceneManagement.SceneUtility.GetScenePathByBuildIndex(worldIndex));
        //worldName.text = sceneName;
        worldImg.sprite = Launcher.Instance.worlThumbnails[worldIndex - 0];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
