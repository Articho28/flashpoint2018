﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Victim : POI
{
    VictimStatus status;
    public bool carried;

    void createVictim()
    {
        //TODO
    }

    public VictimStatus getVictimStatus() {
        return this.status;
    }

    public void setVictimStatus(VictimStatus newStatus)
    {
        this.status = newStatus;
    }

}
