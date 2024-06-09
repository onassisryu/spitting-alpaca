using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class FadeInOut : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public bool fadeIn = false;
    public bool fadeOut = false;

    public float TimeToFade;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(fadeIn == true){
            if(canvasGroup.alpha < 1){
                canvasGroup.alpha += TimeToFade * Time.deltaTime;
                if(canvasGroup.alpha >= 1) {
                    fadeIn = false;
                
                }
            }
        }
        if(fadeOut == true){
            if(canvasGroup.alpha >= 0){
                canvasGroup.alpha -= TimeToFade * Time.deltaTime;
                if(canvasGroup.alpha == 0) {
                    fadeOut = false;
                
                }
            }
        }

    
    }
    public void FadeIn(){
        fadeIn = true;
    }
    public void FadeOut(){
        fadeOut = true;
    }
}
