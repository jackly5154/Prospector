﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour
{

    [Header("Set in Inspector")]
    //Suits
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;

    public Sprite[] faceSprites;
    public Sprite[] rankSprites;

    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;


    // Prefabs
    public GameObject prefabSprite;
    public GameObject prefabCard;

    [Header("Set Dynamically")]

    public PT_XMLReader xmlr;

    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;



    public void InitDeck(string deckXMLText)
    {

        if (GameObject.Find("_Deck") == null)
        {
            GameObject anchorGO = new GameObject("_Deck");
            deckAnchor = anchorGO.transform;
        }

        // init the Dictionary of suits
        dictSuits = new Dictionary<string, Sprite>() {
            {"C", suitClub},
            {"D", suitDiamond},
            {"H", suitHeart},
            {"S", suitSpade}
        };
        ReadDeck(deckXMLText);
        MakeCards();
    }


    // ReadDeck parses the XML file passed to it into Card Definitions
    public void ReadDeck(string deckXMLText)
    {
        xmlr = new PT_XMLReader();
        xmlr.Parse(deckXMLText);


        string s = "xml[0] decorator [0] ";
        s += "type=" + xmlr.xml["xml"][0]["decorator"][0].att("type");
        s += " x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
        s += " y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
        s += " scale=" + xmlr.xml["xml"][0]["decorator"][0].att("scale");
        print(s);

        //Read decorators for all cards

        decorators = new List<Decorator>();

        PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
        Decorator deco;
        for (int i = 0; i < xDecos.Count; i++)
        {
            // for each decorator in the XML, copy attributes and set up location and flip if needed
            deco = new Decorator();
            deco.type = xDecos[i].att("type");
            deco.flip = (xDecos[i].att("flip") == "1");
            deco.scale = float.Parse(xDecos[i].att("scale"));
            deco.loc.x = float.Parse(xDecos[i].att("x"));
            deco.loc.y = float.Parse(xDecos[i].att("y"));
            deco.loc.z = float.Parse(xDecos[i].att("z"));
            decorators.Add(deco);
        }


        cardDefs = new List<CardDefinition>();
        PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];

        for (int i = 0; i < xCardDefs.Count; i++)
        {
            // for each carddef in the XML, copy attributes and set up in cDef
            CardDefinition cDef = new CardDefinition();
            cDef.rank = int.Parse(xCardDefs[i].att("rank"));

            PT_XMLHashList xPips = xCardDefs[i]["pip"];
            if (xPips != null)
            {
                for (int j = 0; j < xPips.Count; j++)
                {
                    deco = new Decorator();
                    deco.type = "pip";
                    deco.flip = (xPips[j].att("flip") == "1");   // too cute by half - if it's 1, set to 1, else set to 0

                    deco.loc.x = float.Parse(xPips[j].att("x"));
                    deco.loc.y = float.Parse(xPips[j].att("y"));
                    deco.loc.z = float.Parse(xPips[j].att("z"));
                    if (xPips[j].HasAtt("scale"))
                    {
                        deco.scale = float.Parse(xPips[j].att("scale"));
                    }
                    cDef.pips.Add(deco);
                } 
            }
            if (xCardDefs[i].HasAtt("face"))
            {
                cDef.face = xCardDefs[i].att("face");
            }
            cardDefs.Add(cDef);
        } // for i < xCardDefs.Count
    } // ReadDeck

    public CardDefinition GetCardDefinitionByRank(int rnk)
    {
        foreach (CardDefinition cd in cardDefs)
        {
            if (cd.rank == rnk)
            {
                return (cd);
            }
        }
        return (null);
    }


    public void MakeCards()
    {
        cardNames = new List<string>();
        string[] letters = new string[] { "C", "D", "H", "S" };
        foreach (string s in letters)
        {
            for (int i = 0; i < 13; i++)
            {
                cardNames.Add(s + (i + 1));
            }
        }

        // list of all Cards
        cards = new List<Card>();
        Sprite tS = null;
        GameObject tGO = null;
        SpriteRenderer tSR = null;

        for (int i = 0; i < cardNames.Count; i++)
        {
            GameObject cgo = Instantiate(prefabCard) as GameObject;
            cgo.transform.parent = deckAnchor;
            Card card = cgo.GetComponent<Card>();

            cgo.transform.localPosition = new Vector3(i % 13 * 3, i / 13 * 4, 0);

            card.name = cardNames[i];
            card.suit = card.name[0].ToString();
            card.rank = int.Parse(card.name.Substring(1));

            if (card.suit == "D" || card.suit == "H")
            {
                card.colS = "Red";
                card.color = Color.red;
            }

            card.def = GetCardDefinitionByRank(card.rank);

            // Add Decorators
            foreach (Decorator deco in decorators)
            {
                tGO = Instantiate(prefabSprite) as GameObject;
                tSR = tGO.GetComponent<SpriteRenderer>();
                if (deco.type == "suit")
                {
                    tSR.sprite = dictSuits[card.suit];
                }
                else
                {
                    tS = rankSprites[card.rank];
                    tSR.sprite = tS;
                    tSR.color = card.color;
                }

                tSR.sortingOrder = 1;
                tGO.transform.parent = cgo.transform;  
                tGO.transform.localPosition = deco.loc;  

                if (deco.flip)
                {
                    tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
                }

                if (deco.scale != 1)
                {
                    tGO.transform.localScale = Vector3.one * deco.scale;
                }

                tGO.name = deco.type;

                card.decoGOs.Add(tGO);
            }


            //Add the pips
            foreach (Decorator pip in card.def.pips)
            {
                tGO = Instantiate(prefabSprite) as GameObject;
                tGO.transform.parent = cgo.transform;
                tGO.transform.localPosition = pip.loc;

                if (pip.flip)
                {
                    tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
                }

                if (pip.scale != 1)
                {
                    tGO.transform.localScale = Vector3.one * pip.scale;
                }

                tGO.name = "pip";
                tSR = tGO.GetComponent<SpriteRenderer>();
                tSR.sprite = dictSuits[card.suit];
                tSR.sortingOrder = 1;
                card.pipGOs.Add(tGO);
            }


            if (card.def.face != "")
            {
                tGO = Instantiate(prefabSprite) as GameObject;
                tSR = tGO.GetComponent<SpriteRenderer>();

                tS = GetFace(card.def.face + card.suit);
                tSR.sprite = tS;
                tSR.sortingOrder = 1;
                tGO.transform.parent = card.transform;
                tGO.transform.localPosition = Vector3.zero; 
                tGO.name = "face";
            }

            tGO = Instantiate(prefabSprite) as GameObject;
            tSR = tGO.GetComponent<SpriteRenderer>();
            tSR.sprite = cardBack;
            tGO.transform.SetParent(card.transform);
            tGO.transform.localPosition = Vector3.zero;
            tSR.sortingOrder = 2;
            tGO.name = "back";
            card.back = tGO;
            card.faceUp = false;

            cards.Add(card);
        }
    }

    //Find the proper face card
    public Sprite GetFace(string faceS)
    {
        foreach (Sprite tS in faceSprites)
        {
            if (tS.name == faceS)
            {
                return (tS);
            }
        }
        return (null);
    }

    static public void Shuffle(ref List<Card> oCards)
    {
        List<Card> tCards = new List<Card>();

        int ndx;

        while (oCards.Count > 0)
        {
            ndx = Random.Range(0, oCards.Count);
            tCards.Add(oCards[ndx]);
            oCards.RemoveAt(ndx);
        }

        oCards = tCards;


    }


}