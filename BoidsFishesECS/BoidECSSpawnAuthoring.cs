using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class BoidSpawnerAuthoring : MonoBehaviour
{
	public List<BoidEntityTypeSO> BoidSettings;
}

public class BoidSpawnerBaker : Baker<BoidSpawnerAuthoring>
{
	public override void Bake(BoidSpawnerAuthoring authoring)
	{
		if (authoring.BoidSettings == null || authoring.BoidSettings.Count == 0)
		{
			Debug.LogError("BoidSpawnerAuthoring: BoidSettings is null or empty. Please assign at least one BoidEntityTypeSO.");
			return;
		}
		foreach (var boidSetting in authoring.BoidSettings)
		{
			if (boidSetting.prefab == null)
			{
				Debug.LogError("BoidSpawnerAuthoring: BoidSettings.prefab is null. Please assign a prefab.");
				return;
			}

			var builder = new BlobBuilder(Allocator.Temp);

			ref BoidTypeBlob root = ref builder.ConstructRoot<BoidTypeBlob>();

			root.minSpeed = boidSetting.minSpeed;
			root.maxSpeed = boidSetting.maxSpeed;

			BlobBuilderArray<BoidBehaviourBlob> behaviours = builder.Allocate(ref root.behaviors, boidSetting.behaviors.Count);

			float maxBehaviorRadius = 0f;
			for (int i = 0; i < behaviours.Length; i++)
			{
				var source = boidSetting.behaviors[i];

				behaviours[i] = new BoidBehaviourBlob
				{
					behavior = source.behavior,

					radius = UnityEngine.Random.Range(source.minBehaviourRadius, source.maxBehaviourRadius),

					weight = UnityEngine.Random.Range(source.minBehaviourWeight, source.maxBehaviourWeight)
				};
				if (behaviours[i].radius > maxBehaviorRadius)
				{
					maxBehaviorRadius = behaviours[i].radius;
				}
			}
			root.maxBehaviorRadius = maxBehaviorRadius;

			BlobAssetReference<BoidTypeBlob> blob = builder.CreateBlobAssetReference<BoidTypeBlob>(Allocator.Temp);

			builder.Dispose();

			Entity entity = GetEntity(TransformUsageFlags.None);

			AddComponent(entity, new BoidSpawner
			{
				Prefab = GetEntity(boidSetting.prefab, TransformUsageFlags.Dynamic),
				Species = blob,
				SpeciesName = boidSetting.speciesName
			});
		}
	}
}