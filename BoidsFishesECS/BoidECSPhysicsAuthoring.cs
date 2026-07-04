using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class BoidECSPhysicsAuthoring : MonoBehaviour
{
}

public class BoidECSPhysicsBaker : Baker<BoidECSPhysicsAuthoring>
{
	public override void Bake(BoidECSPhysicsAuthoring authoring)
	{
		Entity entity = GetEntity(TransformUsageFlags.None);

		MeshFilter meshFilter = authoring.GetComponent<MeshFilter>();
		NativeArray<Unity.Mathematics.float3> vertices = new NativeArray<Unity.Mathematics.float3>(meshFilter.sharedMesh.vertexCount, Allocator.Temp);
		for (int i = 0; i < meshFilter.sharedMesh.vertexCount; i++)
		{
			Vector3 vertex = meshFilter.sharedMesh.vertices[i];
			vertices[i] = new Unity.Mathematics.float3(vertex.x, vertex.y, vertex.z);
		}
		NativeArray<int3> triangles = new NativeArray<int3>(meshFilter.sharedMesh.triangles.Length / 3, Allocator.Temp);
		for (int i = 0; i < meshFilter.sharedMesh.triangles.Length; i += 3)
		{
			triangles[i / 3] = new int3(
				meshFilter.sharedMesh.triangles[i],
				meshFilter.sharedMesh.triangles[i + 1],
				meshFilter.sharedMesh.triangles[i + 2]
			);
		}
		var collider = Unity.Physics.MeshCollider.Create(vertices, triangles);
		collider.Value.SetCollisionFilter(new CollisionFilter
		{
			BelongsTo = 1 << 3,
			CollidesWith = 1 << 0,
			GroupIndex = 0
		});

		AddComponent(entity, new PhysicsCollider
		{
			Value = collider
		});
		// AddComponent<Unity.Physics.PhysicsVelocity>(entity);
		AddComponent<Unity.Physics.PhysicsWorldIndex>(entity);
	}
}