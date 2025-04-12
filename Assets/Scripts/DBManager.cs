using UnityEngine;

public class DBManager : MonoBehaviour
{

    public static string nickname;
    public static int score;

    public static bool LoggedIn { get { return nickname != null; } }

    public static void LogOut()
    {
        nickname = null;
    }
}
