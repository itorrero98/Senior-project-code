using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class MazeScript : MonoBehaviour {
    [System.Serializable]
    private class Cell
    {
        public bool visited = false; //For if the cell has been visted
        public bool outerWall = false; //Lets us tell if it's a  wall on the edge of the maze
        public GameObject north; //1
        public GameObject south; //2
        public GameObject east; //3
        public GameObject west; //4
    }
    public static MazeScript m_Instance;
    public GameObject m_Player; //Player GameObject
    public GameObject m_MazeWall;  //GameObject for MazeWall
    public GameObject m_Floor; //GameObject for the Floor
    public GameObject m_TeleFloor; //GameObject for teleporting around the maze
    public GameObject m_MazeWallHolder; //Holder for all maze walls
    public GameObject m_Pickup; //Pickup prefab
    public GameObject m_POIPedestal;
    public GameObject m_ExitRoom; //Exit area prefab for easy placing

    public int m_xSize = 5; //Width of the maze
    public int m_ySize = 5; //Height of the maze
    public int m_ObjectiveCount = 0; //Amount of POI's in the maze


    private List<int> m_MazeCorners = new List<int>(); //Array for the 4 corners of the cell for player placement
    private Cell[] m_CellArray; //Array of all cells in the maze
    private List<GameObject> m_Mazeboundires = new List<GameObject>(); //Array of all outer walls

    private int m_CurrentCell = 0; //Current cell being manipulated
    private int m_TotalCells; //Total amount of cells in the maze
    private int m_VisitedCells = 0; //Amount of cells that have already been visited
    private int m_CurrentNeighbor = 0; //Current neighboring cell
    private int m_BackingUp = 0; //Used to "Back up" just in case we get stuck in between visted cells
    private int m_WallToBreak = 0; //Value of the wall we want to remove
    private int m_FloorScaler = 10;
    private int m_MazeBoundsArrayCount = 0;
    private int m_POICellsAvailable; //Cells remaining without a POI that way we don't get double in one cell

    private string m_ExitWallHidden; //String for exit wall that we have hidden previously

    private float m_POIHeight = 1f; //Raises the POI off the ground slightly
    private float m_WallneighborLength; //neighborLength of one maze wall

    private bool m_StartedBuilding = false; //To ensure we started to build the maze

    private Vector3 m_FloorPos; //Positioning of the maze floor

    private List<int> m_LastCells = new List<int>(); //To keep track of all the cells we have been to
    private List<int> m_CellsAvailableForPOI = new List<int>(); //List to hold all the cells that don't have a POI in it
    private List<GameObject> m_POIs = new List<GameObject>();
    private List<GameObject> m_Pedestals = new List<GameObject>();

    #region Unity Functions
    // Use this for initialization
    void Start ()
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

        m_MazeWallHolder = new GameObject();
        m_MazeWallHolder.name = "Maze";

        m_WallneighborLength = m_MazeWall.transform.localScale.x;

        CreateMaze();
	}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ResetMaze();
        }
    }

    #endregion

    #region Maze Config

    private void CreateMaze()
    {
        for(int i = 0; i < (m_xSize * m_ySize); i++)
        {
            m_CellsAvailableForPOI.Add(i);
        }
        //Setting initial wall position
        Vector3 initialPos = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 myPos = initialPos;

        //Reference variable for maze walls
        GameObject tempWall;

        //Placing x-axis walls
        for(int i = 0; i <= m_ySize; i++)
        {
            for(int j = 0; j < m_xSize; j++)
            {
                myPos = new Vector3(initialPos.x + (j * m_WallneighborLength) - m_WallneighborLength / 2, 0.0f, initialPos.z + (i * m_WallneighborLength) - m_WallneighborLength / 2);
                tempWall = Instantiate(m_MazeWall, myPos, Quaternion.identity) as GameObject;
                tempWall.transform.parent = m_MazeWallHolder.transform;
            }
        }
        //Placing y-axis walls
        for (int i = 0; i < m_ySize; i++)
        {
            for (int j = 0; j <= m_xSize; j++)
            {
                myPos = new Vector3(initialPos.x + (j * m_WallneighborLength) - m_WallneighborLength, 0.0f, initialPos.z + (i * m_WallneighborLength));
                tempWall = Instantiate(m_MazeWall, myPos, Quaternion.Euler(0f, 90f, 0f)) as GameObject;
                tempWall.transform.parent = m_MazeWallHolder.transform;

            }
        }

        MakeFloor();
        CreateCells();
    }

    private void MakeFloor()
    {
        //Set floor Pos to line up with the bottom left corner of the maze
        m_FloorPos.x = ((m_MazeWall.transform.localScale.x * m_xSize) / 2) - m_MazeWall.transform.localScale.x;
        m_FloorPos.z = ((m_MazeWall.transform.localScale.x * m_ySize) / 2) - (m_MazeWall.transform.localScale.x / 2);
        m_Floor = Instantiate(m_Floor, m_FloorPos, Quaternion.identity);
        //Set teleFloor Pos to the same coordinates as the regular floor
        Vector3 teleFloorPos = m_TeleFloor.transform.localPosition;
        teleFloorPos.x = m_FloorPos.x;
        teleFloorPos.z = m_FloorPos.z;
        teleFloorPos.y = .001f; //Raise the teleport floor just barely above the normal floor
        m_TeleFloor.transform.localPosition = teleFloorPos;
        //Set Floors scale based on wall scale and amount of walls on each side of the maze
        Vector3 floorScale = m_Floor.transform.localScale;
        floorScale.x = (m_MazeWall.transform.localScale.x / m_FloorScaler) * m_xSize;
        floorScale.z = (m_MazeWall.transform.localScale.x / m_FloorScaler) * m_ySize;
        m_Floor.name = "Maze Floor";
        m_Floor.transform.localScale = floorScale;
        //Set TeleFloor's scale to the same as the regular floor
        Vector3 teleFloorScale = m_TeleFloor.transform.localScale;
        teleFloorScale.x = floorScale.x;
        teleFloorScale.z = floorScale.z;
        m_TeleFloor.transform.localScale = teleFloorScale;
    }

    private void CreateCells()
    {
        //Initialize reference variables
        GameObject[] allWalls;

        int children = m_MazeWallHolder.transform.childCount;
        int eastWestCellInt = 0;
        int termCount = 0;
        int rowNum = 0;

        allWalls = new GameObject[children];
        m_CellArray = new Cell[m_xSize * m_ySize];

        m_TotalCells = m_CellArray.Length;
        //Get all walls and put them in the allWalls array
        for (int i = 0; i< children; i++){
            allWalls[i] = m_MazeWallHolder.transform.GetChild(i).gameObject;
        }

        //Create the cells and assign their walls based on direction
        for(int cellNumber = 0; cellNumber < m_CellArray.Length; cellNumber++)
        {
            //If you have iterated as many times as there is columns
            if (termCount == m_xSize)
            {
                //You are at the end of the row, so the west wall is +1 from your current wall
                eastWestCellInt += 1;
                //You are on a new row
                rowNum++;
                //Reset the term count
                termCount = 0;
            }
            //Create a new cell and begin assigning it's walls
            m_CellArray[cellNumber] = new Cell();

            m_CellArray[cellNumber].visited = false;

            m_CellArray[cellNumber].north = allWalls[cellNumber + m_xSize];
            m_CellArray[cellNumber].south = allWalls[cellNumber];
            m_CellArray[cellNumber].east = allWalls[cellNumber + ((m_xSize * m_ySize) + ((m_xSize + 1) + eastWestCellInt))];
            m_CellArray[cellNumber].west = allWalls[cellNumber + ((m_xSize * m_ySize) + (m_xSize + eastWestCellInt))];

            termCount++;
        }
        ConfigMaze();
        GetCorners();
    }

    private void ConfigMaze()
    {
        
        //While there are still cells that we haven't gotten to
        while(m_VisitedCells < m_TotalCells)
        {
            if (m_StartedBuilding)
            {
                //Assign neighbors to current wall
                FindNeighbors();
                if (m_CellArray[m_CurrentNeighbor].visited == false && m_CellArray[m_CurrentCell].visited == true)
                {
                    //Break the wall that was selected
                    BreakWall();
                    //Chenge the current cell and add the previous cell to the list of visited cells
                    m_CellArray[m_CurrentNeighbor].visited = true;
                    m_VisitedCells++;
                    m_LastCells.Add(m_CurrentCell);
                    m_CurrentCell = m_CurrentNeighbor;
                    //Ensure that we have reference to the last cell we visited
                    if (m_LastCells.Count > 0)
                    {
                        m_BackingUp = m_LastCells.Count - 1;
                    }
                }
            }
            else
            {
                //Initialize the build by selecting a random cell in the maze to begin from
                m_CurrentCell = UnityEngine.Random.Range(0, m_TotalCells);
                m_CellArray[m_CurrentCell].visited = true;
                m_VisitedCells++;
                m_StartedBuilding = true;
            }
        }
        
        MakeMazeExit();

        for(int i = 0; i < m_ObjectiveCount; i++)
        {
            CreatePOI();
            print("Made a POI");
        }
    }
    //Find all corner cells of the maze
    private void GetCorners()
    {
        int firstCorner = 0;
        int secondCorner = m_xSize - 1;
        int thirdCorner = m_xSize * (m_ySize - 1);
        int fourthCorner = (m_xSize * m_ySize) - 1;
        //Add the four corners of the maze to the m_MazeCorners list
        for (int i = 0; i < 4; i++)
        {
            switch (i)
            {
                case 0:
                    m_MazeCorners.Add(firstCorner);
                    break;
                case 1:
                    m_MazeCorners.Add(secondCorner);
                    break;
                case 2:
                    m_MazeCorners.Add(thirdCorner);
                    break;
                case 3:
                    m_MazeCorners.Add(fourthCorner);
                    break;
            }
        }
        //Place the player
       // PlacePlayerInit();
    }
    //Create one opening in the maze
    private void MakeMazeExit()
    {
        int chosenExit = UnityEngine.Random.Range(0, m_Mazeboundires.Count);

        GameObject exitWall = m_Mazeboundires[chosenExit];
        PlaceExitRoom(exitWall);
        Destroy(m_Mazeboundires[chosenExit]);
    }

    private void PlaceExitRoom(GameObject exit)
    {
        var initialPos = exit.transform.position;
        var myPos = new Vector3();
        var wallName = "";
        switch (exit.name)
        {
            case "Northern Bound":
                m_ExitRoom.transform.position = new Vector3(initialPos.x, initialPos.y, initialPos.z + (m_WallneighborLength / 2));
                wallName = "South";
                m_ExitWallHidden = wallName;
                m_ExitRoom.transform.Find(wallName).gameObject.SetActive(false);
                //Destroy(m_ExitRoom.transform.Find("South").gameObject);
                break;
            case "Southern Bound":
                m_ExitRoom.transform.position = new Vector3(initialPos.x, initialPos.y, initialPos.z - (m_WallneighborLength / 2));
                wallName = "North";
                m_ExitWallHidden = wallName;
                m_ExitRoom.transform.Find(wallName).gameObject.SetActive(false);
                //Destroy(m_ExitRoom.transform.Find("North").gameObject);
                break;
            case "Eastern Bound":
                m_ExitRoom.transform.position = new Vector3(initialPos.x + (m_WallneighborLength / 2), initialPos.y, initialPos.z);
                wallName = "West";
                m_ExitWallHidden = wallName;
                m_ExitRoom.transform.Find(wallName).gameObject.SetActive(false);
                //Destroy(m_ExitRoom.transform.Find("West").gameObject);
                break;
            case "Western Bound":
                m_ExitRoom.transform.position = new Vector3(initialPos.x - (m_WallneighborLength / 2), initialPos.y, initialPos.z);
                wallName = "East";
                m_ExitWallHidden = wallName;
                m_ExitRoom.transform.Find(wallName).gameObject.SetActive(false);
                //Destroy(m_ExitRoom.transform.Find("East").gameObject);
                break;
        }

    }
    //Put the player in one of the four corners of the maze
    public void PlacePlayerInit()
    {
        int rand = UnityEngine.Random.Range(0, 4); //Returns a number between a min (inclusive) and a max (exclusive)

        int chosenCorner = m_MazeCorners[rand];

        m_Player.transform.position = CenteredInCell(m_CellArray[chosenCorner], 0);

        m_MazeCorners.RemoveAt(chosenCorner);
    }
    //Spawn POI's on the map
    private void CreatePOI()
    {
        if(m_CellsAvailableForPOI.Count > 0)
        {
            float PedestalHeight = .1f;
            float POIPlacementHeight = 1.7f;
            int chosenCell = UnityEngine.Random.Range(0, (m_CellsAvailableForPOI.Count - 1));

            GameObject newPickup = Instantiate(m_Pickup);
            GameObject newPedestal = Instantiate(m_POIPedestal);
            m_Pedestals.Add(newPedestal);
            m_POIs.Add(newPickup);

            newPickup.transform.position = CenteredInCell(m_CellArray[m_CellsAvailableForPOI[chosenCell]], POIPlacementHeight);
            newPedestal.transform.position = CenteredInCell(m_CellArray[m_CellsAvailableForPOI[chosenCell]], PedestalHeight);

            m_CellsAvailableForPOI.RemoveAt(chosenCell);
        }
        else
        {
            return;
        }
        
    }
    //Based on the value of WallToBreak, break the correct wall of the current cell
    private void BreakWall()
    {
        //Destroy the wall based on it's curren't value 1: North 2: South 3: East 4: West
        switch (m_WallToBreak)
        {
            case 1:
                Destroy(m_CellArray[m_CurrentCell].north);
                break;
            case 2:
                Destroy(m_CellArray[m_CurrentCell].south);
                break;
            case 3:
                Destroy(m_CellArray[m_CurrentCell].east);
                break;
            case 4:
                Destroy(m_CellArray[m_CurrentCell].west);
                break;
        }
    }

    private void FindNeighbors()
    {
        //Setup initial variables
        int neighborLength = 0;
        int check = 0;

        int[] neighbors = new int[4];
        int[] connectingWalls = new int[4];
        //A check to ensure that you are not on the furthest right cell
        check = ((m_CurrentCell + 1) / m_xSize);
        check -= 1;
        check *= m_xSize;
        check += m_xSize;

        //Northern Bounds
        if (m_CurrentCell > ((m_xSize * (m_ySize - 1)) - 1))
        {
            m_Mazeboundires.Add(m_CellArray[m_CurrentCell].north);
            m_CellArray[m_CurrentCell].north.name = "Northern Bound";
            m_MazeBoundsArrayCount++;
        }
        //Southern Bounds
        if (m_CurrentCell < m_xSize)
        {
            m_Mazeboundires.Add(m_CellArray[m_CurrentCell].south);
            m_CellArray[m_CurrentCell].south.name = "Southern Bound";
            m_MazeBoundsArrayCount++;
        }
        //Western Bounds
        if (m_CurrentCell == 0 || IsDivisible(m_CurrentCell, m_xSize))
        {
            m_Mazeboundires.Add(m_CellArray[m_CurrentCell].west);
            m_CellArray[m_CurrentCell].west.name = "Western Bound";
            m_MazeBoundsArrayCount++;
        }
        //Eastern Bounds
        if (m_CurrentCell == m_ySize || (m_CurrentCell + 1) == check)
        {
            m_Mazeboundires.Add(m_CellArray[m_CurrentCell].east);
            m_CellArray[m_CurrentCell].east.name = "Eastern Bound";
            m_MazeBoundsArrayCount++;
        }
        //Northern Neighbor
        if (m_CurrentCell + m_xSize < m_TotalCells)
        {
            if (m_CellArray[m_CurrentCell + m_xSize].visited == false)
            {
                neighbors[neighborLength] = m_CurrentCell + m_xSize;
                connectingWalls[neighborLength] = 1;
                neighborLength++;
            }
        }
        //Southern Neighbor
        if (m_CurrentCell - m_xSize >= 0)
        {
            if (m_CellArray[m_CurrentCell - m_xSize].visited == false)
            {
                neighbors[neighborLength] = m_CurrentCell - m_xSize;
                connectingWalls[neighborLength] = 2;
                neighborLength++;
            }
        }
        //Eastern Neighbor
        if (m_CurrentCell + 1 < m_TotalCells && (m_CurrentCell + 1) != check)
        {
            if (m_CellArray[m_CurrentCell + 1].visited == false)
            {
                neighbors[neighborLength] = m_CurrentCell + 1;
                connectingWalls[neighborLength] = 3;
                neighborLength++;
            }
        }
        //Western Neighbor
        if (m_CurrentCell - 1 >= 0 && m_CurrentCell != check)
        {
            if (m_CellArray[m_CurrentCell - 1].visited == false)
            {
                neighbors[neighborLength] = m_CurrentCell - 1;
                connectingWalls[neighborLength] = 4;
                neighborLength++;
            }
        }
        //If the cell has neighbors
        if (neighborLength != 0)
        {
            //Choose a random neighbor and set it as the current neighbor as well as the current wall we want to remove
            int chosenNeighbor = UnityEngine.Random.Range(0, neighborLength);
            m_CurrentNeighbor = neighbors[chosenNeighbor];
            m_WallToBreak = connectingWalls[chosenNeighbor];
        }
        else
        {
            //If it doesn't find any neighbors, revert to the previous cell we were on and try again
            if(m_BackingUp > 0)
            {
                m_CurrentCell = m_LastCells[m_BackingUp];
                m_BackingUp--;
            }
        }

    }

    #endregion

    #region Utility Functions
    //Check if one number is divisible by another
    private bool IsDivisible(int x, int n)
    {
        return (x % n) == 0;
    }
    //Get the center of a specified cell
    private Vector3 CenteredInCell(Cell thisCell, float height)
    {
        Vector3 cellCenter;

        cellCenter.x = thisCell.north.transform.localPosition.x;
        cellCenter.y = height;
        cellCenter.z = thisCell.north.transform.localPosition.z - (thisCell.north.transform.localScale.x / 2);

        return cellCenter;

    }
    private void RemoveAllRemainingPOIs()
    {
        foreach(GameObject poi in m_POIs)
        {
            Destroy(poi);
        }
        foreach(GameObject pedestal in m_Pedestals)
        {
            Destroy(pedestal);
        }
        m_Pedestals.Clear();
        m_POIs.Clear();
    }

    #endregion

    #region Public Methods
    //Reset all variables and remake the maze
    public void ResetMaze()
    {
        m_StartedBuilding = false;

        m_LastCells.Clear();
        m_Mazeboundires.Clear();
        m_CellsAvailableForPOI.Clear();
        m_MazeCorners.Clear();
        m_CellsAvailableForPOI.Clear();
        Array.Clear(m_CellArray, 0, m_CellArray.Length);

        m_BackingUp = 0;
        m_CurrentNeighbor = 0;
        m_VisitedCells = 0;
        m_CurrentCell = 0;

        Destroy(m_MazeWallHolder);
        Destroy(m_Floor);
        RemoveAllRemainingPOIs();
        m_ExitRoom.transform.Find(m_ExitWallHidden).gameObject.SetActive(true);
        m_MazeWallHolder = new GameObject();
        m_MazeWallHolder.name = "Maze";
        CreateMaze();
    }

    public void PortalTransPlayer()
    {
        int rand = UnityEngine.Random.Range(0, m_CellArray.Length);

        m_Player.transform.position = CenteredInCell(m_CellArray[rand], 0);

    }

    #endregion
}
