using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wobble : MonoBehaviour
{
    public int framesPerWobble = 1;
    public float wobbleDisplacement = 0.001f;

    private Vector3 _center;
    private int framesSinceLastWobble;

    // Start is called before the first frame update
    void Start()
    {
        _center = transform.position;
        framesSinceLastWobble = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if(framesSinceLastWobble++ >= framesPerWobble)
        {
            transform.position = _center + new Vector3(UnityEngine.Random.value * wobbleDisplacement, UnityEngine.Random.value * wobbleDisplacement, UnityEngine.Random.value * wobbleDisplacement);
            framesSinceLastWobble = 0;
        }
    }
}
