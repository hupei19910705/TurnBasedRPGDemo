using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;

namespace Utility.GameUtility
{
    public class ScrollLoop : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private enum Direction
        {
            Front,
            Behind,
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
        [SerializeField] private float _slidingSpeed = 0f;
        [Header("Modify Grid Layout Group")]
        [SerializeField] private int _widthCount = 0;

        #region Event
        public event Action<GameObject> InitElement;
        #endregion

        #region Status
        private bool _isDragging = false;
        #endregion

        #region Local Data
        private int _elementCount = 0;
        private List<int> _frontElements;
        private List<int> _behindElements;
        private List<int> _leftElements;
        private List<int> _rightElements;
        private Dictionary<int, GameObject> _elements = new Dictionary<int, GameObject>();
        private Vector2 _velocity = Vector2.zero;
        private Vector2 _startDragPos;
        private Vector2 _elementSize;
        private GridLayoutGroup _layoutGroup;
        private Bounds _viewBounds;
        #endregion

        private void Start()
        {
            Initialize(10);
            int count = 5;
            while (count > 0)
            {
                count--;
            }
        }

        private void Update()
        {
            if (_isElastic)
                _Elastic();
        }

        #region Init
        public void Initialize(int totalCount)
        {
            _InitElements(totalCount);
            _InitViewport();
            _InitLayoutGroup();
        }

        private void _InitElements(int count)
        {
            if (count < 0)
                count = 0;
            _elementCount = count;
            _elementSize = _objPool.GetPrefabBounds().size;
            _objPool.InitPool(5);
        }

        private void _InitViewport()
        {
            _viewBounds = _GetBoundsInWorldSpace(_viewport);
        }

        private void _InitLayoutGroup()
        {
            _layoutGroup = _content.GetComponent<GridLayoutGroup>();
            _layoutGroup.cellSize = _elementSize;
            _layoutGroup.constraintCount = _widthCount;
        }
        #endregion

        #region Drag
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_isDragging || !_viewBounds.Contains(eventData.position))
                return;

            _isDragging = true;
            _startDragPos = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging)
            {
                OnEndDrag(eventData);
                return;
            }

            var posDelta = eventData.position - _startDragPos;
            _Move(posDelta);
            _startDragPos = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
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
            _velocity = posDelta;
        }

        private void _Elastic()
        {
            if (_isDragging || _velocity.magnitude < 0.01f)
                return;

            var posX = Mathf.Abs(_velocity.x) > _slidingSpeed ? (_velocity.x > 0 ? _slidingSpeed : -_slidingSpeed) : _velocity.x;
            var posY = Mathf.Abs(_velocity.y) > _slidingSpeed ? (_velocity.y > 0 ? _slidingSpeed : -_slidingSpeed) : _velocity.y;
            _Move(new Vector2(posX, posY));
            _velocity *= (1 - _elasticity);
        }
        #endregion

        #region Utility
        private void _InstantiateElements(List<int> elements, Direction direction = Direction.Behind)
        {
            switch (direction)
            {
                case Direction.Front:
                    break;
                case Direction.Behind:
                    for (int i = 0; i < elements.Count; i++)
                        _InstantiateElement(elements[i]);
                    break;
                case Direction.Left:
                    break;
                case Direction.Right:
                    break;
            }
            _UpdateBorderElements();
        }

        private void _InstantiateElement(int index)
        {
            var obj = _objPool.GetInstance();
            obj.transform.SetParent(_content, false);
            if (InitElement != null)
                InitElement(obj);
            _elements.Add(index, obj);
        }

        private void _UpdateBorderElements()
        {
            _frontElements.Clear();
            _behindElements.Clear();
            _leftElements.Clear();
            _rightElements.Clear();

            var list = _elements.Keys.OrderBy(idx => idx).ToList();
            int firstIdxAtLast = list.Count - list.Count % _widthCount;

            for (int i = 0; i < list.Count; i++)
            {
                if (i <= _widthCount - 1)
                    _frontElements.Add(list[i]);

                if (i >= firstIdxAtLast)
                    _behindElements.Add(list[i]);

                if (i % (_widthCount - 1) == 1)
                    _leftElements.Add(list[i]);
                else if (i % (_widthCount - 1) == 0)
                    _rightElements.Add(list[i]);
            }
        }

        private Bounds _GetBoundsInWorldSpace(RectTransform transform)
        {
            var center = transform.localToWorldMatrix.MultiplyPoint3x4(transform.rect.center);
            var min = transform.localToWorldMatrix.MultiplyPoint3x4(transform.rect.min);
            var max = transform.localToWorldMatrix.MultiplyPoint3x4(transform.rect.max);
            var size = new Vector3(max.x - min.x, max.y - min.y, 0f);
            return new Bounds(center, size);
        }

        private Vector2 _GetResistance()
        {
            return new Vector2();
        }

        private bool OnTheFrontBorder()
        {
            if (!_isVertical)
                return false;
            
            var contentBounds = _GetBoundsInWorldSpace(_content);
            return false;
        }
        #endregion
    }
}