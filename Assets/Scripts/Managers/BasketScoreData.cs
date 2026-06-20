using System;

[Serializable]
public struct BasketScoreData
{
    public int Points;
    public float Distance;
    public int Tier;

    public BasketScoreData(int points, float distance, int tier)
    {
        Points = points;
        Distance = distance;
        Tier = tier;
    }
}
