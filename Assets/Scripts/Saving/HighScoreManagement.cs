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
    public static (bool, bool, int, int, bool, bool, bool) TryAddScoreThenReturnHighscore(string levelName, int bestCombo, int accuracy, int levelHighestComboPossible)
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
        bool bestComboBeaten = false, accuracyBeaten = false, isBrandNewScore = false;
        bool wasComboPerfect = false, wasAccuracyPerfect = false;
        if (numOfCurrentLevelEntries == 1)
        {
            // If one score exists, then overwrite or notify that the score wasn't beaten.
            currentLevelScore = levelScores.highscoreEntryList.Where(hs => hs.levelName == levelName).ToList()[0];
            bool ifScoreSameOrWorse = bestCombo <= currentLevelScore.bestCombo && accuracy <= currentLevelScore.accuracy;
            if (ifScoreSameOrWorse)
            {
                if (currentLevelScore.bestCombo >= levelHighestComboPossible)
                    wasComboPerfect = true;
                if (currentLevelScore.accuracy == 100)
                    wasAccuracyPerfect = true;
                // Don't bother to update PlayerPrefs.LevelScores
                return (false, false, currentLevelScore.bestCombo, currentLevelScore.accuracy, wasComboPerfect, wasAccuracyPerfect, isBrandNewScore);
            }
            if (bestCombo > currentLevelScore.bestCombo)
            {
                bestComboBeaten = true;
                currentLevelScore.bestCombo = bestCombo;
            }
            if (accuracy > currentLevelScore.accuracy)
            {
                accuracyBeaten = true;
                currentLevelScore.accuracy = accuracy;
            }
        }
        else
        {
            // If zero scores exist, set this victory to the high score.
            // If two or more scores exist, flag an error, and set this victory to the high score.
            if (numOfCurrentLevelEntries != 0)
                Debug.LogError($"Level '{levelName}': Multiple HighscoreEntry entries in PlayerPrefs. This isn't expected. Overwrite them all with latest score.");

            currentLevelScore = new HighscoreEntry { levelName = levelName, bestCombo = bestCombo, accuracy = accuracy };
            isBrandNewScore = true;
        }

        if (currentLevelScore.bestCombo >= levelHighestComboPossible)
            wasComboPerfect = true;
        if (currentLevelScore.accuracy == 100)
            wasAccuracyPerfect = true;

        listOfLevelScores.Add(currentLevelScore);
        Highscores fullListOfScores = new Highscores { highscoreEntryList = listOfLevelScores };

        string json = JsonUtility.ToJson(fullListOfScores);
        Debug.Log(json);
        PlayerPrefs.SetString("LevelScores", json);
        PlayerPrefs.Save();
        return (bestComboBeaten, accuracyBeaten, currentLevelScore.bestCombo, currentLevelScore.accuracy, wasComboPerfect, wasAccuracyPerfect, isBrandNewScore);
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
    public int bestCombo;
    public int accuracy;
}
