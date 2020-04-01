using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {
    public static GameManager m_Instance;
    public Text m_POICountText;
    public Text m_PortalCountText;
    public Text m_TimerText;

    private float m_CurrTime = 0.0f;

    private bool timer = false;
    public bool IsInEndGame = false;
    public bool AllPOIsCollected = false;
	// Use this for initialization
	void Start () {
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
	
	// Update is called once per frame
	void FixedUpdate () {
        int currPOIs = PlayerManager.m_Instance.CurrentPOICount;
        int currPortals = PlayerManager.m_Instance.CurrentPortalCount;
        if (!AllPOIsCollected)
        {
            m_POICountText.text = "POI's Collected: " + currPOIs + " / "+ MazeScript.m_Instance.m_ObjectiveCount;
            m_PortalCountText.text = "Portals Left: " + currPortals + " / 2";
            m_TimerText.text = "Time: " + m_CurrTime.ToString("F");

        }
        else if(AllPOIsCollected && !IsInEndGame)
        {
            m_POICountText.text = "You've collected all available POI's find the exit to stop the timer";

        }else if(IsInEndGame && AllPOIsCollected)
        {
            m_POICountText.text = "Congratulations! You finished in: " + m_CurrTime.ToString("F") + " seconds";
            m_TimerText.text = "";
        }
        if (currPOIs >= MazeScript.m_Instance.m_ObjectiveCount && !AllPOIsCollected)
        {
            AllPOIsCollected = true;
            //StartCoroutine("EndGame");
        }

        if (timer)
        {
            m_CurrTime += Time.deltaTime;
        }
	}

    private IEnumerator EndGame()
    {
        Debug.Log("In end game");
        IsInEndGame = true;
        yield return new WaitForSeconds(3);
        /*
        IsInEndGame = false;
        PlayerManager.m_Instance.GameReset();
        MazeScript.m_Instance.ResetMaze();
        */
        StopCoroutine("EndGame");
    }

    public void StartGameTimer()
    {
        timer = true;
    }

    public void StopGameTimer()
    {
        timer = false;
        StartCoroutine("EndGame");
    }
}
