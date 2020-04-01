using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangePOIAmount : MonoBehaviour {

    public Text m_POIText;
    public int m_POIs = 3;
    // Use this for initialization
    void Start()
    {
        m_POIText.text = m_POIs.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        m_POIText.text = m_POIs.ToString();
    }

    void OnTriggerEnter(Collider col)
    {
        Debug.Log("We in here");
        if (m_POIs < 6)
        {
            m_POIs++;
            MazeScript.m_Instance.m_ObjectiveCount = m_POIs;

        }
        else {
            m_POIs = 2;
            MazeScript.m_Instance.m_ObjectiveCount = m_POIs;
        } 
    }
}
