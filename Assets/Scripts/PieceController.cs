using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public enum TurnState
{
	X = PieceState.X,
	O = PieceState.O,
}

public enum PieceRefType
{
	Horizontal,		// |
	Verticle,		// -
	DiagonalRight,	// /
	DiagonalLeft,	// \
}


public class PieceProfile
{
	public int AI { get; private set; }
	public int user { get; private set; }
	public int empty { get; private set; }
	public int userConsec { get; private set; }

	public PieceProfile(int AI, int user, int empty, int userConsec)
	{
		this.AI = AI;
		this.user = user;
		this.empty = empty;
		this.userConsec = userConsec;
	}

	public PieceProfile(Piece[] pieces, TurnState UserTurn, TurnState AITurn)
	{
		foreach (Piece p in pieces) {
			if (p.state != (PieceState)UserTurn) {
				break;
			}
			userConsec++;
		}
		foreach (Piece p in pieces) {
			if (p.state == (PieceState)UserTurn) {
				user++;
			}
			else if (p.state == (PieceState)AITurn) {
				AI++;
			}
			else if (p.state == PieceState.Empty) {
				empty++;
			}
		}
	}

	public static PieceProfile operator+ (PieceProfile p1, PieceProfile p2)
	{
		return new PieceProfile (p1.AI + p2.AI, p1.user + p2.user, p1.empty + p2.empty, p1.userConsec + p2.userConsec);
	}
}



public static class TicTacToeUtility
{
	public static int ToIndex(this Vector2 coord)
	{
		if (coord.x < 0 || coord.x >= PieceController.size || coord.y < 0 || coord.y >= PieceController.size) {
			return -1;
		}
		return (int)(coord.y * PieceController.size + coord.x);
	}

	public static TurnState Other(this TurnState turn)
	{
		if (turn == TurnState.X) {
			return TurnState.O;
		}
		return TurnState.X;
	}

	public static List<Piece> GetWinningRange(this List<Piece> pieces)
	{
		if (pieces.Count <= PieceController.winning - 1) {
			return pieces;
		}
		return pieces.GetRange (0, PieceController.winning - 1);
	}
}

public class PieceController : MonoBehaviour
{
	public static readonly Dictionary<PieceRefType, Vector2> directionData = new Dictionary<PieceRefType, Vector2> {
		{PieceRefType.Horizontal, new Vector2(1f,0f)},
		{PieceRefType.Verticle, new Vector2(0f,1f)},
		{PieceRefType.DiagonalRight, new Vector2(1f,1f)},
		{PieceRefType.DiagonalLeft, new Vector2(1f,-1f)},
	};

	public Text winningText;

	public bool AIPlayFirst;

	private TurnState AITurn;
	private TurnState UserTurn;

	// winning moves
	public static readonly int winning = 4;

	// size 6x6
	public static readonly int size = 6;

	// establish piece zero
	public static readonly Vector3 pieceZero = new Vector3(-250f, -300f);

	public TurnState turn { get; private set; }

	private Piece[] pieces;

	private List<Piece> availablePieces;

	void Awake()
	{
		// init turn state
		turn = TurnState.X;

		if (AIPlayFirst) {
			AITurn = turn;
			UserTurn = AITurn.Other ();
		} else {
			UserTurn = turn;
			AITurn = UserTurn.Other ();
		}
	}

	void Start()
	{
		// available pieces
		availablePieces = GetComponentsInChildren<Piece> ().OrderBy (p => p.index).ToList ();

		// get an array of pieces order by their index
		pieces = availablePieces.ToArray();
	}


	public Piece GetPiece(Vector2 coord)
	{
		int index = coord.ToIndex ();
		if (index < 0 || index >= pieces.Length) {
			return null;
		}
		return pieces [index];
	}

	private void HighlightWinningPieces(Piece[] winningPieces)
	{
		winningPieces.ForEach (p => {
			Image image = p.GetComponent<Image>();
			image.color = Color.red;
		});
	}

	// using current turn to check all pieceRefType for winning(4) in a row
	public bool CheckWinning(Piece piece)
	{
		foreach (PieceRefType pieceRefType in Utility.EnumEnumerable<PieceRefType>())
		{
			// don't need a list just need a winning counter...
			int winningCount = 1;

			List <Piece> winningPieces = new List<Piece> ();
			winningPieces.Add (piece);

			// loop through both directions and count
			foreach (Vector2 refCoord in new Vector2[] {directionData [pieceRefType], directionData [pieceRefType]*-1f})
			{
				// get the first reference piece
				Piece refPiece = piece.GetRefPiece (refCoord);

				// extend it
				while (refPiece != null && refPiece.state == (PieceState)turn) {
					winningPieces.Add (refPiece);
					// winner
					if (++winningCount >= winning) {
						HighlightWinningPieces (winningPieces.ToArray());
						return true;
					}
					refPiece = refPiece.GetRefPiece (refCoord);
//					Debug.Log("refPiece:" + refPiece);
				}
			}

//			Debug.Log (pieceRefType + ":" + winningCount);
		}

		return false;
	}
	private int CalculateAIWeight(Piece[] pieceRight, Piece[] pieceLeft)
	{
		Debug.Log (pieceLeft.ToString <Piece> () + ":" + pieceRight.ToString <Piece> ());

		// not enough pieces to win
		if (pieceRight.Length + pieceLeft.Length + 1 < winning) {
			return -1;
		}

		// identify if all empty
		if (!pieceRight.Any (p => p.state != PieceState.Empty) && !pieceLeft.Any (p => p.state != PieceState.Empty)) {
			return 0;
		}

		PieceProfile proRight = new PieceProfile (pieceRight, UserTurn, AITurn);
		PieceProfile proLeft = new PieceProfile (pieceLeft, UserTurn, AITurn);
		PieceProfile proCombine = proRight + proLeft;

		if (proCombine.userConsec >= winning - 2) {
			return 1000;
		}
		// no user count
		if (proCombine.user == 0) {
			return proCombine.AI;
		}

		return -1;
	}

	private void AssignAIProfile()
	{
		// loop through all available pieces
		foreach (Piece piece in availablePieces)
		{
			Debug.Log ("Analyzing Piece:" + piece);
			// init weight data
			Dictionary<PieceRefType, int> weightData = new Dictionary<PieceRefType, int> ();
			// assign weight per direction
			foreach (PieceRefType pieceRefType in Utility.EnumEnumerable<PieceRefType>())
			{
				
				List<Piece> pieceRight = new List<Piece> ();
				Vector2 refCoord = directionData[pieceRefType];
				// get the first reference piece
				Piece refPiece = piece.GetRefPiece (refCoord);
				// extend it
				while (refPiece != null) {
					pieceRight.Add (refPiece);
					refPiece = refPiece.GetRefPiece (refCoord);
				}


				List<Piece> pieceLeft = new List<Piece> ();
				refCoord = directionData[pieceRefType] * -1f;
				// get the first reference piece
				refPiece = piece.GetRefPiece (refCoord);
				// extend it
				while (refPiece != null) {
					pieceLeft.Add (refPiece);
					refPiece = refPiece.GetRefPiece (refCoord);
				}

				int weight = CalculateAIWeight (pieceRight.GetWinningRange().ToArray (), pieceLeft.GetWinningRange().ToArray ());
				if (weight < 0) {
					continue;
				}
				weightData.Add (pieceRefType, weight);
			}
			piece.aiProfile.weightData = weightData;
			Debug.Log ("weight:" + weightData.ToString<KeyValuePair<PieceRefType, int>>());
		}
	}

	public void GameOver()
	{
		winningText.gameObject.SetActive (true);
		pieces.ForEach (p => {
			Button b = p.GetComponent<Button>();
			b.enabled = false;
		});
	}

	public void PiecePressed(Piece piece)
	{
		// determine if winning move
		if (CheckWinning (piece)) {
			Debug.Log ("won");
			if (turn == UserTurn) {
				winningText.text = "YOU WON";
			} else {
				winningText.text = "YOU LOSE";
			}
			GameOver ();
			return;
		}

		// flip turns
		turn = turn.Other ();

		availablePieces.Remove (piece);

		// draw
		if (availablePieces.Count == 0) {
			winningText.text = "DRAW";
			GameOver ();
			return;
		}

		// AI turn
		if (turn == AITurn) {
			// calculate AIProfile for all available pieces
			AssignAIProfile();

			// sort the weight high to low
			Piece choosenPiece = availablePieces.OrderByDescending (p => p.aiProfile.weight).FirstOrDefault();
			if (choosenPiece == null) {
				winningText.text = "DRAW";
				GameOver ();
				return;
			} else {
				choosenPiece.PiecePressed ();
			}
		}
	}
}
