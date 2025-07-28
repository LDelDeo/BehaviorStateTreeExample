using UnityEngine;
using System.Collections;
using TMPro;

public class EnemyBehaviorTree : MonoBehaviour
{
    [Header("Scripts")]
    private BehaviorTree behaviorTreeScript;

    [Header("Prey")]
    private GameObject animal;

    [Header("Coroutines")]
    private Coroutine activeCoroutine;

    [Header("Text")]
    public TMP_Text currentStateText;

    public enum Behaviors
    {
        Idle, Chase, Feast, Retreat
    }

    public Behaviors currentBehavior;

    void Start()
    {
        FindObjects(); //Grabs Components Upon Start
    }

    private void FindObjects()
    {
        animal = GameObject.Find("Animal");
        behaviorTreeScript = animal.GetComponent<BehaviorTree>();
    }

    void Update()
    {
        if (activeCoroutine == null) { BehaviorTreeFunc(); } //If there is no Coroutine Active, Activate one based on Behavior State

        CurrentStateOfBear(); //Updates the Current State/Behavior Text of the Bear
    }

    private void CurrentStateOfBear()
    {
        currentStateText.text = "" + currentBehavior;
    }

    private void BehaviorTreeFunc()
    {
        switch (currentBehavior)
        {
            case Behaviors.Idle:
                activeCoroutine = StartCoroutine(BearIsHungry());
                break;
            case Behaviors.Chase:
                activeCoroutine = StartCoroutine(Follow());
                break;
            case Behaviors.Feast:
                HerbertEats();
                break;
            case Behaviors.Retreat:
                activeCoroutine = StartCoroutine(Exit());
                break;
        }
    }

    private IEnumerator Exit()
    {
        Vector3 retreatPos = animal.transform.position + Vector3.right * 10f;
        float retreatDuration = 2f;
        float retreatTimer = 0f;
        Vector3 startPos = this.transform.position;

        while (retreatTimer < retreatDuration)
        {
            retreatTimer += Time.deltaTime;
            this.transform.position = Vector3.Lerp(startPos, retreatPos, retreatTimer / retreatDuration);
            yield return null;
        }

        behaviorTreeScript.isEnemyPresent = false;
        currentBehavior = Behaviors.Idle;
        activeCoroutine = null;
    }

    // Bear Actions
    private IEnumerator BearIsHungry()
    {
        yield return new WaitForSeconds(Random.Range(20f, 40f));
        behaviorTreeScript.isEnemyPresent = true;
        currentBehavior = Behaviors.Chase;
        activeCoroutine = null;
    }

    private IEnumerator Follow()
    {
        float followSpeed = 3f;

        while (true)
        {
            Vector3 direction = (animal.transform.position - this.transform.position).normalized;
            this.transform.position += direction * followSpeed * Time.deltaTime;

            if (Vector3.Distance(this.transform.position, animal.transform.position) < 0.5f &&
                Vector3.Distance(animal.transform.position, behaviorTreeScript.fort.transform.position) > 0.5f)
            {
                currentBehavior = Behaviors.Feast;
                activeCoroutine = null;
                yield break;
            }

            // If Bear reaches Fort with Animal inside, circle the Fort
            if (Vector3.Distance(animal.transform.position, behaviorTreeScript.fort.transform.position) < 0.5f)
            {
                if (Vector3.Distance(this.transform.position, animal.transform.position) < 3f)
                {
                    break;
                }
            }

            yield return null;
        }

        // Orbit Behavior
        float circleTime = Random.Range(5f, 10f);
        float circleSpeed = 180f;
        float radius = 1.5f;
        float angle = Mathf.Atan2(
            this.transform.position.y - animal.transform.position.y,
            this.transform.position.x - animal.transform.position.x) * Mathf.Rad2Deg;

        float t = 0f;
        while (t < circleTime)
        {
            t += Time.deltaTime;
            angle += circleSpeed * Time.deltaTime;

            float rad = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
            this.transform.position = animal.transform.position + offset;

            yield return null;
        }

        currentBehavior = Behaviors.Retreat;
        activeCoroutine = null;
    }

    private void HerbertEats()
    {
        behaviorTreeScript.Dead();
        behaviorTreeScript.isEnemyPresent = false;
        currentBehavior = Behaviors.Retreat;
    }
}
