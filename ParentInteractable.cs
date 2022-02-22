using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ParentInteractable : MonoBehaviour
{
    GameObject keker;
    List<Collision> collisionlist = new List<Collision>();
    SteamVR_Input_Sources hand;
    public string ifcollided;
    public string ifinteractable;
    // Start is called before the first frame update
    void Start()
    {
        keker = gameObject;
        hand = gameObject.GetComponent<SteamVR_Behaviour_Pose>().inputSource;
    }

    // Update is called once per frame
    void Update()
    {
        if(SteamVR_Actions._default.GrabPinch.GetStateDown(hand)){
            Debug.Log("work");
            float lol = Mathf.NegativeInfinity;
            GameObject theone;
            theone = collisionlist[0].gameObject ? collisionlist[0].gameObject : null;
            for(int i = 0; i < collisionlist.Count; i ++){
                float m = collisionlist[i].contacts[0].point.magnitude - gameObject.transform.position.magnitude;
                if(m >= lol){
                    lol = m;
                    theone = collisionlist[i].gameObject;
                }
            }
            if(theone != null){
                theone.transform.SetParent(keker.transform);
            }
        }
    }
    void OnCollisionEnter(Collision collision){
        Debug.Log("Collision, hit" + collision.gameObject);
        ifcollided = "collision with " + collision.gameObject;
        if(collision.gameObject.GetComponent<Interactable>().istrue == true){
            Debug.Log("Object collided" + collision.gameObject + ", is interactable");
            ifinteractable = "interactable" + collision.gameObject;
        }
        else{
            Debug.Log("No interactable?!?!?");
            ifinteractable = "not interactable " + collision.gameObject;
        }
        collisionlist.Add(collision.gameObject.GetComponent<Interactable>().istrue ? collision : null);
        Debug.Log("Succesfully added to list: " + collisionlist[0].gameObject);
    }
    void OnCollisionExit(Collision collision){
        Debug.Log("collision exited, Collision object: " + collision.gameObject);
        collisionlist.Remove(collision.gameObject.GetComponent<Interactable>().istrue ? collision : null);
    }
}
