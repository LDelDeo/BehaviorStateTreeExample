using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using static UnityEngine.EventSystems.EventTrigger;
using Unity.VisualScripting;
using UnityEngine.InputSystem.EnhancedTouch;

public class EnemyBehaviorTree : MonoBehaviour
{
    private BehaviorTree behaviorTreeScript;
    public GameObject animal;
    private Coroutine activeCoroutine;
    public enum Behaviors
    {
        Idle, Chase , Feast, Retreat
    }

    public Behaviors currentBehavior;

    void Start()
    {
        behaviorTreeScript = animal.GetComponent<BehaviorTree>();

    }

    void Update()
    {
        BehaviorTreeFunc();
    }
    private void BehaviorTreeFunc()
    {
        if (activeCoroutine != null) return;

        switch (currentBehavior)
        {
            case Behaviors.Idle:
                StartCoroutine(DepleteHunger());
                break;
            case Behaviors.Chase:
                StartCoroutine(Follow());
                break;
            
            case Behaviors.Feast:
               HubertEats();
                break;
            case Behaviors.Retreat:
                StartCoroutine(Exit());
                break;
           
          
        }
    }

    private IEnumerator Exit()
    {
        Vector3 retreatPos = animal.transform.position + Vector3.right * 10f;
        float retreatDuration = 2f;
        float retreatTimer = 0f;
        Vector3 startPos = this.gameObject.transform.position;

        while (retreatTimer < retreatDuration)
        {
            retreatTimer += Time.deltaTime;
            this.gameObject.transform.position = Vector3.Lerp(startPos, retreatPos, retreatTimer / retreatDuration);
            yield return null;
        }
        behaviorTreeScript.isEnemyPresent = false;
        currentBehavior = Behaviors.Idle;
        activeCoroutine = null;
    }

    private IEnumerator DepleteHunger()
    {
        yield return new WaitForSeconds(Random.Range(20f, 40f));
        behaviorTreeScript.isEnemyPresent = true;
        currentBehavior = Behaviors.Chase;
        activeCoroutine = null;
    }

    private IEnumerator Follow()
    {
        Vector3 offScreenPos = animal.transform.position + Vector3.left * 10f;
        this.gameObject.transform.position = offScreenPos;

        float followSpeed = 3f;

        while (true)
        {
            Vector3 direction = (animal.transform.position - this.gameObject.transform.position).normalized;
            this.gameObject.transform.position += direction * followSpeed * Time.deltaTime;

            if (Vector3.Distance(this.gameObject.transform.position, animal.transform.position) < 0.5f && Vector3.Distance(animal.transform.position, behaviorTreeScript.fort.transform.position) > 0.5f)
            {
                currentBehavior = Behaviors.Feast;
                yield break;
            }

            if (Vector3.Distance(animal.transform.position, behaviorTreeScript.fort.transform.position) < 0.5f)
            {

                if (Vector3.Distance(this.gameObject.transform.position, animal.transform.position) < 3f)
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
        this.gameObject.transform.position.y - animal.transform.position.y, this.gameObject.transform.position.x - animal.transform.position.x) * Mathf.Rad2Deg;

        float t = 0f;
        while (t < circleTime)
        {
            t += Time.deltaTime;
            angle += circleSpeed * Time.deltaTime;

            float rad = angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
            this.gameObject.transform.position = animal.transform.position + offset;

            yield return null;
        }
        currentBehavior = Behaviors.Retreat;
        activeCoroutine = null;
    }

    

    private void HubertEats() 
    {
        behaviorTreeScript.Dead();
        behaviorTreeScript.isEnemyPresent = false;
    }
    
}
