using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowCallRenderer : MonoBehaviour
{
    public Transform OtherPlayerPos;
    public RectTransform Indicator;
    public TMP_Text debugText;

    private Vector3 renderPosition;
    private bool behindCamera;
    private Vector2 distanceToMidpoint;

    void Start()
    {
        if (OtherPlayerPos == null)
        {
            Debug.LogWarning("You forgot to assign the OtherPlayer transform");
        }
    }

    void Update()
    {
        renderPosition = Camera.main.WorldToScreenPoint(OtherPlayerPos.position); //get worldPosition of other player into a screen space format
        
        //clamp it to the screen size so it's always visible
        renderPosition.x = Mathf.Clamp(renderPosition.x, 0, Screen.width); 
        renderPosition.y = Mathf.Clamp(renderPosition.y, 0, Screen.height);

        if (renderPosition.z < 0) //how do we display The Thing when its /behind/ the camera?
        {
            behindCamera = true;
          
            /*
            //maybe push it to the edges of the screen at all times depending on its placement (so you know which way to turn)
            if (renderPosition.x > (Screen.width / 2)) //if it's on the right side push it all the way right
            {
                renderPosition.x = Screen.width;
            } else
            {
                renderPosition.x = 0;
            }

            if (renderPosition.y > (Screen.height / 2)) //if it's on the upper half push it all the way up
            {
                renderPosition.y = Screen.height;
            } else //and vice versa
            {
                renderPosition.y = 0;
            }
            */

            //imperfect – if behind, it'll /always/ be jammed into one of four corners. not ideal? not super wrong either
            
            
            //next attempt: only push one side.
            distanceToMidpoint.Set(renderPosition.x - (Screen.width/2), renderPosition.y - (Screen.height/2));
            if (distanceToMidpoint.x > distanceToMidpoint.y) //if it's further away on the horizontal axis?? maybe?? then we push that side and just clamp y
            {
                if (renderPosition.x > (Screen.width / 2)) //if it's on the right side push it all the way right
                {
                    renderPosition.x = Screen.width;
                } 
                else
                {
                    renderPosition.x = 0;
                }
            }
           
        } 
        else { behindCamera = false; }
    
        
        
        Indicator.position = renderPosition; 

        debugText.text = $"{Indicator.position.x}, {Indicator.position.y}, {Indicator.position.z}" + $"\n behind camera: {behindCamera}" + $"\n dist {distanceToMidpoint.x}";

        
    }
}
