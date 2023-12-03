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

    public void ExplodeRobot() => _robotObject.GetComponent<ExplodedImage>().ReplaceImage();
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

        for (int i = 0; i <= x; i++){
            for (int j = 0; j <= y; j++){
                GameObject pointObject = GameObject.Instantiate(_pointPrefab);
                pointObject.transform.SetParent(_testAreaTransform);
                RectTransform rectTransform = pointObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(i * _mapScaleX, j * _mapScaleY);
                grid.Add(new Point(i, j, false));
            }
        }

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
            _waitTime = 2f;
            Debug.LogError("Invalid wait time");
        }

        string output = "";
        string input = _inputText?.text;
        string firstLine = input.Substring(0, input.IndexOf('\n')); // get grid size from input by getting substring before first \n
        string firstLineNoSpaces = Regex.Replace(firstLine, @"\s+", "");
        Match coords = Regex.Match(firstLineNoSpaces, @"\d+");

        if (coords.Length != 2){ /* check if grid size is valid, can be altered for 3D grids etc */
            Debug.LogError("Invalid grid size");
            yield break;
        }

        int x = int.Parse(coords.Value.Substring(0, 1)); // convert string to int
        int y = int.Parse(coords.Value.Substring(1, 1));

        if (x > _maxGridSize || y > _maxGridSize){ // check if grid size is valid
            Debug.LogError("Grid size out of bounds");
            yield break;
        }

        _mapScaleX = (int) _testAreaRectTransform.rect.width / x;
        _mapScaleY = (int) _testAreaRectTransform.rect.height / y;

        List<Point> grid = Grid(x, y);
         // remove grid size from input by getting substring after first \n
        
        string[] lines = input.Substring(input.IndexOf('\n') + 1)
                                .Split('\n') // split input into lines
                                .Where(line => !string.IsNullOrWhiteSpace(line)) // remove empty lines
                                .ToArray();

        if (lines.Length % 2 != 0){ // check if number of lines is valid
            Debug.LogError("Invalid robot instructions: " + lines.Length);
            yield break;
        }

        for (int i = 0; i < lines.Length; i += 2){
            string startPosition = Regex.Replace(lines[i], @"\s+", "").ToUpper(); // convert to uppercase to allow for lower case input
            
            if (startPosition.Length != 3){ // check if robot start line is valid
                Debug.LogError("Invalid number of characters in robot start line: " + lines[i]);
                yield break;
            }

            Match startCoords = Regex.Match(startPosition, @"\d+");
            if (startCoords.Value.Length != 2){
                Debug.LogError("Invalid robot start coordinates: " + startPosition);
                yield break;
            }

            int robotStartX = int.Parse(startCoords.Value.Substring(0, 1));
            int robotStartY = int.Parse(startCoords.Value.Substring(1, 1));
            if (robotStartX > x || robotStartY > y){
                Debug.LogError("Robot start coordinates out of bounds: " + startPosition);
                continue; // used continue as you may want to skip invalid robots and continue with the rest
            }

            Match startDir = Regex.Match(startPosition, @"[NSEW]");
            if (startDir.Value.Length != 1){
                Debug.LogError("Invalid robot start direction: " + startPosition);
                yield break;
            }

            GameObject robotObject = GameObject.Instantiate(_robotPrefab);
            robotObject.transform.SetParent(_testAreaTransform);
            RectTransform rectTransform = robotObject.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(robotStartX * _mapScaleX, robotStartY * _mapScaleY);
            Robot robot = new Robot(robotStartX, robotStartY, startDir.Value[0], robotObject, _mapScaleX, _mapScaleY);

            string instructions = lines[i + 1].ToUpper();
            instructions = new string(instructions.Where(c => c <= 127).ToArray()); // Remove non-ASCII characters
            instructions = new string(instructions.Where(c => c >= ' ' && c <= '~').ToArray()); // Keep only printable ASCII characters
            instructions = instructions.Replace(" ", ""); // remove whitespaces from the instructions string

            if (instructions.Length >= _maxInstructionLength){ // check if robot instructions are valid
                Debug.LogError("robot instructions too long: " + instructions);
                yield break;
            }
            
            foreach (char instruction in instructions){ // check if robot instructions are valid, couldn't find a way to do this with regex
                if (instruction != 'F' && instruction !='L' && instruction != 'R'){
                    Debug.LogError("robot instructions contain invalid characters: " + instruction + " in " + instructions);
                    yield break;
                }
            }
            
            bool finishEarly = false;
            bool skipInstruction = false;
            foreach (char instruction in instructions){ // check if robot instructions are valid
                yield return new WaitForSeconds(_waitTime); // wait for _waitTime seconds before executing next instruction
                int previousX = robot.x;
                int previousY = robot.y;
                switch (instruction){
                    case 'F':
                        robot.MoveForward();
                        break;
                    case 'L':
                        robot.TurnLeft();
                        break;
                    case 'R':
                        robot.TurnRight();
                        break;
                    default:
                        break;
                }

                if (robot.x < 0 || robot.x > x || robot.y < 0 || robot.y > y){
                    for (int k = 0; k < grid.Count; k++){
                        if (grid[k].scentLeft){
                            robot.MoveBackward();
                            skipInstruction = true;
                            break;
                        }

                        if (grid[k].x == previousX && grid[k].y == previousY && !grid[k].scentLeft){
                            output += previousX + " " + previousY + " " + robot.direction + " LOST\n";
                            grid[k].scentLeft = true;
                            finishEarly = true;
                            robot.ExplodeRobot();
                            break;
                        }
                    }
                }

                if (skipInstruction){
                    skipInstruction = false;
                    continue;
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

    void Start() => _runButton.onClick.AddListener(() => StartCoroutine(Run())); // left undefended as would rather fail loudly
}
