using SFB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [DllImport("__Internal")]
    public static extern string BrowserTextUpload(string extFilter, string gameObjName, string dataSinkFn);


    private Camera mainCamera;

    private GameObject[,] gridSpaces;

    // token buttons
    [SerializeField] GameObject tokenButtonPrefab;
    private static List<Sprite> tokens = new List<Sprite>();
    private static List<GameObject> tokenButtons = new List<GameObject>();

    // custom counters
    [SerializeField] GameObject counterDisplayPrefab;
    private static List<Counter> counters = new List<Counter>();
    private static List<GameObject> counterDisplays = new List<GameObject>();

    [SerializeField] GameObject unitObject;
    [SerializeField] Unit playerUnit;
    [SerializeField] Unit enemyUnit;

    [SerializeField] GameObject block;

    [SerializeField] TextMeshProUGUI gameVersionText;
    [SerializeField] TextMeshProUGUI turnPhaseText;
    [SerializeField] TextMeshProUGUI turnHistoryText;
    // At the start of each round, store the current board state for later recovery
    public List<HistoricBoardState> gameHistory = new List<HistoricBoardState>();

    [SerializeField] TextMeshProUGUI boardSizeText;
    public static int boardSize = 4;

    bool hasGameFinished = false;
    int currentTurnStep = 0;

    public static GameObject clickedUnit;
    public static GameObject clickedSpace;

    public static GameManager instance;

    public delegate void UpdateMessage(int currentTurnStep);
    public event UpdateMessage Message;

    int currentTurnIndex = 0;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (gameVersionText != null)
        {
            gameVersionText.text = "v" + Application.version;
        }

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        CreateGrid();
        drawPieces();

        if (boardSizeText != null)
        {
            boardSizeText.text = boardSize.ToString();
        }

        if (tokens.Count < 1)
        {
            tokens.Add(enemyUnit.sprite);
        }
        drawTokenButtons();
        drawCustomCounters();
    }

    // Update is called once per frame
    void Update()
    {
        Message(currentTurnStep);
        if (hasGameFinished)
        {
            return;
        }

        if (turnHistoryText != null)
        {
            turnHistoryText.text = currentTurnIndex.ToString();
        }
    }

    private void CreateGrid()
    {
        gridSpaces = new GameObject[boardSize + 1, boardSize + 1];
        for (int i = 0; i < gridSpaces.GetUpperBound(0); i++)
        {
            for (int j = 0; j < gridSpaces.GetUpperBound(1); j++)
            {
                gridSpaces[i, j] = CreateSingleSpace(i, j);
            }
        }

        float cameraXOffset = (boardSize / 2);
        if (boardSize % 2 == 0)
        {
            cameraXOffset -= 0.5f;
        }
        mainCamera.transform.position = new Vector3(cameraXOffset, Constants.CAMERA_Y_COORD + (0.5f * (boardSize - 4)), Constants.CAMERA_Z_COORD - (0.15f * (boardSize - 4)));
        mainCamera.transform.eulerAngles = new Vector3(Constants.CAMERA_X_ROTATION, 0, 0);
    }

    private GameObject CreateSingleSpace(int x, int z)
    {
        GameObject newSpace = Instantiate(block);
        newSpace.transform.position = new Vector3(x, Constants.BOARD_Y_COORD, z);
        newSpace.GetComponent<Renderer>().material.color = Color.white;

        // set space location
        Grid newSpaceGrid = newSpace.GetComponent<Grid>();
        newSpaceGrid.x = x;
        newSpaceGrid.z = z;
        newSpaceGrid.initLabel(boardSize);

        // set unit locations
        newSpaceGrid.singleOccupantLocation = new Vector3(x, newSpace.transform.position.y + Constants.UNIT_Y_OFFSET, z);

        Vector3 boxSize = newSpace.GetComponent<Collider>().bounds.size;
        float multiOccupantOffset = boxSize.x / 4.0f;
        newSpaceGrid.multiOccupantLocations = new List<Vector3>();
        newSpaceGrid.multiOccupantLocations.Add(new Vector3((float)(x + multiOccupantOffset), newSpace.transform.position.y + Constants.UNIT_Y_OFFSET, (float)(z + multiOccupantOffset)));
        newSpaceGrid.multiOccupantLocations.Add(new Vector3((float)(x - multiOccupantOffset), newSpace.transform.position.y + Constants.UNIT_Y_OFFSET, (float)(z - multiOccupantOffset)));
        newSpaceGrid.occupants = new List<GameObject>();

        // set marker location
        newSpaceGrid.spaceMarkerLocation = new Vector3((float)(x + multiOccupantOffset), newSpace.transform.position.y + Constants.UNIT_Y_OFFSET, (float)(z - multiOccupantOffset));

        return newSpace;
    }

    private void drawPieces()
    {
        GameObject playerUnitObject = Instantiate(unitObject);
        playerUnitObject.GetComponent<UnitDisplay>().unit = playerUnit;
        createSinglePiece(boardSize - 1, 0, playerUnitObject);

        GameObject enemyUnitObject = Instantiate(unitObject);
        enemyUnitObject.GetComponent<UnitDisplay>().unit = enemyUnit;
        enemyUnitObject.GetComponent<UnitDisplay>().setSprite(enemyUnit.sprite);
        createSinglePiece(0, 0, enemyUnitObject);
    }

    private void createSinglePiece(int x, int z, GameObject unitObject)
    {
        unitObject.transform.position = gridSpaces[x, z].GetComponent<Grid>().singleOccupantLocation;
        gridSpaces[x, z].GetComponent<Grid>().occupants.Add(unitObject);
        gridSpaces[x, z].GetComponent<Grid>().distributeOccupants();
    }

    private void drawTokenButtons()
    {
        if (tokenButtons != null && tokenButtons.Count > 0)
        {
            while (tokenButtons.Count > 0)
            {
                Destroy(tokenButtons[0]);
                tokenButtons.RemoveAt(0);
            }
        }

        tokenButtons = new List<GameObject>();
        for (int i = 0; i < tokens.Count; i++)
        {
            // instantiate a new button prefab
            GameObject tokenButton = Instantiate(tokenButtonPrefab);

            // find the UI Canvas
            var parent = GameObject.Find("UnitButtons");
            var parentPosition = parent.transform.position;
            tokenButton.transform.SetParent(parent.transform);
            tokenButton.transform.position = new Vector3(parentPosition.x, parentPosition.y, parentPosition.z);
            tokenButton.transform.localScale = new Vector3(1f, 1f, 1f);

            var button = tokenButton.GetComponent<Button>();
            button.image.sprite = tokens[i];
            button.onClick.AddListener(() => addUnitToGrid(button.GetComponent<Image>()));
            tokenButtons.Add(tokenButton);
        }
    }

    public void addUnitToGrid(Image image)
    {
            for (int i = 0; i < gridSpaces.GetUpperBound(0); i++)
            {
                for (int j = gridSpaces.GetUpperBound(1) - 1; j > -1; j--)
                {
                    GameObject parentSpace = gridSpaces[i, j];
                    Grid parentSpaceGrid = parentSpace.GetComponent<Grid>();

                    if (parentSpaceGrid.occupants.Count < 2)
                    {
                        GameObject enemyUnitObject = Instantiate(unitObject);
                        enemyUnitObject.GetComponent<UnitDisplay>().unit = enemyUnit;
                        enemyUnitObject.GetComponent<UnitDisplay>().setSprite(image.sprite);
                        createSinglePiece(i, j, enemyUnitObject);
                        return;
                    }
                }
            }
    }

    public void GameQuit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }

    public void GameReset()
    {
        GameReset(0);
    }

    public void GameReset(int boardSizeModifier)
    {
        if (boardSize + boardSizeModifier < 3 || boardSize + boardSizeModifier > 6)
        {
            return;
        }
        boardSize += boardSizeModifier;
        storeCurrentCounters();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void changeCombatPhase(int modifier)
    {
        currentTurnStep += modifier;

        // if passing the final step, start a new round from the beginning
        if (currentTurnStep >= Constants.ALL_TURN_STEPS.Length)
        {
            currentTurnStep = 0;
        }

        if (Constants.ALL_TURN_STEPS[currentTurnStep] == Constants.TURN_STEP_ROLL)
        {
            if (gameHistory.Count > 0)
            {
            currentTurnIndex += 1;
            }
            
            if (turnPhaseText != null)
            {
                turnPhaseText.text = "New Round";
            }

            // ROLL step; start of a new round
            for (int i = 0; i < gridSpaces.GetUpperBound(0); i++)
            {
                for (int j = 0; j < gridSpaces.GetUpperBound(1); j++)
                {
                    gridSpaces[i, j].GetComponent<Grid>().newRoundStart();
                }
            }

            storeBoardState();
        }
    }

    public void advanceRetreatTurn(int turnTrackModifier)
    {
        if (currentTurnIndex + turnTrackModifier < 0 || currentTurnIndex + turnTrackModifier >= gameHistory.Count)
        {
            return;
        }
        currentTurnIndex += turnTrackModifier;

        // erase the current unit objects
        for (int i = 0; i < gridSpaces.GetUpperBound(0); i++)
        {
            for (int j = 0; j < gridSpaces.GetUpperBound(1); j++)
            {
                Grid currentSpace = gridSpaces[i, j].GetComponent<Grid>();
                Destroy(currentSpace.spaceMarker);
                for (int index = currentSpace.occupants.Count - 1; index > -1 ; index--)
                {
                    Destroy(currentSpace.occupants[index]);
                    currentSpace.occupants.RemoveAt(index);
                }
            }
        }

        HistoricBoardState targetTurn = gameHistory[currentTurnIndex];

        for (int i = 0; i < targetTurn.grid.GetUpperBound(0); i++)
        {
            for (int j = 0; j < targetTurn.grid.GetUpperBound(1); j++)
            {
                gridSpaces[i, j].GetComponent<Grid>().terrain = targetTurn.terrain[i, j];
                gridSpaces[i, j].GetComponent<Grid>().setColorByTerrain();

                gridSpaces[i, j].GetComponent<Grid>().setSpaceMarker(targetTurn.markers[i, j], this.unitObject);

                foreach (UnitState savedUnit in targetTurn.units[targetTurn.grid[i, j]])
                {
                    GameObject unitGameObject = Instantiate(this.unitObject);
                    unitGameObject.GetComponent<UnitDisplay>().unit = savedUnit.unit;
                    unitGameObject.GetComponent<UnitDisplay>().currentHealth = savedUnit.currentHealth;
                    unitGameObject.GetComponent<UnitDisplay>().staminaRolls = savedUnit.staminaRolls;
                    unitGameObject.GetComponent<UnitDisplay>().setStaminaText();
                    unitGameObject.GetComponent<UnitDisplay>().setBaseColor(savedUnit.color);
                    unitGameObject.GetComponent<UnitDisplay>().setSprite(savedUnit.sprite);
                    createSinglePiece(i, j, unitGameObject);
                }

            }
        }

        // set the active player positions to the stored position state
        Message(currentTurnStep);
    }

    public void storeBoardState()
    {
        if (gameHistory.Count > 1 && currentTurnIndex <= gameHistory.Count)
        {
            gameHistory.RemoveRange(currentTurnIndex, gameHistory.Count - currentTurnIndex);
        }

        HistoricBoardState tempState = new HistoricBoardState();
        tempState.grid = new Grid[boardSize + 1, boardSize + 1];
        tempState.terrain = new string[boardSize + 1, boardSize + 1];
        tempState.markers = new Sprite[boardSize + 1, boardSize + 1];
        tempState.units = new Dictionary<Grid, List<UnitState>>();
        for (int i = 0; i < gridSpaces.GetUpperBound(0); i++)
        {
            for (int j = 0; j < gridSpaces.GetUpperBound(1); j++)
            {
                Grid space = gridSpaces[i, j].GetComponent<Grid>();
                tempState.grid[i, j] = space;
                tempState.terrain[i, j] = space.terrain;

                if (space.spaceMarker != null)
                {
                    tempState.markers[i, j] = space.spaceMarker.GetComponent<UnitDisplay>().getSprite();
                }

                List<UnitState> units = new List<UnitState>();
                foreach (GameObject unit in space.occupants)
                {
                    UnitDisplay unitToStore = unit.GetComponent<UnitDisplay>();

                    UnitState tempUnit = new UnitState();
                    tempUnit.unit = unitToStore.unit;
                    tempUnit.staminaRolls = unitToStore.staminaRolls;
                    tempUnit.currentHealth = unitToStore.currentHealth;
                    tempUnit.color = unitToStore.getBaseColor();
                    tempUnit.sprite = unitToStore.getSprite();
                    units.Add(tempUnit);
                }
                tempState.units.Add(space, units);
            }
        }        

        gameHistory.Add(tempState);
    }

    public void uploadToken()
    {



#if UNITY_EDITOR
// if in Unity Editor, load files using the local browser
                string path = EditorUtility.OpenFilePanel("Overwrite with png", "", "png");

                if (path != null && File.Exists(path))
                {
                    byte[] FileData = File.ReadAllBytes(path);
                    processTokenFile(FileData);
                }
#elif UNITY_WEBGL
        // if in a full WebGL build, use custom file browser
        BrowserTextUpload(".png", "GameManager", "handleFileFromBrowser");
#else
    ExtensionFilter[] extensions = { new ExtensionFilter("Image Files", "jpg", "png") };
        string[] paths = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false);
        if (paths.Length > 0 && File.Exists(paths[0]))
        {
            byte[] FileData = File.ReadAllBytes(paths[0]);
            processTokenFile(FileData);
        }
#endif
    }

    public void handleFileFromBrowser(string base64str)
    {
        base64str = base64str.Substring(base64str.IndexOf(',') + 1);
        byte[] FileData = Convert.FromBase64String(base64str);
        processTokenFile(FileData);
    }

    public void processTokenFile(byte[] FileData)
    {
        Texture2D imageTexture = new Texture2D(1, 1);
        if (imageTexture.LoadImage(FileData))
        {
            float smallestDimension = imageTexture.width < imageTexture.height ? imageTexture.width : imageTexture.height;
            float pixelsPerUnit = smallestDimension / 0.52f;
            Sprite sprite = Sprite.Create(imageTexture, new Rect(0, 0, imageTexture.width, imageTexture.height), new Vector2(0.5f, 0f), pixelsPerUnit);
            tokens.Add(sprite);
            drawTokenButtons();
        }
    }

    // store the current state of all counters, to repopulate values after redraw
    private void storeCurrentCounters()
    {
        List<Counter> tempCounters = new List<Counter>();
        for (int i = 0; i < counterDisplays.Count; i++)
        {
            tempCounters.Add(counterDisplays[i].GetComponent<CounterDisplay>().getCounter());
        }
        counters = tempCounters;
    }

    // create the UI objects displaying values for each custom counter
    private void drawCustomCounters()
    {
        if (counterDisplays != null && counterDisplays.Count > 0)
        {
            while (counterDisplays.Count > 0)
            {
                Destroy(counterDisplays[0]);
                counterDisplays.RemoveAt(0);
            }
        }

        counterDisplays = new List<GameObject>();
        for (int i = 0; i < counters.Count; i++)
        {
            // instantiate a new button prefab
            GameObject counterDisplay = Instantiate(counterDisplayPrefab);

            // find the UI Canvas
            var parent = GameObject.Find("CountersGroup");
            var parentPosition = parent.transform.position;
            counterDisplay.transform.SetParent(parent.transform);
            counterDisplay.transform.position = new Vector3(parentPosition.x, parentPosition.y, parentPosition.z);
            counterDisplay.transform.localScale = new Vector3(1f, 1f, 1f);

            CounterDisplay counterDisplayObject = counterDisplay.GetComponent<CounterDisplay>();
            counterDisplayObject.setIndex(i);
            counterDisplayObject.setCounter(counters[i]);
            counterDisplays.Add(counterDisplay);
        }
    }

    // add a new custom counter to the UI
    public void createCounter()
    {
        storeCurrentCounters();
        counters.Add(new Counter());
        drawCustomCounters();
    }

    public static void deleteCounter(int index)
    {
        if (counters.Count == 0)
        {
            return; 
        }

        // if the index is larger than the current list size, find the new index of the entry
        if (index >= counterDisplays.Count)
        {
            for (int i = 0; i < counterDisplays.Count; i++)
            {
                CounterDisplay display = counterDisplays[i].GetComponent<CounterDisplay>();
                if (display.getIndex() == index)
                {
                    index = i;
                    break;
                }
            }
        }
        counters.RemoveAt(index);
        Destroy(counterDisplays[index]);
        counterDisplays.RemoveAt(index);
      
    }
}
