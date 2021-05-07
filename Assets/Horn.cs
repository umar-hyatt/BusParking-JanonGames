using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Horn : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
{
   public AudioSource audioSource;
   
   public void OnPointerDown(PointerEventData eventdata)
    {
        audioSource.Play();
        Debug.Log("play");
    }

    public void OnPointerUp(PointerEventData eventdata)
    {

    }
}
