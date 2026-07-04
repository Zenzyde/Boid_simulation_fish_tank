using Unity.Mathematics;
using Unity.Entities;
using Unity.Burst;
using Unity.Transforms;
using System.Linq;

[BurstCompile]
public partial struct BoidECSDelete : ISystem
{
	[BurstCompile]
	public void OnUpdate(ref SystemState state)
	{
		if (!SystemAPI.HasSingleton<DeleteRequest>())
			return;

		var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

		foreach (var (request, entity) in SystemAPI.Query<RefRO<DeleteRequest>>().WithEntityAccess())
		{
			if (state.EntityManager.Exists(request.ValueRO.targetEntity))
			{
				ecb.DestroyEntity(request.ValueRO.targetEntity);
				break;
			}

			ecb.DestroyEntity(entity);
		}
		ecb.Playback(state.EntityManager);
		ecb.Dispose();
	}
}

public struct DeleteRequest : IComponentData
{
	public Entity targetEntity;
}