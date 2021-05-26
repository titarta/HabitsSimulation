using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class AgentScript : MonoBehaviour
{
    private Dictionary<Intentions, double> _intentions;
    public Dictionary<Intentions, double> intentions
    {
        get { return _intentions; }
        set { _intentions = value; }
    }


    private double _habitRate;
    public double habitRate
    {
        get; set;
    }

    private Dictionary<Tuple<EnvironmentContext, Activity>, double> _habituality;
    public Dictionary<Tuple<EnvironmentContext, Activity>, double> habituality
    {
        get;
    }

    private EnvironmentContext _currentContext;
    public EnvironmentContext currentContext
    { get; }


    // Start is called before the first frame update
    void Start()
    {
        randomizeIntentions();
        randomizeHabitRate();
    }

    // Update is called once per frame
    void Update()
    {
        updateContext();
    }

    private void randomizeIntentions()
    {
        _intentions = new Dictionary<Intentions, double>();
        foreach (Intentions intent in Enum.GetValues(typeof(Intentions)))
        {
            _intentions.Add(intent, Random.value);
        }
    }

    private void randomizeHabitRate()
    {
        _habitRate = (Random.value + 0.5) / 100.0;
    }


    private void updateHabituality(EnvironmentContext context, Activity activity)
    {
        Tuple<EnvironmentContext, Activity> connection = new Tuple<EnvironmentContext, Activity>(context, activity);
        if (_habituality.ContainsKey(connection))
        {
            _habituality[connection] += _habitRate * getActivityBonus(activity);
        }
        _habituality.Add(connection, _habitRate * getActivityBonus(activity));

    }

    private double getActivityBonus(Activity activity) //how good the activity is for the agent
    {
        double squaredDifferenceSum = 0;
        foreach (Intentions intent in Enum.GetValues(typeof(Intentions)))
        {
            squaredDifferenceSum += Math.Pow(_intentions[intent] - activity.meaning[intent], 2);
        }
        squaredDifferenceSum /= Enum.GetValues(typeof(Intentions)).Length;

        return (0.5 - Math.Sqrt(squaredDifferenceSum));
    }

    private void updateContext()
    {

    }

    private Activity cognition()
    {
        Activity activityChosen = null;
        double maxProfit = Double.NegativeInfinity;
        foreach (Activity activity in SimulationManager.Instance.activitiesList)
        {
            if(_habituality.ContainsKey(new Tuple<EnvironmentContext, Activity>(_currentContext, activity)))
            {
                if (Random.value <= _habituality[new Tuple<EnvironmentContext, Activity>(_currentContext, activity)])
                {
                    return activity;
                }
            }
        }
        foreach (Activity activity in SimulationManager.Instance.activitiesList)
        {
            double profit = getActivityBonus(activity);
            if (profit > maxProfit)
            {
                activityChosen = activity;
                maxProfit = profit;
            }
        }
        return activityChosen;
    }
}

public class EnvironmentContext
{
    private LocationsContext _location;
    public LocationsContext location
    {
        get { return _location; }
        set { }
    }

    private Activity _previousActivity;
    public Activity previousActivity
    {
        get { return _previousActivity; }
        set { }
    }

    private double _time;
    public double time
    {
        get { return _time; }
        set { }
    }

    public EnvironmentContext(LocationsContext locContext, Activity previous, double time)
    {
        _location = locContext;
        _previousActivity = previous;
        this.time = time;
    }
}

public class Activity
{
    //requirements:
    //location
    //state?
    //etc
    private Dictionary<Intentions, double> _meaning;
    public Dictionary<Intentions, double> meaning
    {
        get { return _meaning; }
        set { _meaning = value; }
    }


    public Activity()
    {
        randomizeMeaning();
    }


    private void randomizeMeaning()
    {
        _meaning = new Dictionary<Intentions, double>();
        foreach (Intentions intent in Enum.GetValues(typeof(Intentions)))
        {
            _meaning.Add(intent, Random.value);
        }
    }
}