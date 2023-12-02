using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class Point
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

public class Robot
{
    public Robot(int x, int y, char direction)
    {
        this.x = x;
        this.y = y;
        this.direction = direction;
    }
    public int x;
    public int y;
    public char direction;
}

public class Controller : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _inputText, _outputText, _maxGridSizeText, _maxInstructionLengthText;
    [SerializeField] private Button _runButton;
    private int _maxGridSize = 50, _maxInstructionLength = 100;
    
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
        if (_maxGridSizeText != null && _maxGridSizeText.text != "") _maxGridSize = int.Parse(_maxGridSizeText.text);
        if (_maxInstructionLengthText != null && _maxInstructionLengthText.text != "") _maxInstructionLength = int.Parse(_maxInstructionLengthText.text);

        string output = "";
        string input = _inputText?.text;
        string firstLine = input.Substring(0, input.IndexOf('\n')); // get grid size from input by getting substring before first \n
        string firstLineNoSpaces = Regex.Replace(firstLine, @"\s+", "");
        Match coords = Regex.Match(firstLineNoSpaces, @"\d+");

        if (coords.Length != 2){ /* check if grid size is valid, can be altered for 3D grids etc */
            Debug.LogError("Invalid grid size");
            return;
        }

        int x = int.Parse(coords.Value.Substring(0, 1)); // convert string to int
        int y = int.Parse(coords.Value.Substring(1, 1));

        if (x > _maxGridSize || y > _maxGridSize){ // check if grid size is valid
            Debug.LogError("Grid size out of bounds");
            return;
        }

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

            Match startDir = Regex.Match(lines[i], @"[NSEW]");
            if (startDir.Value.Length != 1){
                Debug.LogError("Invalid robot start direction: " + lines[i]);
                return;
            }

            Robot robot = new Robot(robotStartX, robotStartY, startDir.Value[0]);

            string instructions = lines[i + 1].ToUpper();
            instructions = new string(instructions.Where(c => c <= 127).ToArray()); // Remove non-ASCII characters
            instructions = new string(instructions.Where(c => c >= ' ' && c <= '~').ToArray()); // Keep only printable ASCII characters
            instructions = instructions.Replace(" ", ""); // remove whitespaces from the instructions string

            if (instructions.Length >= _maxInstructionLength){ // check if robot instructions are valid
                Debug.LogError("robot instructions too long: " + instructions);
                return;
            }
            
            foreach (char instruction in instructions){ // check if robot instructions are valid, couldn't find a way to do this with regex
                if (instruction != 'F' && instruction !='L' && instruction != 'R'){
                    Debug.LogError("robot instructions contain invalid characters: " + instruction + " in " + instructions);
                    return;
                }
            }
            
            bool finishEarly = false;
            foreach (char instruction in instructions){ // check if robot instructions are valid
                int previousX = robot.x;
                int previousY = robot.y;
                switch (instruction){
                    case 'F':
                        if (robot.direction == 'N') robot.y++;
                        else if (robot.direction == 'E') robot.x++;
                        else if (robot.direction == 'S') robot.y--;
                        else if (robot.direction == 'W') robot.x--;
                        break;
                    case 'L':
                        if (robot.direction == 'N') robot.direction = 'W';
                        else if (robot.direction == 'E') robot.direction = 'N';
                        else if (robot.direction == 'S') robot.direction = 'E';
                        else if (robot.direction == 'W') robot.direction = 'S';
                        break;
                    case 'R':
                        if (robot.direction == 'N') robot.direction = 'E';
                        else if (robot.direction == 'E') robot.direction = 'S';
                        else if (robot.direction == 'S') robot.direction = 'W';
                        else if (robot.direction == 'W') robot.direction = 'N';
                        break;
                    default:
                        break;
                }

                Debug.Log("ROBOT DIRECTION: " + robot.direction);
                if (robot.x < 0 || robot.x > x || robot.y < 0 || robot.y > y){
                    for (int k = 0; k < grid.Count; k++){
                        if (grid[k].x == previousX && grid[k].y == previousY && !grid[k].scentLeft){
                            output += previousX + " " + previousY + " " + robot.direction + " LOST\n";
                            grid[k].scentLeft = true;
                            finishEarly = true;
                            break;
                        } else if (grid[k].x == robot.x && grid[k].y == robot.y && grid[k].scentLeft){
                            robot.x = previousX;
                            robot.y = previousY;
                            Debug.Log("Robot ENCOUNTERED SCENT: " + robot.x + ", " + robot.y + ", " + robot.direction + ", " + instructions);
                            output += robot.x + " " + robot.y + " " + robot.direction + "\n";
                            finishEarly = true;
                            break;
                        }
                    }
                    
                    break;
                }

                if (finishEarly) break;
            }

            if (!finishEarly){
                Debug.Log("Robot FINISHED: " + robot.x + ", " + robot.y + ", " + robot.direction);
                output += robot.x + " " + robot.y + " " + robot.direction + "\n";
            } 
        }

        Debug.Log("MADE IT HERE!");
        Debug.Log("output: " + output);
        _outputText?.SetText(output);
    }

    void Start() => _runButton.onClick.AddListener(Run); // left undefended as would rather fail loudly
}
