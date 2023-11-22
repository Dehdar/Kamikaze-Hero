using UnityEngine;
using System.Collections;

public class EmailScript : MonoBehaviour {
    public void SendEmail()
    {
        string email = "kamikazehero@dehdar.net";
        string subject = MyEscapeURL("Kamikaze Hero");
        string body = MyEscapeURL("");
        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
    }
    string MyEscapeURL(string url)
    {
        return WWW.EscapeURL(url).Replace("+", "%20");
    }


    // Update is called once per frame
    void Update () {
	
	}
}
