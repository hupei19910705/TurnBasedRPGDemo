using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Data;
using Excel;
using System.Reflection;
using System;
using System.Linq;

public class AssetModel
{
    private static AssetModel _instance;
    public static AssetModel Instance
    {
        get
        {
            if (_instance == null)
                _instance = new AssetModel();
            return _instance;
        }
    }

    private string _rootPath = Application.dataPath;
    private string _gameDataPath = Application.dataPath + "/Data/GameData.xlsx";

    public T LoadJsonFileToObject<T>(string path)
    {
        var filePath = _rootPath + path;

        StreamReader sr = new StreamReader(filePath);
        string str = sr.ReadToEnd();
        var data = JsonConvert.DeserializeObject<T>(str);
        sr.Close();
        return data;
    }

    public void SaveObjecToJsonFile<T>(T obj,string path)
    {
        var targetPath = _rootPath + path;

        var js = JsonConvert.SerializeObject(obj);
        StreamWriter sw = new StreamWriter(targetPath);
        sw.Write(js);
        sw.Flush();
        sw.Close();
    }

    public GameData LoadGameDataExcelFile()
    {
        FileStream fs = new FileStream(_gameDataPath, FileMode.Open, FileAccess.Read);

        IExcelDataReader reader = ExcelReaderFactory.CreateOpenXmlReader(fs);
        DataSet dataSet = reader.AsDataSet();
        fs.Close();

        var tables = dataSet.Tables;
        var sheetInfos = _LoadDictElement<string, SheetInfo>(tables[0]);

        var heroes = _LoadDictElement<string,HeroDataRow>(tables[sheetInfos["Heroes"].SheetIndex]);
        var heroJobs = _LoadDictElement<HeroJobType, HeroJob>(tables[sheetInfos["HeroJobs"].SheetIndex]);
        var enemies = _LoadDictElement<string, EnemyDataRow>(tables[sheetInfos["Enemies"].SheetIndex]);
        var items = _LoadDictElement<string, ItemRow>(tables[sheetInfos["Items"].SheetIndex]);
        var skills = _LoadDictElement<string, SkillRow>(tables[sheetInfos["Skills"].SheetIndex]);

        GameData gameData = new GameData(heroes, heroJobs,enemies, items,skills);

        return gameData;
    }

    private Dictionary<Tkey, TValue> _LoadDictElement<Tkey, TValue>(DataTable dataTable)
    {
        Dictionary<Tkey, TValue> dict = new Dictionary<Tkey, TValue>();
        var collect = dataTable.Rows;
        var rowCount = dataTable.Rows.Count;
        var columnCount = dataTable.Columns.Count;
        Type type = typeof(TValue);


        string[] fieldsName = new string[columnCount];
        for (int i = 0; i < columnCount; i++)
        {
            var name = collect[0][i].ToString();
            if (!string.IsNullOrEmpty(name))
                fieldsName[i] = name;
        }

        for (int i = 1; i < rowCount; i++)
        {
            var objIns = type.Assembly.CreateInstance(type.ToString());
            object key = string.Empty;

            for (int j = 0; j < columnCount; j++)
            {
                var fieldName = fieldsName[j];

                if (string.IsNullOrEmpty(fieldName))
                    continue;

                var field = type.GetField(fieldName);
                if (field == null)
                    continue;

                object value = string.Empty;

                try
                {
                    if (field.FieldType.IsEnum)
                        value = Enum.Parse(field.FieldType, collect[i][j].ToString());
                    else
                        value = Convert.ChangeType(collect[i][j], field.FieldType);
                }
                catch (InvalidCastException)
                {
                    var result = collect[i][j];
                    value = result.ToString().Split(',').ToList();
                }

                if (j == 0 && typeof(Tkey).Equals(field.FieldType))
                    key = value;

                field.SetValue(objIns, value);
            }

            if (objIns is TValue && !string.IsNullOrEmpty(key.ToString()))
                dict.Add((Tkey)key, (TValue)objIns);
        }

        return dict;
    }
}

public class SheetInfo
{
    public int SheetIndex;
    public string SheetName;
}