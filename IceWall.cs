using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceWall : MonoBehaviour
{
    public bool adjusting;
    public bool together;
    public IceHandler.triangulateMesh firstMesh;
    class creationPalmMovement{
        public bool together;
        public bool rightFirst;//true if right is the first one
        public OpenPalmHandler.palmMovement rightPalm;
        public OpenPalmHandler.palmMovement leftPalm;
    }
    public OpenPalmHandler.palmMovement[] firstAdjustments = new OpenPalmHandler.palmMovement[2];
    public OpenPalmHandler.palmMovement[][] afterAdjustments = new OpenPalmHandler.palmMovement[0][];
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
