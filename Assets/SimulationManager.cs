
using System.Collections.Generic;
using UnityEngine;

public enum Intentions
{
    Lazy,
    Greedy,
    Social,
    Introvert,
    Inovation,
    Methodic
}

public enum LocationsContext
{
    Home,
    OthersHome,
    Work,
    Outside,
    Bed
}

public sealed class SimulationManager
{
    private static SimulationManager instance = null;

    public Activity[] activitiesList;

    private SimulationManager()
    {
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
}
