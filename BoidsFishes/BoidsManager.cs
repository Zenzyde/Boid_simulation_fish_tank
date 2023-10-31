using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BoidsManager : MonoBehaviour
{
	[SerializeField] private Vector3 gridSize;
	[SerializeField] private GameObject[] boidModels;
	[SerializeField] private float maxVelocity;
	[SerializeField] private List<BoidBehaviorSettings> behaviorSettings;
	[SerializeField] private Text numBoidsText, instructionsText;
	[SerializeField] private bool drawBoxGizmo, drawBoidGizmos;

	private List<BoidsController> activeBoids = new List<BoidsController>();

	private Camera camera;

	private int lastFrameIndex;
	private float[] frameTimeDeltas;

#if UNITY_EDITOR
	void OnDrawGizmos()
	{
		Gizmos.color = Color.blue;
		if (drawBoxGizmo)
			HelperMethods.DrawBoxBounds(transform.position - (gridSize / 2.0f), transform.position + (gridSize / 2.0f));

		if (activeBoids != null && activeBoids.Count > 0 && drawBoidGizmos)
			foreach (BoidsController boid in activeBoids)
				boid.DrawGizmos();
	}
#endif

	void Start()
	{
		UnityEngine.Random.InitState(DateTime.Now.Second);
		numBoidsText.text = string.Format("Num active boids: {0}", activeBoids.Count);

#if UNITY_EDITOR
		instructionsText.text = "Left-click to add new boid.\nShift + Left-Click to add 10 boids.\nRight-click to remove targeted boid.";
#elif UNITY_STANDALONE
		instructionsText.text = "Left-click to add new boid.\nShift + Left-Click to add 10 boids.\nRight - click to remove targeted boid.\nEscape to quit";
#endif

		camera = FindObjectOfType<Camera>();

		frameTimeDeltas = new float[60];
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			int numBoids = 1;
			if (Input.GetKey(KeyCode.LeftShift))
				numBoids = 10;
			SpawnNewBoid(numBoids);
		}
		if (Input.GetMouseButtonDown(1) && activeBoids.Count > 0)
		{
			RemoveBoid();
		}

		if (Input.GetKeyDown(KeyCode.Alpha1))
			drawBoxGizmo = !drawBoxGizmo;

		UpdateFPSArray();
		UpdateNumBoidsText();

#if UNITY_STANDALONE
		if (Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();
#endif
	}

	public void SpawnNewBoid(int numBoids)
	{
		for (int i = 0; i < numBoids; i++)
		{
			GameObject boidObj = Instantiate(boidModels[UnityEngine.Random.Range(0, boidModels.Length)]);
			boidObj.transform.position = new Vector3()
			{
				x = UnityEngine.Random.Range(transform.position.x - gridSize.x / 2.0f, transform.position.x + gridSize.x / 2.0f),
					y = UnityEngine.Random.Range(transform.position.y - gridSize.y / 2.0f, transform.position.y + gridSize.y / 2.0f),
					z = UnityEngine.Random.Range(transform.position.z - gridSize.z / 2.0f, transform.position.z + gridSize.z / 2.0f),
			};
			boidObj.transform.rotation = Quaternion.LookRotation(UnityEngine.Random.onUnitSphere);

			BoidsController boid = boidObj.AddComponent<BoidsController>();
			boid.InitializeController(this, maxVelocity);

			boid.AddBoidBehaviour(new BoidAlign(boid, new BoidBehaviourSettings()
			{
				behaviourRadius = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.alignment).FirstOrDefault().behaviourRadius,
					behaviourGridRadius = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.alignment).FirstOrDefault().behaviourGridRadius,
					behaviourWeight = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.alignment).FirstOrDefault().behaviourWeight
			}));

			boid.AddBoidBehaviour(new BoidCohesion(boid, new BoidBehaviourSettings()
			{
				behaviourRadius = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.cohesion).FirstOrDefault().behaviourRadius,
					behaviourGridRadius = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.cohesion).FirstOrDefault().behaviourGridRadius,
					behaviourWeight = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.cohesion).FirstOrDefault().behaviourWeight
			}));

			boid.AddBoidBehaviour(new BoidSeparate(boid, new BoidBehaviourSettings()
			{
				behaviourRadius = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.separation).FirstOrDefault().behaviourRadius,
					behaviourGridRadius = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.separation).FirstOrDefault().behaviourGridRadius,
					behaviourWeight = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.separation).FirstOrDefault().behaviourWeight
			}));

			boid.AddBoidBehaviour(new BoidWander(boid, new BoidBehaviourSettings()
			{
				behaviourRadius = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.wander).FirstOrDefault().behaviourRadius,
					behaviourGridRadius = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.wander).FirstOrDefault().behaviourGridRadius,
					behaviourWeight = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.wander).FirstOrDefault().behaviourWeight
			}));

			boid.AddBoidBehaviour(new BoidAvoidTank(boid, new BoidBehaviourSettings()
			{
				behaviourRadius = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.avoidTank).FirstOrDefault().behaviourRadius,
					behaviourGridRadius = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.avoidTank).FirstOrDefault().behaviourGridRadius,
					behaviourWeight = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.avoidTank).FirstOrDefault().behaviourWeight
			}));

			boid.AddBoidBehaviour(new BoidAvoidObstacle(boid, new BoidBehaviourSettings()
			{
				behaviourRadius = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.avoidObstacle).FirstOrDefault().behaviourRadius,
					behaviourGridRadius = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.avoidObstacle).FirstOrDefault().behaviourGridRadius,
					behaviourWeight = behaviorSettings.Where(x => x.behavior == EBoidsBehaviour.avoidObstacle).FirstOrDefault().behaviourWeight
			}));

			activeBoids.Add(boid);
		}
	}

	public void RemoveBoid()
	{
		Ray mouseRay = camera.ScreenPointToRay(Input.mousePosition);
		if (Physics.SphereCast(mouseRay.origin, 0.5f, mouseRay.direction, out RaycastHit hit, 5000, LayerMask.GetMask("Boid")))
		{
			Transform boidObject = hit.transform.parent;
			if (boidObject.TryGetComponent(out BoidsController controller))
			{
				activeBoids.Remove(controller);
				Destroy(boidObject.gameObject);
			}
		}
	}

	public bool IsOutsideTank(BoidsController boid)
	{
		Vector3 pos = boid.transform.position;
		return pos.x < transform.position.x - gridSize.x / 2.0f || pos.x > transform.position.x + gridSize.x / 2.0f
			|| pos.y < transform.position.y - gridSize.y / 2.0f || pos.y > transform.position.y + gridSize.y / 2.0f
			|| pos.z < transform.position.z - gridSize.z / 2.0f || pos.z > transform.position.z + gridSize.z / 2.0f;
	}

	public bool IsOutsideTank(Vector3 pos)
	{
		return pos.x < transform.position.x - gridSize.x / 2.0f || pos.x > transform.position.x + gridSize.x / 2.0f
			|| pos.y < transform.position.y - gridSize.y / 2.0f || pos.y > transform.position.y + gridSize.y / 2.0f
			|| pos.z < transform.position.z - gridSize.z / 2.0f || pos.z > transform.position.z + gridSize.z / 2.0f;
	}

	public Vector3 GetRandomTankPosition(BoidsController boid)
	{
		Vector3 rand = transform.position + UnityEngine.Random.insideUnitSphere * 25;

		int randomizationAttempts = 50;
		while (Vector3.Distance(rand, boid.transform.position) < 1.5f && randomizationAttempts > 0)
		{
			rand = transform.position + UnityEngine.Random.insideUnitSphere * 25;
			randomizationAttempts--;
		}

		return rand;
	}

	// FPS calculation ref: https://www.youtube.com/shorts/I2r97r9h074
	void UpdateFPSArray()
	{
		frameTimeDeltas[lastFrameIndex] = Time.deltaTime;
		lastFrameIndex = (lastFrameIndex + 1) % frameTimeDeltas.Length;
	}

	float GetFPS()
	{
		float totalFrameDelta = 0;
		for (int i = 0; i < frameTimeDeltas.Length; i++)
			totalFrameDelta += frameTimeDeltas[i];
		return frameTimeDeltas.Length / totalFrameDelta;
	}

	void UpdateNumBoidsText()
	{
		numBoidsText.text = string.Format("No. boids: {0}\nFPS: {1:000}", activeBoids.Count, GetFPS());
	}
}