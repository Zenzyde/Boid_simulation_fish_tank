using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

public class BoidECSTankAuthoring : MonoBehaviour
{
	public float3 tankMin = new float3(-50f, -50f, -50f);
	public float3 tankMax = new float3(100f, 100f, 100f);

	void OnDrawGizmosSelected()
	{
		DrawTankBounds();
	}

	void DrawTankBounds()
	{
		Vector3 min = new Vector3(tankMin.x, tankMin.y, tankMin.z);
		Vector3 max = new Vector3(tankMax.x, tankMax.y, tankMax.z);

		Vector3[] corners = new Vector3[8];
		corners[0] = new Vector3(min.x, min.y, min.z);
		corners[1] = new Vector3(max.x, min.y, min.z);
		corners[2] = new Vector3(max.x, max.y, min.z);
		corners[3] = new Vector3(min.x, max.y, min.z);
		corners[4] = new Vector3(min.x, min.y, max.z);
		corners[5] = new Vector3(max.x, min.y, max.z);
		corners[6] = new Vector3(max.x, max.y, max.z);
		corners[7] = new Vector3(min.x, max.y, max.z);

		Debug.DrawLine(corners[0], corners[1], Color.green);
		Debug.DrawLine(corners[1], corners[2], Color.green);
		Debug.DrawLine(corners[2], corners[3], Color.green);
		Debug.DrawLine(corners[3], corners[0], Color.green);

		Debug.DrawLine(corners[4], corners[5], Color.green);
		Debug.DrawLine(corners[5], corners[6], Color.green);
		Debug.DrawLine(corners[6], corners[7], Color.green);
		Debug.DrawLine(corners[7], corners[4], Color.green);

		Debug.DrawLine(corners[0], corners[4], Color.green);
		Debug.DrawLine(corners[1], corners[5], Color.green);
		Debug.DrawLine(corners[2], corners[6], Color.green);
		Debug.DrawLine(corners[3], corners[7], Color.green);
	}
}

public class BoidECSTankBaker : Baker<BoidECSTankAuthoring>
{
	public override void Bake(BoidECSTankAuthoring authoring)
	{
		Entity entity = GetEntity(TransformUsageFlags.None);

		AddComponent(entity, new BoidTank
		{
			tankMin = authoring.tankMin,
			tankMax = authoring.tankMax
		});
	}
}

public struct BoidTank : IComponentData
{
	public float3 tankMin;
	public float3 tankMax;
}