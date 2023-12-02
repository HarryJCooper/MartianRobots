using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

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
         // remove grid size from input by getting substring after first \n
        
        string[] lines = input.Substring(input.IndexOf('\n') + 1)
                                .Split('\n') // split input into lines
                                .Where(line => !string.IsNullOrWhiteSpace(line)) // remove empty lines
                                .ToArray();

        if (lines.Length % 2 != 0){ // check if number of lines is valid
            Debug.LogError("Invalid robot instructions: " + lines.Length);
            return;
        }

        for (int i = 0; i < lines.Length; i += 2){
            lines[i] = Regex.Replace(lines[i], @"\s+", "").ToUpper(); // convert to uppercase to allow for lower case input
            
            if (lines[i].Length != 3){ // check if robot start line is valid
                Debug.LogError("Invalid number of characters in robot start line: " + lines[i]);
                return;
            }

            Match startCoords = Regex.Match(lines[i], @"\d+");
            if (startCoords.Value.Length != 2){
                Debug.LogError("Invalid robot start coordinates: " + lines[i]);
                return;
            }

            int robotStartX = int.Parse(startCoords.Value.Substring(0, 1));
            int robotStartY = int.Parse(startCoords.Value.Substring(1, 1));
            if (robotStartX > x || robotStartY > y){
                Debug.LogError("Robot start coordinates out of bounds: " + lines[i]);
                continue; // used continue as you may want to skip invalid robots and continue with the rest
            }

            MatchCollection startDir = Regex.Matches(lines[i], @"[NSEW]");
            if (startDir.Count != 1){
                Debug.LogError("Invalid robot start direction: " + lines[i]);
                return;
            }

            Debug.Log("Robot start: " + robotStartX + ", " + robotStartY + ", " + startDir[0].Value + ", " + lines[i + 1]);
        }

        Debug.Log("MADE IT HERE!");
        string output = "";

        _outputText?.SetText(output);
    }

    void Start() => _runButton.onClick.AddListener(Run); // left undefended as would rather fail loudly
}
