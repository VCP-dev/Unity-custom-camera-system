using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;

public class Cameramovementscript : MonoBehaviour
{

    public Camera cam;
    public GameObject parentofcam;
    public GameObject camparentwithanimator;
    public GameObject hitpanel;
    public GameObject damagepanel;



    // ---------------------- game over panel -------------------

    public GameObject gameoverpanel;
    public Text timetakentext;
    [HideInInspector]
    public float starttime;
    [HideInInspector]
    public float endtime;
    [HideInInspector]
    public int numberoftargetskilled;

    // ---------------------- game over panel -------------------



    [HideInInspector]
    public Vector3 facedir;


    
    // ------------------------------------ for different camera positions ----------------------------------

    [HideInInspector]
    public bool followplayer;
    [HideInInspector]
    public bool fixedatpos;
    [HideInInspector]
    public GameObject postobefixedat;

    // ------------------------------------ for different camera positions ----------------------------------


    GameObject player;
    playerscript playerscriptobj;
    Animator anim;


    float minZoom;
    float maxZoom;
    float zoomlimiter;
    float smoothTime;
    float rotsmoothTime;
    float camrotx;
    float rotxval;
    float facedirDelay;
    float faceposDelay;
    float delayaftermousemovement;
    float mousesensitivity;
    float minangleofrotation;
    float maxangleofrotation;
    float moverotsensitivity;


    // ---------- panel amts --------------
    float hitpanelamt;
    float damagepanelamt;
    // ---------- panel amts --------------


    float distlocked;
    float distlockedenemytarget;
    float distlockedenemycentertarget;
    float heightlocked;
    float xdistlock;
    float xanglelocked;
    float smoothDistVal;
    float smoothDirVal;


    bool setdirtoplayer;


    Vector3 velocity;
    Vector3 velocity1;
    Vector3 velocity2;
    Vector3 velocity3;
    float velocityang;
    Vector3 facePosition;
    List<Transform> targets = new List<Transform>();

    void Awake()
     {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
     }

    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        player = GameObject.FindGameObjectWithTag("Player");
        playerscriptobj = player.GetComponent<playerscript>();
        anim = camparentwithanimator.GetComponent<Animator>();
        smoothTime = 0.023f;//0.15f;//0.23f;//0.2f;
        minZoom = 65.12f;        //  minimum FOV
        maxZoom = 55f;          //  max FOV
        zoomlimiter = 0.23f;
        smoothTime = 0.15f;//0.23f;//0.2f;
        rotsmoothTime = 0.165f;
        rotxval = 0f;
        facedirDelay = 0f;
        faceposDelay = 0f;
        mousesensitivity = 1f;
        moverotsensitivity = 0.3f;
        minangleofrotation = -20f;
        maxangleofrotation = 20f;
        delayaftermousemovement = 0f;
        hitpanelamt = damagepanelamt = 0f;

        facedir = DirectionBetweenPlayerandCam();

        targets.Add(player.transform);
        setdirtoplayer = false;


        followplayer = true;

        if(gameoverpanel){
            gameoverpanel.SetActive(false);
        }
        starttime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            Scene scene = SceneManager.GetActiveScene(); 
            SceneManager.LoadScene(scene.name);
        }

        panelset(ref hitpanel, ref hitpanelamt);
    }

    //void FixedUpdate()
    void LateUpdate()
    {
        
        minZoom = 80f;//70f;//65.12f;
        maxZoom = 70f;//60f;//55f;


        smoothDistVal = 0.175f;//0.075f;
        smoothDirVal = 0.045f;//0.13f;//0.09f;
        mousesensitivity = 2.25f;//2.7f;
        
        if(fixedatpos){
            // do nothing
            // cam change script will set everything
            camrotx = rotxval = 0f;
            transform.position = postobefixedat.transform.position;
            transform.rotation = postobefixedat.transform.rotation;
        }else if(followplayer){
            // ----------------- values for when not in combat ------------------
            //distlocked = 0.53f;
            //heightlocked = 0.28f;//0.35f;
            //xanglelocked = 15f;//25f;
            // ----------------- values for when not in combat ------------------
            // ----------------- values for when in combat ------------------
            distlocked = 0.47f;
            heightlocked = 0.3f;//0.33f;
            xanglelocked = 23.5f;//26.6f;
            // ----------------- values for when in combat ------------------
            CamPlayerLockon();
                

            // ----------------------- still very experimental, and will probably require making the entire camera system -------------------
            if(!playerscriptobj.dead && !playerscriptobj.removeinteractivity){
                CamMouseInput();
                CamKeyboardInput();
            }
            // ----------------------- still very experimental, and will probably require making the entire camera system -------------------
        
            Zoom();
        }

        panelset(ref hitpanel, ref hitpanelamt);
        panelset(ref damagepanel, ref damagepanelamt);        

        camrotx = Mathf.SmoothDamp(camrotx,rotxval,ref velocityang,rotsmoothTime);
        parentofcam.transform.localRotation = Quaternion.Euler(camrotx,0f,0f);
    }

    void panelset(ref GameObject panel, ref float panelsetamt, float delatamt = 140f)
    {
        if(panelsetamt>0){
            panelsetamt -= (delatamt*Time.deltaTime);
        }
        panel.GetComponent<Image>().color = new Color(panel.GetComponent<Image>().color.r,panel.GetComponent<Image>().color.g,panel.GetComponent<Image>().color.b,panelsetamt/255f);
    }

    void Zoom()
    {
        float newZoom = Mathf.Lerp(maxZoom,minZoom,GetGreatestDistance()/zoomlimiter);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView,newZoom,Time.deltaTime * 2.87f);
    }

    Vector3 GetCenterPoint()
    {
        if(targets.Count == 1)
        {
            return targets[0].position;
        }

        // just return position of player
        // If you come up with a solution that isn't too jank, can use that
        // else maybe should just add a lock-on
        return new Vector3(player.transform.position.x, 0f, player.transform.position.z);

        // for getting center point of all targets at once
        /*float totalX = 0f;
        float totalZ = 0f;
            
        for(int i = 0; i<targets.Count ;i++)
        {
            totalX += targets[i].position.x;
            totalZ += targets[i].position.z;
        }

        float avgX = totalX / targets.Count;
        float avgZ = totalZ / targets.Count;

        return new Vector3(avgX, 0f, avgZ);*/
    }

    float GetGreatestDistance()
    {
        var bounds = new Bounds(targets[0].position,Vector3.zero);
        for(int i=0;i<targets.Count;i++)
        {
            bounds.Encapsulate(targets[i].position);
        }
        return bounds.size.x;
    }


    public void SetCameraTargets(List<Transform> newObjs)
    {
        targets.Clear();
        targets.Add(player.transform);
        for(int i=0;i<newObjs.Count;i++)
        {
            if(!targets.Contains(newObjs[i])){
                targets.Add(newObjs[i]);
            }
        }
    }


    /** 
        to set the camera in a certain direction
    */
    public void setCamDirection(Vector3 dir, bool addmoredelay = false)
    {
        Vector3 dir1 = dir;
        facedirDelay = faceposDelay = Time.time + (addmoredelay ? 0.088f : 0.05f);
        float ang = Vector3.Angle(facedir, dir1);
        //Debug.Log(ang);
        
        // Just to ensure that the camera actually turns when you're resetting vertically
        if(ang <= 180f && ang >= 160f) {
            Quaternion rotation = Quaternion.AngleAxis(13f, Vector3.up);
            dir1 = rotation * dir;
        }

        facedir = dir1;
        Vector3 playerpos = player.transform.position;
        Vector3 lockonpos = new Vector3(playerpos.x, playerpos.y/*0f*/, playerpos.z);
        Vector3 setpos1 = lockonpos - (facedir * distlocked);
        Vector3 setpos2 = lockonpos + (facedir * distlocked);
        float distsetpos1 = Vector3.Distance(transform.position,setpos1);
        float distsetpos2 = Vector3.Distance(transform.position,setpos2);
        
        Vector3 setpos;

        if(ang < 90){
            setpos = (distsetpos1<distsetpos2) ? setpos1 : setpos2 ;
        }else{
            setpos = (distsetpos1>distsetpos2) ? setpos1 : setpos2 ;
        }        

        facePosition = new Vector3(setpos.x,playerpos.y+heightlocked,setpos.z);
    }


    public Vector3 DirectionBetweenPlayerandCam()
    {
        Vector3 campos = new Vector3(transform.position.x,player.transform.position.y/*0f*/,transform.position.z);
        Vector3 playerpos = new Vector3(player.transform.position.x,player.transform.position.y/*0f*/,player.transform.position.z);
        return (playerpos - campos).normalized;
    }


    // ----------------------- still very experimental, and will probably require making the entire camera system -------------------
    void CamMouseInput()
    {
        // PLAYER CAMERA CONTROL
        Vector3 mouseinput = new Vector3(0f/*Input.GetAxisRaw("Mouse Y")*/,-1*Input.GetAxisRaw("Mouse X"),0f);

        if(Mathf.Abs(mouseinput.y) > 0f){
            Quaternion rot = Quaternion.AngleAxis((mouseinput.y > 0) ? -mousesensitivity : mousesensitivity, Vector3.up);
            Vector3 newfacedir = rot * facedir;
            setCamDirection(newfacedir, true);
            delayaftermousemovement = Time.time + 0.09f;
        }
    }

    void turnCamDuringMovement()
    {
        float xval = playerscriptobj.horizontalaxis();
        
        if(Mathf.Abs(xval) > 0f && !playerscriptobj.attacking){
            Quaternion rot = Quaternion.AngleAxis((xval > 0) ? moverotsensitivity : -moverotsensitivity, Vector3.up);
            Vector3 newfacedir = rot * facedir;
            setCamDirection(newfacedir, true);
        }
    }

    void CamKeyboardInput()
    {
        if(Mathf.Abs(keyturnval()) > 0f){
            Quaternion rot = Quaternion.AngleAxis((keyturnval() > 0) ? -mousesensitivity : mousesensitivity, Vector3.up);
            Vector3 newfacedir = rot * facedir;
            setCamDirection(newfacedir, true);
            delayaftermousemovement = Time.time + 0.09f;
        }
    }

    float keyturnval()
    {
        if(Input.GetKey(KeyCode.I)){
            return 1;
        }
        else if(Input.GetKey(KeyCode.O)){
            return -1;
        }
        else{
            return 0;
        }
    }
    // ----------------------- still very experimental, and will probably require making the entire camera system -------------------


    public bool isenemyonrightofplayer(Vector3 enemypos)
    {
        Vector3 playerpos = player.transform.position;
        Vector3 camerapos = new Vector3(transform.position.x,0f,transform.position.z);
        Vector3 playerdir = (playerpos - camerapos).normalized;
        Vector3 enemydir = (enemypos - camerapos).normalized;
        float playerang = Vector3.SignedAngle(facedir, playerdir, Vector3.up);
        float enemyang = Vector3.SignedAngle(facedir, enemydir, Vector3.up);
        //Debug.Log("player ang : "+playerang+", enemy ang : "+enemyang);
        return (enemyang >= 0f);
    }


    void CamPlayerLockon()
    {
        if(Time.time > facedirDelay && !setdirtoplayer){
            // delay added for when camera direction is reset by player
            facedir = DirectionBetweenPlayerandCam();
            setdirtoplayer = true;
        }

        if(Time.time > delayaftermousemovement && !playerscriptobj.removeinteractivity){
            turnCamDuringMovement();
        }

        rotxval = xanglelocked;
        Vector3 playerpos = player.transform.position;
        Vector3 lockonpos = new Vector3(playerpos.x, playerpos.y, playerpos.z);
        Vector3 setpos1 = lockonpos - (facedir * distlocked);
        Vector3 setpos2 = lockonpos + (facedir * distlocked);
        float distsetpos1 = Vector3.Distance(transform.position,setpos1);
        float distsetpos2 = Vector3.Distance(transform.position,setpos2);
        Vector3 setpos = (distsetpos1<distsetpos2) ? setpos1 : setpos2 ;
        if(Time.time > faceposDelay){
            // delay added for when camera direction is reset by player
            facePosition = new Vector3(setpos.x,heightlocked+playerpos.y,setpos.z);
        }

        transform.position = Vector3.SmoothDamp(transform.position,facePosition,ref velocity,smoothDistVal);

        transform.forward = Vector3.SmoothDamp(transform.forward,DirectionBetweenPlayerandCam()/*facedir*/,ref velocity1,smoothDirVal);
    }



    bool isobjincameraview(GameObject obj)
    {
        Vector3 viewPos = cam.WorldToViewportPoint(obj.transform.position);
        return (viewPos.x >= 0 && viewPos.x <= 1 && viewPos.y >= 0 && viewPos.y <= 1 && viewPos.z > 0);
    }



    public void camshake()
    {
        anim.SetTrigger("shake");
    }

    public void campanel(float amt)
    {
        hitpanelamt = amt;
    }

    public void damagedpanel(float amt)
    {
        damagepanelamt = amt;
    }

    public void activategameoverpanel()
    {
        gameoverpanel.SetActive(true);
        int totaltime = (int)(endtime-starttime);
        timetakentext.text = "Time taken : "+(totaltime/60)+" min "+(totaltime%60)+" s";
    }
}
