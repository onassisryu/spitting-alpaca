using UnityEngine;
using Photon.Pun;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerNameDisplay : MonoBehaviourPunCallbacks
{
    public GameObject nickNameCanvas;
    public TextMeshProUGUI nickNameText;

    private void Start()
    {
        if (photonView.IsMine)
        {
            nickNameCanvas.SetActive(false);
            return;
        }

        string playerNickName = photonView.Owner.NickName;
        nickNameText.text = playerNickName;

        nickNameCanvas.transform.SetParent(transform);
    }

    private void Update()
    {
        if (!photonView.IsMine)
        {
            nickNameCanvas.transform.position = transform.position + new Vector3(0, 1.5f, 0);

            nickNameCanvas.transform.rotation = Quaternion.LookRotation(nickNameCanvas.transform.position - Camera.main.transform.position);
        }
    }

    // use when change player nickname
    /*[PunRPC]
    public void SetNickName(string newNickName)
    {
        nickNameText.text = newNickName;
    }

    public void ChangeNickName(string newNickName)
    {
        photonView.RPC("SetNickName", RpcTarget.Others, newNickName);
    }*/
}
