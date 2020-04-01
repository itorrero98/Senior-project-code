using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class PlayerManager: MonoBehaviour{
    public static PlayerManager m_Instance;
    public SteamVR_Action_Boolean m_UsePowerUpAction;
    //[SteamVR_DefaultAction("Teleport", "default")]
    public SteamVR_Action_Vector2 walkAction;

    public GameObject m_Portal;
    public GameObject m_PlayerCam;
    public float m_PlayerMoveSpeed = 1f;

    private List<PowerUp> PlayerPowerUps = new List<PowerUp>();

    private int m_NumPortals = 2;

    private int m_POICollectedCount = 0;

    private void Start()
    {
        if (m_Instance == null)
        {
            m_Instance = this;
            DontDestroyOnLoad(m_Instance);
        }
        else if (m_Instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (SteamVR_Input._default.inActions.UsePowerUp.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            if (HasPowerup())
            {
                UsePortal();
                m_NumPortals--;

            }
        }

        float currYInput = walkAction.GetAxis(SteamVR_Input_Sources.LeftHand).y;
        float currXInput = walkAction.GetAxis(SteamVR_Input_Sources.LeftHand).x;
        if (currYInput != 0f || currXInput != 0)
        {
            Quaternion orientation = Camera.main.transform.rotation;
            var touchPadVector = walkAction.GetAxis(SteamVR_Input_Sources.LeftHand);
            Vector3 moveDirection = orientation * Vector3.forward * touchPadVector.y + orientation * Vector3.right * touchPadVector.x;
            Vector3 pos = transform.position;
            pos.x += moveDirection.x * m_PlayerMoveSpeed * Time.deltaTime;
            pos.z += moveDirection.z * m_PlayerMoveSpeed * Time.deltaTime;
            transform.position = pos;
        }


        //if (m_UsePowerUpAction)
        //{
        //    print("usingActionButton");
        //}
    }
    #region Private Methods
    private void AddPowerUp(string type)
    {
        int newPowerIndex = PlayerPowerUps.Count;
        PlayerPowerUps.Add(new PowerUp(newPowerIndex));
    }

    private bool HasPowerup()
    {
        if (m_NumPortals > 0) return true;
        else return false;
    }
    #endregion
    #region Fields
    public int CurrentPOICount
    {
        get
        {
            return m_POICollectedCount;
        }
    }
    public int CurrentPortalCount
    {
        get
        {
            return m_NumPortals;
        }
    }
    #endregion

    #region Public Methods
    public void PlayerCollectedPOI()
    {
        m_POICollectedCount++;
    }

    public void GameReset()
    {
        m_POICollectedCount = 0;
        m_NumPortals = 2;
    }

    public void UsePortal()
    {
        //GameObject tempPortal;
        //Vector3 portalSpawn = new Vector3(gameObject.transform.position.x, 2f, gameObject.transform.position.z);
        //tempPortal = Instantiate(m_Portal, portalSpawn, Quaternion.identity) as GameObject;

        Vector3 playerPos = gameObject.transform.position;
        Vector3 playerDirection = Player.instance.bodyDirectionGuess;
        Quaternion playerRotation = m_PlayerCam.transform.rotation;
        playerRotation.eulerAngles += new Vector3(0, 90, 0);
        //playerRotation.y -= 30f;
        float spawnDistance = 3;

        Vector3 spawnPos = playerPos + playerDirection * spawnDistance;
        spawnPos.y = 2f;

        GameObject tempPortal = Instantiate(m_Portal, spawnPos, playerRotation);
        //Vector3 lookDirection = tempPortal.transform.LookAt(m_PlayerCam.transform.position);
    }
    #endregion
}
