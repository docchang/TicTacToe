using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public enum PieceState
{
	Empty,
	X,
	O,
}


public class AIProfile
{
	// weight in all directions
	public Dictionary<PieceRefType, int> weightData;

	public int weight
	{
		get {
			if (weightData.IsNullOrEmpty ()) {
				return 0;
			}
			return weightData.Sum (wd => wd.Value) + weightData.Count;
		}
	}
}


public class Piece : MonoBehaviour
{
	public static readonly float pieceSize = 100f;

	public PieceState state { get; private set; }

	public Vector2 coord { get; private set; }

	public int index
	{
		get { return (int)(coord.y * PieceController.size + coord.x); }
	}

	private Text txt;
	private PieceController pieceController;

	public AIProfile aiProfile;

	void Awake()
	{
		// init text
		txt = GetComponentInChildren<Text> ();
		txt.text = string.Empty;

		// init controller
		pieceController = GetComponentInParent<PieceController> ();

		// initialize state
		state = PieceState.Empty;

		// init coord
		// (-150, -300) - (-250, -300) = (100, 0)
		// (100, 0) / 100 = (1,0)
		Vector3 coord3 = gameObject.GetComponent<RectTransform>().localPosition - PieceController.pieceZero;
		coord = new Vector2 (coord3.x / pieceSize, coord3.y / pieceSize);

		// init ai profile
		aiProfile = new AIProfile ();

//		// make sure all coords are correct
//		Debug.Log (string.Format("{0}:{1}:{2}", index, coord, gameObject.GetComponent<RectTransform>().localPosition));
	}

	// coord + refCoord => (0,0) + (1,1)
	public Piece GetRefPiece(Vector2 refCoord)
	{
		return pieceController.GetPiece (coord + refCoord);
	}

	public override string ToString ()
	{
		if (state == PieceState.Empty) {
			return "_";
		}
		return state.ToString ();
//		return string.Format ("[Piece: state={0}, coord={1}, index={2}]", state, coord, index);
	}

	public void PiecePressed()
	{
		// no effect when already assigned
		if (state != PieceState.Empty) {
			return;
		}

		// assign text
		txt.text = pieceController.turn.ToString ();

		// assign state
		state = (PieceState)pieceController.turn;

		// propagate up to controller
		pieceController.PiecePressed (this);
	}
}
