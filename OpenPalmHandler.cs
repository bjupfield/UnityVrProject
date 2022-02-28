using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class OpenPalmHandler : MonoBehaviour
{
    public class palmMovement
    {
        public Vector3 velocity;
        public string type;
        public Vector3 currPos;
        public Vector3 startPos;
        public Vector3 currMov;
        public bool fin = false;
    }
    public float testRequiredLength = 0;
    public GameObject leftHand;
    public GameObject rightHand;
    float leftTime = 0;
    float rightTime = 0;
    public bool stoppedLeft = true;
    public bool stoppedRight = true;
    Vector3 leftPalm;
    Vector3 rightPalm;
    public Vector3 leftCurrMovement;
    public Vector3 rightCurrMovement;
    public Vector3 testPointing;
    Vector3 leftPrePos;
    Vector3 rightPrePos;
    bool typeChoosen = false;
    bool wroteLeft = false;
    bool wroteRight = false;
    public bool leftOpen;
    public bool rightOpen;
    public bool openTogether;

    public palmMovement exportLeft = new palmMovement{
        velocity = new Vector3(0, 0, 0),
        type = "None"
    };
    public palmMovement exportRight = new palmMovement{
        velocity = new Vector3(0, 0, 0),
        type = "None"
    };
    void leftIncrementTime(){
        leftTime += Time.deltaTime;
    }
    void rightIncrementTime(){
        rightTime += Time.deltaTime;
    }
    void goingForward(palmMovement info, bool which){
    if((which ? stoppedLeft : stoppedRight)){
        info.startPos = which ? leftHand.transform.position : rightHand.transform.position;
        info.currPos = which ? leftHand.transform.position : rightHand.transform.position;
        if(which){
            stoppedLeft = false;
            leftIncrementTime();    
        }
        else{
            stoppedRight = false;
            rightIncrementTime();
        }
    }  
    else{
        info.currPos= which ? leftHand.transform.position : rightHand.transform.position;
        if(which){
            leftIncrementTime();
        }
        else{
            rightIncrementTime();
        }
        if(testRequiredLength <= Mathf.Abs((info.currPos - info.startPos).magnitude) && typeChoosen == false){
            Vector3 distance = info.currPos - info.startPos;
            float xz = Mathf.Sqrt(Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.z, 2));
            float y = distance.y;
            if(Mathf.Abs(y / Mathf.Abs(distance.magnitude)) > Mathf.Sin((13f/36f) * Mathf.PI)){
                if(y > 0){
                    Debug.Log("Lifting Hand Up");
                    Debug.Log(distance);
                    info.type = "Lifting";
                }
                else{
                    Debug.Log("Decending Hand");
                    Debug.Log(distance);
                    info.type = "Descending";
                }
            }
            else{
                Debug.Log("Pushing hand on XZ Axis");
                Debug.Log(distance);
                info.type = "XZ";
            }
            typeChoosen = true;
        }
        if(typeChoosen){
            switch(info.type){
                case "Lifting":
                    if(leftCurrMovement.y > 0){
                        info.currMov.y = leftCurrMovement.y;
                    }
                    break;
                case "Descending":
                    if(leftCurrMovement.y < 0){
                        info.currMov.y = leftCurrMovement.y;
                    }
                    break;
                case "XZ":
                    info.currMov = leftCurrMovement;
                    break;
            }
        }
    }
    }
    void check(palmMovement infoLeft, palmMovement infoRight){
        if(openTogether){
            if((wroteLeft && wroteRight) || !(!leftOpen || !rightOpen)){
                exportLeft.fin = true;
                exportRight.fin = true;
                //if either the hands arenet moving forward or one hand is turned inactive
            }
        }
        else{
            if(!wroteLeft || !leftOpen){
                exportLeft.fin = true;
                exportLeft.velocity = exportLeft.currMov / Time.deltaTime;
                //if lefthand is not moving or hand is inactive
            }
            if(!wroteRight || !rightOpen){
                exportRight.fin = true;
                exportRight.velocity = exportRight.currMov / Time.deltaTime;
                //if righthand is not moving or hand is inactive
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        testPointing = -leftHand.transform.right;
    }

    // Update is called once per frame
    void Update()
    {
        if(leftOpen){
            leftPalm = -leftHand.transform.right.normalized;
            leftCurrMovement = (leftHand.transform.position - leftPrePos).normalized;
            if(Mathf.Abs((leftPalm + leftCurrMovement).magnitude) >= 1.7f){ //This checks if velocity is reasonably aligned with the direction the palm points
                goingForward(exportLeft, true);
                wroteLeft = false;
            }
        }
        if(rightOpen){
            rightPalm = -rightHand.transform.right.normalized;
            rightCurrMovement = (rightHand.transform.position - rightPrePos).normalized;
            if(Mathf.Abs((rightPalm + rightCurrMovement).magnitude) >= 1.7f){
                goingForward(exportRight, false);
                wroteRight = false;
            }
        }
        if(wroteLeft || wroteRight){
            check(exportLeft, exportRight);
        }
        leftPrePos = leftHand.transform.position;
        rightPrePos = rightHand.transform.position;
        wroteLeft = true;
        wroteRight = true;
    }
}
