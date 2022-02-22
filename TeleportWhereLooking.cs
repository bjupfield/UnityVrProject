using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class TeleportWhereLooking : MonoBehaviour
{
    // Start is called before the first frame update
    GameObject headpiece;
    void Start()
    {
        headpiece = GameObject.Find("Camera");
    }

    // Update is called once per frame
    void Update()
    {
        if(SteamVR_Actions._default.Teleport.GetStateDown(SteamVR_Input_Sources.Any)){
            Ray forwardfromhead = new Ray(headpiece.transform.position, headpiece.transform.forward);
            RaycastHit hit;
            if(Physics.Raycast(forwardfromhead, out hit)){
                Instantiate<GameObject>(GameObject.Find("Sphere"), hit.point, new Quaternion(0,0,0,0));
            }
        }
    }
}
