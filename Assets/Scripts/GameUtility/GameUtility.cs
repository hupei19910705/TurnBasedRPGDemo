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

    private const string GAMERECORD_JSON_FILE_PATH = "/GameRecord.json";
    private static Random _random = new Random();
    private GameRecords _gameRecords;
    private int _curRecordId = -1;

    public static string GenerateOrderId()
    {
        string strDateTimeNumber = DateTime.Now.ToString("yyyyMMddHHmmssms");
        string strRandomResult = _random.Next(1, 1000).ToString().PadLeft(4, '0');
        return strDateTimeNumber + strRandomResult;
    }

    public void Save()
    {
        if (_gameRecords == null)
            _gameRecords = new GameRecords();

        AssetModel.Instance.SaveObjecToJsonFile(_gameRecords, GAMERECORD_JSON_FILE_PATH);
    }

    public GameRecords GetGameRecords()
    {
        var records = AssetModel.Instance.LoadJsonFileToObject<GameRecords>(GAMERECORD_JSON_FILE_PATH);

        if (records == null)
            records = new GameRecords();
        else
            records.Init();

        _gameRecords = records;
        return records;
    }

    public GameRecord SelectCurGameRecord(int recordId)
    {
        _curRecordId = recordId;

        if (recordId == -1)
            return null;

        return _gameRecords.Records[_curRecordId];
    }

    public GameRecord GetCurGameRecord()
    {
        if (_curRecordId == -1)
            return null;

        return _gameRecords.Records[_curRecordId];
    }

    public Item CaculateDropItems(DropItem dropItem)
    {
        bool isDrop = _random.Next(0, 100) < dropItem.DropRate;
        return isDrop ? dropItem.Item : null;
    }
}
