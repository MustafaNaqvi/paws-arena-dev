using System;

[Serializable]
public class ChallengeData
{
    public string Identifier;
    public int Id;
    public string Description;
    public int AmountNeeded;
    public int RewardAmount;
    public ItemType RewardType;
    public ChallengeCategory Category;
}