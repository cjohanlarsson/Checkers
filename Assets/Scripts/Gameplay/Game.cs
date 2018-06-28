using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;

namespace Checkers.Gameplay
{
	public struct Position
	{
		public int x;
		public int y;

		public bool GetBetween(Position prev, ref Position between)
		{
			int diffX = this.x - prev.x;
			int diffY = this.y - prev.y;

			if(Math.Abs(diffX) == 2 && Math.Abs(diffY) == 2)
			{
				between = new Position()
				{
					x = this.x + Math.Sign(diffX),
					y = this.y + Math.Sign(diffY)
				};
				return true;
			}
			else
			{
				return false;
			}
		}
	}

	public enum Team
	{
		Empty = 0,
		X,
		O
	}

	public struct CellState
	{
		public Team team;

		public static CellState Empty
		{
			get { return new CellState(); }
		}
	}

	public enum MoveError
	{
		Success = 0,
		Unknown,
		NotEnoughCells,
		ContainsUnplayableCells,
		NotYourPiece
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

	public class Board
	{
		public CellState[,] board;

		public Board(int boardSize)
		{
			board = new CellState[boardSize, boardSize];
			for (int i = 0; i < boardSize; i++)
			{
				for (int j = 0; j < boardSize; j++)
				{
					if (IsCellPlayable(i, j))
					{
						var state = new CellState();
						state.team = j < 3 ? Team.O : (j >= (boardSize - 3) ? Team.X : Team.Empty);
						board[i, j] = state;
					}
				}
			}
		}

		public bool IsCellPlayable(Position p)
		{
			return IsCellPlayable(p.x, p.y);
		}

		public bool IsCellPlayable(int x, int y)
		{
			if (x < 0 || y < 0 || x >= this.GetSize() || y >= this.GetSize())
				return false;
			
			return (x % 2 == y % 2);
		}

		public CellState this[Position p]
		{
			get
			{
				return board[p.x, p.y];
			}
			set
			{
				board[p.x, p.y] = value;
			}
		}

		public CellState this[int x,int y]
		{
			get
			{
				return board[x,y];
			}
			set
			{
				board[x, y] = value;
			}
		}

		public int GetSize()
		{
			return board.GetLength(0);
		}
	}

	public class Game
	{
		Board board;

		public Game(int boardSize) 
		{
			board = new Board(boardSize);
			CurrentTurn = Team.O;
		}

		public Team CurrentTurn { get; private set; }



		private MoveError TestMove(Move move)
		{
			if (move.positions.Count < 2)
				return MoveError.NotEnoughCells;

			// Check unplayable
			foreach(var p in move.positions)
			{
				if(!board.IsCellPlayable(p)) {
					return MoveError.ContainsUnplayableCells;
				}
			}

			// Check if its your piece 
			var firstPos = move.positions[0];
			if (board[firstPos].team != CurrentTurn)
				return MoveError.NotYourPiece;

			// TODO: Check if moving in valid direction

			// TODO: Check first move NOT a jump AND normal play AND no other jumps exist

			// TODO: Check if first move IS a jump AND all other moves are jumps

			return MoveError.Success;
		}

		public MoveError PlayMove(Move move)
		{
			var err = TestMove(move);
			if(err == MoveError.Success) 
			{
				for (int i = 1; i < move.positions.Count; i++)
				{
					var curr = move.positions[i];
					var prev = move.positions[i - 1];

					board[curr] = board[prev];
					board[prev] = CellState.Empty;

					Position between = new Position();
					if(curr.GetBetween(prev, ref between)) {
						board[between] = CellState.Empty;
					}
				}
				CurrentTurn = CurrentTurn == Team.O ? Team.X : Team.O;
			}
			return err;
		}

		public string BoardToString()
		{
			var sb = new StringBuilder();
			for (int y = board.GetSize() - 1; y >= 0; y--) 
			{
				sb.Append(y);
				sb.Append(' ');
				for (int x = 0; x < board.GetSize(); x++) 
				{
					var state = board[x, y];
					sb.Append(state.team == Team.Empty ? '.' : (state.team == Team.O ? 'O' : 'X'));
					sb.Append(' ');
				}
				sb.Append('\n');
			}

			sb.Append("  ");
			for (int x = 0; x < board.GetSize(); x++)
			{
				sb.Append(x);
				sb.Append(' ');
			}
			return sb.ToString();
		}
	}
}