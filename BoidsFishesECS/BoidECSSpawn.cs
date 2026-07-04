using Unity.Mathematics;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Physics;
using Unity.Collections;
using Unity.Jobs;

[BurstCompile]
public partial struct BoidECSSpawn : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		if (!SystemAPI.HasSingleton<BoidSpawner>())
			return;

		// var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

		var spawner = SystemAPI.GetSingleton<BoidSpawner>();
		if (spawner.Prefab == Entity.Null)
			return;

		var tank = SystemAPI.GetSingleton<BoidTank>();
		var ecbSingleton = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>();

		var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);


		foreach (var (request, entity) in SystemAPI.Query<RefRO<SpawnRequest>>().WithEntityAccess())
		{
			var job = new BoidECSSpawnJob
			{
				ecb = ecb.AsParallelWriter(),
				prefab = spawner.Prefab,
				tank = tank,
				spawner = spawner,
				Seed = (uint)System.DateTime.Now.Ticks,
				spawnRequestEntity = entity
			};
			state.Dependency = job.Schedule(request.ValueRO.Amount, 64, state.Dependency);
		}
		// foreach (var (request, entity) in SystemAPI.Query<RefRO<SpawnRequest>>().WithEntityAccess())
		// {
		// 	var entities = new NativeArray<Entity>(request.ValueRO.Amount, Allocator.Temp);
		// 	ecb.Instantiate(spawner.Prefab, entities);
		// 	for (int i = 0; i < request.ValueRO.Amount; i++)
		// 	{
		// 		Entity boid = entities[i];

		// 		float margin = 3f; // Margin to avoid spawning too close to the tank boundaries

		// 		float3 spawnPos = new float3(
		// 			UnityEngine.Random.Range(tank.tankMin.x + margin, tank.tankMax.x - margin),
		// 			UnityEngine.Random.Range(tank.tankMin.y + margin, tank.tankMax.y - margin),
		// 			UnityEngine.Random.Range(tank.tankMin.z + margin, tank.tankMax.z - margin));

		// 		ecb.AddComponent(boid, new BoidSpecies { type = spawner.Species, speciesName = spawner.SpeciesName });
		// 		ecb.SetComponent(boid, LocalTransform.FromPositionRotationScale(spawnPos, quaternion.identity, 1f));
		// 		ecb.AddComponent(boid, new BoidData
		// 		{
		// 			position = spawnPos,
		// 			speed = UnityEngine.Random.Range(spawner.Species.Value.minSpeed, spawner.Species.Value.maxSpeed),
		// 			velocity = math.normalize(new float3(
		// 				UnityEngine.Random.Range(-1f, 1f),
		// 				UnityEngine.Random.Range(-1f, 1f),
		// 				UnityEngine.Random.Range(-1f, 1f))) * UnityEngine.Random.Range(spawner.Species.Value.minSpeed, spawner.Species.Value.maxSpeed),
		// 			hash = BoidHelper.Hash(BoidHelper.GetCell(spawnPos, 2f))
		// 		});
		// 	}
		// 	ecb.DestroyEntity(entity);
		// }

		// ecb.Playback(state.EntityManager);
		// ecb.Dispose();
	}
}

[BurstCompile]
public partial struct BoidECSSpawnJob : IJobParallelFor
{
	public EntityCommandBuffer.ParallelWriter ecb;
	public Entity prefab;
	public BoidTank tank;
	public BoidSpawner spawner;
	public uint Seed;
	public Entity spawnRequestEntity;

	public void Execute(int index)
	{
		Entity boid = ecb.Instantiate(index, prefab);

		var random = Unity.Mathematics.Random.CreateFromIndex(Seed + (uint)index);

		// Set initial position and velocity
		float margin = 3f; // Margin to avoid spawning too close to the tank boundaries
		float3 spawnPos = new float3(
			random.NextFloat(tank.tankMin.x + margin, tank.tankMax.x - margin),
			random.NextFloat(tank.tankMin.y + margin, tank.tankMax.y - margin),
			random.NextFloat(tank.tankMin.z + margin, tank.tankMax.z - margin));

		ecb.AddComponent(index, boid, new BoidSpecies { type = spawner.Species, speciesName = spawner.SpeciesName });
		ecb.SetComponent(index, boid, LocalTransform.FromPositionRotationScale(spawnPos, quaternion.identity, 1f));
		ecb.AddComponent(index, boid, new BoidData
		{
			position = spawnPos,
			speed = random.NextFloat(spawner.Species.Value.minSpeed, spawner.Species.Value.maxSpeed),
			velocity = math.normalize(new float3(
				random.NextFloat(-1f, 1f),
				random.NextFloat(-1f, 1f),
				random.NextFloat(-1f, 1f))) * random.NextFloat(spawner.Species.Value.minSpeed, spawner.Species.Value.maxSpeed),
			hash = BoidHelper.Hash(BoidHelper.GetCell(spawnPos, 2f))
		});
		if (index == 0)
		{
			ecb.DestroyEntity(index, spawnRequestEntity);
		}
	}
}

public struct SpawnRequest : IComponentData
{
	public int Amount;
}

public struct BoidSpawner : IComponentData
{
	public Entity Prefab;
	public FixedString64Bytes SpeciesName;
	public BlobAssetReference<BoidTypeBlob> Species;
}