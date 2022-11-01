using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//An enum to track the possible states of a FloatingScore
public enum eFSState
{
    idle,
    pre,
    active,
    post

}

//FloatingScore can move itself on screen following a Bezier curve

public class FloatingScore : MonoBehaviour
{
    [Header("Set Dynamicall")]
    public eFSState state = eFSState.idle;

    [SerializeField]
    protected int _score = 0;
    public string scoreString;

    //The score properly sets both _score and scoreString
    public int score
    {
        get
        {
            return (_score);
        }
        set
        {
            _score = value;
            scoreString = _score.ToString("NO"); //"NO" adds commas to the num
            //Search "C# Standard Numeric Format Strings" for ToString formats
            GetComponent<Text>().text = scoreString;
        }
    }

    public List<Vector2> bezierPts; // Bezier points for movement
    public List<float> fontSizes; // Bezier points for font scaling
    public float timeStart = -1f;
    public float timeDuration = 1f;
    public string easingCurve = Easing.InOut; //Uses Easing in Utils.cs

    //The GameObject that will receive the SendMessage when this is done moving
    public GameObject reportFinishTo = null;

    private RectTransform rectTrans;
    private Text txt;

    //Set up the Floating Score and movement
    //Note the use of parameter defaults for eTimeS & eTimeD

    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        rectTrans = GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;

        txt = GetComponent<Text>();

        bezierPts = new List<Vector2>(ePts);

        if(ePts.Count == 1)
        {
            //if only one point then just got here
            transform.position = ePts[0];
            return;
        }

        //if eTimes is the default, just start at the current time
        if (eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;

        state = eFSState.pre; //Set it to the pre state, ready to start moving
    }

    public void FSCallback(FloatingScore fs)
    {
        //When this callback is called by Send message
        //add the score from the calling floatingscore
        score += fs.score;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //iff this is not moving just return
        if (state == eFSState.idle) return;

        //Get u from the current time and duration
        //u ranges from 0 to 1 (usually)
        float u = (Time.time - timeStart) / timeDuration;
        //Use Easing class from Utils to curve the u value
        float uC = Easing.Ease(u, easingCurve);
        if (u < 0)
        {
            //if u<0 the dont move yet
            state = eFSState.pre;
            txt.enabled = false; //Hide the score initially
        }else
        {
            if (u >= 1)
            {
                //if u >=1 we are dont moving
                uC = 1; // set uC=1 so we dont overshoot
                state = eFSState.post;
                if(reportFinishTo != null)
                {
                    // if these a callback gameobject use sendmessage to call fscallback method with this param
                    reportFinishTo.SendMessage("FSCallback", this);
                    //Now message send destroy this gameObject
                    Destroy(gameObject);
                }
                else
                {
                    //if there is nothing to callback dont destroy this
                    state = eFSState.idle;
                }
            }
            else
            {
                //0<=u<1 which meas this is active and moving
                state = eFSState.active;
                txt.enabled = true; // Show score once more
            }
            //Use Bezier curve to move this to the right point
            Vector2 pos = Utils.Bezier(uC, bezierPts);
            //RectTransform anchors can be used to position UI objects relative to total size of screen
            rectTrans.anchorMin = rectTrans.anchorMax = pos;
            if(fontSizes != null && fontSizes.Count > 0)
            {
                //if fontSizes has values in it adjust the fontsize of this guitext
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                GetComponent<Text>().fontSize = size;
            }
        }
    }
}
