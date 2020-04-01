using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using UnityEngine.PostProcessing;

public class BoundryDegredation : MonoBehaviour {

    [Header("Public Variables")]
    public float m_WalkSpeedScaler = -0.03f; //Scales the speed at which the player's movement deteriorates
    public float m_WalkSpeedConstant = 5f; //The speed of the player when in the safe area
    public float m_VignetteScaler = 10; //Scales the vignette to match the proper units
    public float m_TimeScaler = .5f; //Scales the lerp speed

    public AnimationCurve m_VignetteCurve; //Curve that is used to change the vignette intensity
    public GameObject m_CameraBlock; //The panel used to completely black out the user's view

    private float t = 0f; //Variable used for lerping the player back to the safe area
    private float m_DistanceThreshold = .5f; //Used when the player is close enough to their start location

    private bool m_IsPlayerLeaving; //Used to determine if the player passes through the safe area collider
    private bool m_IsPlayerInSafeArea = true; //Used to determine if the player is in the safe area collider
    private bool m_IsPlayerReturning = false; //Used to determine if the player is getting reset back to safe area
    private bool m_IsLerping = false; //Used to determine if the player is currently lerping

    private FirstPersonController fpc; //First person controller variable for adjusting walk speed
    private PostProcessingProfile m_ppProfile; //Post processing profile for adjusting vignette 
    private VignetteModel.Settings vignetteSettings; //Vignette settings variable that is required for updating the vignette 

    private Vector3 m_ExitLocation; //Location where the player left the safe area
    private Vector3 m_StartLocation; //Location that the player is starting at
    private Vector3 m_LerpStartLocation; //Where the player is being lerped from

    private Collider m_CurrentSafeZoneCollider; //The current collider that is the safe zone

    private Image m_CameraImage;

    private Color m_BlockStartColor = new Color(0, 0, 0, 0); //The camera block start color
    private Color m_BlockEndColor = new Color(0, 0, 0, 1); //The camera block end color


    // Use this for initialization
    void Start () {
        fpc = this.gameObject.GetComponent<FirstPersonController>();

        m_CameraImage = m_CameraBlock.GetComponent<Image>();

        m_ppProfile = this.GetComponentInChildren<PostProcessingBehaviour>().profile;
        vignetteSettings = m_ppProfile.vignette.settings;

        m_StartLocation = this.transform.localPosition;
        
    }

    void Update()
    {
        //If player is being returned to safe area
        if (m_IsPlayerReturning)
        {
            //Check if they are currently lerping
            if (m_IsLerping)
            {
                if (t < 1)
                {
                    //Update t and lerp the player's position
                    t += Time.deltaTime * m_TimeScaler;
                    this.transform.position = Vector3.Lerp(m_LerpStartLocation, m_StartLocation, t);
                }
                else if(Vector3.Distance(this.transform.position, m_StartLocation) <= m_DistanceThreshold)
                {
                    //Player has reached their destination, stop lerp and reset lerp factor
                    m_IsLerping = false;
                    t = 0;
                }
            }
            else
            {
                //If they aren't lerping anymore, then the player has finished returning
                m_IsPlayerReturning = false;
            }
        }
        //Player is in the safe are collider
        if (m_IsPlayerInSafeArea)
        {
            m_IsPlayerLeaving = false;
            fpc.m_WalkSpeed = m_WalkSpeedConstant;
        }
        //Player is exiting the safe area
        if (m_IsPlayerLeaving)
        {
            SlowPlayerMovement();
        }
    }

    #region Triggers
    private void OnTriggerEnter(Collider other)
    {
        //Check that the collider is on the correct layer
        if(other.gameObject.layer ==  9)
        {
            m_IsPlayerInSafeArea = true;
            //Set the reference for the current safe zone
            m_CurrentSafeZoneCollider = other;

            //Grant player their movement
            fpc.m_MouseLook.XSensitivity = 2;
            fpc.m_MouseLook.YSensitivity = 2;

            //Ensure the vignette is reset
            vignetteSettings.intensity = 0f;
            m_ppProfile.vignette.settings = vignetteSettings;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.gameObject.layer != 9)
        {
            return;
        }

        m_IsPlayerLeaving = false;
        m_IsPlayerInSafeArea = true;
    }

    private void OnTriggerExit(Collider other)
    {
        //Check if player's collision is with a layer other than layer 9
        if (other.gameObject.layer != 9)
        {
            //If it is, return and do nothing else
            return;
        }

        //If it is, switch boolean to trigger slowing effects
        m_IsPlayerLeaving = true;
        m_IsPlayerInSafeArea = false;

    }
    #endregion

    #region Private Methods
    private void SlowPlayerMovement()
    {
        //Get player position and the closest point between the player and safe area
        Vector3 currentPlayerPos = this.gameObject.transform.position;
        Vector3 closestPoint = m_CurrentSafeZoneCollider.ClosestPoint(currentPlayerPos);

        //Calculate distance of player to the safe area
        float playerDist = Vector3.Distance(currentPlayerPos, closestPoint);

        //Adjust the walk speed and vignette based on on the player's distance
        AdjustWalkSpeed(m_WalkSpeedScaler, playerDist, m_WalkSpeedConstant);
        AdjustVignette(playerDist);
    }

    private void AdjustWalkSpeed(float walkScaler, float dist, float speedConst)
    {
        //Adjust player's walk speed based on their current distance from the safe zone
        fpc.m_WalkSpeed = (walkScaler * Mathf.Pow(dist, 2)) + speedConst;
    }

    private void AdjustVignette(float dist)
    {
        //Use the m_Vignette curve to adjust the intensity of the vignettte
        //Remap the 0-1 scale of the curve to 0-10 for correct values of intensity
        vignetteSettings.intensity = Remap(0, 1, 0, 10, m_VignetteCurve.Evaluate(dist / m_VignetteScaler));

        m_ppProfile.vignette.settings = vignetteSettings; //This line is required to officially update the vignette settings you changed

        if (vignetteSettings.intensity >= 10)
        {
            //Ping pong the camera block between black and white
            m_CameraImage.color = Color.Lerp(m_BlockStartColor, m_BlockEndColor, Mathf.PingPong(Time.time, 1));

            //Lock player's movement
            fpc.m_WalkSpeed = 0;
            fpc.m_MouseLook.XSensitivity = 0;
            fpc.m_MouseLook.YSensitivity = 0;

            //Begin count for fading animation
            StartCoroutine("ReturningPlayer");
        }
    }

    private IEnumerator ReturningPlayer()
    {
        //After 5 seconds has passed return and run the rest of the code
        yield return new WaitForSeconds(5);

        //Ensure the camera block is off
        m_CameraImage.color = new Color(0, 0, 0, 0);

        //Set where the player is being lerped from and change bools to begin lerp
        m_LerpStartLocation = this.transform.position;
        m_IsPlayerReturning = true;
        m_IsLerping = true;

        //Ensure that the courotine doesn't run again
        StopCoroutine("ReturningPlayer");
    }

    //Used for remapping a values, doesn't return negatives
    private float Remap(float oldMin, float oldMax, float newMin, float newMax, float currentValue)
    {
        float diff = (currentValue - oldMin) / (oldMax - oldMin);

        float result = newMin * (1 - diff) + newMax * (diff);
        if(result < 0)
        {
            return 0;
        }
        else
        {
            return result;
        }
    }
    
    #endregion
}
