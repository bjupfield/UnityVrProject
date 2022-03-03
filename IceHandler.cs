using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class IceHandler : MonoBehaviour
{
    MeshFinder instance = new MeshFinder();
    MeshFinder.TirangleApi binstance = new MeshFinder.TirangleApi();
    public GameObject rightHand;
    public GameObject leftHand = null;
    public GameObject head;
    public bool firstStarted = false; //false if left started true if right started
    public bool together = false;//false if not together, true if together
    public bool wasTogether = false;//true right after together is created
    public float wasTogetherTime = 0;
    public bool testJoin = false;//false if not started or past limit, true started and not past limit
    public float timeToJoin = 0;//timer for if the second hand will join the first testing with only .4 down there
    public bool leftDrawActive = false;
    public bool rightDrawActive = false;
    plane drawPlane;
    plane secondDrawPlane;
    Vector3[] objectToCreatePoints = new Vector3[0];
    Vector3[] secondObjectToCreatePoints = new Vector3[0];
    public string[] handPosition;
    string[] handPositionUpdateAfter;
    public string [] leftHandPosition;
    string [] leftHandPositionUpdateAfter;
    class Open{
        public bool openTogetherPossible;
        public bool openTogether;
        public float openTogetherTimer;
        public bool leftOpen;
        public bool rightOpen;
        public bool openFirst;
        public triangulateMesh initialMesh1;
        public triangulateMesh initialMesh2;
        public bool leftState2;
        public bool rightState2;
        public bool firstState2;
        public GameObject object1;
        public GameObject object2;
        public string object1Type;
        public string object2Type;
    }
    public bool leftOpen;
    public bool rightOpen;
    public bool firstAdjusting = false; //fasle if left true if right
    public bool adjustingTogether = false;
    bool firstInstatiating; //false if left started true if right started
    bool leftInstatiating;
    bool rightInstatiating;
    public OpenPalmHandler openPalmHandler;
    OpenPalmHandler.palmMovement leftOpenPalm;
    bool opentTogether = false;
    bool testBoolOpenTogether = false;
    float testOpenTogether = 0;
    OpenPalmHandler.palmMovement rightOpenPalm;
    triangulateMesh initialMesh1;
    triangulateMesh initialMesh2;
    GameObject firstChanging;
    GameObject secondChanging;
    bool gameObject1InUse;
    class plane{
        public float x;
        public float y;
        public float z;
        public float d;
    }
    class line{
        public float x1;
        public float x2;
        public float y1;
        public float y2;
        public float z1;
        public float z2;
    }
    public class triangulateMesh{
         public Mesh mesh;
         public int initialPointsLength;
    }
    Open openInfo;
    GameObject openCreationSwitch(triangulateMesh initialMesh, string type){
        GameObject currCreation = new GameObject();
        switch(type){
            case "XZ":
                currCreation.AddComponent<IceIcicle>();
                break;
            case "Lifting":
                triangulateMesh toMake = initialMesh;
                IceWall c = currCreation.AddComponent<IceWall>();
                toMake.mesh = c.wallFunction(initialMesh);
                c.wallMesh = toMake;
                break;
            case "Descending":
                currCreation.AddComponent<IceStallagite>();
                break;
        }
        return currCreation;
    }
    void passPalmMovementToObject(GameObject currObject, string type, OpenPalmHandler.palmMovement palm, bool drawer, Vector3 hand, Vector3 head, Vector3 looking){
        switch(type){
            case "XZ":
                IceIcicle comp = currObject.GetComponent<IceIcicle>();
                break;
            case "Lifting":
                IceWall comp1 = currObject.GetComponent<IceWall>();
                break;
            case "Descending":
                IceStallagite comp2 = currObject.GetComponent<IceStallagite>();
                break;
        }
    }
    void open(string[] updHndPos, bool drawer){
        OpenPalmHandler.palmMovement currPalm = drawer ? openPalmHandler.exportRight : openPalmHandler.exportLeft;
        if(openInfo.openTogetherPossible && drawer == openInfo.openFirst){
            if(openInfo.openTogetherTimer <= 5f){
                openInfo.openTogetherTimer += Time.deltaTime;
            }
            else{
                openInfo.openTogetherPossible = false;
            }
        }
        if(updHndPos[1] == "Ice"){
            if(together){
                openInfo.openTogetherPossible = true;
                together = false;
                openInfo.openFirst = drawer;
                int length = objectToCreatePoints.Length;
                int secondLength = secondObjectToCreatePoints.Length;
                Vector3[] completedPoints = new Vector3[length + secondLength];
                objectToCreatePoints.CopyTo(completedPoints, 0);
                Vector3 firstPointFirstArray = objectToCreatePoints[0];
                Vector3 firstPointSecondArray = secondObjectToCreatePoints[0];
                Vector3 lastPointFirstArray = objectToCreatePoints[length - 1];
                Vector3 lastPointSecondArray = secondObjectToCreatePoints[secondLength - 1];
                float firstCloser = (Mathf.Abs((firstPointSecondArray  - lastPointFirstArray).magnitude) - Mathf.Abs((firstPointSecondArray  - firstPointFirstArray).magnitude));
                float secondCloser = (Mathf.Abs((lastPointSecondArray - lastPointFirstArray).magnitude) - Mathf.Abs((lastPointSecondArray - firstPointFirstArray).magnitude));
                if(firstCloser > secondCloser){
                    Debug.Log("reversed");
                    System.Array.Reverse(secondObjectToCreatePoints);
                }
                else if(firstCloser == secondCloser){
                    if(Mathf.Abs((firstPointSecondArray  - lastPointFirstArray).magnitude) + Mathf.Abs((firstPointSecondArray  - firstPointFirstArray).magnitude) >= Mathf.Abs((lastPointSecondArray - lastPointFirstArray).magnitude) + Mathf.Abs((lastPointSecondArray - firstPointFirstArray).magnitude)){
                        Debug.Log("Equaled reversed");
                        System.Array.Reverse(secondObjectToCreatePoints);
                    }
                }
                secondObjectToCreatePoints.CopyTo(completedPoints, length);
                if(completedPoints.Length > 2 ){
                    initialMesh1 = new triangulateMesh();
                    initialMesh1.mesh = createSurface(completedPoints);
                    initialMesh1.initialPointsLength = initialMesh1.mesh.vertexCount;
                    objectToCreatePoints = new Vector3[0];
                    secondObjectToCreatePoints = new Vector3[0];
                }
                else{
                    initialMesh1 = null;
                }
            }
            else{
                if(openInfo.openTogetherPossible){//the other hand in together has initiated so just set them equal to together
                    openInfo.openTogether = true;
                    openInfo.openTogetherPossible = false;
                }
                else{
                    triangulateMesh whichone = new triangulateMesh();
                    if(drawer == firstStarted){
                        if(objectToCreatePoints.Length > 2){
                            whichone.mesh = createSurface(objectToCreatePoints);
                            whichone.initialPointsLength = whichone.mesh.vertexCount; //wait im not sure this works but it was this way when my brain got here soo, if it doesnt work simply switch whichone.initialpintslength assignment to befroe createsurface
                            objectToCreatePoints = new Vector3[0];
                        }
                        else{
                            whichone = null;
                        }
                    }
                    else{
                        if(secondObjectToCreatePoints.Length > 2){
                            whichone.mesh = createSurface(secondObjectToCreatePoints);
                            whichone.initialPointsLength = whichone.mesh.vertexCount;
                            secondObjectToCreatePoints = new Vector3[0];
                        }
                        else{
                            whichone = null;
                        }
                    }
                    if(drawer ? openInfo.leftOpen : openInfo.rightOpen){ //other hand is active
                        if(drawer == openInfo.openFirst){ //other hand is using initial mesh2
                            initialMesh1 = whichone;
                        }
                        else{
                            initialMesh2 = whichone;
                        }
                    }
                    else{//this is the first one activating but not together
                        initialMesh1 = whichone;
                        openInfo.openFirst = drawer;
                    }
                }
            }
            if(drawer){
                openInfo.rightOpen = true;
            }
            else{
                openInfo.leftOpen = true;
            }
        }
        else{
            if((drawer ? openInfo.rightOpen : openInfo.leftOpen)){//this checks if the object has been created or not
                string type = currPalm.type;
                if(type != "None"){
                    if(openInfo.openTogether){//means must make object1
                        openInfo.object1 = openCreationSwitch(initialMesh1, type);
                        openInfo.rightState2 = true;
                        openInfo.leftState2 = true;
                        openInfo.rightOpen = false;
                        openInfo.leftOpen = false;
                        openInfo.firstState2 = drawer;
                        openInfo.object1Type = type;
                    }
                    else{
                        triangulateMesh toUse;
                        if(drawer == openInfo.openFirst){
                            //make object from initial mesh1
                            toUse = openInfo.initialMesh1;
                            openInfo.initialMesh1 = null;
                        }
                        else{
                            //make object from initial mesh2
                            toUse = openInfo.initialMesh2;
                            openInfo.initialMesh2 = null;
                        }
                        GameObject creation = openCreationSwitch(toUse, type);
                        if((drawer ? openInfo.leftState2 : openInfo.rightState2)){//means one of the others is already in this mode.
                            if((drawer == openInfo.firstState2)){//means the other one is using object 2
                                openInfo.object1 = creation;
                                openInfo.object1Type = type;
                            }
                            else{//means the other one is using object 1
                                openInfo.object2 = creation;
                                openInfo.object2Type = type;
                            }
                        }
                        else{//means need to make using object 1
                            openInfo.object1 = creation;
                            openInfo.object1Type = type;
                            openInfo.firstState2 = drawer;
                        }
                        if(drawer){
                            openInfo.rightState2 = true;
                            openInfo.rightOpen = false;
                        }
                        else{
                            openInfo.leftState2 = true;
                            openInfo.leftOpen = false;
                        }
                    }
                }
            }
            else{//object has been created
                if(openInfo.openTogether){
                    //use object1
                    passPalmMovementToObject(openInfo.object1, openInfo.object1Type, currPalm, drawer, drawer ? rightHand.transform.position : leftHand.transform.position, head.transform.position, head.transform.forward);
                }
                else{
                    if((drawer ? openInfo.leftState2 : openInfo.rightState2)){//means other object is active
                        if((drawer == openInfo.firstState2)){//means the other one is using object 2
                             passPalmMovementToObject(openInfo.object1, openInfo.object1Type, currPalm, drawer, drawer ? rightHand.transform.position : leftHand.transform.position, head.transform.position, head.transform.forward);
                        }
                        else{
                             passPalmMovementToObject(openInfo.object2, openInfo.object1Type, currPalm, drawer, drawer ? rightHand.transform.position : leftHand.transform.position, head.transform.position, head.transform.forward);
                        }
                    }
                    else{//other object is not active
                         passPalmMovementToObject(openInfo.object1, openInfo.object1Type, currPalm, drawer, drawer ? rightHand.transform.position : leftHand.transform.position, head.transform.position, head.transform.forward);
                    }
                }
            }
        }
    }
    plane createPlane(Vector3 point, Vector3 vector){
        float d = -(-(point.x * vector.x) - (point.y * vector.y) - (point.z * vector.z));
        return new plane{x = vector.x, y = vector.y, z = vector.z, d = d};
    }
    Vector3 findIntersect(line line, plane plane){
        float t = (plane.d - (plane.x * line.x2 + plane.y * line.y2 + plane.z * line.z2)) / (plane.x * line.x1 + plane.y * line.y1 + plane.z * line.z1);
        Vector3 point = new Vector3(line.x2 + line.x1 * t, line.y2 + line.y1 * t, line.z2 + line.z1 * t);
        return point;
    }
    void drawStart(GameObject hand, bool which){
        if(testJoin){
            together = true; 
            testJoin = false;
            timeToJoin = 0; 
            // Debug.Log($"{(which ? "RightHand" : "Lefthand")} ativated together");
        }
        else{
            if(which ? leftDrawActive : rightDrawActive){ //this means its the second hand activated but not activated together
                Vector3 doubleReferenceMeansVariable = hand.transform.forward;
                Vector3 pointMeter = hand.transform.position + doubleReferenceMeansVariable;
                secondDrawPlane = createPlane(pointMeter, doubleReferenceMeansVariable);
                // Debug.Log($"{(which ? "RightHand" : "Lefthand")} did not activate together");
            }
            else{
                Vector3 doubleReferenceMeansVariable = hand.transform.forward;
                Vector3 pointMeter = hand.transform.position + doubleReferenceMeansVariable;
                firstStarted = which;
                testJoin = true;
                drawPlane = createPlane(pointMeter, doubleReferenceMeansVariable);
                // Debug.Log($"{(which ? "RightHand" : "Lefthand")} was first");
            }
        }
    }
    void drawActive(GameObject hander, bool drawer){
        Transform hand = hander.transform;
        line currDir = new line{x1 = hand.forward.x, x2 = hand.position.x, y1 = hand.forward.y, y2 = hand.position.y, z1 = hand.forward.z, z2 = hand.position.z};
        Vector3 point;
        if(firstStarted == drawer){
            point = findIntersect(currDir, drawPlane);
            if(((point - hand.position).normalized + hand.forward).magnitude >= 1.7f){//check if point and hand forward are in the same direction so that the point isnt behind the hand because currdir is a line not a segment
                System.Array.Resize<Vector3>(ref objectToCreatePoints, objectToCreatePoints.Length + 1);
                objectToCreatePoints[objectToCreatePoints.Length - 1] = point;
            }
        }
        else{
            if(together){
                point = findIntersect(currDir, drawPlane);
            }
            else{
                point = findIntersect(currDir, secondDrawPlane);
            }
            if(((point - hand.position).normalized + hand.forward).magnitude >= 1.7f){//check if point and hand forward are in the same direction so that the point isnt behind the hand because currdir is a line not a segment
                System.Array.Resize<Vector3>(ref secondObjectToCreatePoints, secondObjectToCreatePoints.Length + 1);
                secondObjectToCreatePoints[secondObjectToCreatePoints.Length - 1] = point;
            }
        }
    }
    Mesh createSurface(Vector3[] vects){
        List<Vector3> vectorList =  new List<Vector3>(vects);
        Vector3 firstPoint = vectorList[0];
        vectorList = vectorList.ConvertAll(new System.Converter<Vector3, Vector3>((point)=>{
            return point - firstPoint;
        }));
        Vector3 linea = vectorList[1] - vectorList[0];
        Vector3 lineb = vectorList[2] - vectorList[0];
        Vector3 cross = Vector3.Cross(linea, lineb);
        linea = linea.normalized;
        cross = cross.normalized;
        Vector3 cross2 = Vector3.Cross(linea, cross);
        Vector3 point1 = vectorList[0];
        Vector3 point2 = point1 + linea;
        Vector3 point3 = point1 + cross2;
        Vector3 point4 = point1 + cross;
        Matrix4x4 pointMatrix = new Matrix4x4(
            new Vector4(point1.x, point1.y, point1.z, 1),
            new Vector4(point2.x, point2.y, point2.z, 1),
            new Vector4(point3.x, point3.y, point3.z, 1),
            new Vector4(point4.x, point4.y, point4.z, 1));
        Matrix4x4 conversionMatrix =  new Matrix4x4(
            new Vector4(0, 0 ,0 ,1),
            new Vector4(1, 0, 0, 1),
            new Vector4(0, 1, 0, 1),
            new Vector4(0, 0, 1, 1));
        Matrix4x4 b = conversionMatrix * pointMatrix.inverse;
        List<Vector2> vectorLi = vectorList.ConvertAll<Vector2>(new System.Converter<Vector3, Vector2>((point)=>{
            Vector3 result; 
            result.x = b.m00 * point.x + b.m01 * point.y + b.m02 * point.z + b.m03; 
            result.y = b.m10 * point.x + b.m11 * point.y + b.m12 * point.z + b.m13; 
            float num = b.m30 * point.x + b.m31 * point.y + b.m32 * point.z + b.m33; 
            num = 1 / num;
            result.x *= num;
            result.y *= num;
            Vector2 xy = new Vector2(result.x, result.y);
            return xy;
        }));
        MeshFinder.PSLG myPoints = new MeshFinder.PSLG(vectorLi);
        MeshFinder.Polygon2D finished = binstance.Triangulate(myPoints);
        Mesh newMesh = new Mesh();
        List<Vector3> meshVertexs = new List<Vector2>(finished.vertices).ConvertAll<Vector3>(new System.Converter<Vector2, Vector3>((xy)=>(new Vector3(xy.x, xy.y, 0))));
        newMesh.vertices = meshVertexs.ToArray();
        newMesh.triangles = finished.triangles;
        return newMesh;
    }
    void instatiateTestMesh(Mesh mesh, GameObject hand){
        var myObject = new GameObject();
        myObject.AddComponent<MeshFilter>().mesh = mesh;
        myObject.AddComponent<MeshRenderer>();
        myObject.transform.position = hand.transform.position;
    }
    void switchForPosition(string[] hndPos, string[] updHndPos, GameObject hand, bool drawer, OpenPalmHandler.palmMovement palmMovement){
        if(hndPos[0] == "Ice"){
            switch(hndPos[1]){
                case "Ice":
                    // if(!wasTogether){ //test if the current hand is active
                    //     if(!(drawer ? rightDrawActive : leftDrawActive)){
                    //         drawStart(hand, drawer); //this creates the plane and sets which hand created it and so forth
                    //         if(drawer){
                    //             rightDrawActive = true;
                    //         }
                    //         else{
                    //             leftDrawActive = true;
                    //         }
                    //     }
                    //     drawActive(hand, drawer);
                    // }
                    // else{
                    //     Debug.Log("ha no going");
                    // }
                    // currently testing de note when done.
                    break;
                case "OpenPalm":
                    open(updHndPos, drawer);
                    break;
                case "ClosedFist":

                    break;
            }
        }
        else{
            if(updHndPos[1] == "OpenPalm"){
                rightHand.GetComponent<OpenPalmHandler>().enabled = false;
            }
        }
        if(drawer != firstStarted && (drawer ? leftDrawActive : rightDrawActive) && testJoin){
            if(timeToJoin <= 10f){
                timeToJoin += Time.deltaTime;
            }
            else{
                testJoin = false;
                timeToJoin = 0;
            }
        }
        if(wasTogether && drawer != firstStarted){
            if(wasTogetherTime <= 5f){
                wasTogetherTime += Time.deltaTime;
            }
            else{
                wasTogether = false;
                wasTogetherTime = 0f;
            }
        }
    }
    void Start()
    {
        handPosition = rightHand.GetComponent<HandAccuracyTest>().handPosition;
        leftHandPosition = leftHand.GetComponent<HandAccuracyTest>().handPosition;
        handPositionUpdateAfter = new string [2]{"", ""};
        leftHandPositionUpdateAfter = new string[2]{"", ""};
    }
    void Update()
    {
        handPosition = rightHand.GetComponent<HandAccuracyTest>().handPosition;
        leftHandPosition = leftHand.GetComponent<HandAccuracyTest>().handPosition;
        switchForPosition(handPosition, handPositionUpdateAfter, rightHand, true, rightOpenPalm); //for the right hand
        switchForPosition(leftHandPosition, leftHandPositionUpdateAfter, leftHand, false, leftOpenPalm); //for the left hand
        handPositionUpdateAfter[0] = handPosition[0];
        handPositionUpdateAfter[1] = handPosition[1];
        leftHandPositionUpdateAfter[0] = leftHandPosition[0];
        leftHandPositionUpdateAfter[1] = leftHandPosition[1];
    }
}
