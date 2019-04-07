﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PhotonEventCodes
{
    fireFighterPlaced = 0,
    IncrementTurn = 1,
    PlaceInitialFireFighter = 2,
    PlacePOI = 3,
    PlaceInitialFireMarker = 4,
    Move = 5,
    Door = 6,
    AdvanceFireMarker = 7,
    AdvanceSmokeMarker = 8,
    BoardSetup = 9,
    RemoveFireMarker = 10,
    RemoveSmokeMarker = 11,
    ChopWall = 12,
    ResolveFlashOvers = 13,
    ResolveExplosion = 14,
    PlaceHazmats = 15,
    PlaceInitialFireMarkerExperienced = 16,
    PlaceInitialHotSpot = 17,
    FlipPOI = 18,
    ReplenishPOI = 19,
    PlaceVehicles = 20,
    KnockdownFireman = 21,
    CachePlayerNames = 22,
    PlaceInitialAmbulance = 23,
    PlaceInitialEngine = 24,
    PickSpecialist = 25,
    PlaceAmbulanceParkingSpot = 26,
    PlaceEngineParkingSpot = 27,
    EndTurn = 28,
    ChangeCrew = 29,
    SpecialistIsPicked = 30,
    DriveAmbulance = 31,
    DriveEngine = 32,
    RideAmbulance = 33,
    RideEngine = 34,
    UpdateCarriedVictimsState = 35
}
