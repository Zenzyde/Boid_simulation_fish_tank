using Unity.Mathematics;
using Unity.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Transforms;
using System.Diagnostics;

[BurstCompile]
public partial struct BoidECSBehaviorSimulation : ISystem
{
	private NativeParallelMultiHashMap<int, GridBoid> map;

	public void OnCreate(ref SystemState state)
	{
		map = new NativeParallelMultiHashMap<int, GridBoid>(100000, Allocator.Persistent);
	}

	public void OnDestroy(ref SystemState state)
	{
		map.Dispose();
	}

	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		map.Clear();

		var cellSize = 2f;

		var buldJob = new BuildHashMapJob
		{
			Map = map.AsParallelWriter(),
			CellSize = cellSize
		};

		state.Dependency = buldJob.ScheduleParallel(state.Dependency);

		var simulateJob = new BoidECSBehaviorBurstSimulation
		{
			Map = map,
			Tank = SystemAPI.GetSingleton<BoidTank>(),
			DeltaTime = SystemAPI.Time.DeltaTime,
			CellSize = cellSize
		};

		state.Dependency = simulateJob.ScheduleParallel(state.Dependency);

		// map.Clear();

		// float cellSize = 2f;

		// var occupiedCells = new NativeHashSet<int3>(128, Allocator.Temp);

		// // Create hashmap
		// foreach (var (boid, entity) in SystemAPI.Query<RefRO<BoidData>>().WithEntityAccess())
		// {
		// 	int3 cell = BoidHelper.GetCell(boid.ValueRO.position, cellSize);
		// 	occupiedCells.Add(cell);
		// 	int hash = BoidHelper.Hash(cell);

		// 	map.Add(hash, entity);
		// }

		// // foreach (var cell in occupiedCells)
		// // {
		// // 	DrawCell(cell, cellSize, UnityEngine.Color.yellow);
		// // }

		// occupiedCells.Dispose();

		// var tank = SystemAPI.GetSingleton<BoidTank>();
		// // Run simulation
		// foreach (var (boid, species, localTransform) in SystemAPI.Query<RefRW<BoidData>, RefRO<BoidSpecies>, RefRW<LocalTransform>>())
		// {
		// 	ref var blob = ref species.ValueRO.type.Value;

		// 	float3 pos = boid.ValueRO.position;
		// 	int3 cell = BoidHelper.GetCell(pos, cellSize);

		// 	float3 movement = float3.zero;

		// 	int count = 0;

		// 	// Check surrounding 27 cells (3x3x3)
		// 	for (int x = -1; x <= 1; x++)
		// 	{
		// 		for (int y = -1; y <= 1; y++)
		// 		{
		// 			for (int z = -1; z <= 1; z++)
		// 			{
		// 				int3 neighborCell = cell + new int3(x, y, z);
		// 				int hash = BoidHelper.Hash(neighborCell);

		// 				if (map.TryGetFirstValue(hash, out Entity entity, out var iterator))
		// 				{
		// 					do
		// 					{
		// 						var otherBoidEntity = SystemAPI.GetComponent<BoidData>(entity);
		// 						otherBoidEntity.position = SystemAPI.GetComponent<LocalTransform>(entity).Position;
		// 						otherBoidEntity.hash = BoidHelper.Hash(BoidHelper.GetCell(otherBoidEntity.position, cellSize));

		// 						float3 offset = otherBoidEntity.position - boid.ValueRO.position;
		// 						float dist = math.length(offset);

		// 						if (dist > 0)
		// 						{
		// 							movement += BoidHelper.Calculate(boid.ValueRO, ref blob, otherBoidEntity, dist, tank);
		// 							count++;
		// 						}
		// 					} while (map.TryGetNextValue(out entity, ref iterator));
		// 				}
		// 			}
		// 		}
		// 	}

		// 	movement += BoidHelper.CalculateOnce(EBoidsBehaviour.wander, boid.ValueRO, ref blob, tank);
		// 	movement += BoidHelper.CalculateOnce(EBoidsBehaviour.avoidObstacle, boid.ValueRO, ref blob, tank);

		// 	// Average forces from neighboring boids
		// 	movement /= count > 0 ? count : 1;
		// 	boid.ValueRW.velocity += movement * SystemAPI.Time.DeltaTime;

		// 	// Clamp velocity to min/max speed
		// 	float currentSpeed = math.length(boid.ValueRW.velocity);
		// 	if (currentSpeed > blob.maxSpeed)
		// 	{
		// 		boid.ValueRW.velocity = math.normalize(boid.ValueRW.velocity) * blob.maxSpeed;
		// 	}
		// 	else if (currentSpeed < blob.minSpeed && currentSpeed > 0)
		// 	{
		// 		boid.ValueRW.velocity = math.normalize(boid.ValueRW.velocity) * blob.minSpeed;
		// 	}

		// 	boid.ValueRW.position += boid.ValueRW.velocity * SystemAPI.Time.DeltaTime;
		// 	boid.ValueRW.hash = BoidHelper.Hash(BoidHelper.GetCell(boid.ValueRW.position, cellSize));
		// 	localTransform.ValueRW.Position = boid.ValueRW.position;
		// 	// UnityEngine.Debug.DrawRay(boid.ValueRW.position, boid.ValueRW.velocity * 5, UnityEngine.Color.green, 0.1f);

		// 	quaternion current = localTransform.ValueRO.Rotation;
		// 	quaternion target = quaternion.LookRotationSafe(math.normalize(boid.ValueRW.velocity), math.up());

		// 	float maxTurnSpeed = math.radians(90f) * SystemAPI.Time.DeltaTime; // 90 degrees per second
		// 	float angle = math.acos(math.clamp(math.dot(current.value, target.value), -1f, 1f)) * 2f;

		// 	float t = angle > 0f ? math.min(1f, maxTurnSpeed / angle) : 1f;
		// 	localTransform.ValueRW.Rotation = math.slerp(current, target, t);
		// }
	}

	private static void DrawCell(int3 cell, float cellSize, UnityEngine.Color color)
	{
		float3 min = new float3(cell.x * cellSize, cell.y * cellSize, cell.z * cellSize);
		float3 max = min + new float3(cellSize);

		float3 p000 = new(min.x, min.y, min.z);
		float3 p100 = new(max.x, min.y, min.z);
		float3 p110 = new(max.x, max.y, min.z);
		float3 p010 = new(min.x, max.y, min.z);

		float3 p001 = new(min.x, min.y, max.z);
		float3 p101 = new(max.x, min.y, max.z);
		float3 p111 = new(max.x, max.y, max.z);
		float3 p011 = new(min.x, max.y, max.z);

		// Bottom
		UnityEngine.Debug.DrawLine(p000, p100, color);
		UnityEngine.Debug.DrawLine(p100, p110, color);
		UnityEngine.Debug.DrawLine(p110, p010, color);
		UnityEngine.Debug.DrawLine(p010, p000, color);

		// Top
		UnityEngine.Debug.DrawLine(p001, p101, color);
		UnityEngine.Debug.DrawLine(p101, p111, color);
		UnityEngine.Debug.DrawLine(p111, p011, color);
		UnityEngine.Debug.DrawLine(p011, p001, color);

		// Vertical
		UnityEngine.Debug.DrawLine(p000, p001, color);
		UnityEngine.Debug.DrawLine(p100, p101, color);
		UnityEngine.Debug.DrawLine(p110, p111, color);
		UnityEngine.Debug.DrawLine(p010, p011, color);
	}
}

[BurstCompile]
public partial struct BoidECSBehaviorBurstSimulation : IJobEntity
{
	[ReadOnly]
	public NativeParallelMultiHashMap<int, GridBoid> Map;

	[ReadOnly]
	public BoidTank Tank;

	public float DeltaTime;
	public float CellSize;

	void Execute(ref BoidData boid, in BoidSpecies species, ref LocalTransform transform)
	{
		ref var blob = ref species.type.Value;

		float3 movement = float3.zero;
		int count = 0;

		int3 cell = BoidHelper.GetCell(boid.position, CellSize);

		int cellRange = (int)math.ceil(blob.maxBehaviorRadius / CellSize);

		for (int x = -cellRange; x <= cellRange; x++)
		{
			for (int y = -cellRange; y <= cellRange; y++)
			{
				for (int z = -cellRange; z <= cellRange; z++)
				{
					int hash = BoidHelper.Hash(cell + new int3(x, y, z));

					if (Map.TryGetFirstValue(hash, out GridBoid other, out var it))
					{
						do
						{
							float3 offset = other.position - boid.position;
							float dist = math.lengthsq(offset);

							if (dist > 0f)
							{
								movement += BoidHelper.Calculate(boid, ref blob, new BoidData { position = other.position, velocity = other.velocity, hash = other.hash }, dist, Tank);
								count++;
							}

						} while (Map.TryGetNextValue(out other, ref it));
					}
				}
			}
		}

		movement += BoidHelper.CalculateOnce(EBoidsBehaviour.wander, boid, ref blob, Tank);

		movement += BoidHelper.CalculateOnce(EBoidsBehaviour.avoidObstacle, boid, ref blob, Tank);

		movement /= math.max(count, 1);

		boid.velocity += movement * DeltaTime;

		float speed = math.length(boid.velocity);

		if (speed > blob.maxSpeed)
			boid.velocity = math.normalize(boid.velocity) * blob.maxSpeed;
		else if (speed < blob.minSpeed && speed > 0f)
			boid.velocity = math.normalize(boid.velocity) * blob.minSpeed;

		boid.position += boid.velocity * DeltaTime;
		boid.hash = BoidHelper.Hash(BoidHelper.GetCell(boid.position, CellSize));

		transform.Position = boid.position;

		quaternion target = quaternion.LookRotationSafe(math.normalize(boid.velocity), math.up());

		float maxTurn = math.radians(90f) * DeltaTime;

		float angle = math.acos(math.clamp(math.dot(transform.Rotation.value, target.value), -1f, 1f)) * 2f;

		float t = angle > 0 ? math.min(1f, maxTurn / angle) : 1f;

		transform.Rotation = math.slerp(transform.Rotation, target, t);
	}
}

[BurstCompile]
public partial struct BuildHashMapJob : IJobEntity
{
	public NativeParallelMultiHashMap<int, GridBoid>.ParallelWriter Map;

	public float CellSize;

	void Execute(Entity entity, in LocalTransform transform, in BoidData boid)
	{
		int hash = BoidHelper.Hash(
			BoidHelper.GetCell(transform.Position, CellSize));

		Map.Add(hash, new GridBoid { position = transform.Position, velocity = boid.velocity, entity = entity, hash = hash });
	}
}

public struct GridBoid
{
	public float3 position;
	public float3 velocity;
	public Entity entity;
	public int hash;
}