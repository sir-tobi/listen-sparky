using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimationController : MonoBehaviour
{
    public float jumpHeight;
    public bool doesSillyJumps;
    private Transform animationTransform;
    private NavMeshAgent agent;
    private float step = 0.07f;
    private float count;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (GetComponent<SheepController>())
        {
            animationTransform = GetComponent<SheepController>().visualTransform;
        }
        if (GetComponent<SheepdogController>())
        {
            animationTransform = GetComponent<SheepdogController>().visualTransform;
        }
        if (GetComponent<ChickenController>())
        {
            animationTransform = GetComponent<ChickenController>().visualTransform;
        }
        if (GetComponent<WolfController>())
        {
            animationTransform = GetComponent<WolfController>().visualTransform;
        }

        animationTransform.position = new Vector3(animationTransform.position.x, animationTransform.position.y + step * 35, animationTransform.position.z);
        count += Random.Range(0f, 1f);
    }

    private void Update()
    {
        count += Time.deltaTime * 3f;

        if ((int)(count) % 2 == 0)
        {
            animationTransform.position += Vector3.up * (step * Random.Range(0.9f, 1.1f));
            
        } else
        {
            animationTransform.position += Vector3.down * (step * Random.Range(0.9f, 1.1f));
        }
    }

}
