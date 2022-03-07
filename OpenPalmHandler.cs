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
    public float testRequiredLength;
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
    bool rightTypeChoosen = false;
    bool leftTypeChoosen = false;
    bool wroteLeft = false;
    bool wroteRight = false;
    public bool leftOpen;
    public bool rightOpen;
    public bool openTogether;

    public palmMovement exportLeft;
    public palmMovement exportRight;
    void leftIncrementTime(){
        leftTime += Time.deltaTime;
    }
    void rightIncrementTime(){
        rightTime += Time.deltaTime;
    }
    void goingForward(palmMovement info, bool which){
    if((which ? stoppedLeft : stoppedRight)){
        info.startPos = which ? rightHand.transform.position : leftHand.transform.position;
        info.currPos = which ? rightHand.transform.position : leftHand.transform.position;
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
        info.currPos= which ? rightHand.transform.position : leftHand.transform.position;
        if(which){
            leftIncrementTime();
        }
        else{
            rightIncrementTime();
        }
        if(testRequiredLength <= Mathf.Abs((info.currPos - info.startPos).magnitude) && (which ? rightTypeChoosen == false : leftTypeChoosen == false)){
            Debug.Log($"{(which ? "Right" : "Left")} reached acceptable magnitude");
            Vector3 distance = info.currPos - info.startPos;
            float xz = Mathf.Sqrt(Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.z, 2));
            float y = distance.y;
            if(Mathf.Abs(y / Mathf.Abs(distance.magnitude)) > .7f){
                if(y > 0){
                    // Debug.Log("Lifting Hand Up");
                    // Debug.Log(distance);
                    // Debug.Log($"Time to Comlpetion: {(which ? rightTime : leftTime)}");
                    // Debug.Log($"Hand {(which ? "Right" : "Left")}");
                    info.type = "Lifting";
                }
                else{
                    // Debug.Log("Decending Hand");
                    // Debug.Log(distance);
                    // Debug.Log($"Time to Comlpetion: {(which ? rightTime : leftTime)}");
                    // Debug.Log($"Hand {(which ? "Right" : "Left")}");
                    info.type = "Descending";
                }
            }
            else{
                // Debug.Log("Pushing hand on XZ Axis");
                // Debug.Log(distance);
                // Debug.Log($"Time to Comlpetion: {(which ? rightTime : leftTime)}");
                // Debug.Log($"Hand {(which ? "Right" : "Left")}");
                info.type = "XZ";
            }
            if(which){
                rightTypeChoosen = true;
            }
            else{
                leftTypeChoosen = true;
            }
        }
        if(which ? rightTypeChoosen : leftTypeChoosen){
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
        // Debug.Log($"{(which ? "RightHand" : "LeftHand")} || CurrMove: {info.currMov} | CurrPos: {info.currPos} | StartPos: {info.startPos} | Fin: {info.fin} | Type: {info.type} | Velocity: {info.velocity} ||");
    }
    }
    void check(palmMovement infoLeft, palmMovement infoRight){
        if(openTogether){
            if(((wroteLeft && wroteRight) || !(!leftOpen || !rightOpen)) && (exportRight.type != "None" ||  exportLeft.type != "None")){
                exportLeft.fin = true;
                exportRight.fin = true;
                exportLeft.velocity = exportLeft.currMov / Time.deltaTime;
                exportRight.velocity = exportLeft.currMov / Time.deltaTime;
                //if either the hands arenet moving forward or one hand is turned inactive
                Debug.Log("setting fin true for both");
            }
            if(wroteLeft && wroteRight){
                exportLeft = new palmMovement();
                exportRight = new palmMovement();
                leftTypeChoosen = false;
                rightTypeChoosen = false;
            }
        }
        else{
            if((wroteLeft || !leftOpen) && exportLeft.type != "None"){
                exportLeft.fin = true;
                exportLeft.velocity = (exportLeft.currPos - exportLeft.startPos) / Time.deltaTime;
                //if lefthand is not moving or hand is inactive
                Debug.Log("setting fin true for left");
                Debug.Log($"LeftHand || CurrMove: {exportLeft.currMov} | CurrPos: {exportLeft.currPos} | StartPos: {exportLeft.startPos} | Fin: {exportLeft.fin} | Type: {exportLeft.type} | Velocity: {exportLeft.velocity} ||");
            }
            if((wroteRight || !rightOpen) && exportRight.type != "None"){
                exportRight.fin = true;
                exportRight.velocity =  (exportRight.currPos - exportRight.startPos) / Time.deltaTime;
                Debug.Log("setting fin true for right");
                Debug.Log($"RightHand || CurrMove: {exportRight.currMov} | CurrPos: {exportRight.currPos} | StartPos: {exportRight.startPos} | Fin: {exportRight.fin} | Type: {exportRight.type} | Velocity: {exportRight.velocity} ||");
                //if righthand is not moving or hand is inactive
            }
            if(wroteLeft && exportLeft.type == "None"){
                exportLeft = new palmMovement(){
                    fin = false,
                    startPos = leftHand.transform.position,
                    type = "None"
                };
                leftTypeChoosen = false;
                Debug.Log("reset left");
            }
            if(wroteRight && exportRight.type == "None"){
                exportRight = new palmMovement(){
                    fin = false,
                    startPos = rightHand.transform.position,
                    type = "None"
                };
                rightTypeChoosen = false;
                Debug.Log("reset right");
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        testPointing = -leftHand.transform.right;
        leftOpen = this.gameObject.GetComponent<IceHandler>().leftPalmOpen;
        rightOpen = this.gameObject.GetComponent<IceHandler>().rightPalmOpen;
        exportRight = new palmMovement(){
            startPos = rightHand.transform.position,
            fin = false,
            type = "None"
        };
        exportLeft = new palmMovement(){
            startPos = leftHand.transform.position,
            fin = false,
            type = "None"
        };
    }

    // Update is called once per frame
    void Update()
    {
        leftOpen = this.gameObject.GetComponent<IceHandler>().leftPalmOpen;
        rightOpen = this.gameObject.GetComponent<IceHandler>().rightPalmOpen;
        if(exportLeft.fin){
            Debug.Log("Reinitiation occurs");
            exportLeft = new palmMovement(){
                startPos = leftHand.transform.position,
                fin = false,
                type = "None"
            };
            leftTypeChoosen = false;
        }
        if(exportRight.fin){
            Debug.Log("Reinitiation occurs");
            exportRight = new palmMovement(){
                startPos = rightHand.transform.position,
                fin = false,
                type = "None"
            };
            rightTypeChoosen = false;
        }
        if(leftOpen){
            leftPalm = leftHand.transform.right.normalized;
            leftCurrMovement = (leftHand.transform.position - leftPrePos).normalized;
            if(Mathf.Abs((leftPalm + leftCurrMovement).magnitude) >= 1.85f){ //This checks if velocity is reasonably aligned with the direction the palm points
                goingForward(exportLeft, false);
                wroteLeft = false;
            }
        }
        if(rightOpen){
            rightPalm = -rightHand.transform.right.normalized;
            rightCurrMovement = (rightHand.transform.position - rightPrePos).normalized;
            if(Mathf.Abs((rightPalm + rightCurrMovement).magnitude) >= 1.7f){
                goingForward(exportRight, true);
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
