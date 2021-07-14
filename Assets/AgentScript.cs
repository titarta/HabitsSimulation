using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class AgentScript : MonoBehaviour
{
    private Dictionary<ContextElement, float> _context;
    public Dictionary<ContextElement, float> context
    {
        get => _context;
        set => _context = value;
    }


    private float _habitRate;
    public float habitRate
    {
        get; set;
    }


    private Dictionary<Tuple<HabitContext, Activity>, float> _habituality;
    public Dictionary<Tuple<HabitContext, Activity>, float> habituality
    {
        get => _habituality;
    }


    private List<Activity> _activityQueue;
    public List<Activity> activityQueue
    {
        get => _activityQueue;
    }


    [SerializeField]
    private List<AgentLocationInfo> _importantLocations;
    public List<AgentLocationInfo> importantLocations
    {
        get => _importantLocations;
    }

    private bool firstWrite = true;

    [SerializeField]
    public float hungerLowerLimit;
    [SerializeField]
    public float hungerUpperLimit;
    [SerializeField]
    public float motivationLowerLimit;
    [SerializeField]
    public float motivationUpperLimit;

    private List<Item> _inventory;
    public List<Item> inventory
    {
        get => _inventory;
    }

    private bool _busyWithTask;
    public bool busyWithTask
    {
        get => _busyWithTask;
        set => _busyWithTask = value;
    }

    private string filePathRoutines;
    private string filePathHabits;
    private List<Activity> routinePerformed;

    // Start is called before the first frame update
    void Start()
    {
        filePathRoutines = "C:\\Users\\trvca\\Pictures\\SSASC\\Csvs\\" + gameObject.name + "\\routines.csv";
        filePathHabits = "C:\\Users\\trvca\\Pictures\\SSASC\\Csvs\\" + gameObject.name + "\\habitsAvg.csv";
        File.WriteAllText(filePathRoutines, "");
        File.WriteAllText(filePathHabits, "");
        randomizeHabitRate();
        _inventory = new List<Item>();
        _busyWithTask = false;
        InitializeContext();
        _activityQueue = new List<Activity>();
        _habituality = new Dictionary<Tuple<HabitContext, Activity>, float>();
    }

    // Update is called once per frame
    void Update()
    {
        if(_busyWithTask)
        {
            return;
        }
        if(_activityQueue.Count != 0)
        {
            _context = Activity.contextAfterEffect(_context, _activityQueue[0]);
            updateHabituality(new HabitContext(_context), _activityQueue[0]);
            _activityQueue[0].execute(this);
            routinePerformed.Add(_activityQueue[0]);
            _activityQueue.RemoveAt(0);
        } else
        {
            _activityQueue = cognition();
        }

        
    }


    private void randomizeHabitRate()
    {
        _habitRate = 0.4f;
    }


    public void updateHabituality(HabitContext context, Activity activity)
    {
        Tuple<HabitContext, Activity> connection = new Tuple<HabitContext, Activity>(context, activity);
        if (_habituality.ContainsKey(connection))
        {
            _habituality[connection] += (_habitRate * activity.interestFunction(this)) * (1 - _habituality[connection]);
            if(_habituality[connection] <= 0)
            {
                _habituality.Remove(connection);
            }
        } else
        {
            _habituality.Add(connection, Math.Max(0, _habitRate * activity.interestFunction(this)));
        }
        List<Tuple<HabitContext, Activity>> connections = new List<Tuple<HabitContext, Activity>>();
        foreach(Activity action in SimulationManager.Instance.activities)
        {
            if(action.Equals(activity))
            {
                continue;
            }
            Tuple<HabitContext, Activity> possibleConnection = new Tuple<HabitContext, Activity>(context, action);
            if (_habituality.ContainsKey(possibleConnection))
            {
                connections.Add(possibleConnection);
            }
        }
        foreach (Tuple<HabitContext, Activity> habitualConn in connections)
        {
            _habituality[habitualConn] = Math.Max(0, _habituality[habitualConn] - ((activity.interestFunction(this) - habitualConn.Item2.interestFunction(this)) * habitRate / connections.Count));
            if(_habituality[habitualConn] == 0)
            {
                _habituality.Remove(habitualConn);
            }
        }
        Debug.Log("Updated Habituality: Activity Type-" + activity.GetType().Name + " ; Strenght-" + _habituality[connection]);
    }


    public void InitializeContext()
    {
        _context = new Dictionary<ContextElement, float>();
        _context.Add(ContextElement.CLEAN, 0);
        _context.Add(ContextElement.DRESSED, 0);
        _context.Add(ContextElement.TIME, 7);
        _context.Add(ContextElement.MOTIVATION, motivationLowerLimit + (Random.value * (motivationUpperLimit - motivationLowerLimit)));
        _context.Add(ContextElement.HUNGER, hungerLowerLimit + (Random.value * (hungerUpperLimit - hungerLowerLimit)));
        foreach(AgentLocationInfo loc in _importantLocations)
        {
            if(loc.description == LocationsContext.BED)
            {
                this.gameObject.transform.position = new Vector3(loc.location.transform.position.x, 1.5f, loc.location.transform.position.z);
            }
        }
        writeToFiles();

    }

    private void writeToFiles()
    {
        if(firstWrite)
        {
            firstWrite = false;
            routinePerformed = new List<Activity>();
            return;
        }
        string routinePerformedText = "";
        foreach(Activity act in routinePerformed)
        {
            if(act is WorkActivity)
            {
                routinePerformedText += "W";
                break;
            }
            if (act is DressActivity)
            {
                routinePerformedText += "D";
            }
            if (act is BreakfastActivity)
            {
                routinePerformedText += "B";
            }
            if (act is ShowerActivity)
            {
                routinePerformedText += "S";
            }
            if (act is NothingActivity)
            {
                routinePerformedText += "N";
            }
            routinePerformedText += "-";
        }
        routinePerformed = new List<Activity>();
        File.AppendAllText(filePathRoutines, routinePerformedText + "\n");
        float sumHabituality = 0;
        float countHabituality = 0;
        foreach(float habitForce in _habituality.Values)
        {
            countHabituality++;
            sumHabituality += habitForce;
        }
        File.AppendAllText(filePathHabits, (sumHabituality / countHabituality) + "\n");
        
    }

    private List<Activity> cognition()
    {
        //Check priority activities (and perform them if necessary)
        foreach (Activity activity in SimulationManager.Instance.priorityActivities)
        {
            if (activity.priorityFunction(this))
            {
                List<Activity> tmp = activity.getNeededActivities(context, new List<Activity>());
                tmp.Reverse();
                return tmp;
            }
        }

        //Get all possible activities and choose the most rewarding
        List<Activity> bestActivity = new List<Activity>();
        List<List<Activity>> possiblePlans = new List<List<Activity>>();
        foreach (Activity activity in SimulationManager.Instance.activities)
        {
            List<Activity> activityTree = activity.getNeededActivities(context, new List<Activity>());
            activityTree.Reverse();
            possiblePlans.Add(activityTree);
        }
        float mostPromissingPlanValue = float.MinValue;
        foreach(List<Activity> actTree in possiblePlans)
        {
            float likeness = getAvgLikenessFromActivities(actTree);
            if(likeness >= mostPromissingPlanValue)
            {
                mostPromissingPlanValue = likeness;
                bestActivity = actTree;
            }
        }

        //Check if there is a possible habit to perform and compare the habit connection with the reward of the activity
        float strongestHabitStrenght = float.MinValue;
        List<Activity> bestActivityHabits = new List<Activity>();
        HabitContext habitCont = new HabitContext(_context);
        foreach(Tuple<HabitContext, Activity> habituality in _habituality.Keys)
        {
            if(habituality.Item1.Equals(habitCont))
            {
                if(_habituality[habituality] >= strongestHabitStrenght)
                {
                    strongestHabitStrenght = _habituality[habituality];
                    List<Activity> tmp = habituality.Item2.getNeededActivities(context, new List<Activity>());
                    tmp.Reverse();
                    bestActivityHabits = tmp;
                }
            }
        }

        return mostPromissingPlanValue >= strongestHabitStrenght ? bestActivity : bestActivityHabits;

    }

    private float getAvgLikenessFromActivities(List<Activity> activities)
    {
        float sum = 0;
        foreach (Activity activity in activities)
        {
            sum += activity.interestFunction(this);
        }
        return sum / activities.Count;
    }
    

    //Activity activityChosen = null;
    //double maxProfit = Double.NegativeInfinity;
    //foreach (Activity activity in SimulationManager.Instance.activities)
    //{
    //    if(!activity.canBePerformed(this))
    //    {
    //        continue;
    //    }
    //    if(_habituality.ContainsKey(new Tuple<EnvironmentContext, Type>(_currentContext, activity.GetType())))
    //    {
    //        if (Random.value <= _habituality[new Tuple<EnvironmentContext, Type>(_currentContext, activity.GetType())])
    //        {
    //            return activity;
    //        }
    //    }
    //}
    //foreach (Activity activity in SimulationManager.Instance.activities)
    //{
    //    if (!activity.canBePerformed(this))
    //    {
    //        continue;
    //    } //add dependencies
    //    double profit = getActivityBonus(activity);
    //    if (profit > maxProfit)
    //    {
    //        activityChosen = activity;
    //        maxProfit = profit;
    //    }
    //}
    //return activityChosen;

   

    public Vector3 getLocationFromLocationContext(LocationsContext locContext)
    {
        return _importantLocations.Find(loc => loc.description == locContext).location.transform.position;
    }

    public void addItemsToInventory(List<Item> items)
    {
        foreach(Item it in items)
        {
            foreach(Item itInv in _inventory)
            {
                if(itInv.name == it.name)
                {
                    itInv.quantity += it.quantity;
                    break;
                }
                _inventory.Add(it);
            }
        }
    }
}

public class EnvironmentContext
{
    private LocationsContext _location;
    public LocationsContext location
    {
        get => _location;
    }

    private Activity _previousActivity;
    public Activity previousActivity
    {
        get => _previousActivity;
    }

    private double _time;
    public double time
    {
        get => _time;
    }

    public EnvironmentContext(LocationsContext locContext, Activity previous, double time)
    {
        _location = locContext;
        _previousActivity = previous;
        this._time = time;
    }

    public override bool Equals(object obj)
    {
        return obj is EnvironmentContext context &&
               _location == context._location &&
               EqualityComparer<Activity>.Default.Equals(_previousActivity, context._previousActivity) &&
               _time == context._time;
    }

    public override int GetHashCode()
    {
        int hashCode = 700725158;
        hashCode = hashCode * -1521134295 + _location.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<Activity>.Default.GetHashCode(_previousActivity);
        hashCode = hashCode * -1521134295 + _time.GetHashCode();
        return hashCode;
    }
}



[Serializable]
public struct AgentLocationInfo{
    [SerializeField]
    public LocationsContext description;
    [SerializeField]
    
    public GameObject location;
}