﻿

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class Fireman : GameUnit
{
    int AP;
    int savedAP;
    FMStatus status;
    Victim carriedVictim;
    private PhotonView PV;
    private bool isWaitingForInput;
    private bool isExtinguishingFire;
    private bool isChoppingWall;
    private bool isSelectingExtinguishOption;
    ArrayList validInputOptions;
    Space locationArgument;

    static int NumFA = 3;
    static int numVictim = 7;

    void Start()
    {
        AP = 4;
        savedAP = 0;
        carriedVictim = null;
        PV = GetComponent<PhotonView>();
        isWaitingForInput = false;
        isExtinguishingFire = false;
        validInputOptions = new ArrayList();
        isChoppingWall = false;
        isSelectingExtinguishOption = false;
    }

    void Update()
    {


        if (PV.IsMine && GameManager.GM.Turn == PhotonNetwork.LocalPlayer.ActorNumber && GameManager.GameStatus ==
       FlashPointGameConstants.GAME_STATUS_PLAY_GAME)
        {

            //MOVE: ARROWS WITH DIRECTION
            //OPEN/CLOSE DOOR: "D"
            //CHOP WALL "C"
            //END TURN "Q"
            //EXTINGUISH FIRE/SMOKE "E" + Number.

            //NORTH = 0; EAST = 1; SOUTH = 2; WEST = 3
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                this.move(0);
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                this.move(2);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                this.move(1);
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                this.move(3);
            }
            else if (Input.GetKeyDown(KeyCode.D)) //open/close door
            {
                int doorDir = 4;//forbidden value
                Door[] doors = this.getCurrentSpace().getDoors();
                Debug.Log(this.getCurrentSpace().worldPosition);
                for (int i = 0; i < 4; i++)
                {
                    if (doors[i] != null)
                    {
                        doorDir = i;
                    }
                }
                if (doorDir >= 0 && doorDir <= 3)
                {
                    if (doors[doorDir].getDoorStatus() == DoorStatus.Open)
                    {
                        Door door = doors[doorDir];
                        if (this.getAP() >= 1)
                        {
                            decrementAP(1);
                            door.setDoorStatus(DoorStatus.Closed);
                            door.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/closed door");
                            GameConsole.instance.UpdateFeedback("Door closed successfully!");
                        }
                        else
                        {
                            GameConsole.instance.UpdateFeedback("Insufficient AP");
                            return;
                        }
                    }
                    else if (doors[doorDir].getDoorStatus() == DoorStatus.Closed)
                    {
                        Door door = doors[doorDir];
                        if (this.getAP() >= 1)
                        {
                            decrementAP(1);
                            door.setDoorStatus(DoorStatus.Open);
                            door.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Images/open door");
                            GameConsole.instance.UpdateFeedback("Door opened successfully!");
                        }
                        else
                        {
                            GameConsole.instance.UpdateFeedback("Insufficient AP");
                            return;
                        }
                    }
                }
                else
                {
                    GameConsole.instance.UpdateFeedback("there are no doors near the space you're on!");
                }

            }

            else if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Extinguish Fire Detected");
                extinguishFire();
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log("Chop Wall Detected");
                chopWall();
            }
            else if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                if (isWaitingForInput && isExtinguishingFire)
                {
                    Debug.Log("Input 0 Received");
                    isWaitingForInput = false;
                    isExtinguishingFire = false;
                    if (validInputOptions.Contains(0))
                    {
                        Debug.Log("This is a valid extinguish option.");
                        validInputOptions = new ArrayList();
                        Space targetSpace = StateManager.instance.spaceGrid.getNeighborInDirection(this.currentSpace, 0);
                        int numAP = this.getAP();
                        if (targetSpace.getSpaceStatus() == SpaceStatus.Smoke)
                        {
                            this.setAP(numAP - 1);
                            FiremanUI.instance.SetAP(this.getAP());
                            sendSmokeMarkerExtinguishEvent(targetSpace);
                        }
                        else
                        {
                            if (numAP < 2)
                            {
                                GameConsole.instance.UpdateFeedback("Turning Fire To Smoke...");
                                this.setAP(numAP - 1);
                                FiremanUI.instance.SetAP(this.getAP());
                                sendTurnFireMarkerToSmokeEvent(targetSpace);
                            }
                            else
                            {
                                GameConsole.instance.UpdateFeedback("Press Y to extinguish the Fire altogether or N to turn the Fire to smoke");

                                isWaitingForInput = true;
                                isSelectingExtinguishOption = true;
                                locationArgument = targetSpace;
                            }
                        }

                    }
                    else
                    {
                        string oldMessage = GameConsole.instance.FeedbackText.text;
                        GameConsole.instance.UpdateFeedback("Not a valid input. \n" + oldMessage);
                        isWaitingForInput = true;
                        isExtinguishingFire = true;
                    }
                }
                else if (isWaitingForInput && isChoppingWall)
                {
                    Debug.Log("Input 0 Received");
                    isWaitingForInput = false;
                    isChoppingWall = false;
                    if (validInputOptions.Contains(0))
                    {
                        Debug.Log("This is a valid chop wall option.");
                        GameConsole.instance.UpdateFeedback("Chopping wall.");
                        validInputOptions = new ArrayList();
                        Space targetSpace = this.getCurrentSpace();
                        this.setAP(this.getAP() - 2);
                        FiremanUI.instance.SetAP(this.getAP());
                        sendChopWallEvent(targetSpace, 0);
                    }
                    else
                    {
                        string oldMessage = GameConsole.instance.FeedbackText.text;
                        GameConsole.instance.UpdateFeedback("Not a valid input. \n" + oldMessage);
                        isWaitingForInput = true;
                        isChoppingWall = true;
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                if (isWaitingForInput && isExtinguishingFire)
                {
                    Debug.Log("Input 1 Received");
                    isWaitingForInput = false;
                    isExtinguishingFire = false;
                    if (validInputOptions.Contains(1))
                    {
                        Debug.Log("This is a valid extinguish option.");
                        validInputOptions = new ArrayList();
                        Space targetSpace = StateManager.instance.spaceGrid.getNeighborInDirection(this.currentSpace, 1);
                        int numAP = this.getAP();
                        if (targetSpace.getSpaceStatus() == SpaceStatus.Smoke)
                        {
                            this.setAP(numAP - 1);
                            FiremanUI.instance.SetAP(this.getAP());
                            sendSmokeMarkerExtinguishEvent(targetSpace);
                        }
                        else
                        {
                            if (numAP < 2)
                            {
                                GameConsole.instance.UpdateFeedback("Turning Fire To Smoke...");
                                this.setAP(numAP - 1);
                                FiremanUI.instance.SetAP(this.getAP());
                                sendTurnFireMarkerToSmokeEvent(targetSpace);
                            }
                            else
                            {
                                GameConsole.instance.UpdateFeedback("Press Y to extinguish the Fire altogether or N to turn the Fire to smoke");

                                isWaitingForInput = true;
                                isSelectingExtinguishOption = true;
                                locationArgument = targetSpace;
                            }
                        }

                    }
                    else
                    {
                        string oldMessage = GameConsole.instance.FeedbackText.text;
                        GameConsole.instance.UpdateFeedback("Not a valid input. \n" + oldMessage);
                        isWaitingForInput = true;
                        isExtinguishingFire = true;
                    }
                }
                else if (isWaitingForInput && isChoppingWall)
                {
                    Debug.Log("Input 1 Received");
                    isWaitingForInput = false;
                    isChoppingWall = false;
                    if (validInputOptions.Contains(1))
                    {
                        Debug.Log("This is a valid chop wall option.");
                        GameConsole.instance.UpdateFeedback("Chopping wall.");
                        validInputOptions = new ArrayList();
                        Space targetSpace = this.getCurrentSpace();
                        this.setAP(this.getAP() - 2);
                        FiremanUI.instance.SetAP(this.getAP());
                        sendChopWallEvent(targetSpace, 1);
                    }
                    else
                    {
                        string oldMessage = GameConsole.instance.FeedbackText.text;
                        GameConsole.instance.UpdateFeedback("Not a valid input. \n" + oldMessage);
                        isWaitingForInput = true;
                        isChoppingWall = true;
                    }
                }
            }

            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                if (isWaitingForInput && isExtinguishingFire)
                {
                    Debug.Log("Input 2 Received");
                    isWaitingForInput = false;
                    isExtinguishingFire = false;
                    if (validInputOptions.Contains(2))
                    {
                        Debug.Log("This is a valid extinguish option.");
                        validInputOptions = new ArrayList();
                        Space targetSpace = StateManager.instance.spaceGrid.getNeighborInDirection(this.currentSpace, 2);
                        int numAP = this.getAP();
                        if (targetSpace.getSpaceStatus() == SpaceStatus.Smoke)
                        {
                            this.setAP(numAP - 1);
                            FiremanUI.instance.SetAP(this.getAP());
                            sendSmokeMarkerExtinguishEvent(targetSpace);
                        }
                        else
                        {
                            if (numAP < 2)
                            {
                                GameConsole.instance.UpdateFeedback("Turning Fire To Smoke...");
                                this.setAP(numAP - 1);
                                FiremanUI.instance.SetAP(this.getAP());
                                sendTurnFireMarkerToSmokeEvent(targetSpace);
                            }
                            else
                            {
                                GameConsole.instance.UpdateFeedback("Press Y to extinguish the Fire altogether or N to turn the Fire to smoke");

                                isWaitingForInput = true;
                                isSelectingExtinguishOption = true;
                                locationArgument = targetSpace;
                            }
                        }

                    }
                    else
                    {
                        string oldMessage = GameConsole.instance.FeedbackText.text;
                        GameConsole.instance.UpdateFeedback("Not a valid input. \n" + oldMessage);
                        isWaitingForInput = true;
                        isExtinguishingFire = true;
                    }
                }
                else if (isWaitingForInput && isChoppingWall)
                {
                    Debug.Log("Input 2 Received");
                    isWaitingForInput = false;
                    isChoppingWall = false;
                    if (validInputOptions.Contains(2))
                    {
                        Debug.Log("This is a valid chop wall option.");
                        GameConsole.instance.UpdateFeedback("Chopping wall.");
                        validInputOptions = new ArrayList();
                        Space targetSpace = this.getCurrentSpace();
                        this.setAP(this.getAP() - 2);
                        FiremanUI.instance.SetAP(this.getAP());
                        sendChopWallEvent(targetSpace, 2);
                    }
                    else
                    {
                        string oldMessage = GameConsole.instance.FeedbackText.text;
                        GameConsole.instance.UpdateFeedback("Not a valid input. \n" + oldMessage);
                        isWaitingForInput = true;
                        isChoppingWall = true;
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                if (isWaitingForInput && isExtinguishingFire)
                {
                    Debug.Log("Input 3 Received");
                    isWaitingForInput = false;
                    isExtinguishingFire = false;
                    if (validInputOptions.Contains(3))
                    {
                        Debug.Log("This is a valid extinguish option.");
                        validInputOptions = new ArrayList();
                        Space targetSpace = StateManager.instance.spaceGrid.getNeighborInDirection(this.currentSpace, 3);
                        int numAP = this.getAP();
                        if (targetSpace.getSpaceStatus() == SpaceStatus.Smoke)
                        {
                            this.setAP(numAP - 1);
                            FiremanUI.instance.SetAP(this.getAP());
                            sendSmokeMarkerExtinguishEvent(targetSpace);
                        }
                        else
                        {
                            if (numAP < 2)
                            {
                                GameConsole.instance.UpdateFeedback("Turning Fire To Smoke...");
                                this.setAP(numAP - 1);
                                FiremanUI.instance.SetAP(this.getAP());
                                sendTurnFireMarkerToSmokeEvent(targetSpace);
                            }
                            else
                            {
                                GameConsole.instance.UpdateFeedback("Press Y to extinguish the Fire altogether or N to turn the Fire to smoke");

                                isWaitingForInput = true;
                                isSelectingExtinguishOption = true;
                                locationArgument = targetSpace;
                            }
                        }

                    }
                    else
                    {
                        string oldMessage = GameConsole.instance.FeedbackText.text;
                        GameConsole.instance.UpdateFeedback("Not a valid input. \n" + oldMessage);
                        isWaitingForInput = true;
                        isExtinguishingFire = true;
                    }
                }
                else if (isWaitingForInput && isChoppingWall)
                {
                    Debug.Log("Input 3 Received");
                    isWaitingForInput = false;
                    isChoppingWall = false;
                    if (validInputOptions.Contains(3))
                    {
                        Debug.Log("This is a valid chop wall option.");
                        GameConsole.instance.UpdateFeedback("Chopping wall.");
                        validInputOptions = new ArrayList();
                        Space targetSpace = this.getCurrentSpace();
                        this.setAP(this.getAP() - 2);
                        FiremanUI.instance.SetAP(this.getAP());
                        sendChopWallEvent(targetSpace, 3);
                    }
                    else
                    {
                        string oldMessage = GameConsole.instance.FeedbackText.text;
                        GameConsole.instance.UpdateFeedback("Not a valid input. \n" + oldMessage);
                        isWaitingForInput = true;
                        isChoppingWall = true;
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                if (isWaitingForInput && isExtinguishingFire)
                {
                    Debug.Log("Input 4 Received");
                    isWaitingForInput = false;
                    isExtinguishingFire = false;
                    if (validInputOptions.Contains(4))
                    {
                        Debug.Log("This is a valid extinguish option.");
                        validInputOptions = new ArrayList();
                        Space targetSpace = StateManager.instance.spaceGrid.getNeighborInDirection(this.currentSpace, 4);
                        int numAP = this.getAP();
                        if (targetSpace.getSpaceStatus() == SpaceStatus.Smoke)
                        {
                            this.setAP(numAP - 1);
                            FiremanUI.instance.SetAP(this.getAP());
                            sendSmokeMarkerExtinguishEvent(targetSpace);
                        }
                        else
                        {
                            if (numAP < 2)
                            {
                                GameConsole.instance.UpdateFeedback("Turning Fire To Smoke...");
                                this.setAP(numAP - 1);
                                FiremanUI.instance.SetAP(this.getAP());
                                sendTurnFireMarkerToSmokeEvent(targetSpace);
                            }
                            else
                            {
                                GameConsole.instance.UpdateFeedback("Press Y to extinguish the Fire altogether or N to turn the Fire to smoke");

                                isWaitingForInput = true;
                                isSelectingExtinguishOption = true;
                                locationArgument = targetSpace;
                            }
                        }

                    }
                    else
                    {
                        string oldMessage = GameConsole.instance.FeedbackText.text;
                        GameConsole.instance.UpdateFeedback("Not a valid input. \n" + oldMessage);
                        isWaitingForInput = true;
                        isExtinguishingFire = true;
                    }
                }
            }


            else if (Input.GetKeyDown(KeyCode.Q))
            {
                endTurn();
            }
            else if (Input.GetKeyDown(KeyCode.Y))
            {
                if (isWaitingForInput && isSelectingExtinguishOption)
                {
                    sendFireMarkerExtinguishEvent(locationArgument);
                    locationArgument = null;
                    isWaitingForInput = false;
                    isSelectingExtinguishOption = false;
                    this.setAP(this.getAP() - 2);
                    FiremanUI.instance.SetAP(this.getAP());
                }
            }
            else if (Input.GetKeyDown(KeyCode.N))
            {
                if (isWaitingForInput && isSelectingExtinguishOption)
                {
                    sendTurnFireMarkerToSmokeEvent(locationArgument);
                    locationArgument = null;
                    isWaitingForInput = false;
                    isSelectingExtinguishOption = false;
                    this.setAP(this.getAP() - 1);
                    FiremanUI.instance.SetAP(this.getAP());
                }

            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                carryVictim();
            }
        }
    }

    public int getAP()
    {
        return AP;
    }

    public void setAP(int newAP)
    {
        AP = newAP;
    }

    public int getSavedAP()
    {
        return AP;
    }


    public void decrementAP(int amount)
    {
        this.AP -= amount;
    }

    public void setAPStartTurn()
    {
        AP = getSavedAP() + 4;
    }

    public FMStatus getStatus()
    {
        return this.status;
    }

    public void setStatus(FMStatus newStatus)
    {
        this.status = newStatus;
    }

    public Victim getVictim()
    {
        return this.carriedVictim;
    }

    public void setVictim(Victim v)
    {
        this.carriedVictim = v;
    }

    public void deassociateVictim()
    {
        this.carriedVictim = null;
    }

    public void extinguishFire()
    {
        int numAP = getAP(); //returns the number of action points

        //Get current space and spacestatus. 

        Space current = this.getCurrentSpace();
        SpaceStatus currentSpaceStatus = current.getSpaceStatus();


        //Get neighbors and their spacestatus. 
        Space[] neighbors = StateManager.instance.spaceGrid.GetNeighbours(current);
        SpaceStatus[] neighborsStatuses = new SpaceStatus[4];

        for (int i = 0; i < neighbors.Length; i++)
        {
            if (neighbors[i] != null)
            {
                neighborsStatuses[i] = neighbors[i].getSpaceStatus();
            }

        }
         

        //Check if sufficient AP.
        if (numAP < 1)
        {
            Debug.Log("Not enough AP!");  //Used to show the player why he can’t perform an action in case of failure
            GameConsole.instance.UpdateFeedback("Not enough AP!");
        }
        else
        {
            //Get indices of all spaces accessible that are not safe (valid neighbors + current Space).
            ArrayList extinguishOptions = getUnsafeSpacesIndecies(currentSpaceStatus, neighborsStatuses);
            validInputOptions = extinguishOptions;

            //Build string to show.
            string optionsToUser = "";

            foreach (int index in extinguishOptions) {

               
                if (index == 0)
                {
                    optionsToUser += "Press 0 for Tile on Top ";
                }
                else if (index == 1)
                {
                    optionsToUser += " Press 1 for Tile to Your Right";
                }
                else if (index == 2)
                {
                    optionsToUser += " Press 2 for the Tile to the Bottom";
                }
                else if (index == 3)
                {
                    optionsToUser += " Press 3 for the Tile to Your Left";

                }
                else
                {
                    optionsToUser += " Press 4 for the current Tile";

                }
            }

            GameConsole.instance.UpdateFeedback(optionsToUser);

            isWaitingForInput = true;
            isExtinguishingFire = true;

        }
    }

    private ArrayList getUnsafeSpacesIndecies(SpaceStatus currentSpaceStatus, SpaceStatus[] neighborsStatuses)
    {
        ArrayList indices = new ArrayList();

        //Collect directions in which there is a smoke or fire marker.
        for (int i = 0; i < neighborsStatuses.Length; i++)
        {
            if (neighborsStatuses[i] != SpaceStatus.Safe)
            {
                indices.Add(i);
            }
        }

        //Check for current space. Current Space index will be 4.
        if (currentSpaceStatus != SpaceStatus.Safe)
        {
            indices.Add(4);
        }

        return indices;
    }



    public void chopWall()     {         int numAP = getAP(); //returns the number of action points          //Check if sufficient AP.         if (numAP < 2)         {             Debug.Log("Not enough AP!");  //Used to show the player why he can’t perform an action in case of failure             GameConsole.instance.UpdateFeedback("Not enough AP!");         }         else         {             //Get indices of all spaces accessible that are not safe (valid neighbors + current Space).             ArrayList nearbyWalls = getNearbyWalls(this.getCurrentSpace());             validInputOptions = nearbyWalls;              //Build string to show.             string optionsToUser = "";              foreach (int index in nearbyWalls)             {                   if (index == 0)                 {                     optionsToUser += "Press 0 for the Wall on Top ";                 }                 else if (index == 1)                 {                     optionsToUser += " Press 1 for the Wall to Your Right";                 }                 else if (index == 2)                 {                     optionsToUser += " Press 2 for the Wall to the Bottom";                 }                 else if (index == 3)                 {                     optionsToUser += " Press 3 for the Wall to Your Left";                  }             }              GameConsole.instance.UpdateFeedback(optionsToUser);              isWaitingForInput = true;
            isChoppingWall = true;          }

        if(GameManager.GM.buildingDamage >= 24)
        {
            //Building colapses
            Debug.Log("u just lost YIKESSS");
            GameManager.GameLost();

        }     }      private ArrayList getNearbyWalls(Space s)     {         ArrayList nearbyWalls = new ArrayList();         Wall[] wallArray = s.getWalls();          //Collect directions in which there is a wall         for (int i = 0; i < wallArray.Length; i++)         {             if (wallArray[i] != null)             {                 nearbyWalls.Add(i);             }         }         return nearbyWalls;     } 


    public void carryVictim()
    {
        //get current space
        Space current = this.getCurrentSpace();

        if (this.getVictim() != null)
        {
            GameConsole.instance.UpdateFeedback("You are already carrying a victim!");
            return;
        }
        else
        {

            List<GameUnit> gameUnits = current.getOccupants();

            foreach (GameUnit gu in gameUnits)
            {
                //if has POI marker
                if (gu.getType() == FlashPointGameConstants.GAMEUNIT_TYPE_POI)
                {
                    Victim v = gu.GetComponent<Victim>();
                    this.setVictim(v);
                    GameConsole.instance.UpdateFeedback("Carried victim successfully!");
                    return;
                }
            }
            GameConsole.instance.UpdateFeedback("There is no victim to be carried!");
        }

    }


    public void move(int direction)
    {
        /*revealVictim(); TODO
         * make an if statement to make sure if the fireman moves into a space with POI marker
         */

        //TODO NEED TO KNOW IF F HAS ENOUGH AP TO MOVE TO A SAFE SPACE
        int ap = this.getAP();
        Victim v = this.getVictim();
        bool reachable = true; //destination.isReachable(); //TODO
        Space curr = this.getCurrentSpace();
        Debug.Log("Index X is " + curr.indexX + " and Index Y is " + curr.indexY);
        Space[] neighbors = StateManager.instance.spaceGrid.GetNeighbours(curr);
        //foreach (Space s in neighbors)
        //{
        //    if (s != null)
        //    {
        //        Debug.Log("not null yeayeay");
        //    }
        //}
        Space destination = neighbors[direction];

        if (destination == null)
        {
            GameConsole.instance.UpdateFeedback("Invalid move. Please try again");
            return;
        }


        SpaceStatus sp = destination.getSpaceStatus();

        if (reachable)
        {
            if (sp == SpaceStatus.Fire)
            {
                if (ap >= 2 && v == null) //&&f has enough to move
                {
                    Debug.Log(ap);
                    Debug.Log(this.transform.position);
                    this.setCurrentSpace(destination);
                    this.decrementAP(2);
                    FiremanUI.instance.SetAP(this.AP);
                    Vector3 newPosition = new Vector3(destination.worldPosition.x, destination.worldPosition.y, -10);
                    this.GetComponent<Transform>().position = newPosition;
                }
                else
                {
                    GameConsole.instance.UpdateFeedback("Insufficient AP");
                    return;
                }
            }
            else
            {
                if (v == null && ap >= 1)
                {
                    this.setCurrentSpace(destination);
                    this.decrementAP(1);
                    FiremanUI.instance.SetAP(this.AP);
                    GameConsole.instance.UpdateFeedback("You have successfully moved");
                    Vector3 newPosition = new Vector3(destination.worldPosition.x, destination.worldPosition.y, -10);
                    this.GetComponent<Transform>().position = newPosition;
                    List<GameUnit> gameUnits = destination.getOccupants();
                    foreach (GameUnit gu in gameUnits)
                    {
                        if (gu.getType() == FlashPointGameConstants.GAMEUNIT_TYPE_POI)
                        {
                            if (gu.GetComponent<POI>().getIsFlipped() == false)
                            {
                                FlipPOI();
                                break;
                            }
                        }
                    }

                }
                else if (v != null && ap >= 2)//if the fireman is carrying a victim
                {
                    SpaceStatus destinationSpaceStatus = destination.getSpaceStatus();

                    SpaceKind destinationSpaceKind = destination.getSpaceKind();


                    if ((destinationSpaceStatus == SpaceStatus.Safe && destinationSpaceKind == SpaceKind.Indoor) || destinationSpaceStatus == SpaceStatus.Smoke)
                    {
                        //carry victim
                        Vector3 newPosition = new Vector3(destination.worldPosition.x, destination.worldPosition.y, -10);

                        this.setCurrentSpace(destination);
                        v.setCurrentSpace(destination);
                        this.decrementAP(2);
                        FiremanUI.instance.SetAP(this.AP);
                        this.GetComponent<Transform>().position = newPosition;
                        v.GetComponent<Transform>().position = newPosition;

                        //removing the victim from the current space.
                        List<GameUnit> currentGameUnits = curr.getOccupants();
                        GameUnit victim = null;
                        foreach (GameUnit gu in currentGameUnits)
                        {
                            if (gu != null && gu.getType() == FlashPointGameConstants.GAMEUNIT_TYPE_POI)
                            {
                                victim = gu;
                                break;
                            }
                        }
                        currentGameUnits.Remove(victim);
                        destination.addOccupant(victim);
                        

                        GameConsole.instance.UpdateFeedback("You have successfully moved with a victim");
                        //if has POI marker
                        List<GameUnit> destinationGameUnits = destination.getOccupants();
                        foreach (GameUnit gu in destinationGameUnits)
                        {
                            if (gu != null && gu.getType() == FlashPointGameConstants.GAMEUNIT_TYPE_POI)
                            {
                                if (gu.GetComponent<POI>().getIsFlipped() == false)
                                {
                                    FlipPOI();
                                    break;
                                }
                            }
                        }
                    }
                    else if (destinationSpaceKind == SpaceKind.Outdoor)
                    {
                        //carry victim outside the building
                        this.setCurrentSpace(destination);
                        this.decrementAP(2);
                        Vector3 newPosition = new Vector3(destination.worldPosition.x, destination.worldPosition.y, -10);
                        this.GetComponent<Transform>().position = newPosition;

                        //change victim status to rescued
                        Debug.Log("YOU ARE OUTDOOR");
                        v.setVictimStatus(VictimStatus.Rescued);
                        Debug.Log("victim status after rescuing:" + v.getVictimStatus());
                        GameManager.savedVictims++;
                        GameUI.instance.AddSavedVictim();

                        Debug.Log("Removing POI Marker");
                        List<GameUnit> gameUnits = curr.getOccupants();
                        GameUnit victim = null;
                        foreach (GameUnit gu in gameUnits)
                        {
                            if (gu != null && gu.getType() == FlashPointGameConstants.GAMEUNIT_TYPE_POI)
                            {
                                victim = gu;
                                break;
                            }
                        }
                        gameUnits.Remove(victim);
                        Destroy(victim.physicalObject);
                        Destroy(victim);
                        deassociateVictim();
                        GameManager.numOfActivePOI--;
                        GameConsole.instance.UpdateFeedback("You have successfully rescued a victim");

                        //check if we won the game.
                        if (GameManager.savedVictims >= 7)
                        {
                            //check for a perfect game
                            if(GameManager.savedVictims == 10)
                            {
                                GameManager.GameWon();
                                System.Environment.Exit(0);
                            }
                            GameManager.GameWon();
                        }
                        return;



                    }
                    else //Fire
                    {
                        //can not carry victim
                        GameConsole.instance.UpdateFeedback("Cannot carry a victim onto fire!");
                        return;
                    }
                    
                }
            }
        }
        else
        {
            GameConsole.instance.UpdateFeedback("Insufficient AP");
            return;
        }

                //after the move TODO??

        //        List<GameUnit> occ = destination.getOccupants();
        //foreach (GameUnit gu in occ)
        //{
        //    if (gu is POI)
        //    {
        //        POIKind gukind = ((POI)gu).getPOIKind();
        //        if (gukind == POIKind.FalseAlarm)
        //        {
        //            //TODO remove false alarm
        //        }
        //    }
        //}
        
        //if (v != null && destination.getSpaceKind() == SpaceKind.Outdoor)
        //{
        //    v.setVictimStatus(VictimStatus.Rescued);
        //    //place victim marker on the rescued space 

        //    Game.incrementNumSavedVictims();
        //    GameUI.instance.AddSavedVictim();
        //    this.deassociateVictim();
        //    if (Game.getNumSavedVictims() >= 7)
        //    {
        //        Game.setGameWon(true);
        //        Game.setGameState(GameState.Completed);
        //        GameUI.instance.AddGameState("Completed");
        //    }
        //}
    }


    public void KnockedDown()
    {
        //A Firefighter is Knocked Down when Fire advances into their space; this could be from an explosion or being in a Smoke
        //filled space that ignites

        //if: KnockedDown
        //take the Firefighter from its space
        //place it on the closest (as the crow flies) Ambulance Parking Spot outside the building
        //if: two Parking Spots are equally distant, choose one


        //Leave the Fire marker in the space

        //if: the KnockedDown Firefighter was carrying a Victim
        //Victim is Lost --> Place the Victim marker on the Lost space at the edge of the board
        //make a function call to VictimLoss
    }

    public void openDoor()
    {
        Debug.Log("open door");
        if (getAP() >= 1)
        {
            decrementAP(1);
            Door[] doors = this.getCurrentSpace().getDoors();
            foreach (Door d in doors)
            {
                d.setDoorStatus(DoorStatus.Open);
            }
            string doorObjectPath = "Board/doorCol45";
            GameObject.Find(doorObjectPath).GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("PhotonPrefabs/open door");
        }
    }

    public void closeDoor()
    {
        Debug.Log("close door");
        if (getAP() >= 1)
        {
            decrementAP(1);
            Door[] doors = this.getCurrentSpace().getDoors();
            foreach (Door d in doors)
            {
                d.setDoorStatus(DoorStatus.Closed);
            }
            string doorObjectPath = "Board/doorCol45";
            GameObject.Find(doorObjectPath).GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("PhotonPrefabs/closed door");
        }
    }

    private void sendFireMarkerExtinguishEvent(Space targetSpace)
    {
        int targetX = targetSpace.indexX;
        int targetY = targetSpace.indexY;

        object[] data = new object[] { targetSpace.indexX, targetSpace.indexY };

        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.RemoveFireMarker, data, GameManager.sendToAllOptions, SendOptions.SendReliable);
    }

    private void sendSmokeMarkerExtinguishEvent(Space targetSpace)
    {
        int targetX = targetSpace.indexX;
        int targetY = targetSpace.indexY;

        object[] data = new object[] { targetSpace.indexX, targetSpace.indexY };
    

        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.RemoveSmokeMarker, data, GameManager.sendToAllOptions, SendOptions.SendReliable);
        
    }

    private void sendTurnFireMarkerToSmokeEvent(Space targetSpace)
    {
        object[] dataRemoveFireMarker = new object[] { targetSpace.indexX, targetSpace.indexY };
        object[] dataAdvanceSmokeMarker = new object[] { targetSpace.worldPosition, targetSpace.indexX, targetSpace.indexY };
        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.RemoveFireMarker, dataRemoveFireMarker, GameManager.sendToAllOptions, SendOptions.SendReliable);
        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.AdvanceSmokeMarker, dataAdvanceSmokeMarker, GameManager.sendToAllOptions, SendOptions.SendReliable);
    }


    private void sendChopWallEvent(Space targetSpace, int direction)
    {
        int indexX = targetSpace.indexX;
        int indexY = targetSpace.indexY;

        object[] data = new object[] { indexX, indexY, direction };
        PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.ChopWall, data, GameManager.sendToAllOptions, SendOptions.SendReliable);
    }

    public void replenishPOI()
    {
        if (GameManager.totalPOIs == 0)
        {
            return;
        }
        switch (GameManager.numOfActivePOI)
        {
            case 0:
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.PlacePOI, null, GameManager.sendToAllOptions, SendOptions.SendReliable);
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.PlacePOI, null, GameManager.sendToAllOptions, SendOptions.SendReliable);
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.PlacePOI, null, GameManager.sendToAllOptions, SendOptions.SendReliable);
                GameManager.totalPOIs -= 3;
                break;
            case 1:
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.PlacePOI, null, GameManager.sendToAllOptions, SendOptions.SendReliable);
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.PlacePOI, null, GameManager.sendToAllOptions, SendOptions.SendReliable);
                GameManager.totalPOIs -= 2;
                break;
            case 2:
                PhotonNetwork.RaiseEvent((byte)PhotonEventCodes.PlacePOI, null, GameManager.sendToAllOptions, SendOptions.SendReliable);
                GameManager.totalPOIs -= 1;
                break;
            default:
                break;
        }
    }

    public void endTurn()
    {
        restoreAP();
        GameManager.advanceFire();
        replenishPOI();
        GameManager.IncrementTurn();
    }


    private void restoreAP()
    {
       
        int currentNumAP = this.getAP();
        int newAP = Mathf.Min(currentNumAP + 4, 8);


        this.setAP(newAP);
        FiremanUI.instance.SetAP(newAP);
    }

    public void FlipPOI () {
        Debug.Log("Flip");
        string[] mylist = new string[] {
            "man POI", "woman POI", "false alarm", "dog POI"
        };
        Debug.Log("Length of array is :" + mylist.Length);
        Space curr = this.getCurrentSpace();
        List<GameUnit> gameUnits = curr.getOccupants();
        GameUnit questionMark = null;
        foreach (GameUnit gu in gameUnits)
        {
            if(gu.getType() == FlashPointGameConstants.GAMEUNIT_TYPE_POI){
                questionMark = gu;
                break;
            }
        }
        Vector3 position = new Vector3(curr.worldPosition.x,curr.worldPosition.y,-5);
        int r;

        while(true)
        {   
            r = Random.Range(0, mylist.Length - 1);
            if(string.Compare(mylist[r],"false alarm") == 0 && NumFA <= 0)
                continue;
            else
            {
                if (numVictim <= 0)
                    continue;
            }
            break;
        }

        string POIname = mylist[r];
        if(string.Compare(POIname,"false alarm") == 0)
        {
            NumFA--;
            gameUnits.Remove(questionMark);
            Destroy(questionMark.physicalObject);
            Destroy(questionMark);
            GameConsole.instance.UpdateFeedback("It was a false alarm!");
            Debug.Log("After revealing FalseAlarm, numFa is: " + NumFA);
            GameManager.numOfActivePOI--;
            return;
        }
        else
        {
            GameConsole.instance.UpdateFeedback("It was a Victim!");
            numVictim--;
            Debug.Log("After revealing Victim, numVictim is: " + numVictim);
        }
        //Instiate Object
        GameObject poi = Instantiate (Resources.Load ("PhotonPrefabs/Prefabs/POIs/" + POIname ) as GameObject);

        poi.GetComponent<POI>().setPOIKind(POIKind.Victim);
        poi.GetComponent<POI>().setIsFlipped(true);
        poi.GetComponent<Transform>().position = position;
        poi.GetComponent<GameUnit>().setCurrentSpace(curr);
        poi.GetComponent<GameUnit>().setType(FlashPointGameConstants.GAMEUNIT_TYPE_POI);
        poi.GetComponent<GameUnit>().setPhysicalObject(poi);

        //NOT DONE : poi.GetComponent<Victim>().setPhysicalObject(poi)
        gameUnits.Remove(questionMark);
        curr.addOccupant(poi.GetComponent<GameUnit>());
        Destroy(questionMark.physicalObject);
        Destroy(questionMark);
        
    }


    //  =============== NETWORK SYNCRONIZATION SECTION ===============
    public void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnEvent;
    }

    public void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnEvent;
    }

    public void OnEvent(EventData eventData)
    {
        byte evCode = eventData.Code;

        //Move = 5
        if (evCode == (byte)PhotonEventCodes.Move)
        {
            object[] data = eventData.CustomData as object[];

            if (data.Length == 3)
            {
                if ((int)data[0] == PV.ViewID)
                {
                    //do stuff here
                }
            }
        }
        //Door = 6
        if (evCode == (byte)PhotonEventCodes.Door)
        {
            object[] data = eventData.CustomData as object[];

        }
    }
}

