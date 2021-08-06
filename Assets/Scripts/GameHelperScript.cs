using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Networking;


public class GameHelperScript : MonoBehaviour
{
    private enum PlayerMove { None, Hit, Stand, Double};
    private enum Result { None, Player, Dealer, Draw, PlayerBlackjack };

    private enum EndOption { None, PlayAgain, CashOut, StartAgain, End };

    public GameObject player, dealer;
    public GameObject cards;
    private List<GameObject> deck = new List<GameObject>();
    
    private Vector3 deckPosition = new Vector3(3, 0, 0);


    private bool handInProgress = false;

    private bool playersTurn = false;
    private PlayerMove userMove = PlayerMove.None;

    public TextMeshProUGUI playersScoreText, dealersScoreText, resultText;

    public TextMeshProUGUI betText, playersMoneyText;

    public TextMeshProUGUI bbDealerText, bbPlayerText;



    private int bet = 0;
    private int tempBet = 0;

    private bool isBetPlaced = false;
    private bool isBettingPhase = false;


    public GameObject betControls, chipButtons, turnButtons, scoresTexts;
    public GameObject doubleButton;

    public GameObject playAgainButton, cashOutButton, startAgainButton, endButton, continueButton;

    private static System.Random rng = new System.Random();

    private bool isContinuing;

    private EndOption endOption = EndOption.None;

    public GameObject startControls;
    public GameObject playButton;

    public InputField nameInput;

    private string userName;
    private bool playStarted = false;

    private bool savingFinished;

    [Serializable]
    public class GameScore
    {
        public string name;
        public int score;
    }

    void Start()
    {
        SetDeck();
        ShuffleCards();
        StartCoroutine(WaitForPlay());
    }

    void Update()
    {
 
    }

    void SetDeck()
    {
        for (int i = 0; i < cards.transform.childCount; i++)
        {
            deck.Add(cards.transform.GetChild(i).gameObject);
        }
        float x=deckPosition.x, y=deckPosition.y, z=deckPosition.z;
        foreach(GameObject card in deck){

            card.transform.position = new Vector3(x, y, z);

            x += 0.01f;
            y += 0.01f;
            z -= 0.01f;
            
        }
    }
    
    void ShuffleCards()
    {
   
        // add shuffling animation ??

        deck = deck.OrderBy(a => rng.Next()).ToList();

        float x = deckPosition.x, y = deckPosition.y, z = deckPosition.z;
        foreach (GameObject card in deck)
        {

            card.transform.position = new Vector3(x, y, z);

            x += 0.01f;
            y += 0.01f;
            z -= 0.01f;

        }
        
    }

    private void ResetScores()
    {
        player.GetComponent<PlayerScript>().Score = 0;
        dealer.GetComponent<PlayerScript>().Score = 0;
    }

    IEnumerator WaitForPlay()
    {
        playStarted = false;

        turnButtons.SetActive(false);
        betControls.SetActive(false);
        chipButtons.SetActive(false);
        scoresTexts.SetActive(false);
        cards.SetActive(false);

        playersMoneyText.enabled = false;
        betText.enabled = false;

        startControls.SetActive(true);

        yield return new WaitUntil(() => playStarted);
        
        startControls.SetActive(false);
        cards.SetActive(true);

        player.GetComponent<PlayerScript>().Money = 1000;
        dealer.GetComponent<PlayerScript>().Money = 100000;
        
        StartCoroutine(BettingPhase());
    }

    IEnumerator BettingPhase()
    {
        ShuffleCards();
        ResetScores();
        
        playersMoneyText.enabled = true;
        betText.enabled = true;
        UpdatePlayersMoneyText();
        turnButtons.SetActive(false);
        betControls.SetActive(true);
        chipButtons.SetActive(true);
        scoresTexts.SetActive(false);
        isBettingPhase = true;
        isBetPlaced = false;
        yield return new WaitUntil(() => isBetPlaced);
        isBettingPhase = false;
        betControls.SetActive(false);
        chipButtons.SetActive(false);
        StartCoroutine(InitialDeal());
    }

    IEnumerator InitialDeal()
    {
        
        yield return new WaitForSeconds(0.5f);
        for (int i = 0; i< 2; i++)
        {

            DealACard(dealer);
            yield return new WaitForSeconds(0.5f);
            DealACard(player);
            yield return new WaitForSeconds(0.5f);

        }
        scoresTexts.SetActive(true);
        handInProgress = true;
        
        InitialReveal();
        UpdatePlayersScoreText();
        UpdateDealersScoreText();
        turnButtons.SetActive(true);
        StartCoroutine(PlayHand());
        
    }

    IEnumerator PlayHand()
    {
        
        if (IsBlackjack(player) || IsBlackjack(dealer))
        {
            turnButtons.SetActive(false);

            if (IsBlackjack(player))
            {
                yield return new WaitForSeconds(0.5f);
                bbPlayerText.text = "Blackjack";
                bbPlayerText.color = Color.blue;

            }
            handInProgress = false;
            
        }
        
        while (handInProgress)

        {
            if (doubleButton.activeSelf)
            {
                var playerSc = player.GetComponent<PlayerScript>();

                if (playerSc.Money < bet || playerSc.Hand.Count > 2 || playerSc.Score > 20)
                {
                    doubleButton.SetActive(false);
                }
            }

            userMove = PlayerMove.None;
            playersTurn = true;
            yield return new WaitUntil(() => !playersTurn);
          
            if (userMove == PlayerMove.Hit)
            {
                DealACard(player);
                yield return new WaitForSeconds(0.5f);
                UpdatePlayersScoreText();
                if (IsBust(player))
                {
                    handInProgress = false;
                    bbPlayerText.text = "Bust";
                    bbPlayerText.color = Color.red;
                }

                if (GetScore(player) == 21)
                {
                    break;
                }

            }
            else if (userMove == PlayerMove.Stand)
            {
                
                break;
            }
            else if (userMove == PlayerMove.Double)
            {
                player.GetComponent<PlayerScript>().Money -= bet;
                bet += bet;
                UpdateBetText(bet);
                UpdatePlayersMoneyText();
                DealACard(player);
                yield return new WaitForSeconds(0.5f);
                UpdatePlayersScoreText();
                if (IsBust(player)) handInProgress = false;
                break; 
            }

        }

        turnButtons.SetActive(false);

        while (handInProgress)
        {
            
            if (dealer.GetComponent<PlayerScript>().Score < 17)
            {
                DealACard(dealer);
                yield return new WaitForSeconds(0.5f);
                UpdateDealersScoreText();
            }
            else
            {
                handInProgress = false;
                break;
            }

            if (IsBust(dealer))
            {
                handInProgress = false;
                
            }
                                
        }
        
        StartCoroutine(Showdown());
        
    }

    IEnumerator Showdown()
    {
        yield return new WaitForSeconds(0.5f);
        RevealAll();
        yield return new WaitForSeconds(2);
        Result result = Result.None;

        if (IsBlackjack(dealer))
        {
            yield return new WaitForSeconds(0.5f);
            bbDealerText.text = "Blackjack";
            bbDealerText.color = Color.blue;
        }
        if (IsBlackjack(player))
        {
            if (IsBlackjack(dealer))
            {
                result = Result.Draw;
            }
            else
            {
                result = Result.PlayerBlackjack;
            }
            
        }
        else if (IsBlackjack(dealer))
        {
             result = Result.Dealer;
        }

        else if (IsBust(player))
        {
            result = Result.Dealer;
        }
        else if (IsBust(dealer))
        {
            result = Result.Player;
            bbDealerText.text = "Bust";
            bbDealerText.color = Color.red;
            
        }
        else
        {
            if (GetScore(player) > GetScore(dealer))
            {
                result = Result.Player;
            }
            else if (GetScore(player) < GetScore(dealer))
            {
                result = Result.Dealer;
            }
            else if(GetScore(player) == GetScore(dealer))
            {
                result = Result.Draw;
            }
        }

        switch (result)
        {
            case Result.Dealer:
                dealer.GetComponent<PlayerScript>().Money += bet;
                resultText.text = "Dealer wins";
                break;
            case Result.Player:
                player.GetComponent<PlayerScript>().Money += bet*2;
                dealer.GetComponent<PlayerScript>().Money -= bet; // ?
                resultText.text = "Player wins "+(bet*2).ToString()+"$";
                break;
            case Result.Draw:
                player.GetComponent<PlayerScript>().Money += bet;
                resultText.text = "Push";
                break;
            case Result.PlayerBlackjack:
                int wonMoney = (int)(bet * 2.5);
                player.GetComponent<PlayerScript>().Money += wonMoney;
                resultText.text = "Player wins " + wonMoney.ToString()+"$";
                break;
        }
        bet = 0;
        yield return new WaitForSeconds(1);
        UpdateBetText(bet);
        UpdatePlayersMoneyText();
        StartCoroutine(AfterHand());
    }

    IEnumerator AfterHand()
    {
        yield return new WaitForSeconds(0.5f);
        continueButton.SetActive(true);
        isContinuing = false;
        yield return new WaitUntil(() => isContinuing);

        continueButton.SetActive(false);
        bbDealerText.text = "";
        bbPlayerText.text = "";
        betText.text = "";
        resultText.text = "";
        scoresTexts.SetActive(false);
        GetCards(dealer);
        GetCards(player);

        endOption = EndOption.None;

        yield return new WaitForSeconds(0.5f);

        if (player.GetComponent<PlayerScript>().Money < 10)
        {

            startAgainButton.SetActive(true);
            endButton.SetActive(true);
        }
        else
        {
            playAgainButton.SetActive(true);
            cashOutButton.SetActive(true);
        }

        yield return new WaitUntil(() => endOption != EndOption.None);

        startAgainButton.SetActive(false);
        endButton.SetActive(false);
        playAgainButton.SetActive(false);
        cashOutButton.SetActive(false);


        switch (endOption)
        {
            case EndOption.PlayAgain:
                StartCoroutine(BettingPhase());
                break;
            case EndOption.CashOut:

                GameScore gameScore = new GameScore();
                gameScore.name = userName;
                gameScore.score = player.GetComponent<PlayerScript>().Money;

                string json = JsonUtility.ToJson(gameScore);
                resultText.text = "Score: " + player.GetComponent<PlayerScript>().Money.ToString() +
                    "\n saving...";

                savingFinished = false;
                StartCoroutine(PostRequest("https://blackjack-game-test.herokuapp.com/post_score", json));

                yield return new WaitForSeconds(0.5f);
                yield return new WaitUntil(() => savingFinished);

                resultText.text = "";
                StartCoroutine(WaitForPlay());

                break;
            case EndOption.StartAgain:
                player.GetComponent<PlayerScript>().Money = 1000;
                dealer.GetComponent<PlayerScript>().Money = 100000;
                StartCoroutine(BettingPhase());
                break;
            case EndOption.End:
                StartCoroutine(WaitForPlay());
                break;
        } 

    }

    private void UpdatePlayersScoreText()
    {
        playersScoreText.text = "Score:\n" + GetScore(player).ToString();
    }
    private void UpdateDealersScoreText()
    {
        int score;
        if (handInProgress)
        {

            score = dealer.GetComponent<PlayerScript>().GetCardScore(
                dealer.GetComponent<PlayerScript>().Hand[0]);
            

        }
        else score = GetScore(dealer);

        dealersScoreText.text = "Score:\n" + score.ToString();
    }

    private int GetScore(GameObject person)
    {
        return person.GetComponent<PlayerScript>().Score;
    }

    private bool IsBust(GameObject person)
    {
        int score = GetScore(person);
        if (score > 21) return true;
        else return false;
        
    }

    private bool IsBlackjack(GameObject person)
    {
        if (person.GetComponent<PlayerScript>().Hand.Count == 2 && GetScore(person) == 21)
        {
            return true;
        }
        else return false;

    }

    private void InitialReveal()
    {
        foreach(GameObject card in player.GetComponent<PlayerScript>().Hand)
        {
            card.GetComponent<CardScript>().RotateCard(true);
        }
        dealer.GetComponent<PlayerScript>().Hand[0].GetComponent<CardScript>().RotateCard(true);

    }

    private void RevealAll()
    {
        foreach (GameObject card in player.GetComponent<PlayerScript>().Hand)
        {
            card.GetComponent<CardScript>().RotateCard(true);
        }
        foreach (GameObject card in dealer.GetComponent<PlayerScript>().Hand)
        {
            card.GetComponent<CardScript>().RotateCard(true);
        }

        UpdateDealersScoreText();
    }

    private void DealACard(GameObject person)
    {
        if (deck.Count < 1) return;
        GameObject card = deck[deck.Count - 1].gameObject;

        deck.RemoveAt(deck.Count - 1);

        person.GetComponent<PlayerScript>().AddCard(card);
        Vector3 cardPosition = person.GetComponent<PlayerScript>().CardPosition;
        card.GetComponent<CardScript>().MoveCard(cardPosition);

        if (handInProgress && person.tag == "PlayerTag")
        {
            card.GetComponent<CardScript>().RotateCard(true);
        }

    }

    private void GetCards(GameObject person)
    {
        List<GameObject> returnedCards = person.GetComponent<PlayerScript>().ReturnCards();
        deck.AddRange(returnedCards);
        
        foreach(GameObject card in returnedCards)
        {
            card.GetComponent<CardScript>().RotateCard(false);
            card.GetComponent<CardScript>().MoveCard(deckPosition);
        }

    }

    public void OnBetButtonClick()
    {

        if (!isBettingPhase) return;
        if (tempBet == 0) return;
        bet = tempBet;
        tempBet = 0;
        isBetPlaced = true;
    }

    public void OnClearButtonClick()
    {
        if (!isBettingPhase) return;
        if (tempBet == 0) return;
        player.GetComponent<PlayerScript>().Money += tempBet;
        tempBet = 0;
        UpdateBetText(tempBet);
        UpdatePlayersMoneyText();
    }

    public void OnHitButtonClick()
    {
        if (playersTurn)
        {
 
          userMove = PlayerMove.Hit;
          playersTurn = false;
  
        }
    }

    public void OnStandButtonClick()
    {
        if (playersTurn)
        {
            
            userMove = PlayerMove.Stand;
            playersTurn = false;
            
        }
    }
    public void OnDoubleButtonClick()
    {
        if (!playersTurn) return;
        if (player.GetComponent<PlayerScript>().Hand.Count > 2) return;
        if (player.GetComponent<PlayerScript>().Money < bet) return;

        userMove = PlayerMove.Double;
        playersTurn = false;
    }

    public void OnChipButtonClick(GameObject chipButton)
    {
        if(!isBettingPhase) return;
        
        int chipValue = Int16.Parse(chipButton.tag.Substring(4));
        if (chipValue > player.GetComponent<PlayerScript>().Money) return;
        tempBet += chipValue;
        player.GetComponent<PlayerScript>().Money -= chipValue;
        UpdatePlayersMoneyText();
        UpdateBetText(tempBet);

    }

    void UpdateBetText(int betValue)
    {
        betText.text = "Bet:\n" + betValue.ToString();
    }

    void UpdatePlayersMoneyText()
    {
        playersMoneyText.text = "Money:\n"+player.GetComponent<PlayerScript>().Money.ToString();
    }

    public void OnPlayAgainButtonClick()
    {
        endOption = EndOption.PlayAgain;

    }
    public void OnCashOutButtonClick()
    {
        endOption = EndOption.CashOut;

    }
    public void OnStartAgainButtonClick()
    {
        endOption = EndOption.StartAgain;
    }

    public void OnContinueButtonClick()
    {
        isContinuing = true;
    }

    public void OnEndButtonClick()
    {
        endOption = EndOption.End;

    }

    public void OnPlayButtonClick()
    {
        if (playStarted) return;
        if (nameInput.text.Length < 1)
        {
            return;
        }
            
        userName = nameInput.text;

        playStarted = true;
    }



    IEnumerator PostRequest(string url, string json)
    {
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
        }

        savingFinished = true;
    }


}
