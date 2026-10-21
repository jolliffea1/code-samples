using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePiece 
{
    public int pieceNumber { get; private set; }
    public string pieceType = Constants.NORMAL_PIECE;

    public GameObject gameObject;
    public GamePiece(int tempPieceNumber)
    {
        pieceNumber = tempPieceNumber;
    }
}
