using System.Linq;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public static class BoidHelper
{
	public static float3 Calculate(BoidData boidData, ref BoidTypeBlob blob, BoidData otherBoidData, float distance, BoidTank tank)
	{
		float3 movement = float3.zero;
		Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);

		if (boidData.hash != otherBoidData.hash)
		{
			ref var behaviors = ref blob.behaviors;
			for (int i = 0; i < behaviors.Length; i++)
			{
				var behavior = behaviors[i];
				if (distance > behavior.radius * behavior.radius)
					continue;

				switch (behavior.behavior)
				{
					case EBoidsBehaviour.alignment:
						movement += otherBoidData.velocity * behavior.weight;
						break;
					case EBoidsBehaviour.cohesion:
						movement += (otherBoidData.position - boidData.position) * behavior.weight;
						break;
					case EBoidsBehaviour.separation:
						movement += (boidData.position - otherBoidData.position) * behavior.weight;
						break;
				}
			}
		}

		return movement;
	}

	public static float3 CalculateOnce(EBoidsBehaviour boidsBehaviour, BoidData boidData, ref BoidTypeBlob blob, BoidTank tank)
	{
		float3 movement = float3.zero;
		Unity.Mathematics.Random random = new Unity.Mathematics.Random((uint)System.DateTime.Now.Ticks);

		ref var behaviors = ref blob.behaviors;
		for (int i = 0; i < behaviors.Length; i++)
		{
			var behavior = behaviors[i];
			if (behavior.behavior != boidsBehaviour)
				continue;

			switch (behavior.behavior)
			{
				case EBoidsBehaviour.wander:
					float3 wanderMovement = new float3(
						random.NextFloat(-1f, 1f),
						random.NextFloat(-1f, 1f),
						random.NextFloat(-1f, 1f)
					);
					movement += wanderMovement * behavior.weight;
					break;
				case EBoidsBehaviour.avoidObstacle:
					float lookAhead = 15f; // Distance to look ahead for obstacles
					float3 futurePosition = boidData.position + math.normalize(boidData.velocity) * lookAhead;
					float margin = 1.5f; // Margin to avoid obstacles
					if (futurePosition.x < tank.tankMin.x + margin || futurePosition.x > tank.tankMax.x - margin ||
						futurePosition.y < tank.tankMin.y + margin || futurePosition.y > tank.tankMax.y - margin ||
						futurePosition.z < tank.tankMin.z + margin || futurePosition.z > tank.tankMax.z - margin)
					{
						movement += (boidData.position - futurePosition) * behavior.weight;
					}
					break;
			}
		}

		return movement;
	}

	public static int Hash(int3 cell)
	{
		unchecked
		{
			int x = cell.x * 73856093;
			int y = cell.y * 19349663;
			int z = cell.z * 83492791;
			return x ^ y ^ z;
		}
	}

	public static int3 GetCell(float3 pos, float cellSize)
	{
		return (int3)math.floor(pos / cellSize);
	}
}