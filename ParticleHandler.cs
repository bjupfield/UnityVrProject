using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ParticleHandler : MonoBehaviour
{
    public SwordController maths;
    public string[] handPosition;
    public string[] handPositionUpdateAfter;
    public Vector3 handPreviousTransformPosition;
    float thrownAccel;
    int frameThrowCount;
    public Sprite mat;
    public GameObject attached;
    public float[] radius = new float[2];
    public float perSec;
    public float lifeSpan;
    public float[] speed = new float[2];
    public float[] anglespeed = new float[2];
    public float percPart;
    public float[] rotateSpeed = new float[2];
    public bool trueSystemStart = false;
    float[] systemStart = new float[1]{0f};
    public float waitTime;
    GameObject[] fireballs;
    class Particle {
        public Vector3 oPos;
        public Vector3 postion;
        public Vector3 objectStart;
        public Vector3 velocity;
        public Vector3 sideVel;
        public Vector3 circleVel;
        public GameObject self1;

        public float time;
        public void update(Sprite mat, SwordController maths, float addti, int index, int index2){
            if(self1.GetComponent<SpriteRenderer>() == null){self1.AddComponent<SpriteRenderer>();}
            self1.GetComponent<SpriteRenderer>().sprite = mat;
            time += addti;
            self1.name = $"{index2}Particle{index}";
        }
        public void physicsUpdate(SwordController maths, float t, float adjustmentrate){
            Vector3 wholeVel = (velocity + sideVel + circleVel) * t;
            postion += wholeVel;
            Transform tr = self1.transform;
            tr.localPosition = postion;
            if(velocity != new Vector3()){
                Vector3 z = velocity + sideVel;
                Vector3 lineStart = oPos - velocity;
                float multiplier = (z.x * (lineStart.x - oPos.x) + z.y * (lineStart.y - oPos.y) + z.z * (lineStart.z - oPos.z)) / (Mathf.Pow(z.x , 2) + Mathf.Pow(z.y, 2) + Mathf.Pow(z.z, 2));
                Vector3 closestPoint = new Vector3(lineStart.x + z.x * multiplier, lineStart.y + z.y * multiplier, lineStart.z + z.z * multiplier);
                Vector3 zdirect = closestPoint - oPos;
                Vector3 cross = Vector3.Cross(zdirect, z);
                Vector3 cross2 = Vector3.Cross(z, cross);
                tr.localRotation = maths.fromForward(cross2.normalized,  zdirect.normalized);
            }
            else
            {
                tr.localRotation = maths.fromForward((postion - velocity * time - oPos).normalized, wholeVel);
            }
            if(circleVel != new Vector3() || sideVel != new Vector3()){
                Vector3 radius = (postion - velocity * time - oPos);
                if(circleVel != new Vector3()){
                    circleVel += -radius.normalized * (Mathf.Pow(circleVel.magnitude, 2) / radius.magnitude) * t;
                }
                if(sideVel != new Vector3()){
                    sideVel = radius.normalized * Mathf.Abs(sideVel.magnitude);
                }
            }
        }
        public void destroyObject(){
            Destroy(self1);
        }
    }
    Particle[] partArr = new Particle[0];
    Particle[][] partArr2 = new Particle[][]{
        new Particle[0]
        }; 
    void particleCreator(bool fireball){
        float t = Time.deltaTime;
        int howmany = (int)Mathf.Floor(t * perSec);
        percPart += t * perSec - howmany;
        if(percPart >= 1){
            percPart = 0;
            ++howmany;
        }
        for(int i = 0; i < fireballs.Length; i++){
            particleCircleInstatiater(t, howmany, fireballs[i]);
        }
        if(!fireball){
            particleLinearInstatiater(t, howmany);
        }  
        for(int i = 0; i < partArr2.Length; i++){
            particleUpdater(t, partArr2[i], i);
        }
    }
    void particleCircleInstatiater(float t, int howmany, GameObject fireball){
        for(int i = howmany; i > 0; i--){
            Vector3 oPos = new Vector3(fireball.transform.position.x, fireball.transform.position.y, fireball.transform.position.z);
            Vector3 forVel = new Vector3(Random.Range(-1f,1f), Random.Range(-1f,1f), Random.Range(-1f,1f)).normalized * Random.Range(.8f, 1f); //not actuall velocity but simply the direction to figure out the circular velocity
            Vector3 pos = new Vector3() + forVel; //might need to create a seperate object to attach the particles to... attached might need to change
            Vector3 sideVel = maths.XDirectionFinder(forVel, Random.Range(0, Mathf.PI * 2)); //Again not real velocity but just to find circlevel... might need to change to a not random range may look better
            Vector3 circleVel = Vector3.Normalize(Vector3.Cross(sideVel, forVel)) * Random.Range(rotateSpeed[0], rotateSpeed[1]); //The only actual velocity
            int nameNum = int.Parse(fireball.name.Remove(0, 8));
            System.Array.Resize<Particle>(ref partArr2[nameNum], partArr2[nameNum].Length + 1);
            GameObject particle = new GameObject(name: $"{nameNum}Particle{partArr2[nameNum].Length-1}");
            particle.transform.SetParent(fireball.transform);
            partArr2[nameNum][partArr2[nameNum].Length-1] = new Particle{postion = pos, oPos = new Vector3(), velocity = new Vector3(), sideVel = new Vector3(), circleVel = circleVel, time = 0, self1 = particle};
        }
    }
    void particleLinearInstatiater(float t, int howmany){
        for(int i = howmany ;i > 0; i--){
            Vector3 oPos = new Vector3(attached.transform.position.x, attached.transform.position.y, attached.transform.position.z);
            float randomRadius = Random.Range(radius[0], radius[1]);
            Vector3 randomPos = ((new Vector3(Random.Range(-1f,1f), Random.Range(-1f,1f), Random.Range(-1f,1f)).normalized) * randomRadius) + oPos;
            float randomForward = Random.Range(speed[0], speed[1]);
            Vector3 forVel = attached.transform.forward * randomForward;
            float multiplier = (oPos.x - randomPos.x + oPos.y - randomPos.y + oPos.z - randomPos.z) / (Mathf.Pow(forVel.x , 2) + Mathf.Pow(forVel.y, 2) + Mathf.Pow(forVel.z, 2));
            Vector3 closestPoint = new Vector3(oPos.x + forVel.x * multiplier, oPos.y + forVel.y * multiplier, oPos.z + forVel.z * multiplier);
            Vector3 sideVel;
            float randomDepAngle = randomForward * (Random.Range(anglespeed[0], anglespeed[1]) / 100f); //anglespeed is a percentage of forward to deal with random issues
            if(randomRadius == 0){
                sideVel = maths.XDirectionFinder(forVel.normalized, Random.Range(0, Mathf.PI * 2)).normalized * randomDepAngle;
            }
            else{
                sideVel = (randomPos - closestPoint).normalized * randomDepAngle;
            }
            Vector3 circleVel = Vector3.Normalize(Vector3.Cross(sideVel, forVel)) * Random.Range(rotateSpeed[0], rotateSpeed[1]);
            if(sideVel != new Vector3(0,0,0)){
                circleVel = Vector3.Normalize(Vector3.Cross(sideVel, forVel)) * Random.Range(rotateSpeed[0], rotateSpeed[1]);
            }
            else{
                circleVel = Vector3.Normalize(Vector3.Cross((randomPos - closestPoint).normalized, forVel)) * Random.Range(rotateSpeed[0], rotateSpeed[1]);
            }
            if(randomRadius == 0 && anglespeed[1] == 0){
                circleVel = new Vector3(0, 0, 0);
                sideVel = new Vector3(0, 0, 0);
            }
            // Debug.Log("Before" + partArr2[partArr2.Length-1].Length);
            System.Array.Resize<Particle>(ref partArr2[partArr2.Length - 1], partArr2[partArr2.Length - 1].Length + 1);
            // Debug.Log("After" +partArr2[partArr2.Length-1].Length);
            partArr2[partArr2.Length - 1][partArr2[partArr2.Length - 1].Length-1] = new Particle{postion = randomPos, oPos = closestPoint, objectStart = attached.transform.position, velocity = forVel, sideVel = sideVel, circleVel = circleVel, time = 0, self1 = new GameObject(name: $"{partArr2.Length-1}Particle{partArr2[partArr2.Length - 1].Length-1}")};
        }
    }
    void particleUpdater(float t, Particle[] prlcs, int index){
        if(prlcs != null){
            for(int i = 0; i < prlcs.Length; i++){
                // Debug.Log("Particle[]: " + index + " || Particle " + i);
                prlcs[i].update(mat, maths, t, i, index);
                prlcs[i].physicsUpdate(maths, t, Mathf.PI / 2);
                }
            prlcs = System.Array.FindAll(prlcs, x => {
                    if(x.time >= lifeSpan){
                        x.destroyObject();
                        return false;
                    }
                    return true;
                    });
            GameObject possibleFireball = GameObject.Find($"Fireball{index}");
            bool destroyFireball = false;
            if(possibleFireball != null){
                if(possibleFireball.GetComponent<BoolScript>().destroyFireball == true){
                    destroyFireball = true;
                }
            }
            if((prlcs.Length == 0 && systemStart[index] > Mathf.Max(waitTime, 1 / (perSec / 2))) || destroyFireball){
                partArr2 = System.Array.FindAll(partArr2, x => {
                    if(x == partArr2[index]){
                        fireballs = System.Array.FindAll(fireballs, y => {
                            string fireballx = y.name; 
                            if(fireballx == $"Fireball{index}"){
                                Destroy(GameObject.Find($"Fireball{index}"));
                                return false;
                            }
                            return true;
                        });
                        for(int i = 0; i < fireballs.Length; i++){
                            int nameNum = int.Parse(fireballs[i].name.Remove(0, 8));
                            if(nameNum > index){
                                fireballs[i].name = $"Fireball{(nameNum-1)}";
                            }
                        }
                        systemStart = System.Array.FindAll(systemStart, x => {
                            if(x == systemStart[index]){
                                return false;
                            }
                            else
                            {
                                return true;
                            }
                        });
                        return false;
                    }
                    return true;
                });
            }
            else
            {
                partArr2[index] = prlcs;
            }
        }
    }
    void fireballHandler(GameObject fireball){
        BoolScript fireballValues = fireball.GetComponent<BoolScript>();
        if(!fireballValues.item){
            Vector3 distance = fireball.transform.position - attached.transform.position;
            float mag = Mathf.Abs(distance.magnitude);
            float dragRadius = (.5f + fireballValues.fireballRadius[1]);
            if(mag > dragRadius){
                Vector3 toTravel = (mag - dragRadius) * -distance.normalized;
                fireball.transform.position += toTravel;
            }
        }
        else{
            if(fireball.transform.parent != null){
                Vector3 currPos = attached.transform.position;
                Vector3 vel = (currPos - handPreviousTransformPosition) / Time.deltaTime;
                Vector3 directionHandPointing = -attached.transform.right.normalized;
                float time = fireballValues.timer.time;
                if(Mathf.Abs((directionHandPointing + vel.normalized).magnitude) >= 1.5f){
                    if(!fireballValues.start){
                        fireballValues.startPos = currPos;
                        fireballValues.start = true;
                    }
                    thrownAccel += (vel.magnitude *  Mathf.Min(Mathf.Pow(thrownAccel, 1/3), 1)) / 5;
                    fireballValues.timer.timerOn = true;
                    frameThrowCount = 0;
                }
                else{
                    fireballValues.timer.timerOn = false;
                    ++frameThrowCount;
                    if(frameThrowCount >= 2){
                        float distanceFromStart = (currPos - fireballValues.startPos).magnitude;
                        float totalSpeed = time * thrownAccel;
                        if(((time >= 2f && totalSpeed >= .2f)|| totalSpeed >= 2f) && distanceFromStart >= .5f){
                            fireball.transform.parent = null;//throw fireball
                            fireball.transform.position =  attached.transform.position + new Vector3(-0.2f * attached.transform.localScale.x, 0, .05f * attached.transform.localScale.z);
                            fireballValues.velocity = totalSpeed * directionHandPointing;
                            fireballValues.timer.timerReset = true;
                            thrownAccel = 0;
                        }
                        else{
                            fireballValues.timer.timerReset = true;
                            thrownAccel = 0;
                            fireballValues.start = false;
                        }
                        frameThrowCount = 0;
                    }
                }
            }
            else{
                fireball.transform.position += fireballValues.velocity * Time.deltaTime;
            }
        }
    }
    void extinguishSetter(){
        for(int i = 0; i < fireballs.Length; i++){
            fireballs[i].GetComponent<BoolScript>().deleteTimer.timerOn = true;
        }
    }
    void updateRadius(GameObject fireball){
        Vector3 handPos = attached.transform.position;
        Vector3 mvFmLsFrame = handPos - handPreviousTransformPosition;
        Vector3 fbToHand = (handPos - fireball.transform.position).normalized;
        float comb = Mathf.Abs((fbToHand + mvFmLsFrame).magnitude);
        if(comb >= .9f && comb <= 1.1f){
            float r = fireball.GetComponent<BoolScript>().fireballRadius[1];
            fireball.GetComponent<BoolScript>().adjustRadius((mvFmLsFrame.magnitude * Time.deltaTime) * ((80 * r + 30) / (Mathf.Pow(22 * r, 3)+ 32)));
        }
    }
    void Start()
    {
        attached = this.gameObject;
        maths = GameObject.Find("Sword").GetComponent<SwordController>();
        handPosition = GameObject.Find("vr_glove_right_model_slim").GetComponent<HandAccuracyTest>().handPosition;
        handPositionUpdateAfter = new string [2]{"", ""};
        handPreviousTransformPosition = new Vector3();
    }
    void Update()
    {
        handPosition = GameObject.Find("vr_glove_right_model_slim").GetComponent<HandAccuracyTest>().handPosition;
        if(handPosition[0] == "Fire"){
            if(trueSystemStart == false){
                fireballs = new GameObject[1];
                fireballs[fireballs.Length - 1] = new GameObject(name: $"Fireball{partArr2.Length-1}");
                fireballs[fireballs.Length - 1].AddComponent<Rigidbody>().isKinematic = false;
                fireballs[fireballs.Length - 1].GetComponent<Rigidbody>().useGravity = false;
                fireballs[fireballs.Length - 1].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                fireballs[fireballs.Length - 1].AddComponent<SphereCollider>().radius = 1;
                fireballs[fireballs.Length - 1].AddComponent<BoolScript>();
            }
            trueSystemStart = true;
            switch(handPosition[1]){
                case "NoValue":
                    if(handPositionUpdateAfter[1] != "NoValue"){
                        System.Array.Resize<Particle[]>(ref partArr2, partArr2.Length + 1);
                        partArr2[partArr2.Length - 1] = new Particle[0];
                        System.Array.Resize<float>(ref systemStart, systemStart.Length + 1);
                        systemStart[systemStart.Length - 1] = -2f;
                        System.Array.Resize<GameObject>(ref fireballs, fireballs.Length + 1);
                        fireballs[fireballs.Length - 1] = new GameObject(name: $"Fireball{partArr2.Length - 1}");
                        fireballs[fireballs.Length - 1].AddComponent<Rigidbody>().isKinematic = false;
                        fireballs[fireballs.Length - 1].GetComponent<Rigidbody>().useGravity = false;
                        fireballs[fireballs.Length - 1].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                        fireballs[fireballs.Length - 1].AddComponent<SphereCollider>().radius = 1;
                        fireballs[fireballs.Length - 1].AddComponent<BoolScript>();
                    }
                    particleCreator(true);
                    updateRadius(fireballs[fireballs.Length-1]);
                    break;
                case "OpenPalm":
                    if(handPositionUpdateAfter[1] == "NoValue"){
                        GameObject toShoot = fireballs[partArr2.Length - 1];
                        toShoot.transform.SetParent(attached.transform);
                        toShoot.transform.localPosition =  new Vector3(-0.5f, 0, 0.02f); //add adding radius of fireball to the distance x
                        toShoot.GetComponent<BoolScript>().item = true;
                        extinguishSetter();
                    }
                    particleCreator(true);
                    break;
                case "ClosedFist":
                    if(handPositionUpdateAfter[1] != "ClosedFist"){
                        System.Array.Resize<Particle[]>(ref partArr2, partArr2.Length + 1);
                        partArr2[partArr2.Length - 1] = new Particle[0];
                        System.Array.Resize<float>(ref systemStart, systemStart.Length + 1);
                        systemStart[systemStart.Length - 1] = 0;
                        extinguishSetter();
                    }
                    particleCreator(false);
                    break;
            }
            if(trueSystemStart){
                for(int i = 0; i < systemStart.Length; i++){
                    systemStart[i] += Time.deltaTime;
                }
                for(int i = 0; i < fireballs.Length; i++){
                fireballHandler(fireballs[i]);
                }
            }
        }
        else{
            for(int i = 0; i < partArr2.Length; i++){
                particleUpdater(Time.deltaTime, partArr2[i], i);
            }
            if(trueSystemStart){
                for(int i = 0; i < systemStart.Length; i++){
                    systemStart[i] += Time.deltaTime;
                }
                for(int i = 0; i < fireballs.Length; i++){
                fireballHandler(fireballs[i]);
                }
            }
            if(handPositionUpdateAfter[0] == "Fire"){
                extinguishSetter();
            }
        }
        handPositionUpdateAfter[0] = handPosition[0];
        handPositionUpdateAfter[1] = handPosition[1];
        handPreviousTransformPosition = attached.transform.position * 1;
    }
}