using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SIGVerse.Competition
{
	public class CameraObstacleController : MonoBehaviour
	{
		public Transform rayTarget;

		public LayerMask layerMask;

		//--------------------------------------------------

		private Camera targetCamera;

		private MeshRenderer targetMeshRenderer;

		private Material[] savedMaterials;

		private Material transparentMaterial;

		void Awake()
		{
			this.targetCamera = this.GetComponent<Camera>();

			this.transparentMaterial = CreateTransparentMaterial();
		}

		private void OnPreRender()
		{
			Vector3 rayDirection = this.targetCamera.transform.position - this.rayTarget.position;

			Ray ray = new Ray(this.rayTarget.position, rayDirection);

			RaycastHit hit;

			Physics.Raycast(ray, out hit, 5.0f, layerMask);

			if (hit.distance != 0 && hit.distance < rayDirection.magnitude - this.targetCamera.nearClipPlane)
			{
				this.targetMeshRenderer = hit.collider.gameObject.GetComponentInChildren<MeshRenderer>(); // 

				if(this.targetMeshRenderer != null)
				{
					this.savedMaterials = this.targetMeshRenderer.materials;

					this.targetMeshRenderer.materials = CreateTemporaryMaterials(this.savedMaterials, this.transparentMaterial);
				}
			}
		}

		private void OnPostRender()
		{
			if (this.targetMeshRenderer != null)
			{
				this.targetMeshRenderer.materials = this.savedMaterials;

				this.targetMeshRenderer = null;
			}
		}


		private static Material CreateTransparentMaterial()
		{
			Material transparentMaterial = new Material(Shader.Find("SIGVerse/SIGVerse_BothSidesShader"));

			transparentMaterial.SetFloat("_Mode", 3); // Transparent
			transparentMaterial.SetOverrideTag("RenderType", "Transparent");
			transparentMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
			transparentMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			transparentMaterial.SetInt("_ZWrite", 0);
			transparentMaterial.DisableKeyword("_ALPHATEST_ON");
			transparentMaterial.DisableKeyword("_ALPHABLEND_ON");
			transparentMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
			transparentMaterial.renderQueue = 3000;
			transparentMaterial.SetColor("_Color", new Color(1, 1, 1, 0.3f));

			return transparentMaterial;
		}

		private static Material[] CreateTemporaryMaterials(Material[] sourceMaterials, Material transparentMaterial)
		{
			Material[] tmpMaterials = new Material[sourceMaterials.Length];

			for (int i = 0; i < sourceMaterials.Length; i++)
			{
				tmpMaterials[i] = new Material(transparentMaterial);

				if (sourceMaterials[i].shader.name == "Standard")
				{
					Color srcColor = sourceMaterials[i].GetColor("_Color");
					tmpMaterials[i].SetColor("_Color", new Color(srcColor.r, srcColor.g, srcColor.b, transparentMaterial.GetColor("_Color").a));
				}
			}

			return tmpMaterials;
		}
	}
}

