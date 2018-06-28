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

		public static bool GetBetween(Position prev, Position next, ref Position between)
		{
			int diffX = next.x - prev.x;
			int diffY = next.y - prev.y;

			if(Math.Abs(diffX) == 2 && Math.Abs(diffY) == 2)
			{
				between = new Position()
				{
					x = prev.x + Math.Sign(diffX),
					y = prev.y + Math.Sign(diffY)
				};
				return true;
			}
			else
			{
				return false;
			}
		}

		public static Position GetBetween(Position prev, Position next)
		{
			Position between = new Position();
			if (GetBetween(prev, next, ref between))
				return between;
			else
				throw new System.Exception("Not inbetween position!");
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
		NotEnoughMoves, // CHECK
		ContainsUnplayableCells, // CHECK
		NotYourPiece, // CHECK
		WrongMoveDirection, // CHECK
		InvalidMoveLength, // CHECK
		OnlyOneMoveAllowedIfFirstNotJump,
		CantMoveOntoOtherPieces,
		OnlyJumpOverOpponent
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

			var arr = input.Trim().Split(' ');
			foreach (var a in arr)
			{
				if (a.Length != 2)
					return false;

				newMove.positions.Add(new Position() { x = a[1] - '0', y = a[0] - 'a' });
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
			var myTeam = CurrentTurn;

			if (move.positions.Count < 2)
				return MoveError.NotEnoughMoves;

			// Check the first piece is ok
			var firstPos = move.positions[0];
			if (!board.IsCellPlayable(firstPos))
			{
				return MoveError.ContainsUnplayableCells;
			}
			else if (board[firstPos].team != myTeam)
			{
				return MoveError.NotYourPiece;
			}

			// Check each subsequent move
			for (int i = 1; i < move.positions.Count; i++)
			{
				var curr = move.positions[i];
				var prev = move.positions[i - 1];
				if (!board.IsCellPlayable(curr))
				{
					return MoveError.ContainsUnplayableCells;
				}
				else if(board[curr].team != Team.Empty)
				{
					return MoveError.CantMoveOntoOtherPieces;
				}

				int diffX = curr.x - prev.x;
				int diffY = curr.y - prev.y;

				int absDiffX = Math.Abs(diffX);
				int absDiffY = Math.Abs(diffY);
				if( absDiffX != absDiffY || absDiffX == 0 || absDiffX > 2)
				{
					return MoveError.InvalidMoveLength;
				}
				else if (Math.Sign(diffY) != (CurrentTurn == Team.X ? -1 : 1))
				{
					return MoveError.WrongMoveDirection;
				}

				if (absDiffX == 1)
				{
					if (move.positions.Count > 2)
						return MoveError.OnlyOneMoveAllowedIfFirstNotJump;

					// TODO: check if jump exists
				}
				else
				{
					Position between = Position.GetBetween(prev, curr);
					var otherTeam = board[between].team;
					if (otherTeam == Team.Empty || otherTeam == myTeam)
						return MoveError.OnlyJumpOverOpponent;
				}
			}

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
					if(Position.GetBetween(prev, curr, ref between)) {
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
				sb.Append((char)('a' + y));
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