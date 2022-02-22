using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class IceHandler : MonoBehaviour
{
    MeshFinder instance = new MeshFinder();
    MeshFinder.TirangleApi binstance = new MeshFinder.TirangleApi();
    public GameObject rightHand;
    public GameObject leftHand = null;
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
    string[] handPosition;
    string[] handPositionUpdateAfter;
    string [] leftHandPosition;
    string [] leftHandPositionUpdateAfter;
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
    class triangulateMesh{
         public Mesh mesh;
         public int initialPointsLength;
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
        return walllFunction(new triangulateMesh{initialPointsLength = vectorLi.Count, mesh = newMesh});
    }
    void instatiateTestMesh(Mesh mesh, GameObject hand){
        var myObject = new GameObject();
        myObject.AddComponent<MeshFilter>().mesh = mesh;
        myObject.AddComponent<MeshRenderer>();
        myObject.transform.position = hand.transform.position;
    }
    Mesh walllFunction(triangulateMesh surface){
        List<Vector3> vectorList = new List<Vector3>(surface.mesh.vertices).ConvertAll<Vector3>(new System.Converter<Vector3, Vector3>((vector)=>(new Vector3(vector.x, 0, vector.y)))); //converts to a z by x surface instead of x and y
        int length = vectorList.Count;
        for(int b = 0; b < 10; ++b){
            for(int i = 0; i < surface.initialPointsLength; ++i){
                Vector3 outlineVector = vectorList[i];
                vectorList.Add(new Vector3(outlineVector.x, -b / 2, outlineVector.z));
            }
        } // adding new vertexes
        List<int> triangleList = new List<int>(surface.mesh.triangles);
        for(int curr = 0; curr < triangleList.Count; curr += 3){
            int reverse1 = triangleList[curr];
            int reverse2 = triangleList[curr + 2];
            triangleList[curr] = reverse2;
            triangleList[curr + 2] = reverse1;
        }
        for(int b = 0; b < 10; ++b){
            for(int i = 0; i < surface.initialPointsLength; ++i){
                int whichLayer = surface.initialPointsLength * b; // decides where the added vertexes from the loop above are
                int whereTop = (b > 0 ? length : 0) +  Mathf.Max(b - 1, 0) * surface.initialPointsLength; //decides where the vertexes above the added vertexes are
                if(i == surface.initialPointsLength - 1){
                    triangleList.AddRange(new int[]{length + i + whichLayer, i + whereTop, whereTop,});
                    triangleList.AddRange(new int[]{length + i + whichLayer, whereTop, length + whichLayer});
                }
                else{
                    triangleList.AddRange(new int[]{length + i + whichLayer, i + whereTop, i + 1 + whereTop,});
                    triangleList.AddRange(new int[]{length + i + whichLayer, i + 1 + whereTop, length + i + 1 + whichLayer});
                }
            }
        }
        surface.mesh.vertices = vectorList.ToArray();
        surface.mesh.triangles = triangleList.ToArray();
        return surface.mesh;
    }
    void switchForPosition(string[] hndPos, string[] updHndPos, GameObject hand, bool drawer){
        if(hndPos[0] == "Ice"){
            switch(hndPos[1]){
                case "Ice":
                    if(!wasTogether){ //test if the current hand is active
                        if(!(drawer ? rightDrawActive : leftDrawActive)){
                            drawStart(hand, drawer); //this creates the plane and sets which hand created it and so forth
                            if(drawer){
                                rightDrawActive = true;
                            }
                            else{
                                leftDrawActive = true;
                            }
                        }
                        drawActive(hand, drawer);
                    }
                    else{
                        Debug.Log("ha no going");
                    }
                    break;
                case "OpenPalm":
                    if(updHndPos[1] != "OpenPalm" && (drawer ? rightDrawActive : leftDrawActive)){
                        Debug.Log($"Checking if this is active after the summon");
                        if(together){
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
                                foreach(Vector3 z in completedPoints){
                                    GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                                    b.transform.localScale = new Vector3(.01f, .01f, .01f);
                                    b.transform.position = z;
                                }
                                Mesh newMesh = createSurface(completedPoints);
                                instatiateTestMesh(newMesh, hand);
                                objectToCreatePoints = new Vector3[0];
                                secondObjectToCreatePoints = new Vector3[0];
                                rightDrawActive = false;
                                leftDrawActive = false;
                                together = false;
                                testJoin = false;
                                timeToJoin = 0;
                                wasTogether = true;
                            }
                            else{
                                Debug.Log("below 3");
                            }
                        }
                        else{
                            if(drawer == firstStarted){
                                if(objectToCreatePoints.Length > 2){
                                    foreach(Vector3 z in objectToCreatePoints){
                                        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                                        b.transform.localScale = new Vector3(.01f, .01f, .01f);
                                        b.transform.position = z;
                                    }
                                    Debug.Log(objectToCreatePoints.Length);
                                    Mesh newMesh = createSurface(objectToCreatePoints);
                                    instatiateTestMesh(newMesh, hand);
                                    objectToCreatePoints = new Vector3[0];
                                    if(drawer){
                                        rightDrawActive = false;
                                    }
                                    else{
                                        leftDrawActive = false;
                                    }
                                    testJoin = false;
                                    timeToJoin = 0;
                                }
                                else{
                                    Debug.Log("below 3");
                                }
                            }
                            else{
                                if(secondObjectToCreatePoints.Length > 2){
                                    foreach(Vector3 z in secondObjectToCreatePoints){
                                        GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                                        b.transform.localScale = new Vector3(.01f, .01f, .01f);
                                        b.transform.position = z;
                                    }
                                    Mesh newMesh = createSurface(secondObjectToCreatePoints);
                                    instatiateTestMesh(newMesh, hand);
                                    secondObjectToCreatePoints = new Vector3[0];
                                    if(drawer){
                                        rightDrawActive = false;
                                    }
                                    else{
                                        leftDrawActive = false;
                                    }
                                    testJoin = false;
                                    timeToJoin = 0;
                                }
                                else{
                                    Debug.Log("below 3");
                                }
                            }
                        }
                        rightHand.GetComponent<OpenPalmHandler>().enabled = true;
                    }
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
        switchForPosition(handPosition, handPositionUpdateAfter, rightHand, true); //for the right hand
        switchForPosition(leftHandPosition, leftHandPositionUpdateAfter, leftHand, false); //for the left hand
        handPositionUpdateAfter[0] = handPosition[0];
        handPositionUpdateAfter[1] = handPosition[1];
        leftHandPositionUpdateAfter[0] = leftHandPosition[0];
        leftHandPositionUpdateAfter[1] = leftHandPosition[1];
    }
}
