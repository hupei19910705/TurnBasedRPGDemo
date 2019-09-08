using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utility.GameUtility;
using Utility;

public interface IBattleView
{
    void Enter(TeamData teamData,Dictionary<int, EnemyData> enemiesData);
    IEnumerator RunEnemyTurn(int enemyPos, int targetMemberPos);
    void ActiveMemberElement();

    event Action<bool, int> SelectMember;
    event Action<int> SelectEnemy;
    event Action<int> MemberEndTurn;
    event Action<double> MemberDamageEffect;
    event Action<double> EnemyDamageEffect;
}

public class BattleView : MonoBehaviour, IBattleView
{
    [Header("Team Member")]
    [SerializeField] private MemberGroupView _teamMemberGroupView = null;
    [SerializeField] private TeamMemberObjectPool _teamMemberPool = null;
    [SerializeField] private GeneralObjectPool _teamMemberElementPool = null;
    [SerializeField] private Transform _elementRoot = null;
    [SerializeField] private ToggleGroup _memberElementToggleGroup = null;
    [SerializeField] private MemberSelectionView _memberSelectionView = null;
    [SerializeField] private Transform _memberArrow = null;

    [Header("Enemy Member")]
    [SerializeField] private MemberGroupView _enemyMemberGroupView = null;
    [SerializeField] private EnemyMemberObjectPool _enemyMemberPool = null;
    [SerializeField] private Transform _enemyArrow = null;

    [Header("Other")]
    [SerializeField] private Transform _defaultArrowRoot = null;
    [SerializeField] private ScrollRect _ItemsOrSkillsScroll = null;
    [SerializeField] private GeneralObjectPool _itemPool = null;
    [SerializeField] private GeneralObjectPool _skillPool = null;
    [SerializeField] private SkillEffectManager _skillEffectManager = null;

    public event Action<bool,int> SelectMember;
    public event Action<int> SelectEnemy;
    public event Action<int> MemberEndTurn;
    public event Action<double> MemberDamageEffect;
    public event Action<double> EnemyDamageEffect;

    private TeamData _teamData = null;
    private Dictionary<int, MemberView> _memberViews = new Dictionary<int, MemberView>();
    private Dictionary<int, MemberElement> _memberElements = new Dictionary<int, MemberElement>();

    private Dictionary<int, EnemyData> _enemiesData = null;
    private Dictionary<int, EnemyView> _enemyViews = new Dictionary<int, EnemyView>();

    private List<GameObject> _itemObjs = new List<GameObject>();
    private List<GameObject> _skillObjs = new List<GameObject>();

    private int _curMemberIdx = -1;
    private int _curEnemyIdx = -1;
    private bool _operable = true;
    private int _targetMemberIdx = -1;

    private ParallelCoroutines _parallelCor = new ParallelCoroutines();

    public void Enter(TeamData teamData, Dictionary<int, EnemyData> enemiesData)
    {
        gameObject.SetActive(true);
        _RegisterEvents();

        _teamData = teamData;
        _enemiesData = enemiesData;

        _ShowItemOrSkillListPanel(false);

        _Init();
        _SetFirstEnemyArrow();
        StartCoroutine(_parallelCor.Execute());
    }

    private void _Init()
    {
        _teamMemberPool.InitPool();
        _teamMemberElementPool.InitPool();
        _enemyMemberPool.InitPool();
        _itemPool.InitPool(10);
        _skillPool.InitPool(10);

        _memberSelectionView.Init();
        _skillEffectManager.Init();

        _InitTeamMembers();
        _InitEnemies();
    }

    private void _RegisterEvents()
    {
        _UnRegisterEvents();
        _memberSelectionView.OnGeneralAttack += _MemberGeneralAttack;
        _memberSelectionView.ListSkills += _ListSkills;
        _memberSelectionView.ListItems += _ListItems;
        _memberSelectionView.OnSkip += _CurMemberEndTurn;
    }

    private void _UnRegisterEvents()
    {
        _memberSelectionView.OnGeneralAttack -= _MemberGeneralAttack;
        _memberSelectionView.OnSkip -= _CurMemberEndTurn;
    }

    private void _InitTeamMembers()
    {
        foreach (var member in _teamData.Members.Values.ToList())
        {
            _SetMemberView(member);
            _SetMemberElement(member);
        }
    }

    private void _SetFirstEnemyArrow()
    {
        _curEnemyIdx = -1;
        for(int i=0;i< 6;i++)
        {
            if(_enemiesData.ContainsKey(i) && _enemiesData[i].IsAlive)
            {
                _curEnemyIdx = i;
                break;
            }
        }
        if (_curEnemyIdx == -1)
            return;
        if (SelectEnemy != null)
            SelectEnemy(_curEnemyIdx);
        _enemyArrow.gameObject.SetActive(true);
        _SelectEnemy(_curEnemyIdx);
    }

    private void _SetMemberView(TeamMemberData member)
    {
        var instance = _teamMemberPool.GetInstance(member.Job);
        _teamMemberGroupView.LocateToPos(instance.transform, member.Pos);
        var memberView = instance.GetComponentInChildren<MemberView>();
        memberView.MemberDamageEffect -= _MemberDamageEffect;
        memberView.MemberEndTurn -= MemberEndTurn;
        memberView.MemberDamageEffect += _MemberDamageEffect;
        memberView.MemberEndTurn += MemberEndTurn;
        memberView.SetData(member);
        _memberViews.Add(member.Pos, memberView);
    }

    private void _SetMemberElement(TeamMemberData member)
    {
        var element = _teamMemberElementPool.GetInstance();
        element.SetActive(true);
        element.transform.SetParent(_elementRoot);
        var elementView = element.GetComponent<MemberElement>();
        elementView.SelectMember -= _SelectMember;
        elementView.SelectMember += _SelectMember;
        elementView.SetData(member, _memberElementToggleGroup);
        _memberElements.Add(member.Pos, elementView);
        elementView.LockToggle(!member.IsAlive);
    }

    private void _SelectMember(bool select,int pos = -1)
    {
        _curMemberIdx = select ? pos : -1;
        _memberSelectionView.Show(select);
        if (SelectMember != null)
            SelectMember(select, pos);
        _memberArrow.gameObject.SetActive(select);
        if (select)
        {
            _memberArrow.SetParent(_memberViews[pos].LeftLocate);
            _memberArrow.localPosition = Vector3.zero;
        }
        else
        {
            _ShowItemOrSkillListPanel(false);
            _memberArrow.SetParent(_defaultArrowRoot);
        }
    }

    private void _SelectEnemy(int pos)
    {
        _curEnemyIdx = pos;
        if (SelectEnemy != null)
            SelectEnemy(pos);
        _enemyArrow.gameObject.SetActive(true);
        _enemyArrow.SetParent(_enemyViews[_curEnemyIdx].FrontLocate);
        _enemyArrow.localPosition = Vector3.zero;
    }

    private void _InitEnemies()
    {
        foreach (var enemy in _enemiesData.Values.ToList())
        {
            _SetEnemyView(enemy);
        }
    }

    private void _SetEnemyView(EnemyData enemy)
    {
        var instance = _enemyMemberPool.GetInstance(enemy.Type);
        _enemyMemberGroupView.LocateToPos(instance.transform, enemy.Pos);
        var enemyView = instance.GetComponentInChildren<EnemyView>();
        enemyView.SelectEnemy -= _SelectEnemy;
        enemyView.EnemyDamageEffect -= _EnemyDamageEffect;
        enemyView.SelectEnemy += _SelectEnemy;
        enemyView.EnemyDamageEffect += _EnemyDamageEffect;
        enemyView.SetData(enemy);
        _enemyViews.Add(enemy.Pos, enemyView);
    }

    private void _MemberGeneralAttack()
    {
        if (!_operable)
            return;
        _operable = false;
        var memberView = _memberViews[_curMemberIdx];
        if (_curEnemyIdx == -1)
            _curEnemyIdx = _enemiesData.First().Key;

        var memberElement = _memberElements[_curMemberIdx];
        memberElement.SelectElement(false);
        memberElement.LockToggle(true);
        _parallelCor.Add(memberView.GeneralAttack(_enemyViews[_curEnemyIdx].FrontLocate.position));
    }

    private void _MemberDamageEffect(double attack)
    {
        if (MemberDamageEffect != null)
            MemberDamageEffect(attack);
        var targetView = _enemyViews[_curEnemyIdx];
        targetView.BeHit();
        var skillEffectView = _skillEffectManager.GetSkillEffectByType(SkillEffetType.Hit);
        skillEffectView.PlaySkillEffect(targetView.SkillEffectPos);

        if (!_enemiesData[_curEnemyIdx].IsAlive)
            _SetFirstEnemyArrow();
        _operable = true;
    }

    private void _CurMemberEndTurn()
    {
        if (!_operable)
            return;
        _operable = false;
        if (MemberEndTurn != null)
            MemberEndTurn(_curMemberIdx);
        var memberElement = _memberElements[_curMemberIdx];
        memberElement.SelectElement(false);
        memberElement.LockToggle(true);
        _operable = true;
    }

    public void ActiveMemberElement()
    {
        foreach(var member in _teamData.Members.Values)
            _memberElements[member.Pos].LockToggle(!member.IsAlive);
        _SetFirstEnemyArrow();
    }

    public IEnumerator RunEnemyTurn(int enemyPos,int targetMemberPos)
    {
        _memberArrow.gameObject.SetActive(false);
        _memberArrow.SetParent(_defaultArrowRoot);
        _enemyArrow.gameObject.SetActive(false);
        _enemyArrow.SetParent(_defaultArrowRoot);

        _targetMemberIdx = targetMemberPos;
        var enemyView = _enemyViews[enemyPos];
        yield return enemyView.GeneralAttack(_memberViews[_targetMemberIdx].FrontLocate.position);
    }

    private void _EnemyDamageEffect(double attack)
    {
        if (EnemyDamageEffect != null)
            EnemyDamageEffect(attack);
        _memberElements[_targetMemberIdx].UpdateStatus();
        var targetView = _memberViews[_targetMemberIdx];
        targetView.BeHit();
        var skillEffectView = _skillEffectManager.GetSkillEffectByType(SkillEffetType.Hit);
        skillEffectView.PlaySkillEffect(targetView.SkillEffectPos);
    }

    private void _ShowItemOrSkillListPanel(bool isShow)
    {
        _ItemsOrSkillsScroll.gameObject.SetActive(isShow);
    }

    private void _ListSkills(bool isShow)
    {
        _ClearItemAndSkillObjs();
        _ShowItemOrSkillListPanel(isShow);
        if(isShow)
        {
            var skills = _teamData.Members[_curMemberIdx].Skills;
            foreach(var pair in skills)
            {
                if (pair.Key == SkillType.GeneralAttack)
                    continue;
                foreach(var skill in pair.Value)
                {
                    var instance = _skillPool.GetInstance();
                    instance.transform.SetParent(_ItemsOrSkillsScroll.content);
                    var view = instance.GetComponent<SkillView>();
                    view.Initialize(skill);
                    _skillObjs.Add(instance);
                }
            }
        }
    }

    private void _ListItems(bool isShow)
    {
        _ClearItemAndSkillObjs();
        _ShowItemOrSkillListPanel(isShow);
        if (isShow)
        {
            var items = _teamData.BackPack;
            foreach (var pair in items)
            {
                if (pair.Key != ItemType.Potion)
                    continue;
                foreach (var item in pair.Value)
                {
                    var instance = _itemPool.GetInstance();
                    instance.transform.SetParent(_ItemsOrSkillsScroll.content);
                    var view = instance.GetComponent<ItemView>();
                    view.Initialize(item);
                    _itemObjs.Add(instance);
                }
            }
        }
    }

    private void _ClearItemAndSkillObjs()
    {
        for(int i= 0;i< _skillObjs.Count;i++)
            _skillPool.ReturnInstance(_skillObjs[i]);
        _skillObjs.Clear();

        for (int i = 0; i < _itemObjs.Count; i++)
            _itemPool.ReturnInstance(_itemObjs[i]);
        _itemObjs.Clear();
    }
}