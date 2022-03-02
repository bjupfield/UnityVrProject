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
    void firstActions(){
        if(!firstAdjustments.done){
            if(firstAdjustments.rightFirst){//this means the rightpalm is the first active
                if(firstAdjustments.leftPalm != null){//this means both are active

                }
                else{//only rightpalm is active

                }
            }
            else{//this means the leftpalm is the first active
                if(firstAdjustments.rightPalm != null){//this means both are active
                    
                }
                else{//only leftpalm is active

                }
            }
        }
    }
    void subsequentActions(){
        List<actionPalmMovement> actions = furtherAdjustments.FindAll(x=> x.done == false);
        actions.ForEach(x=>{
            if(x.rightFirst){//this means the rightpalm is first active
                if(firstAdjustments.leftPalm != null){//this means both are active

                }
                else{//only rightpalm is active

                }
            }
            else{
                if(firstAdjustments.rightPalm != null){//this means both are active

                }
                else{//only leftpalm is active

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
