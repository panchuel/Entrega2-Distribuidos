using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class AuthHandler : MonoBehaviour
{
    private TMP_InputField userNameInputField, passwordInputField;
    public string apiUrl = "https://sid-restapi.onrender.com/api/";

    [Header("Local Stuff")]
    private string token, username;

    [SerializeField] private GameObject prende;
    
    // Start is called before the first frame update
    void Start()
    {
        token = PlayerPrefs.GetString("Token");
        
        if(string.IsNullOrEmpty(token)) Debug.Log("No hay token");
        else
        {
            username = PlayerPrefs.GetString("Username");
            StartCoroutine(GetProfile(username));
        }
        
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
    
    public void Login()
    {
        AuthData authData = new AuthData();
        authData.username = userNameInputField.text;
        authData.password = passwordInputField.text;

        string json = JsonUtility.ToJson(authData);

        StartCoroutine(SendLogin(json));
    }

    IEnumerator GetProfile(string user)
    {
        UnityWebRequest request = UnityWebRequest.Get($"{apiUrl}usuarios/{username}");
        request.SetRequestHeader("x-token", token);

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
                Debug.Log("Sesion activa con el usuario " + data.usuario.username);
                Debug.Log("Su score es " + data.usuario.data.score);
                prende.SetActive(true);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    
    IEnumerator SendRegister(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put($"{apiUrl}usuarios", json);
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
                prende.SetActive(false);
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    
    IEnumerator SendLogin(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(apiUrl + "auth/login", json);
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
                Debug.Log("Inicio de sesion con el usuario " + data.usuario.username + " y su token " + data.token);
                
                PlayerPrefs.SetString("Token", data.token);
                PlayerPrefs.SetString("Username", data.usuario.username);
                prende.SetActive(false);
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
    public string username, password, token;
    public User usuario;
    public User[] users;
}

[System.Serializable]
public class User
{
    public string _id, username;
    public bool estado;
    public DataUser data;
}

[System.Serializable]
public class DataUser
{
    public int score;
    public User[] friends;
}
