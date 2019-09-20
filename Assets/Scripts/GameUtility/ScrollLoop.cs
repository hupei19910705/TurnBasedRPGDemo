using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using Constraint = UnityEngine.UI.GridLayoutGroup.Constraint;
using Axis = UnityEngine.UI.GridLayoutGroup.Axis;

namespace Utility.GameUtility
{
    public class ScrollLoop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private enum Direction
        {
            Top,
            Bottom,
            Left,
            Right
        }

        [SerializeField] private GeneralObjectPool _objPool = null;
        [SerializeField] private RectTransform _viewport = null;
        [SerializeField] private RectTransform _content = null;
        [Header("Move")]
        [SerializeField] private bool _isHorizontal = true;
        [SerializeField] private bool _isVertical = true;
        [SerializeField] private bool _isElastic = true;
        [SerializeField] private float _elasticity = 0.05f;
        [SerializeField] private float _maxSlidingSpeed = 0f;
        [SerializeField] private float _minSlidingSpeed = 0f;
        [Header("Modify Grid Layout Group")]
        [SerializeField] private int _widthCount = 0;

        #region Event
        public event Action<GameObject> InitElement;
        #endregion

        #region Status
        private bool _isDragging = false;
        #endregion

        #region Local Data
        private int _elementTotalCount = 0;
        private Vector2Int _visibleCount;
        private List<int> _topElements = new List<int>();
        private List<int> _bottomElements = new List<int>();
        private List<int> _leftElements = new List<int>();
        private List<int> _rightElements = new List<int>();
        private Dictionary<int, GameObject> _elements = new Dictionary<int, GameObject>();
        private Vector2 _velocity = Vector2.zero;
        private Vector2 _startDragPos;
        private Vector2 _elementSize;
        private GridLayoutGroup _layoutGroup;
        private Bounds _viewBounds;
        #endregion

        #region Const
        private const int VISIBLE_ELEMENT_ADD_NUM = 2;
        #endregion

        private void Start()
        {
            int count = 91;
            Initialize(count);
        }

        private void Update()
        {
            if (!_isDragging)
            {
                if (_isElastic)
                    _Elastic();
                _CounterMove();
                _Move(_velocity);
                _CheckOnBorder();
            }
            _UpdateElements();
        }

        #region Init
        public void Initialize(int totalCount)
        {
            if (totalCount < 0)
                totalCount = 0;
            _elementTotalCount = totalCount;
            _elementSize = _objPool.GetPrefabBounds().size;
            _viewBounds = _GetBoundsInWorldSpace(_viewport);
            _layoutGroup = _content.GetComponent<GridLayoutGroup>();

            _visibleCount.x = Mathf.FloorToInt((_viewBounds.size.x / (_elementSize.x + _layoutGroup.spacing.x)) + (_isHorizontal ? VISIBLE_ELEMENT_ADD_NUM : 0));
            _visibleCount.y = Mathf.FloorToInt((_viewBounds.size.y / (_elementSize.y + _layoutGroup.spacing.y)) + (_isVertical ? VISIBLE_ELEMENT_ADD_NUM : 0));

            _InitLayoutGroup();
            _InitElements();
        }

        private void _InitLayoutGroup()
        {
            _layoutGroup.cellSize = _elementSize;
            _layoutGroup.constraint = Constraint.FixedColumnCount;
            _widthCount = _isHorizontal ? _widthCount : Mathf.Min(_widthCount, _visibleCount.x);
            _layoutGroup.constraintCount = _visibleCount.x;
        }

        private void _InitElements()
        {
            var visibleCount = _visibleCount.x * _visibleCount.y;
            visibleCount = visibleCount < _elementTotalCount ? visibleCount : _elementTotalCount;
            _objPool.InitPool(visibleCount);

            var visibleList = new List<int>();
            for (int i = 0; i < _elementTotalCount; i++)
            {
                if (i % _widthCount < _visibleCount.x && i / _widthCount < _visibleCount.y)
                    visibleList.Add(i);
            }
            visibleList.Sort();
            _InstantiateElements(visibleList);
            _FreshBorderElements();
        }
        #endregion

        #region Elements Func
        private void _InstantiateElements(List<int> elementIndexes,Direction dir = Direction.Bottom)
        {
            var newList = new List<int>();
            for (int i = 0; i < elementIndexes.Count; i++)
            {
                if (elementIndexes[i] < 0 || elementIndexes[i] >= _elementTotalCount)
                    continue;

                if (!_elements.ContainsKey(elementIndexes[i]))
                    _InstantiateElement(elementIndexes[i]);

                newList.Add(elementIndexes[i]);
            }
            _SetElementsSiblings(newList, dir);
        }

        private void _SetElementsSiblings(List<int> elementIndexes, Direction dir = Direction.Bottom)
        {
            switch (dir)
            {
                case Direction.Top:
                    for (int i = elementIndexes.Count - 1; i >= 0; i--)
                        _elements[elementIndexes[i]].transform.SetAsFirstSibling();
                    break;
                case Direction.Bottom:
                    break;
                case Direction.Left:
                    for (int i = 0; i < elementIndexes.Count; i++)
                        _elements[elementIndexes[i]].transform.SetSiblingIndex(_topElements.Count * i);
                    break;
                case Direction.Right:
                    var width = _topElements.Count;
                    for (int i = 0; i < elementIndexes.Count; i++)
                        _elements[elementIndexes[i]].transform.SetSiblingIndex(width * i + width);
                    break;
            }
        }

        private void _InstantiateElement(int index)
        {
            var obj = _objPool.GetInstance();
            obj.transform.SetParent(_content, false);
            if (InitElement != null)
                InitElement(obj);
            _elements.Add(index, obj);

            obj.GetComponentInChildren<Text>().text = index.ToString();
        }

        private void _ReturnElements(List<int> elementIndexes)
        {
            foreach (var idx in elementIndexes)
            {
                _objPool.ReturnInstance(_elements[idx]);
                _elements.Remove(idx);
            }
            
        }

        private void _FreshBorderElements()
        {
            _topElements.Clear();
            _bottomElements.Clear();
            _leftElements.Clear();
            _rightElements.Clear();

            var list = _elements.Keys.OrderBy(idx => idx).ToList();
            var first = list.First();
            var last = list.Last();

            int topColumn = first / _widthCount;
            int bottomColumn = last / _widthCount;
            int leftRow = first % _widthCount;

            foreach (var idx in list)
            {
                if (idx / _widthCount == topColumn)
                    _topElements.Add(idx);

                if (idx / _widthCount == bottomColumn)
                    _bottomElements.Add(idx);

                if (idx % _widthCount == leftRow)
                    _leftElements.Add(idx);
            }

            _topElements.Sort();
            _bottomElements.Sort();
            _leftElements.Sort();

            int rightRow = _topElements.Last() % _widthCount;
            _rightElements = list.FindAll(idx => idx % _widthCount == rightRow);
            _rightElements.Sort();
        }

        private bool _SideElementsLoaded(Direction dir)
        {
            switch (dir)
            {
                case Direction.Top:
                    return _topElements.First() / _widthCount == 0;
                case Direction.Bottom:
                    var first = _bottomElements.First();
                    var lastColumn = (_elementTotalCount - 1) / _widthCount;

                    if (first / _widthCount == lastColumn)
                        return true;

                    if (first + _widthCount > _elementTotalCount - 1)
                        return first / _widthCount == lastColumn - 1;

                    return false;
                case Direction.Left:
                    return _leftElements.First() % _widthCount == 0;
                case Direction.Right:
                    return _rightElements.First() % _widthCount == _widthCount - 1;
            }
            return false;
        }

        private void _UpdateElements()
        {
            _UpdateTopAndBottomElements();
            _UpdateLeftAndRightElements();
        }

        private void _UpdateTopAndBottomElements()
        {
            if (!_isVertical)
                return;

            if(_velocity.y > 0f)
                _UpdateMoveToTopElements();
            else
                _UpdateMoveToBottomElements();
        }

        private void _UpdateLeftAndRightElements()
        {
            if (!_isHorizontal)
                return;

            if(_velocity.x < 0f)
                _UpdateMoveToLeftElements();
            else
                _UpdateMoveToRightElements();
        }

        private void _UpdateMoveToTopElements()
        {
            var offset = _GetBorderOffset();
            var space = _layoutGroup.spacing.y + _elementSize.y;
            var changePos = Vector2.zero;
            var change = false;

            if (_leftElements.Count > _visibleCount.y && offset.top > space * 2)
            {
                change = true;
                _ReturnElements(_topElements);
                changePos += new Vector2(0f, -space);
            }

            if (offset.bottom < space)
            {
                var newList = new List<int>();
                _bottomElements.ForEach(idx =>
                {
                    var newIdx = idx + _widthCount;
                    if (newIdx >= 0 && newIdx < _elementTotalCount && ((idx % _widthCount) == (newIdx % _widthCount)))
                        newList.Add(newIdx);
                });

                if (newList.Count > 0)
                {
                    change = true;
                    newList.Sort();
                    _InstantiateElements(newList, Direction.Bottom);
                }
            }

            if (change)
            {
                _content.anchoredPosition += changePos;
                _FreshBorderElements();
            }
        }

        private void _UpdateMoveToBottomElements()
        {
            var offset = _GetBorderOffset();
            var space = _layoutGroup.spacing.y + _elementSize.y;
            var changePos = Vector2.zero;
            var change = false;

            if (_leftElements.Count > _visibleCount.y && offset.bottom > space * 2)
            {
                change = true;
                _ReturnElements(_bottomElements);
                if (!_SideElementsLoaded(Direction.Top))
                    changePos += new Vector2(0f, space);
            }

            if (offset.top < space)
            {
                var newList = new List<int>();
                _topElements.ForEach(idx =>
                {
                    var newIdx = idx - _widthCount;
                    if (newIdx >= 0 && newIdx < _elementTotalCount && ((idx % _widthCount) == (newIdx % _widthCount)))
                        newList.Add(newIdx);
                });

                if (newList.Count > 0)
                {
                    change = true;

                    var diff = _visibleCount.x - _topElements.Count;
                    while (diff > 0)
                    {
                        var last = newList.Last();
                        if (last + 1 < _elementTotalCount)
                        {
                            newList.Add(last + 1);
                            diff--;
                        }
                        else
                            break;
                    }

                    newList.Sort();
                    _InstantiateElements(newList, Direction.Top);
                    if (_SideElementsLoaded(Direction.Bottom))
                        changePos += new Vector2(0f, space);
                }
            }

            if (change)
            {
                _content.anchoredPosition += changePos;
                _FreshBorderElements();
            }
        }

        private void _UpdateMoveToLeftElements()
        {
            var offset = _GetBorderOffset();
            var space = _layoutGroup.spacing.x + _elementSize.x;
            var changePos = Vector2.zero;
            var change = false;

            if(_topElements.Count > _visibleCount.x && offset.left > space * 2)
            {
                change = true;
                _ReturnElements(_leftElements);
                if (!_SideElementsLoaded(Direction.Right))
                    changePos += new Vector2(space, 0f);
            }

            if(offset.right < space)
            {
                var newList = new List<int>();
                _rightElements.ForEach(idx =>
                {
                    var newIdx = idx + 1;
                    if (newIdx >= 0 && newIdx < _elementTotalCount && ((idx / _widthCount) == (newIdx / _widthCount)))
                        newList.Add(newIdx);
                });

                if (newList.Count > 0)
                {
                    change = true;
                    newList.Sort();
                    _InstantiateElements(newList, Direction.Right);
                    if(_SideElementsLoaded(Direction.Left))
                        changePos += new Vector2(space, 0f);
                }
            }

            if(change)
            {
                _content.anchoredPosition += changePos;
                _FreshBorderElements();
            }
        }

        private void _UpdateMoveToRightElements()
        {
            var offset = _GetBorderOffset();
            var space = _layoutGroup.spacing.x + _elementSize.x;
            var changePos = Vector2.zero;
            var change = false;

            if (_topElements.Count > _visibleCount.x && offset.right > space * 2)
            {
                change = true;
                _ReturnElements(_rightElements);
                if (!_SideElementsLoaded(Direction.Right))
                    changePos += new Vector2(-space, 0f);
            }

            if(offset.left < space)
            {
                var newList = new List<int>();
                _leftElements.ForEach(idx =>
                {
                    var newIdx = idx - 1;
                    if (newIdx >= 0 && newIdx < _elementTotalCount && ((idx / _widthCount) == (newIdx / _widthCount)))
                        newList.Add(newIdx);
                });
                
                if(newList.Count> 0)
                {
                    change = true;

                    var diff = _visibleCount.y - _leftElements.Count;
                    while (diff > 0)
                    {
                        var last = newList.Last();
                        if (last + _widthCount < _elementTotalCount)
                        {
                            newList.Add(last + 1);
                            diff--;
                        }
                        else
                            break;
                    }

                    newList.Sort();
                    _InstantiateElements(newList, Direction.Left);
                    if(_SideElementsLoaded(Direction.Right))
                    changePos += new Vector2(-space, 0f);
                }
            }

            if (change)
            {
                _content.anchoredPosition += changePos;
                _FreshBorderElements();
            }
        }
        #endregion

        #region Drag
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isDragging || !_viewBounds.Contains(eventData.position))
                return;

            _isDragging = true;
            _startDragPos = eventData.position;
            _velocity = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                OnEndDrag(eventData);
                return;
            }
            var posDelta = _TranslatePosDeltaToSpeed(eventData.position - _startDragPos, false);

            if (!_isElastic)
            {
                if ((posDelta.y < 0f && _SideElementsLoaded(Direction.Top)) || (posDelta.y > 0f && _SideElementsLoaded(Direction.Bottom)))
                    posDelta.y = 0f;

                if ((posDelta.x > 0f && _SideElementsLoaded(Direction.Right)) || (posDelta.x < 0f && _SideElementsLoaded(Direction.Left)))
                    posDelta.x = 0f;
            }

            _startDragPos = eventData.position;
            _Move(posDelta);
            _velocity = posDelta;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            _velocity = _TranslatePosDeltaToSpeed(eventData.position - _startDragPos);
            _startDragPos = eventData.position;
        }
        #endregion

        #region Move
        private void _Move(Vector2 posDelta)
        {
            if (!_isHorizontal)
                posDelta.x = 0f;
            if (!_isVertical)
                posDelta.y = 0f;

            _content.anchoredPosition += posDelta;
        }

        private void _Elastic()
        {
            if (Mathf.Abs(_velocity.x) < _minSlidingSpeed)
                _velocity.x = 0f;

            if (Mathf.Abs(_velocity.y) < _minSlidingSpeed)
                _velocity.y = 0f;

            _velocity *= (1 - _elasticity);
        }

        private void _CounterMove()
        {
            var counterPos = _GetCounterPosDelta();
            if (counterPos.sqrMagnitude < 1f)
                return;

            Vector2 posDelta = Vector2.zero;

            if (_velocity.x * counterPos.x > 0f )
                posDelta.x = Mathf.Abs(_velocity.x) > Mathf.Abs(counterPos.x) ? _velocity.x : counterPos.x;
            else
                posDelta.x = counterPos.x;

            if (_velocity.y * counterPos.y > 0f)
                posDelta.y = Mathf.Abs(_velocity.y) > Mathf.Abs(counterPos.y) ? _velocity.y : counterPos.y;
            else
                posDelta.y = counterPos.y;

            _velocity = _TranslatePosDeltaToSpeed(posDelta, false);

            if (Mathf.Abs(_velocity.x) < _minSlidingSpeed)
                _velocity.x = 0f;
            if (Mathf.Abs(_velocity.y) < _minSlidingSpeed)
                _velocity.y = 0f;
        }
        #endregion

        #region Utility
        private Bounds _GetBoundsInWorldSpace(RectTransform transform)
        {
            var center = transform.localToWorldMatrix.MultiplyPoint3x4(transform.rect.center);
            var min = transform.localToWorldMatrix.MultiplyPoint3x4(transform.rect.min);
            var max = transform.localToWorldMatrix.MultiplyPoint3x4(transform.rect.max);
            var size = new Vector3(max.x - min.x, max.y - min.y, 0f);
            return new Bounds(center, size);
        }

        private Vector2 _GetCounterPosDelta()
        {
            var offset = _GetBorderOffset();
            Vector2 counterPosDelta = Vector2.zero;
            if (_SideElementsLoaded(Direction.Top) && offset.top < 0)
                counterPosDelta.y = Mathf.Abs(offset.top);
            else if (_SideElementsLoaded(Direction.Bottom) && offset.bottom < 0)
                counterPosDelta.y = offset.bottom;

            if (_SideElementsLoaded(Direction.Left) && offset.left < 0)
                counterPosDelta.x = offset.left;
            else if (_SideElementsLoaded(Direction.Right) && offset.right < 0)
                counterPosDelta.x = Mathf.Abs(offset.right);

            return counterPosDelta;
        }

        private bool _OnTheBorder(Direction dir)
        {
            var offset = _GetBorderOffset();
            if (!_SideElementsLoaded(dir))
                return false;
            switch (dir)
            {
                case Direction.Top:
                    return Mathf.Abs(offset.top) < _maxSlidingSpeed;
                case Direction.Bottom:
                    return Mathf.Abs(offset.bottom) < _maxSlidingSpeed;
                case Direction.Left:
                    return Mathf.Abs(offset.left) < _maxSlidingSpeed;
                case Direction.Right:
                    return Mathf.Abs(offset.right) < _maxSlidingSpeed;
            }
            return false;
        }

        private RectOffset _GetBorderOffset()
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
            var contentBounds = _GetBoundsInWorldSpace(_content);
            return new RectOffset(
                Mathf.FloorToInt(_viewBounds.min.x - contentBounds.min.x),
                Mathf.FloorToInt(contentBounds.max.x - _viewBounds.max.x),
                Mathf.FloorToInt(contentBounds.max.y - _viewBounds.max.y),
                Mathf.FloorToInt(_viewBounds.min.y - contentBounds.min.y)
                );
        }

        private Vector2 _TranslatePosDeltaToSpeed(Vector2 posDelta,bool useMinSpeed = true)
        {
            var minSpeed = useMinSpeed ? _minSlidingSpeed : 0f;

            if (Mathf.Abs((float)posDelta.x) > _maxSlidingSpeed)
                posDelta.x = posDelta.x >= 0 ? _maxSlidingSpeed : -_maxSlidingSpeed;
            else if(Mathf.Abs((float)posDelta.x) < minSpeed)
                posDelta.x = posDelta.x >= 0 ? minSpeed : -minSpeed;

            if (Mathf.Abs((float)posDelta.y) > _maxSlidingSpeed)
                posDelta.y = posDelta.y >= 0 ? _maxSlidingSpeed : -_maxSlidingSpeed;
            else if (Mathf.Abs((float)posDelta.y) < minSpeed)
                posDelta.y = posDelta.y >= 0 ? minSpeed : -minSpeed;

            return posDelta;
        }

        private void _CheckOnBorder()
        {
            var offset = _GetBorderOffset();
            var movePos = Vector2.zero;
            if (_isVertical)
            {
                if(_SideElementsLoaded(Direction.Top) && Mathf.Abs(offset.top) > 1f && Mathf.Abs(offset.top) < _maxSlidingSpeed)
                {
                    _velocity.y = 0f;
                    movePos.y = -offset.top;
                }
                else if(_SideElementsLoaded(Direction.Bottom) && Mathf.Abs(offset.bottom) > 1f && Mathf.Abs(offset.bottom) < _maxSlidingSpeed)
                {
                    _velocity.y = 0f;
                    movePos.y = offset.bottom;
                }
            }

            if (_isHorizontal)
            {
                if (_SideElementsLoaded(Direction.Left) && Mathf.Abs(offset.left) > 1f && Mathf.Abs(offset.left) < _maxSlidingSpeed)
                {
                    _velocity.x = 0f;
                    movePos.x = offset.left;
                }
                else if (_SideElementsLoaded(Direction.Right) && Mathf.Abs(offset.right) > 1f && Mathf.Abs(offset.right) < _maxSlidingSpeed)
                {
                    _velocity.x = 0f;
                    movePos.x = -offset.right;
                }
            }

            _Move(movePos);
        }
        #endregion
    }
}