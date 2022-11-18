using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//An enum defines a variable type with a few prenamed values //a
public enum eCardState
{
    drawpile,
    tableau,
    target,
    discard
}


public class CardProspector : Card
{
    [Header("Set Dynamically: CardProspector")]
    //This is how you ise the enum eCardState
    public eCardState state = eCardState.drawpile;
    //The hiddenBy list stores which other cards will keep this one face down
    public List<CardProspector> hiddenBy = new List<CardProspector>();
    // the LayoutID matches this card to the tableau XML if its a tableau card
    public int layoutID;
    //the SlotDef class stores infromation pulled in from the LayoutXML <slot>
    public SlotDef slotDef;
    // Start is called before the first frame update

    //This allows the card to react to being clicked
    override public void OnMouseUpAsButton()
    {
        //Call the CardCliecked method on the Prospector singleton
        Prospector.S.CardClicked(this);
        //Also call the base class (card.cs) version of this method
        base.OnMouseUpAsButton(); //a
    }

}
