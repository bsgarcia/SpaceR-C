using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(Selectable))]
public class ButtonSelection : MonoBehaviour, IPointerEnterHandler, IDeselectHandler, IPointerExitHandler
{
    public void OnPointerEnter(PointerEventData eventData)
    { 
        GetComponent<Text>().color = Color.yellow;
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        GetComponent<Text>().color = Color.white;

    }

    public void OnDeselect(BaseEventData eventData)
    {
        this.GetComponent<Selectable>().OnPointerExit(null);
    }
}