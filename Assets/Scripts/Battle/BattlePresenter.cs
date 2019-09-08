using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IBattlePresenter
{
    IEnumerator Run();
}
public class BattlePresenter : IBattlePresenter
{
    private IBattleView _view = null;
    private TeamData _teamData = null;
    private Dictionary<int, EnemyData> _enemiesData = null;
    private bool _leave = false;
    private int _curMemberIdx = -1;
    private int _curEnemyIdx = -1;
    private int _targetMemberIdx = -1;

    public BattlePresenter(
        IBattleView view,
        TeamData teamData,
        Dictionary<int, EnemyData> enemiesData)
    {
        _view = view;
        _teamData = teamData;
        _enemiesData = enemiesData;
    }

    public IEnumerator Run()
    {
        _Register();
        _view.Enter(_teamData, _enemiesData);

        while (!_leave)
        {
            if(_CheckIsMembersTurnEnd())
            {
                yield return _EnemiesTurn();
                _ActiveMembersTurn();
            }
            yield return null;
        }
    }

    private void _Register()
    {
        _UnRegister();
        _view.SelectMember += _SelectMember;
        _view.SelectEnemy += _SelectEnemy;
        _view.MemberEndTurn += _MemberEndTurn;
        _view.MemberDamageEffect += _MemberDamageEffect;
        _view.EnemyDamageEffect += _EnemyDamageEffect;
    }

    private void _UnRegister()
    {
        _view.SelectMember -= _SelectMember;
        _view.SelectEnemy -= _SelectEnemy;
        _view.MemberEndTurn -= _MemberEndTurn;
        _view.MemberDamageEffect -= _MemberDamageEffect;
        _view.EnemyDamageEffect -= _EnemyDamageEffect;
    }

    private void _SelectMember(bool select,int pos)
    {
        _curMemberIdx = select ? pos : -1;
    }

    private void _SelectEnemy(int pos)
    {
        _curEnemyIdx = pos;
    }

    private void _MemberEndTurn(int pos)
    {
        _teamData.Members[pos].SetEndTurnFlag(true);
    }

    private bool _CheckIsMembersTurnEnd()
    {
        foreach(var member in _teamData.Members.Values.ToList())
        {
            if (!member.IsAlive)
                continue;
            if (!member.IsTurnEnd)
                return false;
        }
        return true;
    }

    private IEnumerator _EnemiesTurn()
    {
        _targetMemberIdx = _GetAliveMemberIndex();
        foreach (var enemy in _enemiesData.Values)
        {
            if (enemy.IsAlive && _targetMemberIdx != -1)
            {
                yield return _view.RunEnemyTurn(enemy.Pos, _targetMemberIdx);
                if (!_teamData.Members[_targetMemberIdx].IsAlive)
                    _targetMemberIdx = _GetAliveMemberIndex();
            }
        }
    }

    private int _GetAliveMemberIndex()
    {
        for (int i = 0; i < 6; i++)
        {
            if (_teamData.Members.ContainsKey(i) && _teamData.Members[i].IsAlive)
                return i;
        }
        _leave = true;
        return -1;
    }

    private void _ActiveMembersTurn()
    {
        foreach(var member in _teamData.Members.Values.ToList())
        {
            if (!member.IsAlive)
                continue;
            member.SetEndTurnFlag(false);
        }
        _view.ActiveMemberElement();
    }

    private void _MemberDamageEffect(double attack)
    {
        var enemy = _enemiesData[_curEnemyIdx];
        enemy.BeHit(attack);
        if (!_CheckEnemiesAlive())
            _leave = true;
    }

    private void _EnemyDamageEffect(double attack)
    {
        var member = _teamData.Members[_targetMemberIdx];
        member.BeHit(attack);
    }

    private bool _CheckEnemiesAlive()
    {
        foreach (var enemy in _enemiesData.Values)
            if (enemy.IsAlive)
                return true;
        return false;
    }
}
