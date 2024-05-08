using System.Numerics;

namespace CourseWork.Algorithms
{
	internal static class Matrix4X4Extension
	{
		public static Matrix4x4 CreateWorld(in Vector3 position, in Vector3 forward, in Vector3 up)
		{
			var axisZ = Vector3.Normalize(-forward);
			var axisX = Vector3.Normalize(Vector3.Cross(up, axisZ));
			var axisY = Vector3.Cross(axisZ, axisX);

			var result = new Matrix4x4();
			for (var i = 0; i < 3; i++)
			{
				result[0, i] = axisX[i];
				result[1, i] = axisY[i];
				result[2, i] = axisZ[i];
				result[3, i] = position[i];
			}

			result[3, 3] = 1;
			return result;
		}

		public static Matrix4x4 CreateScale(in float scale)
		{
			return  new Matrix4x4(scale, 0, 0, 0,
				0, scale, 0, 0,
				0, 0, scale, 0,
				0, 0, 0, 1);;
		}

		public static Matrix4x4 CreateLookAt(in Vector3 cameraPosition, in Vector3 cameraTarget,
			in Vector3 cameraUpVector)
		{
			var axisZ = Vector3.Normalize(-(cameraTarget - cameraPosition));
			var axisX = Vector3.Normalize(Vector3.Cross(cameraUpVector, axisZ));
			var axisY = Vector3.Cross(axisZ, axisX);
			var negativeCameraPosition = -cameraPosition;

			return new Matrix4x4(
				axisX.X, axisY.X, axisZ.X, 0,
				axisX.Y, axisY.Y, axisZ.Y, 0,
				axisX.Z, axisY.Z, axisZ.Z, 0,
				Vector3.Dot(axisX, negativeCameraPosition),
				Vector3.Dot(axisY, negativeCameraPosition),
				Vector3.Dot(axisZ, negativeCameraPosition),
				1
			);
		}

		public static Matrix4x4 CreatePerspectiveFieldOfView(in float fieldOfView, in float aspectRatio,
			in float nearPlaneDistance, in float farPlaneDistance)
		{
			var height = 1.0f / MathF.Tan(fieldOfView * 0.5f);
			var width = height / aspectRatio;
			var range = float.IsPositiveInfinity(farPlaneDistance)
				? -1.0f
				: farPlaneDistance / (nearPlaneDistance - farPlaneDistance);

			return new Matrix4x4(
				width, 0, 0, 0,
				0, height, 0, 0,
				0, 0, range, -1.0f,
				0, 0, range * nearPlaneDistance, 0
			);
		}

		public static Matrix4x4 CreateViewport(in float x, in float y, in float width, in float height,
			in float minDepth, in float maxDepth)
		{
			return new Matrix4x4(0.5f * width, 0f, 0f, 0f,
				0f, -0.5f * height, 0f, 0f,
				0f, 0f, minDepth - maxDepth, 0f,
				0.5f * width + x, 0.5f * height + y, minDepth, 1f);
		}

		public static Vector3 VectorMatrixMultiplication(in Vector3 vector, in Matrix4x4 inputMatrix)
		{
			Vector4 newVector = new(vector, 1);
			Vector4 result = new();
			for (var i = 0; i < 4; i++)
			{
				for (var k = 0; k < 4; k++)
				{
					result[i] += inputMatrix[k, i] * newVector[k];
				}
			}

			if (result.W != 0)
				result /= result.W;
			return new Vector3(result.X, result.Y, result.Z);
		}
	}
}
