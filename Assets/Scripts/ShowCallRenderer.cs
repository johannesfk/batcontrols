using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class ShowCallRenderer : MonoBehaviour
{
    public Transform OtherPlayerPos;
    public RectTransform GUIIndicator;
    public TMP_Text debugText;
    public float pulseLength = 3;
    public float fadeSpeed = 1;

    private Vector3 rawScreenPosition;
    private Vector3 finalScreenPosition;

    void Start()
    {
        if (OtherPlayerPos == null)
        {
            Debug.LogWarning("You forgot to assign the OtherPlayer transform");
        }

        GUIIndicator.localScale = Vector3.zero;

    }

    void Update()
    {
        //COORDINATE UPDATING (constant)
        rawScreenPosition = Camera.main.WorldToScreenPoint(OtherPlayerPos.position); //get worldPosition of other player into a screen space format
        //Debug.Log(renderPosition);

        if (rawScreenPosition.z >= 0) //if in front of camera 
        {
          
            //simply clamp it to the screen size so it's always visible
            finalScreenPosition.x = Mathf.Clamp(rawScreenPosition.x, 0, Screen.width); 
            finalScreenPosition.y = Mathf.Clamp(rawScreenPosition.y, 0, Screen.height);
           
        } 
        else //if behind the camera: needs to always be clamped so it doesn't appear mirrored "in front"
        { 

            float xMidpoint = Screen.width / 2;
            if (rawScreenPosition.x > xMidpoint) //if behind on the left side
            {
                finalScreenPosition.x = 0; //clamp to left side of screen
            }
            else
            {
                finalScreenPosition.x = Screen.width; //clamp to right side of screen
            }


            float yMidpoint = Screen.height / 2;
            if (rawScreenPosition.y > yMidpoint) // if behind on lower half
            {
                finalScreenPosition.y = 0; //clamp to bottom of screen
            }
            else
            {
                finalScreenPosition.y = Screen.height; //clamp to top of screen
            }

        }

        GUIIndicator.position = finalScreenPosition; 


        //SHOWCALL INPUT (temporary for now)
        if (Input.GetKeyDown(KeyCode.T))
        {
            StartCoroutine(DisplayShowCallIndicator(pulseLength, fadeSpeed));
        }



        //DEBUG STUFF
        string debug = rawScreenPosition.ToString();
        debug += $"\n scale: {GUIIndicator.localScale.ToString()}";
        debug += $"\nelapsed: {elapsed}";
        debugText.text = debug;
        
    }


    float elapsed = 0f;

    IEnumerator DisplayShowCallIndicator (float pulseDuration, float fadeDuration) //coroutine to scale the GUI, keep it at 0 for normal
    {
        GUIIndicator.localScale = Vector3.one; //make indicator big & visible
        yield return new WaitForSeconds(pulseDuration);  //keep it visible for [seconds]

        //might add a fade-off here: 
        //Vector3 startScale = GUIIndicator.localScale;
        elapsed = 0f;

        while (elapsed < fadeDuration) 
        {
            float t = elapsed / fadeDuration;
            GUIIndicator.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            elapsed += Time.deltaTime;
            yield return null;
            
        }

        GUIIndicator.localScale = Vector3.zero;
    }
}

