using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Checkers.Ux
{
	public class Manager : MonoBehaviour
	{
		[SerializeField] Text consoleBoard;
		[SerializeField] InputField consoleInput;
		[SerializeField] Button consoleSubmit;
		[SerializeField] Text consoleMessage;
		[SerializeField] Text consoleTurn;
		[SerializeField] GameObject consoleInputAnchor;
		[SerializeField] GameObject consoleAiThinking;
		[SerializeField] bool aiEnabled;
		[SerializeField] int boardSize;
		[SerializeField] int boardHeight;
		[SerializeField] float aiThinkTime;

		Gameplay.Game game;

		void Awake()
		{
			game = new Gameplay.Game(boardSize,boardHeight);

			consoleSubmit.onClick.AddListener(() =>
			{
				Gameplay.Move move = null;
				if( Gameplay.Move.TryParse(consoleInput.text, ref move) ) 
				{
					var err = game.PlayMove(move);
					if(err != Gameplay.MoveError.Success)
					{
						DisplayErrorMessage(err);
					}
					else
					{
						RefreshGameView();
						ClearErrorMessage();
						ClearInput();
					}
				}
				else
				{
					DisplayErrorMessage("Couldn't parse moves!");
				}
			});

			ClearErrorMessage();
			RefreshGameView();
		}

		private IEnumerator Start()
		{
			consoleAiThinking.SetActive(false);

			while(!game.GameIsOver)
			{
				
				while(game.CurrentTurn == Gameplay.Team.O)
				{
					yield return null;
				}

				if (aiEnabled)
				{
					consoleInputAnchor.SetActive(false);
					consoleAiThinking.SetActive(true);
					if (game.GameIsOver)
						break;

					// ai thinking ...
					yield return new WaitForSeconds(aiThinkTime * Random.Range(0.75f, 1.25f));
					game.PlayMove(game.GetRandomValidMove());
					RefreshGameView();

					consoleAiThinking.SetActive(false);

					yield return new WaitForSeconds(0.25f);
					consoleInputAnchor.SetActive(true);
				}
				else
				{
					yield return null;
				}
			}
		}


		static readonly Dictionary<Gameplay.MoveError, string> moveErrorMessages = new Dictionary<Gameplay.MoveError, string>()
		{
			{Gameplay.MoveError.Success, "Success!"}
		};

		void RefreshGameView()
		{
			consoleBoard.text = game.BoardToString();
			consoleTurn.text = game.CurrentTurn.ToString();

			if(game.GameIsOver)
			{
				var winner = game.GameWinner;
				if (winner == Gameplay.Team.Empty)
					DisplayMessage("Game Over: Nobody Won!");
				else
					DisplayMessage(string.Format("Game Over: {0} Won!", winner));
			}
		}

		void DisplayErrorMessage(Gameplay.MoveError errorMsg)
		{
			consoleMessage.color = Color.red;
			if (moveErrorMessages.ContainsKey(errorMsg))
				consoleMessage.text = moveErrorMessages[errorMsg];
			else
				consoleMessage.text = errorMsg.ToString();
		}

		void DisplayErrorMessage(string errorMsg)
		{
			consoleMessage.color = Color.red;
			consoleMessage.text = errorMsg;
		}

		void DisplayMessage(string msg)
		{
			consoleMessage.color = Color.green;
			consoleMessage.text = msg;
		}

		void ClearErrorMessage()
		{
			consoleMessage.text = "";
		}

		void ClearInput()
		{
			consoleInput.text = "";
		}
	}
}