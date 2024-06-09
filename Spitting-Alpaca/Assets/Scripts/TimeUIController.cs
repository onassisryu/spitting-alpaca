using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TimeUIController : MonoBehaviour
{

    Timer timer;
    TextMeshProUGUI textObj;

    // Start is called before the first frame update
    void Start()
    {
        timer = GetComponentInChildren<Timer>();

        Transform childTransform = transform.Find("Timer");
        if(childTransform!= null) {
            textObj =  childTransform.GetComponent<TextMeshProUGUI>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(textObj == null) return ;
        textObj.text = $"{Mathf.CeilToInt(timer.time)}";
    }
}
