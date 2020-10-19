using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FinishLevel : MonoBehaviour
{
    private void FixedUpdate()
    {
        CheckForCollisions();
    }

    private void CheckForCollisions()
    {
        Collider[] hitColliders = Physics.OverlapBox(gameObject.transform.position, transform.localScale / 2, Quaternion.identity);
        int i = 0;
      
        while (i < hitColliders.Length)
        {
            if (hitColliders[i].GetComponent<SheepController>())
            {
                SheepController sc = hitColliders[i].GetComponent<SheepController>();
                sc.onDeathPartcileSystemTransform.parent = null;
                sc.onDeathPartcileSystem.Play();
                sc.gameObject.SetActive(false);
                sc.transform.position = new Vector3(1000, 1000, 1000);
                sc.GetComponent<NavMeshAgent>().Warp(new Vector3(1000, 1000, 1000));
                LevelController.Instance.SheepReachedGoal();
            }
            i++;
        }
    }
}
