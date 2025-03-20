using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowCallRenderer : MonoBehaviour
{
    public Transform OtherPlayerPos;
    public RectTransform Indicator;
    public TMP_Text debugText;

    void Start()
    {
        if (OtherPlayerPos == null){
            Debug.LogWarning("You forgot to assign the OtherPlayer transform");
        }
    }

    void Update()
    {
        Indicator.position = Camera.main.WorldToScreenPoint(OtherPlayerPos.position);
        debugText.text = $"{Indicator.position.x}, {Indicator.position.y}, {Indicator.position.z}";
    }
}
