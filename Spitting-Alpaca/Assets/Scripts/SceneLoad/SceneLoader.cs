using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public SceneData dataToPass; // 로드되는 씬으로 전달할 데이터

    public void LoadSceneAsyncWithData(string sceneName)
    {
        // 씬 비동기적으로 로드
        SceneManager.LoadSceneAsync(sceneName).completed += OnSceneLoaded;
    }

    private void OnSceneLoaded(AsyncOperation asyncOperation)
    {
        // 로드된 씬에서 데이터 전달
        var scene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
        var rootGameObjects = scene.GetRootGameObjects();
        foreach (var gameObject in rootGameObjects)
        {
            // var receivers = gameObject.GetComponentsInChildren<ISceneDataReceiver>(true);
            // foreach (var receiver in receivers)
            // {
            //     receiver.ReceiveData(dataToPass);
            // }
        }
    }
}