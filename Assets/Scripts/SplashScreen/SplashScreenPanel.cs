using System.Collections;
using System.Collections.Generic;
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

    private GameRecords _gameRecords = null;
    private GameData _gameData = null;
    private int _maxRecordCount;
    public int SelectRecordId { get; private set; }

    private Dictionary<int, RecordElementView> _recordViews = new Dictionary<int, RecordElementView>();

    private bool _leave = false;

    private void Start()
    {
        SceneModel.Instance.GoToStartScene();
    }

    public void Init(GameRecords records,GameData gameData)
    {
        _gameRecords = records;
        _gameData = gameData;
        _maxRecordCount = _gameData.ConstantData.MAX_RECORD_COUNT;
        SelectRecordId = -1;
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
        _enterGameBtn.onClick.AddListener(_Leave);
        _addRecordBtn.onClick.AddListener(() => _AddRecord());
    }

    private void _UnRegister()
    {
        _enterGameBtn.onClick.RemoveAllListeners();
        _addRecordBtn.onClick.RemoveAllListeners();
    }

    private void _Leave()
    {
        if(SelectRecordId != -1)
            _leave = true;
    }

    private void _CreateRecords()
    {
        _ClearPool();
        if (_gameRecords.Count > 0)
        {
            foreach (var pair in _gameRecords)
            {
                _AddRecordElement(pair.Value);
            }
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
            _gameRecords[pair.Key].Dispose();
        }
        _recordViews.Clear();
        _gameRecords.Clear();
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
        if (_gameRecords.Count < _maxRecordCount)
        {
            GameRecord record = new GameRecord(name);
            _gameRecords.Add(record.RecordID, record);
            _AddRecordElement(record);
            _SelectRecordElement(record.RecordID, true);
            _UpdateAddElementBtn();
            GameUtility.Instance.Save(_gameRecords);
        }
    }

    private void _RemoveRecord(int id)
    {
        var elementView = _recordViews[id];
        elementView.Dispose();
        _recordPool.ReturnInstance(elementView.gameObject);
        _gameRecords[id].Dispose();
        _recordViews.Remove(id);
        _gameRecords.Remove(id);
        GameUtility.Instance.Save(_gameRecords);
    }

    private void _UpdateAddElementBtn()
    {
        _addRecordBtn.gameObject.SetActive(_gameRecords.Count < _maxRecordCount);
        if (_addRecordBtn.gameObject.activeSelf)
            _addRecordBtn.transform.SetAsLastSibling();
    }

    private void _SelectRecordElement(int recordId,bool select)
    {
        if (select)
            SelectRecordId = recordId;
        else if (SelectRecordId == recordId)
            SelectRecordId = -1;
    }
}
