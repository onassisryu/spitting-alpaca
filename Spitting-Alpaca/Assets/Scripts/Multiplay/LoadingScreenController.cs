using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingScreenController : MonoBehaviour
{
    public GameObject loadingCanvas;
    public float loadingDuration = 3.0f;

    private void Start()
    {
        StartCoroutine(HideLoadingScreenAfterDelay());
    }

    private IEnumerator HideLoadingScreenAfterDelay()
    {
        // Ensure the loading canvas is active at the start
        loadingCanvas.SetActive(true);

        // Wait for the specified duration
        yield return new WaitForSeconds(loadingDuration);

        // Hide the loading canvas
        loadingCanvas.SetActive(false);
    }
}
