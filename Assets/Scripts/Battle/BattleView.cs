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
    void Enter(PlayerData playerData,Dictionary<int, EnemyData> enemiesData);
    IEnumerator EnemyAttack(int enemyPos, int targetHeroPos);
    void ActiveHeroElement();
    void PopupResultPage(bool win, Dictionary<string, HeroLevelExpData> levelExpDatas,List<Item> items);
    void SetBattleEndFlag();

    event Action<int> HeroEndTurn;
    event Action<Item, int> UseItem;
    event Action<Skill, int, int> UseSkill;
    event Action OnSettlementBattle;
    event Action LeaveBattle;
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

    [Header("Settlement")]
    [SerializeField] private ResultPopupPanel _resultPopupPanel = null;

    #region Events
    public event Action<int> HeroEndTurn;
    public event Action<Item, int> UseItem;
    public event Action<Skill, int, int> UseSkill;
    public event Action LeaveBattle;
    public event Action OnSettlementBattle;
    #endregion

    private GameData _gameData = null;
    private PlayerData _playerData = null;
    private Dictionary<int, HeroView> _heroViews = new Dictionary<int, HeroView>();
    private Dictionary<int, HeroElement> _heroElements = new Dictionary<int, HeroElement>();
    private Dictionary<string, SkillRow> _skillTable = null;

    private Dictionary<int, EnemyData> _enemiesData = null;
    private Dictionary<int, EnemyView> _enemyViews = new Dictionary<int, EnemyView>();

    private List<ItemView> _itemViews = new List<ItemView>();
    private List<SkillView> _skillViews = new List<SkillView>();

    private IInfoView _infoView = null;

    private bool _battleEnd = false;

    private int _curHeroIdx = -1;
    private int _targetHeroIdx = -1;
    private int _selectTargetHeroIdx = -1;

    private int _curEnemyIdx = -1;
    
    private int _curSelectItemIdx = -1;
    private int _curSelectSkillId = -1;

    private BattleStatus _curStatus = BattleStatus.None;

    private SerialCoroutines _serialCor = new SerialCoroutines();

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

    public void Enter(PlayerData playerData, Dictionary<int, EnemyData> enemiesData)
    {
        gameObject.SetActive(true);
        _RegisterEvents();

        _playerData = playerData;
        _enemiesData = enemiesData;

        _ShowItemOrSkillListPanel(false);

        _Init();
        _SetFirstEnemySelectArrow();
        _SetTopTipText(BattleOperationType.HeroTurn);
        StartCoroutine(_serialCor.Execute());
    }

    private void _Leave()
    {
        if (LeaveBattle != null)
            LeaveBattle();

        _serialCor.Stop();
        StopAllCoroutines();
    }

    #region Init
    public void Initialize(GameData gameData, IInfoView infoView)
    {
        _gameData = gameData;
        _skillTable = _gameData.SkillTable;
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
        foreach(var pair in _playerData.TeamHeroes)
        {
            _SetHeroView(pair.Key, pair.Value);
            _SetHeroElement(pair.Key, pair.Value);
        }
    }

    private void _InitEnemies()
    {
        foreach(var pair in _enemiesData)
        {
            _SetEnemyView(pair.Key,pair.Value);
        }
    }
    #endregion

    #region Hero Elements
    private void _SetHeroElement(int pos,HeroData data)
    {
        var element = _heroMemberElementPool.GetInstance();
        element.SetActive(true);
        element.transform.SetParent(_elementRoot);
        var elementView = element.GetComponent<HeroElement>();
        _heroElements.Add(pos, elementView);
        elementView.SelectHeroElement -= _SelectHeroElement;
        elementView.SelectHeroElement += _SelectHeroElement;
        elementView.SetData(data, pos, _heroElementToggleGroup,_gameData.LevelExpTable);
        elementView.LockToggle(!data.IsAlive);
    }

    private void _SelectHeroElement(bool select, int pos)
    {
        if (_heroElements.ContainsKey(pos))
            _heroElements[pos].ShowIndicator(select);

        if (select)
            _curHeroIdx = pos;
        else if (_curHeroIdx == pos)
            _curHeroIdx = -1;

        _ShowHeroSelections(select);
    }

    private void _ShowHeroSelections(bool isShow)
    {
        if(!isShow)
            _ShowItemOrSkillListPanel(false);

        _heroSelectionView.Show(isShow);
        _infoView.EndShow();
        var arrowRoot = isShow ? _heroViews[_curHeroIdx].BackLocate : null;
        _ShowArrow(ArrowType.HeroSelectArrow, arrowRoot);
    }

    public void ActiveHeroElement()
    {
        foreach (var pair in _playerData.TeamHeroes)
            _heroElements[pair.Key].LockToggle(!pair.Value.IsAlive);
        _SetFirstEnemySelectArrow();
        _SetTopTipText(BattleOperationType.HeroTurn);
    }

    private void _FreshAllHeroViews()
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
        if (_curHeroIdx == -1)
            return;

        _serialCor.Add(_CurHeroEndTurn(_curHeroIdx));
    }

    private IEnumerator _CurHeroEndTurn(int heroIdx)
    {
        _SetTopTipText(BattleOperationType.HeroTurn);
        if (HeroEndTurn != null)
            HeroEndTurn(heroIdx);
        var heroElement = _heroElements[heroIdx];
        if(_curHeroIdx == heroIdx)
            heroElement.SelectElement(false);
        heroElement.LockToggle(true);
        yield return null;
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

            var items = _playerData.BackPack.Values.OrderBy(item => item.Pos).ToList();
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

        if (!_playerData.BackPack.ContainsKey(pos))
            itemView.SetData(null);
        else
            itemView.SetData(_playerData.BackPack[pos]);
    }

    private void _ListSkills(bool isShow)
    {
        _ClearItemAndSkillObjs();
        _ShowItemOrSkillListPanel(isShow);

        if (isShow)
        {
            _SetTopTipText(BattleOperationType.SelectSkill);

            var skills = _GetSkillsByID(_playerData.TeamHeroes[_curHeroIdx].SkillList).Values.OrderBy(skill => int.Parse(skill.ID)).ToList();
            if (skills == null || skills.Count == 0)
                return;

            var curHero = _playerData.TeamHeroes[_curHeroIdx];
            for (int i = 0; i < skills.Count; i++)
            {
                var skill = skills[i];

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
        var curHero = _playerData.TeamHeroes[_curHeroIdx];
        var skills = _GetSkillsByID(curHero.SkillList);
        foreach (var view in _skillViews)
            view.SetSkillViewUseAble(skills[view.ID].MpCost <= curHero.CurrentMp);
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

    private Dictionary<string,Skill> _GetSkillsByID(List<string> skillIds)
    {
        Dictionary<string, Skill> result = new Dictionary<string, Skill>();
        for(int i = 0;i<skillIds.Count;i++)
        {
            var skillRow = _skillTable[skillIds[i]];
            var skill = new Skill(skillRow);
            if (skill != null)
                result.Add(skill.ID, skill);
        }
        return result;
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

        _infoView.Show(item.Name, "数量:" + item.Count, item.Desc, item);

        _infoView.HideArrowAndSelectImage -= _HideHeroTargetArrowAndItemSelectImage;
        _infoView.OnUseAction -= _UseItem;
        _infoView.HideArrowAndSelectImage += _HideHeroTargetArrowAndItemSelectImage;
        _infoView.OnUseAction += _UseItem;

        _SetTopTipText(BattleOperationType.SelectUseTarget);
        _SelectTargetHero(_playerData.TeamHeroes.Keys.First());
    }

    private void _UseItem(IUseData data)
    {
        var item = data as Item;

        if (item == null)
            return;

        _serialCor.Add(_UseItem(item));
    }

    private IEnumerator _UseItem(Item item)
    {
        int toIdx = _selectTargetHeroIdx;

        if (UseItem != null)
            UseItem(item, toIdx);

        _FreshAllHeroViews();
        _FreshAllHeroElements();
        _FreshItemView(item.Pos);
        yield return null;
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

        var useAble = skill.MpCost <= _playerData.TeamHeroes[_curHeroIdx].CurrentMp;

        _infoView.HideArrowAndSelectImage -= _HideHeroTargetArrowAndSkillSelectImage;
        _infoView.OnUseAction -= _UseSkill;
        _infoView.HideArrowAndSelectImage += _HideHeroTargetArrowAndSkillSelectImage;
        _infoView.OnUseAction += _UseSkill;

        _infoView.Show(skill.Name, "消耗MP:" + skill.MpCost, skill.Desc, skill, useAble);
        
        _SetTopTipText(BattleOperationType.SelectAttackTarget);
    }

    private void _UseSkill(IUseData data)
    {
        if (_curHeroIdx == -1)
            return;

        Skill skill = data as Skill;
        if (data != null && skill == null)
            return;

        int fromIdx = _curHeroIdx;
        int toIdx = skill != null && skill.EffectiveResult == EffectiveResult.Restore ? _selectTargetHeroIdx : _curEnemyIdx;

        if (UseSkill != null)
            UseSkill(skill, fromIdx, toIdx);

        _SetTopTipText(BattleOperationType.HeroTurn);
        _ShowHeroSelections(false);
        _heroElements[_curHeroIdx].ShowIndicator(false);
        _heroElements[_curHeroIdx].LockToggle(true);

        _serialCor.Add(_UseSkill(skill, fromIdx, toIdx));
    }

    private IEnumerator _UseSkill(Skill skill,int fromIdx,int toIdx)
    {
        if (_battleEnd)
            yield break;

        if(toIdx == -1)
            toIdx = _enemiesData.First().Key;

        if (!_enemiesData[toIdx].IsAlive)
            _SetFirstEnemySelectArrow();

        var heroView = _heroViews[fromIdx];
        var targetEnemyView = _enemyViews[toIdx];
        
        yield return _PlaySkillAni(skill, heroView, targetEnemyView);

        if (skill != null)
            _FreshAllSkillView();

        yield return _CurHeroEndTurn(fromIdx);

        if (_battleEnd && OnSettlementBattle != null)
            OnSettlementBattle();
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

    private IEnumerator _PlaySkillAni(Skill skill,CharacterView from, CharacterView target)
    {
        var isRemote = skill != null && skill.IsRemote;

        yield return from.AttackAni(target.FrontLocate.position, isRemote, skill != null);

        if(skill == null || skill.EffectiveWay == EffectiveWay.Direct)
            target.BeHit();

        _FreshAllHeroViews();
        _FreshAllHeroElements();

        yield return _skillEffectManager.PlaySkillEffect(skill, from.SkillEffectPos, target.SkillEffectPos);

        if(!isRemote)
        {
            yield return MyCoroutine.Sleep(0.1f);
            yield return from.BackToOriPosition();
        }
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

        _selectTargetHeroIdx = pos;
        _ShowArrow(ArrowType.HeroTargetArrow, _heroViews[pos].TopLocate);
    }

    private void _SetHeroView(int pos,HeroData data)
    {
        var instance = _heroMemberPool.GetInstance(data.Job);
        _heroGroupView.LocateToPos(instance.transform, pos);
        var heroView = instance.GetComponentInChildren<HeroView>();
        heroView.SelectAction -= _SelectTargetHero;
        heroView.SelectAction += _SelectTargetHero;
        heroView.SetData(data, pos);
        _heroViews.Add(pos, heroView);
    }

    private void _FreshAllHeroElements()
    {
        foreach (var pair in _heroElements)
            pair.Value.FreshHeroElementStatus();
    }

    private void _SetEnemyView(int pos,EnemyData enemy)
    {
        var instance = _enemyMemberPool.GetInstance(enemy.Type);
        _enemyMemberGroupView.LocateToPos(instance.transform, pos);
        var enemyView = instance.GetComponentInChildren<EnemyView>();
        enemyView.SelectAction -= _SelectTargetEnemy;
        enemyView.SelectAction += _SelectTargetEnemy;
        enemyView.SetData(enemy,pos);
        _enemyViews.Add(pos, enemyView);
    }

    private void _SelectTargetEnemy(int pos)
    {
        _curEnemyIdx = pos;
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

        yield return _PlaySkillAni(null, enemyView, targetHeroView);
    }
    #endregion

    #region Settlement
    public void PopupResultPage(bool win, Dictionary<string, HeroLevelExpData> levelExpDatas, List<Item> items)
    {
        _resultPopupPanel.OnComfirmAction -= _Leave;
        _resultPopupPanel.OnComfirmAction += _Leave;
        _resultPopupPanel.Show(win, _playerData.Heroes, levelExpDatas, items);
    }

    public void SetBattleEndFlag()
    {
        _battleEnd = true;
    }
    #endregion
}