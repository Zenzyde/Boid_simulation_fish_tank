using System.Text;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BoidsInput : MonoBehaviour
{
	[SerializeField] private Text numBoidsText, instructionsText, boidsInfoCanvas;

	private EntityManager entityManager;

	private Camera cam;

	private int numBoidsToSpawn = 1;

	private EBoidInteractMode boidInteractMode = EBoidInteractMode.spawnordelete;
	private EBoidInteractMode lastBoidInteractMode = EBoidInteractMode.spawnordelete;

	private Entity selectedEntity;

	private Transform tempPivot;
	private CamObserve camObserve;

	enum EBoidInteractMode
	{
		spawnordelete,
		select
	}

	void Start()
	{
		entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
		tempPivot = new GameObject("TempPivot").transform;
		cam = Camera.main;
		camObserve = FindAnyObjectByType<CamObserve>();
	}

	void Update()
	{
		if (boidInteractMode == EBoidInteractMode.spawnordelete)
		{

			// LEFT CLICK → SPAWN
			if (Mouse.current.leftButton.wasPressedThisFrame)
			{
				var spawnEntity = entityManager.CreateEntity();
				entityManager.AddComponentData(spawnEntity, new SpawnRequest
				{
					Amount = numBoidsToSpawn
				});
			}

			// RIGHT CLICK → DELETE
			if (Mouse.current.rightButton.wasPressedThisFrame)
			{
				UnityEngine.Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

				var collisionWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PhysicsSystemGroup>().GetSingleton<PhysicsWorldSingleton>().PhysicsWorld.CollisionWorld;

				var input = new Unity.Physics.RaycastInput
				{
					Start = ray.origin,
					End = ray.origin + ray.direction * 1000f,
					Filter = CollisionFilter.Default
				};

				if (collisionWorld.CastRay(input, out Unity.Physics.RaycastHit hit))
				{
					var entity = collisionWorld.Bodies[hit.RigidBodyIndex].Entity;

					Entity request = entityManager.CreateEntity();
					entityManager.AddComponentData(request, new DeleteRequest
					{
						targetEntity = GetGrandParent(entity)
					});
				}

			}
		}
		else if (boidInteractMode == EBoidInteractMode.select)
		{
			// LEFT CLICK → SELECT
			if (Mouse.current.leftButton.wasPressedThisFrame)
			{
				UnityEngine.Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

				var collisionWorld = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PhysicsSystemGroup>().GetSingleton<PhysicsWorldSingleton>().PhysicsWorld.CollisionWorld;

				var input = new Unity.Physics.RaycastInput
				{
					Start = ray.origin,
					End = ray.origin + ray.direction * 1000f,
					Filter = CollisionFilter.Default
				};

				if (collisionWorld.CastRay(input, out Unity.Physics.RaycastHit hit))
				{
					selectedEntity = collisionWorld.Bodies[hit.RigidBodyIndex].Entity;
					camObserve.SetObservationPivot(tempPivot);
				}
			}

			if (selectedEntity != Entity.Null)
			{
				if (camObserve != null)
				{
					var ltw = entityManager.GetComponentData<LocalToWorld>(selectedEntity);
					tempPivot.position = ltw.Position;
				}
			}
		}

		UpdateBoidsText();
		UpdateNumBoidsToSpawn();
		UpdateInteractionMode();
		UpdateBoidsInfo();
	}

	void UpdateBoidsText()
	{
		int boidCount = entityManager.CreateEntityQuery(typeof(BoidData)).CalculateEntityCount();
		numBoidsText.text = $"No. active boids: {boidCount}";
	}

	void UpdateNumBoidsToSpawn()
	{
		if (Keyboard.current.rKey.isPressed)
			numBoidsToSpawn += 1;
		else if (Keyboard.current.fKey.isPressed)
			numBoidsToSpawn -= 1;

		numBoidsToSpawn = Mathf.Clamp(numBoidsToSpawn, 1, 1000);
		UpdateInteractionInfoCanvas();
	}

	void UpdateInteractionMode()
	{
		if (Keyboard.current.qKey.wasPressedThisFrame || Keyboard.current.eKey.wasPressedThisFrame)
		{
			boidInteractMode = boidInteractMode == EBoidInteractMode.spawnordelete ? EBoidInteractMode.select : EBoidInteractMode.spawnordelete;
			boidsInfoCanvas.text = "";
			boidsInfoCanvas.enabled = false;
		}

		if (lastBoidInteractMode != boidInteractMode)
		{
			lastBoidInteractMode = boidInteractMode;
			selectedEntity = Entity.Null;
			// Reset
			if (camObserve != null)
			{
				camObserve.SetObservationPivot(null);
			}
			UpdateInteractionInfoCanvas();
		}
	}

	T GetBoidInfoRecursive<T>(Entity entity) where T : unmanaged, IComponentData
	{
		if (entityManager.HasComponent<T>(entity))
		{
			return entityManager.GetComponentData<T>(entity);
		}
		else
		{
			var parent = entityManager.GetComponentData<Parent>(entity).Value;
			return GetBoidInfoRecursive<T>(parent);
		}
	}

	Entity GetGrandParent(Entity entity)
	{
		if (entityManager.HasComponent<Parent>(entity))
		{
			var parent = entityManager.GetComponentData<Parent>(entity).Value;
			if (entityManager.HasComponent<Parent>(parent))
			{
				return entityManager.GetComponentData<Parent>(parent).Value;
			}
			else
			{
				return Entity.Null;
			}
		}
		else
		{
			return Entity.Null;
		}
	}

	string Float3ToString(float3 vector)
	{
		return $"(X:{vector.x:F2}, Y:{vector.y:F2}, Z:{vector.z:F2})";
	}

	void UpdateBoidsInfo()
	{
		if (selectedEntity != Entity.Null && entityManager.Exists(selectedEntity))
		{
			var boidData = GetBoidInfoRecursive<BoidData>(selectedEntity);
			var boidSpeciesData = GetBoidInfoRecursive<BoidSpecies>(selectedEntity);

			boidsInfoCanvas.enabled = true;
			boidsInfoCanvas.transform.position = cam.WorldToScreenPoint(new float3(10, 0, 0));
			boidsInfoCanvas.transform.rotation = Quaternion.identity;

			StringBuilder sb = new StringBuilder();

			sb.AppendLine($"Position: {Float3ToString(boidData.position)}");
			sb.AppendLine($"Velocity: {Float3ToString(boidData.velocity)}");
			sb.AppendLine($"Species: {boidSpeciesData.speciesName}");
			for (int i = 0; i < boidSpeciesData.type.Value.behaviors.Length; i++)
			{
				var behavior = boidSpeciesData.type.Value.behaviors[i];
				sb.AppendLine($"	Behavior: Type: {behavior.behavior}, Radius: {behavior.radius}, Weight: {behavior.weight}");
			}
			sb.AppendLine($"	Min Speed: {boidSpeciesData.type.Value.minSpeed}");
			sb.AppendLine($"	Max Speed: {boidSpeciesData.type.Value.maxSpeed}");
			sb.AppendLine($"Speed: {math.length(boidData.velocity)}");

			boidsInfoCanvas.text = sb.ToString();
		}
		else
		{
			boidsInfoCanvas.text = "";
			boidsInfoCanvas.enabled = false;
		}
	}

	void UpdateInteractionInfoCanvas()
	{
		StringBuilder sb = new StringBuilder();
		if (boidInteractMode == EBoidInteractMode.spawnordelete)
		{
			sb.AppendLine($"Current interaction mode: Spawn or Delete");
			sb.AppendLine($"Press Q or E to switch interaction mode.");
			sb.AppendLine($"Hold R/F to increase/decrease the number of boids to spawn.");
			sb.AppendLine($"Amount of boids to spawn on Left-click: {numBoidsToSpawn}");
			sb.AppendLine("Right-click to remove targeted boid.");
			instructionsText.text = sb.ToString();
		}
		else if (boidInteractMode == EBoidInteractMode.select)
		{
			sb.AppendLine($"Current interaction mode: Select");
			sb.AppendLine($"Press Q or E to switch interaction mode.");
			sb.AppendLine("Left-click to select a boid and display info.");
			instructionsText.text = sb.ToString();
		}
	}
}