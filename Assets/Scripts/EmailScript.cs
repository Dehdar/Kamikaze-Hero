using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Opens the player's mail app with a pre-filled message to the developer
/// (wired to the contact button in the About scene).
/// </summary>
public class EmailScript : MonoBehaviour
{
    // Change these in the inspector if you publish your own version of the game.
    public string ContactAddress = "kamikazehero@dehdar.net";
    public string Subject = "Kamikaze Hero";

    public void SendEmail()
    {
        var subject = EscapeForMailto(Subject);
        var body = EscapeForMailto("");
        Application.OpenURL("mailto:" + ContactAddress + "?subject=" + subject + "&body=" + body);
    }

    // EscapeURL encodes spaces as "+", which mail apps show literally; mailto needs "%20".
    private static string EscapeForMailto(string text)
    {
        return UnityWebRequest.EscapeURL(text).Replace("+", "%20");
    }
}
