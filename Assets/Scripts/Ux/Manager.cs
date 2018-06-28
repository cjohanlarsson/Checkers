﻿using System.Collections;
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

		Gameplay.Game game;

		void Awake()
		{
			game = new Gameplay.Game(8);

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


		static readonly Dictionary<Gameplay.MoveError, string> moveErrorMessages = new Dictionary<Gameplay.MoveError, string>()
		{
			{Gameplay.MoveError.Success, "Success!"},
			{Gameplay.MoveError.Unknown, "Unknown!"}
		};

		void RefreshGameView()
		{
			consoleBoard.text = game.BoardToString();
		}

		void DisplayErrorMessage(Gameplay.MoveError errorMsg)
		{
			consoleMessage.text = moveErrorMessages[errorMsg];
		}

		void DisplayErrorMessage(string errorMsg)
		{
			consoleMessage.text = errorMsg;
		}

		void ClearErrorMessage()
		{
			consoleMessage.text = "";
		}
	}
}