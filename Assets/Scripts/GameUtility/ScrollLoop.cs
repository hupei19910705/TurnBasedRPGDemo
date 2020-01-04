using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.GameUtility;

public class ScrollLoop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private GeneralObjectPool _pool = null;
    [SerializeField] private RectTransform _viewport = null;
    [SerializeField] private RectTransform _content = null;

    [Header("Grid Layout")]
    [SerializeField] private RectOffset _padding = null;
    [SerializeField] private Vector2 _spacing = default;

    [Header("Move")]
    [SerializeField] private float _dragSpeed = 0f;
    [SerializeField] [Range(0, 1)] private float _maxSpeedSmooth = 0f;
    [SerializeField] [Range(0, 1)] private float _minSpeedSmooth = 0f;
    [SerializeField] private bool _sliding = false;
    [SerializeField] [Range(0, 1)] private float _deceleritionRate = 0f;

    private Bounds _viewBounds;
    private Vector2 _cellSize;
    private Vector2 _leftOffset;
    private Vector2 _topOffset;
    private Vector2 _curDragPos;
    private Vector3 _slidingSpeed;
    private Vector2 _maxSpeed;
    private Vector2 _minSpeed;

    private int _totalCount;
    private int _maxColumnCount;
    private int _maxActiveColumnCount;
    private int _maxActiveRowCount;
    private bool _horizontalDragable;
    private bool _isDraging;
    private bool _isSliding;

    private List<int> _topElementIdxes = new List<int>();
    private List<int> _bottomElementIdxes = new List<int>();
    private List<int> _leftElementIdxes = new List<int>();
    private List<int> _rightElementIdxes = new List<int>();
    private Dictionary<int, GameObject> _activeElements = new Dictionary<int, GameObject>();

    public event Action<int, GameObject> SetElement;

    private enum SlidingStatus
    {
        None,
        HorizontalStop,
        VerticalStop,
        AllStop
    }

    private void Start()
    {
        //Init(100, 10);
    }

    private void Update()
    {
        var slidingStatus = _CheckBorderElements();
        if (_isSliding)
            _OnSliding(slidingStatus);
    }

    private void _SetTestElementData(int index, GameObject instance)
    {
        instance.GetComponentInChildren<Text>().text = index.ToString();
    }

    #region Init
    public void Init(int column = -1)
    {
        _totalCount = 0;
        _pool.InitPool();
        _viewBounds = _GetBoundsInWorldSpace(_viewport);
        _cellSize = _pool.GetPrefabBounds().size;
        _leftOffset = new Vector2(_spacing.x, _spacing.x * 2 + _cellSize.x);
        _topOffset = new Vector2(_spacing.y, _spacing.y * 2 + _cellSize.y);
        _maxSpeed = _cellSize * _maxSpeedSmooth;
        _minSpeed = _cellSize * _minSpeedSmooth;

        var basicColumnCount = Mathf.FloorToInt((_viewport.sizeDelta.x + _spacing.x - _padding.left - _padding.right) / (_cellSize.x + _spacing.x));
        if (column == -1 || column <= basicColumnCount)
        {
            _maxColumnCount = _maxActiveColumnCount = basicColumnCount;
            _horizontalDragable = false;
        }
        else
        {
            _maxActiveColumnCount = basicColumnCount + 2;
            _maxColumnCount = Mathf.Max(column, _maxActiveColumnCount);
            _horizontalDragable = true;
        }

        var basicRowCount = Mathf.FloorToInt((_viewport.sizeDelta.y + _spacing.y - _padding.top - _padding.bottom) / (_cellSize.y + _spacing.y));
        _maxActiveRowCount = basicRowCount + 2;

        _InitGridLayoutGroup();
        _ClearAllElements();
    }

    public void InitElements(int count)
    {
        _totalCount = count;
        _ClearAllElements();
        _InitElements();
        _UpdateBorderElements();
    }

    private void _InitGridLayoutGroup()
    {
        var layoutGroup = _content.GetComponent<GridLayoutGroup>();
        layoutGroup.padding = _padding;
        layoutGroup.cellSize = _cellSize;
        layoutGroup.spacing = _spacing;
        layoutGroup.constraintCount = _maxActiveColumnCount;
    }

    private void _InitElements()
    {
        for (int row = 0; row < _maxActiveRowCount; row++)
        {
            for (int col = 0; col < _maxActiveColumnCount; col++)
            {
                int index = row * _maxColumnCount + col;
                if (index >= _totalCount)
                    return;
                AddElement(index);
            }
        }
    }
    #endregion

    #region Pointer Event
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_isDraging || !_viewBounds.Contains(eventData.position))
            return;

        _isDraging = true;
        _isSliding = false;
        _curDragPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDraging)
            return;

        Vector3 deltaPos = eventData.position - _curDragPos;
        _curDragPos = eventData.position;
        _Move(deltaPos * _dragSpeed);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDraging)
            return;

        _isDraging = false;
        Vector3 deltaPos = eventData.position - _curDragPos;
        _curDragPos = eventData.position;
        _Sliding(deltaPos * _dragSpeed);
    }
    #endregion

    #region Move
    private void _Move(Vector3 delta)
    {
        _maxSpeed = _cellSize * _maxSpeedSmooth;
        _minSpeed = _cellSize * _minSpeedSmooth;
        delta = _CheckSpeed(delta);
        
        if (Mathf.Abs(delta.x) < _minSpeed.x && Mathf.Abs(delta.y) < _minSpeed.y)
            return;

        _content.position += delta;
    }

    private void _Sliding(Vector3 delta)
    {
        if (!_sliding)
            return;

        if (Mathf.Abs(delta.x) < _minSpeed.x && Mathf.Abs(delta.y) < _minSpeed.y)
            return;

        _slidingSpeed = _CheckSpeed(delta);
        _isSliding = true;
    }

    private void _OnSliding(SlidingStatus slidingStatus)
    {
        bool xStop = Mathf.Abs(_slidingSpeed.x) < _minSpeed.x || 
            slidingStatus == SlidingStatus.AllStop || 
            slidingStatus == SlidingStatus.HorizontalStop;

        bool yStop = Mathf.Abs(_slidingSpeed.y) < _minSpeed.y || 
            slidingStatus == SlidingStatus.AllStop || 
            slidingStatus == SlidingStatus.VerticalStop;

        if (xStop)
            _slidingSpeed.x = 0;

        if (yStop)
            _slidingSpeed.y = 0;

        _isSliding = !xStop || !yStop;
        _content.position += _slidingSpeed;
        _slidingSpeed *= (1 - _deceleritionRate);
    }
    #endregion

    #region Elements
    public void AddElement(int index)
    {
        if (_activeElements.ContainsKey(index) || index >= _totalCount)
            return;

        var instance = _pool.GetInstance();
        var trans = instance.transform;
        trans.SetParent(_content);
        trans.SetSiblingIndex(_GetSerialNumInActiveElements(index));

        if (SetElement != null)
            SetElement(index, instance);
        //_SetTestElementData(index, instance);
        _activeElements.Add(index, instance);
    }

    public void RemoveElement(int index)
    {
        if (!_activeElements.ContainsKey(index) || index >= _totalCount)
            return;

        var instance = _activeElements[index];
        _pool.ReturnInstance(instance);
        _activeElements.Remove(index);
    }

    private void _ClearAllElements()
    {
        _topElementIdxes.Clear();
        _bottomElementIdxes.Clear();
        _leftElementIdxes.Clear();
        _rightElementIdxes.Clear();
        foreach (var obj in _activeElements.Values)
            _pool.ReturnInstance(obj);
        _activeElements.Clear();
    }

    private void _EraseLeftAndAddRight()
    {
        if (_rightElementIdxes.Count == 0)
            return;

        var rightStart = _rightElementIdxes.First();
        if (_IsSameColumn(rightStart, _maxColumnCount - 1))
            return;

        foreach (var index in _leftElementIdxes)
            RemoveElement(index);

        foreach (var index in _rightElementIdxes)
        {
            var newIdx = index + 1;
            AddElement(newIdx);
        }

        _content.position += new Vector3(_cellSize.x + _spacing.x, 0f, 0f);
        _UpdateBorderElements();
    }

    private void _EraseRightAndAddLeft()
    {
        if (_leftElementIdxes.Count == 0)
            return;

        var leftStart = _leftElementIdxes.First();
        if (_IsSameColumn(leftStart, 0))
            return;

        foreach (var index in _rightElementIdxes)
            RemoveElement(index);

        for (int i = 0; i < _maxActiveRowCount; i++)
        {
            var newIdx = leftStart - 1 + _maxColumnCount * i;
            if (newIdx >= _totalCount)
                continue;
            AddElement(newIdx);
        }

        _content.position -= new Vector3(_cellSize.x + _spacing.x, 0f, 0f);
        _UpdateBorderElements();
    }

    private void _EraseTopAndAddBottom()
    {
        if (_bottomElementIdxes.Count == 0)
            return;

        var bottomStart = _bottomElementIdxes.First();
        if (bottomStart + _maxColumnCount >= _totalCount)
            return;

        foreach (var index in _topElementIdxes)
            RemoveElement(index);

        foreach (var index in _bottomElementIdxes)
        {
            var newIdx = index + _maxColumnCount;
            if (newIdx >= _totalCount)
                continue;
            AddElement(newIdx);
        }

        _content.position -= new Vector3(0f, _cellSize.y + _spacing.y, 0f);
        _UpdateBorderElements();
    }

    private void _EraseBottomAndAddTop()
    {
        if (_topElementIdxes.Count == 0)
            return;

        var topStart = _topElementIdxes.First();
        if (_IsSameRow(topStart, 0))
            return;

        foreach (var index in _bottomElementIdxes)
            RemoveElement(index);

        foreach (var index in _topElementIdxes)
        {
            var newIdx = index - _maxColumnCount;
            if (newIdx < 0)
                continue;
            AddElement(newIdx);
        }

        _content.position += new Vector3(0f, _cellSize.y + _spacing.y, 0f);
        _UpdateBorderElements();
    }

    private SlidingStatus _CheckBorderElements()
    {
        var offset = _GetBorderOffset();
        Vector3 posDelta = Vector3.zero;
        bool verticalStop = false;
        bool horizontalStop = false;

        if (offset.top <= 0)
        {
            if(_topElementIdxes.Count > 0 && _IsSameRow(_topElementIdxes[0],0))
            {
                posDelta.y = -offset.top;
                verticalStop = true;
            }
        }
        else
        {
            if (offset.top > _topOffset.y)
                _EraseTopAndAddBottom();
            else if (offset.top < _topOffset.x)
                _EraseBottomAndAddTop();

            offset = _GetBorderOffset();
            if (offset.bottom < 0 && _bottomElementIdxes.Count > 0 && _bottomElementIdxes[0] + _maxColumnCount >= _totalCount)
            {
                posDelta.y = offset.bottom;
                verticalStop = true;
            }
        }


        if (offset.left <= 0)
        {
            if (_leftElementIdxes.Count > 0 && _IsSameColumn(_leftElementIdxes[0], 0))
            {
                posDelta.x = offset.left;
                horizontalStop = true;
            }
        }
        else
        {
            if (offset.left > _leftOffset.y)
                _EraseLeftAndAddRight();
            else if (offset.left < _leftOffset.x)
                _EraseRightAndAddLeft();

            offset = _GetBorderOffset();
            if (offset.right < 0 && _rightElementIdxes.Count > 0 && _IsSameColumn(_rightElementIdxes[0], _maxColumnCount - 1))
            {
                posDelta.x = -offset.right;
                horizontalStop = true;
            }
        }

        _content.position += posDelta;
        if (horizontalStop && verticalStop)
            return SlidingStatus.AllStop;
        else if (horizontalStop)
            return SlidingStatus.HorizontalStop;
        else if (verticalStop)
            return SlidingStatus.VerticalStop;
        else
            return SlidingStatus.None;
    }
    #endregion

    #region Utility
    private bool _IsSameRow(int first, int second)
    {
        return first / _maxColumnCount == second / _maxColumnCount;
    }

    private bool _IsSameColumn(int first, int second)
    {
        return first % _maxColumnCount == second % _maxColumnCount;
    }

    private void _UpdateBorderElements()
    {
        _topElementIdxes.Clear();
        _bottomElementIdxes.Clear();
        _leftElementIdxes.Clear();
        _rightElementIdxes.Clear();

        if (_activeElements.Count == 0)
            return;

        var indexList = _activeElements.Keys.ToList();
        indexList.Sort();
        int start = indexList[0];
        foreach (var index in indexList)
        {
            if (_IsSameRow(start, index))
                _topElementIdxes.Add(index);

            if (_IsSameColumn(start, index))
                _leftElementIdxes.Add(index);
        }

        int bottomStart = _leftElementIdxes.Last();
        int rightStart = _topElementIdxes.Last();
        foreach (var index in indexList)
        {
            if (_IsSameRow(bottomStart, index))
                _bottomElementIdxes.Add(index);

            if (_IsSameColumn(rightStart, index))
                _rightElementIdxes.Add(index);
        }
    }

    private Bounds _GetBoundsInWorldSpace(RectTransform trans)
    {
        var center = trans.localToWorldMatrix.MultiplyPoint3x4(trans.rect.center);
        var min = trans.localToWorldMatrix.MultiplyPoint3x4(trans.rect.min);
        var max = trans.localToWorldMatrix.MultiplyPoint3x4(trans.rect.max);
        var size = new Vector3(max.x - min.x, max.y - min.y, 0f);
        return new Bounds(center, size);
    }

    private int _GetSerialNumInActiveElements(int index)
    {
        if (_activeElements.Count == 0)
            return -1;

        var indexList = _activeElements.Keys.ToList();
        indexList.Sort();
        for (int i = 0; i < indexList.Count; i++)
        {
            if (index < indexList[i])
                return i;
        }

        return -1;
    }

    private RectOffset _GetBorderOffset()
    {
        var contentBounds = _GetBoundsInWorldSpace(_content);
        var top = Mathf.FloorToInt(contentBounds.max.y - _viewBounds.max.y);
        var bottom = Mathf.FloorToInt(_viewBounds.min.y - contentBounds.min.y);
        var left = Mathf.FloorToInt(_viewBounds.min.x - contentBounds.min.x);
        var right = Mathf.FloorToInt(contentBounds.max.x - _viewBounds.max.x);

        return new RectOffset(left, right, top, bottom);
    }

    private Vector3 _CheckSpeed(Vector3 speed)
    {
        var x = Mathf.Clamp(Mathf.Abs(speed.x), _minSpeed.x, _maxSpeed.x);
        var y = Mathf.Clamp(Mathf.Abs(speed.y), _minSpeed.x, _maxSpeed.x);

        speed.x = speed.x > 0 ? x : -x;
        speed.y = speed.y > 0 ? y : -y;

        if (!_horizontalDragable)
            speed.x = 0;

        return speed;
    }
    #endregion
}