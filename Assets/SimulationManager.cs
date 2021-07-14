
using System.Collections.Generic;
using UnityEngine;



public enum ItemNames
{
    CopperOre,
    CopperIngot,
    GoldOre,
    GoldIngot,
    Pickaxe,

}

public sealed class SimulationManager
{
    private static SimulationManager instance = null;

    private List<Activity> _activities;
    public List<Activity> activities
    {
        get => _activities;
        set => _activities = value;
    }

    private List<Activity> _priorityActivities;
    public List<Activity> priorityActivities
    {
        get => _priorityActivities;
        set => _priorityActivities = value;
    }

    private SimulationManager()
    {
        _activities = new List<Activity>();
    }

    public static SimulationManager Instance
    {
        get
        {
            if(instance == null)
            {
                instance = new SimulationManager();
            }
            return instance;
        }
    }

    public void updatePriorityActivitiesRanked()
    {
        List<KeyValuePair<Activity, int>> actRank = new List<KeyValuePair<Activity, int>>();
        foreach(Activity act in activities)
        {
            if(act.priority != -1)
            {
                actRank.Add(new KeyValuePair<Activity, int>(act, act.priority));
            }
        }
        actRank.Sort((a, b) => a.Value - b.Value);
        _priorityActivities = new List<Activity>();
        foreach (KeyValuePair<Activity, int> act in actRank)
        {
            _priorityActivities.Add(act.Key);
        }

    }

}

public class Item
{
    private ItemNames _name;
    public ItemNames name
    {
        get => _name;
        set => _name = value;
    }

    private float _quantity;
    public float quantity
    {
        get => _quantity;
        set => _quantity = value;
    }

    public Item(ItemNames name, float quantity)
    {
        _name = name;
        _quantity = quantity;
    }


}
