using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TeamResultAlpacaNickname : MonoBehaviour
{
    public GameObject nickNameCanvas;
    public TextMeshProUGUI nickNameText;

    void Start()
    {
        nickNameText.text = "";
        nickNameCanvas.transform.SetParent(transform);
    }

    private void Update()
    {
        nickNameCanvas.transform.position = transform.position + new Vector3(0, 2f, 0);

        nickNameCanvas.transform.rotation = Quaternion.LookRotation(nickNameCanvas.transform.position - Camera.main.transform.position);
    }

    public void SetNickname(string nickName)
    {
        nickNameText.text = nickName;
    }
}
