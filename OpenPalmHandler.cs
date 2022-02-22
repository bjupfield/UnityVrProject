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
    }
    public float testRequiredLength = 0;
    GameObject attached;
    Vector3 startPos;
    Vector3 currPos;
    float time = 0;
    public bool stopped = true;
    Vector3 palm;
    public Vector3 currMovement;
    public Vector3 testPointing;
    Vector3 prePos;
    void incrementTime(){
        time += Time.deltaTime;
    }
    void goingForward(){
    if(stopped){
        startPos = attached.transform.position;
        currPos = attached.transform.position;
        incrementTime();
        stopped = false;
    }  
    else{
        currPos= attached.transform.position;
        incrementTime();
    }
    }
    void assess(){
        if(testRequiredLength <= Mathf.Abs((currPos - startPos).magnitude)){
            Vector3 distance = currPos - startPos;
            float xz = Mathf.Sqrt(Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.z, 2));
            float y = distance.y;
            if(Mathf.Abs(y / Mathf.Abs(distance.magnitude)) > Mathf.Sin((13f/36f) * Mathf.PI)){
                if(y > 0){
                    Debug.Log("Lifting Hand Up");
                    Debug.Log(distance);
                }
                else{
                    Debug.Log("Decending Hand");
                    Debug.Log(distance);
                }
            }
            else{
                Debug.Log("Pushing hand on XZ Axis");
                Debug.Log(distance);
            }
        }
        startPos = new Vector3();
        currPos = new Vector3();
        time = 0;
        stopped = true;
    }
    void analyzeY(){
        Vector3 b =attached.transform.position;
        Debug.Log("Currpos: " + currPos);
        Debug.Log("Prepos: " +  prePos);
        Debug.Log("Previous Position: (" + prePos.x + ", " + prePos.y + ", " + prePos.z + ")");
        Debug.Log("Current Position: (" + b.x + ", " + b.y + ", " + b.z + ")");
    }
    // Start is called before the first frame update
    void Start()
    {
        attached = this.gameObject;
        testPointing = -attached.transform.right;
    }

    // Update is called once per frame
    void Update()
    {
        testPointing = -attached.transform.right;
        palm = -attached.transform.right.normalized;
        currMovement = (attached.transform.position - prePos).normalized;
        if(SteamVR_Actions.default_PressA.GetStateDown(SteamVR_Input_Sources.LeftHand)){
            analyzeY();
        }
        if(Mathf.Abs((palm + currMovement).magnitude) >= 1.7f){ //This checks if velocity is reasonably aligned with the direction the palm points
            goingForward();
        }
        else{
            if(!stopped){ //This means that the last frame had hand movement in the direction the palmm points
                assess();
            }
            else{
                ;
            }
        }
        prePos = attached.transform.position;
    }
}
