using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.SceneManagement;

public class AuthHandler : MonoBehaviour
{
    private TMP_InputField userNameInputField, passwordInputField;
    public string apiUrl = "https://sid-restapi.onrender.com/api/";

    [Header("Local Stuff")]
    private string token, username;

    [SerializeField] private GameObject prende;

    [Header("LeaderBoard Stuff")] 
    [SerializeField] private TextMeshProUGUI[] scoreUserNames, scoreValues;

    [Header("Update score reference")]
    [SerializeField] TMP_InputField scoreInputField;
    
    [Header("Other references")]
    [SerializeField] TextMeshProUGUI usernameField;
    
    [SerializeField] private GameObject leaderboard, authObj;
    
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
        UserJson authData = new UserJson();
        authData.username = userNameInputField.text;
        authData.password = passwordInputField.text;

        string json = JsonUtility.ToJson(authData);

        StartCoroutine(SendRegister(json));
    }
    
    public void Login()
    {
        UserJson authData = new UserJson();
        authData.username = userNameInputField.text;
        authData.password = passwordInputField.text;

        string json = JsonUtility.ToJson(authData);

        StartCoroutine(SendLogin(json));
    }

    public void UpdateUserScore()
    {
        UserJson user = new UserJson();
        user.username = username;

        if (int.TryParse(scoreInputField.text, out _))
        {
            user.data.score = int.Parse(scoreInputField.text);
        }

        string postData = JsonUtility.ToJson(user);
        Debug.Log(postData);
        StartCoroutine(UpdateScore(postData));
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
                AuthJson data = JsonUtility.FromJson<AuthJson>(request.downloadHandler.text);
                Debug.Log("Sesion activa con el usuario " + data.usuario.username);
                Debug.Log("Su score es " + data.data.score);
                prende.SetActive(true);
                authObj.SetActive(false);
                leaderboard.SetActive(true);
                usernameField.text = data.usuario.username;
                scoreInputField.text = data.usuario.data.score.ToString();
                
                StartCoroutine(RetrieveAndSetScores());
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
                UserJson data = JsonUtility.FromJson<UserJson>(request.downloadHandler.text);
                Debug.Log("Usuario registrado con el ID " + data._id);
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
                AuthJson data = JsonUtility.FromJson<AuthJson>(request.downloadHandler.text);
                Debug.Log("Inicio de sesion con el usuario " + data.usuario.username + " y su token " + data.token);
                
                PlayerPrefs.SetString("Token", data.token);
                PlayerPrefs.SetString("Username", data.usuario.username);
                prende.SetActive(false);
                authObj.SetActive(false);
                leaderboard.SetActive(true);
                
                
                usernameField.text = data.usuario.username;
                StartCoroutine(RetrieveAndSetScores());
            }
            else
            {
                Debug.Log(request.error);
            }
        }
    }
    
    IEnumerator RetrieveAndSetScores()
    {
        leaderboard.SetActive(true);
        UnityWebRequest www = UnityWebRequest.Get($"{apiUrl}usuarios");
        www.SetRequestHeader("x-token", token);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {
                Userlist jsonList = JsonUtility.FromJson<Userlist>(www.downloadHandler.text);
                Debug.Log(jsonList.usuarios.Count);

                foreach (UserJson a in jsonList.usuarios)
                {
                    Debug.Log(a.username);
                }

                List<UserJson> list = jsonList.usuarios;
                List<UserJson> sortedList = list.OrderByDescending(u => u.data.score).ToList<UserJson>();

                int len = scoreUserNames.Length;
                for (int i = 0; i < len; i++)
                {
                    scoreUserNames[i].text = sortedList[i].username;
                    scoreValues[i].text = sortedList[i].data.score.ToString();
                }
            }
            else
            {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }
        }
    }
    
    IEnumerator UpdateScore(string postData)
    {
        UnityWebRequest www = UnityWebRequest.Put($"{apiUrl}usuarios", postData);

        www.method = "PATCH";
        www.SetRequestHeader("x-token", token);
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();
        if (www.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJson jsonData = JsonUtility.FromJson<AuthJson>(www.downloadHandler.text);
                StartCoroutine(RetrieveAndSetScores());
                Debug.Log(jsonData.usuario.username + " se actualizo " + jsonData.usuario.data.score);
            }
            else
            {
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }

        }
    }
}



[System.Serializable]
public class UserJson
{
    public string _id;
    public string username;
    public string password;

    public UserData data;

    public UserJson()
    {
        data = new UserData();
    }
    public UserJson(string username, string password)
    {
        this.username = username;
        this.password = password;
        data = new UserData();
    }
}

[System.Serializable]
public class UserData
{
    public int score;
}

[System.Serializable]
public class AuthJson
{
    public UserJson usuario;
    public UserData data;
    public string token;
}

[System.Serializable]
public class Userlist
{
    public List<UserJson> usuarios;
}
