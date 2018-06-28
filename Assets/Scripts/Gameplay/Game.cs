using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Checkers.Gameplay
{
	public struct Position
	{
		public int x;
		public int y;
	}

	public enum Team
	{
		Empty,
		X,
		O
	}

	public struct CellState
	{
		public Team team;
	}

	public enum MoveError
	{
		Success,
		Unknown
	}

	public class Move
	{

		public List<Gameplay.Position> positions;

		public Move() 
		{
			positions = new List<Position>();
		}

		/// <summary>
		/// Parse a move in the format X1,Y1 X2,Y2 X3,Y3
		/// </summary>
		/// <returns><c>true</c>, if parse was tryed, <c>false</c> otherwise.</returns>
		/// <param name="input">Input.</param>
		/// <param name="move">Move.</param>
		public static bool TryParse(string input, ref Move move)
		{
			Move newMove = new Move();

			var arr = input.Split(' ');
			foreach(var a in arr)
			{
				var arr2 = a.Split(',');
				if (arr2.Length != 2)
					return false;

				int x; int y;
				if(int.TryParse(arr2[0], out x) && int.TryParse(arr2[1], out y) )
				{
					newMove.positions.Add(new Position() { x = x, y = y });
				}
				else
				{
					return false;
				}
			}

			move = newMove;
			return true;
		}
	}

	public class Game
	{
		public CellState[,] board;

		public Game(int boardSize) {
			board = new CellState[boardSize, boardSize];
		}

		public Team CurrentTurn { get; private set; }

		public MoveError PlayMove(Move move)
		{
			return MoveError.Unknown;
		}

		public CellState GetStateAt(int x, int y)
		{
			if (x < 0 || y < 0 || x >= board.GetLength(0) || y >= board.GetLength(1))
				throw new System.IndexOutOfRangeException("Not a valid position!");
			
			return board[x, y];
		}

		public string BoardToString()
		{
			var sb = new StringBuilder();
			for (int y = board.GetLength(1) - 1; y >= 0; y--) 
			{
				sb.Append(y);
				sb.Append(' ');
				for (int x = 0; x < board.GetLength(0); x++) 
				{
					var state = board[x, y];
					sb.Append(state.team == Team.Empty ? '.' : (state.team == Team.O ? 'O' : 'X'));
					sb.Append(' ');
				}
				sb.Append('\n');
			}

			sb.Append("  ");
			for (int x = 0; x < board.GetLength(0); x++)
			{
				sb.Append(x);
				sb.Append(' ');
			}
			return sb.ToString();
		}
	}
}