using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif


[RequireComponent(typeof(Rigidbody)), RequireComponent(typeof(CapsuleCollider))][AddComponentMenu("SUPER Character/SUPER Character Controller")]
public class SUPERMongliCharacterController : MongliCharacterController
{
    #region Variables

    public bool controllerPaused = false;

    #region Movement

    [Header("Movement Settings")]    
    
    public bool enableMovementControl = true;

    //Walking/Sprinting/Crouching
    [Range(1.0f,650.0f)]public float walkingSpeed = 140, sprintingSpeed = 260, crouchingSpeed = 45;
    [Range(1.0f,400.0f)] public float decelerationSpeed=240;
   
    public bool canSprint=true, isSprinting, toggleSprint, sprintOverride, canCrouch=true, isCrouching, toggleCrouch, crouchOverride, isIdle;
    public Stances currentStance = Stances.Standing;
    public float stanceTransitionSpeed = 5.0f, crouchingHeight = 0.80f;
    public GroundSpeedProfiles currentGroundMovementSpeed = GroundSpeedProfiles.Walking;
    public LayerMask whatIsGround =-1;

    //Slope affectors
    public float hardSlopeLimit = 70, slopeInfluenceOnSpeed = 1, maxStairRise = 0.25f, stepUpSpeed=0.2f;

    //Jumping
    public bool canJump=true,holdJump=false, jumpEnhancements=true, Jumped;
   
    [Range(1.0f,650.0f)] public float jumpPower = 40;
    [Range(0.0f,1.0f)] public float airControlFactor = 1;
    public float decentMultiplier = 2.5f, tapJumpMultiplier = 2.1f;
    float jumpBlankingPeriod;

    //Sliding
    public bool isSliding, canSlide = true;
    public float slidingDeceleration = 150.0f, slidingTransitionSpeed=4, maxFlatSlideDistance =10;
    
    //Walking/Sprinting/Crouching
    public GroundInfo currentGroundInfo = new GroundInfo();
    float standingHeight;
    float currentGroundSpeed;
    Vector3 InputDir;
    float HeadRotDirForInput;
    Vector2 MovInput;
    Vector2 MovInput_Smoothed;
    Vector2 _2DVelocity;
    float _2DVelocityMag, speedToVelocityRatio;
    PhysicMaterial _ZeroFriction, _MaxFriction;
    CapsuleCollider capsule;
    Rigidbody p_Rigidbody;
    bool crouchInput_Momentary, crouchInput_FrameOf, sprintInput_FrameOf,sprintInput_Momentary, slideInput_FrameOf, slideInput_Momentary;
    bool changingStances = false; 

    //Slope Affectors

    //Jumping
    bool jumpInput_Momentary, jumpInput_FrameOf;

    //Sliding
    Vector3 cachedDirPreSlide, cachedPosPreSlide;

    [Space(20)]
    #endregion
    
    #region Footstep System
    [Header("Footstep System")]
    public bool enableFootstepSounds = true;
    public FootstepTriggeringMode footstepTriggeringMode = FootstepTriggeringMode.calculatedTiming;
    [Range(0.0f,1.0f)] public float stepTiming = 0.15f;
    public List<GroundMaterialProfile> footstepSoundSet = new List<GroundMaterialProfile>();
    bool shouldCalculateFootstepTriggers= true;
    float StepCycle = 0;
    AudioSource playerAudioSource;
    List<AudioClip> currentClipSet = new List<AudioClip>();
    [Space(18)]
    #endregion

    #region Interactable
   
    public float interactRange = 4;
    public LayerMask interactableLayer = -1;    
    bool interactInput;
    #endregion  

    #region Collectables
    #endregion

    #region Animation   
    
    //public Animator _3rdPersonCharacterAnimator;
    public MongliAnimatorNetworkController animController;
    public string a_velocity, a_2DVelocity, a_Grounded, a_Idle, a_Jumped, a_Sliding, a_Sprinting, a_Crouching;
    public bool stickRendererToCapsuleBottom = true;

    #endregion
    
    [Space(18)]
    public bool enableGroundingDebugging = false, enableMovementDebugging = false, enableMouseAndCameraDebugging = false, enableVaultDebugging = false;
    #endregion
    void Start()
    {
        #region Movement
        p_Rigidbody = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        standingHeight = capsule.height;
        currentGroundSpeed = walkingSpeed;
        _ZeroFriction = new PhysicMaterial("Zero_Friction");
        _ZeroFriction.dynamicFriction =0f;
        _ZeroFriction.staticFriction =0;
        _ZeroFriction.frictionCombine = PhysicMaterialCombine.Minimum;
        _ZeroFriction.bounceCombine = PhysicMaterialCombine.Minimum;
        _MaxFriction = new PhysicMaterial("Max_Friction");
        _MaxFriction.dynamicFriction =1;
        _MaxFriction.staticFriction =1;
        _MaxFriction.frictionCombine = PhysicMaterialCombine.Maximum;
        _MaxFriction.bounceCombine = PhysicMaterialCombine.Average;
        #endregion
        
        #region Footstep
        playerAudioSource = GetComponent<AudioSource>();
        #endregion
    }
    void Update()
    {
        if(!controllerPaused)
        {
            #region Movement
        #region Anulado, habria que actrivarlo si no funciona
            //if(cameraPerspective == PerspectiveModes._3rdPerson){
            //    HeadRotDirForInput = Mathf.MoveTowardsAngle(HeadRotDirForInput,headRot.y, bodyCatchupSpeed*(1+Time.deltaTime));
            //    MovInput_Smoothed = Vector2.MoveTowards(MovInput_Smoothed, MovInput, inputResponseFiltering*(1+Time.deltaTime));
            //}
            //InputDir = cameraPerspective == PerspectiveModes._1stPerson?  Vector3.ClampMagnitude((transform.forward*MovInput.y+transform.right * (viewInputMethods == ViewInputModes.Traditional ? MovInput.x : 0)),1) : Quaternion.AngleAxis(HeadRotDirForInput,Vector3.up) * (Vector3.ClampMagnitude((Vector3.forward*MovInput_Smoothed.y+Vector3.right * MovInput_Smoothed.x),1));
            #endregion
        GroundMovementSpeedUpdate();
        if(canJump && (holdJump? jumpInput_Momentary : jumpInput_FrameOf)){Jump(jumpPower);}
        #endregion

            #region Interaction
        if(interactInput){
            TryInteract();
        }
        #endregion
        }
        else
        {
            jumpInput_FrameOf = false;
            jumpInput_Momentary = false;
        }
        #region Animation
        UpdateAnimationTriggers(controllerPaused);
        #endregion
    }
    void FixedUpdate() {
        if(!controllerPaused)
        {
            #region Movement
            if(enableMovementControl)
            {
                GetGroundInfo();
                MovePlayer(InputDir,currentGroundSpeed);
                if(isSliding)
                {
                    Slide();
                }
                    UpdateBodyRotation_3rdPerson();
            }
            #endregion
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        #region Collectables
        other.GetComponent<ICollectable>()?.Collect();
        #endregion
    }

    #region InputFunctions

    public override void SetInput(Vector2 moveInput, bool jumpDown, bool jumpPressed, bool actionDown, bool actionPressed, bool slideDown, bool slidePressed, bool crouchDown, bool crouchPressed)
    {
        if (!controllerPaused)
        {
            interactInput = actionPressed;

            jumpInput_Momentary = jumpPressed;
            jumpInput_FrameOf = jumpDown;
            crouchInput_Momentary = crouchPressed;
            crouchInput_FrameOf = crouchDown;
            sprintInput_Momentary = moveInput.magnitude > 0.5f ? true : false;
            sprintInput_FrameOf = moveInput.magnitude > 0.5f ? true : false;
            slideInput_Momentary = slidePressed;
            slideInput_FrameOf = slideDown;

            MovInput = moveInput;
            InputDir = new Vector3(moveInput.x, 0f, moveInput.y);
        }
        else
        {
            jumpInput_FrameOf = false;
            jumpInput_Momentary = false;
        }
    }
    
    #endregion
   
    #region Camera Functions  

        void UpdateBodyRotation_3rdPerson()
        {
            //if is moving, rotate capsule to match camera forward   //change button down to bool of isFiring or isTargeting
            if (!isIdle && !isSliding)
            {
                transform.rotation = (Quaternion.Euler(0, Mathf.MoveTowardsAngle(p_Rigidbody.rotation.eulerAngles.y, (Mathf.Atan2(InputDir.x, InputDir.z) * Mathf.Rad2Deg), 20), 0));
            }
            else if (isSliding)
            {
                transform.localRotation = (Quaternion.Euler(Vector3.up * Mathf.MoveTowardsAngle(p_Rigidbody.rotation.eulerAngles.y, (Mathf.Atan2(p_Rigidbody.velocity.x, p_Rigidbody.velocity.z) * Mathf.Rad2Deg), 10)));
            }            
        }
        #endregion

    #region Movement Functions
        void MovePlayer(Vector3 Direction, float Speed){
       // GroundInfo gI = GetGroundInfo();
        isIdle = Direction.normalized.magnitude <=0;
        _2DVelocity = Vector2.right * p_Rigidbody.velocity.x + Vector2.up * p_Rigidbody.velocity.z;
        speedToVelocityRatio = (Mathf.Lerp(0, 2, Mathf.InverseLerp(0, (sprintingSpeed/50), _2DVelocity.magnitude)));
        _2DVelocityMag = Mathf.Clamp((walkingSpeed/50) / _2DVelocity.magnitude, 0f,10f);
    

       
        if((currentGroundInfo.isGettingGroundInfo) && !Jumped && !isSliding)
        {
            //Deceleration
            if(Direction.magnitude==0&& p_Rigidbody.velocity.normalized.magnitude>0.1f){
                p_Rigidbody.AddForce(-new Vector3(p_Rigidbody.velocity.x,currentGroundInfo.isInContactWithGround? p_Rigidbody.velocity.y-  Physics.gravity.y:0,p_Rigidbody.velocity.z)*(decelerationSpeed*Time.fixedDeltaTime),ForceMode.Force); 
            }
            //normal speed
            else if((currentGroundInfo.isGettingGroundInfo) && currentGroundInfo.groundAngle<hardSlopeLimit && currentGroundInfo.groundAngle_Raw<hardSlopeLimit){
                p_Rigidbody.velocity = (Vector3.MoveTowards(p_Rigidbody.velocity,Vector3.ClampMagnitude(((Direction)*((Speed)*Time.fixedDeltaTime))+(Vector3.down),Speed/50),1));
            }
            capsule.sharedMaterial = InputDir.magnitude>0 ? _ZeroFriction : _MaxFriction;
        }
        //Sliding
        else if(isSliding){
            p_Rigidbody.AddForce(-(p_Rigidbody.velocity-Physics.gravity)*(slidingDeceleration*Time.fixedDeltaTime),ForceMode.Force);
        }
        
        //Air Control
        else if(!currentGroundInfo.isGettingGroundInfo){
            p_Rigidbody.AddForce((((Direction*(walkingSpeed))*Time.fixedDeltaTime)*airControlFactor*5)*currentGroundInfo.groundAngleMultiplier_Inverse_persistent,ForceMode.Acceleration);
            p_Rigidbody.velocity= Vector3.ClampMagnitude((Vector3.right*p_Rigidbody.velocity.x + Vector3.forward*p_Rigidbody.velocity.z) ,(walkingSpeed/50))+(Vector3.up*p_Rigidbody.velocity.y);
            if(!currentGroundInfo.potentialStair && jumpEnhancements){
                if(p_Rigidbody.velocity.y < 0 && p_Rigidbody.velocity.y> Physics.gravity.y*1.5f){
                    p_Rigidbody.velocity += Vector3.up*(Physics.gravity.y*(decentMultiplier)*Time.fixedDeltaTime);
                }else if(p_Rigidbody.velocity.y>0 && !jumpInput_Momentary){
                   p_Rigidbody.velocity += Vector3.up*(Physics.gravity.y*(tapJumpMultiplier-1)*Time.fixedDeltaTime);
                }
            }
        }

        
    }
        void Jump(float Force){
        if((currentGroundInfo.isInContactWithGround) && 
            (currentGroundInfo.groundAngle<hardSlopeLimit) && 
           
            (Time.time>(jumpBlankingPeriod+0.1f)) &&
            (currentStance == Stances.Standing && !Jumped)){

                Jumped = true;
                p_Rigidbody.velocity =(Vector3.right * p_Rigidbody.velocity.x) + (Vector3.forward * p_Rigidbody.velocity.z);
                p_Rigidbody.AddForce(Vector3.up*(Force/10),ForceMode.Impulse);

                capsule.sharedMaterial  = _ZeroFriction;
                jumpBlankingPeriod = Time.time;
        }
    }
        public void DoJump(float Force = 10.0f){
        if(
            (Time.time>(jumpBlankingPeriod+0.1f)) &&
            (currentStance == Stances.Standing)){
                Jumped = true;
                p_Rigidbody.velocity =(Vector3.right * p_Rigidbody.velocity.x) + (Vector3.forward * p_Rigidbody.velocity.z);
                p_Rigidbody.AddForce(Vector3.up*(Force/10),ForceMode.Impulse);
              
                capsule.sharedMaterial  = _ZeroFriction;
                jumpBlankingPeriod = Time.time;
        }
    }
        void Slide(){
        if(!isSliding){
            if(currentGroundInfo.isInContactWithGround){
                //do debug print
                if(enableMovementDebugging) {print("Starting Slide.");}
                p_Rigidbody.AddForce((transform.forward*((sprintingSpeed))+(Vector3.up*currentGroundInfo.groundInfluenceDirection.y)),ForceMode.Force);
                cachedDirPreSlide = transform.forward;
                cachedPosPreSlide = transform.position;
                capsule.sharedMaterial = _ZeroFriction;
                StartCoroutine(ApplyStance(slidingTransitionSpeed,Stances.Crouching));
                isSliding = true;
            }
        }else if(slideInput_Momentary){
            if(enableMovementDebugging) {print("Continuing Slide.");}
            if(Vector3.Distance(transform.position, cachedPosPreSlide)<maxFlatSlideDistance){p_Rigidbody.AddForce(cachedDirPreSlide*(sprintingSpeed/50),ForceMode.Force);}
            if(p_Rigidbody.velocity.magnitude>sprintingSpeed/50){p_Rigidbody.velocity= p_Rigidbody.velocity.normalized*(sprintingSpeed/50);}
            else if(p_Rigidbody.velocity.magnitude<(crouchingSpeed/25)){
                if(enableMovementDebugging) {print("Slide too slow, ending slide into crouch.");}
                //capsule.sharedMaterial = _MaxFrix;
                isSliding = false;
                isSprinting = false;
                StartCoroutine(ApplyStance(stanceTransitionSpeed,Stances.Crouching));
                currentGroundMovementSpeed = GroundSpeedProfiles.Crouching;
            }
        }else{
            if(OverheadCheck()){
                if(p_Rigidbody.velocity.magnitude>(walkingSpeed/50)){
                    if(enableMovementDebugging) {print("Key realeased, ending slide into a sprint.");}
                    isSliding = false;
                    StartCoroutine(ApplyStance(stanceTransitionSpeed,Stances.Standing));
                    currentGroundMovementSpeed = GroundSpeedProfiles.Sprinting;
                }else{
                     if(enableMovementDebugging) {print("Key realeased, ending slide into a walk.");}
                    isSliding = false;
                    StartCoroutine(ApplyStance(stanceTransitionSpeed,Stances.Standing));
                    currentGroundMovementSpeed = GroundSpeedProfiles.Walking;
                }
            }else{
                if(enableMovementDebugging) {print("Key realeased but there is an obstruction. Ending slide into crouch.");}
                isSliding = false;
                isSprinting = false;
                StartCoroutine(ApplyStance(stanceTransitionSpeed,Stances.Crouching));
                currentGroundMovementSpeed = GroundSpeedProfiles.Crouching;
            }

        }
    }
        void GetGroundInfo(){
        //to Get if we're actually touching ground.
        //to act as a normal and point buffer.
        currentGroundInfo.groundFromSweep = null;

        currentGroundInfo.groundFromSweep = Physics.SphereCastAll(transform.position,capsule.radius-0.001f,Vector3.down,((capsule.height/2))-(capsule.radius/2),whatIsGround);
        currentGroundInfo.isInContactWithGround = Physics.Raycast(transform.position, Vector3.down, out currentGroundInfo.groundFromRay, (capsule.height/2)+0.25f,whatIsGround);
        
        if(Jumped && (Physics.Raycast(transform.position, Vector3.down, (capsule.height/2)+0.1f,whatIsGround)||Physics.CheckSphere(transform.position-(Vector3.up*((capsule.height/2)-(capsule.radius-0.05f))),capsule.radius,whatIsGround)) &&Time.time>(jumpBlankingPeriod+0.1f)){
            Jumped=false;
        }
        
        
        if(currentGroundInfo.groundFromSweep!=null&&currentGroundInfo.groundFromSweep.Length!=0){
            currentGroundInfo.isGettingGroundInfo=true;
            currentGroundInfo.groundNormals_lowgrade.Clear();
            currentGroundInfo.groundNormals_highgrade.Clear();
            foreach(RaycastHit hit in currentGroundInfo.groundFromSweep){
                if(hit.point.y > currentGroundInfo.groundFromRay.point.y && Vector3.Angle(hit.normal, Vector3.up)<hardSlopeLimit){
                    currentGroundInfo.groundNormals_lowgrade.Add(hit.normal);
                }else{
                    currentGroundInfo.groundNormals_highgrade.Add(hit.normal);
                }
            }                
            if(currentGroundInfo.groundNormals_lowgrade.Any()){
                currentGroundInfo.groundNormal_Averaged = Average(currentGroundInfo.groundNormals_lowgrade);
            }else{
                currentGroundInfo.groundNormal_Averaged = Average(currentGroundInfo.groundNormals_highgrade);
            }
            currentGroundInfo.groundNormal_Raw = currentGroundInfo.groundFromRay.normal;
            currentGroundInfo.groundRawYPosition = currentGroundInfo.groundFromSweep.Average(x=> (x.point.y > currentGroundInfo.groundFromRay.point.y && Vector3.Angle(x.normal,Vector3.up)<hardSlopeLimit) ? x.point.y :  currentGroundInfo.groundFromRay.point.y); //Mathf.MoveTowards(currentGroundInfo.groundRawYPosition, currentGroundInfo.groundFromSweep.Average(x=> (x.point.y > currentGroundInfo.groundFromRay.point.y && Vector3.Dot(x.normal,Vector3.up)<-0.25f) ? x.point.y :  currentGroundInfo.groundFromRay.point.y),Time.deltaTime*2);
                
        }else{
            currentGroundInfo.isGettingGroundInfo=false;
            currentGroundInfo.groundNormal_Averaged = currentGroundInfo.groundFromRay.normal;
            currentGroundInfo.groundNormal_Raw = currentGroundInfo.groundFromRay.normal;
            currentGroundInfo.groundRawYPosition = currentGroundInfo.groundFromRay.point.y;
        }

        if(currentGroundInfo.isGettingGroundInfo){currentGroundInfo.groundAngleMultiplier_Inverse_persistent = currentGroundInfo.groundAngleMultiplier_Inverse;}
            
        currentGroundInfo.groundInfluenceDirection = Vector3.MoveTowards(currentGroundInfo.groundInfluenceDirection, Vector3.Cross(currentGroundInfo.groundNormal_Averaged, Vector3.Cross(currentGroundInfo.groundNormal_Averaged, Vector3.up)).normalized,2*Time.fixedDeltaTime);
        currentGroundInfo.groundInfluenceDirection.y = 0;
        currentGroundInfo.groundAngle = Vector3.Angle(currentGroundInfo.groundNormal_Averaged,Vector3.up);
        currentGroundInfo.groundAngle_Raw = Vector3.Angle(currentGroundInfo.groundNormal_Raw,Vector3.up);
        currentGroundInfo.groundAngleMultiplier_Inverse = ((currentGroundInfo.groundAngle-90)*-1)/90;
        currentGroundInfo.groundAngleMultiplier = ((currentGroundInfo.groundAngle))/90;
           
        currentGroundInfo.groundTag = currentGroundInfo.isInContactWithGround ? currentGroundInfo.groundFromRay.transform.tag : string.Empty;
        if( Physics.Raycast(transform.position+(Vector3.down*((capsule.height*0.5f)-0.1f)), InputDir,out currentGroundInfo.stairCheck_RiserCheck,capsule.radius+0.1f,whatIsGround)){
            if(Physics.Raycast(currentGroundInfo.stairCheck_RiserCheck.point+(currentGroundInfo.stairCheck_RiserCheck.normal*-0.05f)+Vector3.up,Vector3.down,out currentGroundInfo.stairCheck_HeightCheck,1.1f)){
                if(!Physics.Raycast(transform.position+(Vector3.down*((capsule.height*0.5f)-maxStairRise))+InputDir*(capsule.radius-0.05f), InputDir,0.2f,whatIsGround) ){
                    if(!isIdle &&  currentGroundInfo.stairCheck_HeightCheck.point.y> (currentGroundInfo.stairCheck_RiserCheck.point.y+0.025f) /* Vector3.Angle(currentGroundInfo.groundFromRay.normal, Vector3.up)<5 */ && Vector3.Angle(currentGroundInfo.groundNormal_Averaged, currentGroundInfo.stairCheck_RiserCheck.normal)>0.5f){
                        p_Rigidbody.position -= Vector3.up*-0.1f;
                        currentGroundInfo.potentialStair = true;
                    }
                }else{currentGroundInfo.potentialStair = false;}
            }
        }else{currentGroundInfo.potentialStair = false;}
             

        currentGroundInfo.playerGroundPosition = Mathf.MoveTowards(currentGroundInfo.playerGroundPosition, currentGroundInfo.groundRawYPosition+ (capsule.height/2) + 0.01f,0.05f);
        

        if(currentGroundInfo.isInContactWithGround && enableFootstepSounds && shouldCalculateFootstepTriggers){
            if(currentGroundInfo.groundFromRay.collider is TerrainCollider){
                currentGroundInfo.groundMaterial = null;
                currentGroundInfo.groundPhysicMaterial = currentGroundInfo.groundFromRay.collider.sharedMaterial;
                currentGroundInfo.currentTerrain = currentGroundInfo.groundFromRay.transform.GetComponent<Terrain>();
                if(currentGroundInfo.currentTerrain){
                    Vector2 XZ = (Vector2.right* (((transform.position.x - currentGroundInfo.currentTerrain.transform.position.x)/currentGroundInfo.currentTerrain.terrainData.size.x)) * currentGroundInfo.currentTerrain.terrainData.alphamapWidth) + (Vector2.up* (((transform.position.z - currentGroundInfo.currentTerrain.transform.position.z)/currentGroundInfo.currentTerrain.terrainData.size.z)) * currentGroundInfo.currentTerrain.terrainData.alphamapHeight);
                    float[,,] aMap = currentGroundInfo.currentTerrain.terrainData.GetAlphamaps((int)XZ.x, (int)XZ.y, 1, 1);
                    for(int i =0; i < aMap.Length; i++){
                        if(aMap[0,0,i]==1 ){
                            currentGroundInfo.groundLayer = currentGroundInfo.currentTerrain.terrainData.terrainLayers[i];
                            break;
                        }
                    }
                }else{currentGroundInfo.groundLayer = null;}                
            }else{
                currentGroundInfo.groundLayer = null;
                currentGroundInfo.groundPhysicMaterial = currentGroundInfo.groundFromRay.collider.sharedMaterial;
                currentGroundInfo.currentMesh = currentGroundInfo.groundFromRay.transform.GetComponent<MeshFilter>().sharedMesh;
                if(currentGroundInfo.currentMesh && currentGroundInfo.currentMesh.isReadable){
                    int limit = currentGroundInfo.groundFromRay.triangleIndex*3, submesh;
                    for(submesh = 0; submesh<currentGroundInfo.currentMesh.subMeshCount; submesh++){
                        int indices = currentGroundInfo.currentMesh.GetTriangles(submesh).Length;
                        if(indices>limit){break;}
                        limit -= indices;
                    }
                    currentGroundInfo.groundMaterial = currentGroundInfo.groundFromRay.transform.GetComponent<Renderer>().sharedMaterials[submesh];
                }else{currentGroundInfo.groundMaterial = currentGroundInfo.groundFromRay.collider.GetComponent<MeshRenderer>().sharedMaterial; }
            }
        }else{currentGroundInfo.groundMaterial = null; currentGroundInfo.groundLayer = null; currentGroundInfo.groundPhysicMaterial = null;}
        #if UNITY_EDITOR
        if(enableGroundingDebugging){
            print("Grounded: "+currentGroundInfo.isInContactWithGround + ", Ground Hits: "+ currentGroundInfo.groundFromSweep.Length +", Ground Angle: "+currentGroundInfo.groundAngle.ToString("0.00") + ", Ground Multi: "+ currentGroundInfo.groundAngleMultiplier.ToString("0.00") + ", Ground Multi Inverse: "+ currentGroundInfo.groundAngleMultiplier_Inverse.ToString("0.00"));
            print("Ground mesh readable for dynamic foot steps: "+ currentGroundInfo.currentMesh?.isReadable);
            Debug.DrawRay(transform.position, Vector3.down*((capsule.height/2)+0.1f),Color.green);
            Debug.DrawRay(transform.position, currentGroundInfo.groundInfluenceDirection,Color.magenta);
            Debug.DrawRay(transform.position+(Vector3.down*((capsule.height*0.5f)-0.05f)) + InputDir*(capsule.radius-0.05f) ,InputDir*(capsule.radius+0.1f), Color.cyan);
            Debug.DrawRay(transform.position+(Vector3.down*((capsule.height*0.5f)-0.5f)) + InputDir*(capsule.radius-0.05f) ,InputDir*(capsule.radius+0.3f), new Color(0,.2f,1,1));
        }
        #endif
    }
        void GroundMovementSpeedUpdate(){
        #if SAIO_ENABLE_PARKOUR
        if(!isVaulting)
        #endif
        {
            switch (currentGroundMovementSpeed){
                case GroundSpeedProfiles.Walking:{
                    if(isCrouching || isSprinting){
                        isSprinting = false;
                        isCrouching = false;
                        currentGroundSpeed = MovInput.magnitude * walkingSpeed;
                        StopCoroutine("ApplyStance");
                        StartCoroutine(ApplyStance(stanceTransitionSpeed,Stances.Standing));
                    }
                    #if SAIO_ENABLE_PARKOUR
                    if(vaultInput && canVault){VaultCheck();}
                    #endif
                    //check for state change call
                    if((canCrouch&&crouchInput_FrameOf)||crouchOverride){
                        isCrouching = true;
                        isSprinting = false;
                        currentGroundSpeed = crouchingSpeed;
                        currentGroundMovementSpeed = GroundSpeedProfiles.Crouching;
                        StopCoroutine("ApplyStance");
                        StartCoroutine(ApplyStance(stanceTransitionSpeed,Stances.Crouching));
                        break;
                    }else if((canSprint&& sprintInput_FrameOf /*&& ((enableStaminaSystem && jumpingDepletesStamina)? currentStaminaLevel>s_minimumStaminaToSprint : true) && (enableSurvivalStats ? (!currentSurvivalStats.isDehydrated && !currentSurvivalStats.isStarving) : true)*/) ||sprintOverride){
                        isCrouching = false;
                        isSprinting = true;
                        currentGroundSpeed = MovInput.magnitude * walkingSpeed;
                        currentGroundMovementSpeed = GroundSpeedProfiles.Sprinting;
                        StopCoroutine("ApplyStance");
                        StartCoroutine(ApplyStance(stanceTransitionSpeed,Stances.Standing));
                    }
                    break;
                }
                
                case GroundSpeedProfiles.Crouching:{
                    if(!isCrouching){
                        isCrouching = true;
                        isSprinting = false;
                        currentGroundSpeed = crouchingSpeed;
                        StopCoroutine("ApplyStance");
                        StartCoroutine(ApplyStance(stanceTransitionSpeed,Stances.Crouching));
                    }


                    //check for state change call
                    if((toggleCrouch ? crouchInput_FrameOf : !crouchInput_Momentary)&&!crouchOverride && OverheadCheck()){
                        isCrouching = false;
                        isSprinting = false;
                        currentGroundSpeed = MovInput.magnitude * walkingSpeed;
                        currentGroundMovementSpeed = GroundSpeedProfiles.Walking;
                        StopCoroutine("ApplyStance");
                        StartCoroutine(ApplyStance(stanceTransitionSpeed,Stances.Standing));
                        break;
                    }else if(((canSprint && sprintInput_FrameOf/* && ((enableStaminaSystem && jumpingDepletesStamina)? currentStaminaLevel>s_minimumStaminaToSprint : true)&&(enableSurvivalStats ? (!currentSurvivalStats.isDehydrated && !currentSurvivalStats.isStarving) : true)*/) || sprintOverride) && OverheadCheck()){
                        isCrouching = false;
                        isSprinting = true;
                        currentGroundSpeed = MovInput.magnitude * walkingSpeed;
                        currentGroundMovementSpeed = GroundSpeedProfiles.Sprinting;
                        StopCoroutine("ApplyStance");
                        StartCoroutine(ApplyStance(stanceTransitionSpeed,Stances.Standing));
                    }
                    break;
                }

                case GroundSpeedProfiles.Sprinting:{
                    //if(!isIdle)
                    {
                        if(!isSprinting){
                            isCrouching = false;
                            isSprinting = true;
                            currentGroundSpeed = MovInput.magnitude * walkingSpeed;
                            StopCoroutine("ApplyStance");
                            StartCoroutine(ApplyStance(stanceTransitionSpeed,Stances.Standing));
                        }                       
                        //check for state change call
                        if(canSlide && !isIdle && slideInput_FrameOf && currentGroundInfo.isInContactWithGround){
                            Slide();
                            currentGroundMovementSpeed = GroundSpeedProfiles.Sliding;
                            break;
                        }


                        else if((canCrouch&& crouchInput_FrameOf)||crouchOverride){
                            isCrouching = true;
                            isSprinting = false;
                            currentGroundSpeed = crouchingSpeed;
                            currentGroundMovementSpeed = GroundSpeedProfiles.Crouching;
                            StopCoroutine("ApplyStance");
                            StartCoroutine(ApplyStance(stanceTransitionSpeed,Stances.Crouching));
                            break;
                            //Can't leave sprint in toggle sprint.
                        }else if((toggleSprint ? sprintInput_FrameOf : !sprintInput_Momentary)&&!sprintOverride){
                            isCrouching = false;
                            isSprinting = false;
                            currentGroundSpeed = MovInput.magnitude * walkingSpeed;
                            currentGroundMovementSpeed = GroundSpeedProfiles.Walking;
                            StopCoroutine("ApplyStance");
                            StartCoroutine(ApplyStance(stanceTransitionSpeed,Stances.Standing));
                        }
                        break;
                    }
                }
                case GroundSpeedProfiles.Sliding:{
                }break;
            }
        }
    }
        IEnumerator ApplyStance(float smoothSpeed, Stances newStance){
        currentStance = newStance;
        float targetCapsuleHeight = currentStance==Stances.Standing? standingHeight : crouchingHeight;
        //float targetEyeHeight = currentStance == Stances.Standing? standingEyeHeight : crouchingEyeHeight;
        while(!Mathf.Approximately(capsule.height,targetCapsuleHeight)){
            changingStances = true;
            capsule.height = (smoothSpeed>0? Mathf.MoveTowards(capsule.height, targetCapsuleHeight, stanceTransitionSpeed*Time.fixedDeltaTime) : targetCapsuleHeight);
           // internalEyeHeight = (smoothSpeed > 0 ? Mathf.MoveTowards(internalEyeHeight, targetEyeHeight, stanceTransitionSpeed * Time.fixedDeltaTime) : targetCapsuleHeight);
            
            if(currentStance == Stances.Crouching && currentGroundInfo.isGettingGroundInfo){
                p_Rigidbody.velocity = p_Rigidbody.velocity+(Vector3.down*2);
                if(enableMovementDebugging) {print("Applying Stance and applying down force ");}
            }
            yield return new WaitForFixedUpdate();
        }
        changingStances = false;
        yield return null;
    }
        bool OverheadCheck()
    {    //Returns true when there is no obstruction.
        bool result = false;
        if(Physics.Raycast(transform.position,Vector3.up,standingHeight - (capsule.height/2),whatIsGround)){result = true;}
        return !result;
    }
        Vector3 Average(List<Vector3> vectors)
    {
        Vector3 returnVal = default(Vector3);
        vectors.ForEach(x=> {returnVal += x;});
        returnVal/=vectors.Count();
        return returnVal;
    }
    
        #endregion    

    #region Animator Update   
    void UpdateAnimationTriggers(bool zeroOut = false)
        {
            if (animController)
            {                
                if (!zeroOut)
                {
                    //Setup Thirdperson animation triggers here.
                    if (a_velocity != "")
                    {
                        animController.SetFloat(a_velocity, p_Rigidbody.velocity.sqrMagnitude);
                    }
                    if (a_2DVelocity != "")
                    {
                        animController.SetFloat(a_2DVelocity, _2DVelocity.magnitude);
                    }
                    if (a_Idle != "")
                    {
                        animController.SetBool(a_Idle, isIdle);
                    }
                    if (a_Sprinting != "")
                    {
                        animController.SetBool(a_Sprinting, isSprinting);
                    }
                    if (a_Crouching != "")
                    {
                        animController.SetBool(a_Crouching, isCrouching);
                    }
                    if (a_Sliding != "")
                    {
                        animController.SetBool(a_Sliding, isSliding);
                    }
                    if (a_Jumped != "")
                    {
                        animController.SetBool(a_Jumped, Jumped);
                    }
                    if (a_Grounded != "")
                    {
                        animController.SetBool(a_Grounded, currentGroundInfo.isInContactWithGround);
                    }
                }
                else
                {
                    if (a_velocity != "")
                    {
                        animController.SetFloat(a_velocity, 0);
                    }
                    if (a_2DVelocity != "")
                    {
                        animController.SetFloat(a_2DVelocity, 0);
                    }
                    if (a_Idle != "")
                    {
                        animController.SetBool(a_Idle, true);
                    }
                    if (a_Sprinting != "")
                    {
                        animController.SetBool(a_Sprinting, false);
                    }
                    if (a_Crouching != "")
                    {
                        animController.SetBool(a_Crouching, false);
                    }
                    if (a_Sliding != "")
                    {
                        animController.SetBool(a_Sliding, false);
                    }
                    if (a_Jumped != "")
                    {
                        animController.SetBool(a_Jumped, false);
                    }
                    if (a_Grounded != "")
                    {
                        animController.SetBool(a_Grounded, true);
                    }
                }

            }
        }
    #endregion

    #region Interactables
        public bool TryInteract()
    {      
        Collider[] cols = Physics.OverlapBox(transform.position + (transform.forward*(interactRange/2)), Vector3.one*(interactRange/2),transform.rotation,interactableLayer,QueryTriggerInteraction.Ignore);
        IInteractable interactable = null;
        float lastColestDist = 100;
        foreach(Collider c in cols){
            IInteractable i = c.GetComponent<IInteractable>();
            if(i != null){
                float d = Vector3.Distance(transform.position, c.transform.position);
                if(d<lastColestDist){
                    lastColestDist = d;
                    interactable = i;
                }
            }
        }
        return ((interactable != null)? interactable.Interact() : false);            
         
    }
    #endregion   
    
    public void PausePlayer(PauseModes pauseMode){
        controllerPaused = true;
        switch(pauseMode){
            case PauseModes.MakeKinematic:{
                p_Rigidbody.isKinematic = true;
            }break;
            
            case PauseModes.FreezeInPlace:{
                 p_Rigidbody.constraints = RigidbodyConstraints.FreezeAll;
            }break;

            case PauseModes.BlockInputOnly:{

            }break;
        }
       
        p_Rigidbody.velocity = Vector3.zero;
        InputDir = Vector2.zero;
        MovInput = Vector2.zero;
        MovInput_Smoothed = Vector2.zero;
        capsule.sharedMaterial = _MaxFriction;
        
        UpdateAnimationTriggers(true);
        if(a_velocity != ""){
                animController.SetFloat(a_velocity, 0);   
        }
    }
    public void UnpausePlayer(float delay = 0){
        if(delay==0){
            controllerPaused = false;
            p_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            p_Rigidbody.isKinematic = false;
        }
        else{
            StartCoroutine(UnpausePlayerI(delay));
        }
    }
    IEnumerator UnpausePlayerI(float delay){
        yield return new WaitForSecondsRealtime(delay);
        controllerPaused = false;
        p_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        p_Rigidbody.isKinematic = false;
    }
    
}


#region Classes and Enums
[System.Serializable]
public class GroundInfo{
    public bool isInContactWithGround, isGettingGroundInfo, potentialStair;
    public float groundAngleMultiplier_Inverse = 1, groundAngleMultiplier_Inverse_persistent = 1, groundAngleMultiplier = 0, groundAngle, groundAngle_Raw, playerGroundPosition, groundRawYPosition;    
    public Vector3 groundInfluenceDirection, groundNormal_Averaged, groundNormal_Raw;
    public List<Vector3> groundNormals_lowgrade = new List<Vector3>(), groundNormals_highgrade;
    public string groundTag;
    public Material groundMaterial;
    public TerrainLayer groundLayer;
    public PhysicMaterial groundPhysicMaterial;
    internal Terrain currentTerrain;
    internal Mesh currentMesh;
    internal RaycastHit groundFromRay, stairCheck_RiserCheck, stairCheck_HeightCheck;
    internal RaycastHit[] groundFromSweep;

    
}
[System.Serializable]
public class GroundMaterialProfile{
    public MatProfileType profileTriggerType = MatProfileType.Material;
    public List<Material> _Materials;
    public List<PhysicMaterial> _physicMaterials;
    public List<TerrainLayer> _Layers;
    public List<AudioClip> footstepClips = new List<AudioClip>();
}
[System.Serializable]
public class SurvivalStats{
    public float Health = 250.0f, Hunger = 100.0f, Hydration = 100f;
    public bool hasLowHealth, isStarving, isDehydrated;
}
public enum StatSelector{Health, Hunger, Hydration}
public enum MatProfileType {Material, terrainLayer,physicMaterial}
public enum FootstepTriggeringMode{calculatedTiming, calledFromAnimations}
public enum PerspectiveModes{_1stPerson, _3rdPerson}
public enum ViewInputModes{Traditional, Retro}
public enum MouseInputInversionModes{None, X, Y, Both}
public enum GroundSpeedProfiles{Crouching, Walking, Sprinting, Sliding}
public enum Stances{Standing, Crouching}
public enum PauseModes{MakeKinematic, FreezeInPlace,BlockInputOnly}
#endregion

#region Interfaces
public interface IInteractable{
    bool Interact();
}

public interface ICollectable{
    void Collect();
}
#endregion

#region Editor Scripting
#if UNITY_EDITOR
[CustomEditor(typeof(SUPERMongliCharacterController))]
public class SuperFPEditor : Editor{
    Color32 statBackingColor = new Color32(64,64,64,255);
    
    GUIStyle labelHeaderStyle;
    GUIStyle l_scriptHeaderStyle;
    GUIStyle labelSubHeaderStyle;
    GUIStyle clipSetLabelStyle;
    GUIStyle SupportButtonStyle;
    GUIStyle ShowMoreStyle;
    GUIStyle BoxPanel;
    Texture2D BoxPanelColor;
    SUPERMongliCharacterController t;
    SerializedObject tSO, SurvivalStatsTSO;
    SerializedProperty interactableLayer, obstructionMaskField, groundLayerMask, groundMatProf, defaultSurvivalStats, currentSurvivalStats;
    static bool cameraSettingsFoldout = false, movementSettingFoldout = false, survivalStatsFoldout, footStepFoldout = false;

    public void OnEnable(){
        t = (SUPERMongliCharacterController)target;
        tSO = new SerializedObject(t);
        SurvivalStatsTSO = new SerializedObject(t);
        obstructionMaskField = tSO.FindProperty("cameraObstructionIgnore");
        groundLayerMask = tSO.FindProperty("whatIsGround");
        groundMatProf = tSO.FindProperty("footstepSoundSet");
        interactableLayer = tSO.FindProperty("interactableLayer"); 
        BoxPanelColor= new Texture2D(1, 1, TextureFormat.RGBAFloat, false);;
        BoxPanelColor.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.2f));
        BoxPanelColor.Apply();
    }

    public override void OnInspectorGUI(){
        
        #region Style Null Check
        labelHeaderStyle = labelHeaderStyle != null? labelHeaderStyle : new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 13};
        l_scriptHeaderStyle = l_scriptHeaderStyle != null? l_scriptHeaderStyle : new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,richText = true, fontSize = 16};
        labelSubHeaderStyle = labelSubHeaderStyle != null? labelSubHeaderStyle : new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleCenter,fontStyle = FontStyle.Bold, fontSize = 10, richText = true};
        ShowMoreStyle = ShowMoreStyle != null? ShowMoreStyle : new GUIStyle(GUI.skin.label){ alignment = TextAnchor.MiddleLeft, margin = new RectOffset(15,0,0,0) ,fontStyle = FontStyle.Bold, fontSize = 11, richText = true};
        clipSetLabelStyle = labelSubHeaderStyle != null? labelSubHeaderStyle :  new GUIStyle(GUI.skin.label){fontStyle = FontStyle.Bold, fontSize = 13};
        SupportButtonStyle = SupportButtonStyle != null ? SupportButtonStyle : new GUIStyle(GUI.skin.button){fontStyle = FontStyle.Bold, fontSize = 10, richText = true};
        BoxPanel = BoxPanel != null ? BoxPanel : new GUIStyle(GUI.skin.box){normal = {background = BoxPanelColor}};
        #endregion

        #region PlaymodeWarning
        if(Application.isPlaying){
            EditorGUILayout.HelpBox("It is recommended you switch to another Gameobject's inspector, Updates to this inspector panel during playmode can cause lag in the rigidbody calculations and cause unwanted adverse effects to gameplay. \n\n Please note this is NOT an issue in application builds.", MessageType.Warning);
        }
        #endregion

        #region Label  
        EditorGUILayout.Space();
       
        GUILayout.Label("<b><i><size=18><color=#FC80A5>M</color><color=#FFFF9F>O</color><color=#99FF99>N</color><color=#76D7EA>G</color><color=#BF8FCC>LI</color></size></i></b> <size=12><i>Character Controller</i></size>",l_scriptHeaderStyle,GUILayout.ExpandWidth(true));
        
        #endregion

        #region Movement Settings

        EditorGUILayout.Space(); EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6)); EditorGUILayout.Space();
        GUILayout.Label("Movement Settings",labelHeaderStyle,GUILayout.ExpandWidth(true));
        EditorGUILayout.Space(20);

        EditorGUILayout.BeginVertical(BoxPanel);
        if(movementSettingFoldout){
            #region Stances and Speed
            t.enableMovementControl = EditorGUILayout.ToggleLeft(new GUIContent("Enable Movement","Should the player have control over the character's movement?"),t.enableMovementControl);
            GUILayout.Label("<color=grey>Stances and Speed</color>",labelSubHeaderStyle,GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical(BoxPanel);
            EditorGUILayout.Space(15);
            
            GUI.enabled = false;
            t.currentGroundMovementSpeed = (GroundSpeedProfiles)EditorGUILayout.EnumPopup(new GUIContent("Current Movement Speed", "Displays the player's current movement speed"),t.currentGroundMovementSpeed);
            GUI.enabled = true;

            EditorGUILayout.Space();
            t.walkingSpeed = EditorGUILayout.Slider(new GUIContent("Walking Speed", "How quickly can the player move while walking?"),t.walkingSpeed,1,400);

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(BoxPanel);
            t.canSprint = EditorGUILayout.ToggleLeft(new GUIContent("Can Sprint", "Is the player allowed to enter a sprint?"),t.canSprint);
            GUI.enabled = t.canSprint;
            t.toggleSprint = EditorGUILayout.ToggleLeft(new GUIContent("Toggle Sprint", "Should the spring key act as a toggle?"),t.toggleSprint);
            #if ENABLE_INPUT_SYSTEM
            //t.sprintKey = (Key)EditorGUILayout.EnumPopup(new GUIContent("Sprint Key", "The Key used to enter a sprint."),t.sprintKey);
            #else
            t.sprintKey_L = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Sprint Key", "The Key used to enter a sprint."),t.sprintKey_L);
            #endif
            t.sprintingSpeed = EditorGUILayout.Slider(new GUIContent("Sprinting Speed", "How quickly can the player move while sprinting?"),t.sprintingSpeed,t.walkingSpeed+1,650);
            t.decelerationSpeed = EditorGUILayout.Slider(new GUIContent("Deceleration Factor", "Behaves somewhat like a braking force"),t.decelerationSpeed,1,300);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(BoxPanel);
            t.canCrouch = EditorGUILayout.ToggleLeft(new GUIContent("Can Crouch", "Is the player allowed to crouch?"), t.canCrouch);
            GUI.enabled = t.canCrouch;
            t.toggleCrouch = EditorGUILayout.ToggleLeft(new GUIContent("Toggle Crouch", "Should pressing the crouch button act as a toggle?"),t.toggleCrouch);
            #if ENABLE_INPUT_SYSTEM
            //t.crouchKey= (Key)EditorGUILayout.EnumPopup(new GUIContent("Crouch Key", "The Key used to start a crouch."),t.crouchKey);
            #else
            t.crouchKey_L = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Crouch Key", "The Key used to start a crouch."),t.crouchKey_L);
            #endif
            t.crouchingSpeed = EditorGUILayout.Slider(new GUIContent("Crouching Speed", "How quickly can the player move while crouching?"),t.crouchingSpeed, 1, t.walkingSpeed-1);
            t.crouchingHeight = EditorGUILayout.Slider(new GUIContent("Crouching Height", "How small should the character's capsule collider be when crouching?"),t.crouchingHeight,0.01f,2);
            EditorGUILayout.EndVertical();
        
            GUI.enabled = true;

            
            EditorGUILayout.Space(20);
            GUI.enabled = false;
            t.currentStance = (Stances)EditorGUILayout.EnumPopup(new GUIContent("Current Stance", "Displays the character's current stance"),t.currentStance);
            GUI.enabled = true;
            t.stanceTransitionSpeed = EditorGUILayout.Slider(new GUIContent("Stance Transition Speed", "How quickly should the character change stances?"),t.stanceTransitionSpeed,0.1f, 10);

            EditorGUILayout.PropertyField(groundLayerMask, new GUIContent("What Is Ground", "What physics layers should be considered to be ground?"));

            #region Slope affectors
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(BoxPanel);
            GUILayout.Label("<color=grey>Slope Affectors</color>",new GUIStyle(GUI.skin.label){alignment = TextAnchor.MiddleLeft,fontSize = 10, richText = true},GUILayout.ExpandWidth(true));

            t.hardSlopeLimit = EditorGUILayout.Slider(new GUIContent("Hard Slope Limit", "At what slope angle should the player no longer be able to walk up?"),t.hardSlopeLimit,45, 89);
            t.maxStairRise = EditorGUILayout.Slider(new GUIContent("Maximum Stair Rise", "How tall can a single stair rise?"),t.maxStairRise,0,1.5f);
            t.stepUpSpeed = EditorGUILayout.Slider(new GUIContent("Step Up Speed", "How quickly will the player climb a step?"),t.stepUpSpeed,0.01f,0.45f);
            EditorGUILayout.EndVertical();
            #endregion
            EditorGUILayout.EndVertical();
            #endregion

            #region Jumping
            EditorGUILayout.Space();
            GUILayout.Label("<color=grey>Jumping Settings</color>",labelSubHeaderStyle,GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical(BoxPanel);
            //EditorGUILayout.Space(15);

            t.canJump = EditorGUILayout.ToggleLeft(new GUIContent("Can Jump", "Is the player allowed to jump?"),t.canJump);
            GUI.enabled = t.canJump;
            #if ENABLE_INPUT_SYSTEM
            //t.jumpKey = (Key)EditorGUILayout.EnumPopup(new GUIContent("Jump Key", "The Key used to jump."),t.jumpKey);
            #else
            t.jumpKey_L = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Jump Key", "The Key used to jump."),t.jumpKey_L);
            #endif
            t.holdJump = EditorGUILayout.ToggleLeft(new GUIContent("Continuous Jumping", "Should the player be able to continue jumping without letting go of the Jump key"),t.holdJump);
            t.jumpPower = EditorGUILayout.Slider(new GUIContent("Jump Power", "How much power should a jump have?"),t.jumpPower,1,650f);
            t.airControlFactor = EditorGUILayout.Slider(new GUIContent("Air Control Factor", "EXPERIMENTAL: How much control should the player have over their direction while in the air"),t.airControlFactor,0,1);
                
                //ANULADO
            //GUI.enabled = t.enableStaminaSystem;
            //    t.jumpingDepletesStamina = EditorGUILayout.ToggleLeft(new GUIContent("Jumping Depletes Stamina", "Should jumping deplete stamina?"),t.jumpingDepletesStamina);
            //    t.s_JumpStaminaDepletion = EditorGUILayout.Slider(new GUIContent("Jump Stamina Depletion Amount", "How much stamina should jumping use?"),t.s_JumpStaminaDepletion, 0, t.Stamina);
            GUI.enabled = true;
            t.jumpEnhancements = EditorGUILayout.ToggleLeft(new GUIContent("Enable Jump Enhancements","Should extra math be used to enhance the jump curve?"), t.jumpEnhancements);
            if(t.jumpEnhancements){
                t.decentMultiplier = EditorGUILayout.Slider(new GUIContent("On Decent Multiplier","When the player begins to descend  during a jump, what should gravity be multiplied by?"),t.decentMultiplier, 0.1f,5);
                t.tapJumpMultiplier = EditorGUILayout.Slider(new GUIContent("Tap Jump Multiplier","When the player lets go of space prematurely during a jump, what should gravity be multiplied by?"),t.tapJumpMultiplier, 0.1f,5);
            }

            EditorGUILayout.EndVertical();
            #endregion

            #region Sliding
            EditorGUILayout.Space();
            GUILayout.Label("<color=grey>Sliding Settings</color>",labelSubHeaderStyle,GUILayout.ExpandWidth(true));
            EditorGUILayout.BeginVertical(BoxPanel);
            //EditorGUILayout.Space(15);

            t.canSlide = EditorGUILayout.ToggleLeft(new GUIContent("Can Slide", "Is the player allowed to slide? Use the crouch key to initiate a slide!"),t.canSlide);
            GUI.enabled = t.canSlide;
            #if ENABLE_INPUT_SYSTEM
            //t.slideKey = (Key)EditorGUILayout.EnumPopup(new GUIContent("Slide Key", "The Key used to Slide while the character is sprinting."),t.slideKey);
            #else
            t.slideKey_L = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Slide Key", "The Key used to Slide wile the character is sprinting."),t.slideKey_L);
            #endif
            t.slidingDeceleration = EditorGUILayout.Slider(new GUIContent("Sliding Deceleration", "How much deceleration should be applied while sliding?"),t.slidingDeceleration, 50,300);
            t.slidingTransitionSpeed = EditorGUILayout.Slider(new GUIContent("Sliding Transition Speed", "How quickly should the character transition from the current stance to sliding?"),t.slidingTransitionSpeed,0.01f,10);
            t.maxFlatSlideDistance = EditorGUILayout.Slider(new GUIContent("Flat Slide Distance", "If the player starts sliding on a flat surface with no ground angle influence, How many units should the player slide forward?"),t.maxFlatSlideDistance, 0.5f,15);
            GUI.enabled = true;
            EditorGUILayout.EndVertical();
            #endregion
            
            if(GUI.changed){EditorUtility.SetDirty(t); Undo.RecordObject(t,"Undo Movement Setting changes"); tSO.ApplyModifiedProperties();}
        }else{
            t.enableMovementControl = EditorGUILayout.ToggleLeft(new GUIContent("Enable Movement","Should the player have control over the character's movement?"),t.enableMovementControl);
            t.walkingSpeed = EditorGUILayout.Slider(new GUIContent("Walking Speed", "How quickly can the player move while walking?"),t.walkingSpeed,1,400);
            t.sprintingSpeed = EditorGUILayout.Slider(new GUIContent("Sprinting Speed", "How quickly can the player move while sprinting?"),t.sprintingSpeed,t.walkingSpeed+1,650);
        }
        EditorGUILayout.Space();
        movementSettingFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(movementSettingFoldout,movementSettingFoldout ?  "<color=#B83C82>show less</color>" : "<color=#B83C82>show more</color>", ShowMoreStyle);
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndFoldoutHeaderGroup();
        #endregion              

        #region Footstep Audio
        //EditorGUILayout.Space(); EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6)); EditorGUILayout.Space();
        GUILayout.Label("Footstep Audio",labelHeaderStyle,GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(BoxPanel);
        
        t.enableFootstepSounds = EditorGUILayout.ToggleLeft(new GUIContent("Enable Footstep System", "Should the crontoller enable it's footstep audio systems?"),t.enableFootstepSounds);
        GUI.enabled = t.enableFootstepSounds;
        t.footstepTriggeringMode = (FootstepTriggeringMode)EditorGUILayout.EnumPopup(new GUIContent("Footstep Trigger Mode", "How should a footstep SFX call be triggered? \n\n- Calculated Timing: The controller will attempt to calculate the footstep cycle position based on Headbob cycle position, movement speed, and capsule size. This can sometimes be inaccurate depending on the selected perspective and base walk speed. (Not recommended if character animations are being used)\n\n- Called From Animations: The controller will not do it's own footstep cycle calculations/call for SFX. Instead the controller will rely on character Animations to call the 'CallFootstepClip()' function. This gives much more precise results. The controller will still calculate what footstep clips should be played."),t.footstepTriggeringMode);
        
        if(t.footstepTriggeringMode == FootstepTriggeringMode.calculatedTiming){
            t.stepTiming = EditorGUILayout.Slider(new GUIContent("Step Timing", "The time (measured in seconds) between each footstep."),t.stepTiming,0.0f,1.0f);
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.Space();
        //GUILayout.Label("<color=grey>Clip Stacks</color>",labelSubHeaderStyle,GUILayout.ExpandWidth(true));
        EditorGUI.indentLevel++;
        footStepFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(footStepFoldout,footStepFoldout?  "<color=#B83C82>hide clip stacks</color>" : "<color=#B83C82>show clip stacks</color>",ShowMoreStyle);
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUI.indentLevel--;
        if(footStepFoldout){
            if(t.footstepSoundSet.Any()){
                if(!Application.isPlaying){
                    for(int i =0; i< groundMatProf.arraySize; i++){
                        EditorGUILayout.BeginVertical(BoxPanel);
                        EditorGUILayout.BeginVertical(BoxPanel);

                        SerializedProperty profile = groundMatProf.GetArrayElementAtIndex(i), clipList = profile.FindPropertyRelative("footstepClips"), mat = profile.FindPropertyRelative("_Materials"), physMat = profile.FindPropertyRelative("_physicMaterials"), layer = profile.FindPropertyRelative("_Layers"), triggerType = profile.FindPropertyRelative("profileTriggerType");
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField($"Clip Stack {i+1}", clipSetLabelStyle);
                        if(GUILayout.Button(new GUIContent("X", "Remove this profile"),GUILayout.MaxWidth(20))){t.footstepSoundSet.RemoveAt(i);UpdateGroundProfiles(); break;}
                        EditorGUILayout.EndHorizontal();
                        
                        //Check again that the list of profiles isn't empty incase we removed the last one with the button above.
                        if(t.footstepSoundSet.Any()){
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(triggerType,new GUIContent("Trigger Mode", "Is this clip stack triggered by a Material or a Terrain Layer?"));
                            switch(t.footstepSoundSet[i].profileTriggerType){
                                case MatProfileType.Material:{EditorGUILayout.PropertyField(mat,new GUIContent("Materials", "The materials used to trigger this footstep stack."));}break;
                                case MatProfileType.physicMaterial:{EditorGUILayout.PropertyField(physMat,new GUIContent("Physic Materials", "The Physic Materials used to trigger this footstep stack."));}break;
                                case MatProfileType.terrainLayer:{EditorGUILayout.PropertyField(layer,new GUIContent("Terrain Layers", "The Terrain Layers used to trigger this footstep stack."));}break;
                            }
                            EditorGUILayout.Space();

                            EditorGUILayout.PropertyField(clipList,new GUIContent("Clip Stack", "The Audio clips used in this stack."),true);
                            EditorGUI.indentLevel--;
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.Space();
                            if(GUI.changed){EditorUtility.SetDirty(t); Undo.RecordObject(t,$"Undo changes to Clip Stack {i+1}"); tSO.ApplyModifiedProperties();}
                        }
                    }
                }else{
                    EditorGUILayout.HelpBox("Foot step sound sets hidden to save runtime resources.",MessageType.Info);
                }
            }
        if(GUILayout.Button(new GUIContent("Add Profile", "Add new profile"))){ t.footstepSoundSet.Add(new GroundMaterialProfile(){profileTriggerType = MatProfileType.Material, _Materials = null, _Layers = null, footstepClips = new List<AudioClip>()}); UpdateGroundProfiles();}
        if(GUILayout.Button(new GUIContent("Remove All Profiles", "Remove all profiles"))){ t.footstepSoundSet.Clear();}
        EditorGUILayout.Space();
        footStepFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(footStepFoldout,footStepFoldout?  "<color=#B83C82>hide clip stacks</color>" : "<color=#B83C82>show clip stacks</color>",ShowMoreStyle);
        EditorGUILayout.EndFoldoutHeaderGroup();
        }

        //EditorGUILayout.PropertyField(groundMatProf,new GUIContent("Footstep Sound Profiles"));

        GUI.enabled = true;
        EditorGUILayout.HelpBox("Due to limitations In order to use the Material trigger mode, Imported Mesh's must have Read/Write enabled. Additionally, these Mesh's cannot be marked as Batching Static. Work arounds for both of these limitations are being researched.", MessageType.Info);
        EditorGUILayout.EndVertical();
        if(GUI.changed){EditorUtility.SetDirty(t); Undo.RecordObject(t,"Undo Footstep Audio Setting changes"); tSO.ApplyModifiedProperties();}

        #endregion    
       
        #region Interactable
        //EditorGUILayout.Space(); EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6)); EditorGUILayout.Space();
        GUILayout.Label("Interactables Settings",labelHeaderStyle,GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical(BoxPanel);

        #if ENABLE_INPUT_SYSTEM
        //t.interactKey = (Key)EditorGUILayout.EnumPopup(new GUIContent("Interact Key", "The keyboard key used to Interact with objects that implement IInteract"),t.interactKey);
        #else
        t.interactKey_L =(KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Interact Key", "The keyboard key used to Interact with objects that implement IInteract"),t.interactKey_L);
        #endif
        t.interactRange = EditorGUILayout.Slider(new GUIContent("Range","How far out can an interactable be from the player's position?"), t.interactRange, 0.1f,10);
        EditorGUILayout.PropertyField(interactableLayer,new GUIContent("Interactable Layers", "The Layers to check for interactables  on."));

        EditorGUILayout.EndVertical();
        #endregion

        #region Animation Triggers
        //EditorGUILayout.Space(); EditorGUILayout.LabelField("",GUI.skin.horizontalSlider,GUILayout.MaxHeight(6)); EditorGUILayout.Space();
        GUILayout.Label("Animator Settup",labelHeaderStyle,GUILayout.ExpandWidth(true));
        EditorGUILayout.Space(10);
        EditorGUILayout.BeginVertical(BoxPanel);
       // t._1stPersonCharacterAnimator = (Animator)EditorGUILayout.ObjectField(new GUIContent("1st Person Animator", "The animator used on the 1st person character mesh (if any)"),t._1stPersonCharacterAnimator,typeof(Animator), true);
        //t._3rdPersonCharacterAnimator = (Animator)EditorGUILayout.ObjectField(new GUIContent("3rd Person Animator", "The animator used on the 3rd person character mesh (if any)"),t._3rdPersonCharacterAnimator,typeof(Animator), true);
        t.animController = (MongliAnimatorNetworkController)EditorGUILayout.ObjectField(new GUIContent("Animator Controller", "The animator controller used on the 3rd person character mesh (if any)"), t.animController, typeof(MongliAnimatorNetworkController), true);
        EditorGUILayout.BeginVertical(BoxPanel);
        GUILayout.Label("Parameters", labelSubHeaderStyle, GUILayout.ExpandWidth(true));
        t.a_velocity = EditorGUILayout.TextField(new GUIContent("Velocity (Float)", "(Float) The name of the Velocity Parameter in the animator"), t.a_velocity);
        t.a_2DVelocity = EditorGUILayout.TextField(new GUIContent("2D Velocity (Float)", "(Float) The name of the 2D Velocity Parameter in the animator"), t.a_2DVelocity);
        t.a_Idle = EditorGUILayout.TextField(new GUIContent("Idle (Bool)", "(Bool) The name of the Idle Parameter in the animator"), t.a_Idle);
        t.a_Sprinting = EditorGUILayout.TextField(new GUIContent("Sprinting (Bool)", "(Bool) The name of the Sprinting Parameter in the animator"), t.a_Sprinting);
        t.a_Crouching = EditorGUILayout.TextField(new GUIContent("Crouching (Bool)", "(Bool) The name of the Crouching Parameter in the animator"), t.a_Crouching);
        t.a_Sliding = EditorGUILayout.TextField(new GUIContent("Sliding (Bool)", "(Bool) The name of the Sliding Parameter in the animator"), t.a_Sliding);
        t.a_Jumped = EditorGUILayout.TextField(new GUIContent("Jumped (Bool)", "(Bool) The name of the Jumped Parameter in the animator"), t.a_Jumped);
        t.a_Grounded = EditorGUILayout.TextField(new GUIContent("Grounded (Bool)", "(Bool) The name of the Grounded Parameter in the animator"), t.a_Grounded);
        EditorGUILayout.EndVertical();
        EditorGUILayout.HelpBox("WIP - This is a work in progress feature and currently very primitive.\n\n No triggers, bools, floats, or ints are set up in the script. To utilize this feature, find 'UpdateAnimationTriggers()' function in this script and set up triggers with the correct string names there. This function gets called by the script whenever a relevant parameter gets updated. (I.e. when 'isVaulting' changes)" ,MessageType.Info);
        EditorGUILayout.EndVertical();
        if(GUI.changed){EditorUtility.SetDirty(t); Undo.RecordObject(t,"Undo Animation settings changes"); tSO.ApplyModifiedProperties();}
        #endregion
    }

    void UpdateGroundProfiles(){
        tSO = new SerializedObject(t);
        groundMatProf = tSO.FindProperty("footstepSoundSet");
    }
}
#endif
#endregion

