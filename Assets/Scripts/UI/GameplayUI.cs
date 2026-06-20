using UnityEngine;

public class GameplayUI : MonoBehaviour
{
    [SerializeField] private BallThrower ballThrower;

    private void Awake()
    {
        if (ballThrower == null)
            ballThrower = FindFirstObjectByType<BallThrower>();
    }

    public void OnRecallBallClicked()
    {
        ballThrower?.RecallBallToHands();
    }
}
