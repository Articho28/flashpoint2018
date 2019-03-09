﻿using UnityEngine;

public class Space {
    public Vector3 worldPosition;
    public bool isOutside;
    public int indexX;
    public int indexY;
    Door d1;
    Wall w1;

    public Space(Vector2 _worldPos, bool _isOutside, int _indexX, int _indexY) {
        worldPosition = new Vector3(_worldPos.x, _worldPos.y, 0);
        isOutside = _isOutside;
        indexX = _indexX;
        indexY = _indexY;
    }
}