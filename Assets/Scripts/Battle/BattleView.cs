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
    void SetFightEndFlag();
    void FreshInTheEndOfRound(int round = 1);

    event Action<int> HeroEndTurn;
    event Action<int, int, SelectTargetType> UseItem;
    event Action<Skill, CharacterData, CharacterData, UseTarget> UseSkill;
    event Action CheckIsFightWin;
    event Action LeaveFight;
}

public enum TopTipType
{
    SelectAttackTarget,
    SelectUseTarget,
    SelectHeroOperation,
    SelectSkill,
    SelectItem,
    HeroTurn,
    EnemyTurn
}

public enum SelectTargetType
{
    Hero,
    Enemy
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
    [SerializeField] private Transform _skillEffectRoot = null;
    [SerializeField] private Text _topText = null;

    [Header("Settlement")]
    [SerializeField] private ResultPopupPanel _resultPopupPanel = null;

    #region Events
    public event Action<int> HeroEndTurn;
    public event Action<int, int, SelectTargetType> UseItem;
    public event Action<Skill, CharacterData, CharacterData,UseTarget> UseSkill;
    public event Action LeaveFight;
    public event Action CheckIsFightWin;
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
    private SkillEffectManager _skillEffectManager = null;

    private bool _fightEnd = false;

    private int _curHeroIdx = -1;
    private int _heroTargetIdx = -1;
    private SelectTargetType _heroTargetType;

    private int _curEnemyIdx = -1;
    private int _enemyTargetIdx = -1;
    private SelectTargetType _enemyTargetType;

    private int _curSelectItemIdx = -1;
    private string _curSelectSkillId = string.Empty;

    private IUseData _curUseData;

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

    private static Dictionary<TopTipType, string> _operationTips = new Dictionary<TopTipType, string>
    {
        { TopTipType.SelectAttackTarget,"选择技能使用对象"},
        { TopTipType.SelectUseTarget,"选择物品使用对象"},
        { TopTipType.SelectHeroOperation,"选择操作类型"},
        { TopTipType.SelectSkill,"选择技能"},
        { TopTipType.SelectItem,"选择物品"},
        { TopTipType.HeroTurn,"己方回合"},
        { TopTipType.EnemyTurn,"敌方回合"}
    };

    public void Enter(PlayerData playerData, Dictionary<int, EnemyData> enemiesData)
    {
        gameObject.SetActive(true);
        _RegisterEvents();

        _playerData = playerData;
        _enemiesData = enemiesData;

        _ItemsOrSkillsScroll.gameObject.SetActive(false);

        _Init();
        _SetCurAliveEnemyIdx();
        _SetTopTipText(TopTipType.HeroTurn);
        StartCoroutine(_serialCor.Execute());
    }

    private void _Leave()
    {
        if (LeaveFight != null)
            LeaveFight();

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
        _skillEffectManager = new SkillEffectManager(_skillEffectRoot, _gameData.EffectTable);

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
        for (int i = 0; i < 6; i++)
        {
            var heroes = _playerData.TeamHeroes;
            if (heroes.ContainsKey(i) && heroes[i] != null)
            {
                _SetHeroView(i, heroes[i]);
                _SetHeroElement(i, heroes[i]);
            }
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
        {
            _curHeroIdx = -1;
            _heroTargetIdx = -1;
        }

        _ShowHeroSelections(select);
    }

    private void _ShowHeroSelections(bool isShow)
    {
        _infoView.EndShow();
        _heroSelectionView.Show(isShow);

        _ItemsOrSkillsScroll.gameObject.SetActive(false);
        Transform arrowRoot = null;

        if (isShow)
            arrowRoot = _heroViews[_curHeroIdx].BackLocate;
        else
            _SetTopTipText(TopTipType.HeroTurn);

        _ShowArrow(ArrowType.HeroSelectArrow, arrowRoot);
    }

    public void ActiveHeroElement()
    {
        foreach (var pair in _playerData.TeamHeroes)
            _heroElements[pair.Key].LockToggle(!pair.Value.IsAlive);
        _SetCurAliveEnemyIdx();
        _SetTopTipText(TopTipType.HeroTurn);
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

    private void _ListSkills(bool isShow)
    {
        _ClearItemAndSkillObjs();

        if (isShow)
        {
            _SetTopTipText(TopTipType.SelectSkill);
            _ItemsOrSkillsScroll.gameObject.SetActive(true);

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
        else
        {
            _SetTopTipText(TopTipType.SelectHeroOperation);
            _ItemsOrSkillsScroll.gameObject.SetActive(false);
        }
    }

    private void _ListItems(bool isShow)
    {
        _ClearItemAndSkillObjs();

        if (isShow)
        {
            _SetTopTipText(TopTipType.SelectItem);
            _ItemsOrSkillsScroll.gameObject.SetActive(true);

            var pack = _playerData.BackPack;
            for (int i= 0;i< pack.Length;i++)
            {
                var item = pack[i];
                if (item == null)
                    continue;

                var instance = _itemPool.GetInstance();
                instance.transform.SetParent(_ItemsOrSkillsScroll.content);
                var view = instance.GetComponent<ItemView>();
                view.SetData(item, i);
                view.ClickAction -= _ClickItemView;
                view.ClickAction += _ClickItemView;
                _itemViews.Add(view);
            }
        }
        else
        {
            _SetTopTipText(TopTipType.SelectHeroOperation);
            _ItemsOrSkillsScroll.gameObject.SetActive(false);
        }
    }

    private void _Skip()
    {
        if (_curHeroIdx == -1)
            return;

        _serialCor.Add(_CurHeroEndTurn(_curHeroIdx));
    }

    private IEnumerator _CurHeroEndTurn(int heroIdx)
    {
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
    private void _ClearItemAndSkillObjs()
    {
        for (int i = 0; i < _skillViews.Count; i++)
            _skillPool.ReturnInstance(_skillViews[i].gameObject);
        _skillViews.Clear();

        for (int i = 0; i < _itemViews.Count; i++)
            _itemPool.ReturnInstance(_itemViews[i].gameObject);
        _itemViews.Clear();
    }

    private void _FreshItemView(int pos)
    {
        var itemView = _GetItemView(pos);
        if (itemView == null)
            return;

        var item = _playerData.BackPack[pos];
        itemView.SetData(item, pos);
    }

    private void _FreshAllSkillView(int heroIdx)
    {
        var hero = _playerData.TeamHeroes[heroIdx];
        var skills = _GetSkillsByID(hero.SkillList);
        foreach (var view in _skillViews)
            view.SetSkillViewUseAble(skills[view.ID].MpCost <= hero.CurrentMp);
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
        _curSelectItemIdx = -1;
        _curSelectSkillId = string.Empty;
        _curUseData = null;
    }

    private Dictionary<string,Skill> _GetSkillsByID(List<string> skillIds)
    {
        Dictionary<string, Skill> result = new Dictionary<string, Skill>();
        for(int i = 0;i<skillIds.Count;i++)
        {
            var skill = _GetSkillByID(skillIds[i]);
            if (skill != null)
                result.Add(skill.ID, skill);
        }
        return result;
    }

    private Skill _GetSkillByID(string id)
    {
        var skillRow = _skillTable[id];
        return new Skill(skillRow);
    }
    #endregion

    #region UseItemAndSkill
    private void _UseSelectItemOrSkill()
    {
        if (!string.IsNullOrEmpty(_curSelectSkillId))
        {
            var skill = _GetSkillByID(_curSelectSkillId);
            _UseSkill(skill);
        }
        else if (_curSelectItemIdx != -1)
            _UseItem(_curSelectItemIdx);
    }

    private void _ClickItemView(int pos)
    {
        //先关闭上一个信息界面相关的箭头和选中效果
        _HideHeroTargetArrowAndItemSelectImage();
        //再设置此次信息界面
        if (pos == -1)
            return;

        var item = _playerData.BackPack[pos];
        if (item == null)
            return;

        _curUseData = item;
        _curSelectItemIdx = pos;

        _infoView.HideArrowAndSelectImage -= _HideHeroTargetArrowAndItemSelectImage;
        _infoView.OnUseAction -= _UseSelectItemOrSkill;
        _infoView.HideArrowAndSelectImage += _HideHeroTargetArrowAndItemSelectImage;
        _infoView.OnUseAction += _UseSelectItemOrSkill;

        _infoView.Show(item.Name, "数量:" + item.Count, item.Desc);

        _SetTopTipText(TopTipType.SelectUseTarget);
        if (item.CanUseToSelf)
        {
            int idx = _heroTargetType == SelectTargetType.Hero && _heroTargetIdx != -1 ? _heroTargetIdx : _curHeroIdx;
            _SelectHero(idx);
        }
        else if (item.CanUseToOpposite)
        {
            int idx = _heroTargetType == SelectTargetType.Enemy && _heroTargetIdx != -1 ? _heroTargetIdx : _curEnemyIdx;
            _SelectEnemy(idx);
        }
    }

    private void _UseItem(int pos)
    {
        _serialCor.Add(_UseItem(pos, _heroTargetIdx, _heroTargetType));
    }

    private IEnumerator _UseItem(int pos, int toIdx,SelectTargetType targetType)
    {
        if (_fightEnd || toIdx == -1 || pos == -1 || _playerData.BackPack[pos] == null)
            yield break;

        if (UseItem != null)
            UseItem(pos, toIdx, targetType);

        _FreshAllHeroViews();
        _FreshAllHeroElements();
        _FreshItemView(pos);
        yield return null;
    }

    private ItemView _GetItemView(int pos)
    {
        return _itemViews.Find(view => view.Pos == pos);
    }

    private void _HideHeroTargetArrowAndItemSelectImage()
    {
        _SetTopTipText(TopTipType.SelectItem);
        _HideSelectImage(ListViewType.Item);
        _ShowArrow(ArrowType.HeroTargetArrow);
    }

    private void _ClickSkillView(Skill skill)
    {
        //先关闭上一个信息界面相关的箭头和选中效果
        _HideHeroTargetArrowAndSkillSelectImage();
        //再设置此次信息界面
        _curUseData = skill;
        _curSelectSkillId = skill.ID;

        var useAble = skill.MpCost <= _playerData.TeamHeroes[_curHeroIdx].CurrentMp;

        _infoView.HideArrowAndSelectImage -= _HideHeroTargetArrowAndSkillSelectImage;
        _infoView.OnUseAction -= _UseSelectItemOrSkill;
        _infoView.HideArrowAndSelectImage += _HideHeroTargetArrowAndSkillSelectImage;
        _infoView.OnUseAction += _UseSelectItemOrSkill;

        _infoView.Show(skill.Name, "消耗MP:" + skill.MpCost, skill.Desc, useAble);

        _SetTopTipText(TopTipType.SelectAttackTarget);
        if (skill.CanUseToSelf)
        {
            int idx = _heroTargetType == SelectTargetType.Hero && _heroTargetIdx != -1 ? _heroTargetIdx : _curHeroIdx;
            _SelectHero(idx);
        }
        else if (skill.CanUseToOpposite)
        {
            int idx = _heroTargetType == SelectTargetType.Enemy && _heroTargetIdx != -1 ? _heroTargetIdx : _curEnemyIdx;
            _SelectEnemy(idx);
        }
    }

    private void _UseSkill(Skill skill)
    {
        if (_curHeroIdx == -1)
            return;

        int fromIdx = _curHeroIdx;
        int toIdx = _heroTargetIdx;
        if (skill == null)
        {
            _heroTargetType = SelectTargetType.Enemy;
            toIdx = _curEnemyIdx;
        }

        _ShowHeroSelections(false);
        _heroElements[_curHeroIdx].ShowIndicator(false);
        _heroElements[_curHeroIdx].LockToggle(true);

        _serialCor.Add(_OnUseSkill(skill, fromIdx, toIdx));
    }

    private IEnumerator _OnUseSkill(Skill skill,int fromIdx,int toIdx)
    {
        if (_fightEnd || toIdx == -1)
            yield break;

        var fromView = _heroViews[fromIdx];
        var fromData = _playerData.TeamHeroes[fromIdx];

        toIdx = _CheckTargetIdx(toIdx, _heroTargetType);
        var targetView = _enemyViews[toIdx];
        var targetData = _GetTargetCharacterData(toIdx, _heroTargetType);

        var targetType = _heroTargetType == SelectTargetType.Hero ? UseTarget.SelfSide : UseTarget.OppositeSide;

        if (UseSkill != null)
            UseSkill(skill, fromData, targetData, targetType);

        yield return _PlaySkillAni(skill, fromView, targetView);

        if (skill != null)
            _FreshAllSkillView(fromIdx);

        yield return _CurHeroEndTurn(fromIdx);

        if (CheckIsFightWin != null)
            CheckIsFightWin();
    }

    private SkillView _GetSkillView(string id)
    {
        return _skillViews.Find(view => string.Equals(view.ID, id));
    }

    private void _HideHeroTargetArrowAndSkillSelectImage()
    {
        _SetTopTipText(TopTipType.SelectSkill);
        _HideSelectImage(ListViewType.Skill);
        _ShowArrow(ArrowType.HeroTargetArrow);
    }

    private IEnumerator _PlaySkillAni(Skill skill, CharacterView from, CharacterView target,UseTarget targetType = UseTarget.OppositeSide)
    {
        var isRemote = false;
        List<BuffRow> buffRows = null;
        if (skill != null)
        {
            isRemote = skill.IsRemote;
            buffRows = skill.GetBuffRows(targetType);
        }

        yield return from.AttackAni(target.FrontLocate.position, isRemote, skill != null);

        yield return _skillEffectManager.PlayImmediatelyEffectViews(skill, from.SkillEffectPos, target.SkillEffectPos);

        target.PlayBeHitAni();
        _FreshAllHeroViews();
        _FreshAllEnemyViews();
        _FreshAllHeroElements();
        
        yield return _skillEffectManager.AddBuffEffectViews(buffRows, target.SkillEffectPos);

        if (!isRemote)
        {
            yield return MyCoroutine.Sleep(0.1f);
            yield return from.BackToOriPosition();
        }
    }
    #endregion

    #region Battle Frame
    private void _SetTopTipText(TopTipType type)
    {
        _topText.text = _operationTips[type];
    }

    private void _SelectHero(int pos)
    {
        if (_curUseData == null || !_curUseData.CanUseToSelf)
            return;

        _SelectHeroTarget(pos, SelectTargetType.Hero);
    }

    private void _SelectEnemy(int pos)
    {
        if(_curUseData == null || !_curUseData.CanUseToOpposite)
            _SelectTargetEnemy(pos);
        else
            _SelectHeroTarget(pos, SelectTargetType.Enemy);
    }

    private void _SelectHeroTarget(int pos,SelectTargetType targetType)
    {
        _heroTargetType = targetType;
        _heroTargetIdx = pos;
        switch(targetType)
        {
            case SelectTargetType.Hero:
                _ShowArrow(ArrowType.HeroTargetArrow, _heroViews[pos].TopLocate);
                break;
            case SelectTargetType.Enemy:
                _ShowArrow(ArrowType.HeroTargetArrow, _enemyViews[pos].TopLocate);
                break;
        }
    }

    private void _SetHeroView(int pos,HeroData data)
    {
        var instance = _heroMemberPool.GetInstance(data.Job);
        _heroGroupView.LocateToPos(instance.transform, pos);
        var heroView = instance.GetComponentInChildren<HeroView>();
        heroView.SelectAction -= _SelectHero;
        heroView.SelectAction += _SelectHero;
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
        enemyView.SelectAction -= _SelectEnemy;
        enemyView.SelectAction += _SelectEnemy;
        enemyView.SetData(enemy,pos);
        _enemyViews.Add(pos, enemyView);
    }

    private void _SelectTargetEnemy(int pos)
    {
        _curEnemyIdx = pos;
        Transform locate = null;
        if (_curEnemyIdx != -1)
            locate = _enemyViews[_curEnemyIdx].FrontLocate;
        _ShowArrow(ArrowType.EnemySelectArrow, locate);
    }

    private int _CheckTargetIdx(int pos,SelectTargetType targetType)
    {
        switch (targetType)
        {
            case SelectTargetType.Hero:
                if (!_playerData.TeamHeroes[pos].IsAlive)
                    pos = _GetAliveEnemyIdx();
                break;
            case SelectTargetType.Enemy:
                if (!_enemiesData[pos].IsAlive)
                    pos = _GetAliveEnemyIdx();
                break;
        }

        return pos;
    }

    private CharacterData _GetTargetCharacterData(int pos, SelectTargetType targetType)
    {
        switch(targetType)
        {
            case SelectTargetType.Hero:
                return _playerData.TeamHeroes[pos];
            case SelectTargetType.Enemy:
                return _enemiesData[pos];
        }

        return null;
    }

    private int _GetAliveHeroIdx()
    {
        for (int i = 0; i < 6; i++)
        {
            if (_playerData.TeamHeroes.ContainsKey(i) && _playerData.TeamHeroes[i].IsAlive)
                return i;
        }
        return -1;
    }

    private int _GetAliveEnemyIdx()
    {
        for (int i = 0; i < 6; i++)
        {
            if (_enemiesData.ContainsKey(i) && _enemiesData[i].IsAlive)
                return i;
        }
        return -1;
    }

    private void _SetCurAliveEnemyIdx()
    {
        if(_curEnemyIdx == -1 || !_enemiesData[_curEnemyIdx].IsAlive)
            _curEnemyIdx = _GetAliveEnemyIdx();

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

    public void FreshInTheEndOfRound(int round = 1)
    {
        _FreshAllHeroViews();
        _FreshAllHeroElements();
        _FreshAllEnemyViews();
        _skillEffectManager.EndRound(round);
    }
    #endregion

    #region Enemy Turn
    public IEnumerator EnemyAttack(int enemyPos, int targetHeroPos)
    {
        _SetTopTipText(TopTipType.EnemyTurn);

        _ShowArrow(ArrowType.HeroSelectArrow);
        _ShowArrow(ArrowType.EnemySelectArrow);

        _enemyTargetIdx = targetHeroPos;
        var enemyView = _enemyViews[enemyPos];
        var targetHeroView = _heroViews[_enemyTargetIdx];

        yield return _PlaySkillAni(null, enemyView, targetHeroView);
    }

    private void _FreshAllEnemyViews()
    {
        foreach (var pair in _enemyViews)
        {
            pair.Value.ChangeHpSliderValue();
            pair.Value.ChangeMpSliderValue();
        }
        _SetCurAliveEnemyIdx();
    }
    #endregion

    #region Settlement
    public void PopupResultPage(bool win, Dictionary<string, HeroLevelExpData> levelExpDatas, List<Item> items)
    {
        _resultPopupPanel.OnComfirmAction -= _Leave;
        _resultPopupPanel.OnComfirmAction += _Leave;
        _resultPopupPanel.Show(win, _playerData.Heroes, levelExpDatas, items);
    }

    public void SetFightEndFlag()
    {
        _fightEnd = true;
    }
    #endregion
}