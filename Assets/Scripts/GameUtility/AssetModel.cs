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

    public T LoadJsonFileToObject<T>(string path)
    {
        var filePath = _rootPath + path;
        if (!File.Exists(filePath))
            return default;

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

    public GameData LoadGameDataExcelFile(string path)
    {
        var filePath = _rootPath + path;

        FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

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
        var levelExp = _LoadDictElement<int, int>(tables[sheetInfos["LevelExp"].SheetIndex]);
        var constData = _LoadSingleObject<ConstantData>(tables[sheetInfos["ConstantData"].SheetIndex]);

        GameData gameData = new GameData(heroes, heroJobs, enemies, items, skills, levelExp, constData);

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
            object value = string.Empty;

            for (int j = 0; j < columnCount; j++)
            {
                var fieldName = fieldsName[j];

                if (string.IsNullOrEmpty(fieldName))
                    continue;

                if (type.IsValueType)
                {
                    bool keyIsEmpty = string.IsNullOrEmpty(key.ToString());
                    bool valueIsEmpty = string.IsNullOrEmpty(value.ToString());

                    if (!keyIsEmpty && !valueIsEmpty)
                        break;

                    var data = Convert.ChangeType(collect[i][j], type);
                    if (keyIsEmpty && typeof(Tkey).Equals(type))
                        key = data;
                    else if (valueIsEmpty)
                    {
                        value = data;
                        objIns = data;
                    }
                }
                else
                {
                    var field = type.GetField(fieldName);
                    if (field == null)
                        continue;

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
            }

            if (objIns is TValue && !string.IsNullOrEmpty(key.ToString()))
                dict.Add((Tkey)key, (TValue)objIns);
        }

        return dict;
    }

    private T _LoadSingleObject<T>(DataTable dataTable)
    {
        var collect = dataTable.Rows;
        var rowCount = dataTable.Rows.Count;
        var columnCount = dataTable.Columns.Count;
        Type type = typeof(T);

        if (columnCount < 2)
            return default;

        var objIns = type.Assembly.CreateInstance(type.ToString());
        for(int i =1;i< rowCount; i++)
        {
            var fieldName = collect[i][0].ToString();
            var field = type.GetField(fieldName);
            if (field == null)
                continue;

            var value = Convert.ChangeType(collect[i][1], field.FieldType);
            field.SetValue(objIns, value);
        }

        return (T)objIns;
    }
}

public class SheetInfo
{
    public int SheetIndex;
    public string SheetName;
}