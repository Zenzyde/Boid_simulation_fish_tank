using Unity.Mathematics;
using Unity.Entities;
using Unity.Collections;

public struct BoidData : IComponentData
{
	public float3 position;
	public float3 velocity;
	public int hash;
	public float speed;
}

public struct BoidBehaviourBlob
{
	public EBoidsBehaviour behavior;
	public float radius;
	public float weight;
}

public struct BoidTypeBlob
{
	public BlobArray<BoidBehaviourBlob> behaviors;
	public float minSpeed;
	public float maxSpeed;
	public float maxBehaviorRadius;
}

public struct BoidSpecies : IComponentData
{
	public BlobAssetReference<BoidTypeBlob> type;
	public FixedString64Bytes speciesName;
}

public enum EBoidsBehaviour
{
	alignment,
	cohesion,
	separation,
	wander,
	avoidObstacle
}