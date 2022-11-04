using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;

public class TaskParameters : MonoBehaviour
{

    public int nTrialsPerCondition;
    public float fbTime;
    public bool interleaved;
    // public int n_conditions;
    // public int feedback_info;

    public static int nTrials;
    public static int nConds;
    public static float feedbackTime;

    public int nColor;
    [VectorLabels("mag", "proba", "val")]
    public Vector3 Option1;

    [VectorLabels("mag", "proba", "val")]
    public Vector3 Option2;

    [VectorLabels("mag", "proba", "val")]
    public Vector3 Option3;

    [VectorLabels("mag", "proba", "val")]
    public Vector3 Option4;

    public int[] symMap = new int[8];
    public static int[] symOptionMapping = new int[8];

    [VectorLabels("Opt1", "Opt2", "info")]
    public Vector3Int Condition1;

    [VectorLabels("Opt1", "Opt2", "info")]
    public Vector3Int Condition2;

    [VectorLabels("Opt1", "Opt2", "info")]
    public Vector3Int Condition3;

    [VectorLabels("Opt1", "Opt2", "info")]
    public Vector3Int Condition4;

    [VectorLabels("Opt1", "Opt2", "info")]
    public Vector3Int ConditionTransfer1;

    [VectorLabels("Opt1", "Opt2", "info")]
    public Vector3Int ConditionTransfer2;

    [VectorLabels("Opt1", "Opt2", "info")]
    public Vector3Int ConditionTransfer3;

    [VectorLabels("Opt1", "Opt2", "info")]
    public Vector3Int ConditionTransfer4;


    public static List<Vector2> colors = new List<Vector2>();

    public static List<Vector3> options = new List<Vector3>();
    public static List<Vector3> conditions = new List<Vector3>();
    public static List<Vector3> conditionsTransfer = new List<Vector3>();
    public static List<int> conditionIdx;
    public static List<int> conditionTransferIdx;


    public static List<List<int>> rewards = new List<List<int>>();
    public static List<List<int>> rewardsTransfer = new List<List<int>>();

    private List<int> availableOptions = new List<int>();
    public static List<Vector2> symbols = new List<Vector2>();
    public static List<Vector2> symbolsTransfer = new List<Vector2>();


    void Start()
    {

        for (int i = 0; i < 8; i++)
        {
            availableOptions.Add(i);

        }
        Shuffle2(availableOptions);

        nColor = availableOptions.Count;

        symOptionMapping = symMap;

        options.Add(Option1);
        options.Add(Option2);
        options.Add(Option3);
        options.Add(Option4);

        conditions.Add(Condition1);
        conditions.Add(Condition2);
        conditions.Add(Condition3);
        conditions.Add(Condition4);

        conditionsTransfer.Add(ConditionTransfer1);
        conditionsTransfer.Add(ConditionTransfer2);
        conditionsTransfer.Add(ConditionTransfer3);
        conditionsTransfer.Add(ConditionTransfer4);

        nConds = conditions.Count;
        nTrials = nTrialsPerCondition*conditions.Count;
        feedbackTime = fbTime;

        MakeSymbols();
        MakeRewards();
        MakeConditionsIdx();

    }

    private void MakeSymbols()

    {
        for (int c = 0; c < conditions.Count; c++)
        {

            symbols.Add(new Vector2(
                availableOptions[(int)conditions[c][0]],
                availableOptions[(int)conditions[c][1]]));

            symbolsTransfer.Add(new Vector2(
                availableOptions[(int)conditionsTransfer[c][0]],
                availableOptions[(int)conditionsTransfer[c][1]]));
        }

    }


    private void MakeRewards()
    {
        for (int c = 0; c < conditions.Count; c++)
        {
            // Learning
            // make rewards for option 1 & 2
            rewards.Add(MakeRewardsForOneOption(options[(int)
                symOptionMapping[(int)conditions[c][0]] - 1]
            ));
            rewards.Add(MakeRewardsForOneOption(options[(int)
                symOptionMapping[(int)conditions[c][1]] - 1]
            ));

            // Transfer
            // make rewards for option 1 & 2
            rewardsTransfer.Add(MakeRewardsForOneOption(options[(int)
                    symOptionMapping[(int)conditionsTransfer[c][0]] - 1]
                ));
            rewardsTransfer.Add(MakeRewardsForOneOption(options[(int)
                symOptionMapping[(int)conditionsTransfer[c][1]] - 1]
            ));
        }

    }

    public static Vector3 GetOption(int cond, int n)
    {
        return options[
            (int)symOptionMapping[
                (int)conditions[cond][n - 1]
            ] - 1
       ];
    }

    public static Vector3 GetOptionTransfer(int cond, int n)
    {
        return options[
            (int)symOptionMapping[
                (int)conditionsTransfer[cond][n - 1]
            ] - 1
       ];
    }


    private void MakeConditionsIdx()
    {


        List<List<int>> conditionIdxTemp = new List<List<int>>();
        List<List<int>> conditionTransferIdxTemp = new List<List<int>>();

        for (int c = 0; c < conditions.Count; c++)
        {
            List<int> x = Enumerable.Repeat(c, nTrialsPerCondition).ToList();
            conditionIdxTemp.Add(x);
            conditionTransferIdxTemp.Add(x);
        }

        Shuffle2(conditionIdxTemp);
        Shuffle2(conditionTransferIdxTemp);

        conditionIdx = conditionIdxTemp.SelectMany(i => i).ToList<int>();
        conditionTransferIdx = conditionTransferIdxTemp.SelectMany(i => i).ToList<int>();

        if (interleaved)
        {
            Shuffle2(conditionIdx);
            Shuffle2(conditionTransferIdx);
        }


    }

    private List<int> MakeRewardsForOneOption(Vector3 option)

    {

        List<int> rewardsTemp = new List<int>();

        for (int i = 0; i < nTrialsPerCondition; i++)
        {
            if (i < Math.Round(nTrialsPerCondition * option.y))
            {
                rewardsTemp.Add(1 * (int)option.x * (int)option.z);
            }
            else
            {
                rewardsTemp.Add(0);
            }

        }


        Shuffle2(rewardsTemp);

        return rewardsTemp;

    }

    private void Shuffle2<T>(IList<T> list)
    {
        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        int n = list.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do provider.GetBytes(box);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

}