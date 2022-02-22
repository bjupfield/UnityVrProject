using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class HandAccuracyTest : MonoBehaviour
{
    public SteamVR_Action_Skeleton Skelly = null;
    public bool on = false;
    float[] curlIndex = new float[5];
    // Start is called before the first frame update
    GameObject rightHand;
    public string[] handPosition = new string[2] {"NoValue", "NoValue"};
    float releaseTimer = 0;
    void handChoose(){
        curlIndex = Skelly.fingerCurls;
        // Debug.Log($"Thumb: {curlIndex[0]}|| Index: {curlIndex[1]}|| Middle: {curlIndex[2]}|| Ring: {curlIndex[3]}|| Pink: {curlIndex[4]}");
        float thumbJoySide = 0.65f;
        float thumbButtonSide = 0.76f;
        float indexWrapped = 0.25f;
        float m_rWrapped = .35f;
        if(curlIndex[0] >= thumbJoySide && curlIndex[0] <= thumbButtonSide && curlIndex[1] >= indexWrapped && curlIndex[2] >= m_rWrapped && curlIndex[3] <= m_rWrapped && curlIndex[4] <= m_rWrapped){
            // Debug.Log("SHOOT FIRE");
            fire(); 
        }
        else if(curlIndex[0] >= thumbButtonSide && curlIndex[1] <= indexWrapped && curlIndex[2] <= m_rWrapped && curlIndex[3] >= m_rWrapped && curlIndex[4] >= m_rWrapped){
            ice();
            // Debug.Log("Ice Active");
        }
        // else if(curlIndex[0] <= thumbJoySide && curlIndex[1] >= indexWrapped && curlIndex[2] <= m_rWrapped && curlIndex[3] <= m_rWrapped && curlIndex[4] >= m_rWrapped){
        //     lightning();
        // }
        else if(curlIndex[0] > thumbJoySide && curlIndex[1] >= indexWrapped && curlIndex[2] >= m_rWrapped && curlIndex[3] >= m_rWrapped && curlIndex[4] >= m_rWrapped){
            closedFist();
        }
        else if(curlIndex[0] < thumbJoySide && curlIndex[1] <= indexWrapped && curlIndex[2] <= m_rWrapped && curlIndex[3] <= m_rWrapped && curlIndex[4] <= m_rWrapped){
            openPalm();
        }
        else
        {
            notAny();
        }
    }
    void fire(){
        handPosition[0] = "Fire";
        handPosition[1] = "Fire";
        releaseTimer = 0;
    }
    void ice(){
        handPosition[0] = "Ice";
        handPosition[1] = "Ice";
        releaseTimer = 0;
    }
    void lightning(){
        handPosition[0] = "Lightning";
        handPosition[1] = "Lightning";
        releaseTimer = 0;
    }
    void closedFist(){
        switch(handPosition[0]){
            case "Fire":
                handPosition[1] = "ClosedFist";
                break;
            case "Ice":
                handPosition[1] = "ClosedFist";
                break;
            case "Lightning":
                handPosition[1] = "ClosedFist";
                break;
            case "NoValue":
                handPosition[1] = "NoValue";
                break;
        }
        releaseTimer = 0;
    }
    void openPalm(){
        switch(handPosition[0]){
            case "Fire":
                handPosition[1] = "OpenPalm";
                break;
            case "Ice":
                handPosition[1] = "OpenPalm";
                break;
            case "Lightning":
                handPosition[1] = "OpenPalm";
                break;
            case "NoValue":
                handPosition[1] = "NoValue";
                break;
        }
        releaseTimer = 0;
    }
    void notAny(){
        switch(handPosition[0]){
            case "Fire":
                if(releaseTimer <= 3) {
                    releaseTimer += Time.deltaTime;
                }
                else{
                    handPosition[0] = "NoValue";
                    handPosition[1] = "NoValue";
                    releaseTimer = 0; 
                }
                break;
            case "Ice":
                if(releaseTimer <= 3) {
                    releaseTimer += Time.deltaTime;
                }
                else{
                    handPosition[0] = "NoValue";
                    handPosition[1] = "NoValue";
                    releaseTimer = 0;
                }
                break;
            case "Lightning":
                if(releaseTimer <= 3) {
                    releaseTimer += Time.deltaTime;
                }
                else{
                    handPosition[0] = "NoValue";
                    handPosition[1] = "NoValue";
                    releaseTimer = 0;
                }
                break;
            case "NoValue":
                handPosition[1] = "NoValue";
                releaseTimer = 0;
                break;
        }
    }
    void Start()
    {
        rightHand = GameObject.Find("Controller (right)");
    }

    // Update is called once per frame
    void Update()
    {
        curlIndex = Skelly.fingerCurls;
        if(SteamVR_Actions.default_PressA.GetStateDown(SteamVR_Input_Sources.RightHand)){
            on = !on;
        }
        if(on){
            handChoose();
        }
        // Debug.Log("Position 1: " + handPosition[0] + "||| Position 2: " + handPosition[1]);
    }
}
