using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ErosionComponent : MonoBehaviour
{


    [SerializeField] bool runErosion = false;

    private void OnValidate()
    {
        if (runErosion)
        {
            Erode();
        }
    }

    void Erode()
    {

    }
}
