using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;

public class AuthHandler : MonoBehaviour
{
    private TMP_InputField userNameInputField, passwordInputField;
    public string apiUrl = "https://sid-restapi.onrender.com/api";
    
    // Start is called before the first frame update
    void Start()
    {
        userNameInputField = GameObject.Find("Username").GetComponent<TMP_InputField>();
        passwordInputField = GameObject.Find("Password").GetComponent<TMP_InputField>();
    }

    public void Register()
    {
        AuthData authData = new AuthData();
        authData.username = userNameInputField.text;
        authData.password = passwordInputField.text;

        string json = JsonUtility.ToJson(authData);

        StartCoroutine(SendRegister(json));
    }

    IEnumerator SendRegister(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(apiUrl + "/usuarios", json);
        request.SetRequestHeader("Content-Type", "application/json");
        request.method = "POST";
        yield return request.SendWebRequest();

        if (request.isNetworkError)
        {
            Debug.Log("NETWORK ERROR" + request.error);
        }

        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthData data = JsonUtility.FromJson<AuthData>(request.downloadHandler.text);
                Debug.Log("Usuario registrado con el ID " + data.usuario._id);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
}

[System.Serializable]
public class AuthData
{
    public string username, password;
    public UserData usuario;
}

[System.Serializable]
public class UserData
{
    public string _id, username;
    public bool estado;
}
