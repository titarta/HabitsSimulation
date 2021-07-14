using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationDefinition : MonoBehaviour
{
    public List<GameObject> agents;
    private List<AgentScript> agScripts;
    private List<Activity> activities;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    private void Awake()
    {
        agScripts = new List<AgentScript>();
        foreach (GameObject gameObj in agents)
        {
            if (gameObj.TryGetComponent(out AgentScript agScript))
            {
                agScripts.Add(agScript);
            }
        }
        this.activities = new List<Activity>();
        this.activities.Add(new WorkActivity());
        this.activities.Add(new ShowerActivity());
        this.activities.Add(new DressActivity());
        this.activities.Add(new BreakfastActivity());
        this.activities.Add(new NothingActivity());

        SimulationManager.Instance.activities = this.activities;
        SimulationManager.Instance.updatePriorityActivitiesRanked();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void act()
    {

    }
}


public class WorkActivity : Activity
{
    private float timeSpent;

    public WorkActivity() : base(1, 0.25f)
    {
        _requirements.Add(new Requirement(ContextElement.DRESSED, RequirementComparison.EQUAL, 1));
        

        _location = LocationsContext.WORK;

        timeSpent = 0;
    }
    public override void execute(AgentScript agent)
    {
        agent.busyWithTask = true;
        agent.StopAllCoroutines();
        agent.StartCoroutine(moveAgent(agent));
        
        //agent.updateHabituality(agent.currentContext, this);
    }

    public override float interestFunction(AgentScript agent)
    {
        return agent.context[ContextElement.MOTIVATION] - (agent.context[ContextElement.HUNGER] * 0.6f) - (agent.context[ContextElement.CLEAN] * 0.5f);
    }

    public override bool priorityFunction(AgentScript agent)
    {
        return (agent.context[ContextElement.TIME] >= 8 && agent.context[ContextElement.DRESSED] == 1) || (agent.context[ContextElement.TIME] >= 7.75f && agent.context[ContextElement.DRESSED] == 0);
    }

    private IEnumerator moveAgent(AgentScript agentScript)
    {

        Vector3 initialPos = agentScript.gameObject.transform.position;
        Vector3 finalPos = agentScript.getLocationFromLocationContext(this.location);

        Vector3 dir = finalPos - initialPos;
        Vector2 dir2D = new Vector2(dir.x, dir.z);

        dir2D = dir2D.normalized;

        float dist = Mathf.Sqrt(Mathf.Pow(initialPos.x - finalPos.x, 2) + Mathf.Pow(initialPos.z - finalPos.z, 2));

        float finalTime = 5 / simulationSpeed;

        float velocity = (dist / finalTime) ;

        float elapsedTime = 0;

        while (elapsedTime < finalTime)
        {
            agentScript.gameObject.transform.position = initialPos + new Vector3((dir2D * elapsedTime * velocity).x, 0, (dir2D * elapsedTime * velocity).y);

            elapsedTime += Time.deltaTime;
            yield return null;
        }
        elapsedTime = 0;
        while (elapsedTime < ((8.25 - agentScript.context[ContextElement.TIME])*16) / simulationSpeed)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        agentScript.busyWithTask = false;
        agentScript.InitializeContext();
    }
}

public class BreakfastActivity : Activity
{
    private float timeSpent;

    public BreakfastActivity() : base(-1, 0.25f)
    {
        _effects.Add(new ContextEffect(ContextElement.HUNGER, ContextUpdate.SET, 0));
        _effects.Add(new ContextEffect(ContextElement.TIME, ContextUpdate.INCREASE, 0.25f));

        _location = LocationsContext.KITCHEN;

        timeSpent = 2;
    }

    public override void execute(AgentScript agent)
    {
        agent.busyWithTask = true;
        agent.StopAllCoroutines();
        agent.StartCoroutine(moveAgent(agent));
        //agent.updateHabituality(agent.currentContext, this);
    }

    public override float interestFunction(AgentScript agent)
    {
        return (agent.context[ContextElement.HUNGER] * 0.8f) - 0.1f;
    }

    public override bool priorityFunction(AgentScript agent)
    {
        return false;
    }

    private IEnumerator moveAgent(AgentScript agentScript)
    {

        Vector3 initialPos = agentScript.gameObject.transform.position;
        Vector3 finalPos = agentScript.getLocationFromLocationContext(this.location);

        Vector3 dir = finalPos - initialPos;
        Vector2 dir2D = new Vector2(dir.x, dir.z);

        dir2D = dir2D.normalized;

        float dist = Mathf.Sqrt(Mathf.Pow(initialPos.x - finalPos.x, 2) + Mathf.Pow(initialPos.z - finalPos.z, 2));

        float finalTime = 2 / simulationSpeed;

        float velocity = (dist / finalTime);

        float elapsedTime = 0;

        while (elapsedTime < finalTime)
        {
            agentScript.gameObject.transform.position = initialPos + new Vector3((dir2D * elapsedTime * velocity).x, 0, (dir2D * elapsedTime * velocity).y);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0;
        timeSpent /= simulationSpeed;
        while (elapsedTime < timeSpent)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        agentScript.busyWithTask = false;
    }
}

public class ShowerActivity : Activity
{
    private float timeSpent;

    public ShowerActivity() : base(-1, 0.25f)
    {
        _effects.Add(new ContextEffect(ContextElement.CLEAN, ContextUpdate.SET, 1));
        _effects.Add(new ContextEffect(ContextElement.TIME, ContextUpdate.INCREASE, 0.25f));

        _location = LocationsContext.SHOWER;

        timeSpent = 3;
    }

    public override void execute(AgentScript agent)
    {
        agent.busyWithTask = true;
        agent.StopAllCoroutines();
        agent.StartCoroutine(moveAgent(agent));
        //agent.updateHabituality(agent.currentContext, this);
    }

    public override float interestFunction(AgentScript agent)
    {
        return 0.45f - (agent.context[ContextElement.CLEAN] * 2f) + (0.2f * agent.context[ContextElement.MOTIVATION]);
    }

    public override bool priorityFunction(AgentScript agent)
    {
        return false;
    }

    private IEnumerator moveAgent(AgentScript agentScript)
    {

        Vector3 initialPos = agentScript.gameObject.transform.position;
        Vector3 finalPos = agentScript.getLocationFromLocationContext(this.location);

        Vector3 dir = finalPos - initialPos;
        Vector2 dir2D = new Vector2(dir.x, dir.z);

        dir2D = dir2D.normalized;

        float dist = Mathf.Sqrt(Mathf.Pow(initialPos.x - finalPos.x, 2) + Mathf.Pow(initialPos.z - finalPos.z, 2));

        float finalTime = 1 / simulationSpeed;

        float velocity = (dist / finalTime);

        float elapsedTime = 0;

        while (elapsedTime < finalTime)
        {
            agentScript.gameObject.transform.position = initialPos + new Vector3((dir2D * elapsedTime * velocity).x, 0, (dir2D * elapsedTime * velocity).y);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0;
        timeSpent /= simulationSpeed;
        while (elapsedTime < timeSpent)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        agentScript.busyWithTask = false;
    }
}

public class DressActivity : Activity
{
    private float timeSpent;

    public DressActivity() : base(-1, 0.25f)
    {
        _requirements.Add(new Requirement(ContextElement.DRESSED, RequirementComparison.EQUAL, 0));

        _effects.Add(new ContextEffect(ContextElement.DRESSED, ContextUpdate.SET, 1));
        _effects.Add(new ContextEffect(ContextElement.TIME, ContextUpdate.INCREASE, 0.25f));

        _location = LocationsContext.WARDROBE;

        timeSpent = 2;
    }

    public override void execute(AgentScript agent)
    {
        agent.busyWithTask = true;
        agent.StopAllCoroutines();
        agent.StartCoroutine(moveAgent(agent));
        //agent.updateHabituality(agent.currentContext, this);
    }

    public override float interestFunction(AgentScript agent)
    {
        return 0;
    }

    public override bool priorityFunction(AgentScript agent)
    {
        return false;
    }

    private IEnumerator moveAgent(AgentScript agentScript)
    {

        Vector3 initialPos = agentScript.gameObject.transform.position;
        Vector3 finalPos = agentScript.getLocationFromLocationContext(this.location);

        Vector3 dir = finalPos - initialPos;
        Vector2 dir2D = new Vector2(dir.x, dir.z);

        dir2D = dir2D.normalized;

        float dist = Mathf.Sqrt(Mathf.Pow(initialPos.x - finalPos.x, 2) + Mathf.Pow(initialPos.z - finalPos.z, 2));

        float finalTime = 2 / simulationSpeed;

        float velocity = (dist / finalTime);

        float elapsedTime = 0;

        while (elapsedTime < finalTime)
        {
            agentScript.gameObject.transform.position = initialPos + new Vector3((dir2D * elapsedTime * velocity).x, 0, (dir2D * elapsedTime * velocity).y);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0;
        timeSpent /= simulationSpeed;
        while (elapsedTime < timeSpent)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        agentScript.busyWithTask = false;

    }
}

public class NothingActivity : Activity
{
    private float timeSpent;

    public NothingActivity() : base(-1, 0.25f)
    {
        _effects.Add(new ContextEffect(ContextElement.TIME, ContextUpdate.INCREASE, 0.25f));

        _location = LocationsContext.BED;

        timeSpent = 2;
    }

    public override void execute(AgentScript agent)
    {
        agent.busyWithTask = true;
        agent.StopAllCoroutines();
        agent.StartCoroutine(moveAgent(agent));
        //agent.updateHabituality(agent.currentContext, this);
    }

    public override float interestFunction(AgentScript agent)
    {
        return 0.5f;
    }

    public override bool priorityFunction(AgentScript agent)
    {
        return false;
    }

    private IEnumerator moveAgent(AgentScript agentScript)
    {

        Vector3 initialPos = agentScript.gameObject.transform.position;
        Vector3 finalPos = agentScript.getLocationFromLocationContext(this.location);

        Vector3 dir = finalPos - initialPos;
        Vector2 dir2D = new Vector2(dir.x, dir.z);

        dir2D = dir2D.normalized;

        float dist = Mathf.Sqrt(Mathf.Pow(initialPos.x - finalPos.x, 2) + Mathf.Pow(initialPos.z - finalPos.z, 2));

        float finalTime = 4 / simulationSpeed;

        float velocity = (dist / finalTime);

        float elapsedTime = 0;

        while (elapsedTime < finalTime)
        {
            agentScript.gameObject.transform.position = initialPos + new Vector3((dir2D * elapsedTime * velocity).x, 0, (dir2D * elapsedTime * velocity).y);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        elapsedTime = 0;
        timeSpent /= simulationSpeed;
        while (elapsedTime < timeSpent)
        {
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        agentScript.busyWithTask = false;

    }
}




