using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeMazeSize : MonoBehaviour {
    public Text m_SizeText;
    public Text m_POIText;
    public int m_MazeSize = 3;
	// Use this for initialization
	void Start () {
        m_SizeText.text = m_MazeSize.ToString() + "x" + m_MazeSize.ToString();
	}
	
	// Update is called once per frame
	void Update () {
        m_SizeText.text = m_MazeSize.ToString() + "x" + m_MazeSize.ToString();
    }

    void OnTriggerEnter(Collider col)
    {
        Debug.Log("We in here");
        if (m_MazeSize < 6)
        {
            m_MazeSize++;
            MazeScript.m_Instance.m_xSize = m_MazeSize;
            MazeScript.m_Instance.m_ySize = m_MazeSize;
            MazeScript.m_Instance.ResetMaze();
        }
        else
        {
            m_MazeSize = 2;
            MazeScript.m_Instance.m_xSize = m_MazeSize;
            MazeScript.m_Instance.m_ySize = m_MazeSize;
            MazeScript.m_Instance.ResetMaze();
        }
    }
}
