using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HistoricBoardState {
    public Grid[,] grid;
    public string[,] terrain;
    public Dictionary<Grid, List<UnitState>> units;
    public Sprite[,] markers;
}

public struct UnitState
{
    public Unit unit;
    public int currentHealth;
    public List<int> staminaRolls;
    public Color color;
    public Sprite sprite;
}

public struct ActionSet 
{
    public int[] diceResults;
    public int move;
    public int harm;
    public int block;
    public string effect;
}

public struct Counter
{
    public string name;
    public int value;
}

public static class Constants
{
    public const string CLICK = "CLICK";
    public const string MOVE = "MOVE";
    public const string FINISHED = "FINISHED";
    public const string NORMAL_PIECE = "NORMAL";
    public const string KING_PIECE = "KING";

    // TURN SEQUENCE
    public const string TURN_STEP_ROLL = "ROLL";
    public const string TURN_STEP_ENEMY_MOVE = "ENEMY MOVE";
    public const string TURN_STEP_ASSIGN = "ASSIGN";
    public const string TURN_STEP_PLAYER_MOVE = "PLAYER MOVE";
    public const string TURN_STEP_HARM = "HARM";
    public static readonly string[] ALL_TURN_STEPS = { TURN_STEP_ROLL, TURN_STEP_ENEMY_MOVE, TURN_STEP_ASSIGN, TURN_STEP_PLAYER_MOVE, TURN_STEP_HARM };

    public const float CAMERA_X_ROTATION = 50f;
    public const float CAMERA_Y_COORD = 2.5f;
    public const float CAMERA_Z_COORD = -1f;

    public const float BOARD_Y_COORD = 0f;
    public const float UNIT_Y_OFFSET = 0.01f;

    // TERRAIN
    public const string TERRAIN_NORMAL = "NORMAL TERRAIN";
    public const string TERRAIN_PERMANENT = "PERMANENT TERRAIN";
    public const string TERRAIN_DIFFICULT = "DIFFICULT TERRAIN";

    // TOKEN COLORS
    public static readonly Color TOKEN_COLOR_A = Color.white;
    public static readonly Color TOKEN_COLOR_B = new Color(0.9f, 0.3f, 0.24f, 1);
    public static readonly Color TOKEN_COLOR_C = new Color(0.2f, 0.6f, 0.86f, 1);

    // FILE BROWSER
    public static readonly string[] EXCLUDED_EXTENSIONS = { ".lnk", ".tmp", ".zip", ".rar", ".exe" };
}
