using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector3 lastpos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float h=Input.GetAxis("Horizontal");
        float v=Input.GetAxis("Vertical");
        Vector3 move=new Vector3(h,0,v)*Time.deltaTime*10;
        transform.Translate(move,Space.World);
        if (move != Vector3.zero)
        {
            Quaternion q=Quaternion.LookRotation(move);
            transform.rotation=Quaternion.Slerp(transform.rotation,q,Time.deltaTime*10);
        }

        if (h!=0||v!=0)
        {
            gameObject.GetComponent<Animator>().SetBool("run",true);
        }
        else
        {
            gameObject.GetComponent<Animator>().SetBool("run",false);
        }
      
    }
}
