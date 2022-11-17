using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Golf : MonoBehaviour {

	static public Golf 	S;

	[Header("Set in Inspector")]
	public TextAsset			deckXML;
	public TextAsset layoutXML;
	public float xOffset = 3;
	public float yOffset = -2.5f;
	public Vector3 layoutCenter;
	public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
	public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
	public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
	public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
	public float reloadDelay = 2f; //2 sec delay between rounds
	public Text gameOverText, roundResultText, highScoreText , HoleCounterText, OverallScoreText;


	[Header("Set Dynamically")]
	public Deck					deck;
	public Layout layout;
	public List<CardGolf> drawPile;
	public Transform layoutAnchor;
	public CardGolf target;
	public List<CardGolf> tableau;
	public List<CardGolf> discardPile;
	public FloatingScore fsRun;
	public int hole;
	public int OverallScore;


	void Awake(){
		S = this;
		SetUpUITexts();
		
	}

	void SetUpUITexts()
    {
		//Set up the HighScore UI text
		GameObject go = GameObject.Find("HighScore");
		if(go != null)
        {
			highScoreText = go.GetComponent<Text>();
        }
		int highScore = ScoreManagerGolf.HIGH_SCORE;
		string hScore = "High Score: " + Utils.AddCommasToNumber(highScore);
		go.GetComponent<Text>().text = hScore;

		//Set up the UI Texts that show at end of round
		go = GameObject.Find("GameOver");
		if(go != null)
        {
			gameOverText = go.GetComponent<Text>();
        }

		go = GameObject.Find("RoundResult");
		if (go != null)
        {
			roundResultText = go.GetComponent<Text>();
        }
		go = GameObject.Find("HoleCounter");
		if(go != null)
        {
			HoleCounterText = go.GetComponent<Text>();
        }
		go = GameObject.Find("NewScore");
		if(go != null)
        {
			OverallScoreText = go.GetComponent<Text>();
        }

		//Make the end of round texts invisible
		ShowResultsUI(false);
    }

	void ShowResultsUI(bool show)
    {
		gameOverText.gameObject.SetActive(show);
		roundResultText.gameObject.SetActive(show);
    }

	void Start() {
		Scoreboard.S.score = ScoreManagerGolf.SCORE;
		deck = GetComponent<Deck>();
		deck.InitDeck (deckXML.text);
		Deck.Shuffle(ref deck.cards);
		hole = ScoreManagerGolf.hole;
		HoleCounterText.text = "Hole: " + hole;

		if (PlayerPrefs.HasKey("OverallScore"))
		{
			OverallScore = PlayerPrefs.GetInt("OverallScore");
		}
		if (hole == 1)
        {
			OverallScore = 0;
        }
		
		

		OverallScoreText.text = OverallScore.ToString();

		

		//Card c;
		//for(int cNum=0; cNum<deck.cards.Count; cNum++)
		//{
		//	c = deck.cards[cNum];
		//	c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);
		//}
		layout = GetComponent<Layout>(); //get the Layout component
		layout.ReadLayout(layoutXML.text); //Pass LayoutXML to it

		drawPile = ConvertListCardsToListCardGolfs(deck.cards);
		LayoutGame();
	}

	List<CardGolf> ConvertListCardsToListCardGolfs(List<Card> lCD)
    {
		List<CardGolf> lCP = new List<CardGolf>();
		CardGolf tCP;
		foreach(Card tCD in lCD)
        {
			tCP = tCD as CardGolf; //a
			lCP.Add(tCP);
        }
		return (lCP);
    }

	//The Draw function will pull a single card from the drawPile and return it
	CardGolf Draw()
    {
		CardGolf cd = drawPile[0]; // Pull the 0th CardProspector
		drawPile.RemoveAt(0); // then remove it from the List<> drawPile
		return (cd); // and return it
    }

	//LayoutGame() positions the initial tableau of cards, a.k.a the "mine"
	void LayoutGame()
    {
		//create an empy GameObject to serve as an anchor for the tableau //a
		if(layoutAnchor == null)
        {
			GameObject tGo = new GameObject("_LayoutAnchor");
			// ^Create an empty GameObject names _LayoutAnchor in the Hierarchy
			layoutAnchor = tGo.transform; //Grab its Transform
			layoutAnchor.transform.position = layoutCenter; //Position it
        }

		CardGolf cp;
		//Follow the layout
		foreach (SlotDef tSD in layout.slotDefs)
        {
			//^ Iterate through all the SLotDefs in the layout.slotDefs as tSD
			cp = Draw(); // Pull a card from the top (beginning) of the draw Pile
			cp.faceUp = tSD.faceUp; //Set its faceUp to the value in SlotDef
			cp.transform.parent = layoutAnchor;
			//This replaces the previous parent: deck.deckAnchor, which\
			//appears as _Deck in the Hierarchy when the scene is playing.
			cp.transform.localPosition = new Vector3(
				layout.multiplier.x * tSD.x, layout.multiplier.y * tSD.y, -tSD.layerID);
			//^Set the localPosition of the card based on slotDef
			cp.layoutID = tSD.id;
			cp.slotDef = tSD;
			//CardProspectors in the tableau have the state CardState.tableau
			cp.state = eCardStateGolf.tableau;
			//CardProspectors in the tableau have the state CardState.tableau
			cp.SetSortingLayerName(tSD.layerName);

			tableau.Add(cp); //Add this CardProspector to the List<> tableau
        }

		//Set whcih cards are hiding others
		foreach(CardGolf tCP in tableau)
        {
			foreach(int hid in tCP.slotDef.hiddenBy)
            {
				cp = FindCardByLayoutID(hid);
				tCP.hiddenBy.Add(cp);
            }
        }

		//(another for each loop for gold cards)

		//set up initial target card
		MoveToTarget(Draw());

		//Set up the Draw Pile
		UpdateDrawPile();
    }

	//Convert from the layoutID int to the CardProspector with that ID
	CardGolf FindCardByLayoutID(int layoutID)
    {
		foreach (CardGolf tCP in tableau)
        {
			//Search through all cards in the tableau List<>
			if (tCP.layoutID == layoutID)
            {
				//if the card has the same ID, return it
				return (tCP);
            }
        }
		//if its not found, return null
		return (null);
    }

	//this turns cards in the Mine face-up or face-down
	void SetTableauFaces()
    {
		foreach(CardGolf cd in tableau)
        {
			bool faceUp = true; //Assume the card will be face-up
			foreach (CardGolf cover in cd.hiddenBy)
            {
				//If either of the covering cards are in the tableau
				if(cover.state == eCardStateGolf.tableau)
                {
					faceUp = false; // then this card is face-down
                }
            }
			cd.faceUp = faceUp; // Set the value on the card
        }
    }

	//Moves the current target to the discardPile
	void MoveToDiscard(CardGolf cd)
    {
		//Set the state of the card to discard
		cd.state = eCardStateGolf.discard;
		discardPile.Add(cd); //Add it to the discardPile List<>
		cd.transform.parent = layoutAnchor; //Update its transform parent

		//position this card on the discardPile
		cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID + 0.5f);
		cd.faceUp = true;
		//place it on top of the pile for depth sorting
		cd.SetSortingLayerName(layout.discardPile.layerName);
		cd.SetSortOrder(-100 + discardPile.Count);
    }

	//Make cd the new target card
	void MoveToTarget(CardGolf cd)
    {
		//If there is currently a target card, move it to the discardPile
		if (target != null) MoveToDiscard(target);
		target = cd; //cd is the new target
		cd.state = eCardStateGolf.target;
		cd.transform.parent = layoutAnchor;
		//Move to the target position
		cd.transform.localPosition = new Vector3(layout.multiplier.x * layout.discardPile.x, layout.multiplier.y * layout.discardPile.y, -layout.discardPile.layerID);

		cd.faceUp = true; //Make it face-up
						  //Set the depth sorting
		cd.SetSortingLayerName(layout.discardPile.layerName);
		cd.SetSortOrder(0);
    }

	//Arranges all the cards of the drawPile to show how many are left
	void UpdateDrawPile()
    {
		CardGolf cd;
		//Go Through all the cards of the drawPile
		for(int i=0; i<drawPile.Count; i++)
        {
			cd = drawPile[i];
			cd.transform.parent = layoutAnchor;

			//Position it correclty with the layout.drawPile.stagger
			Vector2 dpStagger = layout.drawPile.stagger;
			cd.transform.localPosition = new Vector3(layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x), layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y), -layout.drawPile.layerID + 0.1f * i);

			cd.faceUp = false; // make them all face down
			cd.state = eCardStateGolf.drawpile;
			//set depth sorting
			cd.SetSortingLayerName(layout.drawPile.layerName);
			cd.SetSortOrder(-10 * i);
        }
    }

	//CardClicked is called any time a card in the game is clicked
	public void CardClicked(CardGolf cd)
    {
		//the reaction is determined by the state of the clicked card
		switch (cd.state)
        {

			case eCardStateGolf.target:
				//clicking the target card does nothing
				break;

			case eCardStateGolf.drawpile:
				//Clicking any card in the drawPile will drawt the next card
				MoveToDiscard(target); //Moves the target to the discard pile
				MoveToTarget(Draw()); // moves the next draw card to the target
				UpdateDrawPile(); //Restacks the draw pile
				ScoreManagerGolf.EVENT(eScoreEventGolf.draw);
				FloatingScoreHandler(eScoreEventGolf.draw);
				break;

			case eCardStateGolf.tableau:
				//clicking a card in the tableau will check if its a valid play
				bool validMatch = true;
				if (!cd.faceUp)
                {
					//if the card is face-down its not valid
					validMatch = false;
                }
				if (!AdjacentRank(cd, target))
                {
					//If its not an adjacent rank, its not valid
					validMatch = false;
                }
				if (!validMatch) return; //return if not valid

				//If we got here, then, its a valid card
				tableau.Remove(cd); //Remove it from the tableau list
				MoveToTarget(cd); // make it the target card
				SetTableauFaces(); //Update tableau card face-ups
				//ScoreManagerGolf.EVENT(eScoreEventGolf.mine);
				//AnotherScoreEvent.GoldMine?
				//FloatingScoreHandler(eScoreEventGolf.mine);
				break;
        }
		//check to see wheter the game is over or not
		CheckForGameOver();
    }

	//test whether the game is over
	void CheckForGameOver()
    {
		//if the tableau is empty, the game is over
		if (tableau.Count == 0)
        {
			//CallGameOver() with a win
			GameOver(true);
			return;
        }
		// if there are still cards in the draw pile, the game's not over
        if (drawPile.Count > 0)
        {
			return;
        }

		//check for remaining valid player
		foreach (CardGolf cd in tableau)
        {
			if(AdjacentRank(cd, target))
            {
				//If there is a valid play, the game's not over
				return;
            }
        }

		GameOver(false);
    }

	//Called When the game is over. Simple for now but expandable
	void GameOver(bool won)
    {
		int score = ScoreManagerGolf.SCORE;
		//if (fsRun != null) score += fsRun.score;
		score += tableau.Count;
		OverallScore += tableau.Count;
		if (won)
        {
			gameOverText.text = "Round Over";
			roundResultText.text = "You won this round!\nRound Score: " + score;
			ShowResultsUI(true);
			//print("Game over. You won! : )");
			ScoreManagerGolf.EVENT(eScoreEventGolf.gameWin);
			FloatingScoreHandler(eScoreEventGolf.gameWin);
			PlayerPrefs.SetInt("OverallScore", OverallScore);
			OverallScoreText.text = OverallScore.ToString();



		}
        else
        {
			gameOverText.text = "Game Over";
			if (ScoreManagerGolf.HIGH_SCORE >= score) 
            {
				string str = "You got the high score!\nHigh score: " + score;
				roundResultText.text = str;
				
				HoleCounterText.text = "Hole: " + hole;

				PlayerPrefs.SetInt("OverallScore", OverallScore);
				OverallScoreText.text = OverallScore.ToString();

			}
            else 
            {
				roundResultText.text = "Your final score was: " + score;

				PlayerPrefs.SetInt("OverallScore", OverallScore);
				OverallScoreText.text = OverallScore.ToString();
			}
			ShowResultsUI(true);
			//print("Game Over. You Lose. :(");
	
			ScoreManagerGolf.EVENT(eScoreEventGolf.gameLoss);
			FloatingScoreHandler(eScoreEventGolf.gameLoss);
        }
		//Reload the scene,resetting the game
		//SceneManager.LoadScene("__Prospector_Scene_0");

		//Reload the scene in reloadDelay seconds
		//This will give the score a moment to travel
		Invoke("ReloadLevel", reloadDelay); //a

    }

	void ReloadLevel()
    {
		//reload the scene, resetting the game
		SceneManager.LoadScene("GolfSolitaire");
		
    }

	//return true if the two cards are adjacent in rank (A & K wrap around)
	public bool AdjacentRank(CardGolf c0, CardGolf c1)
    {
		//If either card is face-down, its not adjacent
		if (!c0.faceUp || !c1.faceUp) return (false);

		//if they are 1 apart, they are adjacent
		if(Mathf.Abs(c0.rank - c1.rank) == 1)
        {
			return (true);
        }

		//if one is Ace and the other King, they are adjacent
		if (c0.rank == 1 && c1.rank == 13) return (true);
		if (c0.rank == 13 && c1.rank == 1) return (true);

		//otherwise return false
		return (false);
    }


	//Handle FloatingScore movement
	void FloatingScoreHandler(eScoreEventGolf evt)
    {
		List<Vector2> fsPts;
		switch (evt)
        {
			//Same things need to happen whether its a draw, a win, or a loss
			case eScoreEventGolf.draw: //Drawing a card
			case eScoreEventGolf.gameWin: // Won the round
			case eScoreEventGolf.gameLoss: //Lost the round
			//Add fsRun to the Scoreboard score
			if (fsRun != null)
                {
					//create points for the bezier curve1
					fsPts = new List<Vector2>();
					fsPts.Add(fsPosRun);
					fsPts.Add(fsPosMid2);
					fsPts.Add(fsPosEnd);
					fsRun.reportFinishTo = Scoreboard.S.gameObject;
					fsRun.Init(fsPts, 0, 1);
					//Also adjust the fontSize
					fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
					fsRun = null; //clear fsRun so it's created again


                }
				break;

			case eScoreEventGolf.mine: // Remove a mine card
								   //Create a FloatingScore for this score
				FloatingScore fs;
				//ove it from the mousePosition to fsPosRun
				Vector2 p0 = Input.mousePosition;
				p0.x /= Screen.width;
				p0.y /= Screen.height;
				fsPts = new List<Vector2>();
				fsPts.Add(p0);
				fsPts.Add(fsPosMid);
				fsPts.Add(fsPosRun);
				fs = Scoreboard.S.CreateFloatingScore(ScoreManagerGolf.CHAIN, fsPts);
				fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
				if (fsRun == null)
                {
					fsRun = fs;
					fsRun.reportFinishTo = null;
                }
                else
                {
					fs.reportFinishTo = fsRun.gameObject;
                }
				break;
        }
    }

	
}
