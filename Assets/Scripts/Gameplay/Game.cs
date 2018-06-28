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
		OnlyOneMoveAllowedIfFirstNotJump, // CHECK
		CantMoveOntoOtherPieces, // CHECK
		OnlyJumpOverOpponent, // CHECK
		MustJumpIfThereIsOne // CHECK
	}

	public class Move
	{

		public List<Gameplay.Position> positions;

		public Move() 
		{
			positions = new List<Position>();
		}

		public Move(Move move)
		{
			positions = new List<Position>(move.positions);	
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

		public Board(int boardSize, int boardHeight)
		{
			board = new CellState[boardSize, boardSize];
			for (int i = 0; i < boardSize; i++)
			{
				for (int j = 0; j < boardSize; j++)
				{
					if (IsCellPlayable(i, j))
					{
						var state = new CellState();
						state.team = j < boardHeight ? Team.O : (j >= (boardSize - boardHeight) ? Team.X : Team.Empty);
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

		public int GetTeamCount(Team team)
		{
			int cnt = 0;
			foreach(var p in GetTeamPositions(team))
			{
				cnt++;
			}
			return cnt;
		}

		public IEnumerable<Position> GetTeamPositions(Team team)
		{
			for (int x = 0; x < board.GetLength(0); x++)
			{
				for (int y = 0; y < board.GetLength(1) ; y++)
				{
					if(board[x,y].team == team)
					{
						yield return new Position()
						{
							x = x,
							y = y
						};
					}
				}
			}
		}
	}

	public class Game
	{
		Board board;

		public Game(int boardSize, int boardHeight) 
		{
			board = new Board(boardSize, boardHeight);
			CurrentTurn = Team.O;
		}

		public Team CurrentTurn { get; private set; }

		public bool GameIsOver
		{
			get; private set;
		}

		public Team GameWinner
		{
			get; private set;
		}

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
				else if (Math.Sign(diffY) != (myTeam == Team.X ? -1 : 1))
				{
					return MoveError.WrongMoveDirection;
				}

				if (absDiffX == 1)
				{
					if (move.positions.Count > 2)
						return MoveError.OnlyOneMoveAllowedIfFirstNotJump;

					// check if jump exists
					foreach(var p in board.GetTeamPositions(myTeam))
					{
						int yDir = CurrentTurn == Team.O ? 1 : -1;
						var jump1 = new Position()
						{
							x = p.x + 2,
							y = p.y + (yDir * 2),
						};
						var jump2 = new Position()
						{
							x = p.x - 2,
							y = p.y + (yDir * 2),
						};

						if(IsValidJump(p,jump1) || IsValidJump(p,jump2))
						{
							return MoveError.MustJumpIfThereIsOne;
						}
					}
				}
				else
				{
					if (!IsValidJump(prev,curr))
						return MoveError.OnlyJumpOverOpponent;
				}
			}

			return MoveError.Success;
		}

		private bool IsValidJump(Position prev, Position next)
		{
			if (!board.IsCellPlayable(prev) || !board.IsCellPlayable(next))
				return false;


			Team myTeam = board[prev].team;
			if (myTeam == Team.Empty || board[next].team != Team.Empty)
				return false;
			
			Position between = new Position();
			return (Position.GetBetween(prev, next, ref between) && board[between].team != Team.Empty && board[between].team != myTeam);
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
				CheckGameIsOver();
			}
			return err;
		}

		private void CheckGameIsOver()
		{
			if(GetRandomValidMove() == null)
			{
				GameIsOver = true;
				if (board.GetTeamCount(Team.X) == 0)
					GameWinner = Team.O;
				else if (board.GetTeamCount(Team.O) == 0)
					GameWinner = Team.X;
			}
		}

		public Move GetRandomValidMove()
		{
			Move tmpMove = new Move();
			List<Move> validJumps = new List<Move>();
			List<Move> validSingles = new List<Move>();

			Position[] attempts = new Position[4];
			foreach(Position curr in board.GetTeamPositions(CurrentTurn))
			{
				int yDir = CurrentTurn == Team.O ? 1 : -1;

				attempts[0] = new Position()
				{
					x = curr.x + 2,
					y = curr.y + (yDir * 2),
				};
				attempts[1] = new Position()
				{
					x = curr.x - 2,
					y = curr.y + (yDir * 2),
				};
				attempts[2] = new Position()
				{
					x = curr.x + 1,
					y = curr.y + (yDir * 1),
				};
				attempts[3] = new Position()
				{
					x = curr.x - 1,
					y = curr.y + (yDir * 1),
				};

				for (int i = 0; i < attempts.Length; i++)
				{
					tmpMove.positions.Clear();
					tmpMove.positions.Add(curr);
					tmpMove.positions.Add(attempts[i]);
					if (this.TestMove(tmpMove) == MoveError.Success)
					{
						if (i < 2)
						{
							validJumps.Add(new Move(tmpMove));
						}
						else
						{
							validSingles.Add(new Move(tmpMove));
						}
					}
				}
			}

			var rnd = new System.Random();
			if (validJumps.Count > 0)
			{
				return validJumps[rnd.Next(0, validJumps.Count)];
			}
			else if (validSingles.Count > 0)
			{
				return validSingles[rnd.Next(0, validSingles.Count)];
			}
			else
			{
				return null;
			}
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