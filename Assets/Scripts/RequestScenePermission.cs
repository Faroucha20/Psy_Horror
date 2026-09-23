using UnityEngine;
using UnityEngine.Android;

public class RequestScenePermission : MonoBehaviour
{
    void Start()
    {
        if (!Permission.HasUserAuthorizedPermission(
            "com.oculus.permission.USE_SCENE"))
        {
            Permission.RequestUserPermission(
                "com.oculus.permission.USE_SCENE");
        }
    }
}