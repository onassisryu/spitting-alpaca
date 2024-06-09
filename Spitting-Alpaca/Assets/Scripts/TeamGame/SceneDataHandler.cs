

using ExitGames.Client.Photon;
using Photon.Pun;

public class SceneDataHandler{
    private const string SceneDataKey = "SceneData";
    public void SaveSceneDataToRoomProperties(SceneData sceneData)
    {
        string serializedData = SerializationHelper.SerializeToString(sceneData);
        Hashtable roomProperties = new Hashtable();
        roomProperties[SceneDataKey] = serializedData;
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProperties);
    }

    // 룸 프로퍼티에서 SceneData를 로드
    public SceneData LoadSceneDataFromRoomProperties()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue(SceneDataKey, out object serializedData))
        {
            return SerializationHelper.DeserializeFromString<SceneData>(serializedData as string);
        }
        return null;
    }
}