using System;
using System.ComponentModel;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Signals;

public static class HighScoreManagement
{
    public static void ResetLevelScores()
    {
        PlayerPrefs.SetString("LevelScores", "{}");
        PlayerPrefs.SetInt("tutorialUpTo", 0);
        PlayerPrefs.Save();
    }

    // Check new score against existing highscore for a level. Return the original highscore or new highscore as int.
    public static (bool, int) TryAddScoreThenReturnHighscore(string levelName, GameEndCondition scoreType, int score)
    {
        string jsonString = PlayerPrefs.GetString("LevelScores");

        // Load the list of level scores from JSON
        Highscores levelScores;
        if (jsonString != "")
            levelScores = JsonUtility.FromJson<Highscores>(jsonString);
        else
            levelScores = new Highscores { highscoreEntryList = new List<HighscoreEntry>() };

        // When adding a high score, take everything from the old list EXCEPT the high score for the current level.
        List<HighscoreEntry> listOfLevelScores = levelScores.highscoreEntryList.Where(hs => hs.levelName != levelName).ToList();

        // Find the high score for the current level. If two or more exist, flag an error.
        int numOfCurrentLevelEntries = levelScores.highscoreEntryList.Where(hs => hs.levelName == levelName).Count();
        HighscoreEntry currentLevelScore;
        if (numOfCurrentLevelEntries == 1)
        {
            // If one score exists, then overwrite or notify that the score wasn't beaten.
            currentLevelScore = levelScores.highscoreEntryList.Where(hs => hs.levelName == levelName).ToList()[0];
            if (scoreType == GameEndCondition.WinType1 && score > currentLevelScore.scoreType1)
                currentLevelScore.scoreType1 = score;
            else if (scoreType == GameEndCondition.WinType2 && score > currentLevelScore.scoreType2)
                currentLevelScore.scoreType2 = score;
            else
            {
                int highscoreToReturn = scoreType == GameEndCondition.WinType1 ? currentLevelScore.scoreType1 : currentLevelScore.scoreType2;
                return (false, highscoreToReturn);
            }
        }
        else
        {
            // If zero scores exist, set this victory to the high score.
            // If two or more scores exist, flag an error, and set this victory to the high score.
            if (numOfCurrentLevelEntries != 0)
                Debug.LogError($"Level '{levelName}': Multiple HighscoreEntry entries in PlayerPrefs. This isn't expected. Overwrite them all with latest score.");

            if (scoreType == GameEndCondition.WinType1)
                currentLevelScore = new HighscoreEntry { levelName = levelName, scoreType1 = score, scoreType2 = -1 };
            else if (scoreType == GameEndCondition.WinType2)
                currentLevelScore = new HighscoreEntry { levelName = levelName, scoreType1 = -1, scoreType2 = score };
            else
                throw new InvalidEnumArgumentException();
            score = -1; // Flag highscore as the first attempt at a level, don't congratulate for new high score
        }

        listOfLevelScores.Add(currentLevelScore);
        Highscores fullListOfScores = new Highscores { highscoreEntryList = listOfLevelScores };

        string json = JsonUtility.ToJson(fullListOfScores);
        Debug.Log(json);
        PlayerPrefs.SetString("LevelScores", json);
        PlayerPrefs.Save();
        return (true, score);
    }
}

/* ------------------------------------------------------------------------------------------------------------------
 * Highscores & HighscoreEntry - Public classes for an entire list of highscores, and a single highscore entry
 * ------------------------------------------------------------------------------------------------------------------ */

[Serializable]
public class Highscores
{
    public List<HighscoreEntry> highscoreEntryList;
}

// Represents a single Highscore Entry
[Serializable]
public class HighscoreEntry
{
    public string levelName;
    public int scoreType1;
    public int scoreType2;
}
