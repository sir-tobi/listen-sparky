using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WolfController : MonoBehaviour
{
    public Transform visualTransform;
    public TextMesh paralizationCounterMesh;
    public ParticleSystem paralizationParticleSystem;
    public float BiteRange;
    public int ParalizationTime;
    public bool isPatrolling;
    public Transform patroullingTarget1;
    public Transform patroullingTarget2;
    public bool isChasingSheep;
    public Vector3 originPosition;

    private float paralizationCounter;
    private NavMeshAgent agent;
    private Transform currentTarget;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        originPosition = transform.position;

        if (ParalizationTime == 0)
        {
            ParalizationTime = 15;
        }

        if (isPatrolling)
        {
            currentTarget = patroullingTarget1;
            agent.SetDestination(currentTarget.position);

            patroullingTarget1.GetComponent<MeshRenderer>().enabled = false;
            patroullingTarget2.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    private void FixedUpdate()
    {
        if (paralizationCounter > 0f)
        {
            ShowParalization();
            return;
        }

        paralizationCounterMesh.text = "";
        paralizationParticleSystem.Stop();

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, BiteRange);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.GetComponent<SheepController>())
            {
                SheepController sc = hitCollider.GetComponent<SheepController>();
                sc.onDeathPartcileSystemTransform.parent = null;
                sc.onDeathPartcileSystem.Play();
                sc.gameObject.SetActive(false);
                sc.transform.position = new Vector3(3000, 1000, 1000);
                LevelController.Instance.SheepDied();
            }
        }
    }

    private void Update()
    {
        paralizationCounter -= Time.deltaTime;

        if (!isParalized())
        {
            agent.isStopped = false;
        }

        if (isPatrolling)
        {
            Patrol();
        }

        if (isChasingSheep)
        {
            ChaseSheep();
        }
    }

    private void Patrol()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    if (currentTarget == patroullingTarget1)
                    {
                        currentTarget = patroullingTarget2;
                    } else
                    {
                        currentTarget = patroullingTarget1;
                    }

                    agent.SetDestination(currentTarget.position);
                }
            }
        }
    }

    private void ChaseSheep()
    {
        if (LevelController.Instance.currentLevel.levelIndex == 8 && !LevelController.Instance.completedDialogue.activeSelf)
        {
            agent.SetDestination(Global.Instance.sheeps[0].position);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, BiteRange);
    }

    private void ShowParalization()
    {
        int counterInt = (int)paralizationCounter;
        paralizationCounterMesh.text = counterInt.ToString();
    }

    public void Paralize()
    {
        agent.isStopped = true;
        paralizationCounter = ParalizationTime;
        paralizationParticleSystem.Play();

    }

    public void EndParalization()
    {
        agent.isStopped = false;
        paralizationCounter = 0f;
        paralizationParticleSystem.Stop();
    }

    public bool isParalized ()
    {
        return paralizationCounter > 0f;
    }
}
