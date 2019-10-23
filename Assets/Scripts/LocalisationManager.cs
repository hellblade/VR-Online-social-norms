﻿using UnityEngine;
using Lean.Localization;
using System.IO;
using System.Collections.Generic;

public class LocalisationManager : LeanLocalization
{

    public static string LanguagesPath
    {
        get { return Path.Combine(Application.streamingAssetsPath, "Languages"); }
    }

    static bool areLanguagesCreated = false;
    static readonly List<string> languages = new List<string>();

    private void Awake()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            return;
#endif

        enabled = true;

        LoadLanguages();

        foreach (var language in languages)
        {
            Languages.Add(new LeanLanguage() { Name = language });
        }
    }

    private void LoadLanguages()
    {
        if (areLanguagesCreated)
            return;

        foreach (var file in Directory.EnumerateFiles(LanguagesPath, "*.txt", SearchOption.TopDirectoryOnly))
        {
            var language = Path.GetFileNameWithoutExtension(file);

            var languageObject = new GameObject("Language: " + language);
            DontDestroyOnLoad(languageObject);

            var languageCSV = languageObject.AddComponent<LeanLanguageCSV>();
            languageCSV.Source = new TextAsset(File.ReadAllText(file));
            languageCSV.Language = language;

            languages.Add(language);
        }

        areLanguagesCreated = true;
    }


}
