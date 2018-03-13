using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SIGVerse.Common
{
	public static class SIGVerseUtil
	{
		public static Vector3 CalcContactAveragePoint(Collision collision)
		{
			ContactPoint[] contactPoints = collision.contacts;

			Vector3 contactPointAve = Vector3.zero;

			foreach(ContactPoint contactPoint in contactPoints)
			{
				contactPointAve += contactPoint.point;
			}

			contactPointAve /= contactPoints.Length;

			return contactPointAve;
		}


		public static string GetHierarchyPath(Transform transform)
		{
			string path = transform.name;

			while (transform.parent != null)
			{
				transform = transform.parent;
				path = transform.name + "/" + path;
			}

			return path;
		}


		public static Transform FindTransformFromChild(Transform root, string name)
		{
			Transform[] transforms = root.GetComponentsInChildren<Transform>();

			foreach (Transform transform in transforms)
			{
				if (transform.name == name)
				{
					return transform;
				}
			}

			return null;
		}


		public static GameObject FindGameObjectFromChild(Transform root, string name)
		{
			Transform transform = FindTransformFromChild(root, name);

			if(transform!=null)
			{
				return transform.gameObject;
			}
			else
			{
				return null;
			}
		}
	}
}


