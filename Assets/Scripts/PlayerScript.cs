using System.Collections.Generic;
using UnityEngine;
using Rank = CardScript.Rank;

public class PlayerScript : MonoBehaviour
{

    public List<GameObject> Hand { get; set; }
    public Vector3 CardPosition { get; set; }

    public int Money { get; set; }

    public int Score { get; set; }

    // Start is called before the first frame update
    void Start()
    {
        Hand = new List<GameObject>();
        CardPosition = transform.position;
       
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddCard(GameObject card)
    {
        
        Hand.Add(card);
        CardPosition += new Vector3(1, 0, -0.1f);
        CountScore();
        
    }

    public List<GameObject> ReturnCards()
    {
        List<GameObject> tempCards = new List<GameObject>(Hand);
        Hand.Clear();
        CardPosition = transform.position;
        return tempCards;
    }

    void CountScore()
    {
        
        int score = 0;
        int aces = 0;
        foreach(GameObject c in Hand)
        {
            int cardScore = 0;
            switch (c.GetComponent<CardScript>().rank)
            {
                case Rank.Ace:
                    cardScore = 11;
                    aces++;
                    break;
                case Rank.King:
                case Rank.Queen:
                case Rank.Jack:
                case Rank.Ten:
                    cardScore = 10;
                    break;
                case Rank.Nine:
                    cardScore = 9;
                    break;
                case Rank.Eight:
                    cardScore = 8;
                    break;
                case Rank.Seven:
                    cardScore = 7;
                    break;
                case Rank.Six:
                    cardScore = 6;
                    break;
                case Rank.Five:
                    cardScore = 5;
                    break;
                case Rank.Four:
                    cardScore = 4;
                    break;
                case Rank.Three:
                    cardScore = 3;
                    break;
                case Rank.Two:
                    cardScore = 2;
                    break;
            }
            score += cardScore;
        }

        while (score > 21)
        {
            if (aces < 1) break;
            score -= 10;
            aces--;
        }
        Score = score;

    }
    public int GetCardScore(GameObject card)
    {
        int cardScore = 0;
        switch (card.GetComponent<CardScript>().rank)
        {
            case Rank.Ace:
                cardScore = 11;
                break;
            case Rank.King:
            case Rank.Queen:
            case Rank.Jack:
            case Rank.Ten:
                cardScore = 10;
                break;
            case Rank.Nine:
                cardScore = 9;
                break;
            case Rank.Eight:
                cardScore = 8;
                break;
            case Rank.Seven:
                cardScore = 7;
                break;
            case Rank.Six:
                cardScore = 6;
                break;
            case Rank.Five:
                cardScore = 5;
                break;
            case Rank.Four:
                cardScore = 4;
                break;
            case Rank.Three:
                cardScore = 3;
                break;
            case Rank.Two:
                cardScore = 2;
                break;
        }
        return cardScore;
    }
 
    
}
