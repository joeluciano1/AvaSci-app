using UnityEngine;
using Vectrosity;
using System.Collections.Generic;

public class VectorObject : MonoBehaviour {

	public enum Shape {Cube = 0, Sphere = 1}
	public Shape shape = Shape.Cube;
	Vector3[] meshvertices;
	List<Vector3> verts=new List<Vector3>();
	
	void Start () {
		meshvertices = GetComponent<MeshFilter>().mesh.vertices;
		
		foreach (var v in meshvertices)
		{
			verts.Add((Vector3)v);
		}
		XrayLineData.use.shapePoints.Add(verts);
		var line = new VectorLine ("Shape", XrayLineData.use.shapePoints[2], XrayLineData.use.lineTexture, XrayLineData.use.lineWidth);
		line.color = Color.green;
		VectorManager.ObjectSetup (gameObject, line, Visibility.Always, Brightness.None);
	}
}