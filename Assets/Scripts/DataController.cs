using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Runtime.InteropServices;


public class DataController : MonoBehaviour
{
    private Dictionary<string, object> data;



    private GameController gameController;
    // Start is called before the first frame update
    void Start()
    {
        data = new Dictionary<string, object>();
        GameObject gameControllerObject = GameObject.FindWithTag(
            "GameController");
        if (gameControllerObject != null)
        {
            gameController = gameControllerObject
                .GetComponent<GameController>();
        }
        if (gameController == null)
        {
            Debug.Log("Cannot find 'GameController' script");
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Save(string key, object value)
    {
        data[key] = value;

    }
    public void PrintData()
    {   
        foreach (KeyValuePair<string, object> entry in data)
        {
            Debug.Log(entry.Key + " = " + entry.Value);
        }

    }


    public IEnumerator SendToDB()
    {
        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        int error = 0;
        bool success = false;

        foreach (KeyValuePair<string, object> entry in data)
        {
            formData.Add(new MultipartFormDataSection(entry.Key, entry.Value.ToString().Replace(",", ".")));

        }

        
        while ((error < 4) && !success)

        {
            UnityWebRequest www = UnityWebRequest.Post(gameController.serverURL, formData);
            www.SetRequestHeader("Access-Control-Allow-Credentials", "true");
            www.SetRequestHeader("Access-Control-Allow-Headers", "Accept, Content-Type, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
            www.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, PUT, OPTIONS");
            www.SetRequestHeader("Access-Control-Allow-Origin", "*");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                error++;
                if (error == 4)
                    gameController.DisplayNetworkError();
            }
           
            string response = www.downloadHandler.text;
            Debug.Log("Server response: " + response);

            if (response.ToLower().Contains("error"))
            {
                error++;
                if (error == 4)
                    gameController.DisplayServerError();
            } 
            else if (response.ToLower().Contains("success"))
            {
                success = true;
            }
            
            yield return new WaitForSeconds(.1f);
            
        } 
        
    }
}
