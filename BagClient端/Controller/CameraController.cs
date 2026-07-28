using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (player==null)
        {
            player=GameObject.FindWithTag("Player");
        }

        if (player!=null&&gameObject.GetComponent<CinemachineVirtualCamera>().Follow==null&&gameObject.GetComponent<CinemachineVirtualCamera>().LookAt==null)
        {
            gameObject.GetComponent<CinemachineVirtualCamera>().Follow = player.transform;
            gameObject.GetComponent<CinemachineVirtualCamera>().LookAt = player.transform;

        }
    }
}
