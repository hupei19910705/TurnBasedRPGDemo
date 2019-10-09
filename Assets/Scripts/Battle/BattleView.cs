using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utility.GameUtility;
using Utility;
using UnityEngine.Events;

public interface IBattleView
{
    void Enter(TeamData teamData,Dictionary<int, EnemyData> enemiesData);
    IEnumerator EnemyAttack(int enemyPos, int targetHeroPos);
    void ActiveHeroElement();

    event Action<bool, int> ChangeSelectHero;
    event Action<int> ChangeSelectEnemy;
    event Action<int> HeroEndTurn;
    event Action<int> ChangeTargetHero;
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
    [Header("Hero")]
    [SerializeField] private MemberGroupView _heroGroupView = null;
    [SerializeField] private HeroObjectPool _heroMemberPool = null;
    [SerializeField] private GeneralObjectPool _heroMemberElementPool = null;
    [SerializeField] private Transform _elementRoot = null;
    [SerializeField] private ToggleGroup _heroElementToggleGroup = null;
    [SerializeField] private HeroSelectionView _heroSelectionView = null;
    [SerializeField] private Transform _heroSelectArrow = null;
    [SerializeField] private Transform _heroTargetArrow = null;

    [Header("Enemy")]
    [SerializeField] private MemberGroupView _enemyMemberGroupView = null;
    [SerializeField] private EnemyObjectPool _enemyMemberPool = null;
    [SerializeField] private Transform _enemySelectArrow = null;

    [Header("Other")]
    [SerializeField] private Transform _defaultArrowRoot = null;
    [SerializeField] private ScrollRect _ItemsOrSkillsScroll = null;
    [SerializeField] private GeneralObjectPool _itemPool = null;
    [SerializeField] private GeneralObjectPool _skillPool = null;
    [SerializeField] private SkillEffectManager _skillEffectManager = null;
    [SerializeField] private Text _topText = null;

    #region Events
    public event Action<bool,int> ChangeSelectHero;
    public event Action<int> ChangeSelectEnemy;
    public event Action<int> HeroEndTurn;
    public event Action<int> ChangeTargetHero;
    public event Action<Item> UseItem;
    public event Action<Skill> UseSkill;
    #endregion

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

    private enum ListViewType
    {
        Item,
        Skill
    }

    private enum ArrowType
    {
        HeroSelectArrow,
        HeroTargetArrow,
        EnemySelectArrow
    }

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

    public void Enter(TeamData teamData, Dictionary<int, EnemyData> enemiesData)
    {
        gameObject.SetActive(true);
        _RegisterEvents();

        _teamData = teamData;
        _enemiesData = enemiesData;

        _ShowItemOrSkillListPanel(false);

        _Init();
        _SetFirstEnemySelectArrow();
        _SetTopTipText(BattleOperationType.HeroTurn);
        StartCoroutine(_parallelCor.Execute());
    }

    #region Init
    public void Initialize(IInfoView infoView)
    {
        _infoView = infoView;
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
        _heroSelectionView.OnSkip += _Skip;
    }

    private void _UnRegisterEvents()
    {
        _heroSelectionView.OnGeneralAttack -= _HeroGeneralAttack;
        _heroSelectionView.ListSkills -= _ListSkills;
        _heroSelectionView.ListItems -= _ListItems;
        _heroSelectionView.OnSkip -= _Skip;
    }

    private void _InitHeroes()
    {
        foreach (var hero in _teamData.Heroes.Values.ToList())
        {
            _SetHeroView(hero);
            _SetHeroElement(hero);
        }
    }

    private void _InitEnemies()
    {
        foreach (var enemy in _enemiesData.Values.ToList())
        {
            _SetEnemyView(enemy);
        }
    }
    #endregion

    #region Hero Elements
    private void _SetHeroElement(HeroData data)
    {
        var element = _heroMemberElementPool.GetInstance();
        element.SetActive(true);
        element.transform.SetParent(_elementRoot);
        var elementView = element.GetComponent<HeroElement>();
        elementView.SelectHeroElement -= _SelectHeroElement;
        elementView.SelectHeroElement += _SelectHeroElement;
        elementView.SetData(data, _heroElementToggleGroup);
        _heroElements.Add(data.Pos, elementView);
        elementView.LockToggle(!data.IsAlive);
    }

    private void _SelectHeroElement(bool select, int pos = -1)
    {
        _curHeroIdx = select ? pos : -1;
        
        if (ChangeSelectHero != null)
            ChangeSelectHero(select, pos);

        if(!select)
            _ShowItemOrSkillListPanel(false);

        _heroSelectionView.Show(select);
        _infoView.EndShow();

        var arrowRoot = select ? _heroViews[pos].BackLocate : null;
        _ShowArrow(ArrowType.HeroSelectArrow, arrowRoot);
    }

    public void ActiveHeroElement()
    {
        foreach (var hero in _teamData.Heroes.Values)
            _heroElements[hero.Pos].LockToggle(!hero.IsAlive);
        _SetFirstEnemySelectArrow();
        _SetTopTipText(BattleOperationType.HeroTurn);
    }

    private void _FreshAllHeroElements()
    {
        foreach (var pair in _heroViews)
        {
            pair.Value.ChangeHpSliderValue();
            pair.Value.ChangeMpSliderValue();
        }
    }
    #endregion

    #region Hero Selections
    private void _HeroGeneralAttack()
    {
        _UseSkill(null);
    }

    private void _Skip()
    {
        if (!_operable)
            return;
        _operable = false;
        _CurHeroEndTurn();
        _operable = true;
    }

    private void _CurHeroEndTurn()
    {
        _SetTopTipText(BattleOperationType.HeroTurn);
        if (HeroEndTurn != null)
            HeroEndTurn(_curHeroIdx);
        var heroElement = _heroElements[_curHeroIdx];
        heroElement.SelectElement(false);
        heroElement.LockToggle(true);
    }
    #endregion

    #region ItemAndSkillList
    private void _ShowItemOrSkillListPanel(bool isShow)
    {
        _ItemsOrSkillsScroll.gameObject.SetActive(isShow);
    }

    private void _ClearItemAndSkillObjs()
    {
        for (int i = 0; i < _skillViews.Count; i++)
            _skillPool.ReturnInstance(_skillViews[i].gameObject);
        _skillViews.Clear();

        for (int i = 0; i < _itemViews.Count; i++)
            _itemPool.ReturnInstance(_itemViews[i].gameObject);
        _itemViews.Clear();
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

            for (int i = 0; i < items.Count; i++)
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

    private void _ListSkills(bool isShow)
    {
        _ClearItemAndSkillObjs();
        _ShowItemOrSkillListPanel(isShow);

        if (isShow)
        {
            _SetTopTipText(BattleOperationType.SelectSkill);

            var skills = _teamData.Heroes[_curHeroIdx].Skills.Values.OrderBy(skill => int.Parse(skill.ID)).ToList();
            if (skills == null || skills.Count == 0)
                return;

            var curHero = _teamData.Heroes[_curHeroIdx];
            for (int i = 0; i < skills.Count; i++)
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

    private void _FreshAllSkillView()
    {
        var curHero = _teamData.Heroes[_curHeroIdx];
        foreach (var view in _skillViews)
            view.SetSkillViewUseAble(curHero.Skills[view.ID].MpCost <= curHero.CurrentMp);
    }

    private void _HideSelectImage(ListViewType type)
    {
        switch(type)
        {
            case ListViewType.Item:
                var itemView = _GetItemView(_curSelectItemIdx);
                if (itemView != null)
                    itemView.HideSelectImage();
                break;
            case ListViewType.Skill:
                var skillView = _GetSkillView(_curSelectSkillId);
                if (skillView != null)
                    skillView.HideSelectImage();
                break;
        }
        _curStatus = BattleStatus.None;
    }
    #endregion

    #region UseItem
    private void _ClickItemView(Item item)
    {
        //先关闭上一个信息界面相关的箭头和选中效果
        _HideHeroTargetArrowAndItemSelectImage();
        //再设置此次信息界面
        _curStatus = BattleStatus.UseItem;
        _curSelectItemIdx = item.Pos;

        _infoView.Show(item.Name, "数量:" + item.Count, item.Desc, () => _UseItem(item));
        _infoView.HideArrowAndSelectImage -= _HideHeroTargetArrowAndItemSelectImage;
        _infoView.HideArrowAndSelectImage += _HideHeroTargetArrowAndItemSelectImage;

        _SetTopTipText(BattleOperationType.SelectUseTarget);
        _SelectTargetHero(_teamData.Heroes.Keys.First());
    }

    private void _UseItem(Item item)
    {
        if (UseItem != null)
            UseItem(item);

        _FreshAllHeroElements();
        _FreshAllHeroViews();
        _FreshItemView(item.Pos);
    }

    private ItemView _GetItemView(int pos)
    {
        return _itemViews.Find(view => view.Pos == pos);
    }

    private void _HideHeroTargetArrowAndItemSelectImage()
    {
        _HideSelectImage(ListViewType.Item);
        _ShowArrow(ArrowType.HeroTargetArrow);
    }
    #endregion

    #region UseSkill
    private void _ClickSkillView(Skill skill)
    {
        //先关闭上一个信息界面相关的箭头和选中效果
        _HideHeroTargetArrowAndSkillSelectImage();
        //再设置此次信息界面
        _curStatus = BattleStatus.UseSkill;
        _curSelectSkillId = int.Parse(skill.ID);

        var useAble = skill.MpCost <= _teamData.Heroes[_curHeroIdx].CurrentMp;

        _infoView.Show(skill.Name, "消耗MP:" + skill.MpCost, skill.Desc, () => _UseSkill(skill), useAble);
        _infoView.HideArrowAndSelectImage -= _HideHeroTargetArrowAndSkillSelectImage;
        _infoView.HideArrowAndSelectImage += _HideHeroTargetArrowAndSkillSelectImage;

        _SetTopTipText(BattleOperationType.SelectAttackTarget);
    }

    private void _UseSkill(Skill skill)
    {
        if (!_operable)
            return;
        _operable = false;

        _SetTopTipText(BattleOperationType.HeroTurn);
        if (_curEnemyIdx == -1)
            _curEnemyIdx = _enemiesData.First().Key;

        if (UseSkill != null)
            UseSkill(skill);

        var heroView = _heroViews[_curHeroIdx];
        var targetEnemyView = _enemyViews[_curEnemyIdx];

        _parallelCor.Add(_PlaySkill(skill, heroView, targetEnemyView,() =>
        {
            if (!_enemiesData[_curEnemyIdx].IsAlive)
                _SetFirstEnemySelectArrow();

            if (skill != null)
                _FreshAllSkillView();

            _CurHeroEndTurn();
            _operable = true;
        }));
    }

    private SkillView _GetSkillView(int id)
    {
        return _skillViews.Find(view => string.Equals(view.ID, id.ToString()));
    }

    private void _HideHeroTargetArrowAndSkillSelectImage()
    {
        _HideSelectImage(ListViewType.Skill);
        _ShowArrow(ArrowType.HeroTargetArrow);
    }

    private IEnumerator _PlaySkill(Skill skill,CharacterView from, CharacterView target,UnityAction callBack = null)
    {
        if (skill != null && skill.IsRemote)
        {
            yield return from.AttackAni(target.FrontLocate.position, true, true);
            yield return _skillEffectManager.PlaySkillEffect(skill, from.SkillEffectPos, target.SkillEffectPos,
            () =>
            {
                target.BeHit();
                _FreshAllHeroElements();
                _FreshAllHeroViews();
            });
        }
        else
        {
            yield return from.AttackAni(target.FrontLocate.position, false, skill != null);
            target.BeHit();
            _FreshAllHeroElements();
            _FreshAllHeroViews();
            yield return MyCoroutine.Sleep(0.1f);
            yield return from.BackToOriPosition();
        }

        if (callBack != null)
            callBack();
    }
    #endregion

    #region Battle Frame
    private void _SetTopTipText(BattleOperationType type)
    {
        _topText.text = _operationTips[type];
    }

    private void _SelectTargetHero(int pos)
    {
        if (_curStatus != BattleStatus.UseItem)
            return;

        if (ChangeTargetHero != null)
            ChangeTargetHero(pos);

        _selectTargetHeroIdx = pos;
        _ShowArrow(ArrowType.HeroTargetArrow, _heroViews[pos].TopLocate);
    }

    private void _SetHeroView(HeroData data)
    {
        var instance = _heroMemberPool.GetInstance(data.Job);
        _heroGroupView.LocateToPos(instance.transform, data.Pos);
        var heroView = instance.GetComponentInChildren<HeroView>();
        heroView.SelectAction -= _SelectTargetHero;
        heroView.SelectAction += _SelectTargetHero;
        heroView.SetData(data);
        _heroViews.Add(data.Pos, heroView);
    }

    private void _FreshAllHeroViews()
    {
        foreach (var pair in _heroElements)
            pair.Value.FreshHeroElementStatus();
    }

    private void _SetEnemyView(EnemyData enemy)
    {
        var instance = _enemyMemberPool.GetInstance(enemy.Type);
        _enemyMemberGroupView.LocateToPos(instance.transform, enemy.Pos);
        var enemyView = instance.GetComponentInChildren<EnemyView>();
        enemyView.SelectAction -= _SelectTargetEnemy;
        enemyView.SelectAction += _SelectTargetEnemy;
        enemyView.SetData(enemy);
        _enemyViews.Add(enemy.Pos, enemyView);
    }

    private void _SelectTargetEnemy(int pos)
    {
        _curEnemyIdx = pos;
        if (ChangeSelectEnemy != null)
            ChangeSelectEnemy(pos);
        _enemySelectArrow.gameObject.SetActive(true);
        _enemySelectArrow.SetParent(_enemyViews[_curEnemyIdx].FrontLocate);
        _enemySelectArrow.localPosition = Vector3.zero;
    }

    private void _SetFirstEnemySelectArrow()
    {
        _curEnemyIdx = -1;
        for (int i = 0; i < 6; i++)
        {
            if (_enemiesData.ContainsKey(i) && _enemiesData[i].IsAlive)
            {
                _curEnemyIdx = i;
                break;
            }
        }
        if (_curEnemyIdx == -1)
            return;
        if (ChangeSelectEnemy != null)
            ChangeSelectEnemy(_curEnemyIdx);
        _SelectTargetEnemy(_curEnemyIdx);
    }

    private void _ShowArrow(ArrowType type,Transform root = null)
    {
        bool isShow = root != null;
        if (root == null)
            root = _defaultArrowRoot;
        Transform arrow = null;

        switch(type)
        {
            case ArrowType.HeroSelectArrow:
                arrow = _heroSelectArrow;
                break;
            case ArrowType.HeroTargetArrow:
                arrow = _heroTargetArrow;
                break;
            case ArrowType.EnemySelectArrow:
                arrow = _enemySelectArrow;
                break;
        }

        if (arrow == null)
            return;

        arrow.SetParent(root);
        arrow.transform.localPosition = Vector3.zero;
        arrow.gameObject.SetActive(isShow);
    }
    #endregion

    #region Enemy Turn
    public IEnumerator EnemyAttack(int enemyPos, int targetHeroPos)
    {
        _SetTopTipText(BattleOperationType.EnemyTurn);

        _ShowArrow(ArrowType.HeroSelectArrow);
        _ShowArrow(ArrowType.EnemySelectArrow);

        _targetHeroIdx = targetHeroPos;
        var enemyView = _enemyViews[enemyPos];
        var targetHeroView = _heroViews[_targetHeroIdx];

        yield return _PlaySkill(null, enemyView, targetHeroView);
    }
    #endregion
}