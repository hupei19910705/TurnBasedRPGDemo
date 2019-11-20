using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Utility;
using Utility.GameUtility;

public class SplashScreenPanel : MonoBehaviour
{
    [SerializeField] private Button _enterGameBtn = null;
    [SerializeField] private GeneralObjectPool _recordPool = null;
    [SerializeField] private Transform _recordAnchor = null;
    [SerializeField] private Button _addRecordBtn = null;
    [SerializeField] private ToggleGroup _toggleGroup = null;
    [SerializeField] private RecordDisplayView _recordDisplayView = null;

    private GameRecords _gameRecords = null;
    private GameData _gameData = null;
    private int _maxRecordCount;
    private int _selectRecordId = -1;

    private Dictionary<int, RecordElementView> _recordViews = new Dictionary<int, RecordElementView>();

    private bool _leave = false;

    private void Start()
    {
        SceneModel.Instance.GoToStartScene();
    }

    public void Init(GameData gameData)
    {
        _gameRecords = GameUtility.Instance.GetGameRecords();
        _gameData = gameData;
        _maxRecordCount = _gameData.ConstantData.MAX_RECORD_COUNT;
        _recordDisplayView.Init(gameData);
    }

    public IEnumerator Run()
    {
        _Register();
        _recordPool.InitPool();
        _CreateRecords();

        while (!_leave)
            yield return null;
    }

    private void _Register()
    {
        _UnRegister();

        _recordDisplayView.OnChangeRecordNameAction += _ChangeRecordName;
        _recordDisplayView.OnRemoveRecordAction += _RemoveRecord;

        _enterGameBtn.onClick.AddListener(_Leave);
        _addRecordBtn.onClick.AddListener(() => _AddRecord());
    }

    private void _UnRegister()
    {
        _recordDisplayView.OnChangeRecordNameAction -= _ChangeRecordName;
        _recordDisplayView.OnRemoveRecordAction -= _RemoveRecord;

        _enterGameBtn.onClick.RemoveAllListeners();
        _addRecordBtn.onClick.RemoveAllListeners();
    }

    private void _Leave()
    {
        if(_selectRecordId != -1)
        {
            GameUtility.Instance.SelectCurGameRecord(_selectRecordId);
            _leave = true;
        }
    }

    private void _CreateRecords()
    {
        _ClearPool();
        if (_gameRecords.Records.Count > 0)
        {
            var records = _gameRecords.Records.Values.OrderBy(record => record.UpdateTime).ToList();
            for (int i = 0; i < records.Count; i++)
                _AddRecordElement(records[i]);
        }
        _UpdateAddElementBtn();
    }

    private void _ClearPool()
    {
        if (_recordViews == null || _recordViews.Count == 0)
            return;

        foreach (var pair in _recordViews)
        {
            var elementView = pair.Value;
            elementView.Dispose();
            _recordPool.ReturnInstance(elementView.gameObject);
            _gameRecords.RemoveRecord(pair.Key);
        }
        _recordViews.Clear();
        _gameRecords.Records.Clear();
    }

    private void _AddRecordElement(GameRecord record)
    {
        var obj = _recordPool.GetInstance();
        var trans = obj.GetComponent<Transform>();
        trans.SetParent(_recordAnchor);

        var elementView = obj.GetComponent<RecordElementView>();
        elementView.SetData(record, _toggleGroup);
        elementView.OnSelectRecordElement -= _SelectRecordElement;
        elementView.OnSelectRecordElement += _SelectRecordElement;
        _recordViews.Add(record.RecordID, elementView);
    }

    private void _AddRecord(string name = "")
    {
        if (_gameRecords.Records.Count < _maxRecordCount)
        {
            GameRecord record = new GameRecord(name);
            _gameRecords.Records.Add(record.RecordID, record);
            _AddRecordElement(record);
            _recordViews[record.RecordID].ChangeToggleStatus(true);
            _UpdateAddElementBtn();
            GameUtility.Instance.Save();
        }
    }

    private void _RemoveRecord(int id)
    {
        var elementView = _recordViews[id];
        elementView.Dispose();
        _recordPool.ReturnInstance(elementView.gameObject);
        _recordViews.Remove(id);
        _gameRecords.RemoveRecord(id);
        GameUtility.Instance.Save();
    }

    private void _UpdateAddElementBtn()
    {
        _addRecordBtn.gameObject.SetActive(_gameRecords.Records.Count < _maxRecordCount);
        if (_addRecordBtn.gameObject.activeSelf)
            _addRecordBtn.transform.SetAsLastSibling();
    }

    private void _SelectRecordElement(int recordId,bool select)
    {
        if (select)
        {
            _selectRecordId = recordId;
            _recordDisplayView.Show(_gameRecords.Records[recordId]);
        }
        else if (_selectRecordId == recordId)
        {
            _selectRecordId = -1;
            _recordDisplayView.Hide();
        }
    }

    private void _ChangeRecordName(int id,string name)
    {
        if (!_gameRecords.Records.ContainsKey(id) || string.IsNullOrEmpty(name))
            return;

        var record = _gameRecords.Records[id];
        record.RecordName = name;
        _recordViews[id].SetData(record, _toggleGroup);
        _recordViews[id].ChangeToggleStatus(true);
        GameUtility.Instance.Save();
    }
}
