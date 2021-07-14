using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum RequirementComparison { EQUAL, LESSEQUAL, BIGGEREQUAL, LESSER, BIGGER}

public enum ContextUpdate { DECREASE, INCREASE, SET}

public enum ContextElement
{
    TIME,
    HUNGER,
    CLEAN,
    DRESSED,
    MOTIVATION
}

public enum LocationsContext
{
    BED,
    WORK,
    SHOWER,
    KITCHEN,
    WARDROBE
}

public class Requirement
{
    private ContextElement _name;
    public ContextElement name
    {
        get => _name;
    }

    private RequirementComparison _comparator;
    public RequirementComparison comparator
    {
        get => _comparator;
    }

    private float _value;
    public float value
    {
        get => _value;
    }

    public Requirement(ContextElement ce, RequirementComparison comp, float value)
    {
        _name = ce;
        _comparator = comp;
        _value = value;
    }

    public bool checkRequirement(float value)
    {
        switch(_comparator)
        {
            case RequirementComparison.EQUAL:
                return value == _value;
            case RequirementComparison.BIGGER:
                return value > _value;
            case RequirementComparison.LESSER:
                return value < _value;
            case RequirementComparison.BIGGEREQUAL:
                return value >= _value;
            case RequirementComparison.LESSEQUAL:
                return value <= _value;
            default:
                return false;
        }
    }
}

public class ContextEffect
{
    private ContextElement _name;
    public ContextElement name
    {
        get => _name;
    }

    private ContextUpdate _comparator;
    public ContextUpdate comparator
    {
        get => _comparator;
    }

    private float _value;
    public float value
    {
        get => _value;
    }

    public ContextEffect(ContextElement ce, ContextUpdate comp, float value)
    {
        _name = ce;
        _comparator = comp;
        _value = value;
    }

    public float apply(float previousValue)
    {
        switch(_comparator)
        {
            case ContextUpdate.DECREASE:
                return previousValue - _value;
            case ContextUpdate.INCREASE:
                return previousValue + _value;
            case ContextUpdate.SET:
                return _value;
            default:
                return 0;
        }
    }
}

public class HabitContext
{
    Dictionary<ContextElement, int> _reducedContext;
    Dictionary<ContextElement, int> reducedContext
    {
        get => _reducedContext;
    }

    public HabitContext(Dictionary<ContextElement, float> realContext)
    {
        _reducedContext = new Dictionary<ContextElement, int>();
        foreach (ContextElement ce in realContext.Keys)
        {
            switch(ce)
            {
                case ContextElement.CLEAN:
                    _reducedContext.Add(ContextElement.CLEAN, realContext[ce] == 1 ? 1 : 0);
                    break;
                case ContextElement.DRESSED:
                    _reducedContext.Add(ContextElement.DRESSED, realContext[ce] == 1 ? 1 : 0);
                    break;
                case ContextElement.HUNGER:
                    _reducedContext.Add(ContextElement.HUNGER, realContext[ce] >= 0.7 ? 1 : 0);
                    break;
                case ContextElement.MOTIVATION:
                    _reducedContext.Add(ContextElement.MOTIVATION, realContext[ce] >= 0.6 ? 1 : 0);
                    break;
                case ContextElement.TIME:
                    _reducedContext.Add(ContextElement.TIME, realContext[ce] >= 7.75 ? 1 : 0);
                    break;
            }
        }
    }

    public override bool Equals(object obj)
    {
        if(!(obj is HabitContext))
        {
            return false;
        }
        foreach(ContextElement ce in _reducedContext.Keys)
        {
            if(_reducedContext[ce] != ((HabitContext)obj).reducedContext[ce])
            {
                return false;
            }
        }
        return true;
    }

    public override int GetHashCode()
    {
        return 1935766155 + EqualityComparer<Dictionary<ContextElement, int>>.Default.GetHashCode(_reducedContext);
    }
}

public abstract class Activity
{
    protected LocationsContext _location;
    public LocationsContext location
    {
        get => _location;
    }

    protected float simulationSpeed = 30f;


    protected List<Requirement> _requirements;
    public List<Requirement> requirements
    {
        get => _requirements;
    }

    protected int _priority;
    public int priority
    {
        get => _priority;
    }

    protected List<ContextEffect> _effects;
    public List<ContextEffect> effects
    {
        get => _effects;
    }

    protected float _timeReq;
    public float timeReq
    {
        get => _timeReq;
    }

    public Activity(int priority, float timeReq)
    {
        //randomizeMeaning();
        _requirements = new List<Requirement>();
        _effects = new List<ContextEffect>();
        _timeReq = timeReq;
        _priority = priority;

    }

    public abstract bool priorityFunction(AgentScript agent);

    public abstract float interestFunction(AgentScript agent);

    public bool meetsRequirements(Dictionary<ContextElement, float> cont)
    {
        foreach(Requirement req in _requirements)
        {
            if(!req.checkRequirement(cont[req.name]))
            {
                return false;
            }
        }
        return true;
    }

    public List<Requirement> getFailedRequirements(Dictionary<ContextElement, float> cont)
    {
        List<Requirement> ret = new List<Requirement>();
        foreach (Requirement req in _requirements)
        {
            if (!req.checkRequirement(cont[req.name]))
            {
                ret.Add(req);
            }
        }
        return ret;
    }

    public abstract void execute(AgentScript agent);

    public List<Activity> getNeededActivities(Dictionary<ContextElement, float> cont, List<Activity> activitiesVisited)
    {
        activitiesVisited.Add(this);
        List<Activity> ret = new List<Activity>();
        ret.Add(this);
        if (meetsRequirements(cont)) //Activity can be done instantly
        {
            return ret;
        }
        //Activity needs other activities before hand
        List<KeyValuePair<Activity, int>> rankedActivities = new List<KeyValuePair<Activity, int>>(); //activities ranked by number of requirements solved
        foreach(Activity act in SimulationManager.Instance.activities)
        {
            if(activitiesVisited.Contains(act))
            {
                continue;
            }
            rankedActivities.Add(new KeyValuePair<Activity, int>(act, this.getFailedRequirements(cont).Count - this.getFailedRequirements(contextAfterEffect(cont, act)).Count));
        }
        rankedActivities.Sort((a, b) => b.Value - a.Value);
        List<List<Activity>> activityTree = new List<List<Activity>>();
        foreach (KeyValuePair<Activity, int> rankedAct in rankedActivities)
        {
            if(rankedAct.Key.meetsRequirements(cont))
            {
                activityTree.Add(new List<Activity>() { rankedAct.Key });
            } else
            {
                activityTree.Add(rankedAct.Key.getNeededActivities(cont, activitiesVisited));
            }
        }
        Dictionary<ContextElement, float> contextTmp = new Dictionary<ContextElement, float>(cont);
        //See the combination of activities that meet the Requirements
        foreach (List<Activity> actBranch in activityTree) //activities are Ranked, so we just add one to another because the most important ones are the first ones
        {
            contextTmp = contextAfterEffect(contextTmp, actBranch[0]);
            ret.AddRange(actBranch);
            if (meetsRequirements(contextTmp))
            {
                return ret;
            }
        }
        return ret;
    }


    public static Dictionary<ContextElement, float> contextAfterEffect(Dictionary<ContextElement, float> cont, Activity act)
    {
        Dictionary<ContextElement, float> ret = new Dictionary<ContextElement, float>(cont);
        foreach (ContextEffect effect in act.effects)
        {
            ret[effect.name] = effect.apply(ret[effect.name]);
        }
        return ret;
    }

}

