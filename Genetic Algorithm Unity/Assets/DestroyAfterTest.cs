using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyAfterTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Time.timeScale = 10;
        Destroy(this.gameObject,10.0f/ UnityEngine.Time.timeScale);
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
