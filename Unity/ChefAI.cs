using UnityEngine;
using System.Collections;

public class ChefAI : MonoBehaviour
{
    public float patrolSpeed = 2f;
    public float patrolRange = 5f;
    private Vector3 startPosition;
    private float patrolTimer;

    void Start()
    {
        startPosition = transform.position;
        patrolTimer = 0f;
    }

    void Update()
    {
        patrolTimer += Time.deltaTime;
        float xOffset = Mathf.Sin(patrolTimer) * patrolRange;
        Vector3 target = startPosition + new Vector3(xOffset, 0f, 0f);
        transform.position = Vector3.MoveTowards(transform.position, target, patrolSpeed * Time.deltaTime);

        // Simple waste interaction (e.g., reduce waste_quants when near player)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && Vector3.Distance(transform.position, player.transform.position) < 2f)
        {
            GameManager.Instance.UpdateWaste(-0.1f); // Reduce waste
        }
    }
}