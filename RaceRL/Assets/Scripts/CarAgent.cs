using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

[RequireComponent(typeof(Rigidbody))]
public class CarAgent : Agent
{
    Rigidbody rb;
    Vector3 initialPosition;
    Quaternion initialRotation;

    [Header("Movement")]
    public float maxSpeed = 10f;
    public float acceleration = 5f;
    public float deceleration = 8f;
    public float turnSpeed = 100f;

    private float currentSpeed = 0f;

    [Header("Sensors")]
    public int numRays = 5; 
    public float rayAngleSpread = 120f;
    public float maxRayDistance = 15f; 
    public LayerMask obstacleMask = ~0; 

    [Header("Rewards")]
    public float forwardRewardFactor = 0.0001f; 

    [Header("Spawn")]
    public Transform spawnPoint;

    public Transform meshFL;
    public Transform meshFR;
    public Transform meshBL;
    public Transform meshBR;

    public GameObject[] checkpoints; 

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        if(rb == null)
        {
            Debug.Log("Rb nullo");
        }

        initialPosition = transform.localPosition;
        initialRotation = transform.localRotation;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        Debug.Log("CarAgent abilitato");
    }



    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode iniziato");

        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogError("Rigidbody mancante su CarAgent!");
                return;
            }
        }

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        currentSpeed = 0f;

        if (spawnPoint != null)
        {
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
        }
        else
        {
            transform.localPosition = initialPosition;
            transform.localRotation = initialRotation;
        }

        foreach (var cp in checkpoints)
        {
            cp.SetActive(true);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (sensor == null)
        {
            Debug.LogError("VectorSensor è NULL - CollectObservations è stata chiamata manualmente!");
            return;
        }

        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
        float maxExpectedSpeed = 10f;
        sensor.AddObservation(Mathf.Clamp(forwardSpeed / maxExpectedSpeed, -1f, 1f));
        float angularY = rb.angularVelocity.y;

        float maxAngularSpeed = 3.14f / 2f;

        sensor.AddObservation(Mathf.Clamp(angularY / maxAngularSpeed, -1f, 1f));


        for (int i = 0; i < numRays; i++)
        {
            float angle = -rayAngleSpread / 2f + (rayAngleSpread / (numRays - 1)) * i;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * transform.forward;
            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, maxRayDistance, obstacleMask))
            {
                sensor.AddObservation(hit.distance / maxRayDistance);
            }
            else
            {
                sensor.AddObservation(1f);
            }
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveInput = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float steerInput = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        float targetSpeed = moveInput * maxSpeed;

        if (Mathf.Abs(moveInput) > 0.01f)
        {
            if (Mathf.Sign(moveInput) == Mathf.Sign(currentSpeed) || Mathf.Abs(currentSpeed) < 0.1f)
            {
                currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, acceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, deceleration * 2f * Time.fixedDeltaTime);
            }
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }


        // --- Movimento ---
        Vector3 velocity = transform.forward * currentSpeed;
        rb.linearVelocity = new Vector3(velocity.x, rb.linearVelocity.y, velocity.z);

        float speedFactor = Mathf.SmoothStep(1f, 0.4f, Mathf.Abs(currentSpeed) / maxSpeed);

        float minSteerSpeed = 1.5f; // soglia minima di velocità per poter sterzare
        if (Mathf.Abs(currentSpeed) < minSteerSpeed)
            steerInput = 0f;

        float turn = steerInput * turnSpeed * speedFactor * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
        UpdateWheels(moveInput, steerInput);

        // --- Ricompense ---
        float steerPenaltyFactor = 0.001f;
        AddReward(-Mathf.Abs(steerInput) * steerPenaltyFactor);
        AddReward(currentSpeed * forwardRewardFactor);
    }





    public override void Heuristic(in ActionBuffers actionsOut)
    {

        var cont = actionsOut.ContinuousActions;
        float move = 0f;
        float steer = 0f;

        if (Input.GetKey(KeyCode.W)) { move = 1f; }
        else if (Input.GetKey(KeyCode.S)) { move = -1f; }

        if (Input.GetKey(KeyCode.D)) { steer = 1f; }
        else if (Input.GetKey(KeyCode.A)) { steer = -1f; }

        cont[0] = move;
        cont[1] = steer;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Checkpoint"))
        {
            AddReward(0.1f);
            other.gameObject.SetActive(false);
            Debug.Log("Checkpoint");
        }

        if (other.gameObject.CompareTag("EndLap"))
        {
            Debug.Log("Giro Terminato");
            EndEpisode();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.red;
        for (int i = 0; i < numRays; i++)
        {
            float angle = -rayAngleSpread / 2f + (rayAngleSpread / (numRays - 1)) * i;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * transform.forward;
            Gizmos.DrawLine(transform.position, transform.position + dir * maxRayDistance);
        }
    }

    void UpdateWheels(float moveInput, float steerInput)
    {
        float rotationAmount = moveInput * 360f * Time.deltaTime;

        Quaternion initialRot = Quaternion.Euler(0f, 270f, 0f);// compensazione mesh

        meshFL.localRotation = initialRot * Quaternion.Euler(0f, steerInput * 10f, 0f);
        meshFR.localRotation = initialRot * Quaternion.Euler(0f, steerInput * 10f, 0f);

        meshBL.localRotation = initialRot * Quaternion.Euler(0f, 0f, rotationAmount);
        meshBR.localRotation = initialRot * Quaternion.Euler(0f, 0f, rotationAmount);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Reset manuale episodio");
            EndEpisode();
        }
    }
}
