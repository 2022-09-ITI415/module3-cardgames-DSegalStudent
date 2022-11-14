using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum eScoreEventGolf
{
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss
}

public class ScoreManagerGolf : MonoBehaviour
{
    static private ScoreManagerGolf S;

    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;
    static public int hole = 1;

    [Header("Set Dynamically")]
    //Fields to track score info
    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;

    private void Awake()
    {
        if (S == null) //c
        {
            S = this; //Set the private singleton
        }
        else
        {
            Debug.LogError("Error: ScoreManager.Awake(): S is already set!");
        }

        //Check for a high score in PlayerPrefs
        if(PlayerPrefs.HasKey("ProspectorHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }

        if (PlayerPrefs.HasKey("GolfHole"))
        {
            hole = PlayerPrefs.GetInt("GolfHole");
        }

        //Add the score from the last round, which will be >0 if it was a win
        score += SCORE_FROM_PREV_ROUND;
        //And reset the SCORE_FROM_PREV_ROUND
        SCORE_FROM_PREV_ROUND = 0;

        if (hole <= 9) {
            hole++;
        }
        else hole = 1;
    }

    static public void EVENT (eScoreEventGolf evt)
    {
        try
        {
            //try-catch stops an error from breaking your program
            S.Event(evt);
        } catch (System.NullReferenceException nre)
        {
            Debug.LogError ("ScoreManager:EVENT() called while S=null. \n" + nre);
        }
    }

    void Event(eScoreEventGolf evt)
    {
        switch (evt)
        {
            // Same things need to happen wheter it's a draw, a win, or a loss
            case eScoreEventGolf.draw: //Drawing a card
            case eScoreEventGolf.gameWin: //won the round
            case eScoreEventGolf.gameLoss: //Lost the round
                chain = 0;
                score += scoreRun; //add scoreRun to total score
                scoreRun = 0;
                break;
            case eScoreEventGolf.mine: //Remove mine card
                chain++; //increase the score chain
                scoreRun += chain; // add score for this card to run
                break;
        }

        //This second switch statement handles round wins and losses
        switch (evt)
        {
            case eScoreEventGolf.gameWin:
                //If it's a win, add the score to the next round
                //static fields are not reset by SceneManager.LoadScene()
                SCORE_FROM_PREV_ROUND = score;
                print("You won this round! Round score: " + score);
                break;
            case eScoreEventGolf.gameLoss:
                //If Loss, check against the high score
                if(HIGH_SCORE <= score)
                {
                    print("You got the high score! High Score: " + score);
                    HIGH_SCORE = score;
                    PlayerPrefs.SetInt("ProspectorHighScore", score);
                    if (hole <= 8)
                    {
                        hole = hole + 1;
                        PlayerPrefs.SetInt("GolfHole", hole);
                    }
                    else
                    {
                        hole = 0;
                        PlayerPrefs.SetInt("GolfHole", hole);
                    }
               
                }
                else
                {
                    print("Your final score for the game was: " + score);
                    if (hole <= 8)
                    {
                        //hole++;
                        PlayerPrefs.SetInt("GolfHole", hole);
                    }
                    else
                    {
                        hole = 0;
                        PlayerPrefs.SetInt("GolfHole", hole);
                    }
                }
                break;

            default:
                print("Score: " + score + " scoreRun:" + scoreRun + " chain:" + chain);
                break;
        }
    }

    static public int CHAIN { get { return S.chain; } }//e
    static public int SCORE { get { return S.score; } }
    static public int SCORE_RUN { get { return S.scoreRun; } }


  

}
