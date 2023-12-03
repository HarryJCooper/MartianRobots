using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Robot
{
    public int x;
    public int y;
    public char direction;
    private GameObject _robotObject;
    private RectTransform _rectTransform;
    private int _mapScaleX = 100, _mapScaleY = 100;
    
    public Robot(int x, int y, char direction, GameObject robotObject, int mapScaleX = 100, int mapScaleY = 100)
    {
        this.x = x;
        this.y = y;
        this.direction = direction;
        this._robotObject = robotObject;
        this._rectTransform = robotObject.GetComponent<RectTransform>();
        this._mapScaleX = mapScaleX;
        this._mapScaleY = mapScaleY;
        UpdateRotation();
        _robotObject.GetComponent<Image>().color = new Color(Random.Range(0, 1f),Random.Range(0f, 1f),Random.Range(0f, 1f), 1f);
    }

    private void UpdateRotation()
    {
        float zRotation = 0;
        switch (direction){
            case 'N': zRotation = 0; break;
            case 'E': zRotation = -90; break;
            case 'S': zRotation = 180; break;
            case 'W': zRotation = 90; break;
        }
        _rectTransform.localEulerAngles = new Vector3(0, 0, zRotation);
    }

    public void MoveForward()
    {
        if (direction == 'N') y++;
        else if (direction == 'E') x++;
        else if (direction == 'S') y--;
        else if (direction == 'W') x--;
        _rectTransform.anchoredPosition = new Vector2(x * _mapScaleX, y * _mapScaleY);
    }

    public void MoveBackward()
    {
        if (direction == 'N') y--;
        else if (direction == 'E') x--;
        else if (direction == 'S') y++;
        else if (direction == 'W') x++;
        _rectTransform.anchoredPosition = new Vector2(x * _mapScaleX, y * _mapScaleY);
    }

    public void TurnLeft()
    {
        if (direction == 'N') direction = 'W';
        else if (direction == 'E') direction = 'N';
        else if (direction == 'S') direction = 'E';
        else if (direction == 'W') direction = 'S';
        UpdateRotation();
    }

    public void TurnRight()
    {
        if (direction == 'N') direction = 'E';
        else if (direction == 'E') direction = 'S';
        else if (direction == 'S') direction = 'W';
        else if (direction == 'W') direction = 'N';
        UpdateRotation();
    }

    // can add support for more commands here

    public void ExplodeRobot() => _robotObject.GetComponent<ExplodedImage>().ReplaceImage();
}
