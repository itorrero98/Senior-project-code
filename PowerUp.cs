using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp {

    private int powerUpIndex; //Used for reference in player's power up list

    //Initialize PowerUp script
	public PowerUp(int index)
    {
        powerUpIndex = index;
    }
    //Set read-only property for index
    public int Index
    {
        get { return powerUpIndex; }
    }
}
