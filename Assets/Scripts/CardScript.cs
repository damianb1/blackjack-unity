using UnityEngine;

public class CardScript : MonoBehaviour
{   
    public enum Rank { Ace, King, Queen, Jack, 
        Ten, Nine, Eight, Seven, Six, Five, Four, Three, Two };
    public enum Suit { Hearts, Diamonds, Clubs, Spades };

    public Rank rank;
    public Suit suit;

    private bool rotateFaceUp = false, rotateFaceDown = false;

    private Quaternion targetRotationUp, targetRotationDown;

    private bool isMoving = false;
    private Vector3 targetPosition;
    private float moveSpeed = 10f;

    private float step = 100f;
    
    private Vector3 rotationMoveVector = new Vector3(2.5f, 0, -2f);

    private bool isFaceUp;
    

    // Start is called before the first frame update
    void Start()
    {
        targetPosition = transform.position;
         
        targetRotationUp = Quaternion.Euler(0, 0, 0);
        targetRotationDown = Quaternion.Euler(0, -179, 0);

        // initial rotation (face down)
        transform.rotation = targetRotationDown;
        isFaceUp = false;

    }

    // Update is called once per frame
    void Update()
    {

        if (rotateFaceUp)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotationUp, step * Time.deltaTime);
                     
        }
        else if (rotateFaceDown)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotationDown, step * Time.deltaTime);
            
        }

        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed*Time.deltaTime);
            if (transform.position == targetPosition) isMoving = false;
        }


    }

    public void RotateCard(bool faceUp)
    {

        if (faceUp)
        {
            if (isFaceUp) return;

            rotateFaceUp = true;
            rotateFaceDown = false;
            targetPosition = targetPosition + rotationMoveVector;
            isMoving = true;

            isFaceUp = true;
            
        }
        else
        {
            if (!isFaceUp) return;

            rotateFaceDown = true;
            rotateFaceUp = false;
            targetPosition = targetPosition - rotationMoveVector;
            isMoving = true;

            isFaceUp = false;

        }

    }

    public void MoveCard(Vector3 targetPosition)
    {
        this.targetPosition = targetPosition;
        this.isMoving = true;
    }

}

