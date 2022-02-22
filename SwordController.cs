using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SwordController : MonoBehaviour
{
    public GameObject headpiece;
    public GameObject righthand;
    GameObject lefthand;
    GameObject sword;
    Vector3 prerhpos;
    Vector3 prelhpos;
    public Vector3 velocity;
    public Quaternion angularvelocity;
    public bool statedownright = false;
    public bool statedownleft = false;
    public Vector3 handmovementvec;
    float n = 0;
    Vector3 lhch = new Vector3();
    Vector3 lHMove = new Vector3();
    float kFricMult = .08f;
    float sFricMult = .05f;
    Vector3 pointer = new Vector3();
    // Start is called before the first frame update
    void Start()
    {
        headpiece = GameObject.Find("Camera");
        righthand = GameObject.Find("Controller (right)");
        lefthand = GameObject.Find("Controller (left)");
        sword = GameObject.Find("TransferFile");
    }

    // Update is called once per frame
    public Vector3 flip(Vector3 toFlip){
        float y;
        if(toFlip.y < 0){
            y = Mathf.Abs(toFlip.y) - 1;
        }
        else{
            y = toFlip.y - 1;
        }
        Vector3 nowFlipped = new Vector3(toFlip.x * Mathf.Sqrt(1 - Mathf.Pow(y, 2)) , y,toFlip.z * Mathf.Sqrt(1 - Mathf.Pow(y, 2)));
        return nowFlipped;
    }
    float degreetorad(float euler){
            return (euler * Mathf.PI) / 180;
    }
    Vector3 adjustheadangl(GameObject head, Vector3 hand){
        float heady = head.transform.localEulerAngles.y;
        Vector2 x = new Vector2(Mathf.Cos(degreetorad(heady)),Mathf.Sin(degreetorad(heady))) * hand.x;
        Vector2 z = new Vector2(-Mathf.Sin(degreetorad(heady)), Mathf.Cos(degreetorad(heady))) * hand.z;
        Vector3 retvect = new Vector3(x.x + z.x,hand.y ,x.y + z.y);
        return retvect;
    }
    public Vector3 XDirectionFinder(Vector3 ford, float adjust){
        ford = ford.normalized;
        float x = ((ford.y * ford.x * Mathf.Sin(adjust)) + (Mathf.Cos(adjust) * Mathf.Sqrt(1-Mathf.Pow(ford.x, 2))));
        float y = Mathf.Sin(adjust) * Mathf.Sqrt(1-Mathf.Pow(ford.y, 2));
        float z = ((ford.y * ford.z * Mathf.Sin(adjust)) + (Mathf.Cos(adjust) * Mathf.Sqrt(1-Mathf.Pow(ford.z, 2))));
        return new Vector3(x, y, z);
    }
    public float reverseXFinder(Vector3 ford, Vector3 pos){
        Vector3 xord = XDirectionFinder(ford, 0);
        Vector3 yord = Vector3.Normalize(Vector3.Cross(xord, ford));
        xord = xord * pos.x;
        yord = yord * pos.y;
        ford = ford * pos.z;
        pos = xord + yord + ford;
        float toReturn = Mathf.Atan(pos.x/pos.y);
        return toReturn;
    }
    public Quaternion fromForward(Vector3 ford, Vector3 xDirect)
    {
        ford.Normalize();
        Vector3 yDirection = Vector3.Normalize(Vector3.Cross(xDirect, ford));
        Vector3 whatDirection = Vector3.Cross(ford, yDirection);
        float vectorSum = (yDirection.x + whatDirection.y) + ford.z;
        Quaternion quaternion = new Quaternion();
        if (vectorSum > 0f)
        {
            float num = (float)Mathf.Sqrt(vectorSum + 1f);
            quaternion.w = num * 0.5f;
            num = 0.5f / num;
            quaternion.x = (whatDirection.z - ford.y) * num;
            quaternion.y = (ford.x - yDirection.z) * num;
            quaternion.z = (yDirection.y - whatDirection.x) * num;
            return quaternion;
        }
        if ((yDirection.x >= whatDirection.y) && (yDirection.x >= ford.z))
        {
            float num7 = (float)Mathf.Sqrt(((1f + yDirection.x) - whatDirection.y) - ford.z);
            float num4 = 0.5f / num7;
            quaternion.x = 0.5f * num7;
            quaternion.y = (yDirection.y + whatDirection.x) * num4;
            quaternion.z = (yDirection.z + ford.x) * num4;
            quaternion.w = (whatDirection.z - ford.y) * num4;
            return quaternion;
        }
        if (whatDirection.y > ford.z)
        {
            float num6 = (float)Mathf.Sqrt(((1f + whatDirection.y) - yDirection.x) - ford.z);
            float num3 = 0.5f / num6;
            quaternion.x = (whatDirection.x+ yDirection.y) * num3;
            quaternion.y = 0.5f * num6;
            quaternion.z = (ford.y + whatDirection.z) * num3;
            quaternion.w = (ford.x - yDirection.z) * num3;
            return quaternion; 
        }
        float num5 = (float)Mathf.Sqrt(((1f + ford.z) - yDirection.x) - whatDirection.y);
        float num2 = 0.5f / num5;
        quaternion.x = (ford.x + yDirection.z) * num2;
        quaternion.y = (ford.y + whatDirection.z) * num2;
        quaternion.z = 0.5f * num5;
        quaternion.w = (yDirection.y - whatDirection.x) * num2;
        return quaternion;
    }
    Vector3 adjustswrdangl(GameObject swrd, Vector3 hand){
        Vector3 swrdangref = new Vector3(degreetorad(swrd.transform.eulerAngles.x), degreetorad(swrd.transform.eulerAngles.x), degreetorad(swrd.transform.eulerAngles.x));
        Vector3 z = swrd.transform.forward;
        Vector3 x = new Vector3(Mathf.Cos(swrdangref.y + (-swrdangref.z * Mathf.Sin(swrdangref.x))),Mathf.Sin(swrdangref.z * Mathf.Cos(swrdangref.x)), -Mathf.Sin(swrdangref.y + (-swrdangref.z * Mathf.Sin(swrdangref.x))));
        Vector3 y = new Vector3(-Mathf.Sin((swrdangref.z * Mathf.Cos(swrdangref.y)) - Mathf.Sin(swrdangref.y - swrdangref.z)), Mathf.Cos(swrdangref.x) * Mathf.Cos(swrdangref.z), (Mathf.Sin(swrdangref.x * (Mathf.Cos(swrdangref.y) - Mathf.Cos(swrdangref.z)))) + (Mathf.Cos(swrdangref.x) * Mathf.Sin(swrdangref.z) * swrdangref.y));
        return (z *  hand.z) + (x * hand.x)  + (y * hand.y);
    }
    void send(Quaternion angVel, Vector3 movVel){
        if(movVel.magnitude > 0){
            sword.transform.position += movVel * Time.deltaTime;
        }
            sword.transform.rotation = angVel;
    }
    Vector3 staticFric(Vector3 vel, float fric){
        float mag = Mathf.Abs(vel.magnitude);
        if(mag <= fric){
            if(mag <= fric / 10){
                return vel * .5f; 
            }
            else{
                return vel * .5f;
            }
        }
        return vel;
    }
    void Update()
    {
        // HERE IS WHAT YOU NEED TO DO
        // FIRST CHANGE THE VELOCITY change of the righthand movement to a more acceptable slow down (Line 169)
        // SECOND PERHAPS CHANGE THE MININUM CHANBGE IN DISTANCE FOR THE LEFTHAND MOVEMENT TO TRIGGER (Line 133-135)
        // THIRD IMPLEMENT THE X DIRECTION CHANGE SO THAT IT WILL ROTATE THE CORRECT WAY DEPENDING ON WHAT THE DIRECTION OF THE VELOCITY IS (DO THIS BY ADJUSTING IN FOUND ON LINE 144)
        // FOURTH IMPLEMENT THE RETURN TO PLAYER AUTOMATICALLY THING AND ALSO THE GRAB SWORD THING...
        if(statedownleft == true){
            lhch = lefthand.transform.position - prelhpos;
            prelhpos = lefthand.transform.position;
            lHMove += lhch;
            if(lhch.magnitude != 0){
            }
            else{
                Debug.Log("No Controller movement/ left");
            }
            lHMove -= lHMove * .2f;
        }
        if(Mathf.Abs(lHMove.magnitude) > 0){
            angularvelocity = fromForward(lHMove.normalized, XDirectionFinder(lhch, n));
        }
        if(statedownright == true)
        {
            Vector3 rhdist = righthand.transform.position - prerhpos;
            handmovementvec = rhdist.normalized;
            prerhpos = righthand.transform.position;
            if(rhdist.magnitude != 0){
                rhdist = adjustheadangl(headpiece, rhdist);
                rhdist = adjustswrdangl(sword, rhdist);
                velocity += rhdist / Time.deltaTime;
            }
            else{
                Debug.Log("No Controller movement/ right");
            }
        }
        if(SteamVR_Actions._default.Teleport.GetStateDown(SteamVR_Input_Sources.RightHand)){
            statedownright = !statedownright;
            prerhpos = righthand.transform.position;

        }
        if(SteamVR_Actions._default.Teleport.GetStateDown(SteamVR_Input_Sources.LeftHand)){
            statedownleft = !statedownleft;
            prelhpos = lefthand.transform.position;
        }
        velocity -= velocity * kFricMult;
        velocity = staticFric(velocity, sFricMult);
        send(angularvelocity, velocity);
    }
}
