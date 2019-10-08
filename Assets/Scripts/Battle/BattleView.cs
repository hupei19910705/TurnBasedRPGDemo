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
    void ActiveHeroElement();
    void SetTargetEnemy(int pos);

    event Action<bool, int> SelectHero;
    event Action<int> SelectEnemy;
    event Action<int> HeroEndTurn;
    event Action<double> EnemyBeHit;
    event Action<double> HeroBeHit;
    event Action<int> SetTargetHero;
    event Action<Item> UseItem;
    event Action<Skill> UseSkill;
}

public enum BattleOperationType
{
    SelectAttackTarget,
    SelectUseTarget,
    SelectHeroOperation,
    SelectSkill,
    SelectItem,
    HeroTurn,
    EnemyTurn
}

public enum BattleStatus
{
    None,
    UseItem,
    UseSkill
}

public class BattleView : MonoBehaviour, IBattleView
{
    [Header("Team Member")]
    [SerializeField] private MemberGroupView _heroGroupView = null;
    [SerializeField] private HeroObjectPool _heroMemberPool = null;
    [SerializeField] private GeneralObjectPool _heroMemberElementPool = null;
    [SerializeField] private Transform _elementRoot = null;
    [SerializeField] private ToggleGroup _heroElementToggleGroup = null;
    [SerializeField] private HeroSelectionView _heroSelectionView = null;
    [SerializeField] private Transform _heroSelectArrow = null;
    [SerializeField] private Transform _heroTargetArrow = null;

    [Header("Enemy Member")]
    [SerializeField] private MemberGroupView _enemyMemberGroupView = null;
    [SerializeField] private EnemyObjectPool _enemyMemberPool = null;
    [SerializeField] private Transform _enemySelectArrow = null;
    [SerializeField] private Transform _enemyTargetArrow = null;

    [Header("Other")]
    [SerializeField] private Transform _defaultArrowRoot = null;
    [SerializeField] private ScrollRect _ItemsOrSkillsScroll = null;
    [SerializeField] private GeneralObjectPool _itemPool = null;
    [SerializeField] private GeneralObjectPool _skillPool = null;
    [SerializeField] private SkillEffectManager _skillEffectManager = null;
    [SerializeField] private Text _topText = null;

    public event Action<bool,int> SelectHero;
    public event Action<int> SelectEnemy;
    public event Action<int> HeroEndTurn;
    public event Action<double> EnemyBeHit;
    public event Action<double> HeroBeHit;
    public event Action<int> SetTargetHero;
    public event Action<Item> UseItem;
    public event Action<Skill> UseSkill;

    private TeamData _teamData = null;
    private Dictionary<int, HeroView> _heroViews = new Dictionary<int, HeroView>();
    private Dictionary<int, HeroElement> _heroElements = new Dictionary<int, HeroElement>();

    private Dictionary<int, EnemyData> _enemiesData = null;
    private Dictionary<int, EnemyView> _enemyViews = new Dictionary<int, EnemyView>();

    private List<ItemView> _itemViews = new List<ItemView>();
    private List<SkillView> _skillViews = new List<SkillView>();

    private IInfoView _infoView = null;

    private bool _operable = true;

    private int _curHeroIdx = -1;
    private int _targetHeroIdx = -1;
    private int _selectTargetHeroIdx = -1;

    private int _curEnemyIdx = -1;
    
    private int _curSelectItemIdx = -1;
    private int _curSelectSkillId = -1;

    private BattleStatus _curStatus = BattleStatus.None;

    private ParallelCoroutines _parallelCor = new ParallelCoroutines();

    private static Dictionary<BattleOperationType, string> _operationTips = new Dictionary<BattleOperationType, string>
    {
        { BattleOperationType.SelectAttackTarget,"选择技能使用对象"},
        { BattleOperationType.SelectUseTarget,"选择物品使用对象"},
        { BattleOperationType.SelectHeroOperation,"选择操作类型"},
        { BattleOperationType.SelectSkill,"选择技能"},
        { BattleOperationType.SelectItem,"选择物品"},
        { BattleOperationType.HeroTurn,"己方回合"},
        { BattleOperationType.EnemyTurn,"敌方回合"}
    };

    public void Initialize(IInfoView infoView)
    {
        _infoView = infoView;
    }

    public void Enter(TeamData teamData, Dictionary<int, EnemyData> enemiesData)
    {
        gameObject.SetActive(true);
        _RegisterEvents();

        _teamData = teamData;
        _enemiesData = enemiesData;

        _ShowItemOrSkillListPanel(false);

        _Init();
        _SetFirstEnemyArrow();
        _SetTopTipText(BattleOperationType.HeroTurn);
        StartCoroutine(_parallelCor.Execute());
    }

    private void _Init()
    {
        _heroMemberPool.InitPool();
        _heroMemberElementPool.InitPool();
        _enemyMemberPool.InitPool();
        _itemPool.InitPool(10);
        _skillPool.InitPool(10);

        _heroSelectionView.Init();
        _skillEffectManager.Init();

        _InitHeroes();
        _InitEnemies();
    }

    private void _RegisterEvents()
    {
        _UnRegisterEvents();
        _heroSelectionView.OnGeneralAttack += _HeroGeneralAttack;
        _heroSelectionView.ListSkills += _ListSkills;
        _heroSelectionView.ListItems += _ListItems;
        _heroSelectionView.OnSkip += _CurHeroEndTurn;
    }

    private void _UnRegisterEvents()
    {
        _heroSelectionView.OnGeneralAttack -= _HeroGeneralAttack;
        _heroSelectionView.ListSkills -= _ListSkills;
        _heroSelectionView.ListItems -= _ListItems;
        _heroSelectionView.OnSkip -= _CurHeroEndTurn;
    }

    private void _InitHeroes()
    {
        foreach (var member in _teamData.Heroes.Values.ToList())
        {
            _SetHeroView(member);
            _SetHeroElement(member);
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
        _enemySelectArrow.gameObject.SetActive(true);
        _SelectEnemy(_curEnemyIdx);
    }

    private void _SetHeroView(HeroData data)
    {
        var instance = _heroMemberPool.GetInstance(data.Job);
        _heroGroupView.LocateToPos(instance.transform, data.Pos);
        var heroView = instance.GetComponentInChildren<HeroView>();
        heroView.HeroDamageEffect -= _HeroDamageEffect;
        heroView.HeroEndTurn -= HeroEndTurn;
        heroView.SetTargetHero -= _SetTargetHero;
        heroView.HeroDamageEffect += _HeroDamageEffect;
        heroView.HeroEndTurn += HeroEndTurn;
        heroView.SetTargetHero += _SetTargetHero;
        heroView.SetData(data);
        _heroViews.Add(data.Pos, heroView);
    }

    private void _SetHeroElement(HeroData data)
    {
        var element = _heroMemberElementPool.GetInstance();
        element.SetActive(true);
        element.transform.SetParent(_elementRoot);
        var elementView = element.GetComponent<HeroElement>();
        elementView.SelectHero -= _SelectHero;
        elementView.SelectHero += _SelectHero;
        elementView.SetData(data, _heroElementToggleGroup);
        _heroElements.Add(data.Pos, elementView);
        elementView.LockToggle(!data.IsAlive);
    }

    private void _SelectHero(bool select,int pos = -1)
    {
        _curHeroIdx = select ? pos : -1;
        _heroSelectionView.Show(select);
        _infoView.EndShow();
        if (SelectHero != null)
            SelectHero(select, pos);
        _heroSelectArrow.gameObject.SetActive(select);
        if (select)
        {
            _heroSelectArrow.SetParent(_heroViews[pos].LeftLocate);
            _heroSelectArrow.localPosition = Vector3.zero;
        }
        else
        {
            _ShowItemOrSkillListPanel(false);
            _heroSelectArrow.SetParent(_defaultArrowRoot);
        }
    }

    private void _SelectEnemy(int pos)
    {
        _curEnemyIdx = pos;
        if (SelectEnemy != null)
            SelectEnemy(pos);
        _enemySelectArrow.gameObject.SetActive(true);
        _enemySelectArrow.SetParent(_enemyViews[_curEnemyIdx].FrontLocate);
        _enemySelectArrow.localPosition = Vector3.zero;
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

    private void _HeroGeneralAttack()
    {
        if (!_operable)
            return;
        _operable = false;
        _SetTopTipText(BattleOperationType.HeroTurn);
        if (_curEnemyIdx == -1)
            _curEnemyIdx = _enemiesData.First().Key;

        var heroElement = _heroElements[_curHeroIdx];
        heroElement.SelectElement(false);
        heroElement.LockToggle(true);
        _parallelCor.Add(_HeroGeneralAtk());
    }

    private IEnumerator _HeroGeneralAtk()
    {
        yield return _heroViews[_curHeroIdx].GeneralAttack(_enemyViews[_curEnemyIdx].FrontLocate.position);
        _operable = true;
    }

    private void _HeroDamageEffect(double attack)
    {
        if (EnemyBeHit != null)
            EnemyBeHit(attack);

        _parallelCor.Add(_PlayHeroSkill(null));

        if (!_enemiesData[_curEnemyIdx].IsAlive)
            _SetFirstEnemyArrow();
    }

    private void _CurHeroEndTurn()
    {
        if (!_operable)
            return;
        _operable = false;
        _SetTopTipText(BattleOperationType.HeroTurn);
        if (HeroEndTurn != null)
            HeroEndTurn(_curHeroIdx);
        var heroElement = _heroElements[_curHeroIdx];
        heroElement.SelectElement(false);
        heroElement.LockToggle(true);
        _operable = true;
    }

    public void ActiveHeroElement()
    {
        foreach(var hero in _teamData.Heroes.Values)
            _heroElements[hero.Pos].LockToggle(!hero.IsAlive);
        _SetFirstEnemyArrow();
        _SetTopTipText(BattleOperationType.HeroTurn);
    }

    public IEnumerator RunEnemyTurn(int enemyPos,int targetMemberPos)
    {
        _SetTopTipText(BattleOperationType.EnemyTurn);

        _heroSelectArrow.gameObject.SetActive(false);
        _heroSelectArrow.SetParent(_defaultArrowRoot);
        _enemySelectArrow.gameObject.SetActive(false);
        _enemySelectArrow.SetParent(_defaultArrowRoot);

        _targetHeroIdx = targetMemberPos;
        var enemyView = _enemyViews[enemyPos];
        yield return enemyView.GeneralAttack(_heroViews[_targetHeroIdx].FrontLocate.position);
    }

    private void _EnemyDamageEffect(double attack)
    {
        if (HeroBeHit != null)
            HeroBeHit(attack);
        _heroElements[_targetHeroIdx].FreshHeroElementStatus();

        _parallelCor.Add(_PlayEnemySkill(null));
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
            _SetTopTipText(BattleOperationType.SelectSkill);

            var skills = _teamData.Heroes[_curHeroIdx].Skills.Values.OrderBy(skill => int.Parse(skill.ID)).ToList();
            if (skills == null || skills.Count == 0)
                return;

            var curHero = _teamData.Heroes[_curHeroIdx];
            for(int i=0;i<skills.Count;i++)
            {
                var skill = skills[i];
                if (skill.Type == SkillType.GeneralAttack)
                    continue;

                var instance = _skillPool.GetInstance();
                instance.transform.SetParent(_ItemsOrSkillsScroll.content);
                var view = instance.GetComponent<SkillView>();
                view.SetData(skill, curHero.CurrentMp >= skill.MpCost);
                view.ClickAction -= () => _ClickSkillView(skill);
                view.ClickAction += () => _ClickSkillView(skill);
                _skillViews.Add(view);
            }
        }
    }

    private void _ListItems(bool isShow)
    {
        _ClearItemAndSkillObjs();
        _ShowItemOrSkillListPanel(isShow);

        if (isShow)
        {
            _SetTopTipText(BattleOperationType.SelectItem);

            var items = _teamData.BackPack.Values.OrderBy(item => item.Pos).ToList();
            if (items == null || items.Count == 0)
                return;

            for(int i= 0;i<items.Count;i++)
            {
                var item = items[i];
                var instance = _itemPool.GetInstance();
                instance.transform.SetParent(_ItemsOrSkillsScroll.content);
                var view = instance.GetComponent<ItemView>();
                view.SetData(item);
                view.ClickAction -= () => _ClickItemView(item);
                view.ClickAction += () => _ClickItemView(item);
                _itemViews.Add(view);
            }
        }
    }

    private void _ClearItemAndSkillObjs()
    {
        for(int i= 0;i< _skillViews.Count;i++)
            _skillPool.ReturnInstance(_skillViews[i].gameObject);
        _skillViews.Clear();

        for (int i = 0; i < _itemViews.Count; i++)
            _itemPool.ReturnInstance(_itemViews[i].gameObject);
        _itemViews.Clear();
    }

    private void _SetTopTipText(BattleOperationType type)
    {
        _topText.text = _operationTips[type];
    }

    #region UseItem
    private void _ClickItemView(Item item)
    {
        //先关闭上一个信息界面相关的箭头和选中效果
        _curSelectItemIdx = _infoView.Flag;
        _HideArrowAndSelectImage();
        //再设置此次信息界面
        _curStatus = BattleStatus.UseItem;
        _curSelectItemIdx = item.Pos;

        _infoView.Show(item.Name,"数量:"+ item.Count, item.Desc,item.Pos, () => _UseItem(item));
        _infoView.HideArrowAndSelectImage -= _HideArrowAndSelectImage;
        _infoView.HideArrowAndSelectImage += _HideArrowAndSelectImage;

        _SetTopTipText(BattleOperationType.SelectUseTarget);
        _SetTargetHero(_teamData.Heroes.Keys.First());
    }

    private void _UseItem(Item item)
    {
        if (UseItem != null)
            UseItem(item);
        _FreshAllHeroStatus();
        _FreshItemView(item.Pos);
    }

    private void _SetTargetHero(int pos)
    {
        if (_curStatus != BattleStatus.UseItem)
            return;

        if (SetTargetHero != null)
            SetTargetHero(pos);

        _selectTargetHeroIdx = pos;
        _ShowHeroTargetArrow(true, pos);
    }

    private void _ShowHeroTargetArrow(bool isShow,int pos = -1)
    {
        _heroTargetArrow.gameObject.SetActive(isShow);
        if (isShow)
            _heroTargetArrow.SetParent(_heroViews[pos].TopLocate);
        else
            _heroTargetArrow.SetParent(_defaultArrowRoot);
        _heroTargetArrow.localPosition = Vector3.zero;
    }

    private void _HideArrowAndSelectImage()
    {
        _ShowHeroTargetArrow(false);

        var itemView = _GetItemView(_curSelectItemIdx);
        if(itemView != null)
            itemView.HideSelectImage();

        var skillView = _GetSkillView(_curSelectSkillId);
        if (skillView != null)
            skillView.HideSelectImage();

        _curStatus = BattleStatus.None;
    }

    private void _FreshAllHeroStatus()
    {
        foreach(var pair in _heroViews)
        {
            pair.Value.ChangeHpSliderValue();
            pair.Value.ChangeMpSliderValue();
        }

        foreach(var pair in _heroElements)
            pair.Value.FreshHeroElementStatus();
    }

    private void _FreshItemView(int pos)
    {
        var itemView = _GetItemView(pos);
        if (itemView == null)
            return;

        if (!_teamData.BackPack.ContainsKey(pos))
            itemView.SetData(null);
        else
            itemView.SetData(_teamData.BackPack[pos]);
    }

    private ItemView _GetItemView(int pos)
    {
        return _itemViews.Find(view => view.Pos == pos);
    }
    #endregion

    #region UseSkill
    private void _ClickSkillView(Skill skill)
    {
        //先关闭上一个信息界面相关的箭头和选中效果
        _curSelectSkillId = _infoView.Flag;
        _HideArrowAndSelectImage();
        //再设置此次信息界面
        _curStatus = BattleStatus.UseSkill;
        _curSelectSkillId = int.Parse(skill.ID);

        var useAble = skill.MpCost <= _teamData.Heroes[_curHeroIdx].CurrentMp;

        _infoView.Show(skill.Name, "消耗MP:"+skill.MpCost, skill.Desc, int.Parse(skill.ID), () => _UseSkill(skill), useAble);
        _infoView.HideArrowAndSelectImage -= _HideArrowAndSelectImage;
        _infoView.HideArrowAndSelectImage += _HideArrowAndSelectImage;

        _SetTopTipText(BattleOperationType.SelectAttackTarget);
    }

    private SkillView _GetSkillView(int id)
    {
        return _skillViews.Find(view => string.Equals(view.ID,id.ToString()));
    }

    private void _UseSkill(Skill skill)
    {
        if (!_operable)
            return;

        _operable = false;
        if (UseSkill != null)
            UseSkill(skill);

        _parallelCor.Add(_PlayHeroSkill(skill));

        if (!_enemiesData[_curEnemyIdx].IsAlive)
            _SetFirstEnemyArrow();

        _FreshAllHeroStatus();
        _FreshAllSkillView();
        _CurHeroEndTurn();
    }

    private void _FreshAllSkillView()
    {
        var curHero = _teamData.Heroes[_curHeroIdx];
        foreach(var view in _skillViews)
            view.SetSkillViewUseAble(curHero.Skills[view.ID].MpCost <= curHero.CurrentMp);
    }

    private IEnumerator _PlayHeroSkill(Skill skill)
    {
        var enemyView = _enemyViews[_curEnemyIdx];
        yield return _skillEffectManager.PlaySkillEffect(skill, _heroViews[_curHeroIdx].SkillEffectPos, enemyView.SkillEffectPos,enemyView.BeHit);
        _operable = true;
    }

    private IEnumerator _PlayEnemySkill(Skill skill)
    {
        var heroView = _heroViews[_targetHeroIdx];
        yield return _skillEffectManager.PlaySkillEffect(skill, _enemyViews[_curEnemyIdx].SkillEffectPos, heroView.SkillEffectPos, heroView.BeHit);
        _operable = true;
    }

    public void SetTargetEnemy(int pos)
    {
        _enemyTargetArrow.gameObject.SetActive(true);
        _enemyTargetArrow.SetParent(_enemyViews[pos].TopLocate);
        _enemyTargetArrow.localPosition = Vector3.zero;
    }
    #endregion
}