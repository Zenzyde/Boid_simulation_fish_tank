using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager<T>
{
	private float sizeX, sizeY, sizeZ;
	private float gridUnitRadius;
	private List<GridUnit<T>> grid = new List<GridUnit<T>>();
	private Vector3 center;

	public GridManager(Vector3 center, float sizeX, float sizeY, float sizeZ, float unitRadius)
	{
		this.sizeX = sizeX;
		this.sizeY = sizeY;
		this.sizeZ = sizeZ;
		this.center = center;
		this.gridUnitRadius = unitRadius;
		CreateGrid();
	}

	void CreateGrid()
	{
		float diameter = gridUnitRadius * 2;
		center += Vector3.one * .5f * diameter;
		float halfSizeX = sizeX / 2.0f;
		float halfSizeY = sizeY / 2.0f;
		float halfSizeZ = sizeZ / 2.0f;
		for (float x = -halfSizeX; x < halfSizeX; x++)
		{
			for (float z = -halfSizeZ; z < halfSizeZ; z++)
			{
				for (float y = -halfSizeY; y < halfSizeY; y++)
				{
					GridUnit<T> unit = new GridUnit<T>(center + new Vector3(x * diameter, y * diameter, z * diameter),
						gridUnitRadius);
					grid.Add(unit);
				}
			}
		}
	}

	public void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		foreach (GridUnit<T> unit in grid)
		{
			HelperMethods.DrawBoxBounds(unit.GetBoundMin(), unit.GetBoundMax());
		}
	}

	public void DrawInhabitedGridGizmos()
	{
		Gizmos.color = Color.red;
		foreach (GridUnit<T> unit in grid)
		{
			if (unit.GetNumObjectsInGridUnit() > 0)
				HelperMethods.DrawBoxBounds(unit.GetBoundMin(), unit.GetBoundMax());
		}
	}

	public bool AddObject(Vector3 pos, T obj)
	{
		foreach (GridUnit<T> unit in grid)
		{
			if (unit.IsObjectWithinGridUnit(pos))
				return unit.AddObject(obj);
		}
		return false;
	}

	public bool RemoveObject(T obj)
	{
		foreach (GridUnit<T> unit in grid)
		{
			if (unit.IsObjectInGridUnit(obj))
				return unit.RemoveObject(obj);
		}
		return false;
	}

	public List<T> GetObjectsInGridUnit(Vector3 pos)
	{
		foreach (GridUnit<T> unit in grid)
		{
			if (unit.IsObjectWithinGridUnit(pos))
				return unit.GetObjects();
		}
		return null;
	}

	public List<T> GetObjectsWithinRangeOfGridUnit(Vector3 pos, float radius)
	{
		List<T> toReturn = new List<T>();
		foreach (GridUnit<T> unit in grid)
		{
			if (unit.IsObjectWithinGridUnit(pos) || unit.IsObjectWithinGridUnitRange(pos, radius))
				toReturn.AddRange(unit.GetObjects());
		}
		return toReturn;
	}

	public List<T> GetObjectsInGridUnit(T obj)
	{
		foreach (GridUnit<T> unit in grid)
		{
			if (unit.IsObjectInGridUnit(obj))
				return unit.GetObjects();
		}
		return null;
	}

	public int GetNumObjectsInGridUnit(Vector3 pos)
	{
		foreach (GridUnit<T> unit in grid)
		{
			if (unit.IsObjectWithinGridUnit(pos))
				return unit.GetNumObjectsInGridUnit();
		}
		return 0;
	}

	public int GetNumObjectsInGridUnit(T obj)
	{
		foreach (GridUnit<T> unit in grid)
		{
			if (unit.IsObjectInGridUnit(obj))
				return unit.GetNumObjectsInGridUnit();
		}
		return 0;
	}

	public bool UpdateGrid(Vector3 pos, T obj)
	{
		bool objAlreadyInUnit = false;
		bool positionWithinUnit = false;
		bool positionWithinOtherUnit = false;
		foreach (GridUnit<T> unit in grid)
		{
			positionWithinUnit = unit.IsObjectWithinGridUnit(pos);
			objAlreadyInUnit = unit.IsObjectInGridUnit(obj);
			if (positionWithinUnit && !objAlreadyInUnit)
			{
				foreach (GridUnit<T> otherUnit in grid)
				{
					if (otherUnit == unit)
						continue;
					positionWithinOtherUnit = otherUnit.IsObjectInGridUnit(obj);
					if (positionWithinOtherUnit)
					{
						bool removeSuccessful = otherUnit.RemoveObject(obj);
						bool addSuccessful = unit.AddObject(obj);

						//Failsafe in case the remove or add failed -- restore previous state so that no data is lost
						if (!removeSuccessful || !addSuccessful)
						{
							otherUnit.AddObject(obj);
							unit.RemoveObject(obj);
						}

						return removeSuccessful && addSuccessful;
					}
				}
			}
		}
		return false;
	}

	[System.Serializable]
	class GridUnit<T>
	{
		private Vector3 position;
		private float unitRadius;
		private List<T> objects = new List<T>();

		public Vector3 POSITION { get { return position; } }
		public float UNITRADIUS { get { return unitRadius; } }

		public GridUnit(Vector3 position, float unitRadius)
		{
			this.position = position;
			this.unitRadius = unitRadius;
		}

		public Vector3 GetBoundMax()
		{
			return position + new Vector3(unitRadius, unitRadius, unitRadius);
		}

		public Vector3 GetBoundMin()
		{
			return position - new Vector3(unitRadius, unitRadius, unitRadius);
		}

		public bool AddObject(T obj)
		{
			if (!objects.Contains(obj))
			{
				objects.Add(obj);
				return true;
			}
			return false;
		}

		public bool RemoveObject(T obj)
		{
			if (objects.Contains(obj))
			{
				objects.Remove(obj);
				return true;
			}
			return false;
		}
		public List<T> GetObjects() => objects;

		public int GetNumObjectsInGridUnit() => objects.Count;
		public bool IsObjectInGridUnit(T obj) => objects.Contains(obj);

		public bool IsObjectWithinGridUnit(Vector3 pos)
		{
			return pos.y < (position + Vector3.up * unitRadius).y && pos.y > (position - Vector3.up * unitRadius).y &&
				pos.x < (position + Vector3.right * unitRadius).x && pos.x > (position - Vector3.right * unitRadius).x &&
				pos.z < (position + Vector3.forward * unitRadius).z && pos.z > (position - Vector3.forward * unitRadius).z;
		}

		public bool IsObjectWithinGridUnitRange(Vector3 origin, float radius)
		{
			Vector3 pos = (position - origin).normalized * radius;
			return pos.y < (position + Vector3.up * unitRadius).y && pos.y > (position - Vector3.up * unitRadius).y &&
				pos.x < (position + Vector3.right * unitRadius).x && pos.x > (position - Vector3.right * unitRadius).x &&
				pos.z < (position + Vector3.forward * unitRadius).z && pos.z > (position - Vector3.forward * unitRadius).z;
		}
	}
}
