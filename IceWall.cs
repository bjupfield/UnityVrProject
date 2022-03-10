using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceWall : MonoBehaviour
{
    float destroyTimer;
    public bool adjusting;
    public bool together;
    public IceHandler.triangulateMesh wallMesh;
    public class actionPalmMovement{
        public Vector3 head;
        public Vector3 looking;
        public Vector3 lefthand;
        public Vector3 righthand;
        public bool rightFirst;//true if right is the first one
        public bool done = false;
        public float rightTimer = 0;
        public float leftTimer = 0;
        public OpenPalmHandler.palmMovement rightPalm = new OpenPalmHandler.palmMovement();
        public OpenPalmHandler.palmMovement leftPalm = new OpenPalmHandler.palmMovement();
    }
    actionPalmMovement firstAdjustments = new actionPalmMovement(){
        rightFirst = false,
        rightPalm = new OpenPalmHandler.palmMovement(),
        leftPalm = new OpenPalmHandler.palmMovement(),
        done = false,
    };
    List<actionPalmMovement> furtherAdjustments = new List<actionPalmMovement>();
    // Start is called before the first frame update
    int comparar2(int x, int y){
            if(x > y){
                return 1;
            }
            else if(x < y){
                return -1;
            }
            else{
                return 0;
            }
    }
    public IceHandler.triangulateMesh wallFunction(IceHandler.triangulateMesh surface){//this creates the mesh original might need to adjust
        List<Vector3> vectorList = new List<Vector3>(surface.mesh.vertices).ConvertAll<Vector3>(new System.Converter<Vector3, Vector3>((vector)=>(new Vector3(vector.x, 0, vector.y)))); //converts to a z by x surface instead of x and y
        int length = vectorList.Count;
        for(int b = 0; b < 10; ++b){
            float number = b;
            for(int i = 0; i < surface.outlineLength; ++i){
                Vector3 outlineVector = vectorList[i];
                vectorList.Add(new Vector3(outlineVector.x, -number / 10f, outlineVector.z));
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
            for(int i = 0; i < surface.outlineLength; ++i){
                int whichLayer = surface.outlineLength * b + surface.surfaceLength; // decides where the added vertexes from the loop above are
                int whereTop = (b > 0 ? surface.surfaceLength : 0) +  Mathf.Max(b - 1, 0) * surface.outlineLength; //decides where the vertexes above the added vertexes are
                if(i == surface.outlineLength - 1){
                    triangleList.AddRange(new int[]{i + whichLayer, i + whereTop, whereTop,});
                    triangleList.AddRange(new int[]{i + whichLayer, whereTop, whichLayer});
                }
                else{
                    triangleList.AddRange(new int[]{i + whichLayer, i + whereTop, i + 1 + whereTop,});
                    triangleList.AddRange(new int[]{i + whichLayer, i + 1 + whereTop, i + 1 + whichLayer});
                }
            }
        }
        Mesh meshToCreateWall = new Mesh();
        meshToCreateWall.vertices = vectorList.ToArray();
        meshToCreateWall.triangles = triangleList.ToArray();
        return new IceHandler.triangulateMesh(){
            mesh = meshToCreateWall,
            outlineLength = surface.outlineLength,
            surfaceLength = surface.surfaceLength,
        };
    }
    public void readPalmMovement(OpenPalmHandler.palmMovement palm, bool which, Vector3 hand, Vector3 looking, Vector3 head){//which is true if right hand false if left hand
        if(palm.type != "None"){
            if((which ? !firstAdjustments.rightPalm.fin : !firstAdjustments.leftPalm.fin) && !firstAdjustments.done){//the hand in firstadjustments is not finished
                if(which){
                    firstAdjustments.rightPalm = new OpenPalmHandler.palmMovement(){
                        velocity = palm.velocity,
                        type = palm.type,
                        currPos = palm.currPos,
                        startPos = palm.startPos,
                        currMov = palm.currMov,
                        fin = palm.fin,
                    };
                    firstAdjustments.righthand = hand;
                }
                else{
                    firstAdjustments.leftPalm = new OpenPalmHandler.palmMovement(){
                        velocity = palm.velocity,
                        type = palm.type,
                        currPos = palm.currPos,
                        startPos = palm.startPos,
                        currMov = palm.currMov,
                        fin = palm.fin,
                    };
                    firstAdjustments.lefthand = hand;
                }
                if((which ? firstAdjustments.leftPalm : firstAdjustments.rightPalm) != null){// seeing if other one is active
                    firstAdjustments.rightFirst = which;
                }
                firstAdjustments.head = head;
                firstAdjustments.looking = looking;
            }
            else{
                furtherAdjustments.RemoveAll(x=> x.done == true);
                actionPalmMovement mostRecentAdjustment = which ? furtherAdjustments.Find(x=>{
                    // Debug.Log(x.rightPalm.fin);                    
                    return x.rightPalm.fin != true;
                    
                }) : furtherAdjustments.Find(x=> {
                    // Debug.Log(x.leftPalm.fin);
                    return x.leftPalm.fin != true;
                    }
                    );// checks if there is one in the list that is currently adjusting this hand, and if not also checks for one where the other hand is adjusting and the current palm is null so it can create a together statement
                if(mostRecentAdjustment == default(actionPalmMovement)){//means there is no recent adjustment and must create new one
                    if(!palm.fin){
                        furtherAdjustments.Add(new actionPalmMovement(){
                            rightFirst = which,
                            rightPalm = which ? palm : new OpenPalmHandler.palmMovement(){ fin = false},
                            leftPalm = which ? new OpenPalmHandler.palmMovement(){fin = false} : palm,
                            head = head,
                            looking = looking,
                            done = false,
                        });
                        // Debug.Log($"Adjusting further creating new || Fin{palm.fin}");
                    }
                }
                else{
                    // Debug.Log($"Adjusting further number: {furtherAdjustments.FindIndex(x=> x == mostRecentAdjustment)}");
                    if(which){
                        mostRecentAdjustment.rightPalm = palm;
                        mostRecentAdjustment.righthand = hand;
                    }
                    else{
                        mostRecentAdjustment.leftPalm = palm;
                        mostRecentAdjustment.lefthand = hand;
                    }
                    mostRecentAdjustment.head = head;
                    mostRecentAdjustment.looking = looking;
                    if(palm.fin){
                        palm.velocity = (palm.currPos - palm.startPos) / (which ? mostRecentAdjustment.rightTimer : mostRecentAdjustment.leftTimer);
                    }
                }
            }
        }
    }
    void simpleLift(actionPalmMovement lift, bool which){
        OpenPalmHandler.palmMovement mov = which ? lift.rightPalm : lift.leftPalm;
        Debug.Log("Before: " + wallMesh.mesh.vertices[0].y);
        Vector3[] newVertexes = wallMesh.mesh.vertices;
        if(mov.fin){
            for(int i = 0; i < wallMesh.surfaceLength; ++i){//for the top
                newVertexes[i].y += mov.velocity.y * Time.deltaTime;
            }
            for(int i = 0; i < 10; ++i){//this means all the sides
                float d = i;
                for(int j = wallMesh.surfaceLength + wallMesh.outlineLength * i; j < wallMesh.outlineLength * i + wallMesh.surfaceLength; ++j){
                    newVertexes[i].y += mov.velocity.y * Time.deltaTime * ((1f + d) / 11f); 
                }
            }
            mov.velocity -= mov.velocity * .2f;
            if(Mathf.Abs(mov.velocity.y) < .1f){
                mov.velocity = new Vector3(0,0,0);
                lift.done = true;
            }
        }
        else{
            for(int i = 0; i < wallMesh.surfaceLength; ++i){//for the top
                newVertexes[i].y += mov.currMov.y;
            }
            for(int i = 0; i < 10; ++i){//this means all the sides
                float d = i;
                for(int j = wallMesh.surfaceLength + wallMesh.outlineLength * i; j < wallMesh.outlineLength * i + wallMesh.surfaceLength; ++j){
                    newVertexes[i].y += mov.currMov.y * ((1f + d) / 11f);
                }
            }
        }
        wallMesh.mesh.vertices = newVertexes;
        Debug.Log($"SimpleLift || Up by {(mov.fin ? mov.velocity.y * Time.deltaTime : mov.currMov.y)} || Fin {mov.fin} || Done?: {lift.done}");
        Debug.Log("After: " + wallMesh.mesh.vertices[0].y);
        Debug.Log($"Added Together{(mov.fin ? mov.velocity.y * Time.deltaTime : mov.currMov.y) + wallMesh.mesh.vertices[0].y}");
    }
    int comparar(Vector3 x, Vector3 y){
            if(x.z > y.z){
                return 1;
            }
            else if(x.z < y.z){
                return -1;
            }
            else{
                return 0;
            }
    }
    void complexLift(actionPalmMovement action){
        Transform wall = this.gameObject.transform;
        Vector3 leftZCheck = action.lefthand.x * wall.right + action.lefthand.z * wall.forward;
        Vector3 rightZCheck = action.righthand.x * wall.right + action.righthand.z * wall.forward;
        Vector3 zFurther = leftZCheck.z >= rightZCheck.z ? action.leftPalm.fin ? action.leftPalm.velocity * Time.deltaTime : action.leftPalm.currMov : action.rightPalm.fin ? action.rightPalm.velocity * Time.deltaTime : action.rightPalm.currMov;
        Vector3 zShorter = leftZCheck.z >= rightZCheck.z ? action.rightPalm.fin ? action.rightPalm.velocity * Time.deltaTime : action.rightPalm.currMov : action.leftPalm.fin ? action.leftPalm.velocity * Time.deltaTime : action.leftPalm.currMov;
        //above finds which hand is further in line with the z axis of the object and assigns it the further away hand and alos see if it needs to use velocity attribute or currmove
        // Vector3 circleCenter;
        // float radius;
        List<Vector3> b = new List<Vector3>(wallMesh.mesh.vertices);
        b.Sort(comparar);
        float furthestZ = b[b.Count - 1].z;
        float closestZ = b[0].z;
        int furthestIndex = new List<Vector3>(wallMesh.mesh.vertices).FindIndex(x => x.z == furthestZ);
        int closestIndex = new List<Vector3>(wallMesh.mesh.vertices).FindIndex(x => x.z == closestZ);
        float m = furthestZ - closestZ == 0 ? 0 : (zFurther.y - zShorter.y) / (furthestZ - closestZ);
        bool isnoteven = m == 0 ? false : true;
        // if(zFurther > zShorter){
        //     Vector3 small = new Vector3(0, zShorter, closestZ);
        //     Vector3 longer = new Vector3(0, zFurther, furthestZ);
        //     Vector2 distance = new Vector2(longer.z - small.z, longer.y - small.y);
        //     radius = Mathf.Abs(distance.magnitude);
        //     Vector3 middle = new Vector3(0, 0.5f * small.z + 0.5f * longer.z, 0.5f * small.x + 0.5f * longer.x);
        //     circleCenter = middle - (((Mathf.Sqrt(Mathf.Pow(radius, 2) - (Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.y, 2)) / 4f)) * 2) / radius) * new Vector3(0, small.z - middle.z, small.y - middle.y);
        // }
        // else if(zFurther < zShorter){
        //     Vector3 small = new Vector3(0, zShorter, closestZ);
        //     Vector3 longer = new Vector3(0, zFurther, furthestZ);
        //     Vector2 distance = new Vector2(longer.z - small.z, longer.y - small.y);
        //     radius = Mathf.Abs(distance.magnitude);
        //     Vector3 middle = new Vector3(0, 0.5f * small.z + 0.5f * longer.z, 0.5f * small.x + 0.5f * longer.x);
        //     circleCenter = middle + (((Mathf.Sqrt(Mathf.Pow(radius, 2) - (Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.y, 2)) / 4f)) * 2) / radius) * new Vector3(0, small.z - middle.z, small.y - middle.y);
        // }
        //to above if statements create a circle that we use to create a fun slope for the wall to follow based off the distance between the two furthest points z and the height difference on the currmoves
        // else{
        //     circleCenter = new Vector3();
        //     isnoteven = false;
        //     radius = 0;
        // }
        Vector3[] newVertices = wallMesh.mesh.vertices;
        for(int i = 0; i < wallMesh.surfaceLength; ++i){//this means for all but the top vertices
            // wallMesh.mesh.vertices[i].y += isnoteven ? ((2 * circleCenter.y) - Mathf.Sqrt(Mathf.Pow(2 * circleCenter.y, 2) - 4 * (Mathf.Pow(wallMesh.mesh.vertices[i].z, 2) - (2 * wallMesh.mesh.vertices[i].z * circleCenter.z) + Mathf.Pow(wallMesh.mesh.vertices[i].z, 2) + Mathf.Pow(wallMesh.mesh.vertices[i].y, 2) - Mathf.Pow(radius, 2)))) : action.leftPalm.currMov.y;//only adding to z from the amount from radius thing above or simply even
            // newVertices[i].y +=isnoteven ? ((2 * circleCenter.y) - Mathf.Sqrt(Mathf.Pow(2 * circleCenter.y, 2) - 4 * (Mathf.Pow(wallMesh.mesh.vertices[i].z, 2) - (2 * wallMesh.mesh.vertices[i].z * circleCenter.z) + Mathf.Pow(wallMesh.mesh.vertices[i].z, 2) + Mathf.Pow(wallMesh.mesh.vertices[i].y, 2) - Mathf.Pow(radius, 2)))) : action.leftPalm.currMov.y;//only adding to z from the amount from radius thing above or simply even
            newVertices[i].y += isnoteven ? m * newVertices[i].z : action.leftPalm.currMov.y;
            //above uses the quadratic formula to find the amount to add to each vertex based on their position along the z axis and the derived circle above, or just evenly if is even is true.
            //uses terniary to choose if foreven
        }
        for(int i = 0; i < 10; ++i){//this means all the sides
            float d = i;
            for(int j = wallMesh.surfaceLength + wallMesh.outlineLength * i; j < wallMesh.outlineLength * i + wallMesh.surfaceLength; ++j){
            //    wallMesh.mesh.vertices[j].y += isnoteven ? ((2 * circleCenter.y) - Mathf.Sqrt(Mathf.Pow(2 * circleCenter.y, 2) - 4 * (Mathf.Pow(wallMesh.mesh.vertices[j].z, 2) - (2 * wallMesh.mesh.vertices[j].z * circleCenter.z) + Mathf.Pow(wallMesh.mesh.vertices[j].z, 2) + Mathf.Pow(wallMesh.mesh.vertices[j].y, 2) - Mathf.Pow(radius, 2)))) * ((1f + d) / 11f) : action.leftPalm.currMov.y * ((1f + d) / 11f); 
            //    newVertices[j].y += isnoteven ? ((2 * circleCenter.y) - Mathf.Sqrt(Mathf.Pow(2 * circleCenter.y, 2) - 4 * (Mathf.Pow(wallMesh.mesh.vertices[j].z, 2) - (2 * wallMesh.mesh.vertices[j].z * circleCenter.z) + Mathf.Pow(wallMesh.mesh.vertices[j].z, 2) + Mathf.Pow(wallMesh.mesh.vertices[j].y, 2) - Mathf.Pow(radius, 2)))) * ((1f + d) / 11f) : action.leftPalm.currMov.y * ((1f + d) / 11f); 
               newVertices[j].y += isnoteven ? m * newVertices[j].z * ((1f + d) / 11f) : action.leftPalm.currMov.y * ((1f + d) / 11f);
            }
        }
        if(action.leftPalm.fin){
            action.leftPalm.velocity -= action.leftPalm.velocity * .2f;
            if(Mathf.Abs(action.leftPalm.velocity.y) < .1f){
                action.leftPalm.velocity = new Vector3(0,0,0);
            }
        }
        if(action.rightPalm.fin){
            action.rightPalm.velocity -= action.rightPalm.velocity * .2f;
            if(Mathf.Abs(action.rightPalm.velocity.y) < .1f){
                action.rightPalm.velocity = new Vector3(0, 0, 0);
            }
        }
        if(action.leftPalm.fin && action.rightPalm.fin){
            if(action.leftPalm.velocity.magnitude == 0 && action.rightPalm.velocity.magnitude == 0){
                action.done = true;
            }
        }
        wallMesh.mesh.vertices = newVertices;
        Debug.Log($"ComplexLift || Further {newVertices[furthestIndex].y} | Closer {newVertices[closestIndex].y}");
    }
    Vector3 pushing(actionPalmMovement movement, bool which){//which is right if true
        OpenPalmHandler.palmMovement mine = which ? movement.rightPalm : movement.leftPalm;
        Debug.Log("Pushing");
        if(!mine.fin){
            return mine.currMov * .2f;
        }
        else{
            if(mine.velocity.magnitude != 0){
                mine.velocity -= mine.velocity * .1f;
                if(mine.velocity.magnitude < .1f){
                    mine.velocity = new Vector3();
                }
                return mine.velocity;
            }
            else{
                if((which ? (movement.leftPalm.fin && movement.leftPalm.velocity.magnitude == 0) || movement.leftPalm.type == "None" : (movement.rightPalm.fin && movement.rightPalm.velocity.magnitude == 0) || movement.rightPalm.type == "None" )){// check to see if the other palm is also has no velocity and fin is active or if type is not choosen meaning that the action is complete
                    movement.done = true;
                }
                return new Vector3();
            }
        }
    }
    void firstSwitchBoth(actionPalmMovement action){
        string left = action.leftPalm.type;
        string right = action.rightPalm.type;
        switch(left + right){
            case "XZXZ"://both just pushing
                Vector3 toPush = (pushing(action, true) + pushing(action, false)) * .7f;
                this.gameObject.transform.position += toPush;
                break;
            case "XZLifting": //left pushing right lifting
                this.gameObject.transform.position += pushing(action,false);
                simpleLift(action, true);
                break;
            case "XZDescending"://left pushing right descending
                this.gameObject.transform.position += pushing(action, false);
                simpleLift(action, true);
                break;
            case "LiftingXZ"://left lifting right pushing
                this.gameObject.transform.position += pushing(action, true);
                simpleLift(action, false);
                break;
            case "LiftingLifting"://both lifting
                complexLift(action);
                break;
            case "LiftingDescending"://left lifting right descending
                complexLift(action);
                break;
            case "DescendingXZ"://left descending right pushing
                this.gameObject.transform.position += pushing(action, true);
                simpleLift(action, false);
                break;
            case "DescendingLifting"://left descending right lifting
                complexLift(action);
                break;
            case "DescendingDescending"://both descending
                complexLift(action);
                break;
        }
        firstAdjustments.leftTimer += Time.deltaTime;
        firstAdjustments.rightTimer += Time.deltaTime;
        // Debug.Log("first switch both");
    }
    void firstSwitchSingular(actionPalmMovement action, bool which){
        switch(which ? action.rightPalm.type : action.leftPalm.type){
            case "XZ":
                this.gameObject.transform.position += pushing(action, which);
                break;
            case "Lifting":
                simpleLift(action, which);
                break;
            case "Descending":
                simpleLift(action, which);
                break;
        }
        if(which){
            firstAdjustments.rightTimer += Time.deltaTime;
        }
        else{
            firstAdjustments.leftTimer += Time.deltaTime;
        }
        // Debug.Log("first switch singular");
    }
    void subsequentSwitchBoth(actionPalmMovement action){
        string left = action.leftPalm.type;
        string right = action.rightPalm.type;
        switch(left + right){
            case "XZXZ"://both just pushing
                Vector3 toPush = (pushing(action, true) + pushing(action, false)) * .7f;
                this.gameObject.transform.position += toPush;
                break;
            case "XZLifting": //left pushing right lifting
                simpleLift(action, true);
                this.gameObject.transform.position += pushing(action, false);
                break;
            case "XZDescending"://left pushing right descending
                Vector3 toPush2 = (pushing(action, true) + pushing(action, false)) * .7f;
                this.gameObject.transform.position += toPush2;
                break;
            case "LiftingXZ"://left lifting right pushing
                simpleLift(action, false);
                this.gameObject.transform.position += pushing(action, true);
                break;
            case "LiftingLifting"://both lifting
                complexLift(action);
                break;
            case "LiftingDescending"://left lifting right descending
                complexLift(action);
                break;
            case "DescendingXZ"://left descending right pushing
                simpleLift(action, false);
                this.gameObject.transform.position += pushing(action, true);
                break;
            case "DescendingLifting"://left descending right lifting
                complexLift(action);
                break;
            case "DescendingDescending"://both descending
                complexLift(action);
                break;
        }
        action.leftTimer += Time.deltaTime;
        action.rightTimer += Time.deltaTime;
        // Debug.Log("subsequent switch");
    }
    void subsequentSwitchSingular(actionPalmMovement action, bool which){//true if right
        switch(which ? action.rightPalm.type : action.leftPalm.type){
            case "XZ":
                this.gameObject.transform.position += pushing(action, which);
                break;
            case "Lifting":
                simpleLift(action, which);
                break;
            case "Descending":
                simpleLift(action, which);
                break;
        }
        if(which){
            action.rightTimer += Time.deltaTime;
        }
        else{
            action.leftTimer += Time.deltaTime;
        }
        // Debug.Log("subsequent singular");
    }
    void firstActions(){
        if(!firstAdjustments.done){
            if(firstAdjustments.rightFirst){//this means the rightpalm is the first active
                if(firstAdjustments.leftPalm.type != "None"){//this means both are active
                    firstSwitchBoth(firstAdjustments);
                }
                else{//only rightpalm is active
                    firstSwitchSingular(firstAdjustments, true);
                }
            }
            else{//this means the leftpalm is the first active
                if(firstAdjustments.rightPalm.type != "None"){//this means both are active
                    firstSwitchBoth(firstAdjustments);
                }
                else{//only leftpalm is active
                    firstSwitchSingular(firstAdjustments, false);
                }
            }
        }
    }
    void subsequentActions(){
        List<actionPalmMovement> actions = furtherAdjustments.FindAll(x=> x.done == false);
        actions.ForEach(x=>{
            if(x.rightFirst){//this means the rightpalm is first active
                if(x.leftPalm.type != "None"){//this means both are active
                    subsequentSwitchBoth(x);
                }
                else{//only rightpalm is active
                    subsequentSwitchSingular(x, true);
                }
            }
            else{
                if(x.rightPalm.type != "None"){//this means both are active
                    subsequentSwitchBoth(x);
                }
                else{//only leftpalm is active
                    subsequentSwitchSingular(x, false);
                }
            }
        });
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        firstActions();
        subsequentActions();
    }
}
