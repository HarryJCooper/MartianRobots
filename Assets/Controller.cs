using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

public struct Robot
{
    public Robot(int x, int y, int direction)
    {
        this.x = x;
        this.y = y;
        this.direction = direction;
    }
    public int x;
    public int y;
    public int direction;
}

public class Controller : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _inputText, _outputText;
    [SerializeField] private Button _runButton;
    
    private List<Point> Grid(int x, int y)
    {
        List<Point> grid = new List<Point>();

        for (int i = 0; i <= x; i++){
            for (int j = 0; j <= y; j++){
                grid.Add(new Point(i, j, false));
            }
        }

        return grid;
    }

    private string RobotOutput(int startX, int startY, int startDir, string instructions) // using int for direction as makes NE/SW/etc. easier to handle later
    {
        string output = "";
        Robot robot = new Robot(startX, startY, startDir);
        


        return output;
    }

    private void Run()
    {
        /* Sample Input
        5 3
        1 1 E
        RFRFRFRF
        3 2 N
        FRRFLLFFRRFLL
        0 3 W
        LLFFFLFLFL

        Sample Output
        1 1 E
        3 3 N LOST
        2 3 S */
        string input = _inputText?.text;
        string firstLine = input.Substring(0, input.IndexOf('\n')); // get grid size from input by getting substring before first \n
        MatchCollection coords = Regex.Matches(firstLine, @"\d+"); // get all numbers from first line, done this way to allow for spaces or no spaces in input

        if (coords.Count != 2){ /* check if grid size is valid, can be altered for 3D grids etc */
            Debug.LogError("Invalid grid size");
            return;
        }

        int x = int.Parse(coords[0].Value); // convert string to int
        int y = int.Parse(coords[1].Value); 

        List<Point> grid = Grid(x, y);

        input = input.Substring(input.IndexOf('\n')); // remove grid size from input by getting substring after first \n
        string[] lines = input.Split('\n'); // split input into lines

        if (lines.Length % 2 != 0){ // check if number of lines is valid
            Debug.LogError("Invalid robot instructions");
            return;
        }

        for (int i = 0; i < lines.Length; i += 2){
            MatchCollection startCoords = Regex.Matches(lines[i], @"\d+");
            if (startCoords.Count != 2){
                Debug.LogError("Invalid robot start coordinates");
                return;
            }

            MatchCollection startDir = Regex.Matches(lines[i], @"[A-Z]");          
        }

        Debug.Log("MADE IT HERE!");
        string output = "";

        _outputText?.SetText(output);
    }

    void Start() => _runButton.onClick.AddListener(Run); // left undefended as would rather fail loudly
}
