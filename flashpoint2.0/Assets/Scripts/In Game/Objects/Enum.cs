﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FMStatus
{
    FamilyFireFighter,
    ExperiencedFirefighter
}

public enum WallStatus
{
    Intact,
    Damaged,
    Destroyed
}

public enum POIKind
{
    Victim,
    FalseAlarm
}

public enum VictimStatus
{
    Unconscious,
    Treated,
    Rescued,
    Lost
}

public enum SpaceKind
{
    Indoor,
    Outdoor
}

public enum Kind
{
    Empty,
    ParkingSpot,
    Victim,
    FalseAlarm
}

public enum Rules
{
    FamilyMode,
    ExperiencedMode
}

public enum GameState
{
    ReadyToJoin,
    NotReadyToJoin,
    KnockedDownPlacement,
    Completed
}

public enum DoorStatus
{
    Open,
    Closed,
    Destroyed
}

public enum SpaceStatus
{
    Safe,
    Smoke,
    Fire
}

public enum PlayerStatus
{
    Ready,
    NotReady
}

public enum Direction
{
    North,
    East,
    West,
    South
}

public enum Action
{
    PlaceFire,
    FlipSmoke,
    FlipFire,
    RemoveSmoke,
    RemoveFire,
    Move,
    OpenDoor,
    CloseDoor,
    ChopWall,
    ExtinguishFire,
    CarryVictim,

} 