using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoolScript : MonoBehaviour
{
    public class timerr{
        public bool timerOn = false;
        public bool timerReset = false;
        public float time = 0;
        public void updateTimer(){
            time += Time.deltaTime;
        }
        public void resetTimer(){
            time = 0;
            timerOn = false;
            timerReset = false;
        }
    }
    public bool item = false;
    public timerr timer;
    public timerr deleteTimer = new timerr();
    public Vector3 velocity;
    public bool start = false;
    public Vector3 startPos = new Vector3();
    public bool destroyFireball = false;
    public float[] fireballRadius = {0.08f, 0.1f};
    public void adjustRadius(float amountToAdjust){
        fireballRadius[1] += amountToAdjust;
        fireballRadius[0] = fireballRadius[1] * .8f;
    }
    void Start()
    {
        timer = new timerr();
    }

    // Update is called once per frame
    void OnCollisionEnter(Collision collision){
        if(collision.collider.gameObject.name == "vr_glove_right_model_slim" && !item){ //just saying if player, adjust to be tags later
            GameObject body = GameObject.Find("vr_glove_right_model_slim"); 
            Vector3 amountToGo = body.transform.position - body.GetComponent<ParticleHandler>().handPreviousTransformPosition;
            gameObject.transform.position += amountToGo;
        }
        else if(collision.collider.gameObject.name != "vr_glove_right_model_slim" && item){
            Debug.Log(collision.collider.gameObject.name);
            destroyFireball = true;
        }
    }
    void Update()
    {
        if(timer.timerOn){
            timer.updateTimer();
        }
        if(timer.timerReset){
            timer.resetTimer();
        }
        if(deleteTimer.timerOn){
            deleteTimer.updateTimer();
        }
        if((deleteTimer.time >= 5 && item == false) || deleteTimer.time >= 200){
            destroyFireball = true;
        }
        gameObject.transform.localScale = new Vector3(fireballRadius[1], fireballRadius[1], fireballRadius[1]);
    }
}
