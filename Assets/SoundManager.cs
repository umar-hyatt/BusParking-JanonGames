using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public AudioSource click;
 public static SoundManager instance;
 
 void Awake()
 {
     if(instance==null)
     {
         instance=this;
         DontDestroyOnLoad(gameObject);
         click= GetComponent<AudioSource>();
     }
     else
     {
        DestroyImmediate(this);
     }
 }
 public void Click()
 {
     click.Play();
 }
    // Start is called before the first frame update
 
}
