using UnityEngine;
using System.Collections;

public class BehaviorTree : MonoBehaviour
{
    [Header("Animal Values")]
    public int hunger;
    public int thirst;
    public int energy;
    public int age;
    public int speed;

    [Header("Max Values")]
    private int maxHunger = 100;
    private int maxThirst = 100;
    private int maxEnergy = 100;
    private int maxAge = 15;

    [Header("Interactables")]
    private GameObject foodSource;
    private GameObject waterSource;
    private GameObject home;
    private GameObject field;

    public enum Behaviors
    {
        Idle, Wandering, Hungry, Thirsty, Asleep
    }

    public Behaviors currentBehavior;

    void Start()
    {
        //Find Transforms
        foodSource = GameObject.Find("FoodSource");
        waterSource = GameObject.Find("WaterSource");
        home = GameObject.Find("Bed");
        field = GameObject.Find("Field");

        //Set Values to Default
        hunger = maxHunger;
        thirst = maxThirst;
        energy = maxEnergy;
        speed = 15;
        age = 1;

        StartCoroutine(GainAge());
        StartCoroutine(DepleteHunger());
        StartCoroutine(DepleteThirst());
    }

    void Update()
    {
        //Behaviors
        BehaviorTreeFunc();

        //Managements
        AgeManagement();
        HungerManagement();
        ThirstManagement();
        EnergyManagement();
        
    }

    //Movement
    private void MoveAnimal(Vector3 destination)
    {
        float step = speed * Time.deltaTime;
        Vector3 newPos = Vector3.MoveTowards(transform.position, destination, step);
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
    }

    private void BehaviorTreeFunc()
    {
        switch (currentBehavior)
        {
            case Behaviors.Idle:
                MoveAnimal(field.transform.position);
                break;
            case Behaviors.Wandering:
                break;
            case Behaviors.Hungry:
                MoveAnimal(foodSource.transform.position);
                if (hunger != maxHunger)
                {
                    StartCoroutine(AddHunger());
                }
                break;
            case Behaviors.Thirsty:
                MoveAnimal(waterSource.transform.position);
                if (thirst != maxThirst)
                {
                    StartCoroutine(AddThirst());
                }
                break;
            case Behaviors.Asleep:
                MoveAnimal(home.transform.position);
                if (energy != maxThirst)
                {
                    StartCoroutine(AddEnergy());
                }
                break;
        }
    }

    //Hungry
    private IEnumerator DepleteHunger()
    {
        int Interval = Random.Range(1, 10);
        int hungerLoss = 10;

        yield return new WaitForSeconds(Interval);
        hunger = hunger - hungerLoss;

        StartCoroutine(DepleteHunger());
    }

    private IEnumerator AddHunger()
    {
        yield return new WaitForSeconds(2f);
        hunger = maxHunger;

        if (hunger == maxHunger)
        {
            currentBehavior = Behaviors.Idle;
        }
    }

    private void HungerManagement()
    {
        if (hunger <= maxHunger / 1.5f && currentBehavior == Behaviors.Idle)
        {
            currentBehavior = Behaviors.Hungry;    
        }
    }

    //Thirsty
    private IEnumerator DepleteThirst()
    {
        int Interval = Random.Range(1, 10);
        int thirstLoss = 10;

        yield return new WaitForSeconds(Interval);
        thirst = thirst - thirstLoss;

        StartCoroutine(DepleteThirst());
    }

    private IEnumerator AddThirst()
    {
        yield return new WaitForSeconds(2f);
        thirst = maxThirst;

        if (thirst == maxThirst)
        {
            currentBehavior = Behaviors.Idle;
        }
    }

    private void ThirstManagement()
    {
        if (thirst <= maxThirst / 2 && currentBehavior == Behaviors.Idle)
        {
            currentBehavior = Behaviors.Thirsty;
        }
    }

    //Energy
    private IEnumerator DepleteEnergy()
    {
        int Interval = Random.Range(1, 10);
        int energyLoss = 10;

        yield return new WaitForSeconds(Interval);
        energy = energy - energyLoss;

        StartCoroutine(DepleteEnergy());
    }

    private IEnumerator AddEnergy()
    {
        yield return new WaitForSeconds(8f);
        energy = maxEnergy;

        if (energy == maxEnergy)
        {
            currentBehavior = Behaviors.Idle;
        }
    }

    private void EnergyManagement()
    {
        if (energy <= maxEnergy / 1.25f && currentBehavior == Behaviors.Idle)
        {
            currentBehavior = Behaviors.Asleep;    
        }
    }

    //Aging 
    private IEnumerator GainAge()
    {
        yield return new WaitForSeconds(10f);
        age++;
        StartCoroutine(GainAge());
    }

    private void AgeManagement()
    {
        speed = 20 - age;
        if (age >= maxAge)
        {
            Destroy(gameObject);
            //Die of Old Age Screen
        }
    }


}
