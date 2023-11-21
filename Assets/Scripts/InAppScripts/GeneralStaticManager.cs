using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

public static class GeneralStaticManager 
{
    public static Dictionary<string, string> GlobalVar = new Dictionary<string, string>();

    public static string AddSpacesToSentence(string text, bool preserveAcronyms)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;
        StringBuilder newText = new StringBuilder(text.Length * 2);
        newText.Append(text[0]);
        for (int i = 1; i < text.Length; i++)
        {
            if (char.IsUpper(text[i]))
                if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                    (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                     i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                    newText.Append(' ');
            newText.Append(text[i]);
        }
        return newText.ToString();
    }

    [DllImport("__Internal")]
    private static extern void _OpenFile(string path);

    public static void OpenFile(string path)
    {
#if UNITY_IOS && !UNITY_EDITOR
        _OpenFile(path);
#else
        Debug.Log("Only IOS");
#endif
    }
}
