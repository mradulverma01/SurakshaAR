using UnityEngine;

public class TrainingProgress : MonoBehaviour
{
    public static TrainingProgress Instance;

    [Header("Player Progress")]
    public int xp = 0;
    public int fireSafetyLevel = 0;

    [Header("Current Language")]
    public string language = "English";

    private const string XP_KEY = "SurakshaAR_XP";
    private const string FIRE_LEVEL_KEY = "SurakshaAR_FireLevel";
    private const string LANGUAGE_KEY = "SurakshaAR_Language";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        LoadProgress();
    }

    public void AddXP(int amount)
    {
        xp += amount;
        SaveProgress();

        Debug.Log("XP: " + xp);
    }

    public void CompleteFireLevel()
    {
        fireSafetyLevel++;

        SaveProgress();

        Debug.Log(
            "Fire Safety Level: " +
            fireSafetyLevel
        );
    }

    public bool IsFireLevelUnlocked(int level)
    {
        return fireSafetyLevel >= level;
    }

    public void SetLanguage(string newLanguage)
    {
        language = newLanguage;

        SaveProgress();
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(XP_KEY, xp);
        PlayerPrefs.SetInt(
            FIRE_LEVEL_KEY,
            fireSafetyLevel
        );

        PlayerPrefs.SetString(
            LANGUAGE_KEY,
            language
        );

        PlayerPrefs.Save();
    }

    private void LoadProgress()
    {
        xp = PlayerPrefs.GetInt(XP_KEY, 0);

        fireSafetyLevel =
            PlayerPrefs.GetInt(
                FIRE_LEVEL_KEY,
                0
            );

        language =
            PlayerPrefs.GetString(
                LANGUAGE_KEY,
                "English"
            );
    }
}