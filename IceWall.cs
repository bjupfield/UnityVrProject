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
        public OpenPalmHandler.palmMovement rightPalm;
        public OpenPalmHandler.palmMovement leftPalm;
    }
    actionPalmMovement firstAdjustments = new actionPalmMovement(){
        rightFirst = false,
        rightPalm = null,
        leftPalm = null,
    };
    List<actionPalmMovement> furtherAdjustments = new List<actionPalmMovement>();
    // Start is called before the first frame update
    public Mesh wallFunction(IceHandler.triangulateMesh surface){//this creates the mesh original might need to adjust
        List<Vector3> vectorList = new List<Vector3>(surface.mesh.vertices).ConvertAll<Vector3>(new System.Converter<Vector3, Vector3>((vector)=>(new Vector3(vector.x, 0, vector.y)))); //converts to a z by x surface instead of x and y
        int length = vectorList.Count;
        for(int b = 0; b < 10; ++b){
            for(int i = 0; i < surface.initialPointsLength; ++i){
                Vector3 outlineVector = vectorList[i];
                vectorList.Add(new Vector3(outlineVector.x, -b / 10, outlineVector.z));
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
    public void  readPalmMovement(OpenPalmHandler.palmMovement palm, bool which, Vector3 hand, Vector3 looking, Vector3 head){//which is true if right hand false if left hand
        if(palm.type != "None"){
            if(!(which ? firstAdjustments.rightPalm.fin : firstAdjustments.leftPalm.fin)){//the hand in firstadjustments is not finished
                if(which){
                    firstAdjustments.rightPalm = palm;
                    firstAdjustments.righthand = hand;
                }
                else{
                    firstAdjustments.leftPalm = palm;
                    firstAdjustments.lefthand = hand;
                }
                if((which ? firstAdjustments.leftPalm : firstAdjustments.rightPalm) != null){// seeing if other one is active
                    firstAdjustments.rightFirst = which;
                }
                firstAdjustments.head = head;
                firstAdjustments.looking = looking;
            }
            else{
                actionPalmMovement mostRecentAdjustment = furtherAdjustments.Find(x=> //dont know if im using this right
                    (which ? x.rightPalm.fin == false : x.leftPalm.fin == false || ((which ? x.leftPalm.fin == false : x.rightPalm.fin == false) && (which ? x.rightPalm == null : x.leftPalm == null)))
                );// checks if there is one in the list that is currently adjusting this hand, and if not also checks for one where the other hand is adjusting and the current palm is null so it can create a together statement
                if(mostRecentAdjustment == new actionPalmMovement()){//means there is no recent adjustment and must create new one
                    furtherAdjustments.Add(new actionPalmMovement(){
                        rightFirst = which,
                        rightPalm = which ? palm : null,
                        leftPalm = which ? null : palm,
                        head = head,
                        looking = looking,
                    });
                }
                else{
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
                }
            }
        }
    }
    void simpleLift(OpenPalmHandler.palmMovement lift){
        if(lift.fin){

        }
        else{

        }
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
    async void complexLift(actionPalmMovement action){
        //need to add way to where a hand was assigned after its movement was done, as when a hand movement reaches done it would need to permanently assign which hand was controlling which side for purposes.
        Transform wall = this.gameObject.transform;
        Vector3 leftZCheck = action.lefthand.x * wall.right + action.lefthand.z * wall.forward;
        Vector3 rightZCheck = action.righthand.x * wall.right + action.righthand.z * wall.forward;
        OpenPalmHandler.palmMovement zFurther = leftZCheck.z >= rightZCheck.z ? action.leftPalm : action.rightPalm;
        OpenPalmHandler.palmMovement zShorter = leftZCheck.z >= rightZCheck.z ? action.rightPalm : action.leftPalm;
        Vector3 circleCenter;
        float radius;
        List<Vector3> b = new List<Vector3>(wallMesh.mesh.vertices);
        b.Sort(comparar);
        float furthestZ = b[b.Count - 1].z;
        float closestZ = b[0].z;
        bool isnoteven = true;
        if(zFurther.currMov.y > zShorter.currMov.y){
            Vector3 small = new Vector3(0, zShorter.currMov.y, closestZ);
            Vector3 longer = new Vector3(0, zFurther.currMov.y, furthestZ);
            Vector2 distance = new Vector2(longer.z - small.z, longer.y - small.y);
            radius = Mathf.Abs(distance.magnitude);
            Vector3 middle = new Vector3(0, 0.5f * small.z + 0.5f * longer.z, 0.5f * small.x + 0.5f * longer.x);
            circleCenter = middle - (((Mathf.Sqrt(Mathf.Pow(radius, 2) - (Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.y, 2)) / 4f)) * 2) / radius) * new Vector3(0, small.z - middle.z, small.y - middle.y);
        }
        else if(zFurther.currMov.y < zShorter.currMov.y){
            Vector3 small = new Vector3(0, zShorter.currMov.y, closestZ);
            Vector3 longer = new Vector3(0, zFurther.currMov.y, furthestZ);
            Vector2 distance = new Vector2(longer.z - small.z, longer.y - small.y);
            radius = Mathf.Abs(distance.magnitude);
            Vector3 middle = new Vector3(0, 0.5f * small.z + 0.5f * longer.z, 0.5f * small.x + 0.5f * longer.x);
            circleCenter = middle + (((Mathf.Sqrt(Mathf.Pow(radius, 2) - (Mathf.Pow(distance.x, 2) + Mathf.Pow(distance.y, 2)) / 4f)) * 2) / radius) * new Vector3(0, small.z - middle.z, small.y - middle.y);
        }
        //to above if statements create a circle that we use to create a fun slope for the wall to follow based off the distance between the two furthest points z and the height difference on the currmoves
        else{
            circleCenter = new Vector3();
            isnoteven = false;
            radius = 0;
        }
        for(int i = 0; i < wallMesh.mesh.vertexCount - wallMesh.initialPointsLength; ++i){//this means for all but the bottem vertices
            wallMesh.mesh.vertices[i] = wallMesh.mesh.vertices[i] + new Vector3(0, isnoteven ? ((2 * circleCenter.y) - Mathf.Sqrt(Mathf.Pow(2 * circleCenter.y, 2) - 4 * (Mathf.Pow(wallMesh.mesh.vertices[i].z, 2) - (2 * wallMesh.mesh.vertices[i].z * circleCenter.z) + Mathf.Pow(wallMesh.mesh.vertices[i].z, 2) + Mathf.Pow(wallMesh.mesh.vertices[i].y, 2) - Mathf.Pow(radius, 2)))) : action.leftPalm.currMov.y, 0);//only adding to z from the amount from radius thing above or simply even
            //above uses the quadratic formula to find the amount to add to each vertex based on their position along the z axis and the derived circle above, or just evenly if is even is true.
            //uses terniary to choose if foreven
        }
        //above should adjust the wall based on everything
    }
    Vector3 pushing(actionPalmMovement movement, bool which){//which is right if true
        OpenPalmHandler.palmMovement mine = which ? movement.rightPalm : movement.leftPalm;
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
                break;
            case "XZDescending"://left pushing right descending
                this.gameObject.transform.position += pushing(action, false);
                break;
            case "LiftingXZ"://left lifting right pushing
                this.gameObject.transform.position += pushing(action, true);
                break;
            case "LiftingLifting"://both lifting
                break;
            case "LiftingDescending"://left lifting right descending
                break;
            case "DescendingXZ"://left descending right pushing
                this.gameObject.transform.position += pushing(action, true);
                break;
            case "DescendingLifting"://left descending right lifting
                break;
            case "DescendingDescending"://both descending
                break;
        }
    }
    void firstSwitchSingular(OpenPalmHandler.palmMovement action){
        switch(action.type){
            case "XZ":
                break;
            case "Lifting":
                break;
            case "Descending":
                break;
        }
    }
    void subsequentSwitchBoth(actionPalmMovement action){
        string left = action.leftPalm.type;
        string right = action.rightPalm.type;
        switch(left + right){
            case "XZXZ"://both just pushing
                break;
            case "XZLifting": //left pushing right lifting
                break;
            case "XZDescending"://left pushing right descending
                break;
            case "LiftingXZ"://left lifting right pushing
                break;
            case "LiftingLifting"://both lifting
                break;
            case "LiftingDescending"://left lifting right descending
                break;
            case "DescendingXZ"://left descending right pushing
                break;
            case "DescendingLifting"://left descending right lifting
                break;
            case "DescendingDescending"://both descending
                break;
        }

    }
    void subsequentSwitchSingular(OpenPalmHandler.palmMovement action){
        switch(action.type){
            case "XZ":
                break;
            case "Lifting":
                break;
            case "Descending":
                break;
        }
    }
    void firstActions(){
        if(!firstAdjustments.done){
            if(firstAdjustments.rightFirst){//this means the rightpalm is the first active
                if(firstAdjustments.leftPalm != null){//this means both are active
                    firstSwitchBoth(firstAdjustments);
                }
                else{//only rightpalm is active
                    firstSwitchSingular(firstAdjustments.rightPalm);
                }
            }
            else{//this means the leftpalm is the first active
                if(firstAdjustments.rightPalm != null){//this means both are active
                    firstSwitchBoth(firstAdjustments);
                }
                else{//only leftpalm is active
                    firstSwitchSingular(firstAdjustments.leftPalm);
                }
            }
        }
    }
    void subsequentActions(){
        List<actionPalmMovement> actions = furtherAdjustments.FindAll(x=> x.done == false);
        actions.ForEach(x=>{
            if(x.rightFirst){//this means the rightpalm is first active
                if(x.leftPalm != null){//this means both are active
                    subsequentSwitchBoth(x);
                }
                else{//only rightpalm is active
                    subsequentSwitchSingular(x.rightPalm);
                }
            }
            else{
                if(x != null){//this means both are active
                    subsequentSwitchBoth(x);
                }
                else{//only leftpalm is active
                    subsequentSwitchSingular(x.leftPalm);
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
        
    }
}
