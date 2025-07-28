using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class BehaviorTree : MonoBehaviour
{
    [Header("Animal Values")]
    public int hunger;
    public int thirst;
    public int energy;
    public int age;
    public int speed;
    public bool isReplenishing = false;

    [Header("Max Values")]
    private int maxHunger = 100;
    private int maxThirst = 100;
    private int maxEnergy = 100;
    private int maxAge = 15;
    private int startingAge = 1;
    private int startingSpeed = 15;

    [Header("Interactables")]
    private GameObject foodSource;
    private GameObject waterSource;
    private GameObject home;
    private GameObject field;
    private GameObject enemy;
    private GameObject fort;

    [Header("Shelter Logic")]
    public bool isEnemyPresent = false;

    [Header("Text & Bars")]
    public TMP_Text[] TMP_Values;
    public Image[] Life_Bars;

    [Header("Animal Apperance")]
    private SpriteRenderer animalImage;
    public Sprite aliveApperance;
    public Sprite deadApperance;

    public enum Behaviors
    {
        Idle, Hungry, Thirsty, Asleep, Shelter, Dead
    }

    public Behaviors currentBehavior;

    void Start()
    {
        FindTransforms(); //Positions of Interactables
        SetApperance(); //Set Apperance of Animal
        DefaultValues(); //Starting Values
        BeginLife(); //Coroutines
    }

    private void SetApperance()
    {
        animalImage = GetComponent<SpriteRenderer>();
        animalImage.sprite = aliveApperance;
    }

    private void FindTransforms()
    {
        foodSource = GameObject.Find("FoodSource");
        waterSource = GameObject.Find("WaterSource");
        home = GameObject.Find("Bed");
        field = GameObject.Find("Field");
        enemy = GameObject.Find("Enemy");
        fort = GameObject.Find("Fort");
    }

    private void DefaultValues()
    {
        hunger = maxHunger;
        thirst = maxThirst;
        energy = maxEnergy;
        speed = startingSpeed;
        age = startingAge;
    }

    private void BeginLife()
    {
        StartCoroutine(GainAge());
        StartCoroutine(DepleteHunger());
        StartCoroutine(DepleteThirst());
        StartCoroutine(DepleteEnergy());
        StartCoroutine(EnemySpawner());
    }

    void Update()
    {
        BehaviorTreeFunc(); //Behaviors
        PriorityList(); //Priority of Needs

        TextValues(); //TMP Text
        LifeBars(); //Var Bars

        AgeManagement(); //Life Manager
    }

    private void TextValues()
    {
        TMP_Values[0].text = "Hunger " + hunger;
        TMP_Values[1].text = "Thirst " + thirst;
        TMP_Values[2].text = "Energy " + energy;
        TMP_Values[3].text = "Age " + age;
        TMP_Values[4].text = "Speed " + speed;
        TMP_Values[5].text = "State " + currentBehavior;
    }

    private void LifeBars()
    {
        Life_Bars[0].fillAmount = (float)hunger / maxHunger;
        Life_Bars[1].fillAmount = (float)thirst / maxThirst;
        Life_Bars[2].fillAmount = (float)energy / maxEnergy;
        Life_Bars[3].fillAmount = (float)age / maxAge;
        Life_Bars[4].fillAmount = (float)speed / startingSpeed;
        Life_Bars[5].fillAmount = 1;
    }

    private void PriorityList()
    {
        if (currentBehavior == Behaviors.Dead || isReplenishing) return;

        if (isEnemyPresent)
        {
            currentBehavior = Behaviors.Shelter;
            return;
        }
        if (hunger <= maxHunger / 1.5f) //Below or Equal to 66.6%
        {
            currentBehavior = Behaviors.Hungry;
        }
        else if (thirst <= maxThirst / 2) //Below or Equal to 50%
        {
            currentBehavior = Behaviors.Thirsty;
        }
        else if (energy <= maxEnergy / 3f) //Below or Equal to 33.3%
        {
            currentBehavior = Behaviors.Asleep;
        }
        else if (age < maxAge)
        {
            currentBehavior = Behaviors.Idle;
        }
        else
        {
            currentBehavior = Behaviors.Dead;
        }
    }

    //Movement
    private void MoveAnimal(Vector3 destination)
    {
        float step = speed * Time.deltaTime;
        Vector3 newPos = Vector3.MoveTowards(transform.position, destination, step);
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
    }

    //Collision
    private bool IsTouching(GameObject interactable)
    {
        return Vector3.Distance(transform.position, interactable.transform.position) < 0.5f;
    }

    private void BehaviorTreeFunc()
    {
        switch (currentBehavior)
        {
            case Behaviors.Idle:
                MoveAnimal(field.transform.position);
                break;
            case Behaviors.Hungry:
                MoveAnimal(foodSource.transform.position);
                if (!isReplenishing && hunger != maxHunger && IsTouching(foodSource)) { StartCoroutine(AddHunger()); }
                break;
            case Behaviors.Thirsty:
                MoveAnimal(waterSource.transform.position);
                if (!isReplenishing && thirst != maxThirst && IsTouching(waterSource)) { StartCoroutine(AddThirst()); }
                break;
            case Behaviors.Asleep:
                MoveAnimal(home.transform.position);
                if (!isReplenishing && energy != maxEnergy && IsTouching(home)) { StartCoroutine(AddEnergy()); }
                break;
            case Behaviors.Shelter:
                MoveAnimal(fort.transform.position);
                break;
            case Behaviors.Dead:
                hunger = 0;
                thirst = 0;
                energy = 0;
                age = 15;
                speed = 0;

                animalImage.sprite = deadApperance;

                TextValues();
                LifeBars();

                Time.timeScale = 0f; //End Simulation
                break;
        }
    }

    //Hungry
    private IEnumerator DepleteHunger()
    {
        int Interval = Random.Range(1, 10);
        int hungerLoss = 10;

        yield return new WaitForSeconds(Interval);
        hunger -= hungerLoss;
        hunger = Mathf.Clamp(hunger, 0, maxHunger);

        StartCoroutine(DepleteHunger());
    }

    private IEnumerator AddHunger()
    {
        isReplenishing = true;
        yield return new WaitForSeconds(3f);
        hunger = maxHunger;

        if (hunger == maxHunger)
        {
            currentBehavior = Behaviors.Idle;
        }
        isReplenishing = false;
    }

    //Thirsty
    private IEnumerator DepleteThirst()
    {
        int Interval = Random.Range(1, 10);
        int thirstLoss = 10;

        yield return new WaitForSeconds(Interval);
        thirst -= thirstLoss;
        thirst = Mathf.Clamp(thirst, 0, maxThirst);

        StartCoroutine(DepleteThirst());
    }

    private IEnumerator AddThirst()
    {
        isReplenishing = true;
        yield return new WaitForSeconds(2f);
        thirst = maxThirst;
        
        if (thirst == maxThirst)
        {
            currentBehavior = Behaviors.Idle;
        }
        isReplenishing = false;
    }

    //Energy
    private IEnumerator DepleteEnergy()
    {
        int Interval = Random.Range(1, 10);
        int energyLoss = 10;

        yield return new WaitForSeconds(Interval);
        energy -= energyLoss;
        energy = Mathf.Clamp(energy, 0, maxEnergy);

        StartCoroutine(DepleteEnergy());
    }

    private IEnumerator AddEnergy()
    {
        isReplenishing = true;
        yield return new WaitForSeconds(8f);
        energy = maxEnergy;

        if (energy == maxEnergy)
        {
            currentBehavior = Behaviors.Idle;
        }
        isReplenishing = false;
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
        speed = startingSpeed - age;
        if (age >= maxAge)
        {
            currentBehavior = Behaviors.Dead;
        }
    }
    //Enemy
    private IEnumerator EnemySpawner()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(20f, 40f));

            isEnemyPresent = true;
            enemy.SetActive(true);

            Vector3 offScreenPos = transform.position + Vector3.left * 10f;
            enemy.transform.position = offScreenPos;

            float followSpeed = 3f;

            while (true)
            {
                Vector3 direction = (transform.position - enemy.transform.position).normalized;
                enemy.transform.position += direction * followSpeed * Time.deltaTime;

                if (Vector3.Distance(enemy.transform.position, transform.position) < 0.5f && Vector3.Distance(transform.position, fort.transform.position) > 0.5f)
                {
                    currentBehavior = Behaviors.Dead;
                    enemy.SetActive(false);
                    isEnemyPresent = false;
                    yield break;
                }

                if (Vector3.Distance(transform.position, fort.transform.position) < 0.5f)
                {

                    if (Vector3.Distance(enemy.transform.position, transform.position) < 3f)
                    {
                        break; 
                    }
                }

                yield return null;
            }

            float circleTime = Random.Range(5f, 10f);
            float circleSpeed = 180f;
            float radius = 1.5f;
            float angle = Mathf.Atan2(
                enemy.transform.position.y - transform.position.y, enemy.transform.position.x - transform.position.x) * Mathf.Rad2Deg;

            float t = 0f;
            while (t < circleTime)
            {
                t += Time.deltaTime;
                angle += circleSpeed * Time.deltaTime;

                float rad = angle * Mathf.Deg2Rad;
                Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
                enemy.transform.position = transform.position + offset;

                yield return null;
            }

            Vector3 retreatPos = transform.position + Vector3.right * 10f;
            float retreatDuration = 2f;
            float retreatTimer = 0f;
            Vector3 startPos = enemy.transform.position;

            while (retreatTimer < retreatDuration)
            {
                retreatTimer += Time.deltaTime;
                enemy.transform.position = Vector3.Lerp(startPos, retreatPos, retreatTimer / retreatDuration);
                yield return null;
            }

            enemy.SetActive(false);
            isEnemyPresent = false;
        }
    }
}
