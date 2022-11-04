using System.Collections;
using System.Collections.Generic;
using Input = UnityEngine.Input;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using UnityEngine.Windows;
using UnityEngine;




public class GameController : MonoBehaviour
{
    // public members
    public string serverURL;
    public GameObject hazard1;
    public GameObject hazard2;
    public Vector3 spawnValues;
    public float startWait;
    public float waveWait;

    public Text scoreText;

    public Text restartText;
    public Text gameOverText;
    public Text missedTrialText;
    public Button playButton;

    public Text rewardText;
    public Text counterText;


    public bool waveAllowed;

    public GameObject option1;
    public GameObject option2;

    public Object[] optionpath;

  
    public int outcomeOpt1;
    public int outcomeOpt2;

    public Animation anim1;
    public Animation anim2;

    public bool transfer;

    public int feedbackInfo;

    public string subID;
    public int score;

    public int missedTrial = 0;

    public bool sendData = false;

  

    // private members

    private bool gameOver;
    private bool restart;

    private Texture symbol1;
    private Texture symbol2;


    private bool networkError;

    private StateMachine stateMachine;

    private DataController dataController;
    private PlayerController playerController;
    private OptionController optionController;
    private PauseController pauseController;

    private bool isQuitting = false;


    // JS interactions
    [DllImport("__Internal")]
    private static extern void SetScore(int score);

    [DllImport("__Internal")]
    private static extern string GetSubID();

    [DllImport("__Internal")]
    private static extern void Alert(string text);

    //[DllImport("__Internal")]
    //private static extern void DisplayNextButton();


    // Getters / Setters
    // -------------------------------------------------------------------- //
    // Option controller is regenerated each trial, 
    // so we call this setter from the new generated object 
    // each time
    public void SetOptionController(OptionController obj)
    {
        optionController = obj;
    }

    public OptionController GetOptionController()
    {
        return optionController;
    }

    public void SetOutcomes(int v1, int v2)
    {
        outcomeOpt1 = v1;
        outcomeOpt2 = v2;
    }

    public PlayerController GetPlayerController()
    {
        return playerController;
    }


    public bool IsGameOver()
    {
        return gameOver;
    }

    public void SetGameOver()
    {
        gameOver = true;
    }

    public void AllowWave(bool value)
    {
        waveAllowed = value;
    }

    public void AllowSendData(bool value)
    {
        sendData = value;
    }

    public void Save(string key, object value)
    {
        dataController.Save(key, value);
    }

    public IEnumerator SendToDB()
    {
        //PrintData();
        yield return dataController.SendToDB();
        AfterSendToDB();

        Debug.Log("******************************************************");

    }

    public void PrintData()
    {
        dataController.PrintData();
    }

    public void AfterSendToDB()
    {
        playerController.ResetCount();
        missedTrial = 0;
        AllowWave(true);

    }


    void Start()
    {
        // optionpath = Resources.LoadAll("colors/"); 
        optionpath = Resources.LoadAll("colors/");

        score = 0;
        gameOver = false;
        restart = false;
        transfer = false;
        waveAllowed = true;
        restartText.text = "";
        gameOverText.text = "";
        rewardText.text = "";
        counterText.text = "";
        missedTrialText.text = "";

        gameOverText.gameObject.SetActive(false);
        rewardText.gameObject.SetActive(false);
        scoreText.gameObject.SetActive(false);
        counterText.gameObject.SetActive(false);
        missedTrialText.gameObject.SetActive(false);

        UpdateScore();

        try
        {
            subID = GetSubID();
        }
        catch
        {
            subID = "Unity";
        }
        //StartCoroutine(SpawnWaves()); 
        dataController = GameObject.FindWithTag("DataController").GetComponent<DataController>();
        playerController = GameObject.FindWithTag("Player").GetComponent<PlayerController>();
        pauseController = GameObject.FindWithTag("PauseController").GetComponent<PauseController>();

    }

    public void Run()
    {
        
        stateMachine = new StateMachine(this);
        stateMachine.NextState();
        stateMachine.Update();

        playButton.gameObject.SetActive(false);
        gameOverText.gameObject.SetActive(true);
        rewardText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);
        counterText.gameObject.SetActive(true);
        missedTrialText.gameObject.SetActive(true);
    }

    
    void Update()
    { 
        if (isQuitting)
        {
            return;
        }
    
        if (IsGameOver())
        {
            StartCoroutine(DisplayGameOver());
            SetScore(score);
            StartCoroutine(QuitGame());
        }

        if (restart)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }

        if (stateMachine != null && stateMachine.CurrentStateIsDone())
        {
            stateMachine.currentState.Exit();
            stateMachine.NextState();
            stateMachine.Update();
        }

        
    }

    IEnumerator QuitGame()
    {
        isQuitting = true;
        yield return new WaitForSeconds(6.5f);
        Application.Quit();
    }

    // Graphical manager (to put in its own controller later)
    // ------------------------------------------------------------------------------------------------ //

    public void AddScore(int newScoreValue)
    {
        score += newScoreValue;
        Save("score", (int)score);
        UpdateScore();
    }

    public void PrintFeedback(int newScoreValue, int counterScoreValue, Vector3 ScorePosition)
    {

        if (feedbackInfo == 0)
        {
            return;
        }

        rewardText.transform.position = ScorePosition;
        rewardText.text = "" + newScoreValue;

        if (feedbackInfo == 2)
        {
            counterText.transform.position = new Vector3(
                         -ScorePosition.x, ScorePosition.y, ScorePosition.z);
            counterText.text = "" + counterScoreValue;
        }

        StartCoroutine("DeleteFeedback", TaskParameters.feedbackTime);

    }

    IEnumerator DeleteFeedback(float feedbacktime) {
        yield return new WaitForSeconds(feedbacktime);
        rewardText.text = "";
        counterText.text = "";
        missedTrialText.text = "";
        // Destroy(option1);
        // Destroy(option2); 
        yield return null;
    }

    IEnumerator DestroyWithDelay(GameObject toDestroy, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(toDestroy);
    }

    void UpdateScore()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    public void DisplayNetworkError()
    {
        string msg = "Network error!\n" +
            "Check your \ninternet \nconnection and click to continue...";
        pauseController.PauseGame(msg);
       
    }
    public void DisplayServerError()
    {
        string msg = "Server error\n" +
            "Click to continue...";
        pauseController.PauseGame(msg);

    }

    IEnumerator DisplayGameOver()
    {
        yield return new WaitForSeconds(5);
        gameOverText.text = "End!";
    }


    public void MissedTrial()
    {
        missedTrial = 1;
        missedTrialText.text = "Missed trial!\n -2";
        AddScore(-1);
        AllowSendData(true);
        StartCoroutine("DeleteFeedback", TaskParameters.feedbackTime);
    }

 
    public void FadeAndDestroyOption(GameObject option, float delay)
    {

        option.GetComponent<Animation>().Play();
        //option.GetComponent<Collider>().enabled = false;

        //StartCoroutine(SendToDB());
        StartCoroutine(DestroyWithDelay(option, delay));
    }


    public void ChangeBackground()
    {
        GameObject background = GameObject.FindWithTag("Background");
        background.GetComponent<MeshRenderer>().material.mainTexture =
            (Texture)Resources.Load("backgrounds/red");
        GameObject child = background.transform.GetChild(0).gameObject;
        child.GetComponent<MeshRenderer>().material.mainTexture =
        (Texture)Resources.Load("backgrounds/red");
    }

    public void SetSymbolsTexture(Vector2 id)
    {
        symbol1 = (Texture)optionpath[(int)id[0]];
        symbol2 = (Texture)optionpath[(int)id[1]];//CopyTexture((Texture2D)optionpath[0]);

        option1.GetComponent<MeshRenderer>().material.mainTexture = symbol1;

        int[] p = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        int pp = p[Random.Range(0, p.Length)];
        int numOfChildren = option2.transform.childCount;

        for (int i = 0; i < numOfChildren; i++)
        {
            GameObject child = option2.transform.GetChild(i).gameObject;

            if (i < pp)
            {
                child.GetComponent<Renderer>().materials[0].color = Color.red;
                child.GetComponent<Renderer>().materials[1].color = Color.red;
                child.GetComponent<Renderer>().materials[2].color = Color.red;


            }
            else
            {

                child.GetComponent<Renderer>().materials[0].color = Color.green;
                child.GetComponent<Renderer>().materials[1].color = Color.green;
                child.GetComponent<Renderer>().materials[2].color = Color.green;

            }
        }

        //option2.GetComponent<MeshRenderer>().material.mainTexture = symbol2;

    }



   


    public void SpawnOptions()
    {
        Quaternion spawnRotation = Quaternion.identity;
       
        float leftright;

        if (Random.value < 0.5f)
        {
            leftright = spawnValues.x;
        }
        else
        {
            leftright = -spawnValues.x;
        }

        Vector3 spawnPosition1 = new Vector3(leftright, spawnValues.y, spawnValues.z);
        option1 = Instantiate(hazard1, spawnPosition1, spawnRotation);
        option1.tag = "Opt1";

        Vector3 spawnPosition2 = new Vector3(-leftright, spawnValues.y, spawnValues.z);
        option2 = Instantiate(hazard2, spawnPosition2, spawnRotation);
        //option2.transform.Rotate(0f, 110f, 0.0f, Space.World);
        option2.tag = "Opt2";

    }
    

}
// ------------------------------------------------------------------------------------------------//
// State Machine (to put in its own controller later)
// ------------------------------------------------------------------------------------------------ //

public interface IState
{
    void Enter();
    bool IsDone();
    IEnumerator Execute();
    void Exit();
}


public class StateMachine
{
    public IState currentState;
    private GameController owner;
    public List<IState> states;

    int stateNumber;


    public StateMachine(GameController owner) 
    { 
        this.owner = owner;
        states = new List<IState>();
        states.Add(new LearningTest());
        states.Add(new TransferTest());

        stateNumber = -1;
    }


    public void ChangeState(IState newState)
    {
        if (currentState != null)
            currentState.Exit();

        currentState = newState;

        currentState.Enter();
    }

    public void Update()
    {
        if (currentState != null && !CurrentStateIsDone())
        {
            owner.StartCoroutine(currentState.Execute());
        }
    }

    public void NextState()
    {
        stateNumber += 1;

        if (stateNumber < states.Count)
        {
            ChangeState(states[stateNumber]);

        }
        else
        {
            Debug.Log("Last state reached");
        }
    }

    public bool CurrentStateIsDone()
    {
        return currentState.IsDone();
    }
}


public class LearningTest : IState
{
    GameController gameController;
    public bool isDone;

    public void Enter()
    {
        gameController = GameObject.FindWithTag("GameController").
            GetComponent<GameController>();
        Debug.Log("entering learning test");

    }

    public bool IsDone()
    {
        return isDone;
    }

    public IEnumerator Execute()
    {

        int[] condTrial = new int[TaskParameters.nConds];

        for (int t = 0; t < TaskParameters.nTrials; t++)
        {

            while (!gameController.waveAllowed)
            {
                yield return new WaitForSeconds(.5f);
            }

            yield return new WaitForSeconds(gameController.waveWait);

            int cond = (int) TaskParameters.conditionIdx[t];

            gameController.feedbackInfo = (int) TaskParameters.conditions[cond][2];

            gameController.SpawnOptions();
            gameController.SetSymbolsTexture(TaskParameters.symbols[cond]);
      

            gameController.SetOutcomes(
                TaskParameters.rewards[cond * 2][condTrial[cond]], 
                TaskParameters.rewards[cond * 2 + 1][condTrial[cond]]);

            condTrial[cond]++;


            gameController.AllowWave(false);
            gameController.AllowSendData(false);

            while (!gameController.sendData)
            {
                yield return new WaitForSeconds(.5f);

            }
            // once the option is shot we can get the option controller and gather the data 
            OptionController optionController = gameController.GetOptionController();
            PlayerController playerController = gameController.GetPlayerController();

            gameController.Save("con", (int)cond + 1);
            gameController.Save("t", t);
            gameController.Save("session", 1);

            gameController.Save("choice", (int) optionController.choice);
            gameController.Save("outcome", (int) optionController.scoreValue);
            gameController.Save("cfoutcome", (int) optionController.counterscoreValue);
            gameController.Save("rt", (int)optionController.st.ElapsedMilliseconds);
            gameController.Save("choseLeft", (int)optionController.choseLeft);
            gameController.Save("corr", (int)optionController.corr);

            gameController.Save("fireCount", (int)playerController.fireCount);
            gameController.Save("upCount", (int)playerController.upCount);
            gameController.Save("downCount", (int)playerController.downCount);
            gameController.Save("leftCount", (int)playerController.leftCount);
            gameController.Save("rightCount", (int)playerController.rightCount);

            gameController.Save("prolificID", gameController.subID);
            gameController.Save("feedbackInfo", (int)gameController.feedbackInfo);
            gameController.Save("missedTrial", (int)gameController.missedTrial);
            gameController.Save("score", (int) gameController.score);
            gameController.Save("optFile1",
                 (string)TaskParameters.symbols[cond][0].ToString() + ".tiff");
            gameController.Save("optFile2",
                 (string)TaskParameters.symbols[cond][1].ToString() + ".tiff");
            //gameController.Save("optFile2", (string)gameController.symbol2.ToString());


            // retrieve probabilities
            float p1 = TaskParameters.GetOption(cond, 1)[1];
            float p2 = TaskParameters.GetOption(cond, 2)[1];

            gameController.Save("p1", (float)p1);
            gameController.Save("p2", (float)p2);

            yield return gameController.SendToDB();

        }
    
        isDone = true;
    }


    public void Exit()
    {
        Debug.Log("Exiting learning test");

    }
}



public class TransferTest : IState
{
    GameController gameController;
    public bool isDone;

    public void Enter()
    {
        gameController = GameObject.FindWithTag("GameController").
            GetComponent<GameController>();
        Debug.Log("entering transfer test");
    
    }

    public bool IsDone()
    {
        return isDone;
    }

    public IEnumerator Execute()
    {
        yield return new WaitForSeconds(1.5f);
        //gameController.ChangeBackground();
        //yield return new WaitForSeconds(1.5f);
        int[] condTrial = new int[TaskParameters.nConds];


        for (int t = 0; t < TaskParameters.nTrials; t++)
        {
            while (!gameController.waveAllowed)
            {
                yield return new WaitForSeconds(.5f);
            }


            yield return new WaitForSeconds(gameController.waveWait);

            int cond = (int)TaskParameters.conditionTransferIdx[t];

            gameController.feedbackInfo = (int)TaskParameters.conditionsTransfer[cond][2];

            gameController.SpawnOptions();
            gameController.SetSymbolsTexture(TaskParameters.symbolsTransfer[cond]);

            gameController.SetOutcomes(
                TaskParameters.rewardsTransfer[cond * 2][condTrial[cond]],
                TaskParameters.rewardsTransfer[cond * 2 + 1][condTrial[cond]]);
            condTrial[cond]++;

            gameController.AllowWave(false);
            gameController.AllowSendData(false);

            while (!gameController.sendData)
            {
                yield return new WaitForSeconds(.5f);

            }

            // once the option is shot we can get the option controller and gather the data 
            OptionController optionController = gameController.GetOptionController();
            PlayerController playerController = gameController.GetPlayerController();

            gameController.Save("con", (int)cond + 1);
            gameController.Save("t", t);
            gameController.Save("session", 2);

            gameController.Save("choice", (int)optionController.choice);
            gameController.Save("outcome", (int)optionController.scoreValue);
            gameController.Save("cfoutcome", (int)optionController.counterscoreValue);
            gameController.Save("rt", (int)optionController.st.ElapsedMilliseconds);
            gameController.Save("choseLeft", (int)optionController.choseLeft);
            gameController.Save("corr", (int)optionController.corr);


            gameController.Save("fireCount", (int)playerController.fireCount);
            gameController.Save("upCount", (int)playerController.upCount);
            gameController.Save("downCount", (int)playerController.downCount);
            gameController.Save("leftCount", (int)playerController.leftCount);
            gameController.Save("rightCount", (int)playerController.rightCount);

            gameController.Save("prolificID", gameController.subID);
            gameController.Save("feedbackInfo", (int)gameController.feedbackInfo);
            gameController.Save("missedTrial", (int)gameController.missedTrial);
            gameController.Save("score", (int)gameController.score);
            gameController.Save("optFile1",
                 (string)TaskParameters.symbolsTransfer[cond][0].ToString() + ".tiff");
            gameController.Save("optFile2",
                 (string)TaskParameters.symbolsTransfer[cond][1].ToString() + ".tiff");


            // retrieve probabilities
            float p1 = TaskParameters.GetOptionTransfer(cond, 1)[1];
            float p2 = TaskParameters.GetOptionTransfer(cond, 2)[1];

            gameController.Save("p1", (float)p1);
            gameController.Save("p2", (float)p2);

            yield return gameController.SendToDB();
        }
        isDone = true;
    }


    public void Exit()
    {
        Debug.Log("Exiting transfer test");
        gameController.SetGameOver();

    }


}


////CopiedTexture is the original Texture  which you want to copy.
//public Texture CopyTexture(Texture2D copiedTexture)
//{
//    //Create a new Texture2D, which will be the copy.
//    Texture2D texture = new Texture2D(copiedTexture.width, copiedTexture.height);

//    texture.SetPixels(copiedTexture.GetPixels());


//    //Choose your filtermode and wrapmode here.
//    //texture.filterMode = FilterMode.Point;
//    //texture.wrapMode = TextureWrapMode.Clamp;
//    double[] p = { .1, .2, .3, .4, .6, .7, .8 };
//    double pp = p[Random.Range(0, p.Length)];
//    int x = 0;
//    while (x < texture.width)
//    {
//        int y = 0;
//        while (y < texture.height)
//        {
//            //INSERT YOUR LOGIC HERE
//            if (copiedTexture.GetPixel(x, y).a != 0)
//            {
//                if (y > 115)
//                {
//                    if (x > (texture.width * pp))
//                    {
//                        //This line of code and if statement, turn Green pixels into Red pixels.
//                        texture.SetPixel(x, y, Color.red);
//                    }
//                    else
//                    {
//                        //This line of code is REQUIRED. Do NOT delete it. This is what copies the image as it was, without any change.
//                        texture.SetPixel(x, y, Color.green);
//                    }
//                }
//                else
//                {
//                    if (x > (texture.width * (1f - pp)))
//                    {
//                        //This line of code and if statement, turn Green pixels into Red pixels.
//                        texture.SetPixel(x, y, Color.green);
//                    }
//                    else
//                    {
//                        //This line of code is REQUIRED. Do NOT delete it. This is what copies the image as it was, without any change.
//                        texture.SetPixel(x, y, Color.red);
//                    }
//                }
//            }
//            y++;

//        }
//        x++;
//    }
//    //Name the texture, if you want.
//    //texture.name = (Species + Gender + "_SpriteSheet");

//    //This finalizes it. If you want to edit it still, do it before you finish with .Apply(). Do NOT expect to edit the image after you have applied. It did NOT work for me to edit it after this function.
//    texture.Apply();

//    //then Save To Disk as PNG
//    byte[] bytes = texture.EncodeToPNG();
//    var dirPath = Application.dataPath + "/../SaveImages/";
//    if (!Directory.Exists(dirPath))
//    {
//        Directory.CreateDirectory(dirPath);
//    }
//    File.WriteAllBytes(dirPath + "Image" + ".png", bytes);

//    //Return the variable, so you have it to assign to a permanent variable and so you can use it.
//    return texture;
//}
