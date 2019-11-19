using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class GameUtility
{
    private static GameUtility _instance;
    public static GameUtility Instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameUtility();
            return _instance;
        }
    }

    private const string GAMERECORD_JSON_FILE_PATH = "/Data/GameRecord.json";
    private static Random _random = new Random();

    public static string GenerateOrderId()
    {
        string strDateTimeNumber = DateTime.Now.ToString("yyyyMMddHHmmssms");
        string strRandomResult = _random.Next(1, 1000).ToString().PadLeft(4, '0');
        return strDateTimeNumber + strRandomResult;
    }

    public void Save(GameRecords records)
    {
        if(records == null)
            records = new GameRecords();

        AssetModel.Instance.SaveObjecToJsonFile(records, GAMERECORD_JSON_FILE_PATH);
    }

    public GameRecords Load()
    {
        var record = AssetModel.Instance.LoadJsonFileToObject<GameRecords>(GAMERECORD_JSON_FILE_PATH);

        if (record == null)
            record = new GameRecords();
        else
            record.Init();

        return record;
    }
}
