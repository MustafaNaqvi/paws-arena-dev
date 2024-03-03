using System;
using System.Collections.Generic;

[Serializable]
public class DailyChallenges
{
    public List<ChallengeData> Challenges = new ();
    public DateTime NextReset = DateTime.MinValue;
}
