using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public class LeaderboardData
{
    public List<LeaderboardEntries> Entries = new ();
    public List<string> TopPlayers = new (); 

    public void FinishSetup(int _amountOfTopPlayers)
    {
        Entries = Entries.OrderByDescending(_entry => _entry.Points).ToList();
        for (int _i = 0; _i < _amountOfTopPlayers; _i++)
        {
            if (Entries.Count==_i)
            {
                break;
            }

            TopPlayers.Add(Entries[_i].KittyUrl);
        }
    }
}
