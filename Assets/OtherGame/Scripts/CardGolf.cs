using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//An enum defines a variable type with a few prenamed values //a
public enum eCardStateGolf
{
    drawpile,
    tableau,
    target,
    discard
}


public class CardGolf : Card
{
    [Header("Set Dynamically: CardGolf")]
    //This is how you ise the enum eCardState
    public eCardStateGolf state = eCardStateGolf.drawpile;
    //The hiddenBy list stores which other cards will keep this one face down
    public List<CardGolf> hiddenBy = new List<CardGolf>();
    // the LayoutID matches this card to the tableau XML if its a tableau card
    public int layoutID;
    //the SlotDef class stores infromation pulled in from the LayoutXML <slot>
    public SlotDef slotDef;
    // Start is called before the first frame update

    //This allows the card to react to being clicked
    override public void OnMouseUpAsButton()
    {
        //Call the CardCliecked method on the Prospector singleton
        Golf.S.CardClicked(this);
        //Also call the base class (card.cs) version of this method
        base.OnMouseUpAsButton(); //a
    }

}
