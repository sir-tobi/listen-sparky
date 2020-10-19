using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using FrostweepGames.Plugins.GoogleCloud.SpeechRecognition;

public class SheepController : MonoBehaviour
{
    public Transform visualTransform;
    public float ViewDistance;
    public Transform onDeathPartcileSystemTransform;
    public ParticleSystem onDeathPartcileSystem;
    public bool IsInGoal;
    public Vector3 originPosition;
    public Vector3 introTargetVector;

    public float coolDownTimeStamp;
    private Transform dogTransform;
    private SheepdogController dog;
    private List<Transform> sheeps;
    NavMeshAgent agent;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        originPosition = transform.position;
        dogTransform = GameObject.FindGameObjectWithTag("Sheepdog").transform;
        dog = dogTransform.GetComponent<SheepdogController>();

        if (!dogTransform)
        {
            Debug.Log("No sheepdog found!");
        }

        sheeps = new List<Transform>();
        GameObject[] foundSheeps = GameObject.FindGameObjectsWithTag("Sheep");
        foreach (GameObject sheep in foundSheeps)
        {
            sheeps.Add(sheep.transform);
        }
        sheeps.Remove(transform);
    }

    private void Update()
    {
        if (dog.IsAgressive && LevelController.Instance.currentLevel.levelIndex != 0 && Time.time > coolDownTimeStamp)
        {
            Vector3 distanceVector = (transform.position - dogTransform.position).normalized * 3f; ;

            if (Vector3.Distance(transform.position, dogTransform.position) < ViewDistance) {
                Vector3 direction = transform.position - dogTransform.transform.position;
                direction.Normalize();

                coolDownTimeStamp = Time.time + 500f;
                SoundController.Instance.PlaySound("sheep", 0f, UnityEngine.Random.Range(0.5f, 0.7f), UnityEngine.Random.Range(1f, 1.3f));
                agent.SetDestination(transform.position + direction * 17f);
            }
        }

        // Move sheep to each other
        if (sheeps.Count == 0) return;
        Transform nearestSheep = sheeps[0];

        foreach (Transform s in sheeps)
        {
            if (Vector3.Distance(s.position, transform.position) < Vector3.Distance(nearestSheep.position, transform.position))
            {
                nearestSheep = s;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, ViewDistance);
    }
}
