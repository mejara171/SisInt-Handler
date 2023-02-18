using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Linq;
public class HttpHandler : MonoBehaviour
{
    [SerializeField] private GameObject login,index;
    [SerializeField]
    private string ServerApiURL;
    public string Token { get; set; }
    public string Username { get; set; }
    [SerializeField]
    private string token ;
    public TMP_Text[] Puestos ;
    public TMP_Text Nombre;
    //eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1aWQiOiI2YThlYTg3N2I0YTkwZDQ2IiwiaWF0IjoxNjc1OTg2MjgyLCJleHAiOjE2NzYwMDA2ODJ9.IQL4pTBsGki6yglfZt-9U0tmL6L-RBtfxsfWUUKH3x0
    // Start is called before the first frame update
    void Start()
    {
    
        Token = PlayerPrefs.GetString("token");
        Username = PlayerPrefs.GetString("username");

        List<User> lista = new List<User>();
        List<User> listaOrdenada = lista.OrderByDescending(u => u.data.score).ToList<User>();
        if (string.IsNullOrEmpty(Token))
        {
            index.SetActive(false);
            login.SetActive(true);
            Debug.Log("No hay token");
            //Ir a Login
        }
        else
        {
            login.SetActive(false);
            index.SetActive(true);
            token = Token;
            Debug.Log(Token);
            Debug.Log(Username);
            StartCoroutine(GetPerfil());
        }
    }

    public void Registrar()
    {
        User user = new User();
        user.username = GameObject.Find("InputUsername").GetComponent<InputField>().text;
        user.password = GameObject.Find("InputPassword").GetComponent<InputField>().text;
        string postData = JsonUtility.ToJson(user);
        StartCoroutine(Registro(postData));
    }

    public void Ingresar()
    {
        User user = new User();
        user.username = GameObject.Find("InputUsername").GetComponent<InputField>().text;
        user.password = GameObject.Find("InputPassword").GetComponent<InputField>().text;
        string postData = JsonUtility.ToJson(user);
        StartCoroutine(Login(postData));
    }
    public void Actualizar()
    {
        User user = new User();
        user.username = Username;
        if (int.TryParse(GameObject.Find("InputDatascore").GetComponent<InputField>().text,out _))
        {
            user.data.score = int.Parse(GameObject.Find("InputDatascore").GetComponent<InputField>().text);
        }
        string postData = JsonUtility.ToJson(user);
        Debug.Log(postData);
        StartCoroutine(updateDate(postData));
    }
    public void Out()
    {
        PlayerPrefs.DeleteAll();
        index.SetActive(false);
        login.SetActive(true);
    }
    IEnumerator Registro(string postData)
    {

        UnityWebRequest www = UnityWebRequest.Put(ServerApiURL + "/api/usuarios", postData);
        www.method = "POST";
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(jsonData.usuario.username + " se regitro con id " + jsonData.usuario._id);


                //Proceso de autenticacion
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
    IEnumerator Login(string postData)
    {

        UnityWebRequest www = UnityWebRequest.Put(ServerApiURL + "/api/auth/login", postData);
        www.method = "POST";
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(jsonData.usuario.username + " inicio sesion");

                Token = jsonData.token;
                Username = jsonData.usuario.username;

                PlayerPrefs.SetString("token", Token);
                PlayerPrefs.SetString("username", Username);
                login.SetActive(false);
                index.SetActive(true);
                StartCoroutine(MoreInfo());
                Nombre.text = "Usuario :" + jsonData.usuario.username;

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
    IEnumerator GetPerfil()
    {
        UnityWebRequest www = UnityWebRequest.Get(ServerApiURL + "/api/usuarios/" + Username);
        www.SetRequestHeader("x-token", Token);



        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                Debug.Log(jsonData.usuario.username + " Sigue con la sesion inciada");
                Nombre.text = "Usuario :" + jsonData.usuario.username;

                //hola 
                StartCoroutine(MoreInfo());

            }
            else
            {
                index.SetActive(false);
                login.SetActive(true);
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }

        }
    }
    IEnumerator MoreInfo()
    {
        UnityWebRequest www = UnityWebRequest.Get(ServerApiURL + "/api/usuarios"+"?limit=5");
        www.SetRequestHeader("x-token", Token);
        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                //hola 
                userlist jsonList = JsonUtility.FromJson<userlist>(www.downloadHandler.text);
                Debug.Log(jsonList.usuarios.Count);
                foreach (User a in jsonList.usuarios)
                {
                    Debug.Log(a.username);
                }
                List<User> lista = jsonList.usuarios;
                List<User> listaOrdenada = lista.OrderByDescending(u => u.data.score).ToList<User>();
                int puesto=0;
                foreach (User a in listaOrdenada)
                {
                    string nombre =puesto+1+"."+"Usuario:"+a.username+",Puntaje:"+a.data.score;
                    Puestos[puesto].text=nombre;
                    puesto++;
                }
            }
            else
            {
                index.SetActive(false);
                login.SetActive(true);
                string mensaje = "Status :" + www.responseCode;
                mensaje += "\ncontent-type:" + www.GetResponseHeader("content-type");
                mensaje += "\nError :" + www.error;
                Debug.Log(mensaje);
            }

        }
    }
    IEnumerator updateDate(string postData)
    {
        
        UnityWebRequest www = UnityWebRequest.Put(ServerApiURL + "/api/usuarios/", postData);
        www.method = "PATCH";
        www.SetRequestHeader("x-token", Token);
        www.SetRequestHeader("Content-Type", "application/json");
        yield return www.SendWebRequest();
        if (www.isNetworkError)
        {
            index.SetActive(false);
            login.SetActive(true);
            Debug.Log("NETWORK ERROR :" + www.error);
        }
        else
        {
            Debug.Log(www.downloadHandler.text);

            if (www.responseCode == 200)
            {

                AuthJsonData jsonData = JsonUtility.FromJson<AuthJsonData>(www.downloadHandler.text);

                StartCoroutine(MoreInfo());
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
public class User
{
    public string _id;
    public string username;
    public string password;
   
    public userData data;

    public User()
    {
        data = new userData();
    }
    public User(string username, string password)
    {
        this.username = username;
        this.password = password;
        data = new userData();
    }
}
[System.Serializable]
public class userData
{
    public int score;
    public userData()
    {
        
    }

}

public class AuthJsonData
{
    public User usuario;
    public userData data;
    public string token;
}

[System.Serializable]
public class userlist
{
    public List<User> usuarios;
}
