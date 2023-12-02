using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public struct Point
{
    public Point(int x, int y, bool scentLeft)
    {
        this.x = x;
        this.y = y;
        this.scentLeft = scentLeft;
    }
    public int x;
    public int y;
    public bool scentLeft;
}

public class Controller : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _inputText, _outputText;
    [SerializeField] private Button _runButton;
    
    private void Run()
    {
        string input = _inputText?.text;
        int x = input[0] - '0';
        int y = input[2] - '0';
        List<Point> grid = new List<Point>();

        for (int i = 0; i <= x; i++){
            for (int j = 0; j <= y; j++){
                grid.Add(new Point(i, j, false));
                Debug.Log("i: " + i + " j: " + j);
            }
        }

        Debug.Log("x: " + x + " y: " + y);
        string output = "";

        _outputText?.SetText(output);
    }

    void Start() => _runButton.onClick.AddListener(Run); // left undefended as would rather fail loudly
}
