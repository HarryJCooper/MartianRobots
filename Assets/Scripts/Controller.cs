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
    public GameObject pointObject;
}

public class Controller : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _inputText, _outputText;
    [SerializeField] private TMP_InputField _waitTimeInputField;
    [SerializeField] private Button _runButton;
    [SerializeField] private GameObject _pointPrefab, _robotPrefab;
    [SerializeField] private float _waitTime = 2f;
    [SerializeField] private Transform _testAreaTransform;
    [SerializeField] private RectTransform _testAreaRectTransform;
    private int _maxGridSize = 50, _maxInstructionLength = 100, _mapScaleX = 100, _mapScaleY = 100;

    // future work - [SerializeField] private TextMeshProUGUI _maxGridSizeText, _maxInstructionLengthText;

    private List<Point> Grid(int x, int y)
    {
        List<Point> grid = new List<Point>();

        /* ________CREATE GRID POINTS FOR UI________  
        if there was no grid UI, this wouldn't be required as could just save positions with scent, then check if robot has moved into that position, 
        rather than creating the list upfront */
        for (int i = 0; i <= x; i++){
            for (int j = 0; j <= y; j++){
                GameObject pointObject = GameObject.Instantiate(_pointPrefab);
                pointObject.transform.SetParent(_testAreaTransform);
                RectTransform rectTransform = pointObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(i * _mapScaleX, j * _mapScaleY);
                grid.Add(new Point(i, j, false));
            }
        }
        /* ________END CREATE GRID POINTS________ */

        return grid;
    }

    private IEnumerator Run()
    {
        /* future work, add variable max grid size and instruction length
        if (_maxGridSizeText != null && _maxGridSizeText.text != "") _maxGridSize = int.Parse(_maxGridSizeText.text);
        if (_maxInstructionLengthText != null && _maxInstructionLengthText.text != "") _maxInstructionLength = int.Parse(_maxInstructionLengthText.text);
        */

        if (float.TryParse(_waitTimeInputField?.text, out _waitTime)) Debug.Log("Wait time: " + _waitTime);
        else {
            _waitTime = 1f;
            Debug.LogError("Invalid wait time given, waitTime set to 1");
        }

        string output = "";
        string input = _inputText.text; // *NEW NOTE* - removed null-conditional operator (?.) as input text is required to run the program

        /* ________GET GRID COORDINATES________ */
        string firstLine = input.Substring(0, input.IndexOf('\n')); // get grid size from input by getting substring before first \n
        MatchCollection coords = Regex.Matches(firstLine, @"\d+");
        
        if (coords.Count != 2){ // check if grid size is valid, can be altered for 3D grids etc
            Debug.LogError("Invalid grid size");
            yield break;
        }

        int x = int.Parse(coords[0].Value); // convert string to int
        int y = int.Parse(coords[1].Value);

        if (x > _maxGridSize || y > _maxGridSize){ // check if grid size is valid
            Debug.LogError("Grid size out of bounds");
            yield break;
        }

        _mapScaleX = (int) _testAreaRectTransform.rect.width / x;
        _mapScaleY = (int) _testAreaRectTransform.rect.height / y;

        List<Point> grid = Grid(x, y);
        /*________END GET GRID COORDINATES________*/


        /* ________SPLIT ROBOT AND ROBOT DIRECTIOS INTO LINES________ */
        string[] lines = input.Substring(input.IndexOf('\n') + 1) // remove grid size from input by getting substring after first new line
                                .Split('\n') // split input into lines
                                .Where(line => !string.IsNullOrWhiteSpace(line)) // remove empty lines
                                .ToArray();

        if (lines.Length % 2 != 0){ // check if number of lines is valid
            Debug.LogError("Invalid robot instructions: " + lines.Length);
            yield break;
        }
        /* ________END SPLIT ROBOT AND ROBOT DIRECTIOS INTO LINES________ */


        for (int i = 0; i < lines.Length; i += 2){ // i is the robot start position, i + 1 is the robot instructions
            /* ________GET ROBOT START POSITION________ */
            string startPosition = lines[i].ToUpper(); // convert to uppercase to allow for lower case input
            
            MatchCollection startCoords = Regex.Matches(startPosition, @"\d+");
            if (startCoords.Count != 2){
                Debug.LogError("Invalid robot start coordinates: " + startPosition);
                yield break;
            }

            int robotStartX = int.Parse(startCoords[0].Value);
            int robotStartY = int.Parse(startCoords[1].Value);
            if (robotStartX > x || robotStartY > y){
                Debug.LogError("Robot start coordinates out of bounds: " + startPosition);
                continue; // used continue as you may want to skip invalid robots and continue with the rest
            }

            Match startDir = Regex.Match(startPosition, @"[NSEW]");
            if (startDir.Value.Length != 1){
                Debug.LogError("Invalid robot start direction: " + startPosition);
                yield break;
            }
            /*________END GET ROBOT START POSITION________*/

            /*________CREATE ROBOT OBJECT ________*/
            GameObject robotObject = GameObject.Instantiate(_robotPrefab);
            robotObject.transform.SetParent(_testAreaTransform);
            RectTransform rectTransform = robotObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(robotStartX * _mapScaleX, robotStartY * _mapScaleY);
            Robot robot = new Robot(robotStartX, robotStartY, startDir.Value[0], robotObject, _mapScaleX, _mapScaleY);
            /*________END CREATE ROBOT OBJECT________ */
            
            /*________GET ROBOT INSTRUCTIONS________ */
            string instructions = lines[i + 1].ToUpper();
            instructions = new string(instructions.Where(c => c <= 127).ToArray()); // Remove non-ASCII characters
            instructions = new string(instructions.Where(c => c >= ' ' && c <= '~').ToArray()); // Keep only printable ASCII characters
            instructions = instructions.Replace(" ", ""); // remove whitespaces from the instructions string

            if (instructions.Length >= _maxInstructionLength){ // check if robot instructions are valid
                Debug.LogError("robot instructions too long: " + instructions);
                yield break;
            }
            
            foreach (char instruction in instructions){ // check if robot instructions are valid
                if (instruction != 'F' && instruction !='L' && instruction != 'R'){
                    Debug.LogError("robot instructions contain invalid characters: " + instruction + " in " + instructions);
                    yield break;
                }
            }
            /*________END GET ROBOT INSTRUCTIONS________*/
            
            /*________RUN ROBOT INSTRUCTIONS________*/
            // *NEW NOTE* - we let the robot move, then we check for scent, hence 'previousX' and 'previousY'
            bool finishEarly = false, skipInstruction = false;
            foreach (char instruction in instructions){
                yield return new WaitForSeconds(_waitTime); // wait for _waitTime seconds before executing next instruction (for UI purposes)
                
                int previousX = robot.x, previousY = robot.y; // save previous position for checking if robot has left scent
                
                switch (instruction){ // here can add more instructions for future work
                    case 'F': robot.MoveForward(); break;
                    case 'L': robot.TurnLeft(); break;
                    case 'R': robot.TurnRight(); break;
                    default: break;
                }

                bool robotOffGrid = (robot.x < 0 || robot.x > x || robot.y < 0 || robot.y > y); 
                if (robotOffGrid){
                    for (int k = 0; k < grid.Count; k++){
                        bool robotAtGridPoint = (grid[k].x == previousX && grid[k].y == previousY);
                        if (!robotAtGridPoint) continue; // if robot is not at grid[k], continue
                        if (grid[k].scentLeft){ 
                            // if scent has been left and the robot is in the same position as that scent, take back the instruction
                            robot.MoveBackward();
                            skipInstruction = true;
                            Debug.Log("scent left, skipping instruction: " + instruction);
                            break;
                        }
                        
                        // else, farewell sweet prince!
                        output += previousX + " " + previousY + " " + robot.direction + " LOST\n"; // add lost robot to output
                        grid[k].scentLeft = true;
                        finishEarly = true; // stop running instructions early as robot is lost
                        robot.ExplodeRobot(); // change robot sprite to exploded image
                        break;
                    }
                }

                if (skipInstruction){ // robot has encountered scent, skip instruction
                    skipInstruction = false;
                    continue;
                }

                if (finishEarly) break; // if robot has exploded, stop running instructions early
            }
            /*________END RUN ROBOT INSTRUCTIONS________*/

            if (!finishEarly) output += robot.x + " " + robot.y + " " + robot.direction + "\n";
        }

        Debug.Log("output: " + output);
        _outputText?.SetText(output);
    }

    // *NEW NOTE* - Start is called on the first frame

    void Start() => _runButton.onClick.AddListener(() => StartCoroutine(Run())); // left undefended as would rather fail loudly
}
